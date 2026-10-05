# E6. Diagnóstico con indicadores y criterios

> **Estado:** cerrada el 2026-10-04 y consolidada el 2026-10-05 con la línea base retrospectiva, la entrevista y el hallazgo de la reversión bancaria. Queda pendiente únicamente la línea base experimental (I3, I6, I7), prevista para el 2026-10-06, y aclarar la discrepancia de conteo de I2. Los criterios de éxito se congelaron **antes** de construir.

## 1. Evidencia del problema (multi-fuente)

| Fuente | Evidencia | Estado |
|:--|:--|:--|
| Literatura | La idempotencia y la deduplicación son requisitos estructurales con reintentos y mensajería al-menos-una-vez (Helland, 2012); la verificación de idempotencia tiene antecedente (Hummer et al., 2013) | Documentada |
| Código y configuración | `CPagoSimple.cs` (líneas 182–209) y `Web.config` (líneas 42–43): el reintento solo se dispara ante `ResultCode` 0003/0004; con `NumeroIntentos = 1`, `MilisegundosDelay = 1000` | Documentada |
| Comportamiento observado | Ante timeout o caída del servicio, se lanza excepción, se registra y se retorna el error 10002; **no se reintenta**. No se distingue "no procesado" de "procesado con respuesta perdida" | Documentada |
| Registros | `log4net 2.0.11`, `D:\IIS_Log4Net\WebServicePagoSimple\logInternos_yyyy-MM-dd.txt`; se registran solicitud, respuesta y excepciones, pero **no** el número de intento ni su resultado | Documentada |
| Tickets de soporte | 69 casos tipo EFECTIVIZAR en 2025 (BD_HELP_DESK → T_TICKET + T_REGISTRO) | Documentada |
| Personal de soporte | Entrevista realizada al personal de soporte de software (2026-10-05); ocho preguntas sobre manifestación del problema, intervención manual y registros deseables | Documentada |

## 2. Advertencia metodológica (subregistro)

La ausencia de instrumentación **no prueba la ausencia del problema**: solo prueba que la organización no puede detectarlo. Se distingue entre **no ocurrió** y **no se registró**. Además, los tickets EFECTIVIZAR son un **proxy indirecto** de operaciones inconsistentes: registran la intervención manual, no necesariamente el estado inconsistente, por lo que la cifra de 69 es un límite inferior sujeto a justificación.

## 3. Dos líneas base

1. **Retrospectiva** (2025): solo indicadores instrumentados.
2. **Experimental** (prospectiva): se reproduce la conducta actual en el entorno controlado sobre los seis escenarios, para los indicadores no instrumentados (I3, I6, I7).

## 4. Indicadores

| # | Indicador | Definición operacional | Unidad | Fuente | ¿2025? | Valor / línea base | Valor esperado |
|:--|:--|:--|:--|:--|:--|:--|:--|
| I1 | Operaciones con estado inconsistente | Operaciones que terminaron en estado distinto al esperado tras agotar los intentos | por año | Tickets EFECTIVIZAR (proxy) | Sí (indirecto) | ~69 | 0 en escenarios cubiertos |
| I2 | Errores no controlados | Operaciones que terminan en error no controlado (10002) | por año | Registros de aplicación | Sí | 2025: 4.302 errores y 226 timeouts | 0 en escenarios cubiertos |
| I3 | Tasa de reintentos | Reintentos por operación | razón | **No instrumentado** | No | Experimental | Reducir sin perder éxito |
| I4 | Intervenciones manuales | Correcciones manuales de operaciones inconsistentes | por año | Tickets de soporte | Sí | 69 | 0 en escenarios cubiertos |
| I5 | Tiempo de resolución | Tiempo medio para resolver una operación inconsistente | horas | T_TICKET.TIEMPO_RESPUESTA | Sí | 9,67 h | Reducir |
| I6 | Compensaciones con efecto duplicado | Compensaciones cuya repetición produce un efecto adicional | por escenario | **No instrumentado** | No | Experimental | 0 |
| I7 | Cobertura de modos de fallo | Modos anticipados sobre el total de la taxonomía | proporción | Entorno controlado | No | Experimental | Total del conjunto fijado |

## 5. Mecanismo actual de reintento (evidencia de código y configuración)

- `NumeroIntentos = 5` en el `Web.config` de producción, y hasta **10** en los meses de mayor transaccionalidad según el personal; **no existe registro de esa variación**. Con 5 reintentos hay hasta seis llamadas por operación.
- `MilisegundosDelay = 1000`: intervalo fijo de 1 segundo, con `Thread.Sleep`, que **bloquea el hilo** del servidor web.
- Disparador: `ResultCode == "0003"` (excepción genérica) o `"0004"` (excepción de base de datos).
- Terminación: al alcanzar el número de intentos o cuando el `ResultCode` deja de ser 0003/0004.
- Error no controlado: si `mensajeError` no está vacío, retorna el error **10002** ("El servicio no está disponible, por favor intente de nuevo más tarde").
- No hay manejo posterior al reintento; el flujo continúa hacia la validación de firma y puede terminar en 10003.
- No hay retardo progresivo (backoff): el intervalo es siempre 1 segundo.

**Aclaración (2026-10-05):** el valor real en producción es `NumeroIntentos = 5` (hasta 10 en meses pico), no 1 ni 2. La variación estacional **no está registrada**, lo que constituye en sí mismo un problema de trazabilidad. La cifra de I2 queda en **4.302 errores no controlados** y 226 timeouts en 2025; la mención de 10.000 ocurrencias fue un error de redacción del tesista y se descarta.

