# E6. Diagnóstico con indicadores y criterios

> **Estado:** en curso. El diagnóstico no describe la situación: demuestra que el problema existe y fija la **línea base** con la que se medirá la mejora. Regla de oro: los criterios de éxito se congelan **antes** de construir.

## 1. Evidencia del problema (multi-fuente)

| Fuente | Evidencia | Estado |
|:--|:--|:--|
| Literatura | La idempotencia y la deduplicación son requisitos estructurales con reintentos y mensajería al-menos-una-vez (Helland, 2012); la verificación de idempotencia tiene antecedente (Hummer et al., 2013) | Documentada |
| Práctica actual | La pasarela es una implementación propia; ante un fallo de comunicación espera un intervalo y reintenta hasta un límite; al agotar los intentos produce un **error no controlado**; no hay diseño explícito de idempotencia ni procedimiento formal de compensación | Documentada (testimonio del tesista) |
| Instrumentación | El sistema **no registra explícitamente** compensaciones ni duplicados de compensación | Documentada |
| Personal de soporte | Entrevistas a personal de soporte; posible contacto con personal técnico del servicio externo | Por obtener |
| Incidentes y tickets | Revisión de incidentes, tickets y registros para identificar operaciones fallidas o inconsistentes | Por obtener |

## 2. Advertencia metodológica (subregistro)

La ausencia de instrumentación **no prueba la ausencia del problema**: solo prueba que la organización no puede detectarlo. Se distinguirá siempre entre:

- **No ocurrió:** el evento no se produjo.
- **No se registró:** el evento pudo producirse, pero el sistema no lo captura.

Esta distinción se declarará como limitación y justifica parte de la estrategia de medición.

## 3. Dos líneas base

1. **Línea base retrospectiva** (datos históricos de 2025): solo para los indicadores que el sistema ya instrumenta.
2. **Línea base experimental** (prospectiva): para los indicadores no instrumentados, se **reproduce el comportamiento actual** (reintentos con espera fija, sin idempotencia) en el entorno controlado, sobre el mismo conjunto de escenarios, y se mide ahí. Esta línea base es la que permite comparar contra el método.

Sin la línea base experimental, los criterios de idempotencia no serían evaluables, porque el sistema actual no los registra.

## 4. Indicadores

| # | Indicador | Definición operacional | Unidad | Fuente | ¿Histórico? | Línea base | Valor esperado |
|:--|:--|:--|:--|:--|:--|:--|:--|
| I1 | Operaciones con estado inconsistente | Operaciones que terminan en un estado distinto al esperado tras agotar los intentos | por mes | Tablas de operaciones | Por verificar | `[por medir]` | 0 |
| I2 | Errores no controlados | Operaciones que terminan en el error no controlado al agotar los intentos | por mes | Registros de aplicación | Probable | `[por medir]` | 0 en escenarios cubiertos |
| I3 | Tasa de reintentos | Reintentos por operación | razón | Registros de aplicación | Probable | `[por medir]` | Reducir sin perder éxito |
| I4 | Intervenciones manuales | Correcciones manuales de operaciones inconsistentes | por mes | Tickets de soporte | Probable | `[por medir]` | 0 en escenarios cubiertos |
| I5 | Tiempo de resolución | Tiempo medio para resolver una operación inconsistente | horas | Tickets de soporte | Parcial | `[por medir]` | Reducir |
| I6 | Compensaciones con efecto duplicado | Compensaciones cuya repetición produce un efecto adicional | por escenario | **No instrumentado** | No | **Experimental** | 0 |
| I7 | Cobertura de modos de fallo | Modos de fallo anticipados por el método sobre el total de la taxonomía | proporción | Entorno controlado | No | **Experimental** | Total del conjunto fijado |

`[E6]` Completar los valores: el tesista debe verificar qué indicadores tienen datos disponibles y medir la línea base retrospectiva de esos, y preparar la reproducción de la conducta actual para la línea base experimental.

## 5. Criterios de éxito (congelados el 2026-09-26, antes de construir)

**Utilidad:**

1. Compensaciones con efecto duplicado: 0 en todos los escenarios del conjunto fijado.
2. Operaciones con estado final inconsistente: 0 en los escenarios reproducibles.
3. Ejecución repetida de una compensación: el estado final es idéntico al de una única ejecución, en N de N escenarios.

**Contribución:**

4. Aplicabilidad: el método puede implementarse y usarse en escenarios distintos de los del diseño, conservando sus propiedades y con costo de implementación razonable; se operacionaliza como cobertura de la taxonomía de fallos y aplicación independiente por practicantes.

La fecha de congelamiento se registra para proteger la evaluación contra la definición a posteriori (HARKing).

## 6. Conjunto de escenarios (fijado el 2026-09-26)

1. Caída de un servicio a mitad de la transacción.
2. Superación del tiempo de espera.
3. Pérdida de respuesta.
4. Solicitud duplicada.
5. Reintento posterior a un éxito real.
6. Fallo durante la compensación.

Aceptados por el tesista. El número de repeticiones por escenario se fija en E8.

## 7. Pendientes de E6 (puerta)

- Verificar disponibilidad de cada indicador en tablas y registros.
- Medir la línea base retrospectiva de los indicadores disponibles.
- Reproducir la conducta actual para la línea base experimental (I6, I7).
- Obtener entrevistas, incidentes o tickets como evidencia de contexto real (requisito de la puerta).
- Documentar el subregistro como limitación.
