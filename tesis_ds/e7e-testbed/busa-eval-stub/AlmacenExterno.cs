using System.Collections.Concurrent;
using System.Text;

namespace BusaEvalStub;

/// <summary>
/// Estados que el servicio externo puede reportar. Mapeables a la
/// categorizacion de K3 (procesado / no procesado / indeterminado).
/// </summary>
public enum EstadoExterno
{
    /// <summary>QR creado y disponible. No procesado: habilita reproceso.</summary>
    SOLICITADO,

    /// <summary>QR pagado. Procesado: no compensar.</summary>
    PAGADO,

    /// <summary>QR anulado. No procesado.</summary>
    ANULADO,

    /// <summary>QR vencido. No procesado.</summary>
    VENCIDO,

    /// <summary>Transitorio en el servicio externo. Indeterminado.</summary>
    PENDIENTE
}

/// <summary>
/// Una unidad de recurso creada por el servicio externo. El doble es NEUTRO:
/// cada invocacion de GenerarQR crea una unidad nueva, aunque la identidad
/// logica sea la misma. Si el cliente llama dos veces, hay dos QR.
/// </summary>
public sealed class UnidadQr
{
    public string IdQr { get; init; } = "";
    public string OperacionLogica { get; init; } = "";
    public EstadoExterno Estado { get; set; } = EstadoExterno.SOLICITADO;
    public DateTimeOffset? PendienteDesde { get; set; }
    public List<string> EfectosAplicados { get; } = new();
}

/// <summary>
/// Registro de una invocacion recibida por el servicio externo, con el
/// resultado observable. Es la "verdad externa" que el cliente no ve.
/// </summary>
public sealed class RegistroInvocacion
{
    public int Secuencia { get; init; }
    public string OperacionLogica { get; init; } = "";
    public string Servicio { get; init; } = "";
    public string ModoFallo { get; init; } = "Ninguno";
    public bool EfectoAplicado { get; init; }
    public string Detalle { get; init; } = "";
}

/// <summary>
/// Contadores observables del servicio externo, separados por salto y por
/// servicio. Son la evidencia que permite medir el efecto duplicado.
/// </summary>
public sealed class Contadores
{
    public int Invocaciones { get; set; }
    public int EfectosAplicados { get; set; }
    public int CompensacionesAplicadas { get; set; }

    public override string ToString() =>
        $"invocaciones={Invocaciones};efectosAplicados={EfectosAplicados};compensacionesAplicadas={CompensacionesAplicadas}";
}

/// <summary>
/// Estado externo de una operacion logica. Incluye TODOS los contadores,
/// incluidos los de los saltos internos que el doble simula.
/// </summary>
public sealed class OperacionExterna
{
    public string OperacionLogica { get; init; } = "";
    public string LlaveMetodo { get; init; } = "";

    /// <summary>Servicio externo determinations de la operacion.</summary>
    public EstadoExterno Estado { get; set; } = EstadoExterno.SOLICITADO;

    public DateTimeOffset? PendienteDesde { get; set; }

    /// <summary>Unidades QR creadas. Un doble neutro crea una por invocacion.</summary>
    public List<UnidadQr> Unidades { get; } = new();

    public List<RegistroInvocacion> Bitacora { get; } = new();

    /// <summary>Contadores por salto.</summary>
    public Dictionary<string, Contadores> ContadoresPorSalto { get; } = new();

    /// <summary>Ultimo servicio BUSA solicitado para esta operacion.</summary>
    public string ServicioSolicitado { get; set; } = "";

    /// <summary>Marca de pago (simula el canal externo que cobra).</summary>
    public bool PagadaPorCanalExterno { get; set; }

    private int _secuencia;

    public Contadores Contadores(string salto)
    {
        if (!ContadoresPorSalto.TryGetValue(salto, out var c))
        {
            c = new Contadores();
            ContadoresPorSalto[salto] = c;
        }
        return c;
    }

