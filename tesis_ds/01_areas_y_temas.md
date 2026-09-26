# E1. Áreas de experiencia y temas candidatos

## 1. Perfil de experiencia del tesista

- **Área de experiencia mayor:** desarrollo y mantenimiento de software, con especialidad en desarrollo de servidor y backend con .NET/C#.
- **Áreas secundarias con práctica real:** integración de sistemas empresariales y transacciones distribuidas.
- **Rol habitual:** desarrollador backend, con participación en decisiones de arquitectura y diseño.
- **Años de experiencia:** aproximadamente ocho.

## 2. Contexto real disponible

- **Dominio:** seguros.
- **Sistemas:** pasarela de pagos interna y servicios de pago externos por QR. Tarjetas y Tigo Money son servicios tercerizados.
- **Tecnología de integración:** servicios REST y APIs.
- **Nivel de acceso:** código fuente, bases de datos, procesos operativos y contacto con las personas que operan los sistemas. Disponible durante el desarrollo de la investigación.
- **Entorno controlado:** varios servicios .NET, bases de datos y simulación de integraciones, con datos sintéticos. Permite reproducir caída de servicios, superación de tiempo de espera, pérdida de respuestas, mensajes o solicitudes duplicadas y reintentos, incluida la falla durante una compensación.
- **Rol del entorno controlado:** medio principal de evaluación del artefacto.

## 3. Problema observado (sin cuantificar aún)

Operaciones distribuidas que quedan en estado inconsistente ante una falla parcial. Una operación se ejecuta parcialmente y luego ocurre una superación de tiempo de espera, una pérdida de respuesta o un reintento, lo que obliga a compensar. El interés específico del tesista es **cómo diseñar y verificar que las compensaciones sean idempotentes**, de modo que su ejecución repetida no genere efectos nuevos ni inconsistencias. La cuantificación del costo se realizará en la caracterización inicial (E6).

## 4. Tipología de artefacto preferida

**Método**, acompañado de **principios de diseño** y **criterios de verificación**. El software y el entorno controlado cumplen el papel de instanciación y de vehículo de aplicación y evaluación; la contribución principal es el método y los principios generalizables.

## 5. Temas candidatos

> Nota de terminología: lo que en Design Science llamamos **problema de investigación** (el conocimiento transferible que genera construir) se registrará en el formato institucional como **problema científico**. Esta equivalencia se mantiene en todo el trabajo.

### T1. Método para el diseño y la verificación de compensaciones idempotentes en Sagas de pago

- **Problema de diseño tentativo**
  - *Contexto:* pasarela de pagos interna de una aseguradora que integra servicios de pago externos (QR, tarjeta, Tigo Money) por REST; un equipo reducido los mantiene; los terceros no participan de una transacción global y fallan de forma parcial.
  - *Problema:* ante superación de tiempo de espera, pérdida de respuesta o reintento, la operación queda a medias y la compensación puede ejecutarse más de una vez, con efectos duplicados (doble reversión, doble acreditación o estados divergentes).
  - *Solución conceptual:* un método que guía el diseño de la compensación idempotente (identificación de la operación, llave de idempotencia, registro de estado, orden de compensación, reconciliación), con principios de diseño y criterios de verificación.
  - *Criterios de éxito:* cero compensaciones con efecto duplicado en el entorno controlado bajo inyección de fallos; practicantes distintos del autor aplican el método sin su ayuda.
- **Tipo de artefacto:** método (principal) + principios de diseño + criterios de verificación; instanciación de software como vehículo.
- **Clase de contextos:** integraciones empresariales con participantes externos no transaccionales, donde la falla parcial es la norma.
- **Contribución potencial (maestría):** principios de diseño validados sobre cuándo y cómo una compensación resulta idempotente y verificable, transferibles a otros flujos y dominios.
- **Base teórica probable (por verificar en E4–E5):** García-Molina y Salem (1987), *Sagas*; Gray y Reuter (1993), *Transaction Processing: Concepts and Techniques*; Vogels (2009), *Eventually Consistent*; Richardson (2018), *Microservices Patterns*.
- **Relevancia práctica:** alta. **Sustentación teórica:** alta.

### T2. Marco de verificación de idempotencia y consistencia para integraciones de pago

- **Problema de diseño tentativo**
  - *Contexto:* el mismo equipo no dispone de una forma sistemática de comprobar, antes de producción, que sus compensaciones son idempotentes.
  - *Problema:* las fallas de idempotencia se descubren en producción; las pruebas se concentran en el camino sin fallos.
  - *Solución conceptual:* un marco que combina inyección de fallos, pruebas basadas en propiedades y criterios de aceptación para exponer la falta de idempotencia, sobre escenarios reproducibles en el entorno controlado.
  - *Criterios de éxito:* el marco detecta un conjunto conocido de defectos sembrados de idempotencia; practicantes lo aplican y encuentran defectos por sí mismos.
