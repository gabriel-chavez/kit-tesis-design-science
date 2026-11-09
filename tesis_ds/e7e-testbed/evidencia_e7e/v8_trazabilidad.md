# Matriz de trazabilidad — paso ↔ requisito ↔ componente ↔ comprobación

**Etapa:** E7e
**Fecha real:** 2026-10-13
**Artefacto:** `E7e_MetodoCompensacion` (netstandard2.0, BCL únicamente)

Esta matriz responde a la comprobación **V8**: *consistencia interna, sin huérfanos*. Un
huérfano sería un paso sin componente, un componente sin requisito, un requisito sin
comprobación, o un componente que no aparece en ningún paso.

---

## 1. Requisitos → componente → decisión → comprobación

| Requisito | Componente | Decisión | Comprobación | Archivo del artefacto |
|---|---|---|---|---|
| F1 | K1 Generador de identidad | Dc1 | V8 (aserción de identidad) | `K1_K2_K9.cs` |
| F2 | K2 Bitácora | Dc4 | V8, V6 | `K1_K2_K9.cs` |
| F3 | K3 Verificador | Dc2, Dc3 | **V1**, V4 | `K3_K4.cs` |
| F4 | K4 Motor de decisión | Dc3 | V1, matriz de escenarios | `K3_K4.cs` |
| F5 | K2 + K5 Compensador | Dc5 | **V2**, **V3** | `K5_K6.cs` |
| F6 | K6 Ordenador | Dc7 | V2, escenario 6 | `K5_K6.cs` |
| F7 | K7 Reconciliador | Dc3, Dc9, Dc12 | **V5** | `K7_K8.cs` |
| F8 | K8 Verificador | — | **V2**, **V3**, V8 | `K7_K8.cs` |
| F9 | K9 Telemetría | Dc6 | **V6** | `K1_K2_K9.cs` |
| C1 (sin mensajería) | Puertos | — | Inspección de código | `Puertos/Puertos.cs` |
| C4 (sin espera bloqueante) | K4, K9 | Dc6 | **V7** | todo el artefacto |

## 2. Pasos del método → componente → comprobación

Los 14 pasos de `metodo.md` §3.3.

| Paso | Actividad | Componente | Código | Comprobación |
|---|---|---|---|---|
| 1 | Asignar identidad determinista | K1 | `CoordinadorMetodo.Procesar` | V8 |
| 2 | Registrar estado inicial | K2 | `K2Bitacora.Abrir` | V8 |
| 3 | Ejecutar la cadena | — (orquestación) | `CoordinadorMetodo.Procesar` | Escenarios 1–6 |
| 4 | Registrar «procesada» | K2 | `Procesar` rama 0000 | Escenario 5 |
| 5 | Reintento por `0003`/`0004` | K4, K9 | `Procesar` rama `EsErrorNegocio` | V3 (tasa) |
| 6 | Consultar el estado ante sospecha | K3 | `K3Verificador.Verificar` | **V1** |
| 7 | Normalizar | K3 | `K3Verificador.Normalizar` | **V1** |
| 8 | Decidir la acción | K4 | `K4MotorDecision.Decidir` | V1, matriz |
| 9 | Reconsultar PENDIENTE dentro de la ventana | K4 | `Decidir` rama `PENDIENTE` | **V4** |
| 10 | Enviar a reconciliación | K4, K7 | `Decidir` rama `Reconciliar` | **V4**, **V5** |
| 11 | Compensar de forma idempotente y ordenada | K5, K6 | `CoordinadorMetodo.Compensar` | **V2**, **V3** |
| 12 | Reconciliar operaciones huérfanas | K7 | `K7Reconciliador.EjecutarPasada` | **V5** |
| 13 | Verificar idempotencia y consistencia | K8 | `K8Verificador` | **V2**, **V3**, V8 |
| 14 | Registrar telemetría con el salto | K9 | `K9Telemetria.Registrar` | **V6** |

## 3. Decisiones de diseño → dónde se materializan

