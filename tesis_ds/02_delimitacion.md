# E2. Delimitación

> **Estado:** etapa cerrada el 2026-09-26. Quedan trasladados a E6 la caracterización del comportamiento actual del sistema y la operacionalización de la aplicabilidad.

## 1. Problema de diseño (qué construir para mejorar la situación práctica)

### 1.1 Contexto

- Empresa del sector seguros; los pagos corresponden a la comercialización y gestión de productos de seguros.
- Arquitectura de pago: una pasarela de pagos interna más un servicio de pago mediante QR, integrados por servicios REST y APIs.
- Los pagos con tarjeta y con Tigo Money son servicios tercerizados y quedan **fuera del alcance** de esta investigación.
- Volumen de 2025: 198.843 transacciones anuales, con un promedio mensual de aproximadamente 16.570. Dos picos concentran el 79,59 % del volumen anual: diciembre (109.310) y enero (48.953). Los otros diez meses promedian aproximadamente 4.058 transacciones al mes. Los picos se explican por la naturaleza anual de la póliza: las renovaciones se realizan desde diciembre y en enero inicia la cobertura, con mayor concentración de compras del seguro obligatorio. Fuente: transacciones registradas en el sistema durante 2025. `[E6]`: serie completa y comportamiento del sistema ante fallos.
- Equipo de soporte de los sistemas: dos personas.
- Acceso disponible: código fuente, bases de datos, procesos operativos y contacto con las personas que operan los sistemas, durante el desarrollo de la investigación.

### 1.2 Problema

Existe una operación distribuida que puede quedar en estado inconsistente ante una **falla parcial** de comunicación, una **respuesta perdida** o una **superación del tiempo de espera**, cuando la operación ya pudo haber sido procesada por el servicio externo. En ese escenario se produce un reintento y la operación puede requerir una **compensación**. El problema específico es que la compensación debe ser **idempotente** y, cuando no lo es, su ejecución repetida produce efectos adicionales (por ejemplo, doble reversión o doble acreditación) o deja la operación en un estado divergente. `[E6]`: comportamiento actual del sistema en cada escenario y frecuencia observada.

### 1.3 Solución conceptual (confirmada)

Un **método** que guíe el diseño y la verificación de compensaciones idempotentes en flujos de pago basados en Sagas, compuesto por:

1. Identificación de las operaciones compensables y de su ciclo de vida.
2. Diseño de la identidad de la operación y de las llaves de idempotencia.
3. Registro de estado (bitácora) que permita reconocer una compensación ya ejecutada.
4. Reglas de orden y de deduplicación de las compensaciones.
5. Reconciliación de operaciones huérfanas.

El método se acompaña de **principios de diseño** y de **criterios de verificación**.

### 1.4 Criterios de éxito (fijados antes de construir; detalle en E6)

**Familia A — Utilidad (el artefacto resuelve el problema):**

1. Compensaciones con efecto duplicado: 0 en todos los escenarios de fallo definidos.
2. Operaciones con estado final inconsistente: 0 en los escenarios reproducibles.
3. Ejecución repetida de una compensación: el estado final es idéntico al de una única ejecución, en N de N escenarios.

**Familia B — Contribución (el método transfiere más allá del caso):**

4. Aplicabilidad: el método puede implementarse y usarse de forma efectiva en escenarios de operaciones compensables distintos de los que sirvieron de diseño, conservando sus propiedades esenciales y con un costo de implementación razonable.
   - Operacionalización `[E6/E8]`: cobertura de la taxonomía de fallos (el método anticipa los modos de fallo identificados) y aplicación independiente por practicantes distintos del autor, sin su ayuda, sobre una operación compensable nueva, cumpliendo los criterios de utilidad. El número de practicantes y el umbral de costo se fijan en E6.

`[E6]`: número y definición de los escenarios.

## 2. Problema de investigación (se registrará como "problema científico")

