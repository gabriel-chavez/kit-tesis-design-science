# E6. Diagnóstico con indicadores y criterios

> **Estado:** cerrada. Línea base **retrospectiva** (2025) y **experimental** (2026-10-08) completas. El detalle experimental, con su evidencia y sus declaraciones de alcance, está en el anexo `06_anexo_linea_base_experimental.md`. Los criterios de éxito se congelaron **antes** de construir.

## 1. Evidencia del problema (multi-fuente)

| Fuente | Evidencia | Estado |
|:--|:--|:--|
| Literatura | La idempotencia y la deduplicación son requisitos estructurales (Helland, 2012); la verificación de idempotencia tiene antecedente (Hummer et al., 2013) | Documentada |
| Código y configuración | Dos rutas de reintento divergentes (RpcA `CPagoSimple`, RpcB `CNPagosQR`); sin timeout de transporte configurado; espera bloqueante de 1 s | Documentada |
| Diagnóstico retrospectivo 2025 | 4.302 errores no controlados y 226 timeouts | Documentada |
| Tickets de soporte | 69 casos EFECTIVIZAR (proxy) | Documentada |
| Entrevista al personal de soporte | Realizada el 2026-10-05 | Documentada |
| Línea base experimental | Réplica del algoritmo + doble HTTP sobre datos sintéticos; 16 pruebas, 14 operaciones, 34 solicitudes | Documentada (anexo) |

## 2. Advertencias metodológicas

**Subregistro.** La ausencia de instrumentación no prueba la ausencia del problema: solo prueba que no puede detectarse. Se distingue **no ocurrió** de **no se registró**.

**Sobre los tickets (aclaración del tesista).** Los tickets no son un registro exhaustivo ni individualizado: un mismo ticket puede agrupar varias operaciones. Por eso la cantidad de tickets **no equivale** a la cantidad de incidentes ni a la de operaciones afectadas; se usan como evidencia de intervenciones documentadas y se analizan **por separado** de los errores no controlados, sin considerarlos equivalentes ni directamente comparables. La ausencia de ticket no demuestra la ausencia de incidente.

## 3. Dos líneas base (completas)

1. **Retrospectiva (2025):** datos de producción de solo lectura.
2. **Experimental (2026-10-08):** réplica del algoritmo y doble HTTP en entorno aislado; **no se contactó BUSA real** (`172.30.140.139:443` alcanzable, deliberadamente no invocado) y **no se modificó producción** (solo `SELECT`).

## 4. Indicadores (estado final)

| # | Indicador | Valor | Alcance / advertencia |
|:--|:--|:--|:--|
| I1 | Operaciones con estado inconsistente | ~69 (proxy) | Límite inferior; tickets agrupan varios casos |
| I2 | Errores no controlados (2025) | 4.302; 226 timeouts | Concentrados en enero (1.865) y diciembre (1.843) |
| I3 | Tasa de reintentos | **No instrumentado en producción** | Tasas observadas por rama de código, denominadores de 2 a 4 operaciones; no estiman volumen |
| I4 | Intervenciones manuales (2025) | 69 tickets | Proxy; subrepresenta |
| I5 | Tiempo de resolución | 9,67 h | `T_QR_SOLICITUD.TIEMPO_RESPUESTA` |
| I6 | Compensaciones con efecto duplicado | Sin efecto adicional en los contadores del doble | Mide el doble, diseñado idempotente por referencia; **no demuestra idempotencia de BUSA** |
| I7 | Cobertura de modos de fallo | 6/6 = 100 % | Sobre el conjunto definido por el investigador |

## 5. Mecanismo de reintento actual (dos rutas divergentes)

| | Ruta A (`CPagoSimple`) | Ruta B (`CNPagosQR`) |
|:--|:--|:--|
| Clave | `NumeroIntentos`, editado manualmente en producción (de 1 a 10 según la transaccionalidad; 1 al momento de la inspección) | `RepetirIntento` = true (no existe `NumeroIntentos`) |
| Mecanismo | bucle `while` | reintento único dentro de `catch` |
| Dispara ante timeout / transporte | **No** | **Sí** (cualquier excepción) |
| Dispara ante `0003`/`0004` | Sí | No evalúa el código |
| Espera | 1000 ms (`Thread.Sleep`, bloqueante) | 1000 ms (bloqueante) |

**Hallazgo de la discrepancia:** la misma condición de fallo produce comportamientos opuestos según la ruta de entrada. Cualquier método de compensación idempotente debe resolver esta divergencia antes de aplicarse.

**Sin timeout de transporte configurado:** se usa el valor por defecto de WCF.

**Valor dinámico (2026-10-08):** `NumeroIntentos` en producción **no es fijo**: el encargado lo ajusta manualmente hasta 10 en los meses de mayor transaccionalidad, según la carga. Al momento de la inspección estaba en 1; la línea base experimental usó 5 como parámetro de sensibilidad, sin tocar producción. La **ausencia de registro de estos cambios** es en sí misma un problema de trazabilidad y refuerza el requisito F9 (telemetría).

