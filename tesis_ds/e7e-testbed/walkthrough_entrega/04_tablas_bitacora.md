# Tablas disponibles para la bitácora

El método necesita dónde registrar. Estas son las tablas que ya existen y qué se propone usar
de cada una.

> **Procedencia de esta descripción.** El esquema se leyó directamente de `BD_PASARELA_PAGO`
> (entorno de desarrollo) el **2026-10-27**, consultando `sys.columns`, `sys.indexes` y las
> tablas paramétricas. Las tablas de QR **no están en `dbo`**: viven en el esquema
> **`PAGO_SIMPLE`**. Una versión anterior de este documento las describía a partir del código de
> acceso a datos; esa reconstrucción tenía columnas que no existen y fue reemplazada por el
> esquema real.

## `PAGO_SIMPLE.T_QR_SOLICITUD`

Es la tabla de estado. Una fila por solicitud de QR.

| Columna | Tipo | Nulo | Para qué se usa |
|---|---|---|---|
| `SECUENCIAL` | int (identity) | no | Clave de la solicitud |
| `TRAMITE_SECUENCIAL` | int | no | Identificador del trámite. **Parte 1 de la llave lógica** |
| `T_PAR_SIMPLE_TRAMITE_FK` | int | no | **Canal**, no la gestión. Ver la nota de abajo |
| `T_PAR_SIMPLE_ESTADO_SOLICITUD_FK` | int | no | Estado de la solicitud (1 a 7) |
| `RESULTADO_CODIGO` | nvarchar(10) | sí | Código que devolvió BUSA |
| `RESULTADO_MENSAJE` | nvarchar(200) | sí | Mensaje que devolvió BUSA |
| `RESULTADO_ID_QR` | nvarchar(40) | sí | Identificador del QR. Es el que se manda en la consulta de estado |
| `T_ADJUNTO_FK_DATO_QR` | decimal | sí | Vínculo con la imagen del QR |
| `IMPORTE` | decimal | no | Importe de la solicitud |
| `MONEDA` | nvarchar(6) | no | Moneda (por ejemplo `BOB`) |
| `MENSAJE_REFERENCIA` | nvarchar(120) | no | Referencia del mensaje |
| `T_PAR_SIMPLE_VALIDEZ_FK` | int | no | Vigencia aplicada |
| `FORMATO_QR` | int | no | Formato del QR |
| `SOLICITUD_FECHA` | nvarchar(20) | no | Fecha de la solicitud |
| `SOLICITUD_HORA` | nvarchar(16) | no | Hora de la solicitud |
| `T_SISTEMA_FK` | nvarchar(24) | no | Sistema de origen |
| `CORREOS_NOTIFICACION` | nvarchar(200) | no | Correos de notificación |
| `HABILITADO` | bit | no | Baja lógica del registro |
| `ADICIONADO_POR` / `FECHA_ADICION` | nvarchar(160) / datetime2 | no | Auditoría de alta |
| `MODIFICADO_POR` / `FECHA_MODIFICACION` | nvarchar(160) / datetime2 | sí | Auditoría de modificación |

### El canal no es la gestión

`T_PAR_SIMPLE_TRAMITE_FK` apunta a `PARAMETRICAS.T_PAR_SIMPLE_TRAMITE`, que describe el **canal**
del trámite:

| `T_PAR_SIMPLE_TRAMITE_FK` | Descripción |
|---|---|
| 1 | SOAT · APP Móvil (Pago Único) |
| 2 | SOAT · Página Web (Pago Único) |
| 3 | SOAT · UnividaNet (Pago Único) |
| 4 | SOAT · POS (Pago Único) |
| 5 | SOAT · Rural |

La **gestión** (el año) no está en esta tabla. Tampoco está en `T_QR_SOLICITUD`: hay que resolverla
desde el trámite. Esto importa porque la llave lógica del método es
`(TRAMITE_NUMERO, GESTION)`, y ninguna de las dos columnas de `T_QR_SOLICITUD` es la gestión.