    public RegistroInvocacion Registrar(string servicio, string salto, string modoFallo, bool efecto, string detalle)
    {
        _secuencia++;
        var r = new RegistroInvocacion
        {
            Secuencia = _secuencia,
            OperacionLogica = OperacionLogica,
            Servicio = servicio,
            ModoFallo = modoFallo,
            EfectoAplicado = efecto,
            Detalle = detalle
        };
        Bitacora.Add(r);
        return r;
    }

    public int TotalInvocaciones => ContadoresPorSalto.Values.Sum(c => c.Invocaciones);
    public int TotalEfectos => ContadoresPorSalto.Values.Sum(c => c.EfectosAplicados);
    public int TotalCompensaciones => ContadoresPorSalto.Values.Sum(c => c.CompensacionesAplicadas);
}

/// <summary>
/// Modos de fallo inyectables en el salto externo. Reproducibles: se activan
/// por operacion logica y un numero determinista de veces.
/// </summary>
public enum ModoFallo
{
    Ninguno = 0,

    /// <summary>Escenario 1: aborta ANTES del efecto. Nada se aplica.</summary>
    CaidaMitad = 1,

    /// <summary>Escenario 2: aplica el efecto y RETIENE la respuesta mas alla del timeout del cliente.</summary>
    TimeoutPostEfecto = 2,

    /// <summary>Escenario 3 y 6: aplica el efecto y ABORTA la respuesta.</summary>
    RespuestaPerdidaPostEfecto = 3,

    /// <summary>Deja el estado en PENDIENTE (indeterminado) sin aplicar efecto terminal.</summary>
    DejarPendiente = 4
}

/// <summary>
/// <summary>Programacion de fallo pendiente para una operacion logica.</summary>
public sealed class ProgramacionFallo
{
    public ProgramacionFallo(ModoFallo modo, int restantes)
    {
        Modo = modo;
        Restantes = restantes;
    }

    public ModoFallo Modo { get; }
    public int Restantes { get; }
}

/// Almacen externo del doble. NEUTRO por diseno: no deduplica, no verifica
/// y no compensa. Cada invocacion que llega con exito aplica su efecto.
///
/// Si este almacen dedujera, el metodo no podria demostrar su aporte. Esa es
/// exactamente la razon de esta clase (nota de 07c, punto 4).
/// </summary>
public sealed class AlmacenExterno
{
    private readonly ConcurrentDictionary<string, OperacionExterna> _ops = new();

    private readonly ConcurrentDictionary<string, ProgramacionFallo> _fallos = new();

    public DateTimeOffset Ahora { get; private set; } = new(2026, 10, 13, 10, 0, 0, TimeSpan.Zero);

    /// <summary>Ventana de PENDIENTE, en minutos. Parametro de la doble, no del metodo.</summary>
    public int VentanaPendenteMinutos { get; set; } = 20;

    public void FijarReloj(DateTimeOffset instante) => Ahora = instante;

    public DateTimeOffset Avanzar(int minutos) => Ahora = Ahora.AddMinutes(minutos);

    public void ConfigurarFallo(string operacionLogica, ModoFallo modo, int veces)
    {
        if (veces <= 0) _fallos.TryRemove(operacionLogica, out _);
        else _fallos[operacionLogica] = new ProgramacionFallo(modo, veces);
    }

    public void Reiniciar()
    {
        _ops.Clear();
        _fallos.Clear();
        Ahora = new DateTimeOffset(2026, 10, 13, 10, 0, 0, TimeSpan.Zero);
    }

    private ModoFallo ConsumirFallo(string operacionLogica)
    {
        while (true)
        {
            if (!_fallos.TryGetValue(operacionLogica, out var cfg) || cfg.Restantes <= 0)
                return ModoFallo.Ninguno;

            var restantes = cfg.Restantes - 1;
            if (restantes <= 0) _fallos.TryRemove(operacionLogica, out _);
            else _fallos[operacionLogica] = new ProgramacionFallo(cfg.Modo, restantes);

            return cfg.Modo;
        }
    }

