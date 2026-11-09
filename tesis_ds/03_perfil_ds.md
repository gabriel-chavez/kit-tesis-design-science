# E3. Perfil de investigación en Design Science

> **Estado:** cerrada el 2026-09-26. La puerta de E3 se cumplió con la oración única de contribución y el cronograma confirmado.

## 1. Título provisional (elegido por el tesista)

> Método para el diseño y la verificación de compensaciones idempotentes en transacciones distribuidas basadas en Sagas: un estudio de Design Science.

Señal de alerta evitada: no aparece como "desarrollo de un sistema para X".

## 2. Planteamiento del problema

**Problema de diseño.** En la pasarela de pagos por QR de una aseguradora, la operación puede quedar en estado inconsistente cuando, tras un fallo parcial de comunicación, una respuesta perdida o una superación del tiempo de espera, la operación ya fue procesada por el servicio externo y se produce un reintento que exige compensarla. Cuando la compensación no es idempotente, su ejecución repetida genera efectos adicionales o estados divergentes. El problema es de clase: afecta a todo sistema empresarial que integra participantes externos que no forman parte de una transacción global.

**Problema de investigación (científico).** La literatura ofrece patrones descriptivos para gestionar transacciones distribuidas (Sagas) y propiedades como la idempotencia, pero no ofrece un procedimiento evaluado para **diseñar y verificar** compensaciones idempotentes en integraciones con participantes externos que no forman parte de una transacción global; este estudio produce y evalúa ese procedimiento y articula los principios de diseño que lo sustentan.

## 3. Pregunta de investigación (confirmada por el tesista)

> ¿Cómo diseñar y verificar compensaciones idempotentes en transacciones distribuidas basadas en Sagas con servicios externos no transaccionales, de manera que los reintentos no produzcan efectos duplicados ni estados inconsistentes, y qué principios de diseño permiten aplicar el método a problemas de la misma clase?

Es prescriptiva-analítica y de nivel de clase, con un componente de utilidad y un componente de conocimiento. Prueba de fuego superada: no puede responderse sin construir y evaluar.

**Sub-preguntas:**

- *Diagnóstico:* ¿qué modos de fallo y estados inconsistentes se producen hoy en el flujo de pago por QR ante fallos parciales y reintentos?
- *Construcción:* ¿qué componentes, reglas y mecanismos de verificación debe contener un método de compensaciones idempotentes para esa clase de flujo?
- *Evaluación:* ¿en qué medida el método elimina las compensaciones con efecto duplicado y los estados inconsistentes en el entorno controlado, y en qué medida resulta aplicable por practicantes independientes?

## 4. Objeto de estudio

El artefacto propuesto: un **método** para el diseño y la verificación de compensaciones idempotentes, con sus principios de diseño y sus criterios de verificación. El proceso de pagos de la aseguradora es el campo de acción.

## 5. Objetivos (confirmados por el tesista)

**General:** diseñar y verificar un método para compensaciones idempotentes en transacciones distribuidas basadas en Sagas con servicios externos no transaccionales, y articular los principios de diseño que lo sustentan.

**Específicos:**

1. Caracterizar los modos de fallo y los estados inconsistentes del flujo de pago por QR ante fallos parciales y reintentos (diagnóstico).
2. Especificar el método y sus criterios de verificación (construcción).
3. Formalizar el método e instanciarlo en el entorno controlado (construcción).
4. Evaluar formativamente el método, documentar el rediseño y evaluarlo de forma sumativa (evaluación).
5. Articular los principios de diseño y sus condiciones de contorno (contribución al conocimiento).

## 6. Tipología del artefacto

Método (principal) + principios de diseño + criterios de verificación. Instanciación de software como vehículo. Al no ser una instanciación la contribución principal, "construir" significa **formalizar y aplicar**, y se exige al menos una **exemplar**: una aplicación real del método en el entorno controlado que produzca evidencia.

## 7. Fundamentación en la base de conocimiento

Teorías y resultados que guían el diseño (por verificar en E4): el patrón Saga y las transacciones compensables; el procesamiento de transacciones y la consistencia; la idempotencia; la consistencia eventual; los modelos de fallo en sistemas distribuidos; la verificación por inyección de fallos y pruebas basadas en propiedades. Referencias candidatas: García-Molina y Salem (1987); Gray y Reuter (1993); Vogels (2009); Cristian (1991); Richardson (2018); Basiri et al. (2016); Claessen y Hughes (2000). `[E4: verificar cada fuente]`

Prueba de eliminación: el marco debe cambiar decisiones de diseño; si al eliminarlo el método no cambia, es decorativo.

## 8. Diseño metodológico

