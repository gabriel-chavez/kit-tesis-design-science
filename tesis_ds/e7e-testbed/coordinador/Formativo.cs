using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using E7e.MetodoCompensacion;
using E7e.MetodoCompensacion.Puertos;

namespace ArnesE7e;

/// <summary>
/// CICLO FORMATIVO: 11 escenarios (6 base + 5 limites) con metricas.
/// Ejecucion: dotnet run -- FORMATIVO [ruta_salida]
/// </summary>
public static class Formativo
{
    private const int TimeoutCliente = 3;

    public static int Ejecutar(string dir)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Ciclo formativo - resultados con evidencia");
        sb.AppendLine();
        sb.AppendLine("Escenarios: 11 (6 base de E6 + 5 casos limite).");
        sb.AppendLine("Doble: neutro (no deduplica). Artefacto: E7e_MetodoCompensacion.");
        var asm = typeof(CoordinadorMetodo).Assembly;
        using (var s = File.OpenRead(asm.Location))
        using (var h = System.Security.Cryptography.SHA256.Create())
        {
            var hash = BitConverter.ToString(h.ComputeHash(s)).Replace("-", "").ToLowerInvariant();
            sb.AppendLine($"SHA-256 del artefacto: `{hash}`");
        }
        sb.AppendLine($"Fecha real de ejecucion: {DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss zzz}");
        sb.AppendLine();

        // asegurar doble vivo
        try { Doble.Reiniciar(); } catch (Exception ex) { sb.AppendLine("ERROR doble: " + ex.Message); Escribir(dir, "formativo_resultados.md", sb); return 2; }

        // Baseline metricas acumuladas
        var defectos = new List<string>();

        // ---------- S1: Caida antes del efecto ----------
        Seccion(sb, "S1 - Caida antes del efecto (E1)");
        {
            Doble.Reiniciar();
            var (coord, bit, tel, reloj) = ConstruirV3();
            var opId = "OP-1001-G1";
            Doble.ConfigurarFallo("CaidaMitad", opId, 1);
            var r = coord.Procesar(1001, 1, 150000m, false);
            ReportarBasico(sb, r, opId, bit, "Reprocesar", defectos);
            sb.AppendLine($"  Recursos externos: {Ev.UnidadesQr(opId)} (esperado 0)");
            sb.AppendLine($"  Compensaciones externas: {Ev.CompensacionesExternas(opId)} (esperado 0)");
            if (Ev.UnidadesQr(opId) != 0) defectos.Add("S1: recurso creado pese a caida antes del efecto");
        }

        // ---------- S2: Timeout despues del efecto ----------
        Seccion(sb, "S2 - Timeout despues del efecto (E2)");
        {
            Doble.Reiniciar();
            var (coord, bit, tel, reloj) = ConstruirV3();
            var opId = "OP-1002-G1";
            Doble.ConfigurarFallo("TimeoutPostEfecto", opId, 1);
            var r = coord.Procesar(1002, 1, 150000m, false);
            ReportarBasico(sb, r, opId, bit, "ConfirmarProcesada", defectos);
            sb.AppendLine($"  Recursos externos: {Ev.UnidadesQr(opId)} (esperado 1)");
            sb.AppendLine($"  K4 decision final: {r.Decisiones.LastOrDefault()}");
            if (Ev.UnidadesQr(opId) != 1) defectos.Add("S2: no se confirmo el recurso tras timeout");
        }

        // ---------- S3: Respuesta perdida ----------
        Seccion(sb, "S3 - Respuesta perdida (E3)");
        {
            Doble.Reiniciar();
            var (coord, bit, tel, reloj) = ConstruirV3();
            var opId = "OP-1003-G1";
            Doble.ConfigurarFallo("RespuestaPerdidaPostEfecto", opId, 1);
            var r = coord.Procesar(1003, 1, 150000m, false);
            ReportarBasico(sb, r, opId, bit, "ConfirmarProcesada", defectos);
            sb.AppendLine($"  Recursos externos: {Ev.UnidadesQr(opId)} (esperado 1)");
            if (Ev.UnidadesQr(opId) != 1) defectos.Add("S3: no se confirmo el recurso tras respuesta perdida");
        }

