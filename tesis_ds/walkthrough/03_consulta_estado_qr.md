# Consulta del estado del QR (`UNIVIDA_*_QR_002`)

Es la operación que el método usa para averiguar el estado real. Es la pieza sin la cual el
método no aplica: si no se puede consultar, la operación queda indeterminada y se declara así.

## Nombre y variantes

El nombre del servicio sale de la configuración y cambia según el tipo de trámite:

| Tipo de trámite | Clave de configuración | Valor |
|---|---|---|
| 1 | `ServicioBusaVerificarQr` | `UNIVIDA_QR_002` |
| 2 | `ServicioBusaVerificarQrWeb` | `UNIVIDA_WEB_QR_002` |
| 3 | `ServicioBusaVerificarQrNet` | `UNIVIDA_NET_QR_002` |
| 4 | `ServicioBusaVerificarQrPos` | `UNIVIDA_POS_QR_002` |
| 5 | `ServicioBusaVerificarQrRural` | `UNIVIDA_RURAL_QR_002` |

El código de servicio viaja dentro del sobre, no en la URL. La URL es la misma para todas las
operaciones de QR (`QRServiceClient.BunApi`).

## Cómo se invoca

1. Se obtiene la credencial con `FAtParSimpleCredencialOAutenticacion`, que devuelve `ApiKey`,
   `ApiServicio`, `ApiUsuario`, `ApiPassword` y los datos del certificado.
2. Se arma el objeto de petición:

| Campo | Tipo | Contenido |
|---|---|---|
| `RequestHeader.ApiKey` | string | De la credencial |
| `RequestHeader.Servicio` | string | El código de la tabla de arriba |
| `RequestHeader.Usuario` | string | De la credencial |
| `RequestHeader.Password` | string | De la credencial |
| `RequestHeader.Fecha` | string | `dd/MM/yyyy` |
| `RequestHeader.Hora` | string | `HH:mm:ss` |
| `RequestData.IdQr` | string | El identificador del QR que devolvió la generación |

3. El objeto se serializa a JSON y ese JSON se firma (`CrearMensajeXmlFirmado`).
4. El XML firmado se envía por `QRServiceClient.BunApi`.

## Qué devuelve

| Campo | Tipo | Contenido |
|---|---|---|
| `ResponseHeader.ResultCode` | string | `"0000"` cuando el servicio respondió bien |
| `ResponseHeader.Message` | string | Mensaje del servicio |
| `ResponseData.Estado` | string | **Acá está el dato que importa**: la letra de estado |
| `ResponseData.Detalle` | string | Detalle adicional |

## Estados que devuelve BUSA

`ResponseData.Estado` viene como **una letra**, y el sistema la traduce a un secuencial:

| Letra | Significado | Secuencial interno |
|---|---|---|
| `H` | Habilitado (el QR existe y no está pagado) | 2 |
| `P` | Pagado | 3 |
| `A` | Anulado | 4 |
| `V` | Vencido | 5 |
| (cualquier otra) | No reconocido | 0 |

Los secuenciales internos de `T_PAR_SIMPLE_ESTADO_SOLICITUD_FK` son: 1 Solicitado,
2 Habilitado, 3 Pagado, 4 Anulado, 5 Vencido, 6 Rechazado, 7 Error de imagen.

## Códigos de error que puede devolver el camino de consulta

| Código | Cuándo |
|---|---|
| 10001 | No se pudo firmar el mensaje, así que no se llegó a invocar al servicio |
| 10002 | El servicio lanzó una excepción (incluye timeout) |
| 10003 | La validación de firma de la respuesta falló |
| 20000 + `ResultCode` | El servicio respondió con un `ResultCode` distinto de `"0000"` |

## Un punto a tener en cuenta al aplicar el método

De los cinco valores que devuelve BUSA, **ninguno se llama `PENDIENTE`**. El método define la
categoría «indeterminado» para el caso en que la consulta no alcanza a confirmar el estado: el
timeout, la respuesta perdida, o un estado que no se reconoce. Es una categoría del método, no
un valor que devuelva el servicio.

---

**Procedencia**

| Campo | Valor |
|---|---|
| Fuente de los nombres de servicio | `Web.config` de `WebServicePagoSimple`, claves `ServicioBusa*Qr` |
| Fuente del contrato | `NegociosPagoSimple/Modulos/Busa/CPagoSimple.cs` y `ClasesSalida/CSResponse*.cs` |
| Fuente del mapeo de estados | `CMetodosGenericos.SecuencialEstadoBusa` y `ExternosNegociosPagoSimple/Modulos/Parametros/CConstantes.cs` |
| Verificación | Lectura directa del código en los repositorios, 2026-10-14. No se invocó el servicio real |
