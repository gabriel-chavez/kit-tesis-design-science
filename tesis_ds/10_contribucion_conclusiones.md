# E10. Contribución y conclusiones

> **Estado:** cerrada el 2026-10-28. La contribución se formula como **principios de diseño** de nivel de clase, con objetivo, contexto y mecanismo, y con condiciones de contorno explícitas. Solo se usan las afirmaciones respaldadas en `09_enlace_solucion.md` §8.

## 1. Regla de contención

Esta etapa no agrega evidencia: **ordena y acota la que existe**. Se aplican dos reglas:

1. Ningún principio se formula por encima de lo que la evidencia sostiene.
2. Toda afirmación retirada en E9 sigue retirada aquí, aunque suene plausible.

## 2. Respuesta a la pregunta de investigación

> ¿Cómo diseñar y verificar compensaciones idempotentes en transacciones distribuidas basadas en Sagas con servicios externos no transaccionales, de manera que los reintentos no produzcan efectos duplicados ni estados inconsistentes, y qué principios de diseño permiten aplicar el método a problemas de la misma clase?

**Respuesta.** Se diseña y verifica un **método** de nueve componentes que trata la compensación como una operación con identidad propia, decide con el estado del participante externo y no con la memoria del proceso, y se comprueba con un doble neutro y un control negativo. El método elimina los efectos duplicados **en el entorno controlado** y en los seis escenarios fijados antes de construir. Los principios que lo sustentan son cinco y se enuncian en §4.

La respuesta es **prescriptiva y de alcance acotado**: prescribe cómo diseñar y verificar, y declara en qué condiciones deja de valer (§7).

## 3. Sub-preguntas respondidas

| Sub-pregunta | Respuesta | Evidencia |
|:--|:--|:--|
| **Diagnóstico:** ¿qué modos de fallo y estados inconsistentes se producen hoy? | Seis escenarios; 4.302 errores no controlados y 226 timeouts en 2025; 69 casos EFECTIVIZAR; 9,67 h de resolución media; dos rutas de reintento divergentes; 44 pares duplicados sin índice único | E6 §4–§7 |
| **Construcción:** ¿qué componentes, reglas y mecanismos debe contener el método? | Nueve componentes (K1–K9), catorce decisiones (Dc1–Dc14), catorce pasos y criterios de verificación | E7b, E7c, `metodo.md` |
| **Evaluación:** ¿en qué medida elimina duplicados e inconsistencias y en qué medida es aplicable? | Elimina duplicados en el entorno controlado: 12/12 frente a 0/12, 0 recursos y 0 compensaciones duplicadas. La aplicabilidad **no se alcanzó**: queda como evidencia exploratoria | E8 §9; `e8_evidencia/sumativo/` |

## 4. Contribución: principios de diseño

Cada principio se enuncia con la estructura **objetivo → contexto → mecanismo → principio**, según Wieringa (2014), y se acompaña de la evidencia que lo sostiene y de su condición de contorno.

### Pr1 — Identidad previa al efecto

- **Objetivo:** que la repetición de una solicitud no produzca un segundo efecto.
- **Contexto:** integraciones donde el participante externo no forma parte de una transacción global y el cliente puede reintentar por timeout, caída o duplicación de la solicitud.
- **Mecanismo:** derivar la identidad de la operación de datos de negocio que ya existen y no cambian entre intentos; usarla en todos los reintentos.
- **Principio:** *antes de invocar un efecto en un participante externo, deriva la identidad de la operación de datos estables del negocio; nunca de un valor generado en memoria ni de la sesión.*
- **Evidencia:** F1; V8 (identidad reproducible); sumativo E4 (3 solicitudes → 1 recurso).
- **Condición de contorno:** requiere un identificador único y estable. Si no existe, hay que persistir la llave antes de la primera llamada; sin eso el principio no aplica.

### Pr2 — El estado del externo es la fuente de verdad

- **Objetivo:** decidir correctamente cuando la respuesta es ambigua.
- **Contexto:** fallos de transporte en los que el efecto pudo ocurrir o no.
- **Mecanismo:** consultar el estado del externo —y la existencia del recurso— antes de reintentar, compensar o reconciliar. La bitácora local ordena, pero no decide.
- **Principio:** *la ausencia de respuesta no informa sobre el efecto; ante ambigüedad, consulta el estado del participante externo antes de actuar.*
- **Evidencia:** F3, F4; V1; sumativo E2/E3 (`ConfirmarProcesada`); formativo S8/S9.
- **Condición de contorno:** el externo debe exponer consulta de estado. Si no la expone, el método no aplica: sin esa capacidad no hay forma de distinguir procesado de no procesado.

### Pr3 — La compensación tiene identidad propia y se reserva antes de aplicarse