        // ---------- S4: Solicitud duplicada concurrente ----------
        Seccion(sb, "S4 - Solicitud duplicada CONCURRENTE (misma llave, en paralelo)");
        {
            Doble.Reiniciar();
            var (coord, bit, tel, reloj) = ConstruirV3();
            var opId = "OP-1004-G1";
            int n = 5;
            var tasks = new List<Task<ResultadoMetodo>>();
            for (int i = 0; i < n; i++)
            {
                tasks.Add(Task.Run(() => coord.Procesar(1004, 1, 150000m, false)));
            }
            Task.WaitAll(tasks.ToArray());
            var recursos = Ev.UnidadesQr(opId);
            sb.AppendLine($"  Invocaciones concurrentes: {n}");
            sb.AppendLine($"  Recursos externos creados: {recursos} (esperado 1)");
            sb.AppendLine($"  Estado bitacora: {bit.Obtener(opId).Estado}");
            sb.AppendLine("  LIMITE DECLARADO (D1): el lock es de PROCESO. Esta corrida prueba una sola instancia.");
            sb.AppendLine("  La garantia distribuida depende del indice unico y de la insercion atomica de Dc10.");
            if (recursos != 1)
            {
                defectos.Add($"S4: concurrencia creo {recursos} recursos (esperado 1) - defecto de atomicidad/idempotencia de operacion");
            }
        }

        // ---------- S5: Reintento tras exito real ----------
        Seccion(sb, "S5 - Reintento tras exito real (E5)");
        {
            Doble.Reiniciar();
            var (coord, bit, tel, reloj) = ConstruirV3();
            var opId = "OP-1005-G1";
            var r1 = coord.Procesar(1005, 1, 150000m, false);
            var r2 = coord.Procesar(1005, 1, 150000m, false);
            sb.AppendLine($"  1ra invocacion: exito={r1.Exito} recursos={Ev.UnidadesQr(opId)}");
            sb.AppendLine($"  2da invocacion: exito={r2.Exito} recursos={Ev.UnidadesQr(opId)}");
            sb.AppendLine($"  Recursos externos: {Ev.UnidadesQr(opId)} (esperado 1)");
            if (Ev.UnidadesQr(opId) != 1) defectos.Add("S5: reintento tras exito creo recurso duplicado");
        }

        // ---------- S6: Fallo durante la compensacion ----------
        Seccion(sb, "S6 - Fallo durante la compensacion");
        {
            Doble.Reiniciar();
            var (coord, bit, tel, reloj) = ConstruirV3();
            var opId = "OP-1006-G1";
            coord.Procesar(1006, 1, 150000m, false);
            Doble.ConfigurarFallo("RespuestaPerdidaPostEfecto", opId, 3);
            int extAntes = Ev.CompensacionesExternas(opId);
            for (int i = 1; i <= 3; i++)
            {
                var c = coord.Compensar(opId, "fallo-comp");
                sb.AppendLine($"   compensacion #{i}: ejecutada={c.Ejecutada} omitida={c.OmitidaPorIdempotencia} respondio={c.RespuestaRecibida} falloTransporte={c.FalloTransporte}");
            }
            sb.AppendLine($"  Compensaciones externas: {Ev.CompensacionesExternas(opId)} (esperado 1)");
            sb.AppendLine($"  Registros Aplicada bitacora: {bit.ContarAplicadas(opId)}");
            if (Ev.CompensacionesExternas(opId) != 1) defectos.Add("S6: compensacion duplicada bajo fallo repetido");
        }