    public OperacionExterna Obtener(string operacionLogica, string llaveMetodo)
    {
        return _ops.GetOrAdd(operacionLogica, id => new OperacionExterna
        {
            OperacionLogica = id,
            LlaveMetodo = llaveMetodo
        });
    }

    public IEnumerable<OperacionExterna> Todas => _ops.Values.OrderBy(o => o.OperacionLogica);

    /// <summary>
    /// Aplica una generacion de QR. NEUTRO: cada llamada crea una unidad
    /// nueva con su propio IdQr, aunque la identidad logica sea identica.
    /// </summary>
    public (string IdQr, EstadoExterno Estado, bool EfectoAplicado) GenerarQr(string operacionLogica, string llaveMetodo, string salto)
    {
        var op = Obtener(operacionLogica, llaveMetodo);
        var c = op.Contadores(salto);
        c.Invocaciones++;

        var modo = ConsumirFallo(operacionLogica);

        if (modo == ModoFallo.CaidaMitad)
        {
            // Escenario 1: el servicio cae ANTES de aplicar efecto.
            op.Registrar("GenerarQR", salto, modo.ToString(), false, "ABORT_ANTES_DE_EFECTO");
            return (null, EstadoExterno.SOLICITADO, false);
        }

        if (modo == ModoFallo.DejarPendiente)
        {
            op.Estado = EstadoExterno.PENDIENTE;
            op.PendienteDesde = Ahora;
            op.Registrar("GenerarQR", salto, modo.ToString(), false, "DEJADO_PENDIENTE");
            return (null, EstadoExterno.PENDIENTE, false);
        }

        // Efecto aplicado. NEUTRO: SIEMPRE crea una unidad nueva.
        var unidad = new UnidadQr
        {
            IdQr = $"{operacionLogica}-QR{op.Unidades.Count + 1}",
            OperacionLogica = operacionLogica
        };
        op.Unidades.Add(unidad);
        op.Estado = EstadoExterno.SOLICITADO;
        op.PendienteDesde = null;
        c.EfectosAplicados++;
        op.Registrar("GenerarQR", salto, modo.ToString(), true, $"EFECTO_GENERAR_QR idQr={unidad.IdQr}");

        return (unidad.IdQr, op.Estado, true);
    }

    /// <summary>
    /// Aplica una anulacion. NEUTRO: cada llamada registra un efecto de
    /// compensacion, INCLUSO cuando el estado ya es ANULADO. Esa es la
    /// propiedad que permite que el control negativo observe duplicados.
    /// </summary>
    public (EstadoExterno Estado, bool EfectoAplicado, string Detalle) AnularQr(string operacionLogica, string llaveMetodo, string salto)
    {
        var op = Obtener(operacionLogica, llaveMetodo);
        var c = op.Contadores(salto);
        c.Invocaciones++;

        var modo = ConsumirFallo(operacionLogica);

        if (modo == ModoFallo.CaidaMitad)
        {
            op.Registrar("AnularQR", salto, modo.ToString(), false, "ABORT_ANTES_DE_EFECTO");
            return (op.Estado, false, "ABORT_ANTES_DE_EFECTO");
        }

        // NEUTRO: sin importacion del estado previo. Siempre aplica.
        if (op.Unidades.Count == 0)
        {
            var unidad = new UnidadQr
            {
                IdQr = $"{operacionLogica}-QR1",
                OperacionLogica = operacionLogica,
                Estado = EstadoExterno.ANULADO
            };
            op.Unidades.Add(unidad);
        }

        var blanco = op.Unidades[^1];
        blanco.Estado = EstadoExterno.ANULADO;
        blanco.EfectosAplicados.Add($"CANCEL_QR#{op.Unidades.Sum(u => u.EfectosAplicados.Count(e => e.StartsWith("CANCEL_QR"))) + 1}");

        op.Estado = EstadoExterno.ANULADO;
        op.PendienteDesde = null;
        c.CompensacionesAplicadas++;
        c.EfectosAplicados++;

        var detalle = $"EFECTO_ANULAR_QR idQr={blanco.IdQr} " +
                      $"(invocacionAnulacion={c.CompensacionesAplicadas})";
        op.Registrar("AnularQR", salto, modo.ToString(), true, detalle);
        return (op.Estado, true, detalle);
    }

