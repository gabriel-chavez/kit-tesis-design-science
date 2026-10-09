# E6 — Diagnóstico y caracterización del problema

**Actividad:** E6 — Línea base del comportamiento actual
**Investigación:** «Método para el diseño y la verificación de compensaciones idempotentes en transacciones distribuidas basadas en Sagas: un estudio de Design Science»


---

## Índice

- [0. Advertencia sobre la naturaleza de la evidencia](#0-advertencia)
  - [Cómo leer los tres indicadores](#cómo-leer-los-tres-indicadores)
  - [Decisión metodológica sobre `NumeroIntentos = 5`](#decisión-metodológica-sobre-numerointentos--5)
- [A. Informe de inspección inicial](#a-informe-de-inspección-inicial)
  - A.1 Arquitectura · A.2 Flujo de procesamiento · **A.3 Lógica de reintentos** · A.4 Timeouts · **A.5 Compensaciones** · A.6 Consulta de estado · A.7 Instrumentación · A.8 Entorno
- [B. Plan de pruebas](#b-plan-de-pruebas)
  - B.1 a B.6 modos de fallo · **B.7 EC1** (prueba de sensibilidad, *fuera del conjunto E6*)
- [C. Matriz de resultados](#c-matriz-de-resultados)
- [D. Resultados de I3, I6, I7](#d-resultados-de-i3-i6-i7)
  - [D.1 I3](#d1-i3--comportamiento-y-tasa-de-reintentos) · [D.2 I6](#d2-i6--efectos-duplicados-en-la-repetición-de-una-anulación) · [D.3 I7](#d3-i7--cobertura-de-modos-de-fallo) · [D.4 Hallazgos](#d4-hallazgos-principales)
- [E. Evidencia reproducible](#e-evidencia-reproducible)
- [F. Conclusión de línea base](#f-conclusión-de-línea-base)
- [Anexo — Resumen de métricas](#anexo--resumen-de-métricas)

### Resumen de los tres indicadores

| Indicador | Valor reportado | Advertencia esencial |
|---|---|---|
| **I3** | RpcA transporte **0 %**; RpcA negocio `0003` **100 %**; RpcB transporte **100 %** | Tasas **observadas en escenarios seleccionados**, con denominadores de 2 a 4 operaciones. **No medido en producción** |
| **I6** | **Ningún efecto adicional** en los contadores observables | Medido **solo sobre el doble local**, diseñado con idempotencia por referencia. **No demuestra idempotencia de BUSA** |
| **I7** | **6 / 6 = 100 %** de los modos de fallo definidos | El denominador fue **definido por el investigador**. No representa los fallos posibles del sistema real |

---

## 0. Advertencia

Ninguna prueba se ejecutó contra BUSA real ni contra datos de producción. Verificamos que `172.30.140.139:443` es alcanzable desde la máquina y **no lo contactamos**.

La evidencia combina cuatro fuentes con validez distinta:

| Fuente | Qué es | Qué sostiene | Qué NO sostiene |
|---|---|---|---|
| **Inspección estática** | Lectura de código y configuración | Estructura del algoritmo, valores configurados | Comportamiento en ejecución |
| **Réplica del algoritmo** | Copia fiel del control de flujo | Qué rama se ejecuta, cuántas solicitudes emite | Que el binario desplegado se comporte igual |
| **Doble de servicio HTTP** | Simulador local de BUSA | Fallos de transporte genuinos; estado real que el cliente no ve | Que BUSA real tenga la misma semántica |
| **Configuración experimental** | `test-config/Web.config` | Sensibilidad al parámetro `NumeroIntentos` | Comportamiento en producción con ese valor |

**Ninguna demuestra el comportamiento del sistema desplegado.** Se requeriría un entorno de pruebas autorizado con el binario real.

### Cómo leer los tres indicadores

| Indicador | Qué mide | Dónde se reporta | Advertencia de lectura |
|---|---|---|---|
| **I3** | Comportamiento y tasa de reintentos | [D.1](#d1-i3--comportamiento-y-tasa-de-reintentos) | Las tasas son **observadas en escenarios seleccionados**, con denominadores de 2 a 4 operaciones. No representan el volumen del sistema |
| **I6** | Efectos duplicados al repetir una operación | [D.2](#d2-i6--efectos-duplicados-en-la-repetición-de-una-anulación) | Se midió **solo sobre el doble local**, diseñado con idempotencia por referencia. No demuestra idempotencia de BUSA |
| **I7** | Cobertura de modos de fallo | [D.3](#d3-i7--cobertura-de-modos-de-fallo) | El 100 % mide la cobertura del **conjunto definido por el investigador**, no de los fallos posibles del sistema real |

### Decisión metodológica sobre `NumeroIntentos = 5`

Se solicitó evaluar el cambio de `NumeroIntentos` de 1 a 5. **No se modificó el archivo de producción.** El valor se aplicó como parámetro experimental en `e6-testbed\test-config\Web.config`.

Razón: E6 documenta la línea base del comportamiento **actual**. Alterar la configuración de producción violaría la restricción *"No modifiques el código de producción ni la lógica de negocio para facilitar las pruebas"* y contaminaría la línea base. Variar el parámetro pertenece a la evaluación del método propuesto.

| Archivo | Valor | Estado |
|---|---|---|
| `WebServicePagoSimple\Web.config` (producción) | `NumeroIntentos = 1` | **INTACTO** |
| `e6-testbed\test-config\Web.config` | `NumeroIntentos = 5` | Banco de pruebas |

---

# A. Informe de inspección inicial

## A.1 Arquitectura y componentes

Dos soluciones cubren el flujo de pagos QR:

| Solución | Rol |
|---|---|
| `UNIVidaNetPagosSimple.sln` | **Pasarela de pago.** WCF service que habla con BUSA |
| `PasarelaPagos.sln` | **Integración y procesos.** Consume la pasarela y ejecuta ventas |

```
WebServicePagoSimple          WCF público (IwsQRPagoSimple)
└── NegociosPagoSimple        Lógica de negocio: CPagoSimple
    └── AgenteServiciosPagoSimple   Acceso a BD
        └── BD_PASARELA_PAGO

WebServicePasarelaPagos       WCF de integración (CNPagosQR)
WinServiceSoatConsultaPagosQR     Consulta estado de pagos QR   ← convergencia
WinServiceSoatAnulaQR            Anula QR no pagados
WinServiceSoatConsultaAnulaPagosQR  Variante asíncrona
WinServiceSoatEfectivizacion        Efectiviza ventas (procesa lote)
WinServiceNotificaVulcan           Notifica a Vulcan
```

`CPagoSimple.cs` (1 160 líneas): `GenerarQRBusa`, `AnularQRBusa`, `ConsultarQRBusa`, `LlamadaServicioBusaQr`, `CrearMensajeXmlFirmado`, `ValidaMensajeFirmado`.

## A.2 Flujo actual de procesamiento QR

```
Cliente (CoreSOAT / UNIVidaNet / POS / Web)
   │
   ├─ ObtenerQR ──► GenerarQRBusa ──► [firmar XML] ──► BUSA _QR_001
   │                    └─► guarda T_QR_SOLICITUD (SOLICITADO)
   │
   ├─ Cliente paga el QR (canal externo)
   │
   ├─ WinServiceSoatConsultaPagosQR ──► consulta estado cada 10 s
   │                                    └─► si PAGADO, encola ejecución
   │
   ├─ WinServiceSoatEfectivizacion ──► emite venta
   │                                    └─► T_QR_SOLICITUD_EJECUCION
   │                                    └─► T_QR_SOLICITUD_EJECUCION_INTENTOS
   │
   └─ AnularQR ──► AnularQRBusa ──► BUSA _QR_003
```

## A.3 Lógica real de reintentos — discrepancia crítica

**Dos mecanismos distintos, en proyectos distintos, ambos activos.**

### Réplica A — `CPagoSimple.cs:182-210`

```csharp
int numeroIntentos = Convert.ToInt32(CMetodosGenericos.ObtenerValorLlave("NumeroIntentos"));
int milliseconds    = Convert.ToInt32(CMetodosGenericos.ObtenerValorLlave("MilisegundosDelay"));
int intento = 0;

while (intento < numeroIntentos && (ResultCode == "0003" || ResultCode == "0004"))
{
    Thread.Sleep(milliseconds);
    mensajeXmlBusa = LlamadaServicioBusaQr(...);
    intento++;
}
```

El reintento está **dentro** del `if` que evalúa `ResultCode`. Si la llamada lanza excepción, el `return` de error 10002 (líneas 165-177) se ejecuta y **el bucle nunca se alcanza**.

### Réplica B — `CNPagosQR.cs:29-195`

```csharp
bool repetirIntento = Convert.ToBoolean(CMetodosGenericos.ObtenerValorLlave("RepetirIntento"));
...
catch (Exception e)
{
    if (repetirIntento)
    {
        Thread.Sleep(milliseconds);
        oResultadoJsonDocument = ws.ObtenerQR(oEIntegracionObtenerQR);   // reintento
    }
}
```

**No hay bucle.** Reintento único dentro del `catch`, disparado por **cualquier** excepción, incluido timeout.

### Discrepancia

| | Réplica A | Réplica B |
|---|---|---|
| Clave | `NumeroIntentos` = **1** | `RepetirIntento` = **true** |
| ¿Existe `NumeroIntentos`? | Sí | **No existe en ningún `.config`** (verificado) |
| Mecanismo | `while` | `catch` + reintento único |
| Dispara ante timeout | **No** | **Sí** |
| Dispara ante `ResultCode` 0003/0004 | Sí | No evalúa el código |
| `MilisegundosDelay` | 1000 ms | 1000 ms |

**Implicación:** la misma condición de fallo produce comportamientos opuestos según la ruta de entrada.

## A.4 Tratamiento de timeouts y respuestas perdidas

| Aspecto | Estado |
|---|---|
| Timeout de transporte configurado | **No existe.** No se declara `openTimeout`, `closeTimeout`, `sendTimeout` ni `receiveTimeout` |
| Valor efectivo | Default de WCF (`basicHttpBinding`) |
| Timeout explícito del cliente | Ninguno; solo `Thread.Sleep(1000)` entre intentos |

`LlamadaServicioBusaQr` (líneas 1088-1112) captura `Exception` sin distinguir:

| Excepción colapsada | Tipo |
|---|---|
| Timeout | `TimeoutException` |
| Conexión perdida | `CommunicationException` |
| Respuesta malformada | `ProtocolException` |
| Fallo de negocio BUSA | `FaultException` |

**Los cuatro colapsan al mismo resultado observable.** Tras el fallo, el código retorna 10002 sin consultar el estado.

## A.5 Mecanismos de compensación existentes

**No hay compensación explícita en el sentido de Saga.** El sistema anula, no compensa.

| Componente de una compensación Saga | ¿Existe? | Evidencia |
|---|---|---|
| Acción de compensación definida | **No** — la anulación cancela una solicitud; no compensa un pago | `WinServiceSoatAnulaQR` |
| Registro de compensaciones ejecutadas | **No** — no hay tabla ni bitácora de anulaciones | `T_QR_SOLICITUD` sin columna de compensación |
| Verificación de compensación previa antes de reintentar | **No** — el reintento no consulta si la anulación previa se aplicó | Réplicas A y B |
| Reversión de un pago ejecutado | **No existe** | BUSA no la ofrece |

| Operación | Servicio BUSA | Condición |
|---|---|---|
| Anular QR | `UNIVIDA_*_QR_003` | **Solo si no está pagado** |
| Revertir pago | **No existe** | — |

`WinServiceSoatAnulaQR.cs:64` documenta la regla:
```csharp
// solo cambia el estado a cancelado y notifica a vulcan si el estado es solicitado
if (estadoSolicitud == 1) { ... }
```

**Límite funcional declarado:** BUSA permite anular una solicitud QR antes de ser pagada, pero **no proporciona reversión de pagos ejecutados**.

### Implicación para el indicador I6

Como **no existe una compensación implementada**, I6 no puede evaluarse sobre una compensación real. Se evalúa **la repetición de la anulación de un QR no pagado**, que es la operación repeatable más cercana a una compensación disponible en el sistema actual.

Este es un **sustituto funcional elegido para el experimento**, no una afirmación de que el sistema implemente compensaciones idempotentes.

| Operación | ¿Repetible? | Incluida en I6 |
|---|---|---|
| Anulación de QR **no pagado** | Sí | **Sí** |
| Anulación de QR **pagado** | Rechazada con `ResultCode 0009` | **No** — fuera del alcance |
| **Reversión de un pago ejecutado** | No existe | **No** — fuera del alcance funcional |

## A.6 Capacidad de consultar el estado externo

**Existe y funciona, pero no se invoca tras un fallo.**

| Componente | Detalle |
|---|---|
| Servicio | `WinServiceSoatConsultaPagosQR.cs` |
| Intervalos | `ConsultaIntervaloSegundos` = 10 s; `ProcesarIntervaloSegundos` = 5 s |
| Lote | `FAtQrSolicitudPendientesPagoListar` |
| Invocación | `CNPagosQR.Consultar()` → `ws.ConsultarQR()` → BUSA `_QR_002` |

**Discrepancia estructural:** el servicio de consulta existe para converger estados, pero el código de reintento de las réplicas A y B **no lo llama** tras una excepción.

## A.7 Limitaciones de instrumentación

| Limitación | Consecuencia |
|---|---|
| Sin contador de intentos por operación | I3 no medible en producción sin analizar logs |
| Sin marca de "reintento" en el log | Imposible distinguir reintento de primera llamada por texto |
| Sin correlación operación↔intento | `CIdentificador` se registra pero no se expone en el log de texto |
| Sin registro del estado consultado | No se sabe si el sistema alguna vez consultó |
| Índice no único en BD | Deduplicación no garantizada |

### Evidencia en producción

`BD_PASARELA_PAGO.PAGO_SIMPLE.T_QR_SOLICITUD`:

| Métrica | Valor |
|---|---|
| Filas totales | 892 970 |
| Pares `(TRAMITE_SECUENCIAL, T_PAR_SIMPLE_TRAMITE_FK)` distintos | 892 926 |
| **Pares duplicados** | **44** |

Índices — **ninguno único** sobre la llave lógica:

```
PK_T_QR_SOLICITUD                            ÚNICO  (SECUENCIAL)
T_QR_SOLICITUD_NUI_TRAMITE_SECUENCIAL        NO ÚNICO
T_QR_SOLICITUD_NUI_T_PAR_SIMPLE_TRAMITE_FK   NO ÚNICO
T_QR_SOLICITUD_NUI_RESULTADO_ID_QR_HABILITADO NO ÚNICO
...
```

## A.8 Entorno utilizado

| Aspecto | Detalle |
|---|---|
| Entorno | **Aislado, local** |
| Componentes originales | **No modificados** |
| BUSA real | **No invocado** |
| Datos de producción | **Solo lectura** (`SELECT`) |
| Doble | HTTP en `localhost:5099` |
| Datos sintéticos | `SINT-*`, placas y credenciales ficticias |

---

# B. Plan de pruebas

## B.0 Restricciones aplicadas

| Restricción | Cumplimiento |
|---|---|
| No modificar código de producción | Código nuevo en `e6-testbed/`; `Web.config` de producción intacto |
| No ejecutar operaciones reales contra BUSA | Doble local; BUSA nunca invocado |
| No alterar datos de producción | Solo `SELECT` |
| No afirmar pruebas no ejecutadas | Cada resultado indica estado de ejecución |
| No inventar resultados | `no medido` donde no hay dato |

## B.1 Escenario 1 — Fallo durante una transacción

| Campo | Detalle |
|---|---|
| **Objetivo** | Observar interrupción, número de intentos, estado resultante |
| **Precondición** | Doble activo; operación inexistente |
| **Inyección** | `/admin/fallo FallaAntesDeProcesar;{id};99` → 502 sin efecto |
| **Hipótesis** | A no reintenta; B reintenta una vez; ambos devuelven indeterminado |
| **Observación** | Solicitudes, reintentos, estado cliente, estado real |

## B.2 Escenario 2 — Timeout

| Campo | Detalle |
|---|---|
| **Objetivo** | Verificar reintento automático y consulta de estado tras timeout |
| **Precondición** | Timeout del cliente = 3 s (parámetro de prueba) |
| **Inyección** | `/admin/fallo TimeoutDespuesDeProcesar;{id};99` → aplica efecto, retiene 30 s |
| **Hipótesis** | El efecto queda aplicado; el cliente no puede saberlo |
| **Observación** | Excepción, intentos, si consulta estado, estado real |

## B.3 Escenario 3 — Respuesta perdida tras procesamiento

| Campo | Detalle |
|---|---|
| **Objetivo** | Determinar si el sistema distingue "no procesada" de "procesada con respuesta perdida" |
| **Precondición** | El doble distingue internamente los dos casos |
| **Inyección** | Variante A: timeout post-efecto. Variante B: `HttpContext.Abort()` post-efecto |
| **Hipótesis** | Ambos casos indistinguibles para el cliente |
| **Observación** | Estados devueltos, efectos acumulados, estado final |

## B.4 Escenario 4 — Solicitud duplicada

| Campo | Detalle |
|---|---|
| **Objetivo** | Verificar deduplicación por operación lógica |
| **Precondición** | Mismo `IdOperacionLogica` en varias llamadas |
| **Inyección** | Tres ejecuciones consecutivas sin fallo |
| **Hipótesis** | El doble deduplica; la base de producción no lo garantiza |
| **Observación** | Solicitudes, efectos acumulados, índices reales |

## B.5 Escenario 5 — Reintento tras resultado exitoso

| Campo | Detalle |
|---|---|
| **Objetivo** | Verificar efectos adicionales al repetir |
| **Precondición** | Operación ya generada |
| **Inyección** | Repetir la misma operación lógica |
| **Hipótesis** | Idempotencia depende del servicio externo |
| **Observación** | Contadores antes/después; distinción pagada vs no pagada |

## B.6 Escenario 6 — Fallo durante la repetición de una anulación

> **Precisión terminológica:** el escenario se denominaba «fallo durante una compensación». Como el sistema **no implementa compensaciones** (ver A.5), se reformula como «fallo durante la repetición de una anulación». La anulación se trata aquí como **operación funcional equivalente** para el experimento, no como una compensación existente.

| Campo | Detalle |
|---|---|
| **Objetivo** | Verificar registro y reintentabilidad |
| **Precondición** | Operación `Generada` (no pagada) |
| **Inyección** | `/admin/fallo FallaEnCompensacion;{id};1` → aplica anulación, aborta |
| **Hipótesis** | Cliente recibe excepción; el efecto puede haberse aplicado |
| **Observación** | Estado tras fallo, tras reintento, contador de anulaciones |

## B.7 EC1 — Prueba de sensibilidad de `NumeroIntentos` (1 vs 5)

> **Nota de nomenclatura:** esta prueba se identifica como **EC1** para no confundirla con la etapa E7 de la investigación, dedicada al diseño y construcción del artefacto. EC1 es una **prueba de sensibilidad de configuración**, no un modo de fallo adicional.

| Campo | Detalle |
|---|---|
| **Objetivo** | Medir el efecto de aumentar el número máximo de intentos |
| **Precondición** | Modos de error de **negocio** (`0003`), no de transporte |
| **Inyección** | A) `ErrorBusaPorExcepcion` siempre. B) 4 errores `0003` y luego éxito |
| **Hipótesis** | Con 1 intento no converge; con 5 sí, si BUSA se recupera en ≤5 intentos |
| **Observación** | Solicitudes emitidas, reintentos, resultado final, estado real |
| **Configuración** | `test-config\Web.config` → `NumeroIntentos = 5` |

**Nota metodológica:** este escenario requiere los modos `0003`/`0004` porque la condición del `while` de la Réplica A exige un `ResultCode` recibido. Con fallos de transporte, el bucle nunca se alcanza y cambiar el parámetro no tendría efecto observable.

---

# C. Matriz de resultados

Fecha de ejecución: **2026-10-08**. Doble HTTP: `localhost:5099`.

| ID | Escenario | Estado | Solicitudes | Reintentos | Efectos duplicados | Estado final (cliente) | Evidencia (L = `evidencia_stub.txt`) |
|---|---|---|---|---|---|---|---|
| **1A** | Fallo pre-procesamiento, RpcA | Ejecutado | 1 | **0** | No aplica | INDETERMINADO / 10002 | L20-29 |
| **1B** | Fallo pre-procesamiento, RpcB | Ejecutado | 2 | 1 | No aplica | INDETERMINADO / 0 | L31-43 |
| **2** | Timeout (RpcB) | Ejecutado | 2 | 1 | No aplica | INDETERMINADO / 0 | L48-62 |
| **3A** | Respuesta perdida (timeout), RpcA | Ejecutado | 1 | **0** | **No observables** | INDETERMINADO / 10002 | L68-77 |
| **3B** | Respuesta perdida (timeout), RpcB | Ejecutado | 2 | 1 | **No observables** | INDETERMINADO / 0 | L80-91 |
| **3C** | Respuesta perdida (abort), RpcB | Ejecutado | 2 | 1 | **No observables** | INDETERMINADO / 0 | L94-105 |
| **3D** | Comparación de los tres casos de respuesta perdida | **No es una ejecución** | 0 | — | No aplica | Los 4 casos → `INDETERMINADO` | L107-114 |
| **4** | Solicitud duplicada | Ejecutado | 3 | — | **No en el doble** | Generada | L117-124 |
| **5A** | Reintento post-éxito | Ejecutado | 2 | — | **No observables** | Generada | L129-132 |
| **5B** | Anulación no pagada | Ejecutado | 1 | 0 | No aplica | ANULADA (`0000`) | L135-144 |
| **5B'** | Anulación pagada | Ejecutado | 1 | 0 | No aplica | FALLIDO `0009` | L146-155 |
| **6A** | Anulación fallida (no pagada) | Ejecutado | 1 | 0 | **No observables** | INDETERMINADO | L160-172 |
| **6B** | Reintento de anulación | Ejecutado | 1 | 0 | **No observables** | ANULADA | L174-185 |
| **EC1-A-1** | `NumeroIntentos=1`, `0003` siempre | Ejecutado | 2 | 1 | No aplica | FALLIDO / 10003 | L199-210 |
| **EC1-B-1** | `NumeroIntentos=1`, 4×`0003`+éxito | Ejecutado | 2 | 1 | No aplica | FALLIDO / 10003 — **no converge** | L212-224 |
| **EC1-A-5** | `NumeroIntentos=5`, `0003` siempre | Ejecutado | **6** | **5** | No aplica | FALLIDO / 10003 | L227-246 |
| **EC1-B-5** | `NumeroIntentos=5`, 4×`0003`+éxito | Ejecutado | **5** | **4** | No aplica | **PROCESADO — converge** | L248-266 |

### Aclaración sobre el identificador 3D

El identificador **3D** aparece en `evidencia_stub.txt` con el rótulo `[3D] COMPARACION DE LOS TRES CASOS` (líneas 107–114). **No corresponde a una ejecución adicional**, sino a un bloque de comparación que resume los resultados ya obtenidos en 1A, 3A, 3B y 3C:

```
[3D] COMPARACION DE LOS TRES CASOS
  1A (no procesada)          estado cliente: INDETERMINADO / 10002
  3A (procesada+timeout)     estado cliente: INDETERMINADO / 10002
  3B (procesada+timeout)     estado cliente: INDETERMINADO / 0
  3C (procesada+abort)       estado cliente: INDETERMINADO / 0
  >> Los tres son INDETERMINADO para el cliente.
  >> El doble sabe en los tres casos cual es el estado real.
  >> El sistema NO consulta el estado tras el fallo: no puede converger.
```

| Aspecto | Detalle |
|---|---|
| Solicitudes HTTP emitidas | **0** — es una síntesis, no una llamada |
| Operación lógica | Ninguna |
| Medición que aporta | Ninguna por sí mismo; **reproduce** la observación de 1A, 3A, 3B y 3C |
| Razón de su aparición en la matriz | Trazabilidad: el rótulo existe en la evidencia y sin esta fila parecería una ejecución omitida |
| Recuento de escenarios | **No cuenta** para I7. Las ejecuciones del escenario 3 son 3A, 3B y 3C |

El rótulo dice «los tres casos» y lista cuatro líneas porque incluye 1A como referencia del caso «no procesada», que pertenece al escenario 1 pero comparte la misma característica de estado indeterminado.

### Estados reales en el doble

| ID | Estado real | Peticiones | `efectosGenerarQr` | `anulaciones` |
|---|---|---|---|---|
| 1A | `NoProcesada` | 1 | **0** | 0 |
| 1B | `NoProcesada` | 2 | **0** | 0 |
| 2 | **`Generada`** | 2 | **1** | 0 |
| 3A | **`Generada`** | 1 | **1** | 0 |
| 3B | **`Generada`** | 2 | **1** | 0 |
| 3C | **`Generada`** | 2 | **1** | 0 |
| 4 | `Generada` | 3 | **1** | 0 |
| 5B | `Anulada` | 2 | 1 | **1** |
| 6 | `Anulada` | 3 | 1 | **1** |
| EC1-A-1 | `NoProcesada` | 2 | 0 | 0 |
| EC1-B-1 | `NoProcesada` | 2 | 0 | 0 |
| EC1-A-5 | `NoProcesada` | 6 | 0 | 0 |
| EC1-B-5 | **`Generada`** | 5 | **1** | 0 |

---

# D. Resultados de I3, I6, I7

## D.1 I3 — Comportamiento y tasa de reintentos

### Configuración efectiva

| Proyecto | Clave | Declarado | Configurado | **Usado** |
|---|---|---|---|---|
| WebServicePagoSimple | `NumeroIntentos` | 1 | 1 | **1** (producción) |
| WebServicePagoSimple | `NumeroIntentos` | 5 | 5 | **5** (experimental, solo banco) |
| WebServicePagoSimple | `MilisegundosDelay` | 1000 | 1000 | 1000 |
| WebServicePasarelaPagos | `RepetirIntento` | true | true | **true** |
| WebServicePasarelaPagos | `MilisegundosDelay` | 1000 | 1000 | 1000 |
| WebServicePasarelaPagos | `NumeroIntentos` | — | **ausente** | **no aplica** |

**Discrepancia:** `WebServicePasarelaPagos` no tiene `NumeroIntentos` en ningún `.config`. Su comportamiento depende de un booleano, lo que impide modular el número de intentos.

### Motivo de cada reintento

| Escenario | Réplica | Motivo |
|---|---|---|
| 1B | B | Excepción de transporte; `RepetirIntento=true` |
| 2 | B | Timeout |
| 3B | B | Timeout |
| 3C | B | Conexión abortada |
| 1A, 3A | A | **Ninguno**: la excepción no satisface la condición del `while` |
| EC1-A-1, EC1-B-1 | A | `ResultCode=0003` recibido |
| EC1-A-5, EC1-B-5 | A | `ResultCode=0003` recibido (5 veces) |

### Denominación de las tasas

Los porcentajes que siguen se denominan **«tasa observada en los escenarios experimentales seleccionados»**. No son tasas representativas del sistema, por tres razones:

1. Los denominadores son de **2 a 4 operaciones sintéticas**.
2. Los escenarios fueron **elegidos por el investigador** para provocar fallos específicos.
3. **La Réplica A tiene dos rutas de reintento mutuamente excluyentes**, agrupadas aquí por separado.

**El 0 % de la Réplica A no significa que nunca reintente.** Significa que, en las **dos operaciones de fallo de transporte** seleccionadas para ese cálculo, no reintentó. La misma réplica **sí reintenta** ante errores de negocio `0003`/`0004`, como se reporta en EC1.

### Cálculo A-1 — Réplica A, fallos de TRANSPORTE, línea base (`NumeroIntentos = 1`)

Operaciones incluidas: **1A y 3A** — fallos donde la llamada lanza excepción antes de recibir respuesta.

```
Operaciones lógicas ejecutadas:        2
Operaciones con al menos 1 reintento:  0
Reintentos adicionales totales:        0

Tasa observada = (0 / 2) × 100 = 0 %
Promedio       = 0 / 2 = 0
```

**Alcance de este 0 %:** limitado a fallos de transporte. La ruta de reintento por error de negocio no se incluye aquí.

### Cálculo A-2 — Réplica A, errores de NEGOCIO `0003`, línea base (`NumeroIntentos = 1`)

Operaciones incluidas: **EC1-A-1 y EC1-B-1** — BUSA *responde* con `ResultCode = 0003`, que sí satisface la condición del `while`.

```
Operaciones lógicas ejecutadas:        2
Operaciones con al menos 1 reintento:  2
Reintentos adicionales totales:        2   (1 + 1)

Tasa observada = (2 / 2) × 100 = 100 %
Promedio       = 2 / 2 = 1,0
```

### Cálculo A-3 — Réplica A, errores de negocio, experimental (`NumeroIntentos = 5`)

Operaciones incluidas: **EC1-A-5 y EC1-B-5** — misma condición que A-2, con el parámetro alterado.

```
Operaciones lógicas ejecutadas:        2
Operaciones con al menos 1 reintento:  2
Reintentos adicionales totales:        9   (5 + 4)

Tasa observada = (2 / 2) × 100 = 100 %
Promedio       = 9 / 2 = 4,5
```

### Cálculo B — Réplica B, fallos de transporte

Operaciones incluidas: **1B, 2, 3B y 3C**. Las cuatro son fallos provocados por el investigador:

| ID | Fallo inyectado | ¿Efecto aplicado en el doble? | Reintentos |
|---|---|---|---|
| 1B | `502 Bad Gateway` previo al efecto | No | 1 |
| 2 | Timeout con retención de 30 s | **Sí** | 1 |
| 3B | Timeout con retención de 30 s | **Sí** | 1 |
| 3C | Conexión abortada (`HttpContext.Abort`) | **Sí** | 1 |

La Réplica B **no separa por tipo de fallo**: reintenta ante cualquier excepción, por lo que las cuatro operaciones se comportan de la misma manera.

```
Operaciones lógicas evaluadas:          4
Operaciones con al menos 1 reintento:  4
Reintentos adicionales totales:        4   (1 + 1 + 1 + 1)

Tasa observada = (4 / 4) × 100 = 100 %
Promedio       = 4 / 4 = 1,0
```

**Alcance de este 100 %:** limitado a los cuatro escenarios de fallo de transporte ejecutados. La Réplica B no se evaluó ante errores de negocio `0003`/`0004`, que son los que su política de reintento no contempla de forma diferenciada.

### Exclusión del escenario 4 del cálculo de I3

El **escenario 4** (solicitud duplicada) queda **excluido** del cálculo de I3 por dos razones:

| Razón | Detalle |
|---|---|
| **No provoca un fallo** | Es una prueba de deduplicación: tres ejecuciones deliberadamente exitosas de la misma operación lógica |
| **Mide otro comportamiento** | Evalúa la acumulación de solicitudes ante repetición voluntaria, no la respuesta del sistema ante un fallo |

Sus tres solicitudes no son reintentos: son tres invocaciones deliberadas del cliente. Incluirlo en el denominador habría hecho depender la tasa de I3 de un escenario que no ejercita la lógica de reintento.

Con la exclusión del escenario 4, la tasa observada de la Réplica B pasa de 80 % a **100 %**, y el promedio de 0,8 a **1,0**. La conclusión de fondo no cambia: **la Réplica B reintenta ante todos los fallos de transporte probados.**

### Resumen de tasas observadas por ruta

| Réplica | Ruta | Operaciones | Tasa observada | Promedio |
|---|---|---|---|---|
| A | Fallo de **transporte** | 2 | **0 %** | 0 |
| A | Error de **negocio `0003`** (`NumeroIntentos=1`) | 2 | **100 %** | 1,0 |
| A | Error de **negocio `0003`** (`NumeroIntentos=5`) | 2 | **100 %** | 4,5 |
| B | Fallo de **transporte** | 4 | **100 %** | 1,0 |

**Lectura correcta:** ante fallos de transporte, la Réplica A **no reintenta** y la Réplica B **reintenta siempre**. Ante errores de negocio `0003`/`0004`, la Réplica A **sí reintenta**. La Réplica B no se evaluó contra esa segunda vía.

### EC1 - Efecto de `NumeroIntentos` sobre la convergencia

| Condición | `NumeroIntentos=1` | `NumeroIntentos=5` |
|---|---|---|
| BUSA devuelve `0003` siempre | Falla tras 2 solicitudes | Falla tras 6 solicitudes |
| BUSA falla 4 veces y luego responde bien | **Falla** (`10003`) | **Converge** (`PROCESADO`) |

**Hallazgo:** con `NumeroIntentos=1` el sistema **no puede converger** ante errores transitorios de BUSA que se resuelven en el segundo o tercer intento, aunque BUSA ya esté sano. El valor 1 limita la capacidad de recuperación ante fallos que no son permanentes.

**Límite del experimento:** aumentar el número de intentos **no resuelve** la incertidumbre de los escenarios 3A–3C (respuesta perdida), porque esos fallos son de transporte y el bucle nunca se alcanza. Con 5 intentos, RpcA siguió emitiendo **1 sola solicitud** y reportando `10002`.

### Limitaciones de I3

| Limitación | Detalle |
|---|---|
| **Denominadores mínimos** | 2 a 4 operaciones sintéticas por ruta. Caracterizan ramas de código; **no estiman volumen ni riesgo** |
| **Muestra no representativa** | Escenarios elegidos por el investigador para provocar fallos concretos, no extraídos de la distribución real de fallos |
| **Objeto medido** | Algoritmo réplica, **no** el binario desplegado |
| **Sin datos de producción** | El código actual no instrumenta el número de intento; I3 no es medible en producción sin analizar logs de texto |
| **Valor 5 sin validar** | No se probó contra BUSA real |
| **Sin control de concurrencia** | Los escenarios son secuenciales. No se midió el comportamiento bajo carga ni con reintentos simultáneos |

**Consecuencia para la interpretación:** las tasas observadas describen el comportamiento de las ramas de código bajo condiciones fabricadas. No deben citarse como métricas del sistema en producción.

## D.2 I6 — Efectos duplicados en la repetición de una anulación

### Delimitación del alcance: qué operación se evaluó

> Ver A.5 para la demostración de que el sistema no implementa compensación en el sentido de Saga.

**El sistema actual no implementa una compensación idempotente en el sentido de Saga.** No existe mecanismo de compensación, ni registro de compensaciones, ni verificación de que una compensación previa se haya completado.

Lo que el sistema ofrece es la **anulación de una solicitud QR no pagada** (`UNIVIDA_*_QR_003`), con un Windows Service que la ejecuta en lote.

Para evaluar I6 se evaluó **la repetición de una anulación como operación funcional equivalente**, bajo el supuesto de que:

| Operaciones | ¿Repetible? | Incluida en I6 |
|---|---|---|
| Anulación de QR **no pagado** | Sí — es la única repeatable del sistema | **Sí** |
| Anulación de QR **ya pagado** | Rechazada con `ResultCode 0009` | **No** — fuera del alcance |
| **Reversión de un pago ejecutado** | **No existe** en BUSA | **No** — fuera del alcance funcional |

**Esta delimitación es una limitación del indicador en el sistema actual**, no una decisión metodológica. I6 solo puede medirse sobre la repetición de la anulación; para una compensación de Saga real sería necesario un prototipo o un componente que no existe hoy.

### Definición del efecto observable

| Contador | Significado |
|---|---|
| `efectosGenerarQr` | Veces que una operación pasó de `NoProcesada` a `Generada` |
| `anulacionesAplicadas` | Veces que una operación pasó de `Generada` a `Anulada` |

Un reenvío que no cambia estado **no** incrementa contadores.

### Medición A — repetición de la ANULACIÓN (escenario 6)

Esta es la medición que corresponde a I6: la repetición de la anulación.

| Momento | Inyección | Peticiones de anulación | `anulacionesAplicadas` |
|---|---|---|---|
| Estado inicial | — | 0 | 0 |
| 1ª invocación | `FallaEnCompensacion` (efecto aplicado, respuesta abortada) | 1 | **1** |
| 2ª invocación | Ninguna (reintento explícito del cliente) | 2 | **1** |

Bitácora del doble:
```
EFFECT_CANCEL #1
NO_EFFECT_ALREADY_CANCELLED
```

**Denominador de I6:** **2 invocaciones de anulación sobre una misma operación lógica.**

| Aspecto | Valor |
|---|---|
| Invocaciones de anulación | 2 |
| Operaciones lógicas distintas | 1 (`SINT-6\|E6`) |
| Efectos aplicados por el doble | 1 |
| Efectos adicionales registrados | **0** |

No se mezclan en este denominador los reintentos de generación: son una operación distinta con un contador distinto (`efectosGenerarQr`, no `anulacionesAplicadas`).

### Medición B — repetición de la GENERACIÓN (escenarios 5A y EC1-B-5)

Se reporta por separado porque mide otro contador. **No forma parte del denominador de I6.**

| Prueba | Invocaciones de generación | `efectosGenerarQr` | Efectos adicionales |
|---|---|---|---|
| Escenario 5A | 2 (segunda sobre operación ya exitosa) | 1 | **0** |
| EC1-B-5 | 5 (`NumeroIntentos=5`, 4×`0003` + éxito) | 1 | **0** |

Ambas observations coinciden en que el doble no registró efectos adicionales, pero responden a preguntas distintas: la primera sobre repetición voluntaria, la segunda sobre reintentos automáticos tras error de negocio.

### Advertencia: efecto aplicado ≠ resultado confirmado

El escenario 6 requiere una distinción explícita para no sobreinterpretar el resultado:

| Afirmación | Sostenida por la evidencia | **No** sostenida |
|---|---|---|
| El doble aplicó el efecto de anulación | **Sí** — `anulacionesAplicadas=1` | — |
| El doble no aplicó un segundo efecto observable | **Sí** — el contador quedó en 1 tras el reintento | — |
| El cliente confirmó el estado externo | — | **No.** El cliente nunca consultó; su estado permanecióo en `INDETERMINADO` tras el primer fallo |
| BUSA real garantiza el mismo comportamiento | — | **No.** Es propiedad del doble, no verificada en el servicio real |

Tras el primer fallo, el cliente quedó en `INDETERMINADO`. El contador del doble bajó a 1 **no porque el cliente lo verificara**, sino porque el doble es idempotente por construcción. Esta distinción es exactamente el problema que el método propuesto debe resolver.

### Distinción entre operación pagada y no pagada

| Caso | ResultCode | Estado | Interpretación |
|---|---|---|---|
| No pagada | `0000` | `Anulada` | Anulación legítima, **única dentro del alcance de I6** |
| **Pagada** | `0009` | `Pagada` | Rechazada — **fuera del alcance** |

La anulación de un QR pagado y la reversión de un pago ejecutado quedan **excluidas de I6** conforme al límite funcional declarado.

### Resultado — redacción corregida

En los escenarios reproducidos, el doble local no registró efectos adicionales en los contadores observables de generación y anulación. Este resultado caracteriza el comportamiento del doble y no permite concluir que el sistema desplegado ni BUSA real sean idempotentes.

### Limitaciones de I6

| Limitación | Detalle |
|---|---|
| **Objeto medido** | El doble local, diseñado por el investigador. No es BUSA ni el sistema desplegado |
| **Naturaleza del diseño** | El doble fue construido con idempotencia por referencia desde el inicio, precisamente porque el método propuesto la requiere. Es una asunción, no un hallazgo |
| **Alcance reducido** | Solo se evaluó la repetición de la anulación de QR no pagado. No hay compensación de Saga en el sistema |
| **Efectos no observables** | El doble instrumenta únicamente generación y anulación. Otros efectos de negocio externos no se midieron |
| **Sin datos de producción** | No se observaron duplicados reales en BUSA por falta de acceso |

**Requisito para evaluar I6 sobre el sistema real:** contrato formal de idempotencia de BUSA, doble que reproduzca su implementación real, o entorno de pruebas autorizado.

**No hay evidencia de que el sistema actual produzca efectos duplicados.** Tampoco de que no los produzca en producción.

## D.3 I7 — Cobertura de modos de fallo

| # | Escenario | Objetivo | Precondición | Inyección | Resultado observado | Estado |
|---|---|---|---|---|---|---|
| 1 | Fallo en transacción | Verificar interrupción y estado | Operación inexistente | 502 pre-efecto | RpcA: 0 reintentos / 10002. RpcB: 1 reintento / 0 | **Ejecutado** |
| 2 | Timeout | Verificar reintento y consulta de estado | Timeout 3 s | Retener 30 s post-efecto | RpcB reintenta; efecto aplicado; **no consulta estado** | **Ejecutado** |
| 3 | Respuesta perdida | Distinguir no procesada vs procesada | Doble distingue internamente | Timeout y abort post-efecto | **3 ejecuciones** (3A, 3B, 3C), todas `INDETERMINADO`, con estados reales distintos | **Ejecutado** |
| 4 | Solicitud duplicada | Verificar deduplicación | Mismo ID lógico | 3 ejecuciones | Doble deduplica; BD producción no garantiza | **Ejecutado** |
| 5 | Reintento post-éxito | Verificar efectos adicionales | Operación ya generada | Repetición | Efectos no crecen | **Ejecutado** |
| 6 | Fallo en la repetición de una anulación | Verificar registro y reintento | Operación `Generada` | Abort post-efecto | Excepción con efecto aplicado; reintento idempotente **en el doble** | **Ejecutado** |

### Cálculo

```
Escenarios de modo de fallo definidos en E6:      6
Escenarios de modo de fallo ejecutados:           6
Pruebas de sensibilidad de configuración (EC1):   1   ← NO es un modo de fallo

Cobertura de modos de fallo = (6 / 6) × 100 = 100 %
```

### Alcance del 100 % — advertencia de interpretación

Este porcentaje mide **la cobertura del conjunto de modos de fallo previamente definido por el investigador**, y nada más.

| Lo que el 100 % **sí** significa | Lo que el 100 % **no** significa |
|---|---|
| Los 6 modos de fallo definidos para E6 fueron reproducidos con evidencia verificable | Que se hayan cubierto el 100 % de los fallos posibles del sistema real |
| Cada uno fue ejecutado en el banco de pruebas con el doble HTTP | Que el binario desplegado presente el mismo comportamiento |
| La inyección de fallos fue controlada y reproducible | Que se hayan probado modos de fallo no previstos |

**El denominador es un artefacto del diseño de la investigación, no una propiedad del sistema.** Un conjunto de 8 modos habría dado 75 % sin que nada peor hubiera ocurrido en el sistema.

### Pruebas de sensibilidad de configuración — fuera de I7

La prueba **EC1** (`NumeroIntentos` = 1 vs 5) **no es un modo de fallo**. Mide cómo responde el algoritmo ante una modificación de un parámetro, usando modos de fallo de negocio ya existentes como variable de prueba.

| Prueba | Tipo | Condición | Configuración | Resultado | Pertenece a I7 |
|---|---|---|---|---|---|
| EC1-A-1 | Sensibilidad de parámetro | `0003` siempre | `NumeroIntentos=1` | Falla tras 2 solicitudes | **No** |
| EC1-B-1 | Sensibilidad de parámetro | 4×`0003` + éxito | `NumeroIntentos=1` | Falla `10003`, no converge | **No** |
| EC1-A-5 | Sensibilidad de parámetro | `0003` siempre | `NumeroIntentos=5` | Falla tras 6 solicitudes | **No** |
| EC1-B-5 | Sensibilidad de parámetro | 4×`0003` + éxito | `NumeroIntentos=5` | **Converge** | **No** |

### Detalle de la cobertura de modos de fallo

| Aspecto | Detalle |
|---|---|
| Escenario 2 | Ejecutado en su subcaso más exigente |
| Escenarios 1, 3 | Ejecutados en dos variantes (RpcA y RpcB) |
| Escenario 4 | Ejecutado con evidencia de producción adicional |
| Escenarios 5, 6 | Ejecutados con y sin distinción de estado de pago |

**Sobre la cobertura por réplica:** no se reporta un porcentaje por réplica. El conjunto de 6 modos de fallo se considera cubierto cuando el modo fue reproducido **al menos una vez** por alguna de las dos réplicas, independientemente de cuántas variantes se ejecuten. Los escenarios 1 y 3 se ejecutaron con ambas réplicas porque su comportamiento difiere, pero cuentan una sola vez cada uno. Reportar «RpcA: 3 de 6» y «RpcB: 5 de 6» exigiría una definición de cobertura que no está en el diseño de la actividad, y daría la falsa impresión de que cada réplica se evaluó de forma independiente y completa.

## D.4 Hallazgos principales

Ordenados por alcance. Cada uno indica su fuente de evidencia y si es un hallazgo **del sistema** o **del banco de pruebas**.

| # | Hallazgo | Tipo | Evidencia |
|---|---|---|---|
| H1 | **No puede distinguir «no procesada» de «procesada con respuesta perdida»** | Sistema | Escenarios 1 y 3: 1A + 3A/3B/3C |
| H2 | **Dos rutas de reintento divergentes en la Réplica A**, y política distinta en la Réplica B | Sistema | Escenarios 1, 2, 3 y EC1 |
| H3 | **La base de producción no garantiza unicidad** de la llave lógica | Sistema | `T_QR_SOLICITUD` |
| H4 | **Existe el mecanismo de convergencia pero no se invoca en el camino de error** | Sistema | `WinServiceSoatConsultaPagosQR` |
| H5 | **`NumeroIntentos=1` limita la recuperación ante errores de negocio transitorios** | Banco de pruebas | EC1-B-1 vs EC1-B-5 |
| H6 | **Aumentar el número de intentos no resuelve la incertidumbre de respuesta perdida** | Banco de pruebas | EC1-A-5: 1 sola solicitud |

### H1 — Incapacidad de distinguir dos situaciones distintas

| Caso | Pertenece a | Estado cliente | Estado real en el doble |
|---|---|---|---|
| 1A — fallo previo al procesamiento | Escenario 1 | `INDETERMINADO` / 10002 | `NoProcesada` |
| 3A — respuesta perdida por timeout (RpcA) | Escenario 3 | `INDETERMINADO` / 10002 | **`Generada`** |
| 3B — respuesta perdida por timeout (RpcB) | Escenario 3 | `INDETERMINADO` / 0 | **`Generada`** |
| 3C — respuesta perdida por abort (RpcB) | Escenario 3 | `INDETERMINADO` / 0 | **`Generada`** |

**La comparación considera cuatro casos: uno de fallo previo al procesamiento (1A) y tres de respuesta perdida tras el procesamiento (3A–3C). Los cuatro resultan indeterminados para el cliente**, pese a que el estado real difiere entre el primero y los otros tres.

El caso 1A se incluye como referencia porque comparte la característica de estado indeterminado; su origen es el escenario 1, no el escenario 3.

Existe el servicio `ConsultarQR`, pero **el camino de error no lo invoca**.

### H2 — Divergencia de políticas de reintento

| Componente | Ruta | Comportamiento observado | Tasa observada |
|---|---|---|---|
| Réplica A | Fallo de **transporte** | **No reintenta.** La excepción se retorna como 10002 antes de alcanzar el `while` | **0 %** |
| Réplica A | Error de **negocio `0003`/`0004`** | Reintenta hasta `NumeroIntentos` | **100 %** |
| Réplica B | Cualquier excepción de transporte | Reintenta **una vez**, sin distinguir el tipo de fallo | **100 %** |

Una misma condición de fallo produce comportamientos opuestos según la ruta de entrada al sistema.

### H3 — La base de producción no garantiza unicidad

| Métrica | Valor |
|---|---|
| Filas en `T_QR_SOLICITUD` | 892 970 |
| Pares `(TRAMITE_SECUENCIAL, T_PAR_SIMPLE_TRAMITE_FK)` distintos | 892 926 |
| **Pares duplicados** | **44** |
| Índice único sobre la llave lógica | **No existe** |

Esta condición es **independiente de la lógica de reintento**: aunque un reintento fuera perfectamente idempotente en la capa de servicio, la base de datos seguiría admitiendo filas duplicadas para la misma llave.

### H4 — El mecanismo de convergencia existe pero no se usa

| Aspecto | Detalle |
|---|---|
| Componente | `WinServiceSoatConsultaPagosQR` |
| Intervalo de consulta | 10 s (`ConsultaIntervaloSegundos`) |
| Invocación | `CNPagosQR.Consultar()` → `ws.ConsultarQR()` → BUSA `_QR_002` |
| **Uso desde el camino de error** | **Ninguno.** Las réplicas A y B no lo invocan tras una excepción |

El sistema dispone de la infraestructura necesaria para converger estados por consulta, pero el flujo de reintento no la utiliza.

### H5 — Efecto de `NumeroIntentos` sobre la convergencia

| Condición | `NumeroIntentos=1` | `NumeroIntentos=5` |
|---|---|---|
| BUSA devuelve `0003` siempre | Falla tras 2 solicitudes | Falla tras 6 solicitudes |
| BUSA falla 4 veces y luego responde bien | **Falla** (`10003`) | **Converge** (`PROCESADO`) |

Con un solo intento, el sistema **no puede recuperarse** de errores transitorios de BUSA que se resuelven en el segundo o tercer intento, aunque el servicio ya esté sano.

### H6 — El límite del parámetro

Con 5 intentos, los escenarios de respuesta perdida (3A–3C) siguieron emitiendo **1 sola solicitud** y reportando `10002`. Los fallos de transporte **quedan fuera del alcance del bucle**, por lo que el parámetro no los afecta.

---


# E. Conclusión de línea base

## E.1 Comportamientos demostrados

| # | Comportamiento | Evidencia |
|---|---|---|
| 1 | **No puede distinguir "no procesada" de "procesada con respuesta perdida"** | Escenarios 1 y 3: 1A + 3A/3B/3C |
| 2 | **Dos rutas de reintento divergentes en la Réplica A.** Transporte: 0 % observado. Negocio `0003`: 100 % observado | Escenarios 1, 3 y EC1 |
| 3 | **La Réplica B reintenta ante cualquier excepción**, sin separar por tipo | Escenarios 1, 2, 3 |
| 4 | **`NumeroIntentos` no existe** en `WebServicePasarelaPagos` | Inspección de `.config` |
| 5 | **No hay timeout de transporte configurado** | `Web.config` |
| 6 | **El doble no aplicó un segundo efecto observable** en la repetición de anulación. No demuestra idempotencia de BUSA ni que el cliente confirmara el estado | Escenario 6A/6B |
| 7 | **Reversión de pagos no existe** — límite funcional. La anulación de QR no pagado **no equivale a una compensación Saga** | Escenario 5B |
| 8 | **La BD no garantiza unicidad**: 44 pares duplicados, sin índice único | `T_QR_SOLICITUD` |
| 9 | **Existe endpoint de consulta que no se invoca tras fallo** | `WinServiceSoatConsultaPagosQR` |
| 10 | **`NumeroIntentos=1` impide converger ante fallos transitorios de BUSA** | EC1-B-1 vs EC1-B-5 |
| 11 | **Aumentar intentos no resuelve la incertidumbre de respuesta perdida** | EC1-A-5: 1 sola solicitud |

**Alcance de la columna «Evidencia»:** los escenarios 1 a 11 se observaron en el banco de pruebas sobre la réplica del algoritmo y el doble HTTP. Ninguno se observó en el binario desplegado ni contra BUSA real.

## E.2 Indicadores con resultado cuantificable

| Indicador | Ruta o condición | Estado | Valor | Denominador |
|---|---|---|---|---|
| **I3** | RpcA, fallo de **transporte** | Cuantificado sobre réplica | **0 %** observado | 2 operaciones |
| **I3** | RpcA, error de negocio `0003`, `NumeroIntentos=1` | Cuantificado sobre réplica | **100 %** observado | 2 operaciones |
| **I3** | RpcA, error de negocio `0003`, `NumeroIntentos=5` | Cuantificado sobre réplica | **100 %** observado, prom. **4,5** | 2 operaciones |
| **I3** | RpcB, fallos de transporte | Cuantificado sobre réplica | **100 %** observado | 4 operaciones |
| **I6** | Repetición de la anulación | Cuantificado **solo sobre el doble** | **Ningún efecto adicional en los contadores observables** | **2 invocaciones de anulación** sobre 1 operación lógica (escenario 6) |
| **I7** | Modos de fallo definidos en E6 | Completa sobre el conjunto definido | **6 / 6 = 100 %** | 6 modos |

**Notas de alcance de los indicadores:**

| Nota | Detalle |
|---|---|
| Denominación de I3 | Las tasas se denominan **«observadas en los escenarios experimentales seleccionados»**. No son tasas representativas del sistema ni permiten estimar volumen o riesgo |
| I3 en producción | **No medido.** El código actual no instrumenta el número de intento |
| Objeto de I6 | El doble local, diseñado con idempotencia por referencia. **No permite concluir** que el sistema desplegado ni BUSA real sean idempotentes |
| Denominador de I7 | El conjunto de 6 modos de fallo fue **definido por el investigador**. El 100 % mide cobertura de ese conjunto, no de los fallos posibles del sistema real |
| Denominador de I6 | **2 invocaciones de anulación** sobre 1 operación lógica (escenario 6). Los reintentos de generación de 5A y EC1-B-5 se reportan por separado y **no entran en el denominador** |
| EC1 | Prueba de sensibilidad de parámetro. **Fuera de I7** por no ser un modo de fallo |

## E.3 Lo que NO pudo verificarse

| Aspecto | Motivo | Requisito |
|---|---|---|
| Comportamiento del binario desplegado | No se ejecutó producción | Entorno autorizado |
| Volumen real de reintentos | Sin instrumentación | Contador de intentos |
| Idempotencia de BUSA real | Doble no reproduce implementación real | Contrato formal |
| Validación de firma digital | El doble no firma | `wsFirmaDigital` real |
| Duplicados reales en BUSA | Sin acceso | Autorización |
| `NumeroIntentos=5` en producción | **No se aplicó** | Aprobación y despliegue |
| Reversión de pagos | **Fuera del alcance funcional** | — |

## E.4 Declaraciones finales

### 4.1 Declaraciones sobre el alcance de la evidencia

| # | Declaración | Fundamento |
|---|---|---|
| 1 | **No se ejecutó ninguna operación contra BUSA real.** Se verificó que `172.30.140.139:443` es alcanzable y deliberadamente no se contactó | Sección 0 |
| 2 | **No se modificó ningún archivo de producción.** `Web.config` conserva `NumeroIntentos = 1` | Verificación posterior a la ejecución |
| 3 | **No se realizó ninguna escritura en producción.** El acceso a base de datos fue exclusivamente `SELECT` | Sección A.8 |
| 4 | **Los datos utilizados son sintéticos** (`SINT-*`, placas y credenciales ficticias). No se emplearon credenciales, tokens ni datos personales reales | Sección A.8 |
| 5 | **Ninguna prueba demuestra el comportamiento del binario desplegado.** Las tres fuentes de evidencia —inspección estática, réplica del algoritmo y doble HTTP— tienen alcances distintos y ninguno cubre el despliegue | Sección 0 |

### 4.2 Declaraciones sobre los indicadores

| # | Declaración |
|---|---|
| 6 | **I3 no es medible en producción con el código actual**, porque no se instrumenta el número de intento. Las tasas reportadas son «observadas en los escenarios experimentales seleccionados», con denominadores de 2 a 4 operaciones, y no estiman volumen ni riesgo |
| 7 | **La Réplica A tiene dos rutas de reintento distintas.** El 0 % corresponde exclusivamente a fallos de transporte. La misma réplica presenta 100 % en errores de negocio `0003`/`0004`. Reportar un único valor para esta réplica sería incorrecto |
| 8 | **En los escenarios reproducidos, el doble local no registró efectos adicionales en los contadores observables de generación y anulación.** Este resultado caracteriza el comportamiento del doble y **no permite concluir** que el sistema desplegado ni BUSA real sean idempotentes |
| 9 | **El sistema actual no implementa compensación en el sentido de Saga.** Lo evaluado en I6 fue la repetición de la anulación de un QR no pagado, como operación funcional equivalente para el experimento. La anulación de un QR pagado y la reversión de un pago ejecutado quedan **fuera del alcance** |
| 10 | **El 100 % de I7 mide la cobertura del conjunto de 6 modos de fallo definido por el investigador**, no la cobertura de los fallos posibles del sistema real. Un conjunto más amplio habría producido un porcentaje menor sin que el sistema hubiera empeorado |

### 4.3 Declaraciones sobre el experimento de sensibilidad

| # | Declaración |
|---|---|
| 11 | **`NumeroIntentos = 5` es un resultado experimental, no una recomendación de despliegue.** Se aplicó en `e6-testbed\test-config\Web.config`; la configuración de producción permanece intacta |
| 12 | **El beneficio del valor 5 se limita a errores de negocio `0003`/`0004`.** Con 5 intentos, los escenarios de respuesta perdida siguieron emitiendo **1 sola solicitud**: aumentar intentos **no resuelve** la incertidumbre de estado |
| 13 | **El valor 5 no se validó contra BUSA real.** Su efecto real depende de la disponibilidad y de los tiempos de recuperación del servicio |

### 4.4 Declaraciones sobre los hallazgos

| # | Declaración |
|---|---|
| 14 | **La divergencia entre las réplicas A y B es el hallazgo de mayor alcance.** Dos componentes activos con políticas de reintento opuestas ante la misma condición de fallo. Cualquier método de compensación idempotente debe resolver esta inconsistencia antes de aplicarse |
| 15 | **El sistema dispone del mecanismo de convergencia** (`WinServiceSoatConsultaPagosQR`, intervalo de 10 s) **pero el camino de error no lo invoca.** El método propuesto podría apoyarse en esta infraestructura existente en lugar de introducirla |
| 16 | **La base de datos no garantiza la unicidad de la llave lógica:** 44 pares duplicados y ningún índice único sobre `(TRAMITE_SECUENCIAL, T_PAR_SIMPLE_TRAMITE_FK)`. Esto es una condición independiente de la lógica de reintento y debe considerarse en el diseño del método |

### 4.5 Distinción entre sistema actual, doble y método propuesto

Esta sección separa lo que E6 **caracterizó** de lo que las **etapas siguientes deberán construir y validar**. La confusión entre estos tres planos sería un error metodológico en la tesis.

| Plano | Qué se afirma con base en E6 | Qué **no** se afirma |
|---|---|---|
| **Sistema actual** (código de producción inspeccionado) | Tiene rutas de reintento divergentes entre RpcA y RpcB; existe un mecanismo de consulta de estado que **no se invoca desde los caminos de error inspeccionados**; no implementa compensación en el sentido de Saga; la BD no garantiza unicidad de la llave lógica | Nada sobre su comportamiento en ejecución, volumen de fallos o tasa real de reintentos |
| **Doble HTTP** (banco de pruebas) | En los escenarios ejecutados **no produjo efectos adicionales** en los contadores observables de generación y anulación | No es BUSA, no es el sistema desplegado y **no acredita idempotencia de ningún servicio real** |
| **Método propuesto** (E7 en adelante) | Nada todavía — es el objeto de construcción | Debe diseñar y verificar **identidad de la operación**, **idempotencia de la compensación** y **tratamiento de estados indeterminados** |
| **Validación posterior** (evaluación del método) | Nada todavía | Deberá **demostrar el comportamiento del método** mediante escenarios de fallo propios. No basta con repetir los resultados del doble actual, porque aquel doble fue construido con la idempotencia como asunción de diseño |

### 4.6 Advertencia sobre el alcance funcional evaluado

**La anulación de un QR no pagado no equivale a una compensación Saga.**

En E6 funcionó como **operación repetible para caracterizar la línea base**, y es la única acción del sistema actual sobre la que I6 puede medirse. Esto no habilita la conclusión de que el sistema implemente compensaciones idempotentes.

| Afirmación válida | Afirmación **no** válida |
|---|---|
| La anulación es la única operación repetible disponible en el sistema actual | El sistema implementa compensación idempotente |
| El doble no aplicó un segundo efecto observable al repetir la anulación | BUSA garantiza idempotencia en la anulación |
| El cliente quedó en estado indeterminado tras el primer fallo | El cliente verificó el estado externo |

**La idempotencia de compensaciones debe verificarse en la evaluación del método propuesto**, sobre una compensación real, no sobre la anulación.

---

## Anexo — Resumen de métricas

| Métrica | Valor |
|---|---|
| Modos de fallo definidos en E6 | 6 |
| Modos de fallo ejecutados | **6 / 6** |
| Cobertura de I7 sobre el conjunto definido | 100 % |
| Pruebas de sensibilidad de configuración | 1 (EC1, 4 pruebas, **fuera del conjunto E6**) |
| Pruebas individuales ejecutadas | 16 (12 en modos de fallo + 4 en EC1) |
| Operaciones lógicas sintéticas distintas | 14 |
| Solicitudes HTTP totales emitidas | 34 |


---

