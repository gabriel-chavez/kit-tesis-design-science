# Cuaderno de Trabajo del Evaluador (Completado)
**Evaluación de Aplicabilidad de Método de Integración y Compensación**

---

## 1. Datos del Evaluador y Entorno

| Campo | Tu Respuesta |
|---|---|
| **Nombre del Evaluador:** | Leonel Chamaca Lima Analista de Mantenimiento y Soporte de Software
| **Rol / Años de experiencia en desarrollo:** | Desarrollador .NET / WCF (1.5 años de experiencia) |
| **Fecha de inicio:** | 2026-10-16 09:30 |
| **Fecha de finalización / Tiempo dedicado:** | 2026-10-16 12:15 (2 horas 45 minutos) |
| **Caso asignado:** | Venta SOAT en `BD_PASARELA_PAGO` (`TRAMITE_SECUENCIAL = 3147`, Gestión 2026 / `GESTION_FK = 3`) |
| **Escenario de falla:** | Corte de red en salto externo hacia BUSA tras creación del QR |

---

## 2. Instrucciones Generales Cumplidas

- [x] Tarea 0 resuelta con criterio propio antes de leer `01_metodo.md`.
- [x] Documentos `01_metodo.md` a `05_restricciones.md` consultados para Tareas 1 a 7.
- [x] Base de datos de pruebas `sad-desa` inspeccionada en las tablas de pagos (`PAGO_SIMPLE.T_QR_SOLICITUD`).
- [x] Código de implementación previo del autor no consultado para preservar la validez del ejercicio.

---

## 3. Tareas Resueltas

---

### TAREA 0. Análisis Conceptual del Problema (Resuelta antes de ver el método)

- **0.1 ¿Qué riesgo o problema técnico ocurre si la llamada se corta o da timeout justo después de que el externo ya procesó la operación?**
  > **Respuesta del Evaluador:**  
  > El cliente o el servicio llamador no sabe qué pasó realmente y asume que la operación falló porque recibió un error de timeout / conexión cerrada. Si el llamador reintenta la operación a ciegas, se generará una duplicación de transacciones (en BUSA se crearía un segundo QR o un cobro doble para el mismo trámite). El problema de fondo es que desde el lado del cliente un timeout *antes* de procesar se ve exactamente igual que un timeout *después* de procesar.

- **0.2 En tu criterio como desarrollador, ¿qué debería hacer el sistema ante ese escenario de fallo en lugar de un reintento a ciegas?**
  > **Respuesta del Evaluador:**  
  > Lo primero que debe hacer el sistema no es reintentar la llamada de creación, sino invocar una consulta o verificación hacia el servicio externo para preguntarle si la operación con ese número de trámite ya existe y en qué estado quedó. Solo después de conocer la respuesta del externo se debería decidir si se reintenta, se reutiliza o se cancela.

- **0.3 En una arquitectura de microservicios o multicapa (capa de integración vs. servicio específico de pago), ¿en qué capa ubicarías la lógica para resolver esta inconsistencia y por qué?**
  > **Respuesta del Evaluador:**  
  > La ubicaría en la capa de integración (`PasarelaPagos`). Si la coloco en el servicio de pago (`UNIVidaNetPagosSimple`), la pasarela aún podría sufrir un timeout intermedio con el servicio de pago, lo que crearía desincronizaciones entre ambas capas. Teniendo un único coordinador en la capa de integración se centraliza la bitácora de estado y se evita que coexistan dos políticas de reintento divergentes.

---

### TAREA 1. Clave de Identidad e Idempotencia

- **1.1 Analizando los datos de la venta SOAT, ¿cuál propones que sea la clave lógica única para identificar la transacción en toda la cadena de servicios y en la base de datos?**
  > **Respuesta del Evaluador:**  
  > La tupla natural `(TRAMITE_NUMERO, GESTION)`. Al revisar la base de datos `BD_PASARELA_PAGO`, esto corresponde físicamente a las columnas `(TRAMITE_SECUENCIAL, T_PAR_SIMPLE_TRAMITE_FK)` en la tabla `PAGO_SIMPLE.T_QR_SOLICITUD`. No basta con el número de trámite solo porque las gestiones anuales reinician la numeración.

- **1.2 Si el cliente reintenta enviar la misma venta 3 veces, ¿cómo aseguras que el sistema reconozca que es exactamente la misma operación y no genere una duplicada?**
  > **Respuesta del Evaluador:**  
  > Como la clave `(TRAMITE_SECUENCIAL, GESTION)` es determinista y proviene del dato del negocio (la venta del SOAT), ante cualquier reintento el sistema derivará exactamente la misma clave. Con la restricción de unicidad en la base de datos (decisión Dc10), el sistema detecta que el registro ya existe y no crea una nueva transacción, a diferencia de generar UUIDs aleatorios por request HTTP.