- **Objetivo:** que repetir una compensación no produzca un segundo efecto.
- **Contexto:** compensaciones disparadas por reintentos, por fallos durante la propia compensación o por reconciliación.
- **Mecanismo:** identificar la compensación por `(operación, motivo)` y registrar la reserva **antes** de ejecutarla; la repetición encuentra la reserva y no vuelve a aplicar el efecto.
- **Principio:** *trata la compensación como una operación con identidad determinista y registra su intención antes del efecto; la deduplicación posterior por marca de tiempo no es suficiente.*
- **Evidencia:** F5; V2, V3; control negativo (sin K5 = 3, con K5 = 1); formativo S1–S11; sumativo E6 (4 compensaciones en la línea base → 1).
- **Condición de contorno:** la garantía se demuestra **en una instancia**. Entre instancias depende de unicidad en la base de datos; mientras esa condición no se aplique, no se puede afirmar idempotencia distribuida.

### Pr4 — Lo indeterminado no se asume

- **Objetivo:** no convertir una duda en un duplicado ni en una operación sin entregar.
- **Contexto:** estados transitorios del externo y procesos que mueren entre la reserva y la ejecución.
- **Mecanismo:** reconsultar dentro de una ventana declarada; si el estado persiste, reconciliar en lote. Nunca asumir procesado ni no procesado.
- **Principio:** *cuando el estado no se puede confirmar, no se asume: se reconsulta dentro de una ventana explícita y se reconcilia fuera del camino crítico.*
- **Evidencia:** F7; V4, V5; formativo S7 (0 reservas pendientes), S8, S9.
- **Condición de contorno:** requiere un reloj y una ventana acordada con el negocio. La ventana se declara en minutos completos; no se afirma precisión de segundos.

### Pr5 — Se verifica con el efecto observable, no con la lógica del cliente

- **Objetivo:** que la afirmación de idempotencia sea atribuible al método y no al medio de prueba.
- **Contexto:** evaluación de cualquier método de compensación.
- **Mecanismo:** doble **neutro** que no deduplica, control negativo que desactiva el componente que garantiza, y medición sobre contadores del externo.
- **Principio:** *la idempotencia se verifica contra el efecto en el participante externo; si el medio de prueba deduplica o compensa por su cuenta, el resultado no es atribuible al método.*
- **Evidencia:** control negativo 3/1; V2, V3; formativo; sumativo.
- **Condición de contorno:** exige poder desactivar el componente y observar el externo. Un entorno que no permita el control negativo no permite esta verificación.

## 5. Qué se entiende por "validado" en esta tesis

Los principios están **validados a nivel de clase y en entorno controlado**: cada uno se instanció, se ejecutó y produjo el efecto previsto en los escenarios fijados antes de construir. No están validados como ley general, ni en producción, ni con una muestra de practicantes suficiente.

| Tipo de validez | Alcance logrado |
|:--|:--|
| Construcción | El artefacto implementa los principios y la verificación los distingue de la lógica del cliente |
| Utilidad | Medida contra la línea base en los seis escenarios, en el doble |
| Aplicabilidad | Exploratoria: dos participantes, piloto con consentimiento y declaración firmados |
| Generalización | Analítica, no estadística |

## 6. Cumplimiento de objetivos

| Objetivo específico | Estado |
|:--|:--|
| 1. Caracterizar modos de fallo y estados inconsistentes | **Cumplido** (E6: I1–I7, seis escenarios) |
| 2. Especificar el método y sus criterios de verificación | **Cumplido** (E7b, E7c, `metodo.md`) |
| 3. Formalizar e instanciar en entorno controlado | **Cumplido con salvedad**: instanciado sobre el doble; la cadena real no se ejecutó |
| 4. Evaluar formativamente y de forma sumativa | **Parcial**: formativo y sumativo técnico cumplidos; el sumativo humano no alcanzó su criterio |
| 5. Articular principios y condiciones de contorno | **Cumplido** (§4 y §7) |

## 7. Condiciones de contorno

Los principios **aplican** cuando:

- el participante externo no participa de una transacción global y expone consulta de estado;
- existe un identificador de negocio único y estable por operación;
- la acción compensatoria está disponible y es reversible en el externo;
- el sistema puede mantener una bitácora de estado y una ventana de reconsulta.

Los principios **no aplican o no están demostrados** cuando:

- el externo no ofrece consulta de estado (Pr2, Pr3 y Pr4 quedan sin base);
- la operación ya produjo un pago irreversible y no existe acción compensatoria: el caso queda fuera del método (anulación de QR no pagado);
- el despliegue es multi-instancia sin unicidad en la base: la idempotencia distribuida **no está garantizada** (Pr3);
- la conciliación es financiera con el banco: está fuera del alcance del estudio.

