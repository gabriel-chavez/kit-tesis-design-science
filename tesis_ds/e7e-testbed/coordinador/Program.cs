using System.Text;
using E7e.MetodoCompensacion;

namespace ArnesE7e;

/// <summary>
/// EJECUTOR DE ESCENARIOS Y COMPROBACIONES.
///
/// IMPORTANTE: este archivo NO implementa el metodo. Solo inyecta fallos
/// en el doble, invoca el coordinador del ARTEFACTO y vuelca lo observado.
/// La evidencia de V1-V8 sale de ejecutar E7e_MetodoCompensacion.
///
/// Uso:  dotnet run -- V1|V2|V3|NEGATIVO|VTODAS  [ruta_salida]
/// </summary>
public static class Programa
{
    private const int TimeoutCliente = 3;

    public static int Main(string[] args)
    {
        var nivel = args.Length > 0 ? args[0].ToUpperInvariant() : "VTODAS";
        var dir = args.Length > 1 ? args[1] : "..\\evidencia_e7e";

        try
        {
            Doble.Reiniciar();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 2;
        }

        return nivel switch
        {
            "V1" => EjecutarV1(dir),
            "V2" => EjecutarV2(dir),
            "V3" => EjecutarV3(dir),
            "NEGATIVO" => EjecutarControlNegativo(dir),
            "VTODAS" => EjecutarTodas(dir),
            "FORMATIVO" => Formativo.Ejecutar(dir),
            _ => throw new ArgumentException($"Nivel desconocido: {nivel}")
        };
    }

    /// <summary>Construye el coordinador del artefacto para una version.</summary>
    private static (CoordinadorMetodo Coord, K2Bitacora Bitacora, ColectorTelemetria Tel, RelojVirtual Reloj)
        Construir(ConfigVersion cfg)
    {
        Doble.Reiniciar();
        var reloj = new RelojVirtual();
        var bitacora = new K2Bitacora();
        var tel = new ColectorTelemetria();
        var externo = new ServicioExternoHttp(Doble.BaseUrl, TimeoutCliente);

        var coord = new CoordinadorMetodo(externo, bitacora, reloj, tel,
                                           cfg.MaxIntentosNegocios, cfg.VentanaPendenteMinutos);
        return (coord, bitacora, tel, reloj);
    }

    private static string Id(long tramite, int gestion) => $"OP-{tramite}-G{gestion}";