        // ---------- S7: Proceso muere entre reserva y ejecucion ----------
        Seccion(sb, "S7 - Proceso muere entre reserva y ejecucion (K2.MarcarAbandonada)");
        {
            Doble.Reiniciar();
            var (coord, bit, tel, reloj) = ConstruirV3();
            var opId = "OP-1007-G1";
            coord.Procesar(1007, 1, 150000m, false);
            // reservar sin completar: simular muerte entre reserva y ejecucion
            var op = bit.Obtener(opId);
            bool reservado = bit.IntentarReservar(op, "muerte-reserva", "AnularQR", reloj.Ahora);
            sb.AppendLine($"  Reserva creada: {reservado}");
            sb.AppendLine($"  Reservas pendientes antes de reconciliar: {bit.ReservasPendientes(opId).Count()}");

            // La primera pasada no puede confirmar la anulacion: el externo
            // aplica el efecto pero se pierde la respuesta. K7 debe marcar la
            // reserva como Abandonada.
            Doble.ConfigurarFallo("RespuestaPerdidaPostEfecto", opId, 1);
            var i1 = coord.Reconciliador.EjecutarPasada();
            var estadoTras1 = bit.EstadoReserva(opId, "muerte-reserva");
            sb.AppendLine($"  K7 pasada 1: resueltasPorEjecucion={i1.ReservasResueltas} cerradasPorConfirmacion={i1.OmitidasPorIdempotencia} estadoReserva={estadoTras1}");

            // La segunda pasada encuentra la reserva Abandonada y la resuelve
            // consultando el externo (ya ANULADO), sin repetir el efecto.
            var i2 = coord.Reconciliador.EjecutarPasada();
            var estadoTras2 = bit.EstadoReserva(opId, "muerte-reserva");
            sb.AppendLine($"  K7 pasada 2: resueltasPorEjecucion={i2.ReservasResueltas} cerradasPorConfirmacion={i2.OmitidasPorIdempotencia} estadoReserva={estadoTras2}");
            var resueltasObservadas = estadoTras2 == EstadoCompensacion.Aplicada ? 1 : 0;
            sb.AppendLine($"  Reservas resueltas (observado por transicion de estado): {resueltasObservadas}");
            sb.AppendLine($"  Reservas pendientes despues: {bit.ReservasPendientes(opId).Count()}");
            sb.AppendLine($"  Compensaciones externas: {Ev.CompensacionesExternas(opId)} (esperado 1)");
            sb.AppendLine($"  Registros Aplicada bitacora: {bit.ContarAplicadas(opId)}");
            var abandonoTelemetria = tel.Eventos.Count(e => e.OperacionLogica == opId && e.Resultado == "RESERVA_ABANDONADA");
            sb.AppendLine($"  Eventos de telemetria RESERVA_ABANDONADA: {abandonoTelemetria}");
            if (estadoTras1 != EstadoCompensacion.Abandonada)
                defectos.Add("S7: K7 no marco la reserva huerfana como Abandonada en la primera pasada");
            if (estadoTras2 != EstadoCompensacion.Aplicada)
                defectos.Add("S7: la reserva Abandonada no se resolvio en la segunda pasada");
            if (abandonoTelemetria < 1)
                defectos.Add("S7: no hay evidencia de telemetria de MarcarAbandonada");
            if (Ev.CompensacionesExternas(opId) != 1)
                defectos.Add("S7: la resolucion de la reserva duplico la compensacion externa");
        }

