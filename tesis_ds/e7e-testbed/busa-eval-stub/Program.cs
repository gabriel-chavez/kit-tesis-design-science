using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BusaEvalStub;

/// <summary>Contrato de entrada/salida del doble. JSON por simplicidad del banco de pruebas.</summary>
public sealed class SolicitudBusa
{
    public string OperacionLogica { get; set; } = "";
    public string LlaveMetodo { get; set; } = "";
    public decimal Importe { get; set; }
    public string Referencia { get; set; } = "";
}

public sealed class RespuestaBusa
{
    public string ResultCode { get; set; } = "0000";
    public string Message { get; set; } = "";
    public string IdQr { get; set; }
    public string Estado { get; set; } = "";
}

/// <summary>
/// Doble de evaluacion del servicio externo BUSA.
///
/// DISENO DELIBERADAMENTE NEUTRO: no deduplica, no verifica, no compensa.
/// Cada invocacion recibida con exito aplica su efecto. Si el cliente
/// reintenta, el doble aplica el efecto de nuevo y eso queda registrado en
/// los contadores. La garantia de no-duplicacion debe provenir del METODO,
/// no de aqui.
///
/// Contrato de neutralidad verificado por el control negativo
/// (evidencia_e7e/control_negativo.md).
/// </summary>
public static class Program
{
    private static readonly AlmacenExterno Almacen = new();
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();
        app.MapPost("/busa/_QR_001", GenerarQr);
        app.MapPost("/busa/_QR_002", ConsultaQrRoute);
        // Endpoint SOAP del CONTRATO REAL (wsUniQrService.IQRService).
        // A el enlaza el cliente WCF de produccion sin modificacion alguna.
        app.MapPost("/UNIQRService/UNIQRService.svc", BunApiSoap);
        app.MapPost("/busa/_QR_003", AnularQr);

        app.MapPost("/admin/reloj/avanzar", (int minutos) =>
        {
            var ahora = Almacen.Avanzar(minutos);
            return Results.Json(new { ahora = ahora.ToString("O"), minutosAvanzados = minutos });
        });

        app.MapPost("/admin/fallo", (string modo, string id, int veces) =>
        {
            if (!Enum.TryParse<ModoFallo>(modo, true, out var m))
                return Results.BadRequest(new { error = $"modo desconocido: {modo}" });
            Almacen.ConfigurarFallo(id, m, veces);
            return Results.Json(new { modo = m.ToString(), id, veces });
        });

        app.MapPost("/admin/pagar", (string id) =>
        {
            Almacen.PagarPorCanalExterno(id);
            return Results.Json(new { id, estado = "PAGADO" });
        });

        app.MapPost("/admin/vencer", (string id) =>
        {
            Almacen.Vencer(id);
            return Results.Json(new { id, estado = "VENCIDO" });
        });

        app.MapPost("/admin/pendiente", (string id) =>
        {
            Almacen.DejarPendiente(id);
            return Results.Json(new { id, estado = "PENDIENTE" });
        });

        app.MapPost("/admin/reiniciar", () =>
        {
            Almacen.Reiniciar();
            return Results.Json(new { estado = "reiniciado" });
        });

        app.MapGet("/admin/volcado/{id}", (string id) =>
            Results.Text(Almacen.Volcado(id), "text/plain; charset=utf-8"));

        app.MapGet("/admin/todas", () =>
            Results.Text(string.Join("\n", Almacen.Todas.Select(o => o.OperacionLogica)), "text/plain; charset=utf-8"));

        app.MapGet("/", () => Results.Text(
            "DOBLE DE EVALUACION NEUTRO. NO DEDUPLICA. NO ES BUSA REAL.\n" +
            "POST /busa/_QR_001 GenerarQR\n" +
            "POST /busa/_QR_002 ConsultarQR\n" +
            "POST /busa/_QR_003 AnularQR\n",
            "text/plain; charset=utf-8"));

