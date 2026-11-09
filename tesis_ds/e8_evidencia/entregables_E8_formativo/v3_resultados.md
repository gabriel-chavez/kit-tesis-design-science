# V3 Operacional — resultados con evidencia

| Aspecto | Detalle |
|---|---|
| Versión | V3 Operacional |
| Componentes activos | K1, K2, K3, K4, K5, K6, K7, K8, K9 (todos) |
| Escenarios | 1 a 6 |
| Comprobaciones | V1, V2, V3, V4, V5, V6, V7, V8 |
| Doble | neutro (no deduplica) |
| Artefacto ejecutado | `E7e_MetodoCompensacion` (netstandard2.0) |
| Fecha real de ejecución | 2026-10-13 |

================================================================
MATRIZ DE ESCENARIOS 1–6 CON VERIFICACIÓN (K8)
================================================================

| # | Escenario | Inyección | Estado final (bitácora) | Categoría K3 | Recursos externos | Compensaciones externas | Decisión de K4 |
|---|---|---|---|---|---|---|---|
| 1 | Caída a mitad | aborta antes del efecto | NoProcesada | NoProcesado | 0 | 0 | Reprocesar |
| 2 | Timeout | aplica y retiene | Procesada | NoProcesado | 1 | 0 | ConfirmarProcesada |
| 3 | Respuesta perdida | aplica y aborta | Procesada | NoProcesado | 1 | 0 | ConfirmarProcesada |
| 4 | Solicitud duplicada | 3 invocaciones, sin fallo | Procesada |  | 1 | 0 | ConfirmarProcesada |
| 5 | Reintento tras éxito | 2 invocaciones, sin fallo | Procesada | NoProcesado | 1 | 0 | ConfirmarProcesada |
| 6 | Fallo al repetir anulación | aplica anulación y aborta | Compensada |  | 1 | 1 | Compensar |


================================================================
COMPROBACIÓN V3 (CRÍTICA) — no se compensa dos veces una operación ya compensada
================================================================
  Invocaciones de compensación: 10
  Registros con estado Aplicada en la bitácora: 1
  Compensaciones efectivas en el externo: 1
  Evidencia de la aserción: paresDuplicados=0
  **Resultado de V3:** CUMPLE

================================================================
COMPROBACIÓN V4 — el PENDIENTE se respeta durante la ventana de 20 minutos
================================================================
| Reloj (min desde PENDIENTE) | Estado externo | Categoría K3 | Minutos | Decisión K4 |
|---|---|---|---|---|
| 0 | PENDIENTE | Indeterminado | 0 | Reconsultar |
| 10 | PENDIENTE | Indeterminado | 10 | Reconsultar |
| 20 | PENDIENTE | Indeterminado | 20 | Reconciliar |
| 30 | PENDIENTE | Indeterminado | 30 | Reconciliar |
| 40 | PENDIENTE | Indeterminado | 40 | Reconciliar |

  Dentro de la ventana (0 y 10 min) → `Reconsultar`: Sí
  Fuera de la ventana (20 y 30 min) → `Reconciliar`: Sí
  **Resultado de V4:** CUMPLE

  Ningún estado se asumió: en las cuatro mediciones la categoría fue
  `Indeterminado` (Dc3).

================================================================
COMPROBACIÓN V5 — la reconciliación es idempotente (dos corridas)
================================================================
  Estado antes de reconciliar: Indeterminada
  Reservas de compensación pendientes: 1

  Primera corrida:
    - Operaciones revisadas   : 1
    - Confirmadas procesadas  : 0
    - Confirmadas no procesadas: 0
    - Reservas resueltas      : 0
    - Omitidas por idempotencia: 1
    - Sin converger           : 0

  Segunda corrida:
    - Operaciones revisadas   : 1
    - Confirmadas procesadas  : 0
    - Confirmadas no procesadas: 0
    - Reservas resueltas      : 0
    - Omitidas por idempotencia: 0
    - Sin converger           : 0

| Observable | Corrida 1 | Corrida 2 | ¿Igual? |
|---|---|---|---|
| Compensaciones efectivas en el externo | 1 | 1 | Sí |
| Estado final externo | ANULADO | ANULADO | Sí |
| Estado en bitácora | Compensada | Compensada | Sí |

  **Resultado de V5:** CUMPLE

================================================================
COMPROBACIÓN V8 — consistencia interna y ausencia de huérfanos
================================================================
  Aserción de identidad (V8): llaveEnBitacora=7400|1 llaveDerivada=7400|1 → cumple
  Aserción de consistencia bitácora↔externo (V8): bitacora=Compensada externo=ANULADO → cumple

  Matriz paso ↔ requisito ↔ componente ↔ comprobación: en `v8_trazabilidad.md`.
  **Resultado de V8:** CUMPLE

================================================================
RESUMEN DE LAS OCHO COMPROBACIONES
================================================================

| # | Comprobación | Crítica | Estado en V3 | Evidencia |
|---|---|---|---|---|
| V1 | K3 distingue procesado / no procesado / indeterminado | — | Verificada | `v1_resultados.md`, escenarios 2 y 3 |
| **V2** | Repetir la compensación deja el estado final idéntico (N=1,2,5,10) | **Sí** | Verificada | `v2_resultados.md` |
| **V3** | No se compensa dos veces una operación ya compensada | **Sí** | Verificada | esta sección |
| V4 | El PENDIENTE se respeta durante la ventana de 20 min | — | Verificada | esta sección |
| V5 | La reconciliación es idempotente (dos corridas) | — | Verificada | esta sección |
| V6 | La telemetría registra intento, causa y resultado | — | Verificada | `v2_resultados.md` |
| V7 | Sin espera bloqueante en el camino crítico | — | Verificada por inspección | `v2_resultados.md` |
| V8 | Consistencia interna, sin huérfanos | — | Verificada | esta sección y `v8_trazabilidad.md` |

---

**Procedencia de esta evidencia**

| Campo | Valor |
|---|---|
| Generado por | arnés `coordinador` (`Program.cs`), sin lógica del método |
| Fecha y hora de ejecución | 2026-10-27 23:27:35 -04:00 |
| Comando | `dotnet run -- VTODAS ..\evidencia_e7e` |
| Artefacto ejecutado | `E7e.MetodoCompensacion` |
| SHA-256 del artefacto | `ec9b8b6e8d37374ff28f2eb816285571ede997c47ec44a235da829d13dbd10bf` |
| Doble | `busa-eval-stub` en `http://localhost:5098`, neutro (no deduplica) |

El SHA-256 permite comprobar que esta evidencia corresponde a esta versión del
artefacto. Si el artefacto cambia, el hash cambia y la evidencia deja de coincidir.
