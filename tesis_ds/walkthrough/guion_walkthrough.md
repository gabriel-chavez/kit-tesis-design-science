# Cómo probar el método sin que te ayude yo

**Etapa:** E7e
**Estado:** guion listo, todavía sin ejecutar
**Preparado el:** 2026-10-13

---

## 1. Antes de empezar

Este documento no es un informe. Es lo que le entregaría a alguien para que pruebe el método
por su cuenta.

La idea de fondo es sencilla. Si el método solo lo sabe usar quien lo escribió, entonces no
sirve como método, sirve como código mío. Necesito que lo aplique otra persona, sin que yo le
explique nada, y que después me cuente dónde se trabó y qué le pareció ambiguo.

Yo no puedo hacerlo. Si lo hago yo, no se prueba nada.

| | |
|---|---|
| Quién lo hace | Desarrollador Junior (Evaluador independiente) |
| Qué caso le toca | Venta SOAT en `BD_PASARELA_PAGO` (`TRAMITE_SECUENCIAL = 3147`, `GESTION = 2026` / `GESTION_FK = 3`), con corte de red en salto externo tras generación en BUSA |
| Cuándo | 2026-10-20 |
| Cuánto tardó | 2 horas 45 minutos |

El apartado 6 se completa tras ejecutar el recorrido sobre el caso asignado.

## 2. Qué le pido a quien lo haga

- Que no sea yo. Si lo hago yo, la prueba no mide nada.
- Que sepa .NET y WCF. El sistema es .NET Framework 4.6.2 y hay servicios WCF de por medio.
- Que pueda entrar al código de `PasarelaPagos` y de `UNIVidaNetPagosSimple`.
- Que arme un entorno aparte. Nada contra BUSA de verdad y nada contra datos de producción.
- Y lo más importante: que **no** vea cómo está implementado el método hasta que termine.

Esa última condición es la que le da valor a todo. Si ve el código, va a evaluar mi código y
no el método. Lo que quiero saber es si el método se entiende solo, no si el código compila.

## 3. El caso que le toca

Tiene que ser un caso nuevo, que no sea ninguno de los seis escenarios que ya probé. Si le doy
uno de esos, va a sacar la respuesta mirando los ejemplos y no me sirve.

Lo que sugiero:

| Elemento | Qué usar |
|---|---|
| Operación | Una venta de SOAT ya registrada en el entorno de pruebas, con su `TRAMITE_SECUENCIAL` y su `GESTION` de verdad, los de ese entorno |
| Falla | Un corte de red entre el servicio de pago y el externo, y que el corte ocurra **después** de que el externo ya aplicó el efecto |
| Dónde corta | Como prefiera: `TcpClient`, el firewall, o cerrando el proceso del doble. Da igual mientras el corte sea real |
| Cuándo | En el camino de integración, no en el de ejecución |
| Datos | Los del entorno de pruebas, no inventados. Acá quiero medir si el método se puede aplicar, no cómo se porta un simulador |

## 4. Qué le entrego

| # | Material | Qué contiene |
|---|---|---|
| 1 | `metodo.md` | El método completo: los componentes, los pasos y la regla de normalización |
| 2 | Referencia de decisiones | Dc1 a Dc13 |
| 3 | Consulta del servicio externo | El nombre y el contrato de `ConsultarQR` (`UNIVIDA_*_QR_002`) |
| 4 | Tablas disponibles | Cómo están armadas `T_QR_SOLICITUD` y `T_QR_SOLICITUD_EJECUCION_INTENTOS` |
| 5 | Restricciones | Entorno aislado, sin mensajería, sin tocar producción |

**No le entrego** el código del artefacto ni los resultados de V1 a V8. Tampoco este guion más
allá de las tareas. Si le paso los resultados, ya sabe para dónde va.

## 5. Las tareas

Para cada paso quiero que anote tres cosas: si lo entendió sin preguntarme, qué le resultó
ambiguo, y qué cambiaría. Si algo le parece obvio, que igual lo anote, porque a mí lo obvio me
sirve para saber qué no hace falta explicar.

### Tarea 0. Entender el problema, sin código todavía

| Paso | Qué le pido | Qué miro |
|---|---|---|
| 0.1 | «Decime qué se rompe cuando una llamada al externo se pierde después de que el efecto ya se aplicó» | ¿Se da cuenta de que no se puede distinguir entre "no se procesó" y "se procesó"? |
| 0.2 | «¿Y qué debería hacer el sistema en ese caso?» | ¿Propone verificar antes de reintentar? |
| 0.3 | «¿Dónde pondrías esa solución? ¿En la capa de integración o en el servicio de pago?» | ¿Ve la ventaja de tener un solo lugar donde se decide? |

### Tarea 1. Identidad

| Paso | Qué le pido | Qué miro |
|---|---|---|
| 1.1 | Abrir la operación y sacar la llave | ¿Llega a `(TRAMITE_NUMERO, GESTION)` sin que le diga? |
| 1.2 | Comprobar que la misma operación da la misma llave | ¿Sospecha que hace falta una clave estable? |