**Formulación del tesista (provisional):** la literatura ofrece patrones y mecanismos para gestionar transacciones distribuidas mediante Sagas y compensaciones, pero existe una necesidad de evidencia sobre cómo verificar sistemáticamente la idempotencia de dichas compensaciones ante fallos parciales y reintentos, particularmente cuando las operaciones involucran servicios externos que no participan de una transacción global.

**Reformulación aceptada por el tesista (2026-09-26):** la literatura ofrece patrones descriptivos para gestionar transacciones distribuidas (Sagas) y propiedades como la idempotencia, pero no ofrece un procedimiento evaluado para **diseñar y verificar** compensaciones idempotentes en integraciones con participantes externos que no forman parte de una transacción global; este estudio produce y evalúa ese procedimiento y articula los principios de diseño que lo sustentan.

`[E5 por contrastar]`: la brecha debe confirmarse contra la revisión de soluciones y artefactos existentes antes de darla por válida.

## 3. Objeto de estudio (resignificación de Design Science)

El objeto de estudio es **el artefacto propuesto**: el método para el diseño y la verificación de compensaciones idempotentes, con sus principios de diseño y sus criterios de verificación. El proceso de pagos y las transacciones distribuidas de la aseguradora son el **campo de acción**, no el objeto de estudio. (Confirmado por el tesista.)

## 4. Campo de acción

- **Temático:** transacciones distribuidas basadas en Sagas con servicios externos que no participan de una transacción global; compensaciones idempotentes.
- **Espacial:** empresa del sector seguros; pasarela de pagos interna y pago por QR. **Justificación:** el canal QR forma parte del flujo interno —el sistema de venta de seguros se integra con la pasarela de pagos de la empresa y esta con el sistema QR de un banco—, lo que permite analizar el proceso completo. La aseguradora es la organización donde se desarrolla el estudio, se brinda soporte de software y se identificaron los problemas que motivan la propuesta.
- **Temporal:** datos de enero a diciembre de 2025 y el período de construcción y evaluación.
- **Fuera de alcance:** pagos con tarjeta y con Tigo Money, gestionados por una empresa externa y fuera del alcance técnico del estudio; otras líneas de negocio no vinculadas a pagos. **Justificación:** no se dispone del proceso completo de esos canales.
- **Clase de contextos:** sistemas empresariales que integran terceros mediante APIs y servicios externos que no participan de una transacción global (aseguradoras, bancos, comercios, pasarelas de pago y similares). **Justificación:** comparten la estructura del problema (falla parcial del participante externo y necesidad de compensar), aunque difieran en dominio y tecnología.

## 5. Propuesta

- **Artefacto y tipología:** método (principal) con principios de diseño y criterios de verificación. (Confirmado.)
- **Instanciación:** software y entorno controlado con datos sintéticos, que permiten provocar caída de servicios, superación del tiempo de espera, pérdida de respuestas, duplicación de solicitudes y reintentos, incluida la falla durante una compensación. Actúan como vehículo de aplicación y evaluación, no como contribución.
- **Tipo de contribución reclamada:** principios de diseño validados de nivel de clase (maestría).

## 6. Puerta de E2 (cerrada el 2026-09-26)

- [x] Los cuatro componentes del problema de diseño están identificados (contexto, problema, solución conceptual, criterios de éxito).
- [x] Solución conceptual confirmada por el tesista.
- [x] Criterios de éxito confirmados, con la separación utilidad / contribución.
- [x] Objeto de estudio confirmado con su tipología.
- [x] Campo de acción delimitado y justificado (por qué el QR, por qué esta aseguradora, por qué la clase).
- [x] Brecha reformulada y aceptada (su contraste definitivo ocurre en E5).
- [ ] Comportamiento actual del sistema y frecuencia documentados (se traslada a E6).
- [ ] Operacionalización de la aplicabilidad (se traslada a E6/E8).