- **Tipo de artefacto:** marco (composición: método de verificación + herramienta de apoyo).
- **Clase de contextos:** servicios con reintentos y operaciones no idempotentes, con dependencias externas.
- **Contribución potencial (maestría):** principios sobre cómo verificar idempotencia y consistencia en sistemas distribuidos con terceros.
- **Base teórica probable (por verificar):** Basiri et al. (2016), *Chaos Engineering*; Claessen y Hughes (2000), *QuickCheck*; Venable, Pries-Heje y Baskerville (2016), *FEDS*.
- **Relevancia práctica:** alta. **Sustentación teórica:** media-alta.

### T3. Modelo y taxonomía de fallos de compensación en flujos de pago distribuidos

- **Problema de diseño tentativo**
  - *Contexto:* no existe una clasificación compartida de los modos en que una compensación falla.
  - *Problema:* el equipo discute los incidentes como "se cayó" sin distinguir superación de tiempo de espera, respuesta perdida, compensación parcial, duplicado u orden invertido; sin taxonomía no hay indicadores ni priorización.
  - *Solución conceptual:* un constructo (taxonomía de modos de fallo de compensación) y un modelo que relaciona cada modo con la probabilidad de inconsistencia y con el efecto del reintento.
  - *Criterios de éxito:* la taxonomía cubre los incidentes observados; el modelo predice el estado final en escenarios reproducibles.
- **Tipo de artefacto:** constructo + modelo.
- **Clase de contextos:** sistemas distribuidos con fallos parciales y acciones compensatorias.
- **Contribución potencial (maestría):** taxonomía y modelo validados de forma representacional, con condiciones de uso.
- **Base teórica probable (por verificar):** Cristian (1991), *Understanding Fault-Tolerant Distributed Systems*; García-Molina y Salem (1987).
- **Relevancia práctica:** media-alta. **Sustentación teórica:** alta.

### T4. Método de reconciliación y deduplicación para integraciones de pago con terceros

- **Problema de diseño tentativo**
  - *Contexto:* pagos entre una pasarela interna y terceros (QR, tarjeta, Tigo Money) que no comparten una transacción global.
  - *Problema:* no hay un procedimiento definido para reconciliar estados divergentes ni para deduplicar operaciones cuando el tercero responde tarde o nunca.
  - *Solución conceptual:* un método con llaves de idempotencia, bitácora de operaciones, proceso de reconciliación por lotes e identificación de operaciones huérfanas.
  - *Criterios de éxito:* cero descuadres persistentes tras la reconciliación en el entorno controlado; trazabilidad de cada operación.
- **Tipo de artefacto:** método (composición: método + catálogo de patrones).
- **Clase de contextos:** integraciones financieras con participantes externos.
- **Contribución potencial (maestría):** principios de reconciliación y deduplicación aplicables a distintas integraciones.
- **Base teórica probable (por verificar):** García-Molina y Salem (1987); Vogels (2009); Hohpe y Woolf (2003), *Enterprise Integration Patterns*.
- **Relevancia práctica:** muy alta. **Sustentación teórica:** media-alta.

## 6. Comparación de los temas candidatos

| Tema | Tipo de artefacto | Relevancia práctica | Sustentación teórica | Cercanía con la experiencia del tesista |
|:--|:--|:--|:--|:--|
| T1. Compensaciones idempotentes en Sagas | Método + principios + criterios | Alta | Alta | Muy alta |
| T2. Verificación de idempotencia y consistencia | Marco | Alta | Media-alta | Alta |
| T3. Taxonomía y modelo de fallos de compensación | Constructo + modelo | Media-alta | Alta | Media-alta |
| T4. Reconciliación y deduplicación | Método + catálogo | Muy alta | Media-alta | Alta |

## 7. Recomendación del asesor (preliminar)

El tema **T1** es el más sólido para una maestría: parte del problema que el tesista observa, admite una tipología de método con principios transferibles y aprovecha un contexto real y un entorno controlado. Se recomienda **no fusionar los cuatro temas** (sería sobrealcance), sino tomar T1 como columna vertebral e incorporar de T2 los **criterios y el mecanismo de verificación**, y de T3 la **taxonomía de fallos** como insumo de los requisitos. T4 queda como posible trabajo futuro o como componente de reconciliación si el alcance lo permite.

Advertencia de calibración: el objetivo debe ser un conjunto de **principios de diseño de nivel de clase**, no una arquitectura propia de la aseguradora ni una teoría generalizable (eso es doctorado).

## 8. Decisión (puerta de E1, cerrada el 2026-09-26)

El tesista selecciona el tema **T1 como columna vertebral** de la investigación, con esta composición:

- **T1 (método para el diseño y la verificación de compensaciones idempotentes en Sagas de pago):** eje central, define el artefacto, la clase de problema y la contribución.
- **T2 (verificación de idempotencia y consistencia):** se incorporan sus criterios y su mecanismo de verificación.
- **T3 (taxonomía de fallos de compensación):** se incorpora la taxonomía como insumo para definir los escenarios de evaluación.

**Justificación de la elección:** parte de un problema observado en el contexto real del tesista, admite una tipología de método con principios transferibles y aprovecha el entorno controlado como medio de evaluación.

**Temas descartados:** T4 queda como posible trabajo futuro o como componente de reconciliación si el alcance lo permite.

