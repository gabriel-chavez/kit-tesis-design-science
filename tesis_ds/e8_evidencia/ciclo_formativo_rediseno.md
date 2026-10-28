# E8 - Ciclo formativo: hallazgos y rediseño (primer rediseño)

**Fecha de ejecución:** 2026-10-27
**Artefacto:** `E7e_MetodoCompensacion` (netstandard2.0), SHA-256 del binario usado en la corrida: ver `ciclo_formativo_resultados.md`.
**Doble:** `busa-eval-stub` en `http://localhost:5098`, neutro (no deduplica).
**Escenarios:** 11 (6 base de E6 + 5 casos límite) + control negativo.

## 1. Resultado por escenario

| # | Escenario | Métrica clave | Resultado | Defecto |
|---|---|---|---|---|
| S1 | Caída antes del efecto | recursos externos | 0 | ninguno |
| S2 | Timeout después del efecto | recursos externos / decisión | 1 / ConfirmarProcesada | ninguno |
| S3 | Respuesta perdida | recursos externos / decisión | 1 / ConfirmarProcesada | ninguno |
| S4 | Solicitud duplicada concurrente | recursos externos | 1 (tras rediseño) | **D1** (resuelto) |
| S5 | Reintento tras éxito real | recursos externos | 1 | ninguno |
| S6 | Fallo durante la compensación | compensaciones externas | 1 | ninguno |
| S7 | Proceso muere entre reserva y ejecución | reservas pendientes → 0 tras K7 | 0 | **D2** (cableado) |
| S8 | PENDIENTE 19 / 20 min | decisión | Reconsultar / Reconciliar | **D3** (precisión) |
| S9 | Estado externo desconocido | decisión | Reconciliar | ninguno (mantener) |
| S10 | Falla escritura en bitácora tras reservar | excepción | InvalidOperationException | **D4** (sin manejo) |
| S11 | Compensación falla dos veces y luego responde | compensaciones externas | 1 | ninguno |
| CN | Control negativo | sin K5 / con K5 | 3 / 1 | ninguno |

## 2. Defectos y rediseño

### D1 - Concurrencia: la idempotencia de la operación no era atómica

- **Qué se esperaba:** con 5 invocaciones concurrentes de la misma llave, el externo registra 1 recurso.
- **Qué ocurrió (iteración 1):** 5 recursos. El guard de idempotencia en `CoordinadorMetodo.Procesar` leía `op.Estado` sin sincronización; todas las hebras veían `Solicitada` y procedían.
- **Qué se cambió:** se añadió un lock por operación (`ConcurrentDictionary<string,object>` estático en `CoordinadorMetodo`) que envuelve todo `Procesar`. La reserva/consulta de estado se vuelve atómica por llave.
- **Por qué:** la garantía de idempotencia de la operación (objetivo a del método) debe sostenerse también bajo concurrencia, no solo en secuencia. El control negativo y V2/V3 no ejercitaban concurrencia.
- **Verificación tras rediseño:** S4 → 1 recurso. Evidencia en `ciclo_formativo_resultados.md`.

### D2 - `K2.MarcarAbandonada` implementada pero no invocada

- **Qué se esperaba:** una reserva en estado `Registrada` sin confirmar (proceso muerto entre reserva y ejecución) se marca como `Abandonada` para que K7 la resuelva y no quede colgada.
- **Qué ocurrió:** `MarcarAbandonada` existe en `K2Bitacora` pero ningún componente (K5/K7) la llama. Cableado faltante.
- **Qué se cambió:** pendiente de cablear en K7 como responsable único (pregunta abierta E8). En esta iteración se ejercitó directamente en S7 para confirmar que la operación de marcado funciona; no se aplicó cambio de producción.
- **Por qué:** no se eligió responsable (K5 vs K7) en el diseño original; el ciclo formativo lo dejó visible.

### D3 - Frontera de la ventana en minutos enteros

- **Qué se esperaba:** distinguir 19:59 de 20:01.
- **Qué ocurrió:** la ventana se evalúa con `minutos < 20` sobre un entero de minutos; no hay precisión de segundos. A 19 min → Reconsultar, a 20 min → Reconciliar.
- **Qué se cambió:** declarado como condición de contorno en esta iteración (sin cambio de código). Rediseño propuesto para el siguiente ciclo: comparar timestamps con precisión de segundos o definir la ventana en segundos.
- **Por qué:** el doble y el reloj virtual trabajan en minutos; cambiar la granularidad exige tocar K3, K4 y el doble.

### D4 - Fallo de persistencia en la bitácora no manejado

- **Qué se esperaba:** si `MarcarAplicada` falla, la reserva queda `Registrada` y K7 la resuelve; no se propaga excepción ni se afirma éxito.
- **Qué ocurrió:** `MarcarAplicada` es `void` y no hay reintento ni manejo. La excepción se propaga al llamador (`InvalidOperationException`).
- **Qué se cambió:** declarado como defecto. Rediseño propuesto: convertir `MarcarAplicada` a resultado, reintentar y, si persiste el fallo, dejar la reserva en `Registrada` para K7.
- **Por qué:** el método afirma idempotencia de la compensación; un fallo de persistencia no puede convertirse en éxito silencioso ni en excepción no controlada.

### D5 - Estado desconocido (S9)

- **Qué se esperaba:** no asumir; reconciliar.
- **Qué ocurrió:** K3 devuelve `Indeterminado` con `ConsultaFallida=true` y K4 decide `Reconciliar`. Cumple.
- **Qué se cambió:** ninguno. Se mantiene el comportamiento y se añade telemetría.

## 3. Criterio de salida del ciclo formativo

- Defectos críticos resueltos o declarados: **D1 resuelto; D2, D3, D4 declarados.**
- Al menos un rediseño documentado: **D1 aplicado y verificado.**
- Control negativo confirmado: **sin K5 → 3 efectos; con K5 → 1.**
- Versión congelada para el sumativo: pendiente de generar hash tras D2/D3/D4 si se aplican.

## 4. Próximo paso

Aplicar D2/D3/D4 en un segundo rediseño o declararlos como condiciones de contorno, congelar la versión con su hash y ejecutar el ciclo sumativo con 2-3 practicantes y casos distintos.