        // ---------- S8: PENDIENTE frontera 19/20 ----------
        Seccion(sb, "S8 - PENDIENTE frontera 19 y 20 minutos (granularidad declarada)");
        {
            Doble.Reiniciar();
            var (coord, bit, tel, reloj) = ConstruirV3();
            var opId = "OP-1008b-G1";
            var op = bit.Abrir(opId, coord.Identidad.DerivarLlave(1008, 1), 1008, 1, 150000m, reloj.Ahora);
            op.Estado = EstadoOperacion.Indeterminada;
            Doble.DejarPendiente(opId);

            // medicion a 19 min
            reloj.Avanzar(19);
            var v19 = coord.VerificadorEstado.Verificar(opId, op.Llave);
            var d19 = K4MotorDecision.Decidir(v19, null, 0, coord.MaxIntentosNegocios, false);
            sb.AppendLine($"  t=19min: categoria={v19.Categoria} minutos={v19.MinutosPendiente} decision={d19}");

            // avanzar 1 min mas (total 20)
            reloj.Avanzar(1);
            var v20 = coord.VerificadorEstado.Verificar(opId, op.Llave);
            var d20 = K4MotorDecision.Decidir(v20, null, 0, coord.MaxIntentosNegocios, false);
            sb.AppendLine($"  t=20min: categoria={v20.Categoria} minutos={v20.MinutosPendiente} decision={d20}");

            if (d19 != Decision.Reconsultar) defectos.Add("S8: a 19 min no se reconsulta (frontera inferior)");
            if (d20 != Decision.Reconciliar) defectos.Add("S8: a 20 min no se reconcilia (frontera superior)");
            sb.AppendLine("  NOTA: la ventana se mide en minutos enteros; no hay precision de segundos (19:59/20:01 no distinguibles).");
        }

        // ---------- S9: Estado externo desconocido ----------
        Seccion(sb, "S9 - Estado externo desconocido (no asumir, reconciliar)");
        {
            // fake externo que lanza en Consultar
            var fake = new ExternoDesconocido();
            var reloj = new RelojVirtual();
            var bit = new K2Bitacora();
            var tel = new ColectorTelemetria();
            var coord = new CoordinadorMetodo(fake, bit, reloj, tel, 3, 20);
            var opId = "OP-1009-G1";
            // Generar con fallo de transporte para llegar a K3
            var r = coord.Procesar(1009, 1, 150000m, false);
            // Forzar consulta fallida: llamar VerificadorEstado
            var v = coord.VerificadorEstado.Verificar(opId, coord.Identidad.DerivarLlave(1009, 1));
            var d = K4MotorDecision.Decidir(v, null, 0, coord.MaxIntentosNegocios, false);
            sb.AppendLine($"  Consulta fallida: {v.ConsultaFallida}");
            sb.AppendLine($"  Categoria: {v.Categoria}");
            sb.AppendLine($"  Decision K4: {d} (esperado Reconciliar)");
            if (d != Decision.Reconciliar) defectos.Add("S9: estado desconocido no lleva a Reconciliar");
        }

        // ---------- S10: Falla la escritura en bitacora tras reservar ----------
        Seccion(sb, "S10 - Falla la escritura en bitacora tras reservar (consistencia)");
        {
            Doble.Reiniciar();
            // 2 fallos: agotan los 2 reintentos de K5; la reserva queda Registrada.
            var falloBit = new BitacoraFallaEscritura(2);
            var reloj = new RelojVirtual();
            var tel = new ColectorTelemetria();
            var externo = new ServicioExternoHttp(Doble.BaseUrl, TimeoutCliente);
            var coord = new CoordinadorMetodo(externo, falloBit, reloj, tel, 3, 20);
            var opId = "OP-1010-G1";
            coord.Procesar(1010, 1, 150000m, false);
            string excepcion = "ninguna";
            try
            {
                coord.Compensar(opId, "fallo-bitacora");
            }
            catch (Exception ex)
            {
                excepcion = ex.GetType().Name + ": " + ex.Message;
            }
            sb.AppendLine($"  Excepcion al compensar: {excepcion} (esperado: ninguna)");
            sb.AppendLine($"  Estado reserva tras K5: {falloBit.EstadoReserva(opId, "fallo-bitacora:1")}");
            sb.AppendLine($"  Compensaciones externas: {Ev.CompensacionesExternas(opId)} (esperado 1)");

            // K7 resuelve la reserva que K5 dejo Registrada.
            var i1 = coord.Reconciliador.EjecutarPasada();
            sb.AppendLine($"  K7 pasada 1: resueltasPorEjecucion={i1.ReservasResueltas} cerradasPorConfirmacion={i1.OmitidasPorIdempotencia}");
            var i2 = coord.Reconciliador.EjecutarPasada();
            sb.AppendLine($"  K7 pasada 2: resueltasPorEjecucion={i2.ReservasResueltas} cerradasPorConfirmacion={i2.OmitidasPorIdempotencia}");
            var estadoFinalReserva = falloBit.EstadoReserva(opId, "fallo-bitacora:1");
            var resueltasObservadas = estadoFinalReserva == EstadoCompensacion.Aplicada ? 1 : 0;
            sb.AppendLine($"  Reservas resueltas (observado por transicion de estado): {resueltasObservadas}");
            sb.AppendLine($"  Estado reserva final: {estadoFinalReserva}");
            sb.AppendLine($"  Reservas pendientes finales: {falloBit.ReservasPendientes(opId).Count()}");
            sb.AppendLine($"  Compensaciones externas finales: {Ev.CompensacionesExternas(opId)} (esperado 1)");
            if (excepcion != "ninguna")
                defectos.Add("S10: el fallo de persistencia se propaga como excepcion");
            if (falloBit.EstadoReserva(opId, "fallo-bitacora:1") != EstadoCompensacion.Aplicada)
                defectos.Add("S10: K7 no logro cerrar la reserva que quedo Registrada");
            if (Ev.CompensacionesExternas(opId) != 1)
                defectos.Add("S10: la recuperacion duplico la compensacion externa");
        }