## 8. Amenazas a la validez

| Tipo | Amenaza | Mitigación aplicada | Impacto residual |
|:--|:--|:--|:--|
| Constructo | El doble no es BUSA | Doble neutro, control negativo y enlace del cliente real (2/2) | No se afirma el comportamiento exacto del proveedor |
| Interna | Aprendizaje y sesgo del observador | Casos distintos, observadora no autora, guion preestructurado | Sin aleatorización; una sola observadora; P1 ya conocía el método |
| Interna | Datos humanos con medición de ayuda no evaluable | Se clasifican como piloto y no se usan para afirmar aplicabilidad | Salvedad §4; sin notas crudas independientes; independencia parcial (misma organización); coincidencia con el documento previo no trazada |
| Externa | Una organización, una cadena, dos casos | Se declaran; condiciones de contorno explícitas | No se generaliza a otras organizaciones ni a otros externos |
| Conclusión | n = 2 en lo humano; una instancia en lo técnico | Conteos exactos; generalización analítica | No se estiman tasas poblacionales |
| Conclusión | Idempotencia distribuida no probada | Se declara como condición de despliegue | La afirmación queda limitada a una instancia |

## 9. Lo que esta tesis no afirma

| Afirmación | Estado |
|:--|:--|
| Que garantice idempotencia distribuida | **No**. Depende de unicidad en la base, no aplicada |
| Que funcione dentro de la cadena real de pagos | **No**. La cadena completa no se ejecutó |
| Que reduzca el tiempo de resolución (9,67 h) | **No medido** después del método |
| Que reduzca el costo operativo | **No medido** |
| Que sea aplicable por terceros sin ayuda | **No demostrado**; evidencia exploratoria |
| Que cubra la reversión de pagos ejecutados | **No**. Fuera del alcance del proveedor |
| Que sus resultados sean generalizables estadísticamente | **No**. Generalización analítica |

## 10. Aportes

**Práctico.** Un procedimiento aplicable sin migrar a un marco de mensajería, con nueve componentes, catorce decisiones y criterios de verificación. En el entorno controlado evita 10 recursos y 6 compensaciones duplicadas frente a la conducta actual.

**Metodológico.** Un diseño de verificación reproducible: doble neutro, control negativo y medición sobre el efecto externo. Es transferible a otros métodos de compensación, no solo al evaluado.

**De conocimiento.** Cinco principios de diseño que hacen explícito el mecanismo que los marcos existentes dejan implícito: identidad previa al efecto, estado externo como fuente de verdad, identidad y reserva de la compensación, no asumir lo indeterminado y verificar contra el efecto observable. No constituyen una teoría: son conocimiento prescriptivo de nivel de clase.

## 11. Trabajo futuro

1. **Aplicar Dc10**: resolver los 44 pares duplicados y crear el índice único; probar dos procesos concurrentes para afirmar idempotencia distribuida.
2. **Unificar el ordenamiento**: una sola ruta entre `K6.EjecutarPlan` y el bucle del coordinador; definir el responsable único de cerrar una reserva sin confirmar.
3. **Ejecutar la cadena real** (`CNPagosQR → CPagoSimple → BUSA`) y caracterizar los estados reales del proveedor.
4. **Revisión por expertos** en .NET y en Sagas, pendiente desde E7e.
5. **Re-ejecutar el sumativo humano** con registro de ayuda, hojas firmadas y consentimientos.
6. **Medir costo y tiempo**: C5 sin evidencia; el efecto sobre las 9,67 h de resolución no se midió.
7. **Cubrir compensaciones encadenadas** (F6) y el segundo exemplar (`Ven06Revertir`).
8. **Extender a otros externos** de la misma clase (bancos, comercios, pasarelas).

## 12. Cierre

La tesis entrega un método y cinco principios para un problema de clase: cómo diseñar y verificar compensaciones idempotentes cuando el participante externo no participa de la transacción. La evidencia es sólida donde se midió —los seis escenarios, en un doble neutro, contra una línea base— y es explícitamente insuficiente donde no: la cadena real, el despliegue multi-instancia, el costo y la aplicación por terceros. La contribución es de nivel de maestría: principios de clase con mecanismo y condiciones de contorno, no una teoría general.

## 13. Puerta de E10

| Condición | Estado |
|:--|:--|
| Contribución formulada como principios con objetivo, contexto y mecanismo | **Cumple** (§4) |
| Condiciones de contorno explícitas | **Cumple** (§7) |
| Ninguna afirmación excede la evidencia | **Cumple** (§9; coherente con E9 §8) |
| Amenazas a la validez declaradas | **Cumple** (§8) |
| Trabajo futuro derivado de las brechas reales | **Cumple** (§11) |
