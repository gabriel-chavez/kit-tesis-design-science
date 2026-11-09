# Cuaderno de Trabajo del Evaluador
**Evaluación de Aplicabilidad de Método de Integración y Compensación**

Walkthrough de aplicabilidad (E7e) — **completado por el practicante independiente**.

---

## 1. Datos del Evaluador y Entorno

| Campo | Tu Respuesta |
|---|---|
| **Nombre del Evaluador:** | Anahi Quisbert |
| **Rol / Años de experiencia en desarrollo:** | Analista de desarrollo de sistemas — 1 año de experiencia (.NET, SQL Server, WCF, REST) |
| **Fecha de inicio:** | 2026-10-20 |
| **Fecha de finalización / Tiempo dedicado:** | 2026-10-20 / 2 horas 45 minutos |
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
  No se puede distinguir entre "no se procesó" y "se procesó pero la respuesta se perdió". Eso admite duplicados: si se reintenta a ciegas, el externo puede aplicar el efecto de nuevo.

- **0.2 En tu criterio como desarrollador, ¿qué debería hacer el sistema ante ese escenario de fallo en lugar de un reintento a ciegas?**
  > *Escribe aquí tu propuesta:*  
  Consultar el estado real en el servicio externo antes de decidir. Si ya se procesó, no repetir; si no, reprocesar; si no se puede determinar, marcar indeterminado y reconciliar.

- **0.3 En una arquitectura de microservicios o multicapa (capa de integración vs. servicio específico de pago), ¿en qué capa ubicarías la lógica para resolver esta inconsistencia y por qué?**
  > *Escribe aquí tu justificación:*  
  En la capa de integración (`PasarelaPagos`), porque así hay un solo punto que decide y registra, y los servicios de abajo quedan como ejecución.

---

### TAREA 1. Clave de Identidad e Idempotencia
*(Lee ahora `01_metodo.md` y `02_decisiones.md` antes de continuar)*

- **1.1 Analizando los datos de la venta SOAT, ¿cuál propones que sea la clave lógica única para identificar la transacción en toda la cadena de servicios y en la base de datos?**
  > *Escribe aquí los campos elegidos:*  
  `(TRAMITE_SECUENCIAL, T_PAR_SIMPLE_TRAMITE_FK)` equivalente a `(TRAMITE_NUMERO, GESTION)`.

- **1.2 Si el cliente reintenta enviar la misma venta 3 veces, ¿cómo aseguras que el sistema reconozca que es exactamente la misma operación y no genere una duplicada?**
  > *Escribe aquí tu respuesta:*  
  Usando una clave estable y determinista compartida por todos los saltos, con índice único en la base sobre la llave lógica (Dc10). La misma operación produce siempre la misma llave.

---

### TAREA 2. Verificación del Estado Real ante Fallo
*(Revisa `03_consulta_estado_qr.md`)*

- **2.1 Ocurrido el corte de red simulado, ¿cómo averiguarías con certeza el estado real de la operación en el proveedor externo?**
  > *Escribe aquí cómo procederías y qué operación invocarías:*  
  Invocar la consulta de estado `UNIVIDA_*_QR_002` con el `IdQr` obtenido en la generación. El servicio externo es la fuente de verdad; no deducir el estado de la base local.

- **2.2 Si en tu tabla local la fila sigue en estado "Solicitado", pero al consultar al externo te devuelve código de estado `H` (Habilitado): ¿cuál es el estado real que debes adoptar y qué haces con la llamada de generación de QR?**
  > *Escribe aquí tu decisión técnica:*  
  Adoptar el estado del externo (`H` → Habilitado). No regenerar el QR; asociar el `IdQr` existente o anularlo si se aborta el trámite.

- **2.3 Si la consulta al externo no responde (timeout) o devuelve un estado no reconocido, ¿qué debe asumir el sistema y cuánto tiempo se debe esperar antes de forzar un cierre?**
  > *Escribe aquí tu respuesta:*  
  No asumir estado (indeterminado). Reconsultar hasta 20 minutos; si persiste, enviar a reconciliación. No forzar cierre asumiendo.

---

### TAREA 3. Toma de Decisiones Técnicas

- **3.1 Según el estado obtenido del externo, describe qué acción corresponde en cada caso:**
  - *Si el externo confirma que ya está Pagado (`P`):* No compensar; continuar el flujo de forma idempotente.
  - *Si el externo confirma que está Habilitado (`H`) pero el cliente abortó la compra:* Compensar (anular el QR no pagado).
  - *Si el externo confirma que nunca existió la solicitud:* Reprocesar / registrar inconsistencia.

- **3.2 ¿Cuál es el principio técnico que justifica tus respuestas anteriores?**
  > *Escribe aquí tu justificación:*  
  El estado del servicio externo es la fuente de verdad, no la memoria del proceso (principio rector del método).

---

### TAREA 4. Compensación e Idempotencia
*(Revisa `04_tablas_bitacora.md`)*