---

### TAREA 2. Verificación del Estado Real ante Fallo

- **2.1 Ocurrido el corte de red simulado, ¿cómo averiguarías con certeza el estado real de la operación en el proveedor externo?**
  > **Respuesta del Evaluador:**  
  > Invocando el servicio de consulta expuesto por BUSA: `UNIVIDA_*_QR_002` (`ConsultarQR`), enviando el `IdQr` obtenido o la referencia en el payload firmado con las credenciales de `FAtParSimpleCredencialOAutenticacion`.

- **2.2 Si en tu tabla local la fila sigue en estado "Solicitado", pero al consultar al externo te devuelve código de estado `H` (Habilitado): ¿cuál es el estado real que debes adoptar y qué haces con la llamada de generación de QR?**
  > **Respuesta del Evaluador (Tropiezo inicial y rectificación):**  
  > *Mi primera respuesta:* Iba a quedarme con el estado local `SOLICITADO` (1) y volver a ejecutar la llamada de generación de QR porque la llamada original dio excepción.  
  > *Corrección tras contrastar con el método:* Eso violaría el principio rector del método. El servicio externo es la única fuente de verdad; por tanto, debo adoptar el estado real `Habilitado` (`H` -> secuencial 2). **No debo volver a invocar la generación de QR** porque el QR ya existe en BUSA. Debo actualizar el registro local en `T_QR_SOLICITUD` con el `RESULTADO_ID_QR` y devolver el QR existente al cliente.

- **2.3 Si la consulta al externo no responde (timeout) o devuelve un estado no reconocido, ¿qué debe asumir el sistema y cuánto tiempo se debe esperar antes de forzar un cierre?**
  > **Respuesta del Evaluador:**  
  > El sistema no debe asumir ni éxito ni fracaso. Debe catalogar la operación como `INDETERMINADO` (la categoría que el método llama `PENDIENTE`) y reintentar la consulta periódicamente dentro de una ventana de 20 minutos. Si superados los 20 minutos no se resuelve, la operación pasa a la cola de reconciliación asíncrona.

---

### TAREA 3. Toma de Decisiones Técnicas

- **3.1 Según el estado obtenido del externo, describe qué acción corresponde en cada caso:**
  - *Si el externo confirma que ya está Pagado (`P`):* No se compensa ni se revierte. Se actualiza la base a Pagado (3) y se continúa el flujo normal de emisión de la póliza SOAT.
  - *Si el externo confirma que está Habilitado (`H`) pero el cliente abortó la compra:* Se debe ejecutar una compensación idempotente (anulación del QR en BUSA) para que el QR no quede activo.
  - *Si el externo confirma que nunca existió la solicitud:* Se marca localmente como rechazada o fallida, habilitando un reproceso limpio sin riesgo de duplicados.

- **3.2 ¿Cuál es el principio técnico que justifica tus respuestas anteriores?**
  > **Respuesta del Evaluador:**  
  > El principio rector de la Sección 2 del método: la base de datos externa del banco es la única fuente de verdad de los efectos aplicados; la bitácora interna solo coordina y acelera.

---

### TAREA 4. Compensación e Idempotencia

- **4.1 Para anular o compensar una operación sin riesgo de duplicar la anulación, ¿cómo identificarías unívocamente dicha acción compensatoria?**
  > **Respuesta del Evaluador:**  
  > Mediante la tupla `(OPERACION_LOGICA, MOTIVO)` (por ejemplo: `('SOAT_3147_2026', 'TIMEOUT_SALTO_EXTERNO')`), asociada a la llave lógica de la venta.

- **4.2 En el flujo de compensación: ¿qué debe hacerse primero?**
  - Opción A: Invocar la anulación en el servicio externo y, si responde exitoso, registrar en la base de datos.
  - Opción B: Registrar la intención de compensación en la base de datos y luego invocar al servicio externo.
  > **Respuesta del Evaluador (Tropiezo inicial y rectificación):**  
  > *Mi primera respuesta:* Elegí la Opción A porque en el desarrollo CRUD tradicional uno llama primero al servicio web y si da OK guarda en la base de datos.  
  > *Corrección tras contrastar con el método:* La Opción A es incorrecta en sistemas distribuidos. Si el servidor se apaga justo después de que el externo anuló el QR pero antes de que se ejecute el `INSERT`, el sistema olvidará que compensó y lo volverá a intentar. **La respuesta correcta es la Opción B (Reserva previa):** registrar primero la fila en estado `Registrada`, luego invocar a BUSA, y al recibir respuesta actualizar a `Aplicada`.