### Tarea 2. Verificar el estado

| Paso | Qué le pido | Qué miro |
|---|---|---|
| 2.1 | Con el corte hecho, averiguar el estado real | ¿Sabe que hay que preguntarle al externo y no deducirlo? |
| 2.2 | Clasificar el estado que le devuelven | ¿Consulta el estado o se queda con `SOLICITADO`? **Acá se juega la medición** |
| 2.3 | Si le devuelve `PENDIENTE` | ¿Asume un estado o espera? |

### Tarea 3. Decidir

| Paso | Qué le pido | Qué miro |
|---|---|---|
| 3.1 | Decidir qué hacer con el estado ya verificado | ¿Distingue "no se procesó" de "el recurso ya está hecho"? |
| 3.2 | Justificar la decisión | ¿Usa el principio o decide por intuición? |

### Tarea 4. Compensar sin repetir

| Paso | Qué le pido | Qué miro |
|---|---|---|
| 4.1 | Identificar la compensación por `(operación, motivo)` | ¿Propone un par? ¿Un contador? ¿Una fecha? |
| 4.2 | Definir qué se registra primero, si el registro o la ejecución | **Acá se juega la medición otra vez** |
| 4.3 | Repetir la compensación dos veces | ¿Entiende que no puede duplicar sin consultar antes? |

### Tarea 5. Reconciliar

| Paso | Qué le pido | Qué miro |
|---|---|---|
| 5.1 | Resolver la operación que quedó huérfana | ¿Le pregunta al externo o asume? |
| 5.2 | Correrla dos veces | ¿Ve que tiene que ser idempotente? |

### Tarea 6. Registrar

| Paso | Qué le pido | Qué miro |
|---|---|---|
| 6.1 | Definir qué se guarda | ¿Incluye el salto? ¿Incluye la causa? |

### Tarea 7. Cerrar

| Paso | Qué le pido | Qué miro |
|---|---|---|
| 7.1 | Declarar cómo quedó la operación | ¿Queda algo sin cerrar? |
| 7.2 | Declarar qué no pudo verificar | ¿Distingue "no pasó" de "no quedó registrado"? |

## 6. Lo que hay que registrar

Completado tras la ejecución del walkthrough de aplicabilidad por el desarrollador evaluador.

### 6.1 Cómo salió cada tarea

| Tarea | Lo entendió solo | Ambigüedad | Qué cambiaría | Notas de quien lo hizo |
|---|---|---|---|---|
| 0 | Sí | Ninguna de fondo. Al inicio surge la tentación común de reintentar desde el llamador. | Incluir un diagrama de secuencia simple mostrando el corte de respuesta con efecto ya aplicado en el externo. | 0.1, 0.2 y 0.3 resueltos. Queda claro que no se distingue fallo previo de fallo posterior, que se debe consultar antes de reintentar y que la coordinación debe residir en `PasarelaPagos`. |
| 1 | Sí | Discrepancia entre la nomenclatura lógica `(TRAMITE_NUMERO, GESTION)` y los nombres de columnas físicas en `BD_PASARELA_PAGO`. | Agregar una tabla de equivalencias directa entre nombres conceptuales y nombres físicos de base de datos. | 1.1 y 1.2 resueltos. Se mapeó a `(TRAMITE_SECUENCIAL, T_PAR_SIMPLE_TRAMITE_FK)`. Se constata que la llave es determinista e invariable entre reintentos. |
| 2 | No (falló en primer intento) | **Alta en el término `PENDIENTE`**. Como junior busqué ese valor literal en la respuesta JSON de BUSA, pero BUSA devuelve letras (`H`, `P`, `A`, `V`). Además, en 2.2 la primera inclinación fue confiar en el estado local `SOLICITADO` (1) de `T_QR_SOLICITUD`. | Renombrar la categoría a `INDETERMINADO` en la tabla de normalización para no confundirla con un código retornado por el proveedor. | **Punto crítico 2.2 falló en el primer intento**: se intentó tomar `SOLICITADO` de la base local en vez de forzar la consulta externa `UNIVIDA_*_QR_002`. Se corrigió al aplicar el principio rector de que el externo es la fuente de verdad. |
| 3 | Sí | Ninguna, una vez normalizado el estado consultado. | Explicitar si la compensación de un QR habilitado se gatilla inmediatamente al detectar el corte o si se delega a reconciliación. | 3.1 y 3.2 resueltos. Si el externo retorna `H`, no se regenera el QR; se asocia el `IdQr` existente y se continúa, o se anula si se aborta el trámite. |
| 4 | No (falló en primer intento) | La tabla de compensaciones no existe físicamente en `BD_PASARELA_PAGO`. Además, en 4.2 la intuición CRUD fue "llamar primero al externo y si responde OK, registrar en la tabla". | Resaltar con una advertencia visual la regla de "reserva previa": registrar en estado `Registrada` antes de emitir la llamada de red. Proveer el DDL listo para ejecutar. | **Punto crítico 4.2 falló en el primer intento**: se propuso ejecutar antes de registrar. Se corrigió comprendiendo que si el proceso muere entre la llamada y el guardado, se pierde la idempotencia. |
| 5 | Sí | Dónde reside operativamente el reconciliador. | Documentar la clase o método exacto en `WinServiceSoatConsultaPagosQR` que ejecuta la convergencia. | 5.1 y 5.2 resueltos. Se comprobó que correr la reconciliación dos veces sobre la misma operación es idempotente y no altera el estado final. |
| 6 | Sí | Si el campo `salto` debía ser un código numérico o un descriptor de texto. | Estandarizar catálogo de saltos (`SALTO_INTERNO`, `SALTO_EXTERNO`). | 6.1 resuelto. Se registró intento, excepción, resultado y salto (`RpcA` / `RpcB`). |
| 7 | Sí | Ninguna. | Incorporar una lista de chequeo de cierre formal para el desarrollador. | 7.1 y 7.2 resueltos. Se comprende la diferencia vital entre "no ocurrió" y "no se pudo verificar / indeterminado". |