## 6. Comportamiento por escenario (resumen)

| Escenario | Estado final en el cliente |
|:--|:--|
| Éxito | Normal |
| `0003`/`0004` | Reintenta según la ruta; puede continuar con datos inválidos |
| Timeout o respuesta perdida | **Indeterminado** (10002 en RpcA, 0 en RpcB) |
| Procesada con respuesta perdida | **Indeterminado**, indistinguible del anterior |
| Anulación de QR no pagado | `0000` (anulada) |
| Anulación de QR pagado | `0009` (rechazada, fuera de alcance) |

## 7. Hallazgos de la línea base experimental

| # | Hallazgo | Tipo |
|:--|:--|:--|
| H1 | No puede distinguir "no procesada" de "procesada con respuesta perdida" | Sistema |
| H2 | Rutas de reintento divergentes entre RpcA y RpcB | Sistema |
| H3 | La base de producción no garantiza unicidad: 44 pares duplicados en 892.970 filas y ningún índice único sobre la llave lógica | Sistema |
| H4 | El mecanismo de convergencia existe (`WinServiceSoatConsultaPagosQR`, cada 10 s) pero **no se invoca desde los caminos de error** | Sistema |
| H5 | `NumeroIntentos = 1` impide converger ante errores de negocio transitorios resueltos en el 2.º o 3.er intento | Banco de pruebas |
| H6 | Aumentar los intentos **no resuelve** la incertidumbre de respuesta perdida (siguen 1 sola solicitud) | Banco de pruebas |

## 8. Riesgos identificados

| Riesgo | Descripción | Severidad |
|:--|:--|:--|
| Sin verificación posterior al error | No se comprueba si la operación se procesó | Alta |
| Posible duplicación | Reintento manual o del usuario | Alta |
| Sin manejo posterior al reintento | El flujo continúa con datos posiblemente inválidos | Alta |
| Rutas de reintento divergentes | Comportamientos opuestos ante la misma falla | Alta |
| BD sin unicidad | 44 pares duplicados, sin índice único | Alta |
| Sin timeout configurado | Espera el valor por defecto de WCF | Media |
| Bloqueo de hilo | `Thread.Sleep(1000)` por reintento | Media |
| Reintento sin retardo progresivo | Intervalo fijo de 1 s | Baja |

## 9. Criterios de éxito (congelados el 2026-09-26, antes de construir)

**Utilidad:** (1) compensaciones con efecto duplicado = 0; (2) operaciones con estado inconsistente = 0; (3) repetir una compensación deja el estado final idéntico al de una ejecución única, en N de N escenarios.
**Contribución:** (4) aplicabilidad medida por cobertura de la taxonomía de fallos y aplicación independiente por practicantes.

## 10. Conjunto de escenarios (fijado el 2026-09-26)

1. Caída de un servicio a mitad de la transacción.
2. Superación del tiempo de espera.
3. Pérdida de respuesta.
4. Solicitud duplicada.
5. Reintento posterior a un éxito real.
6. Fallo durante la repetición de una anulación.

## 11. Entrevista al personal de soporte (2026-10-05)

Confirma H1 con lenguaje operativo ("no siempre se sabe si la operación se procesó"; reintentar sin verificar "existe el riesgo de generar una operación duplicada"). La información que el personal considera valiosa registrar coincide con los componentes K2 y K9 del método.

## 12. Reversión a nivel de banco (condición de contorno)

Existen casos en que el banco debita al cliente, la pasarela no se entera y la venta no se efectiviza; la conciliación con el banco puede revertir el débito, **sin intervención de TI**. El método cubre la operación de la pasarela, no la conciliación financiera con el banco. Evidencia del costo real del problema.

## 13. Implicaciones para el diseño y la evaluación (insumo de E7)

1. **Cubrir las dos rutas** (RpcA y RpcB) o declarar y resolver la divergencia.
2. **Reforzar la unicidad** en la base (índice único sobre la llave lógica) como condición de la idempotencia.
3. **Apoyarse en el servicio de convergencia existente** (`WinServiceSoatConsultaPagosQR`) en lugar de introducir uno nuevo.
4. **Para la evaluación del método:** el doble de evaluación **no debe ser idempotente por defecto**, o debe permitir medir el efecto duplicado; si el doble ya deduplica, el método no puede demostrar su aporte.
5. **La anulación de un QR no pagado no equivale a una compensación Saga:** la idempotencia de la compensación debe verificarse sobre una compensación real en la evaluación, no sobre la anulación.

## 14. Pendientes

- Aclarar la relación entre errores no controlados (4.302) y timeouts (226) — si los timeouts son un subconjunto.