## 6. Comportamiento por escenario

| Escenario | Estado final | Código |
|:--|:--|:--|
| BUSA responde con éxito | Flujo normal | — |
| BUSA responde 0003/0004 | Se reintenta una vez; si sigue fallando, el flujo continúa con la respuesta errónea (posiblemente datos inválidos) | — |
| BUSA no responde (timeout o red caída) | Retorna error no controlado | 10002 |
| BUSA procesó pero se perdió la respuesta | Retorna error no controlado, **indistinguible** del caso anterior | 10002 |
| Error en la firma digital del XML | Error | 10001 |
| Error en la validación de la firma de respuesta | Error | 10003 |

**Hallazgo central:** el sistema no puede distinguir "no se procesó" de "se procesó pero se perdió la respuesta". Ante el segundo caso **no reintenta y no verifica**: cualquier reintento posterior (manual o del usuario) puede **duplicar** el QR o el pago. La idempotencia no está diseñada ni verificada.

## 7. Reproducción de la conducta actual (base de la línea base experimental)

- Cliente WCF `wsUniQrService.QRServiceClient`, método `BunApi(string mensajeSolicitud)`.
- Endpoint `https://172.30.140.139/UNIQRService/UNIQRService.svc`; binding `BasicHttpsBinding_IQRService`.
- **Sin timeout personalizado:** usa el valor por defecto de WCF (1 minuto).
- Reproducción: reintento con espera fija sobre el mismo conjunto de seis escenarios.

## 8. Riesgos identificados

| Riesgo | Descripción | Severidad |
|:--|:--|:--|
| Sin verificación posterior al error | No se comprueba si la operación se procesó cuando se pierde la respuesta | Alta |
| Posible duplicación | Un reintento manual o del usuario podría duplicar la operación | Alta |
| Sin manejo posterior al reintento | Si el reintento falla con 0003/0004, el flujo continúa con datos posiblemente inválidos | Alta |
| Sin timeout personalizado | Espera hasta 1 minuto por defecto de WCF | Media |
| Bloqueo de hilo | `Thread.Sleep(1000)` por cada reintento (hasta 5, y 10 en meses pico) bloquea el hilo del servidor | Alta |
| Reintento sin retardo progresivo | Intervalo fijo de 1 segundo | Baja |

## 9. Criterios de éxito (congelados el 2026-09-26, antes de construir)

**Utilidad:** (1) compensaciones con efecto duplicado = 0; (2) operaciones con estado inconsistente = 0; (3) repetir una compensación deja el estado final idéntico al de una ejecución única, en N de N escenarios.
**Contribución:** (4) aplicabilidad del método medida por cobertura de la taxonomía de fallos y aplicación independiente por practicantes.

## 10. Conjunto de escenarios (fijado el 2026-09-26)

1. Caída de un servicio a mitad de la transacción.
2. Superación del tiempo de espera.
3. Pérdida de respuesta.
4. Solicitud duplicada.
5. Reintento posterior a un éxito real.
6. Fallo durante la compensación.

## 11. Estado de la línea base (2026-10-05)

**Resuelto:**

- **I2:** valor de 2025 confirmado — 4.302 errores no controlados y 226 timeouts. Concentrados en enero (1.865) y diciembre (1.843), lo que coincide con los picos de renovación del seguro obligatorio.
- **Número de intentos:** 5 en producción, hasta 10 en meses pico; la variación no está registrada.
- **Proxy I1/I4:** justificado; los 69 tickets son intervenciones que no equivalen necesariamente a inconsistencias y subrepresentan el total.
- **Entrevista al personal de soporte:** realizada y documentada.

**Pendiente:**

- **I3, I6 e I7:** línea base experimental (sesión del 2026-10-06).

## 12. Entrevista al personal de soporte (2026-10-05)

Perfil: personal de soporte de software con conocimiento del flujo de pagos por QR. Hallazgos:

- Los problemas se manifiestan por interrupción de comunicación, demora o falta de respuesta del servicio externo; **no siempre se sabe si la operación se procesó**.
- Antes de volver a solicitar, el personal debe determinar qué ocurrió con la operación anterior; si se reprocesa sin verificar, **existe riesgo de duplicar**.
- Los 69 tickets no equivalen a 69 inconsistencias ni representan todos los casos; parte de las situaciones se resuelven sin ticket formal.
- El personal confirma la utilidad de **consultar el estado real antes de reintentar**.
- Información que considera valiosa registrar: identificación de la operación, intentos, estado, motivo, respuesta del servicio externo y acciones posteriores. Esto respalda directamente los componentes K2 y K9.

## 13. Hallazgo adicional: la reversión a nivel de banco (fuera de TI)

Existen casos en los que el **banco debita al cliente por QR, la pasarela no se entera y la venta no se efectiviza**; durante la conciliación con el banco puede revertirse el débito (por ejemplo, si el cliente ya compró por otros medios). Ese proceso **no involucra a soporte de TI**.

**Implicación:** existe una reconciliación **a nivel de banco**, distinta y posterior a la de la pasarela, ejecutada por el área de negocio. Se declara como **condición de contorno**: el método cubre la operación de la pasarela, no la conciliación financiera con el banco. A la vez, es evidencia del costo real del problema: débitos sin venta.