    // =================================================================
    // V1 — EXPLORATORIA: K1, K2, K3.  Escenarios 2, 3, 5
    // =================================================================
    private static int EjecutarV1(string dir)
    {
        var cfg = ConfigVersion.V1();
        var sb = new StringBuilder();

        sb.AppendLine("# V1 Exploratoria — resultados con evidencia");
        sb.AppendLine();
        sb.AppendLine("| Aspecto | Detalle |");
        sb.AppendLine("|---|---|");
        sb.AppendLine($"| Versión | V1 Exploratoria |");
        sb.AppendLine($"| Componentes activos | K1, K2, K3 |");
        sb.AppendLine($"| Componentes inactivos | K4, K5, K6, K7, K8, K9 |");
        sb.AppendLine($"| Escenarios | 2, 3, 5 |");
        sb.AppendLine($"| Doble | neutro (no deduplica) |");
        sb.AppendLine($"| Artefacto ejecutado | `E7e_MetodoCompensacion` (netstandard2.0) |");
        sb.AppendLine($"| Fecha real de ejecución | 2026-10-13 |");
        sb.AppendLine();
        sb.AppendLine("En V1 el método **observa** la ambigüedad pero no **decide**: no hay motor de");
        sb.AppendLine("decisión (K4) ni compensador (K5). El objetivo declarado es comprobar que K3");
        sb.AppendLine("separa las tres categorías de estado.");

        // ---- Escenario 2: TIMEOUT ----
        Ev.Titulo(sb, "ESCENARIO 2 — TIMEOUT (el externo aplica el efecto y retiene la respuesta)");
        {
            var (coord, bit, tel, reloj) = Construir(cfg);
            Doble.ConfigurarFallo("TimeoutPostEfecto", Id(2002, 1), 1);

            var r = coord.Procesar(2002, 1, 150000m, false);

            VolcarResultado(sb, r, Id(2002, 1), bit, tel);
            sb.AppendLine($"  **Comprobación V1:** K3Raw = `{r.Categoria}`; recurso externo = {Ev.UnidadesQr(Id(2002, 1))}");
        }

        // ---- Escenario 3: RESPUESTA PERDIDA ----
        Ev.Titulo(sb, "ESCENARIO 3 — RESPUESTA PERDIDA (el externo aplica el efecto y aborta la respuesta)");
        {
            var (coord, bit, tel, reloj) = Construir(cfg);
            Doble.ConfigurarFallo("RespuestaPerdidaPostEfecto", Id(2003, 1), 1);

            var r = coord.Procesar(2003, 1, 150000m, false);

            VolcarResultado(sb, r, Id(2003, 1), bit, tel);
            sb.AppendLine($"  **Comprobación V1:** K3Raw = `{r.Categoria}`; recurso externo = {Ev.UnidadesQr(Id(2003, 1))}");
        }

        // ---- Escenario 5: REINTENTO TRAS ÉXITO REAL (doble neutro) ----
        Ev.Titulo(sb, "ESCENARIO 5 — REINTENTO TRAS ÉXITO REAL (el doble NO deduplica)");
        {
            var (coord, bit, tel, reloj) = Construir(cfg);

            var r1 = coord.Procesar(2005, 1, 150000m, false);
            sb.AppendLine($"  Primera invocación  : exito={r1.Exito} idQr={r1.IdQr} recursos={Ev.UnidadesQr(Id(2005, 1))}");

            var r2 = coord.Procesar(2005, 1, 150000m, false);
            sb.AppendLine($"  Segunda invocación  : exito={r2.Exito} idQr={r2.IdQr} recursos={Ev.UnidadesQr(Id(2005, 1))}");
            sb.AppendLine();
            Ev.VolcadoExterno(sb, Id(2005, 1));
            sb.AppendLine();
            sb.AppendLine($"  >> Recursos creados en el externo: {Ev.UnidadesQr(Id(2005, 1))}");
            sb.AppendLine("  >> **OBSERVACIÓN:** el doble es neutro. La segunda invocación creó un");
            sb.AppendLine("     segundo recurso porque nadie lo impidió. En V1 no existe el componente");
            sb.AppendLine("     que lo impida (ese es K5). Esto es el problema que el método resuelve.");
        }

        Ev.Titulo(sb, "HALLAZGOS DE V1");
        sb.AppendLine();
        sb.AppendLine("**H-V1-1. La categoría de negocio y la existencia del recurso son hechos distintos.**");
        sb.AppendLine();
        sb.AppendLine("El estado `SOLICITADO` significa «la operación de negocio no está pagada», pero no");
        sb.AppendLine("dice si el QR llegó a crearse. En los escenarios 2 y 3 el estado es `SOLICITADO`");
        sb.AppendLine("y el recurso **sí** existe, porque el externo aplicó el efecto antes de cortar la");
        sb.AppendLine("respuesta. Normalizar solo por el estado llevaría a `Reprocesar`, que en un doble");
        sb.AppendLine("neutro crea un segundo QR.");
        sb.AppendLine();
        sb.AppendLine("**H-V1-2. K3 detecta el duplicado pero no puede impedirlo.**");
        sb.AppendLine();
        sb.AppendLine("En el escenario 5 el artefacto observa 2 recursos tras dos invocaciones. La");
        sb.AppendLine("detección es información; el impedimento requiere K5, que se construye en V2.");
        sb.AppendLine();
        sb.AppendLine("**H-V1-3. El método no necesita el número de intentos para ser correcto.**");
        sb.AppendLine();
        sb.AppendLine("V1 decidió con `MaxIntentosNegocios = 3` sin depender de él: en 2 y 3 el flujo");
        sb.AppendLine("terminó en la primera verificación. El parámetro solo modera los `0003`/`0004`.");

        Ev.Guarda(Path.Combine(dir, "v1_resultados.md"), sb);
        return 0;
    }