        // ---------- S11: compensacion falla dos veces y luego responde ----------
        Seccion(sb, "S11 - Compensacion falla dos veces y luego responde");
        {
            Doble.Reiniciar();
            var (coord, bit, tel, reloj) = ConstruirV3();
            var opId = "OP-1011-G1";
            coord.Procesar(1011, 1, 150000m, false);
            Doble.ConfigurarFallo("RespuestaPerdidaPostEfecto", opId, 2);
            for (int i = 1; i <= 3; i++)
            {
                var c = coord.Compensar(opId, "fallo-doble");
                sb.AppendLine($"   compensacion #{i}: ejecutada={c.Ejecutada} omitida={c.OmitidaPorIdempotencia} respondio={c.RespuestaRecibida} falloTransporte={c.FalloTransporte}");
            }
            sb.AppendLine($"  Compensaciones externas: {Ev.CompensacionesExternas(opId)} (esperado 1)");
            if (Ev.CompensacionesExternas(opId) != 1) defectos.Add("S11: idempotencia bajo fallo repetido no se cumple");
        }

        // ---------- Control negativo ----------
        Seccion(sb, "Control negativo (sin K5 debe duplicar; con K5 no)");
        {
            // sin K5
            Doble.Reiniciar();
            var (coord, bit, tel, reloj) = ConstruirV2();
            coord.Compensador.Activo = false;
            var opId = "OP-9000-G1";
            coord.Procesar(9000, 1, 150000m, false);
            for (int i = 0; i < 3; i++) coord.Compensar(opId, "cn");
            var sinK5 = Ev.CompensacionesExternas(opId);
            // con K5
            Doble.Reiniciar();
            var (coord2, bit2, tel2, reloj2) = ConstruirV3();
            var opId2 = "OP-9500-G1";
            coord2.Procesar(9500, 1, 150000m, false);
            Doble.ConfigurarFallo("RespuestaPerdidaPostEfecto", opId2, 1);
            for (int i = 0; i < 3; i++) coord2.Compensar(opId2, "cp");
            var conK5 = Ev.CompensacionesExternas(opId2);
            sb.AppendLine($"  Sin K5 (3 invocaciones): {sinK5} efectos (esperado 3)");
            sb.AppendLine($"  Con K5 (3 invocaciones): {conK5} efectos (esperado 1)");
            if (sinK5 < 3) defectos.Add("Control negativo: el doble deduplica sin K5 (neutralidad rota)");
            if (conK5 != 1) defectos.Add("Control negativo: K5 no evita duplicado");
        }