- **4.1 Para anular o compensar una operación sin riesgo de duplicar la anulación, ¿cómo identificarías unívocamente dicha acción compensatoria?**
  > *Escribe aquí qué campos o identificador usarías:*  
  Par `(operación, motivo)` = `(OPERACION_LOGICA, MOTIVO)`, con registro previo en la tabla de idempotencia de compensaciones.

- **4.2 En el flujo de compensación: ¿qué debe hacerse primero?**
  - Opción A: Invocar la anulación en el servicio externo y, si responde exitoso, registrar en la base de datos.
  - Opción B: Registrar la intención de compensación en la base de datos y luego invocar al servicio externo.
  > *Elige una opción y justifica por qué:*  
  Opción B. Registrar primero (estado `Registrada`) y luego ejecutar; si el proceso muere entre la llamada y el guardado, se pierde la idempotencia. Patrón Reserva-Ejecución-Confirmación.

- **4.3 Si el proceso se interrumpe y la compensación se dispara dos veces seguidas, ¿cómo evita el sistema ejecutar un doble efecto en el externo?**
  > *Escribe aquí tu explicación:*  
  La segunda invocación encuentra la fila `Registrada`/`Aplicada` (o el estado externo `ANULADO`) y omite la ejecución. Sin registro previo, se duplica.

---

### TAREA 5. Reconciliación de Operaciones Huérfanas

- **5.1 Si una operación quedó en estado indeterminado tras 20 minutos de reintentos, ¿cómo debe proceder el servicio en segundo plano (reconciliador) para cerrar el caso?**
  > *Escribe aquí tu propuesta:*  
  El reconciliador (servicio existente, cada 10 s) consulta el estado externo y converge la operación; mantiene en cola con alerta si persiste. No introduce infraestructura nueva.

- **5.2 Si el proceso de reconciliación se ejecuta dos veces seguidas sobre la misma operación huérfana, ¿qué resultado se espera?**
  > *Escribe aquí tu análisis:*  
  Idempotente: la segunda corrida no cambia el estado ni produce efectos duplicados.

---

### TAREA 6. Registro y Telemetría

- **6.1 Al auditar un fallo en la bitácora técnica, ¿qué datos mínimos consideras que deben quedar registrados para poder rastrear exactamente qué ocurrió?**
  > *Lista aquí los campos/datos que guardarías:*  
  Intento, causa, resultado, salto, timestamp, operación, código de error y estado externo consultado.

---

### TAREA 7. Cierre de la Operación

- **7.1 ¿Cómo debe quedar declarada finalmente la transacción? ¿Puede quedar alguna operación con estado "en proceso" indefinidamente?**
  > *Escribe aquí tu respuesta:*  
  Declarada con estado final convergido (procesado / no procesado / compensado). Ninguna operación queda "en proceso" indefinidamente; la reconciliación cierra los indeterminados.

- **7.2 Desde el punto de vista del negocio y de auditoría, ¿cuál es la diferencia entre declarar que una operación "No se ejecutó" frente a declarar que "No se pudo verificar su estado"?**
  > *Escribe aquí tu diferenciación:*  
  "No se ejecutó" = confirmado no procesado por el externo. "No se pudo verificar" = indeterminado, requiere reconciliación; no equivale a no ejecutado.

---

## 4. Evaluación de la Documentación (Feedback del Desarrollador)

1. **¿Qué partes de los documentos te resultaron ambiguas, confusas o difíciles de entender sin ayuda?**
   > *Escribe aquí tus observaciones:*  
   a) Confusión con `PENDIENTE` como categoría del método vs. letras devueltas por BUSA (`H`, `P`, `A`, `V`).  
   b) Discrepancia entre nombres lógicos `(TRAMITE_NUMERO, GESTION)` y columnas físicas `(TRAMITE_SECUENCIAL, T_PAR_SIMPLE_TRAMITE_FK)`.  
   c) Ausencia física de la tabla de compensaciones en la base de datos de desarrollo.

2. **¿Encontraste alguna discrepancia técnica entre la teoría del método y lo que viste en la base de datos o en los servicios?**
   > *Escribe aquí tus observaciones:*  
   Sobre `(TRAMITE_SECUENCIAL, T_PAR_SIMPLE_TRAMITE_FK)` no hay índice único y existen 44 pares duplicados (Dc10 no estaba aplicado). La tabla de compensaciones no existe físicamente. No se invocó el servicio real.

3. **¿Qué cambios o agregados concretos le harías a la documentación del método para que cualquier otro desarrollador pueda aplicarlo sin dudar?**
   > *Escribe aquí tus propuestas:*  
   a) Renombrar la categoría a `INDETERMINADO (PENDIENTE_RESOLUCION)` y explicitar que es sintética del método.  
   b) Agregar tabla de equivalencias conceptuales ↔ columnas físicas en las precondiciones.  
   c) Adjuntar script DDL de la tabla de compensaciones y del índice único Dc10, con advertencia visual sobre Reserva-Ejecución-Confirmación.

---
*Fin del cuaderno de trabajo. ¡Muchas gracias por tu evaluación!*