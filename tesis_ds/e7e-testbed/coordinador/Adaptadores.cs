using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using E7e.MetodoCompensacion;
using E7e.MetodoCompensacion.Puertos;

namespace ArnesE7e;

// ===================================================================
// ADAPTADORES
//
// Unico codigo propio del arnes. Conectan el artefacto (netstandard2.0,
// BCL) al doble neutro (HTTP). No contienen logica del metodo.
// ===================================================================

/// <summary>
/// Adaptador del puerto IServicioExterno contra el doble neutro HTTP.
///
/// NEUTRALIDAD: este adaptador NO deduplica, NO consulta antes de actuar y
/// NO reintenta. Si el metodo no evita una repeticion, el contador del
/// doble la registra. Esa asimetria es intencional: si el doble dedupiera,
/// el metodo no podria demostrar su aporte.
/// </summary>
public sealed class ServicioExternoHttp : IServicioExterno
{
    private readonly HttpClient _http;

    public int TimeoutSegundos { get; }

    public ServicioExternoHttp(string baseUrl, int timeoutSegundos)
    {
        TimeoutSegundos = timeoutSegundos;
        _http = new HttpClient { BaseAddress = new Uri(baseUrl), Timeout = TimeSpan.FromSeconds(timeoutSegundos) };
    }

    public RespuestaServicio Generar(string operacionLogica, string llave, decimal importe)
        => Enviar("/busa/_QR_001", operacionLogica, llave, importe);

    public RespuestaServicio Anular(string operacionLogica, string llave)
        => Enviar("/busa/_QR_003", operacionLogica, llave, 0m);

    public RespuestaServicio Consultar(string operacionLogica, string llave)
        => Enviar("/busa/_QR_002", operacionLogica, llave, 0m);

    private RespuestaServicio Enviar(string ruta, string operacionLogica, string llave, decimal importe)
    {
        var cuerpo = new { operacionLogica, llaveMetodo = llave, importe, referencia = "E7E" };

        try
        {
            using var resp = _http.PostAsJsonAsync(ruta, cuerpo).GetAwaiter().GetResult();
            var texto = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            using var doc = JsonDocument.Parse(texto);
            var r = doc.RootElement;

            return new RespuestaServicio
            {
                Respondio = true,
                FalloTransporte = false,
                ResultCode = r.TryGetProperty("resultCode", out var rc) ? rc.GetString() : "0000",
                Estado = r.TryGetProperty("estado", out var e) ? e.GetString() : "",
                IdQr = r.TryGetProperty("idQr", out var i) ? i.GetString() : null,
                PendienteDesde = r.TryGetProperty("pendienteDesde", out var pd) && pd.ValueKind == JsonValueKind.String
                    ? DateTimeOffset.Parse(pd.GetString())
                    : (DateTimeOffset?)null,
                UnidadesQr = r.TryGetProperty("unidadesQr", out var uq) ? uq.GetInt32() : -1
            };
        }
        catch (TaskCanceledException)
        {
            // El cliente agoto su tiempo de espera. No sabemos si el
            // externo aplico el efecto: esa es la ambiguedad del metodo.
            return new RespuestaServicio
            {
                Respondio = false,
                FalloTransporte = true,
                ResultCode = "TIMEOUT",
                ExcepcionTransporte = "timeout esperando respuesta del externo",
                UnidadesQr = -1
            };
        }
        catch (HttpRequestException ex)
        {
            var msg = ex.InnerException?.Message ?? ex.Message;
            var esConexion = msg.Contains("interrump", StringComparison.OrdinalIgnoreCase)
                             || msg.Contains("forcibly closed", StringComparison.OrdinalIgnoreCase);
            return new RespuestaServicio
            {
                Respondio = false,
                FalloTransporte = true,
                ResultCode = esConexion ? "CONEXION_PERDIDA" : "ERROR_TRANSPORTE",
                ExcepcionTransporte = msg,
                UnidadesQr = -1
            };
        }
        catch (Exception ex)
        {
            return new RespuestaServicio
            {
                Respondio = false,
                FalloTransporte = true,
                ResultCode = "ERROR",
                ExcepcionTransporte = ex.Message,
                UnidadesQr = -1
            };
        }
    }
}

/// <summary>
/// Reloj virtual. Permite simular la ventana de 20 minutos de PENDIENTE sin
/// esperar en tiempo real, que es lo que exige la propiedad 6 del doble.
/// El reloj se comparte con el doble por el endpoint admin.
/// </summary>
public sealed class RelojVirtual : IReloj
{
    public DateTimeOffset Ahora { get; private set; } = new(2026, 10, 13, 10, 0, 0, TimeSpan.Zero);

    public void Avanzar(int minutos)
    {
        Ahora = Ahora.AddMinutes(minutos);
        Doble.AvanzarReloj(minutos);
    }
}

/// <summary>Colector de telemetria K9 para inspeccion en la evidencia.</summary>
public sealed class ColectorTelemetria : ITelemetria
{
    public List<EventoTelemetria> Eventos { get; } = new();

    public void Registrar(EventoTelemetria evento) => Eventos.Add(evento);

    public void Volcar(StringBuilder sb, string operacionLogica)
    {
        foreach (var e in Eventos.Where(e => e.OperacionLogica == operacionLogica))
        {
            sb.Append("      t=").Append(e.Instante.ToString("HH:mm:ss"));
            sb.Append(" intento=").Append(e.Intento);
            sb.Append(" salto=").Append(e.Salto?.ToString() ?? "-");
            sb.Append(" causa=").Append(e.Causa);
            sb.Append(" resultado=").Append(e.Resultado);
            sb.Append(" decision=").Append(e.Decision);
            if (!string.IsNullOrEmpty(e.Observacion)) sb.Append(" obs=").Append(e.Observacion);
            sb.AppendLine();
        }
    }

