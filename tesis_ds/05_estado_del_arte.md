# E5. Estado del arte (soluciones y artefactos existentes)

> **Estado:** en curso. El foco no es el fenómeno (transacciones distribuidas), sino las **soluciones y artefactos que ya existen** para la clase de problema: qué se construyó, cómo se evaluó, en qué contextos, con qué limitaciones. Incluye herramientas comerciales y de código abierto, no solo publicaciones.

## 1. Protocolo: mapeo sistemático

**Pregunta:** ¿qué soluciones y artefactos existentes abordan el diseño y la verificación de compensaciones idempotentes o su equivalente (idempotencia y deduplicación en transacciones distribuidas con dependencias externas)?

**Cadenas de búsqueda (inglés y español):**

- "saga pattern" AND ("idempotent compensation" OR "idempotency")
- "compensating transaction" AND "idempotent"
- "saga" AND "microservices" AND ("transaction consistency" OR "distributed transaction")
- "idempotent consumer" OR "exactly-once" AND "message"
- "saga orchestration" AND ("evaluation" OR "comparison")

**Bases y fuentes:** OpenAlex, DOAJ, Google Scholar, revistas y conferencias de acceso abierto (IEEE Access, Applied Sciences, ACM, Springer), y **documentación oficial** de los marcos candidatos.

**Criterios de inclusión:** artefactos académicos y herramientas; preferencia 2019–2026; recuperables en acceso abierto; con algún tipo de evaluación.

**Criterios de exclusión:** trabajos sin evaluación; fuentes no verificables; material de mercado sin especificación técnica.

## 2. Soluciones académicas relevantes

| Referencia | Qué aporta | Limitación frente al problema |
|:--|:--|:--|
| Daraghmi et al. (2022) | Mejora el patrón Saga para transacciones distribuidas en microservicios | No aborda la idempotencia de la compensación ni su verificación |
| Reza y Rahman (2022) | Extiende Saga y patrones de microservicios para transacción resiliente | Énfasis en resiliencia, no en compensación idempotente |
| Štefanko et al. (2019) | Aplica el patrón Saga en un entorno reactivo de microservicios | No define un procedimiento evaluado de diseño/verificación |
| Aydın y Çebi (2022) | Compara orquestación frente a coreografía en el patrón Saga | Comparación de coordinación, no de idempotencia de compensaciones |
| Laigner et al. (2021) | Estado de la práctica de la gestión de datos en microservicios | No propone un método de compensaciones idempotentes |

## 3. Herramientas y marcos candidatos

Candidatos a caracterizar: **MassTransit, NServiceBus, Temporal, Axon y Camunda**.

### 3.1 MassTransit (caracterizado el 2026-09-26, fuente: documentación oficial)

- Soporta **máquinas de estado de saga** (orquestación) y **listas de ruta** (coreografía).
- Persiste el estado de la saga (varios repositorios) y correlaciona eventos.
- Ofrece **reintentos** y **buzón de salida** (outbox) para evitar mensajes duplicados ante fallos de concurrencia.
- Reconoce el problema de la **entrega duplicada** y de los eventos fuera de orden.
- La **compensación** no es automática: debe diseñarla el desarrollador; la documentación no ofrece un procedimiento para garantizar ni verificar la idempotencia de la compensación.
- Nota: la versión 9 pasó a ser un producto comercial.

### 3.2 NServiceBus (caracterizado el 2026-09-26, fuente: documentación oficial)

- Sagas como **máquina de estado dirigida por mensajes**, con correlación, persistencia, tiempos de espera y recuperabilidad (reintentos).
- Advierte explícitamente que una saga **no debe realizar operaciones de entrada/salida** (llamadas a servicios externos): debe delegarlas a manejadores.
- Reconoce problemas de **pérdida de mensajes** y de consistencia al completar una saga y recomienda el **outbox** o el borrado lógico.
- La **compensación** es responsabilidad del desarrollador; no existe un procedimiento integrado para su idempotencia ni para verificarla.

### 3.3 Temporal, Axon y Camunda

`[pendiente de caracterización]`

## 4. Estado de la práctica en el contexto del tesista

- La pasarela de pagos es una **implementación propia**.
- Ante un fallo de comunicación, **espera un intervalo y reintenta**; si el problema persiste, reintenta de nuevo y, al agotar los intentos, **produce un error no controlado**.
- **No** hay un diseño explícito de idempotencia ni un procedimiento formal de reintentos y compensaciones.

Este es el comportamiento de línea base que la caracterización (E6) documentará con datos.

## 5. Hallazgo preliminar (por confirmar al cerrar E5)

Los marcos existentes aportan **coordinación, estado persistente, reintentos y buzón de salida**, pero dejan la **compensación y su idempotencia en manos del desarrollador**, sin un procedimiento de diseño ni de verificación evaluado. Las soluciones académicas mejoran el patrón Saga pero no abordan la idempotencia de la compensación como objeto de diseño y verificación. Eso delimita la novedad del artefacto: un **método para diseñar y verificar compensaciones idempotentes**, con criterios de verificación.

## 6. Criterios de superioridad (para la evaluación)

| Dimensión | Descripción |
|:--|:--|
| Cobertura de modos de fallo | Cuántos modos de la taxonomía de fallos anticipa el método |
| Garantía de idempotencia | Si asegura que la repetición no produce efectos nuevos |
| Verificabilidad | Si incluye criterios y mecanismos para comprobar la idempotencia |
| Costo de implementación | Esfuerzo de adopción por un equipo real |

## 7. Pendientes de E5

- Caracterizar Temporal, Axon y Camunda con el mismo esquema.
- Completar el mapeo académico con las cadenas de búsqueda (registro de selección).
- Redactar la respuesta documentada a: por qué las soluciones existentes son insuficientes y qué aporta el artefacto que ellas no aportan (puerta de E5).