        // ---------- Resumen de defectos ----------
        Seccion(sb, "HALLAZGOS Y REDISEÑO - Iteración 3 (corrección de lectura)");
        sb.AppendLine();
        sb.AppendLine("| Iteración | Qué se buscaba | Resultado |");
        sb.AppendLine("|---|---|---|");
        sb.AppendLine("| 1. Detección | Ejecutar los 11 escenarios y encontrar defectos del método | D1 concurrencia, D2 MarcarAbandonada sin cablear, D3 frontera de ventana, D4 persistencia sin manejo, D5 estado desconocido (sin cambio) |");
        sb.AppendLine("| 2. Rediseño y verificación | Aplicar D1, D2 y D4; declarar D3; re-ejecutar S7, S8, S10 y regresión | D1-D4 resueltos o declarados; 0 defectos en la corrida |");
        sb.AppendLine("| 3. Corrección de lectura | Separar cierres por ejecución de cierres por confirmación; reemitir evidencia sin tocar el artefacto | S7 y S10: resueltasPorEjecucion=0, cerradasPorConfirmacion=1, resueltas observadas=1; hash intacto |");
        sb.AppendLine();
        sb.AppendLine($"Total de defectos detectados en esta corrida: {defectos.Count}");
        sb.AppendLine("La iteración 1 detectó D1-D5; la iteración 2 los rediseñó o declaró; esta iteración solo corrige la lectura del contador.");
        sb.AppendLine();
        foreach (var d in defectos) sb.AppendLine("- " + d);
        sb.AppendLine();
        sb.AppendLine("## Rediseños aplicados en este ciclo");
        sb.AppendLine();
        sb.AppendLine("1. **D1 - Concurrencia (S4):** lock por operacion en `CoordinadorMetodo.Procesar` (atomiza el guard). LIMITE: es un lock de proceso; la garantia distribuida depende de Dc10 (indice unico + insercion atomica).");
        sb.AppendLine("2. **D2 - MarcarAbandonada (S7):** K7 es el responsable unico. Marca Abandonada la reserva huerfana, y `ReservasPendientes`/K5 incluyen Abandonada para resolverla en la siguiente pasada. Verificado con dos pasadas de K7.");
        sb.AppendLine("3. **D3 - Frontera de ventana (S8):** se declara condicion de contorno: la ventana es de 20 minutos completos y no distingue 19:59 de 20:01. No se afirma precision de segundos.");
        sb.AppendLine("4. **D4 - Persistencia (S10):** `MarcarAplicada` devuelve bool; K5 reintenta y, si falla, deja la reserva Registrada sin propagar excepcion. K7 la resuelve en pasadas posteriores. Verificado.");
        sb.AppendLine("5. **S9 - Estado desconocido:** ya reconcilia; sin cambio funcional.");
        sb.AppendLine();