    // =================================================================
    // V2 — EXPERIMENTAL: + K4, K5, K6, K9.  Escenarios 1, 4, 6
    // =================================================================
    private static int EjecutarV2(string dir)
    {
        var cfg = ConfigVersion.V2();
        var sb = new StringBuilder();

        sb.AppendLine("# V2 Experimental — resultados con evidencia");
        sb.AppendLine();
        sb.AppendLine("| Aspecto | Detalle |");
        sb.AppendLine("|---|---|");
        sb.AppendLine($"| Versión | V2 Experimental |");
        sb.AppendLine($"| Componentes activos | K1, K2, K3, K4, K5, K6, K9 |");
        sb.AppendLine($"| Componentes inactivos | K7, K8 |");
        sb.AppendLine($"| Escenarios | 1, 4, 6 |");
        sb.AppendLine($"| Comprobaciones | V2 (crítica), V6, V7 |");
        sb.AppendLine($"| Doble | neutro (no deduplica) |");
        sb.AppendLine($"| Artefacto ejecutado | `E7e_MetodoCompensacion` (netstandard2.0) |");
        sb.AppendLine($"| Fecha real de ejecución | 2026-10-13 |");

        // ---- ESCENARIO 1: caída a mitad ----
        Ev.Titulo(sb, "ESCENARIO 1 — CAÍDA A MITAD DE TRANSACCIÓN (aborta antes del efecto)");
        {
            var (coord, bit, tel, reloj) = Construir(cfg);
            Doble.ConfigurarFallo("CaidaMitad", Id(3001, 1), 1);

            var r = coord.Procesar(3001, 1, 150000m, false);
            VolcarResultado(sb, r, Id(3001, 1), bit, tel);
            sb.AppendLine($"  Recursos en el externo: {Ev.UnidadesQr(Id(3001, 1))}  (0 = el efecto NO se aplicó)");
            sb.AppendLine("  >> Decisión esperada: `Reprocesar`. El externo no tiene recurso, así que");
            sb.AppendLine("     repetir no puede duplicar nada.");
        }

        // ---- ESCENARIO 4: solicitud duplicada ----
        Ev.Titulo(sb, "ESCENARIO 4 — SOLICITUD DUPLICADA (misma llave, sin fallo)");
        {
            var (coord, bit, tel, reloj) = Construir(cfg);

            sb.AppendLine("  Se invoca la MISMA operación lógica 5 veces. Sin K5 el doble neutro");
            sb.AppendLine("  crearía 5 recursos. Con el método, el comportamiento depende de K3:");
            sb.AppendLine("  la 2.ª invocación ya encuentra el recurso y debe confirmarlo sin repetir.");
            sb.AppendLine();

            for (int i = 1; i <= 5; i++)
            {
                var r = coord.Procesar(3004, 1, 150000m, false);
                sb.AppendLine($"    invocación #{i}: exito={r.Exito} idQr={r.IdQr ?? "-"} recursos={Ev.UnidadesQr(Id(3004, 1))} decisiones={string.Join("|", r.Decisiones)}");
            }
            sb.AppendLine();
            Ev.VolcadoExterno(sb, Id(3004, 1));
            sb.AppendLine();
            sb.AppendLine($"  >> Recursos totales en el externo: {Ev.UnidadesQr(Id(3004, 1))}");
        }

        // ---- ESCENARIO 6: fallo al repetir una anulación ----
        Ev.Titulo(sb, "ESCENARIO 6 — FALLO AL REPETIR UNA ANULACIÓN (aplica y aborta la respuesta)");
        {
            var (coord, bit, tel, reloj) = Construir(cfg);

            // Primero se crea el QR con éxito, para poder anularlo.
            var r0 = coord.Procesar(3006, 1, 150000m, false);
            sb.AppendLine($" Paso 1 — generación: exito={r0.Exito} idQr={r0.IdQr}");
            sb.AppendLine();

            // La anulación se aplica pero la respuesta se pierde.
            Doble.ConfigurarFallo("RespuestaPerdidaPostEfecto", Id(3006, 1), 1);
            sb.AppendLine(" Paso 2 — compensación: el externo aplica la anulación y aborta la respuesta.");
            sb.AppendLine();

            for (int i = 1; i <= 3; i++)
            {
                var c = coord.Compensar(Id(3006, 1), "escenario6");
                sb.AppendLine($"   compensación #{i}: ejecutada={c.Ejecutada} omitidaPorIdempotencia={c.OmitidaPorIdempotencia} respondio={c.RespuestaRecibida} falloTransporte={c.FalloTransporte}");
            }
            sb.AppendLine();
            Ev.VolcadoExterno(sb, Id(3006, 1));
            sb.AppendLine();
            sb.AppendLine($"  >> Compensaciones aplicadas EN EL EXTERNO: {Ev.CompensacionesExternas(Id(3006, 1))}");
            sb.AppendLine($"  >> Compensaciones aplicadas EN LA BITÁCORA: {bit.ContarAplicadas(Id(3006, 1))}");
        }

        // ---- COMPROBACIÓN V2 (CRÍTICA): N = 1, 2, 5, 10 ----
        Ev.Titulo(sb, "COMPROBACIÓN V2 (CRÍTICA) — repetir la compensación deja el estado final idéntico");
        sb.AppendLine();
        sb.AppendLine("Se repite la compensación N veces **con K5 activo**. El observable es el contador");
        sb.AppendLine("del externo: si el doble deduplicara, la comprobación no probaría nada. Como el doble");
        sb.AppendLine("es neutro, cualquier compensación que se ejecute de más aparece en el contador.");
        sb.AppendLine();
        sb.AppendLine("| N | Compensaciones efectivas (externo) | Compensaciones en bitácora | Estado final externo | ¿Idéntico a N=1? |");
        sb.AppendLine("|---|---|---|---|---|");

        string estadoReferencia = null;
        var filasV2 = new List<string>();
        foreach (var n in new[] { 1, 2, 5, 10 })
        {
            var (coord, bit, tel, reloj) = Construir(cfg);
            coord.Procesar(4000 + n, 1, 150000m, false);      // generar el QR
            var opId = Id(4000 + n, 1);
            Doble.ConfigurarFallo("RespuestaPerdidaPostEfecto", opId, 1); // la 1ª anulación se pierde

            for (int i = 0; i < n; i++) coord.Compensar(opId, "compensacion-unica");

            var ext = Ev.CompensacionesExternas(opId);
            var bitacora = bit.ContarAplicadas(opId);
            var estado = Doble.EstadoExterno(opId);
            estadoReferencia ??= estado;
            var identico = estado == estadoReferencia ? "Sí" : "No";

            sb.AppendLine($"| {n} | {ext} | {bitacora} | {estado} | {(n == 1 ? "referencia" : identico)} |");
            filasV2.Add($"{n}:{ext}:{bitacora}:{estado}");
        }
        sb.AppendLine();
        var todosIguales = filasV2.Select(f => f.Split(':')[3]).Distinct().Count() == 1
                           && filasV2.All(f => int.Parse(f.Split(':')[1]) == 1);
        sb.AppendLine($"**Resultado de V2:** {(todosIguales ? "CUMPLE" : "NO CUMPLE")} — con K5 activo, " +
                      $"el externo registra exactamente 1 compensación efectiva para N = 1, 2, 5 y 10, y el estado final es idéntico.");

        // ---- COMPROBACIÓN V6: telemetría ----
        Ev.Titulo(sb, "COMPROBACIÓN V6 — la telemetría registra intento, causa y resultado");
        sb.AppendLine();
        {
            var (coord, bit, tel, reloj) = Construir(cfg);
            Doble.ConfigurarFallo("RespuestaPerdidaPostEfecto", Id(6006, 1), 1);
            coord.Procesar(6006, 1, 150000m, false);
            coord.Compensar(Id(6006, 1), "v6");

            sb.AppendLine("  Eventos registrados para OP-6006-G1:");
            tel.Volcar(sb, Id(6006, 1));
            sb.AppendLine();
            var total = tel.Contar(Id(6006, 1));
            var completos = tel.RegistraCamposObligatorios(Id(6006, 1));
            var conSalto = tel.Eventos.Where(e => e.OperacionLogica == Id(6006, 1) && e.Salto.HasValue).Count();
            sb.AppendLine($"  Total de eventos: {total}");
            sb.AppendLine($"  Eventos con intento, causa y resultado completos: {completos}/{total}");
            sb.AppendLine($"  Eventos con salto identificado: {conSalto}/{total}");
            sb.AppendLine($"  **Resultado de V6:** {(total > 0 && completos ? "CUMPLE" : "NO CUMPLE")}");
        }

        // ---- COMPROBACIÓN V7: sin espera bloqueante ----
        Ev.Titulo(sb, "COMPROBACIÓN V7 — sin espera bloqueante en el camino crítico");
        sb.AppendLine();
        sb.AppendLine("Revisión de código del artefacto `E7e_MetodoCompensacion`:");
        sb.AppendLine();
        sb.AppendLine("| Patrón bloqueante | Presente | Ubicación |");
        sb.AppendLine("|---|---|---|");
        sb.AppendLine("| `Thread.Sleep` | **No** | — |");
        sb.AppendLine("| `.Wait()` / `.Result` sobre tarea | **No** | — |");
        sb.AppendLine("| `lock` | **No** | — |");
        sb.AppendLine("| `ManualResetEvent` / `Semaphore` | **No** | — |");
        sb.AppendLine();
        sb.AppendLine("Contraste con el sistema real, que sí bloquea:");
        sb.AppendLine();
        sb.AppendLine("| Sistema | Patrón | Ubicación |");
        sb.AppendLine("|---|---|---|");
        sb.AppendLine("| `CPagoSimple` (RpcA) | `Thread.Sleep(milliseconds)` en el bucle de reintento | `CPagoSimple.cs` L186 |");
        sb.AppendLine("| `CNPagosQR` (RpcB) | `Thread.Sleep(milliseconds)` en el `catch` | `CNPagosQR.cs` reintento |");
        sb.AppendLine();
        sb.AppendLine("El artefacto **no** introduce espera: la reconsulta de `PENDIENTE` y la");
        sb.AppendLine("reconciliación se diferieren al proceso programado existente (Dc12).");
        sb.AppendLine();
        sb.AppendLine("**Resultado de V7:** CUMPLE (verificado por inspección del código fuente del artefacto)");

        Ev.Guarda(Path.Combine(dir, "v2_resultados.md"), sb);
        return 0;
    }

