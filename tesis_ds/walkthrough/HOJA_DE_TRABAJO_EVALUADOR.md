# Cuaderno de Trabajo del Evaluador
**Evaluación de Aplicabilidad de Método de Integración y Compensación**

Bienvenido/a. Estás participando en una evaluación técnica independiente (walkthrough de aplicabilidad). 

Tu objetivo es leer la especificación de un método de compensación e idempotencia para integraciones y resolver las tareas planteadas sobre un caso de prueba, registrando tus decisiones técnicas, dudas y observaciones.

---

## 1. Datos del Evaluador y Entorno

Por favor completa esta sección antes de empezar:

| Campo | Tu Respuesta |
|---|---|
| **Nombre del Evaluador:** | |
| **Rol / Años de experiencia en desarrollo:** | |
| **Fecha de inicio:** | |
| **Fecha de finalización / Tiempo dedicado:** | |
| **Caso asignado:** | Venta SOAT en `BD_PASARELA_PAGO` (`TRAMITE_SECUENCIAL = 3147`, Gestión 2026) |
| **Escenario de falla:** | Corte de red hacia BUSA tras confirmación externa del QR |

---

## 2. Instrucciones Generales

1. **Orden:** Resuelve primero la **Tarea 0** guiándote por tu criterio profesional de desarrollo, **antes de leer el documento `01_metodo.md`**.
2. **Lectura:** Luego de la Tarea 0, lee los documentos de apoyo:
   - `01_metodo.md` (El método propuesto)
   - `02_decisiones.md` (Decisiones de diseño)
   - `03_consulta_estado_qr.md` (Contrato del servicio externo)
   - `04_tablas_bitacora.md` (Estructura de tablas)
   - `05_restricciones.md` (Reglas del entorno)
3. **Completar:** Resuelve las Tareas 1 a 7 respondiendo con tus propias palabras y registrando cualquier punto confuso.
4. **Entrega:** Devuelve este único documento completado.

---

## 3. Tareas a Resolver

---

### TAREA 0. Análisis Conceptual del Problema (Resolver ANTES de leer 01_metodo.md)

*Contexto: Un servicio local invoca a un servicio externo (banco/pasarela) para procesar un pago o emitir un recurso. La llamada da timeout o se corta la conexión.*

- **0.1 ¿Qué riesgo o problema técnico ocurre si la llamada se corta o da timeout justo después de que el externo ya procesó la operación?**
  > *Escribe aquí tu análisis:*

- **0.2 En tu criterio como desarrollador, ¿qué debería hacer el sistema ante ese escenario de fallo en lugar de un reintento a ciegas?**
  > *Escribe aquí tu propuesta:*

- **0.3 En una arquitectura de microservicios o multicapa (capa de integración vs. servicio específico de pago), ¿en qué capa ubicarías la lógica para resolver esta inconsistencia y por qué?**
  > *Escribe aquí tu justificación:*

---

### TAREA 1. Clave de Identidad e Idempotencia
*(Lee ahora `01_metodo.md` y `02_decisiones.md` antes de continuar)*

- **1.1 Analizando los datos de la venta SOAT, ¿cuál propones que sea la clave lógica única para identificar la transacción en toda la cadena de servicios y en la base de datos?**
  > *Escribe aquí los campos elegidos:*

- **1.2 Si el cliente reintenta enviar la misma venta 3 veces, ¿cómo aseguras que el sistema reconozca que es exactamente la misma operación y no genere una duplicada?**
  > *Escribe aquí tu respuesta:*

---

### TAREA 2. Verificación del Estado Real ante Fallo
*(Revisa `03_consulta_estado_qr.md`)*

- **2.1 Ocurrido el corte de red simulado, ¿cómo averiguarías con certeza el estado real de la operación en el proveedor externo?**
  > *Escribe aquí cómo procederías y qué operación invocarías:*