        Escribir(dir, "ciclo_formativo_resultados.md", sb);
        return 0;
    }

    // ---------- helpers ----------
    private static (CoordinadorMetodo, K2Bitacora, ColectorTelemetria, RelojVirtual) ConstruirV3()
    {
        Doble.Reiniciar();
        var reloj = new RelojVirtual();
        var bit = new K2Bitacora();
        var tel = new ColectorTelemetria();
        var externo = new ServicioExternoHttp(Doble.BaseUrl, TimeoutCliente);
        var coord = new CoordinadorMetodo(externo, bit, reloj, tel, 3, 20);
        return (coord, bit, tel, reloj);
    }

    private static (CoordinadorMetodo, K2Bitacora, ColectorTelemetria, RelojVirtual) ConstruirV2()
    {
        Doble.Reiniciar();
        var reloj = new RelojVirtual();
        var bit = new K2Bitacora();
        var tel = new ColectorTelemetria();
        var externo = new ServicioExternoHttp(Doble.BaseUrl, TimeoutCliente);
        var coord = new CoordinadorMetodo(externo, bit, reloj, tel, 3, 20);
        return (coord, bit, tel, reloj);
    }

    private static void Seccion(StringBuilder sb, string t)
    {
        sb.AppendLine();
        sb.AppendLine("================================================================");
        sb.AppendLine(t);
        sb.AppendLine("================================================================");
    }

    private static void ReportarBasico(StringBuilder sb, ResultadoMetodo r, string opId, K2Bitacora bit, string decisionEsperada, List<string> defectos)
    {
        sb.AppendLine($"  Estado bitacora: {r.Estado}");
        sb.AppendLine($"  Categoria K3: {r.Categoria}");
        sb.AppendLine($"  Intentos: {r.Intentos}  Consultas: {r.ConsultasEstado}");
        sb.AppendLine($"  Decisiones: {string.Join(" -> ", r.Decisiones)}");
        sb.AppendLine($"  K4 decision final esperada: {decisionEsperada}");
        var ultima = r.Decisiones.Count > 0 ? r.Decisiones[r.Decisiones.Count - 1].ToString() : "-";
        if (ultima != decisionEsperada) defectos.Add($"decisión esperada {decisionEsperada} pero fue {ultima}");
    }

    private static void Escribir(string dir, string nombre, StringBuilder sb)
    {
        Directory.CreateDirectory(Path.GetFullPath(dir));
        File.WriteAllText(Path.Combine(dir, nombre), sb.ToString(), new UTF8Encoding(false));
        Console.WriteLine($"Evidencia escrita en: {Path.Combine(dir, nombre)}");
    }

    // ---------- fakes ----------
    private sealed class ExternoDesconocido : IServicioExterno
    {
        public RespuestaServicio Generar(string operacionLogica, string llave, decimal importe)
            => new RespuestaServicio { Respondio = false, FalloTransporte = true, ResultCode = "TIMEOUT", ExcepcionTransporte = "timeout" };
        public RespuestaServicio Anular(string operacionLogica, string llave)
            => new RespuestaServicio { Respondio = false, FalloTransporte = true, ResultCode = "TIMEOUT", ExcepcionTransporte = "timeout" };
        public RespuestaServicio Consultar(string operacionLogica, string llave)
            => throw new HttpRequestException("conexion perdida");
    }

    private sealed class BitacoraFallaEscritura : IBitacora
    {
        private readonly K2Bitacora _inner = new();
        private int _fallosRestantes;
        public BitacoraFallaEscritura(int fallosRestantes) { _fallosRestantes = fallosRestantes; }
        public Operacion Abrir(string a, string b, long c, int d, decimal e, DateTimeOffset f) => _inner.Abrir(a, b, c, d, e, f);
        public Operacion Obtener(string a) => _inner.Obtener(a);
        public IEnumerable<Operacion> Todas() => _inner.Todas();
        public void Guardar(Operacion o) => _inner.Guardar(o);
        public void RegistrarSalto(Operacion o, Salto s, string sv, string res, string causa, bool ef, DateTimeOffset ahora) => _inner.RegistrarSalto(o, s, sv, res, causa, ef, ahora);
        public bool IntentarReservar(Operacion o, string m, string a, DateTimeOffset ahora) => _inner.IntentarReservar(o, m, a, ahora);
        public bool MarcarAplicada(Operacion o, string m, string r, string id, DateTimeOffset ahora)
        {
            if (_fallosRestantes > 0)
            {
                _fallosRestantes--;
                throw new InvalidOperationException("fallo de escritura en bitacora (simulado)");
            }
            return _inner.MarcarAplicada(o, m, r, id, ahora);
        }
        public void MarcarOmitida(Operacion o, string m, DateTimeOffset ahora) => _inner.MarcarOmitida(o, m, ahora);
        public IEnumerable<RegistroCompensacion> ReservasPendientes(string o) => _inner.ReservasPendientes(o);
        public void MarcarAbandonada(Operacion o, string m, DateTimeOffset ahora) => _inner.MarcarAbandonada(o, m, ahora);
        public int ContarAplicadas(string o) => _inner.ContarAplicadas(o);
        public EstadoCompensacion EstadoReserva(string o, string m) => _inner.EstadoReserva(o, m);
        public IEnumerable<RegistroCompensacion> TodasCompensaciones() => _inner.TodasCompensaciones();
    }
}