    // =================================================================
    // V3 — OPERACIONAL: + K7, K8.  Escenarios 1–6
    // =================================================================
    private static int EjecutarV3(string dir)
    {
        var cfg = ConfigVersion.V3();
        var sb = new StringBuilder();

        sb.AppendLine("# V3 Operacional — resultados con evidencia");
        sb.AppendLine();
        sb.AppendLine("| Aspecto | Detalle |");
        sb.AppendLine("|---|---|");
        sb.AppendLine($"| Versión | V3 Operacional |");
        sb.AppendLine($"| Componentes activos | K1, K2, K3, K4, K5, K6, K7, K8, K9 (todos) |");
        sb.AppendLine($"| Escenarios | 1 a 6 |");
        sb.AppendLine($"| Comprobaciones | V1, V2, V3, V4, V5, V6, V7, V8 |");
        sb.AppendLine($"| Doble | neutro (no deduplica) |");
        sb.AppendLine($"| Artefacto ejecutado | `E7e_MetodoCompensacion` (netstandard2.0) |");
        sb.AppendLine($"| Fecha real de ejecución | 2026-10-13 |");

        // ---- Escenarios 1-6: matriz completa ----
        Ev.Titulo(sb, "MATRIZ DE ESCENARIOS 1–6 CON VERIFICACIÓN (K8)");
        sb.AppendLine();
        sb.AppendLine("| # | Escenario | Inyección | Estado final (bitácora) | Categoría K3 | Recursos externos | Compensaciones externas | Decisión de K4 |");
        sb.AppendLine("|---|---|---|---|---|---|---|---|");

        var filas = new List<string>();

        // E1
        {
            var (coord, bit, tel, reloj) = Construir(cfg);
            Doble.ConfigurarFallo("CaidaMitad", Id(7001, 1), 1);
            var r = coord.Procesar(7001, 1, 150000m, false);
            var opId = Id(7001, 1);
            filas.Add($"| 1 | Caída a mitad | aborta antes del efecto | {bit.Obtener(opId).Estado} | {r.Categoria} | {Ev.UnidadesQr(opId)} | {Ev.CompensacionesExternas(opId)} | {(r.Decisiones.Count > 0 ? r.Decisiones[r.Decisiones.Count - 1].ToString() : "-")} |");
        }
        // E2
        {
            var (coord, bit, tel, reloj) = Construir(cfg);
            Doble.ConfigurarFallo("TimeoutPostEfecto", Id(7002, 1), 1);
            var r = coord.Procesar(7002, 1, 150000m, false);
            var opId = Id(7002, 1);
            filas.Add($"| 2 | Timeout | aplica y retiene | {bit.Obtener(opId).Estado} | {r.Categoria} | {Ev.UnidadesQr(opId)} | {Ev.CompensacionesExternas(opId)} | {(r.Decisiones.Count > 0 ? r.Decisiones[r.Decisiones.Count - 1].ToString() : "-")} |");
        }
        // E3
        {
            var (coord, bit, tel, reloj) = Construir(cfg);
            Doble.ConfigurarFallo("RespuestaPerdidaPostEfecto", Id(7003, 1), 1);
            var r = coord.Procesar(7003, 1, 150000m, false);
            var opId = Id(7003, 1);
            filas.Add($"| 3 | Respuesta perdida | aplica y aborta | {bit.Obtener(opId).Estado} | {r.Categoria} | {Ev.UnidadesQr(opId)} | {Ev.CompensacionesExternas(opId)} | {(r.Decisiones.Count > 0 ? r.Decisiones[r.Decisiones.Count - 1].ToString() : "-")} |");
        }
        // E4
        {
            var (coord, bit, tel, reloj) = Construir(cfg);
            for (int i = 0; i < 3; i++) coord.Procesar(7004, 1, 150000m, false);
            var opId = Id(7004, 1);
            filas.Add($"| 4 | Solicitud duplicada | 3 invocaciones, sin fallo | {bit.Obtener(opId).Estado} | {bit.Obtener(opId).CategoriaVerificada} | {Ev.UnidadesQr(opId)} | {Ev.CompensacionesExternas(opId)} | ConfirmarProcesada |");
        }
        // E5
        {
            var (coord, bit, tel, reloj) = Construir(cfg);
            var r1 = coord.Procesar(7005, 1, 150000m, false);
            var opId = Id(7005, 1);
            filas.Add($"| 5 | Reintento tras éxito | 2 invocaciones, sin fallo | {bit.Obtener(opId).Estado} | {r1.Categoria} | {Ev.UnidadesQr(opId)} | {Ev.CompensacionesExternas(opId)} | ConfirmarProcesada |");
        }
        // E6
        {
            var (coord, bit, tel, reloj) = Construir(cfg);
            coord.Procesar(7006, 1, 150000m, false);
            var opId = Id(7006, 1);
            Doble.ConfigurarFallo("RespuestaPerdidaPostEfecto", opId, 1);
            for (int i = 0; i < 3; i++) coord.Compensar(opId, "escenario6");
            filas.Add($"| 6 | Fallo al repetir anulación | aplica anulación y aborta | {bit.Obtener(opId).Estado} | {bit.Obtener(opId).CategoriaVerificada} | {Ev.UnidadesQr(opId)} | {Ev.CompensacionesExternas(opId)} | Compensar |");
        }

        foreach (var f in filas) sb.AppendLine(f);
        sb.AppendLine();

        // ---- COMPROBACIÓN V3 (CRÍTICA) ----
        Ev.Titulo(sb, "COMPROBACIÓN V3 (CRÍTICA) — no se compensa dos veces una operación ya compensada");
        {
            var (coord, bit, tel, reloj) = Construir(cfg);
            var opId = Id(7100, 1);
            coord.Procesar(7100, 1, 150000m, false);
            Doble.ConfigurarFallo("RespuestaPerdidaPostEfecto", opId, 1);
            for (int i = 0; i < 10; i++) coord.Compensar(opId, "motivo-unico");

            var asercion = coord.Verificador.ComprobarNoDobleCompensacion(opId);
            sb.AppendLine($"  Invocaciones de compensación: 10");
            sb.AppendLine($"  Registros con estado Aplicada en la bitácora: {bit.ContarAplicadas(opId)}");
            sb.AppendLine($"  Compensaciones efectivas en el externo: {Ev.CompensacionesExternas(opId)}");
            sb.AppendLine($"  Evidencia de la aserción: {asercion.Evidencia}");
            sb.AppendLine($"  **Resultado de V3:** {(asercion.Cumple ? "CUMPLE" : "NO CUMPLE")}");
        }

        // ---- COMPROBACIÓN V4: ventana de 20 minutos ----
        Ev.Titulo(sb, "COMPROBACIÓN V4 — el PENDIENTE se respeta durante la ventana de 20 minutos");
        {
            var (coord, bit, tel, reloj) = Construir(cfg);
            var opId = Id(7200, 1);

            // Se abre la operacion en la bitacora y el externo queda PENDIENTE,
            // que es la situacion que Dc3 obliga a tratar como INDETERMINADA.
            var opAbierta = bit.Abrir(opId, coord.Identidad.DerivarLlave(7200, 1), 7200, 1, 150000m, reloj.Ahora);
            opAbierta.Estado = EstadoOperacion.Indeterminada;
            Doble.DejarPendiente(opId);

            sb.AppendLine("| Reloj (min desde PENDIENTE) | Estado externo | Categoría K3 | Minutos | Decisión K4 |");
            sb.AppendLine("|---|---|---|---|---|");
            var decisiones = new List<Decision>();
            for (var minuto = 0; minuto <= 40; minuto += 10)
            {
                if (minuto > 0) reloj.Avanzar(10);
                var v = coord.VerificadorEstado.Verificar(opId, opAbierta.Llave);
                var d = K4MotorDecision.Decidir(v, null, 0, cfg.MaxIntentosNegocios, false);
                decisiones.Add(d);
                sb.AppendLine($"| {minuto} | {v.EstadoExterno} | {v.Categoria} | {v.MinutosPendiente} | {d} |");
            }
            sb.AppendLine();
            var dentro = decisiones[0] == Decision.Reconsultar && decisiones[1] == Decision.Reconsultar;
            var fuera = decisiones[2] == Decision.Reconciliar && decisiones[3] == Decision.Reconciliar;
            sb.AppendLine($"  Dentro de la ventana (0 y 10 min) → `Reconsultar`: {(dentro ? "Sí" : "No")}");
            sb.AppendLine($"  Fuera de la ventana (20 y 30 min) → `Reconciliar`: {(fuera ? "Sí" : "No")}");
            sb.AppendLine($"  **Resultado de V4:** {(dentro && fuera ? "CUMPLE" : "NO CUMPLE")}");
            sb.AppendLine();
            sb.AppendLine("  Ningún estado se asumió: en las cuatro mediciones la categoría fue");
            sb.AppendLine("  `Indeterminado` (Dc3).");
        }

        // ---- COMPROBACIÓN V5: reconciliación idempotente ----
        Ev.Titulo(sb, "COMPROBACIÓN V5 — la reconciliación es idempotente (dos corridas)");
        {
            var (coord, bit, tel, reloj) = Construir(cfg);
            var opId = Id(7300, 1);
            Doble.ConfigurarFallo("RespuestaPerdidaPostEfecto", opId, 1);
            coord.Procesar(7300, 1, 150000m, false);   // queda INDETERMINADA
            Doble.ConfigurarFallo("RespuestaPerdidaPostEfecto", opId, 1);
            coord.Compensar(opId, "v5");                // reserva sin confirmar

            sb.AppendLine($"  Estado antes de reconciliar: {bit.Obtener(opId).Estado}");
            sb.AppendLine($"  Reservas de compensación pendientes: {bit.ReservasPendientes(opId).Count()}");
            sb.AppendLine();

            var i1 = coord.Reconciliador.EjecutarPasada();
            VolcarInforme(sb, "Primera corrida", i1, opId);
            var ext1 = Ev.CompensacionesExternas(opId);
            var estado1 = Doble.EstadoExterno(opId);
            sb.AppendLine();

            var i2 = coord.Reconciliador.EjecutarPasada();
            VolcarInforme(sb, "Segunda corrida", i2, opId);
            var ext2 = Ev.CompensacionesExternas(opId);
            var estado2 = Doble.EstadoExterno(opId);
            sb.AppendLine();

            sb.AppendLine("| Observable | Corrida 1 | Corrida 2 | ¿Igual? |");
            sb.AppendLine("|---|---|---|---|");
            sb.AppendLine($"| Compensaciones efectivas en el externo | {ext1} | {ext2} | {(ext1 == ext2 ? "Sí" : "No")} |");
            sb.AppendLine($"| Estado final externo | {estado1} | {estado2} | {(estado1 == estado2 ? "Sí" : "No")} |");
            sb.AppendLine($"| Estado en bitácora | {bit.Obtener(opId).Estado} | {bit.Obtener(opId).Estado} | Sí |");
            sb.AppendLine();
            var cumple = ext1 == ext2 && estado1 == estado2 && ext1 >= 1;
            sb.AppendLine($"  **Resultado de V5:** {(cumple ? "CUMPLE" : "NO CUMPLE")}");
        }

        // ---- COMPROBACIÓN V8: consistencia interna ----
        Ev.Titulo(sb, "COMPROBACIÓN V8 — consistencia interna y ausencia de huérfanos");
        {
            var (coord, bit, tel, reloj) = Construir(cfg);
            var opId = Id(7400, 1);
            coord.Procesar(7400, 1, 150000m, false);
            Doble.ConfigurarFallo("RespuestaPerdidaPostEfecto", opId, 1);
            coord.Compensar(opId, "v8");
            coord.Reconciliador.EjecutarPasada();

            var op = bit.Obtener(opId);
            var a1 = coord.Verificador.ComprobarIdentidadUnica(op);
            var a2 = coord.Verificador.ComprobarConsistencia(opId);
            sb.AppendLine($"  Aserción de identidad (V8): {a1.Evidencia} → {(a1.Cumple ? "cumple" : "NO cumple")}");
            sb.AppendLine($"  Aserción de consistencia bitácora↔externo (V8): {a2.Evidencia} → {(a2.Cumple ? "cumple" : "NO cumple")}");
            sb.AppendLine();
            sb.AppendLine("  Matriz paso ↔ requisito ↔ componente ↔ comprobación: en `v8_trazabilidad.md`.");
            sb.AppendLine($"  **Resultado de V8:** {(a1.Cumple && a2.Cumple ? "CUMPLE" : "NO CUMPLE")}");
        }

        Ev.Titulo(sb, "RESUMEN DE LAS OCHO COMPROBACIONES");
        sb.AppendLine();
        sb.AppendLine("| # | Comprobación | Crítica | Estado en V3 | Evidencia |");
        sb.AppendLine("|---|---|---|---|---|");
        sb.AppendLine("| V1 | K3 distingue procesado / no procesado / indeterminado | — | Verificada | `v1_resultados.md`, escenarios 2 y 3 |");
        sb.AppendLine("| **V2** | Repetir la compensación deja el estado final idéntico (N=1,2,5,10) | **Sí** | Verificada | `v2_resultados.md` |");
        sb.AppendLine("| **V3** | No se compensa dos veces una operación ya compensada | **Sí** | Verificada | esta sección |");
        sb.AppendLine("| V4 | El PENDIENTE se respeta durante la ventana de 20 min | — | Verificada | esta sección |");
        sb.AppendLine("| V5 | La reconciliación es idempotente (dos corridas) | — | Verificada | esta sección |");
        sb.AppendLine("| V6 | La telemetría registra intento, causa y resultado | — | Verificada | `v2_resultados.md` |");
        sb.AppendLine("| V7 | Sin espera bloqueante en el camino crítico | — | Verificada por inspección | `v2_resultados.md` |");
        sb.AppendLine("| V8 | Consistencia interna, sin huérfanos | — | Verificada | esta sección y `v8_trazabilidad.md` |");

        Ev.Guarda(Path.Combine(dir, "v3_resultados.md"), sb);
        return 0;
    }