- **2.2 Si en tu tabla local la fila sigue en estado "Solicitado", pero al consultar al externo te devuelve código de estado `H` (Habilitado): ¿cuál es el estado real que debes adoptar y qué haces con la llamada de generación de QR?**
  > *Escribe aquí tu decisión técnica:*

- **2.3 Si la consulta al externo no responde (timeout) o devuelve un estado no reconocido, ¿qué debe asumir el sistema y cuánto tiempo se debe esperar antes de forzar un cierre?**
  > *Escribe aquí tu respuesta:*

---

### TAREA 3. Toma de Decisiones Técnicas

- **3.1 Según el estado obtenido del externo, describe qué acción corresponde en cada caso:**
  - *Si el externo confirma que ya está Pagado (`P`):* 
  - *Si el externo confirma que está Habilitado (`H`) pero el cliente abortó la compra:* 
  - *Si el externo confirma que nunca existió la solicitud:* 

- **3.2 ¿Cuál es el principio técnico que justifica tus respuestas anteriores?**
  > *Escribe aquí tu justificación:*

---

### TAREA 4. Compensación e Idempotencia
*(Revisa `04_tablas_bitacora.md`)*

- **4.1 Para anular o compensar una operación sin riesgo de duplicar la anulación, ¿cómo identificarías unívocamente dicha acción compensatoria?**
  > *Escribe aquí qué campos o identificador usarías:*

- **4.2 En el flujo de compensación: ¿qué debe hacerse primero?**
  - Opción A: Invocar la anulación en el servicio externo y, si responde exitoso, registrar en la base de datos.
  - Opción B: Registrar la intención de compensación en la base de datos y luego invocar al servicio externo.
  > *Elige una opción y justifica por qué:*

- **4.3 Si el proceso se interrumpe y la compensación se dispara dos veces seguidas, ¿cómo evita el sistema ejecutar un doble efecto en el externo?**
  > *Escribe aquí tu explicación:*

---

### TAREA 5. Reconciliación de Operaciones Huérfanas

- **5.1 Si una operación quedó en estado indeterminado tras 20 minutos de reintentos, ¿cómo debe proceder el servicio en segundo plano (reconciliador) para cerrar el caso?**
  > *Escribe aquí tu propuesta:*

- **5.2 Si el proceso de reconciliación se ejecuta dos veces seguidas sobre la misma operación huérfana, ¿qué resultado se espera?**
  > *Escribe aquí tu análisis:*

---

### TAREA 6. Registro y Telemetría

- **6.1 Al auditar un fallo en la bitácora técnica, ¿qué datos mínimos consideras que deben quedar registrados para poder rastrear exactamente qué ocurrió?**
  > *Lista aquí los campos/datos que guardarías:*

---

### TAREA 7. Cierre de la Operación

- **7.1 ¿Cómo debe quedar declarada finalmente la transacción? ¿Puede quedar alguna operación con estado "en proceso" indefinidamente?**
  > *Escribe aquí tu respuesta:*

- **7.2 Desde el punto de vista del negocio y de auditoría, ¿cuál es la diferencia entre declarar que una operación "No se ejecutó" frente a declarar que "No se pudo verificar su estado"?**
  > *Escribe aquí tu diferenciación:*

---

## 4. Evaluación de la Documentación (Feedback del Desarrollador)

Por favor, sé completamente honesto. Este punto es el más valioso para la investigación:

1. **¿Qué partes de los documentos te resultaron ambiguas, confusas o difíciles de entender sin ayuda?**
   > *Escribe aquí tus observaciones:*

2. **¿Encontraste alguna discrepancia técnica entre la teoría del método y lo que viste en la base de datos o en los servicios?**
   > *Escribe aquí tus observaciones:*

3. **¿Qué cambios o agregados concretos le harías a la documentación del método para que cualquier otro desarrollador pueda aplicarlo sin dudar?**
   > *Escribe aquí tus propuestas:*

---
*Fin del cuaderno de trabajo. ¡Muchas gracias por tu evaluación!*