        Console.WriteLine("Doble de evaluacion neutro escuchando. NO DEDUPLICA por diseno.");
        Console.WriteLine("El servicio real BUSA nunca es invocado por este proceso.");
        app.Run();
    }

    private static async Task GenerarQr(HttpContext ctx)
    {
        var req = await LeerSolicitud(ctx);
        if (req is null) return;

        var (idQr, estado, efecto) = Almacen.GenerarQr(req.OperacionLogica, req.LlaveMetodo, "externo");

        var modo = UltimoModo(req.OperacionLogica);
        if (modo == ModoFallo.TimeoutPostEfecto && efecto)
        {
            // Escenario 2: el efecto quedo aplicado; la respuesta no llega a tiempo.
            await Retener(ctx, TimeSpan.FromSeconds(30));
            return;
        }
        if (modo == ModoFallo.RespuestaPerdidaPostEfecto && efecto)
        {
            // Escenarios 3 y 6: el efecto quedo aplicado; la respuesta se pierde.
            Console.WriteLine($"[{Almacen.Ahora:HH:mm:ss}] RESP_PERDIDA id={req.OperacionLogica} idQr={idQr}");
            ctx.Abort();
            return;
        }
        if (modo == ModoFallo.CaidaMitad || modo == ModoFallo.DejarPendiente)
        {
            ctx.Abort();
            return;
        }

        await Responder(ctx, new RespuestaBusa
        {
            ResultCode = "0000",
            Message = "Proceso Exitoso",
            IdQr = idQr,
            Estado = estado.ToString()
        });
    }

    private static async Task AnularQr(HttpContext ctx)
    {
        var req = await LeerSolicitud(ctx);
        if (req is null) return;

        var (estado, efecto, detalle) = Almacen.AnularQr(req.OperacionLogica, req.LlaveMetodo, "externo");
        var modo = UltimoModo(req.OperacionLogica);

        if (modo == ModoFallo.TimeoutPostEfecto && efecto)
        {
            await Retener(ctx, TimeSpan.FromSeconds(30));
            return;
        }
        if (modo == ModoFallo.RespuestaPerdidaPostEfecto && efecto)
        {
            Console.WriteLine($"[{Almacen.Avanzar(0):HH:mm:ss}] RESP_PERDIDA_ANULACION id={req.OperacionLogica} {detalle}");
            ctx.Abort();
            return;
        }
        if (modo == ModoFallo.CaidaMitad)
        {
            ctx.Abort();
            return;
        }

        await Responder(ctx, new RespuestaBusa
        {
            ResultCode = "0000",
            Message = "Proceso Exitoso",
            Estado = estado.ToString()
        });
    }

    private static async Task ConsultaQrRoute(HttpContext ctx)
    {
        var req = await LeerSolicitud(ctx);
        if (req is null) return;

        var (estado, desde, unidades, anulaciones) =
            Almacen.ConsultarQr(req.OperacionLogica, req.LlaveMetodo);

        await ctx.Response.WriteAsync(JsonSerializer.Serialize(new Dictionary<string, object>
        {
            ["resultCode"] = "0000",
            ["message"] = "Consulta Exitosa",
            ["estado"] = estado.ToString(),
            ["pendienteDesde"] = desde?.ToString("O"),
            ["unidadesQr"] = unidades,
            ["anulacionesAplicadas"] = anulaciones
        }, Json));
    }

    private static async Task<SolicitudBusa> LeerSolicitud(HttpContext ctx)
    {
        try
        {
            var req = await ctx.Request.ReadFromJsonAsync<SolicitudBusa>();
            Console.WriteLine($"[LEER] op={req?.OperacionLogica ?? "<null>"} contentLength={ctx.Request.ContentLength} bodyVacio={req is null}");

            if (req is null || string.IsNullOrWhiteSpace(req.OperacionLogica))
            {
                await Responder(ctx, new RespuestaBusa { ResultCode = "1000", Message = "OperacionLogica requerida" });
                return null;
            }
            return req;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LEER-ERROR] {ex.GetType().Name}: {ex.Message}");
            await Responder(ctx, new RespuestaBusa { ResultCode = "1000", Message = ex.Message });
            return null;
        }
    }

        /// <summary>
        /// Punto de entrada SOAP con el contrato real de BUSA.
        /// Delega en el mismo Almacen que los endpoints REST, de modo que
        /// contadores, estados e inyeccion de fallos son los mismos.
        /// </summary>
        private static async Task BunApiSoap(HttpContext ctx)
        {
            string cuerpo;
            using (var lector = new StreamReader(ctx.Request.Body))
            {
                cuerpo = await lector.ReadToEndAsync();
            }

            if (!SoapBusa.EsPeticionBunApi(cuerpo, out var data))
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.WriteAsync(SoapBusa.EnvolturaFault("Sobre no reconocido"));
                return;
            }

            var r = Almacen.ManejarPeticionBusa(data, ctx);

            if (r.Abortar)
            {
                ctx.Abort();
                return;
            }
            if (r.RetenerSegundos > 0)
            {
                try { await Task.Delay(TimeSpan.FromSeconds(r.RetenerSegundos), ctx.RequestAborted); }
                catch (OperationCanceledException) { }
                return;
            }

            ctx.Response.ContentType = "text/xml; charset=utf-8";
            await ctx.Response.WriteAsync(SoapBusa.EnvolturaRespuesta(r.Xml));
        }

    private static Task Responder(HttpContext ctx, RespuestaBusa r) =>
        ctx.Response.WriteAsync(JsonSerializer.Serialize(r, Json));

    private static async Task Retener(HttpContext ctx, TimeSpan duracion)
    {
        Console.WriteLine($"[{Almacen.Avanzar(0):HH:mm:ss}] RETENIENDO id=... duracion={duracion.TotalSeconds}s");
        try
        {
            await Task.Delay(duracion, ctx.RequestAborted);
        }
        catch (OperationCanceledException)
        {
            // el cliente cerro: es exactamente el comportamiento de un timeout
        }
    }

    private static ModoFallo UltimoModo(string operacionLogica)
    {
        // El modo se consume dentro del almacen; lo recuperamos del ultimo registro.
        var op = Almacen.Obtener(operacionLogica, "");
        var ultimo = op.Bitacora.LastOrDefault();
        return ultimo is null ? ModoFallo.Ninguno : Enum.Parse<ModoFallo>(ultimo.ModoFallo);
    }
}