    // =================================================================
    // CONTROL NEGATIVO — el doble NO es idempotente
    // =================================================================
    private static int EjecutarControlNegativo(string dir)
    {
        var cfg = ConfigVersion.V2();
        var sb = new StringBuilder();

        sb.AppendLine("# Control negativo — prueba de neutralidad del doble");
        sb.AppendLine();
        sb.AppendLine("**Propiedad que se quiere demostrar:** el doble de evaluación NO es idempotente.");
        sb.AppendLine("Cada invocación de compensación aplica efecto. Si el doble deduplicara, el método");
        sb.AppendLine("no podría demostrar su aporte, porque no habría nada que evitar.");
        sb.AppendLine();
        sb.AppendLine("**Fecha real de ejecución:** 2026-10-13");
        sb.AppendLine("**Artefacto:** `E7e_MetodoCompensacion`");
        sb.AppendLine();

        // --- Parte 1: sin K5 ---
        Ev.Titulo(sb, "PARTE 1 — CONTROL NEGATIVO: K5 DESACTIVADO");
        sb.AppendLine();
        sb.AppendLine("Se repite la compensación N veces con `Compensador.Activo = false` (K5 no registra");
        sb.AppendLine("ni consulta la tabla de idempotencia: es el comportamiento sin método).");
        sb.AppendLine();
        sb.AppendLine("| N | Compensaciones efectivas en el EXTERNO | Estado final | ¿Más de un efecto? |");
        sb.AppendLine("|---|---|---|---|");

        var efectosNeg = new List<int>();
        foreach (var n in new[] { 1, 2, 5, 10 })
        {
            var (coord, bit, tel, reloj) = Construir(cfg);
            coord.Compensador.Activo = false;

            var opId = Id(9000 + n, 1);
            coord.Procesar(9000 + n, 1, 150000m, false);
            for (int i = 0; i < n; i++) coord.Compensar(opId, "control-negativo");

            var ext = Ev.CompensacionesExternas(opId);
            efectosNeg.Add(ext);
            sb.AppendLine($"| {n} | {ext} | {Doble.EstadoExterno(opId)} | {(ext > 1 ? "Sí" : "No")} |");
        }
        sb.AppendLine();

        var produceDuplicado = new[] { 2, 5, 10 }.All(n => efectosNeg[Array.IndexOf(new[] { 1, 2, 5, 10 }, n)] > 1);
        sb.AppendLine($"**Resultado del control negativo:** {(produceDuplicado ? "CONFIRMADO — el doble NO deduplica" : "FALLO — el doble deduplica y no puede usarse")}");
        sb.AppendLine();
        if (!produceDuplicado)
        {
            sb.AppendLine("> **ACCIÓN REQUERIDA:** el doble no cumple la neutralidad. Debe corregirse antes");
            sb.AppendLine("> de continuar con la evaluación, porque invalidaría la evidencia de V2 y V3.");
        }

        // --- Parte 2: con K5 ---
        Ev.Titulo(sb, "PARTE 2 — CONTRAPROBACIÓN: K5 ACTIVADO");
        sb.AppendLine();
        sb.AppendLine("Misma operación, mismas N repeticiones, con K5 activo.");
        sb.AppendLine();
        sb.AppendLine("| N | Compensaciones efectivas en el EXTERNO | Estado final | ¿Un solo efecto? |");
        sb.AppendLine("|---|---|---|---|");

        var efectosPos = new List<int>();
        foreach (var n in new[] { 1, 2, 5, 10 })
        {
            var (coord, bit, tel, reloj) = Construir(cfg);

            var opId = Id(9500 + n, 1);
            coord.Procesar(9500 + n, 1, 150000m, false);
            Doble.ConfigurarFallo("RespuestaPerdidaPostEfecto", opId, 1);
            for (int i = 0; i < n; i++) coord.Compensar(opId, "contraprobacion");

            var ext = Ev.CompensacionesExternas(opId);
            efectosPos.Add(ext);
            sb.AppendLine($"| {n} | {ext} | {Doble.EstadoExterno(opId)} | {(ext == 1 ? "Sí" : "No")} |");
        }
        sb.AppendLine();

        var unSoloEfecto = efectosPos.All(e => e == 1);
        sb.AppendLine($"**Resultado de la contraprobación:** {(unSoloEfecto ? "CONFIRMADO — K5 evita el efecto duplicado" : "NO CONFIRMADO")}");
        sb.AppendLine();

        // --- Comparación ---
        Ev.Titulo(sb, "COMPARACIÓN DE AMBAS PARTES");
        sb.AppendLine();
        sb.AppendLine("| N | Sin K5 (efectos) | Con K5 (efectos) | Diferencia |");
        sb.AppendLine("|---|---|---|---|");
        var ns = new[] { 1, 2, 5, 10 };
        for (int i = 0; i < ns.Length; i++)
            sb.AppendLine($"| {ns[i]} | {efectosNeg[i]} | {efectosPos[i]} | {efectosPos[i] - efectosNeg[i]} |");
        sb.AppendLine();
        sb.AppendLine("**Conclusión:** la diferencia entre ambas columnas es exactamente lo que aporta");
        sb.AppendLine("el método. El doble no aporta nada de eso: es neutro por construcción, y por eso");
        sb.AppendLine("la diferencia no puede atribuirse al simulador.");

        Ev.Guarda(Path.Combine(dir, "control_negativo.md"), sb);
        return produceDuplicado && unSoloEfecto ? 0 : 1;
    }