    /// <summary>Consulta el estado. No verifica ni compensa: solo informa.</summary>
    public (EstadoExterno Estado, DateTimeOffset? PendienteDesde, int Unidades, int AnulacionesAplicadas) ConsultarQr(string operacionLogica, string llaveMetodo)
    {
        var op = Obtener(operacionLogica, llaveMetodo);
        var generar = op.ContadoresPorSalto.Values.Sum(c => c.EfectosAplicados);
        return (op.Estado, op.PendienteDesde, op.Unidades.Count, op.TotalCompensaciones);
    }

    /// <summary>
    /// Marca el QR como pagado. Simula el canal externo que cobra. Produce
    /// el caso que la anulacion no puede cubrir (Dc8).
    /// </summary>
    public void PagarPorCanalExterno(string operacionLogica)
    {
        var op = Obtener(operacionLogica, "");
        op.PagadaPorCanalExterno = true;
        op.Estado = EstadoExterno.PAGADO;
        foreach (var u in op.Unidades) u.Estado = EstadoExterno.PAGADO;
    }

    public void Vencer(string operacionLogica)
    {
        var op = Obtener(operacionLogica, "");
        op.Estado = EstadoExterno.VENCIDO;
    }

    public void DejarPendiente(string operacionLogica)
    {
        var op = Obtener(operacionLogica, "");
        op.Estado = EstadoExterno.PENDIENTE;
        op.PendienteDesde ??= Ahora;
    }

    /// <summary>Vista de introspeccion para el harness y los artefactos.</summary>
    /// <summary>Desenlace de una peticion SOAP, con la instruccion de transporte.</summary>
    public sealed class ResultadoSoap
    {
        /// <summary>Cuerpo del sobre de respuesta.</summary>
        public string Xml { get; set; } = "";

        /// <summary>Si es true, el servidor debe cerrar la conexion sin responder.</summary>
        public bool Abortar { get; set; }

        /// <summary>Segundos que hay que retener la respuesta antes de enviarla.</summary>
        public int RetenerSegundos { get; set; }
    }