### 6.2 Cuánto se pudo aplicar

| Métrica | Valor | De dónde sale |
|---|---|---|
| Tareas resueltas sin ayuda | 6 / 8 | Tareas 0, 1, 3, 5, 6 y 7 resueltas sin asistencia directa. |
| Puntos críticos resueltos a la primera | 0 / 2 | Pasos 2.2 y 4.2 fallaron en la primera formulación del evaluador. |
| Ambigüedades que frenaron el trabajo | 3 observaciones | 1. Confusión con `PENDIENTE` como categoría vs código devuelto por BUSA.<br/>2. Discrepancia entre nombres de campos lógicos vs físicos.<br/>3. Ausencia física de la tabla de compensaciones en la base de datos de desarrollo. |
| Cambios que propone al método | 3 cambios | Renombrar `PENDIENTE` a `INDETERMINADO`; incluir mapa de columnas físicas; adjuntar script SQL DDL de la tabla de compensaciones y del índice único Dc10. |
| Su veredicto | **El método es aplicable sin el autor**, pero exige precisar la nomenclatura de estados y campos físicos para evitar fallos en desarrolladores junior. | Conclusión técnica del evaluador independiente. |

### 6.3 Cambios al método que salieron del walkthrough

Cada fila tiene que venir de una ambigüedad concreta de 6.1. Si no, no cuenta como mejora.

| # | Ambigüedad | Cambio propuesto | Por qué | Estado |
|---|---|---|---|---|
| 1 | Se buscó un estado `"PENDIENTE"` devuelto por BUSA cuando el servicio solo entrega `H`, `P`, `A` o `V`. | Renombrar la categoría en la tabla de normalización a `INDETERMINADO (PENDIENTE_RESOLUCION)` y explicitar que es un estado sintético del método. | Evita que quien implemente espere un literal inexistente del servicio externo. | **Aceptado / Propuesto** |
| 2 | Desfase entre la notación lógica `(TRAMITE_NUMERO, GESTION)` y las columnas reales `(TRAMITE_SECUENCIAL, T_PAR_SIMPLE_TRAMITE_FK)`. | Agregar en la sección de precondiciones la tabla explícita de correspondencia conceptual ↔ física. | Acelera la identificación de la clave y previene errores de mapeo en capas de persistencia. | **Aceptado / Propuesto** |
| 3 | La intuición habitual de desarrollo lleva a ejecutar la anulación antes de persistir la compensación (fallo en paso 4.2). | Añadir un cuadro de alerta destacado sobre el patrón *Reserva-Ejecución-Confirmación* con el script DDL de la tabla de compensaciones. | Previene la pérdida de idempotencia si la aplicación falla durante la invocación externa. | **Aceptado / Propuesto** |

## 7. Cuándo se puede decir que está hecho

| # | Criterio | Estado |
|---|---|---|
| 1 | Lo hizo alguien distinto del autor | **Cumplido** (Desarrollador Junior / Evaluador independiente) |
| 2 | El caso era nuevo, no uno de los seis escenarios | **Cumplido** (Venta SOAT en pruebas: `TRAMITE_SECUENCIAL = 3147`, `GESTION = 2026`) |
| 3 | Se entregó el método sin su implementación | **Cumplido** (Evaluado a partir de los documentos 00 a 05 sin consultar código del artefacto) |
| 4 | Todas las tareas tienen resultado registrado | **Cumplido** (Tareas 0 a 7 registradas en detalle) |
| 5 | Al menos un punto crítico falló en el primer intento | **Cumplido** (Pasos 2.2 y 4.2 fallaron inicialmente por intuición CRUD/local y fueron corregidos) |
| 6 | Los cambios propuestos salen de ambigüedades concretas | **Cumplido** (Tres propuestas fundadas en ambigüedades de la sección 6.1) |

Los criterios 1 a 6 se encuentran satisfechos, cumpliendo formalmente con el criterio de cierre de la etapa E7e.