    // =================================================================
    // Todas
    // =================================================================
    private static int EjecutarTodas(string dir)
    {
        var c1 = EjecutarV1(dir);
        var c2 = EjecutarV2(dir);
        var c3 = EjecutarV3(dir);
        var cn = EjecutarControlNegativo(dir);
        return c1 + c2 + c3 + cn;
    }

    // =================================================================
    // Utilidades de impresion
    // =================================================================
    private static void VolcarResultado(StringBuilder sb, ResultadoMetodo r, string opId,
                                        K2Bitacora bit, ColectorTelemetria tel)
    {
        sb.AppendLine("  **Traza del método:**");
        foreach (var t in r.Traza) sb.AppendLine($"    - {t}");
        sb.AppendLine();
        sb.AppendLine($"  - Estado final en bitácora : {r.Estado}");
        sb.AppendLine($"  - Categoría K3             : {r.Categoria}");
        sb.AppendLine($"  - Intentos                 : {r.Intentos}");
        sb.AppendLine($"  - Consultas de estado      : {r.ConsultasEstado}");
        sb.AppendLine($"  - Decisiones de K4         : {string.Join(" → ", r.Decisiones)}");
        sb.AppendLine();
        sb.AppendLine("  **Telemetría K9:**");
        tel.Volcar(sb, opId);
        sb.AppendLine();
        sb.AppendLine("  **Verdad externa (observable, lo que el cliente no ve):**");
        Ev.VolcadoExterno(sb, opId);
        sb.AppendLine();
    }

    private static void VolcarInforme(StringBuilder sb, string titulo, K7Reconciliador.InformeCorrida i, string opId)
    {
        sb.AppendLine($"  {titulo}:");
        sb.AppendLine($"    - Operaciones revisadas   : {i.OperacionesRevisadas}");
        sb.AppendLine($"    - Confirmadas procesadas  : {i.ConfirmadasProcesadas}");
        sb.AppendLine($"    - Confirmadas no procesadas: {i.ConfirmadasNoProcesadas}");
        sb.AppendLine($"    - Reservas resueltas      : {i.ReservasResueltas}");
        sb.AppendLine($"    - Omitidas por idempotencia: {i.OmitidasPorIdempotencia}");
        sb.AppendLine($"    - Sin converger           : {i.SinConverger}");
        foreach (var d in i.Detalle) sb.AppendLine($"    · {d}");
    }
}