- Modelo de proceso: DSRM de Peffers et al. (2007), con las actividades de identificación y motivación, definición de objetivos, diseño y desarrollo, demostración, evaluación y comunicación.
- Iteración: ciclos de diseño de Wieringa (2014).
- Evaluación: marco FEDS de Venable, Pries-Heje y Baskerville (2016).
- Secuencia prevista: un ciclo **formativo** en laboratorio con rediseño documentado y un ciclo **sumativo**.

`[E4/E8: verificar referencias]`

## 9. Estrategia de evaluación

- **Formativa (artificial, laboratorio):** primera versión del método aplicada sobre el entorno controlado con los escenarios de fallo; se documenta el rediseño.
- **Sumativa:** aplicación del método a una operación compensable nueva por practicantes distintos del autor, más la verificación técnica de las propiedades de idempotencia y consistencia en el entorno controlado.
- **Criterios:** los de E2 (utilidad: cero duplicados, cero inconsistencias, estado final invariante; contribución: aplicabilidad y cobertura de la taxonomía de fallos).
- **Amenazas a la validez:** constructo, interna, externa y de conclusión, con mitigación e impacto residual.

## 10. Tipo de contribución reclamada y oración de contribución

**Tipo:** principios de diseño validados, de nivel de clase (maestría). No una teoría de diseño generalizable (doctorado) ni solo la instanciación (grado).

**Oración única de contribución (tesista, 2026-09-26):**

> El estudio aporta principios de diseño y un procedimiento para diseñar y verificar compensaciones idempotentes en transacciones distribuidas basadas en Sagas con servicios externos no transaccionales, validados mediante escenarios de fallo y su aplicación en un flujo de pagos QR.

Nota de calibración del asesor: para una tesis de método, la contribución son los **principios de diseño** y el procedimiento es el vehículo que los articula. La oración cumple la puerta; conviene que, en redacciones posteriores, los principios queden en primer plano y el procedimiento como su sustento.

## 11. Selección y muestra

- **Contexto de evaluación:** pasarela de pagos por QR de la aseguradora, reproducida en un entorno controlado con datos sintéticos.
- **Criterio de la muestra de evaluación (tesista):** practicantes con experiencia en desarrollo, arquitectura o integración de sistemas que **no hayan participado en el diseño del método**.
- **Muestra de diseño frente a muestra de evaluación:** se mantiene la separación; los dos integrantes del equipo de soporte no pueden ser los únicos evaluadores.
- `[E8]`: fijar el número de practicantes, su procedencia (otros equipos, banco, contactos externos) y su representatividad de la clase.

## 12. Cronograma (corregido por el asesor, confirmado por el tesista el 2026-09-26)

El cronograma propuesto por el tesista omitía las etapas **E4 (marco teórico)** y **E5 (estado del arte)**, que son condición para diseñar y para confirmar la novedad de la brecha. Se incorporan.

| Semana | Fase | Productos |
|:--|:--|:--|
| 1–3 | Identificación y fundamentación | E4 marco teórico, E5 estado del arte, E6 caracterización |
| 4–5 | Alternativas y diseño | E7a–E7d |
| 6 | Construcción y verificación | E7e–E7f |
| 7 | Evaluación formativa y rediseño | E8 (ciclo formativo) |
| 8–9 | Evaluación sumativa | E8 (ciclo sumativo) |
| 10 | Enlace y contribución | E9–E10 |
| 11–12 | Redacción y traducción institucional | documento final |

Advertencias de viabilidad: (a) tres semanas para E4–E5–E6 es agresivo si se exige verificación de cada fuente; (b) una semana para construir el método e instanciarlo en el entorno controlado es el mayor riesgo del plan; (c) la evaluación formativa en la semana 7 deja margen para el rediseño, lo cual es correcto y debe protegerse. Las fechas calendario deben anclarse en cuanto el programa fije la entrega y la defensa.

## 13. Plan de análisis

- De los resultados de cada ciclo a los principios de diseño, con la estructura de Wieringa (2014): **objetivo → contexto → mecanismo → principio**.
- Suficiencia: los principios se consideran validados cuando la evidencia de los ciclos sostenida por la verificación soporta cada componente y sus condiciones de contorno.

## 14. Anexo de artefacto previsto

Descripción preliminar: un método con las etapas de identificación de operaciones compensables, diseño de llaves de idempotencia, registro de estado, reglas de orden y deduplicación, y reconciliación; más los criterios de verificación. La descripción detallada se produce en E7.

## Puerta de E3

- [x] Oración única de contribución, con las palabras del tesista.
- [x] Título provisional elegido.
- [x] Pregunta de investigación y objetivos confirmados.
- [x] Criterio de selección de la muestra de evaluación definido.
- [x] Cronograma confirmado, con E4 y E5 incorporados. El anclaje en fechas calendario queda pendiente para cuando el programa fije la entrega y la defensa.