## `PARAMETRICAS.T_PAR_SIMPLE_ESTADO_SOLICITUD`

Traduce el secuencial de estado a una letra:

| Secuencial | Letra | Descripción |
|---|---|---|
| 1 | `S` | Solicitado |
| 2 | `H` | Habilitado |
| 3 | `P` | Pagado |
| 4 | `A` | Anulado |
| 5 | `V` | Vencido |
| 6 | `R` | Rechazado |
| 7 | `E` | Error de imagen |

**Ojo con esto:** la consulta al externo solo devuelve `H`, `P`, `A` o `V`. Las letras `S`, `R` y
`E` existen en la tabla paramétrica pero no las devuelve la consulta de estado. El método llama
«indeterminado» al caso en que la consulta no alcanza a confirmar el estado; no es una letra que
devuelva BUSA.

## `PAGO_SIMPLE.T_QR_SOLICITUD_EJECUCION`

Una fila por ejecución de la solicitud.

| Columna | Tipo | Nulo |
|---|---|---|
| `SECUENCIAL` | int | no |
| `PARAMETROS_EJECUCION` | nvarchar(max) | sí |
| `T_PAR_SIMPLE_ESTADO_SOLICITUD_EJECUCION_FK` | int | sí |
| `MENSAJE` | nvarchar(8000) | sí |
| `FECHA_HORA_REGISTRO` | datetime2 | sí |
| `T_PAR_CANAL_FK` | int | sí |
| `ADICIONADO_POR` / `FECHA_ADICION` | nvarchar(160) / datetime2 | no |
| `MODIFICADO_POR` / `FECHA_MODIFICACION` | nvarchar(160) / datetime2 | sí |

## `PAGO_SIMPLE.T_QR_SOLICITUD_EJECUCION_INTENTOS`

Una fila por intento sobre una ejecución.

| Columna | Tipo | Nulo |
|---|---|---|
| `SECUENCIAL` | int | no |
| `T_QR_SOLICITUD_EJECUCION_FK` | int | sí |
| `EXITO` | bit | sí |
| `MENSAJE` | nvarchar(8000) | sí |
| `RESULTADO` | int | sí |
| `ADICIONADO_POR` / `FECHA_ADICION` | nvarchar(160) / datetime2 | no |
| `MODIFICADO_POR` / `FECHA_MODIFICACION` | nvarchar(160) / datetime2 | sí |

Esta tabla **no** tiene `TRAMITE_SECUENCIAL` ni un estado de ejecución propio. Para llegar al
trámite hay que pasar por `T_QR_SOLICITUD_EJECUCION`.

## Unicidad de la llave (Dc10)

Estado medido el 2026-10-27:

| Entorno | Pares duplicados sobre `(TRAMITE_SECUENCIAL, T_PAR_SIMPLE_TRAMITE_FK)` | ¿Índice único? |
|---|---|---|
| Desarrollo (DESA) | **1** | **No** |
| Producción | **39** | **No** |

Los índices que existen sobre `T_QR_SOLICITUD` son todos **no únicos**, salvo la clave primaria
sobre `SECUENCIAL`. En particular:

- `T_QR_SOLICITUD_NUI_TRAMITE_SECUENCIAL` — no único
- `T_QR_SOLICITUD_NUI_T_PAR_SIMPLE_TRAMITE_FK` — no único

Es decir: la condición que Dc10 exige **no está puesta** en ninguno de los dos entornos.

## Tabla nueva que hay que crear: idempotencia de compensaciones

Las tablas anteriores no alcanzan, porque no tienen dónde registrar la identidad
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
| Esquema leído de | `BD_PASARELA_PAGO` en el servidor de desarrollo (`NAD-DESA-C`) |
| Método de lectura | `sys.columns`, `sys.indexes`, `sys.index_columns`, tablas paramétricas |
| Fecha de lectura | 2026-10-27 |
| Escrituras realizadas | **ninguna** (solo `SELECT`) |
| Dato de producción | Medido el 2026-10-27 sobre `NAD-PROD-H`; 39 pares duplicados |