| Decisión | Enunciado | Dónde se cumple | Verificado por |
|---|---|---|---|
| Dc1 | Llave derivada de `(TRAMITE_NUMERO, GESTION)` | `K1GeneradorIdentidad.DerivarLlave` | V8 |
| Dc2 | Verificar el estado ante sospecha, no siempre | `Procesar` solo llama a `K3` tras fallo de transporte | V1, matriz |
| Dc3 | `PENDIENTE` = indeterminado; reconsultar y luego reconciliar | `K4.Decidir` rama `PENDIENTE` | **V4** |
| Dc4 | Reutilizar tablas existentes y crear solo la de idempotencia | `Sql/01_tabla_idempotencia_compensaciones.sql` | V8 |
| Dc5 | Compensación por `(operación, motivo)` con registro previo | `K5.Compensar` → `IntentarReservar` antes de ejecutar | **V2**, **V3** |
| Dc6 | No bloquear el hilo | `Puertos` sin esperas; reconsulta diferida | **V7** |
| Dc7 | Orden inverso de compensaciones | `K6.Planificar` itera `Saltos` al revés | V2 escenario 6 |
| Dc8 | Solo se compensa la anulación del QR no pagado | `K4.Compensable` | V2, V3 |
| Dc9 | Reconciliación con servicios Windows existentes | `K7.EjecutarPasada` invocable por lote | V5 |
| Dc10 | Índice único sobre la llave lógica | `Sql/01_...sql` sección 3, **comentado** | Pendiente de aplicar |
| Dc11 | Cubrir RpcA y RpcB | `Procesar(forzarFalloSaltoInterno)` cubre el salto interno | V1, V2 |
| Dc12 | Reconciliación con el servicio de convergencia existente | `K7` como paso de lote | V5 |
| Dc13 | Compensación agnóstica a la acción | `K6.CompensacionDe(salto)` por salto, no por acción | V2 |

## 4. Comprobación de huérfanos

| Búsqueda | Resultado |
|---|---|
| Requisito sin componente | **0** — F1…F9 y C1, C4 tienen componente asignado |
| Componente sin requisito | **0** — K1…K9 mapean a F1…F9 |
| Componente sin paso en el flujo | **0** — K1…K9 aparecen en los pasos 1–14 |
| Requisito sin comprobación | **0** — F1…F9 mapean a V1…V8 |
| Paso sin componente | **0** — el paso 3 es orquestación, no un componente |
| Componente sin archivo | **0** — los 9 están en 5 archivos `.cs` |
| Decisión sin materialización | **0** — Dc1…Dc13 tienen location en la sección 3 |

## 5. Componentes que permanecen sin ejercitar en la evaluación

Declarado explícitamente para no presentar cobertura como total.

| Componente | Situación | Consecuencia |
|---|---|---|
| `K6.EjecutarPlan` | **No invocada por el arnés.** El coordinador usa el bucle equivalente en `Compensar` | Riesgo de divergencia entre ambas rutas. Se reporta como pendiente de unificación |
| `K2.LiberarReserva` / `MarcarAbandonada` | Implementadas, **no ejercitadas** | El escenario de «proceso muere entre el registro y la llamada» no se probó |
| `K1.ExigirLlaveCorrecta` | Implementada, **no invocada** por el arnés | La aserción de identidad de V8 usa `ComprobarIdentidadUnica`, no el Lanzamiento de excepción |
| Dc10 (índice único) | DDL **producido, no ejecutado** | No verificado en base de datos |

## 6. Resultado de V8

**CUMPLE**, con dos salvedades registradas en la sección 5: cuatro miembros del artefacto
están implementados pero no ejercitados por los escenarios 1–6, y la condición Dc10 no se
aplicó contra ninguna base de datos.

Las aserciones ejecutadas fueron:

| Aserción | Evidencia |
|---|---|
| Identidad única y reproducible | `llaveEnBitacora=7400\|1 llaveDerivada=7400\|1` |
| Consistencia bitácora ↔ externo | `bitacora=Compensada externo=ANULADO` |