    public int Contar(string operacionLogica) => Eventos.Count(e => e.OperacionLogica == operacionLogica);

    public bool RegistraCamposObligatorios(string operacionLogica)
        => Eventos.Where(e => e.OperacionLogica == operacionLogica)
                   .All(e => e.Intento >= 0 && !string.IsNullOrEmpty(e.Causa) && !string.IsNullOrEmpty(e.Resultado));
}

/// <summary>
/// Control de activacion por version. La construccion en tres versiones se
/// refleja en que componentes se instancian y en queestan cableados.
/// </summary>
public sealed class ConfigVersion
{
    public string Nivel { get; init; } = "V3";

    public bool K1 { get; init; }
    public bool K2 { get; init; }
    public bool K3 { get; init; }
    public bool K4 { get; init; }
    public bool K5 { get; init; }
    public bool K6 { get; init; }
    public bool K7 { get; init; }
    public bool K8 { get; init; }
    public bool K9 { get; init; }

    /// <summary>
    /// Tope de reintentos por error de negocio 0003/0004. NO sustenta la
    /// idempotencia: la garantia viene de K3 + K5. Se varia precisamente
    /// para demostrarlo (comprobacion de independencia).
    /// </summary>
    public int MaxIntentosNegocios { get; init; } = 3;

    public int VentanaPendenteMinutos { get; init; } = 20;

    public static ConfigVersion V1() => new()
    {
        Nivel = "V1", K1 = true, K2 = true, K3 = true,
        K4 = false, K5 = false, K6 = false, K7 = false, K8 = false, K9 = false
    };

    public static ConfigVersion V2() => new()
    {
        Nivel = "V2", K1 = true, K2 = true, K3 = true, K4 = true, K5 = true, K6 = true,
        K7 = false, K8 = false, K9 = true
    };

    public static ConfigVersion V3() => new()
    {
        Nivel = "V3", K1 = true, K2 = true, K3 = true, K4 = true, K5 = true, K6 = true,
        K7 = true, K8 = true, K9 = true
    };
}

/// <summary>Ayudas de impresion de la evidencia.</summary>
public static class Ev
{
    public static void Titulo(StringBuilder sb, string s)
    {
        sb.AppendLine();
        sb.AppendLine("================================================================");
        sb.AppendLine(s);
        sb.AppendLine("================================================================");
    }

    public static void Guarda(string ruta, StringBuilder sb)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(ruta))!);

        // Bloque de procedencia: sin el, la evidencia no es auditable porque
        // no se puede saber que binario del artefacto la produjo ni cuando.
        var artefacto = typeof(E7e.MetodoCompensacion.CoordinadorMetodo).Assembly;
        var sha = Sha256(artefacto.Location);

        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine("**Procedencia de esta evidencia**");
        sb.AppendLine();
        sb.AppendLine("| Campo | Valor |");
        sb.AppendLine("|---|---|");
        sb.AppendLine($"| Generado por | arnés `coordinador` (`Program.cs`), sin lógica del método |");
        sb.AppendLine($"| Fecha y hora de ejecución | {DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss zzz} |");
        sb.AppendLine($"| Comando | `dotnet run -- VTODAS ..\\evidencia_e7e` |");
        sb.AppendLine($"| Artefacto ejecutado | `{artefacto.GetName().Name}` |");
        sb.AppendLine($"| SHA-256 del artefacto | `{sha}` |");
        sb.AppendLine($"| Doble | `busa-eval-stub` en `http://localhost:5098`, neutro (no deduplica) |");
        sb.AppendLine();
        sb.AppendLine("El SHA-256 permite comprobar que esta evidencia corresponde a esta versión del");
        sb.AppendLine("artefacto. Si el artefacto cambia, el hash cambia y la evidencia deja de coincidir.");

        File.WriteAllText(ruta, sb.ToString(), new UTF8Encoding(false));
        Console.WriteLine($"Evidencia escrita en: {ruta}");
    }

    private static string Sha256(string rutaArchivo)
    {
        using var s = File.OpenRead(rutaArchivo);
        using var h = System.Security.Cryptography.SHA256.Create();
        var bytes = h.ComputeHash(s);
        return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
    }

    /// <summary>Lee un contador observable del doble (verdad externa).</summary>
    public static int ContadorExterno(string id, string campo)
    {
        var v = Doble.Volcado(id);
        var linea = v.Split('\n').FirstOrDefault(l => l.TrimStart().StartsWith("unidadesQr="));
        if (linea == null) return -1;
        foreach (var parte in linea.Trim().Split(' '))
        {
            if (!parte.StartsWith(campo + "=")) continue;
            return int.Parse(parte.Substring(campo.Length + 1));
        }
        return -1;
    }

    public static int UnidadesQr(string id) => ContadorExterno(id, "unidadesQr");

    public static int CompensacionesExternas(string id)
    {
        // El volcado del doble separa los contadores con ';', no con espacio.
        var v = Doble.Volcado(id);
        foreach (var linea in v.Split('\n'))
        {
            var t = linea.Trim();
            if (!t.StartsWith("[salto")) continue;
            foreach (var tok in t.Split(new[] { ' ', ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!tok.StartsWith("compensacionesAplicadas=")) continue;
                return int.Parse(tok.Substring("compensacionesAplicadas=".Length));
            }
        }
        return 0;
    }

    public static void VolcadoExterno(StringBuilder sb, string id)
    {
        foreach (var l in Doble.Volcado(id).Split('\n'))
            sb.AppendLine("    " + l.TrimEnd());
    }
}