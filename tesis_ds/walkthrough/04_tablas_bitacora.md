# Tablas disponibles para la bitácora

El método necesita dónde registrar. Estas son las tablas que ya existen y qué se propone usar
de cada una.

> **Cómo se obtuvo esta descripción.** Las bases de datos del entorno de pruebas que están
> accesibles no contienen las tablas de QR, así que **no se pudo leer el DDL**. Las columnas que
> figuran abajo salen de leer el código de acceso a datos que las usa (las clases de parámetros
> de entrada y las llamadas a los procedimientos). Es una reconstrucción desde el código, no una
> lectura del esquema. Si hace falta el detalle exacto, hay que leerlo en la base del entorno de
> pruebas con `sp_help`.

## `T_QR_SOLICITUD`

Es la tabla de estado. Una fila por solicitud de QR.

| Columna | Tipo esperado | Para qué se usa |
|---|---|---|
| `SECUENCIAL` | int | Clave de la solicitud |
| `TRAMITE_SECUENCIAL` | int | **Parte 1 de la llave lógica** (`TRAMITE_NUMERO`) |
| `T_PAR_SIMPLE_TRAMITE_FK` | int | **Parte 2 de la llave lógica** (`GESTION`) |
| `T_PAR_SIMPLE_ESTADO_SOLICITUD_FK` | int | Estado: 1 Solicitado, 2 Habilitado, 3 Pagado, 4 Anulado, 5 Vencido, 6 Rechazado, 7 Error de imagen |
| `RESULTADO_CODIGO` | string | Código que devolvió BUSA |
| `RESULTADO_MENSAJE` | string | Mensaje que devolvió BUSA |
| `RESULTADO_ID_QR` | string | Identificador del QR. Es el que se manda en la consulta de estado |
| `T_ADJUNTO_FK_DATO_QR` | decimal | Vínculo con la imagen del QR |
| `T_POLIZA_DETALLE_FK` | int | Vínculo con la póliza |
| `T_VEHI_SOAT_PROP_FK` | int | Vínculo con el vehículo (SOAT) |
| `CANAL_JSON` | string | Datos del canal |

**Nota de la decisión Dc10:** sobre `(TRAMITE_SECUENCIAL, T_PAR_SIMPLE_TRAMITE_FK)` **no hay
índice único todavía**, y hoy existen 44 pares duplicados. El método lo exige como condición
necesaria. El practicante debe darlo por hecho para su caso, pero conviene que sepa que en el
entorno real esa condición no estaba puesta.

## `T_QR_SOLICITUD_EJECUCION_INTENTOS`

Es la tabla de intentos. Una fila por intento sobre una solicitud.

| Columna | Tipo esperado | Para qué se usa |
|---|---|---|
| `T_QR_SOLICITUD_EJECUCION_FK` | int | Vínculo con la solicitud |
| `TRAMITE_SECUENCIAL` | int | Repite la llave para poder buscar sin hacer la unión |
| `T_PAR_SIMPLE_ESTADO_SOLICITUD_EJECUCION_FK` | int | Estado del intento |
| `EXITO` | bit | Si el intento salió bien |
| `MENSAJE` | string | Mensaje del intento |
| `RESULTADO` | int | Resultado numérico del intento |

## Tabla nueva que hay que crear: idempotencia de compensaciones

Las dos tablas anteriores no alcanzan, porque no tienen dónde registrar la identidad
`(operación, motivo)` de una compensación. Por eso el método pide **una tabla nueva** y solo una
(Dc4, Dc5).

Esquema sugerido:

| Columna | Tipo | Restricción | Para qué |
|---|---|---|---|
| `OPERACION_LOGICA` | varchar(64) | parte de la clave | Identifica la operación |
| `LLAVE_METODO` | varchar(128) | — | La llave derivada `(TRAMITE_NUMERO, GESTION)` |
| `MOTIVO` | varchar(64) | parte de la clave | Motivo de la compensación |
| `ESTADO` | varchar(16) | — | `Registrada` (reservada) o `Aplicada` (confirmada) |
| `INTENTO` | int | — | Número de intento |
| `REGISTRADO_EN` | datetime2 | — | Cuándo se reservó |
| `APLICADO_EN` | datetime2 | nullable | Cuándo se confirmó |
| `RESPUESTA` | nvarchar(max) | nullable | Respuesta del externo |

Con una **clave primaria o única sobre `(OPERACION_LOGICA, MOTIVO)`**.

El orden importa y es lo que hace que la repetición sea reconocible: se **registra primero**, se
ejecuta después, y se **confirma** cuando llega la respuesta. Si el proceso muere entre el
registro y la confirmación, queda una fila `Registrada` sin confirmar, y el método la resuelve
consultando al externo en vez de repetir a ciegas.

---

**Procedencia**

| Campo | Valor |
|---|---|
| Fuente de las columnas | Clases de parámetros de entrada de los agentes (`CPFAtQrSolicitud*`) y las llamadas a los procedimientos |
| DDL | **No leído.** Las bases conectadas no contienen las tablas de QR |
| Fuente de los estados | `ExternosNegociosPagoSimple/Modulos/Parametros/CConstantes.cs` |
| Fecha | 2026-10-14 |