    /// <summary>
    /// Punto de entrada para el cliente SOAP/WCF real.
    ///
    /// Delega en el MISMO almacen que los endpoints REST: el contador, el
    /// estado y el modo de fallo son los mismos, de modo que la evidencia
    /// no depende de que el llamante use JSON o SOAP.
    /// </summary>
    public ResultadoSoap ManejarPeticionBusa(string data, Microsoft.AspNetCore.Http.HttpContext ctx)
    {
        var id = Extraer(data, "IdOperacionLogica");
        var servicio = DetectarServicioBusa(data);

        if (string.IsNullOrEmpty(id))
        {
            return new ResultadoSoap { Xml = Envoltura("1000", "No se pudo determinar la operacion", null, null) };
        }

        var modo = ConsumirFallo(id);
        var esAnular = servicio.EndsWith("_QR_003", StringComparison.Ordinal);
        var esConsultar = servicio.EndsWith("_QR_002", StringComparison.Ordinal);

        var op = Obtener(id, "");

        if (esConsultar)
        {
            op.Registrar("ConsultarQR", "externo", modo.ToString(), false, "CONSULTA (sin efecto)");
            return new ResultadoSoap
            {
                Xml = EnvolturaConsulta(op, servicio)
            };
        }

        if (modo == ModoFallo.CaidaMitad)
        {
            // Se registra la invocacion pero NO se aplica efecto.
            var c0 = op.Contadores("externo");
            c0.Invocaciones++;
            op.Registrar(servicio, "externo", modo.ToString(), false, "ABORT_ANTES_DE_EFECTO");
            return new ResultadoSoap { Abortar = true };
        }

        if (modo == ModoFallo.DejarPendiente)
        {
            var cP = op.Contadores("externo");
            cP.Invocaciones++;
            op.Estado = EstadoExterno.PENDIENTE;
            op.PendienteDesde = Ahora;
            op.Registrar(servicio, "externo", modo.ToString(), false, "DEJADO_PENDIENTE");
            return new ResultadoSoap { Xml = Envoltura("0000", "Proceso Exitoso", null, "PENDIENTE") };
        }

        // ---- Aplicar efecto. NEUTRO: siempre. ----
        if (esAnular)
        {
            if (op.Unidades.Count == 0)
            {
                op.Unidades.Add(new UnidadQr
                {
                    IdQr = id + "-QR1",
                    OperacionLogica = id,
                    Estado = EstadoExterno.ANULADO,
                    EfectosAplicados = { "GENERAR_QR#1" }
                });
            }

            var blanco = op.Unidades[^1];
            blanco.Estado = EstadoExterno.ANULADO;
            var nAnul = blanco.EfectosAplicados.Count(e => e.StartsWith("ANULAR_QR", StringComparison.Ordinal)) + 1;
            blanco.EfectosAplicados.Add("ANULAR_QR#" + nAnul);

            op.Estado = EstadoExterno.ANULADO;
            op.PendienteDesde = null;
            var cA = op.Contadores("externo");
            cA.Invocaciones++;
            cA.EfectosAplicados++;
            cA.CompensacionesAplicadas++;
            op.Registrar("AnularQR", "externo", modo.ToString(), true,
                        $"EFECTO_ANULAR_QR idQr={blanco.IdQr} (invocacionAnulacion={cA.CompensacionesAplicadas})");

            if (modo == ModoFallo.TimeoutPostEfecto)
                return new ResultadoSoap { RetenerSegundos = 30 };
            if (modo == ModoFallo.RespuestaPerdidaPostEfecto)
                return new ResultadoSoap { Abortar = true };

            return new ResultadoSoap { Xml = Envoltura("0000", "Proceso Exitoso", null, "ANULADO") };
        }

        // Generacion: NEUTRO, siempre crea una unidad nueva.
        var unidad = new UnidadQr
        {
            IdQr = id + "-QR" + (op.Unidades.Count + 1),
            OperacionLogica = id,
            Estado = EstadoExterno.SOLICITADO,
            EfectosAplicados = { "GENERAR_QR#1" }
        };
        op.Unidades.Add(unidad);
        op.Estado = EstadoExterno.SOLICITADO;
        op.PendienteDesde = null;
        var cG = op.Contadores("externo");
        cG.Invocaciones++;
        cG.EfectosAplicados++;
        op.Registrar("GenerarQR", "externo", modo.ToString(), true, $"EFECTO_GENERAR_QR idQr={unidad.IdQr}");

        if (modo == ModoFallo.TimeoutPostEfecto)
            return new ResultadoSoap { RetenerSegundos = 30 };
        if (modo == ModoFallo.RespuestaPerdidaPostEfecto)
            return new ResultadoSoap { Abortar = true };

        return new ResultadoSoap { Xml = Envoltura("0000", "Proceso Exitoso", unidad.IdQr, "SOLICITADO") };
    }

    private static string DetectarServicioBusa(string data)
    {
        var s = Extraer(data, "Servicio");
        if (!string.IsNullOrEmpty(s)) return s;

        foreach (var cand in new[]
        {
            "UNIVIDA_QR_001", "UNIVIDA_QR_002", "UNIVIDA_QR_003",
            "UNIVIDA_WEB_QR_001", "UNIVIDA_WEB_QR_002", "UNIVIDA_WEB_QR_003",
            "UNIVIDA_NET_QR_001", "UNIVIDA_NET_QR_003", "UNIVIDA_POS_QR_001", "UNIVIDA_POS_QR_003",
            "UNIVIDA_C1_QR_001", "UNIVIDA_C2_QR_001"
        })
        {
            if (data != null && data.Contains(cand, StringComparison.Ordinal)) return cand;
        }
        return "";
    }

