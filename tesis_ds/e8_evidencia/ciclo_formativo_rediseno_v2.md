# E8 - Ciclo formativo: hallazgos y rediseño

**Fecha de ejecución:** 2026-10-27
**Artefacto:** `E7e_MetodoCompensacion` (netstandard2.0)
**SHA-256 de la versión congelada:** `ec9b8b6e8d37374ff28f2eb816285571ede997c47ec44a235da829d13dbd10bf`
**Doble:** `busa-eval-stub` en `http://localhost:5098`, neutro (no deduplica).
**Escenarios:** 11 (6 base de E6 + 5 casos límite) + control negativo.

## 1. Resultado por escenario

| # | Escenario | Métrica clave | Resultado | Estado |
|---|---|---|---|---|
| S1 | Caída antes del efecto | recursos externos | 0 | cumple |
| S2 | Timeout después del efecto | recursos / decisión | 1 / ConfirmarProcesada | cumple |
| S3 | Respuesta perdida | recursos / decisión | 1 / ConfirmarProcesada | cumple |
| S4 | Solicitud duplicada concurrente | recursos externos | 1 | cumple (una instancia) |
| S5 | Reintento tras éxito real | recursos externos | 1 | cumple |
| S6 | Fallo durante la compensación | compensaciones externas | 1 | cumple |
| S7 | Proceso muere entre reserva y ejecución | reserva → Abandonada → Aplicada | 1 compensación, 0 pendientes | cumple |
| S8 | PENDIENTE 19 / 20 min | decisión | Reconsultar / Reconciliar | cumple (granularidad declarada) |
| S9 | Estado externo desconocido | decisión | Reconciliar | cumple |
| S10 | Falla escritura en bitácora tras reservar | excepción / recuperación | ninguna excepción; K7 cierra la reserva | cumple |
| S11 | Compensación falla dos veces y luego responde | compensaciones externas | 1 | cumple |
| CN | Control negativo | sin K5 / con K5 | 3 / 1 | cumple |

## 2. Defectos y rediseño

### D1 - Concurrencia: la idempotencia de la operación no era atómica

- **Qué se esperaba:** con 5 invocaciones concurrentes de la misma llave, el externo registra 1 recurso.
- **Qué ocurrió (iteración 1):** 5 recursos. El guard de idempotencia en `CoordinadorMetodo.Procesar` leía `op.Estado` sin sincronización.
- **Qué se cambió:** lock por operación (`ConcurrentDictionary<string,object>` estático) que envuelve `Procesar`.
- **Por qué:** la idempotencia de la operación debe sostenerse también bajo concurrencia.
- **Verificación:** S4 → 1 recurso.

**Límite declarado:** el lock es **de proceso**. En un despliegue con dos o más instancias no coordina entre ellas. La garantía distribuida depende del índice único y de la inserción atómica de **Dc10**. Sin Dc10, el método no puede afirmar idempotencia distribuida. **Dc10 pasa de pendiente a condición necesaria del método.**

### D2 - `K2.MarcarAbandonada` implementada pero no invocada

- **Qué se esperaba:** una reserva `Registrada` sin confirmar (proceso muerto entre reserva y ejecución) se marca `Abandonada` y K7 la resuelve.
- **Qué ocurrió:** `MarcarAbandonada` existía pero ningún componente la llamaba.
- **Qué se cambió:** K7 es el responsable único. Marca `Abandonada` la reserva huérfana; `ReservasPendientes` y K5 incluyen `Abandonada` para resolverla en la siguiente pasada. Se añadió telemetría `RESERVA_ABANDONADA`.
- **Por qué:** sin el cableado, la reserva quedaba colgada para siempre.
- **Verificación:** S7, dos pasadas de K7: pasada 1 → `Abandonada`; pasada 2 → `Aplicada`; 1 compensación externa; 0 pendientes.

### D3 - Frontera de la ventana en minutos enteros

- **Qué se esperaba:** decidir explícitamente la granularidad.
- **Decisión:** la ventana es de **20 minutos completos**. No distingue 19:59 de 20:01. Se declara como condición de contorno; no se afirma precisión de segundos.
- **Verificación:** S8, 19 min → Reconsultar; 20 min → Reconciliar.

### D4 - Fallo de persistencia en la bitácora no manejado

- **Qué se esperaba:** si `MarcarAplicada` falla, la reserva queda `Registrada` y K7 la resuelve; sin excepción no controlada.
- **Qué ocurrió:** `MarcarAplicada` era `void`; la excepción se propagaba.
- **Qué se cambió:** `MarcarAplicada` devuelve `bool`; K5 reintenta (2 intentos) y, si falla, deja la reserva `Registrada`, marca la operación `Indeterminada` y no propaga la excepción. K7 la resuelve después.
- **Verificación:** S10, 2 fallos simulados: sin excepción; reserva final `Aplicada`; 1 compensación externa; 0 pendientes.

### D5 - Estado desconocido (S9)

- **Qué se esperaba:** no asumir; reconciliar.
- **Qué ocurrió:** K3 devuelve `Indeterminado` con `ConsultaFallida=true`; K4 decide `Reconciliar`.
- **Cambio:** ninguno; se mantiene.

## 3. Criterio de salida del ciclo formativo

- Defectos críticos resueltos o declarados: **D1 resuelto (con límite distribuido declarado); D2 y D4 resueltos; D3 declarado.**
- Al menos un rediseño documentado: **D1, D2 y D4 aplicados y verificados.**
- Control negativo confirmado: **sin K5 → 3 efectos; con K5 → 1.**
- Versión congelada: **SHA-256 `ec9b8b6e8d37374ff28f2eb816285571ede997c47ec44a235da829d13dbd10bf`.**

## 4. Próximo paso

Ciclo sumativo con 2–3 practicantes independientes, casos distintos, comparación contra la línea base de E6 y análisis de amenazas. No iniciar el sumativo con otra versión del método.