- **4.3 Si el proceso se interrumpe y la compensación se dispara dos veces seguidas, ¿cómo evita el sistema ejecutar un doble efecto en el externo?**
  > **Respuesta del Evaluador:**  
  > Gracias a la clave primaria/única sobre `(OPERACION_LOGICA, MOTIVO)` en la tabla de compensaciones. Al segundo intento, la base de datos rechaza la inserción duplicada y el compensador detecta que ya existe una compensación en curso o aplicada, evitando volver a llamar a BUSA.

---

### TAREA 5. Reconciliación de Operaciones Huérfanas

- **5.1 Si una operación quedó en estado indeterminado tras 20 minutos de reintentos, ¿cómo debe proceder el servicio en segundo plano (reconciliador) para cerrar el caso?**
  > **Respuesta del Evaluador:**  
  > El servicio programado (`WinServiceSoatConsultaPagosQR`) toma las operaciones huérfanas, vuelve a invocar `ConsultarQR` hacia BUSA y, con el estado definitivo obtenido, actualiza la bitácora. Si BUSA confirma que el QR expiró o no se pagó, ejecuta la compensación pendiente.

- **5.2 Si el proceso de reconciliación se ejecuta dos veces seguidas sobre la misma operación huérfana, ¿qué resultado se espera?**
  > **Respuesta del Evaluador:**  
  > Debe ser totalmente idempotente. En la segunda pasada, la operación ya tendrá un estado final consolidado y el reconciliador no modificará nada ni enviará comandos redundantes.

---

### TAREA 6. Registro y Telemetría

- **6.1 Al auditar un fallo en la bitácora técnica, ¿qué datos mínimos consideras que deben quedar registrados para poder rastrear exactamente qué ocurrió?**
  > **Respuesta del Evaluador:**  
  > Número de intento, timestamp, código de error/excepción técnica, mensaje devuelto, y de forma obligatoria el **salto** donde ocurrió (`RpcA` / `RpcB`, interno vs externo).

---

### TAREA 7. Cierre de la Operación

- **7.1 ¿Cómo debe quedar declarada finalmente la transacción? ¿Puede quedar alguna operación con estado "en proceso" indefinidamente?**
  > **Respuesta del Evaluador:**  
  > Ninguna operación puede quedar indefinidamente en proceso. Al finalizar debe quedar en un estado terminal: `Pagado`, `Habilitado`, `Anulado` o formalmente declarada como `Indeterminada transferida a reconciliación` con alerta.

- **7.2 Desde el punto de vista del negocio y de auditoría, ¿cuál es la diferencia entre declarar que una operación "No se ejecutó" frente a declarar que "No se pudo verificar su estado"?**
  > **Respuesta del Evaluador:**  
  > "No se ejecutó" significa que hay certeza de que el banco no aplicó ningún cambio (se puede descartar o reintentar sin riesgo). "No se pudo verificar su estado" significa que hubo una falla de comunicación y el banco podría haber cobrado el dinero; no se puede descartar ni reintentar a ciegas sin antes conciliar.

---

## 4. Feedback del Evaluador sobre la Documentación

1. **Ambigüedades detectadas:**
   - La categoría `PENDIENTE` en la tabla de normalización me confundió mucho. Creí que BUSA devolvía un string literal `"PENDIENTE"`, pero BUSA solo devuelve letras (`H`, `P`, `A`, `V`). Tardé tiempo en darme cuenta de que `PENDIENTE` era un estado inventado por el método para agrupar fallas de consulta.
   - En la base de datos `BD_PASARELA_PAGO`, las columnas se llaman `TRAMITE_SECUENCIAL` y `T_PAR_SIMPLE_TRAMITE_FK`, mientras que el texto del método usa `TRAMITE_NUMERO` y `GESTION`.

2. **Discrepancias técnicas encontradas:**
   - La tabla de compensaciones `(OPERACION_LOGICA, MOTIVO)` descrita en el documento 04 **no existe todavía** en la base de datos de desarrollo `sad-desa`; hay que crearla mediante script.
   - En `T_QR_SOLICITUD` todavía no existe el índice único sobre trámite y gestión (Dc10), existiendo registros duplicados históricos.

3. **Cambios propuestos al método:**
   - Renombrar en la documentación el estado `PENDIENTE` a `INDETERMINADO` para evitar que otros programadores busquen ese texto en el JSON de BUSA.
   - Incluir una tabla de mapeo explícita entre nombres lógicos y columnas físicas de SQL Server.
   - Resaltar con una advertencia en negrita que en compensaciones se **registra primero en BD y se ejecuta después en la red** (reserva previa), porque los desarrolladores tendemos a hacer lo opuesto por inercia de desarrollo CRUD.

---