    private static string Extraer(string data, string clave)
    {
        if (string.IsNullOrEmpty(data)) return null;
        var marcador = "\"" + clave + "\"";
        var i = data.IndexOf(marcador, StringComparison.OrdinalIgnoreCase);
        if (i < 0) return null;
        var j = data.IndexOf(':', i + marcador.Length);
        if (j < 0) return null;
        var ini = data.IndexOf('"', j + 1);
        if (ini < 0) return null;
        var fin = data.IndexOf('"', ini + 1);
        if (fin < 0) return null;
        return data.Substring(ini + 1, fin - ini - 1);
    }

    /// <summary>Envoltura XML con la forma que el codigo real espera.</summary>
    private static string Envoltura(string resultCode, string message, string idQr, string estado)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append("<ns2:Response xmlns:ns2=\\\"http://www.bancounion.com.bo/\\\">");
        sb.Append("<ns2:ResultCode>").Append(resultCode).Append("</ns2:ResultCode>");
        sb.Append("<ns2:Message>").Append(message).Append("</ns2:Message>");
        if (idQr != null) sb.Append("<ns2:IdQR>").Append(idQr).Append("</ns2:IdQR>");
        if (estado != null) sb.Append("<ns2:EstadoDescripcion>").Append(estado).Append("</ns2:EstadoDescripcion>");
        sb.Append("</ns2:Response>");
        return sb.ToString();
    }

    private static string EnvolturaConsulta(OperacionExterna op, string servicio)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append("<ns2:Response xmlns:ns2=\\\"http://www.bancounion.com.bo/\\\">");
        sb.Append("<ns2:ResultCode>0000</ns2:ResultCode>");
        sb.Append("<ns2:Message>Consulta Exitosa</ns2:Message>");
        sb.Append("<ns2:EstadoDescripcion>").Append(op.Estado).Append("</ns2:EstadoDescripcion>");
        sb.Append("<ns2:IdQR>")
           .Append(op.Unidades.Count > 0 ? op.Unidades[op.Unidades.Count - 1].IdQr : "")
           .Append("</ns2:IdQR>");
        sb.Append("<ns2:UnidadesQR>").Append(op.Unidades.Count).Append("</ns2:UnidadesQR>");
        sb.Append("</ns2:Response>");
        return sb.ToString();
    }

    public string Volcado(string operacionLogica)
    {
        if (!_ops.TryGetValue(operacionLogica, out var op))
            return $"operacion={operacionLogica} NO_ENCONTRADA";

        var sb = new StringBuilder();
        sb.Append($"operacion={op.OperacionLogica}\n");
        sb.Append($"  llaveMetodo={op.LlaveMetodo}\n");
        sb.Append($"  estado={op.Estado}");
        if (op.Estado == EstadoExterno.PENDIENTE && op.PendienteDesde.HasValue)
        {
            var min = (Ahora - op.PendienteDesde.Value).TotalMinutes;
            sb.Append($" pendienteDesde={op.PendienteDesde.Value:HH:mm:ss} minPendiente={min:0}");
        }
        sb.Append('\n');

        sb.Append($"  unidadesQr={op.Unidades.Count}");
        sb.Append($" efectosGenerarQr={op.Unidades.Count}");
        sb.Append($" compensacionesAplicadas={op.TotalCompensaciones}\n");

        foreach (var kv in op.ContadoresPorSalto.OrderBy(k => k.Key))
            sb.Append($"  [salto {kv.Key}] {kv.Value}\n");

        foreach (var u in op.Unidades)
        {
            sb.Append($"    unidad {u.IdQr} estado={u.Estado} efectos=[{string.Join(", ", u.EfectosAplicados)}]\n");
        }

        foreach (var r in op.Bitacora)
            sb.Append($"    #{r.Secuencia} {r.Servicio} modo={r.ModoFallo} efecto={r.EfectoAplicado} {r.Detalle}\n");

        return sb.ToString();
    }
}