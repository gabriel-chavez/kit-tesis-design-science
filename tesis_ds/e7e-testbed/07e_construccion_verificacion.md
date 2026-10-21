# E7e. Construcción y verificación del artefacto

**Actividad:** E7e — Construcción y verificación del método
**Investigación:** «Método para el diseño y la verificación de compensaciones idempotentes en transacciones distribuidas basadas en Sagas: un estudio de Design Science»
**Fecha real de ejecución:** 2026-10-13
**Fecha de referencia de la sesión:** 2026-10-13 — coincide; no se ajustó

> **Tipología del artefacto: MÉTODO.** El software es vehículo, no contribución. La
> contribución son los principios de diseño que los nueve componentes formalizan. La prueba de
> aplicabilidad es la existencia de al menos una exemplar aplicada: la cadena de pagos QR sobre
> el servicio externo BUSA.

---

## Índice

1. [Resumen ejecutivo y estado de la etapa](#1-resumen-ejecutivo-y-estado-de-la-etapa)
2. [Construcción](#2-construcción)
3. [Verificación V1–V8](#3-verificación-v1v8)
4. [Registro de cambios](#4-registro-de-cambios)
5. [Revisión por expertos](#5-revisión-por-expertos)
6. [Limitaciones](#6-limitaciones)
7. [Reproducción](#7-reproducción)

---

# 1. Resumen ejecutivo y estado de la etapa

## 1.1 Qué se construyó

El artefacto es una librería de nueve componentes, escrita en el repositorio real de la
organización y compilada para `netstandard2.0` para que pueda referenciarse tanto desde los
proyectos `net462` del sistema como desde el arnés de verificación.

| Aspecto | Detalle |
|---|---|
| Ubicación | `C:\Users\lchavez\Source\Workspaces\UNIVidaNetPasarelaPagos\UNIVidaNetPasarelaPagos\E7e_MetodoCompensacion\` |
| Target | `netstandard2.0`, C# 7.3 |
| Dependencias | **Ninguna** fuera de la BCL |
| Archivos | `Modelo.cs`, `Puertos/Puertos.cs`, `K1_K2_K9.cs`, `K3_K4.cs`, `K5_K6.cs`, `K7_K8.cs`, `CoordinadorMetodo.cs`, `Sql/01_tabla_idempotencia_compensaciones.sql` |
| Solución real | **No se modificó el archivo `.sln`.** Ver 6.2 |
| Archivos de producción modificados | **0** |

## 1.2 Qué se ejecutó y qué no — leer antes de citar cualquier resultado

Esta sección está al principio del documento porque **la distinción cambia qué significan los
números**.

| Componente | ¿Se ejecutó? | Qué es realmente |
|---|---|---|
| Artefacto `E7e_MetodoCompensacion` (K1–K9) | **Sí** | `CoordinadorMetodo.Procesar` se ejecutó de verdad |
| Doble neutro `busa-eval-stub` | **Sí** | Servicio HTTP real; los fallos de transporte son cortes de socket y timeouts genuinos |
| `CNPagosQR` — salto interno, RpcB | **No** | Sustituido por `InyectorSaltoInterno`, un inyector de fallos dentro del arnés |
| `CPagoSimple` — salto externo, RpcA | **No** | Sustituido por `ServicioExternoHttp`, un cliente HTTP directo al doble |
| `WebServicePasarelaPagos`, `WinServiceSoatConsultaPagosQR` | **No** | Nunca compilados |
| `PasarelaPagos.sln` completa | **No** | `net462` con frameworks UVNet no disponibles en esta estación |

### La cadena que realmente se ejercitó

```
            ARTEFACTO REAL (ejecutado)
  CoordinadorMetodo  ·  K1..K9
            |
            +-- salto interno: InyectorSaltoInterno      <-- NO es CNPagosQR
            |
            +-- salto externo: ServicioExternoHttp
                        |
                        v
                  busa-eval-stub                       <-- doble neutro real
```

### Qué sostienen estos resultados

| Afirmación | Estado |
|---|---|
| Los componentes K1–K9 implementan la idempotencia que declaran | **Demostrado** |
| La garantía proviene de K5 y no del simulador | **Demostrado** por el control negativo |
| El método no depende del número de intentos | **Demostrado** |
| Los seis modos de fallo producen el resultado que K4 decide | **Demostrado** |

### Qué NO sostienen

| Afirmación | Por qué no |
|---|---|
| El método funciona dentro de la cadena real | Los dos saltos son adaptadores |
| `CNPagosQR` y `CPagoSimple` propagan la llave de K1 | No se ejecutaron |
| `T_QR_SOLICITUD` tiene la forma que K2 asume | K2 corrió sobre una implementación en memoria |
| El comportamiento con WCF y `basicHttpBinding` | No se probó; el doble es HTTP simple |
| El índice único de Dc10 | El DDL no se ejecutó |

**En una frase:** la evidencia verifica **el método**, no **su integración**. La integración
queda pendiente de un entorno donde la solución real compile.
## 1.3 Decisión de diseño que ordena todo

El artefacto se define contra **puertos**, no contra una tecnología:

```csharp
public interface IServicioExterno { ... }
public interface IBitacora       { ... }
public interface IReloj          { DateTimeOffset Ahora { get; } }
public interface ITelemetria     { void Registrar(EventoTelemetria e); }
```

Esto tiene dos consecuencias que sostienen la validez de la evidencia:

1. **El mismo código** se aplica al sistema real (BUSA por WCF, bitácora en SQL Server) y a la
   evaluación (doble neutro, bitácora en memoria). No hay dos implementaciones del método.
2. **La garantía de idempotencia no depende de la tecnología del externo**, que es lo que
   Dc13 exige del método: es transportable por construcción, no por afirmación.

El arnés de verificación referencia la librería por `ProjectReference`. No contiene ninguna
copia de los componentes K.

## 1.4 Estado de las ocho comprobaciones

| # | Comprobación | Crítica | Estado | Evidencia |
|---|---|---|---|---|
| V1 | K3 distingue procesado / no procesado / indeterminado | — | **CUMPLE** | `v1_resultados.md` |
| **V2** | Repetir la compensación deja el estado final idéntico (N = 1, 2, 5, 10) | **Sí** | **CUMPLE** | `v2_resultados.md` |
| **V3** | No se compensa dos veces una operación ya compensada | **Sí** | **CUMPLE** | `v3_resultados.md` |
| V4 | El `PENDIENTE` se respeta durante la ventana de 20 min | — | **CUMPLE** | `v3_resultados.md` |
| V5 | La reconciliación es idempotente (dos corridas) | — | **CUMPLE** | `v3_resultados.md` |
| V6 | La telemetría registra intento, causa y resultado | — | **CUMPLE** | `v2_resultados.md` |
| V7 | Sin espera bloqueante en el camino crítico | — | **CUMPLE** (inspección) | `v2_resultados.md` |
| V8 | Consistencia interna, sin huérfanos | — | **CUMPLE** con salvedades | `v8_trazabilidad.md` |

## 1.5 Estado del control negativo

**CONFIRMADO.** Con K5 desactivado, N invocaciones producen N efectos en el externo. Con K5
activado, siempre uno.

| N | Sin K5 | Con K5 |
|---|---|---|
| 1 | 1 | 1 |
| 2 | 2 | 1 |
| 5 | 5 | 1 |
| 10 | 10 | 1 |

## 1.6 Estado del criterio de cierre

| # | Criterio | Estado |
|---|---|---|
| 1 | Las tres versiones construidas y verificadas | **Cumplido** |
| 2 | V1–V8 con resultado y evidencia | **Cumplido** |
| 3 | Control negativo demuestra la neutralidad del doble | **Cumplido** |
| 4 | Walkthrough hecho por persona independiente | **NO CUMPLIDO** — guion preparado, sin practicante asignado |
| 5 | Registro de cambios con razones ligadas a hallazgos | **Cumplido** |
| 6 | Un tercero puede ir a cada comprobación y ver su evidencia | **Cumplido** |

**La etapa no está cerrada.** Falta el criterio 4, que no puede ser cumplido por quien
construyó el artefacto.

---

# 2. Construcción

## 2.1 Componentes

| # | Componente | Función | Requisito | Archivo |
|---|---|---|---|---|
| K1 | Generador de identidad | Deriva `(TRAMITE_NUMERO, GESTION)`, única en toda la cadena | F1 | `K1_K2_K9.cs` |
| K2 | Bitácora de estado | Estado de la operación, de cada salto y de la compensación | F2, F5 | `K1_K2_K9.cs` |
| K3 | Verificador de estado | Consulta el externo y normaliza | F3 | `K3_K4.cs` |
| K4 | Motor de decisión | Decide reprocesar / compensar / reconciliar | F4 | `K3_K4.cs` |
| K5 | Compensador idempotente | Compensa por `(operación, motivo)` sin efecto repetido | F5 | `K5_K6.cs` |
| K6 | Ordenador | Orden inverso y dependencias entre saltos | F6 | `K5_K6.cs` |
| K7 | Reconciliador | Resuelve operaciones indeterminadas, de forma idempotente | F7 | `K7_K8.cs` |
| K8 | Verificador | Aserciones sobre observables externos | F8 | `K7_K8.cs` |
| K9 | Telemetría de intentos | Intento, causa, resultado y **salto** | F9 | `K1_K2_K9.cs` |

## 2.2 Principio rector y su implementación

**«El estado del servicio externo es la fuente de verdad, no la memoria del proceso.»**

El artefacto lo implementa en tres puntos concretos, no como declaración:

| Punto | Ubicación | Mecanismo |
|---|---|---|
| Ante sospecha | `CoordinadorMetodo.Procesar` | Ante timeout o respuesta perdida, **consulta** en lugar de inferir |
| Confirmación de recurso | `K4MotorDecision.Decidir` | Un estado `NoProcesado` con recurso existente se confirma como aplicado, **no** se reprocesa |
| Reserva sin cerrar | `K5Compensador.Compensar` | Ante una reserva no confirmada, **consulta** antes de decidir si ejecuta |

Los tres son la misma regla aplicada en puntos distintos, y los tres aparecieron porque la
evidencia los exigía.

## 2.3 La normalización del estado, y su límite

La tabla de Dc3 se implementa tal cual:

| Estado externo | Categoría | Acción |
|---|---|---|
| `PAGADO` | procesado | No compensar; siguiente paso idempotente |
| `SOLICITADO`, `ANULADO`, `VENCIDO` | no procesado | Habilitar reproceso |
| `PENDIENTE` (< 20 min) | indeterminado | Reconsultar |
| `PENDIENTE` (≥ 20 min) | indeterminado | Reconciliar |

**Pero la ejecución mostró que esa tabla no basta**, y el artefacto la amplía. El estado
`SOLICITADO` significa «la operación de negocio no está pagada», y no dice si el QR llegó a
crearse. En los escenarios 2 y 3 el estado es `SOLICITADO` **y el recurso existe**, porque el
externo aplicó el efecto antes de cortar la respuesta.

Tomar la decisión solo con la categoría lleva a `Reprocesar`, que en un doble neutro crea un
segundo QR. Por eso K3 consulta además cuántos recursos tiene el externo para la identidad, y
K4 usa los dos hechos:

```csharp
if (v.Categoria == CategoriaEstado.NoProcesado)
{
    if (v.RecursoExiste)
        return Decision.ConfirmarProcesada;   // el efecto YA ocurrió
    return Decision.Reprocesar;               // no ocurrió: repetir es seguro
}
```

**Es una extensión del método, no del doble**, y quedó registrada como cambio de V1.

## 2.4 La compensación: qué es y por qué es idempotente

**Definición operativa** (sección 4.7 del informe de E6, coherente con esta construcción):

> El método define el contrato de idempotencia de **cualquier acción compensatoria** registrada
> en la bitácora. La anulación del QR no pagado es el primer exemplar; la reversión de la venta
> ya efectivizada es el segundo.

El mecanismo de K5, en orden:

| # | Paso | Por qué en ese orden |
|---|---|---|
| 1 | **Reservar** en la tabla de idempotencia por `(operación, motivo)` | Si se registrara después, un fallo de transporte entre el efecto y el registro dejaría la repetición sin reconocer |
| 2 | **Ejecutar** la acción contra el externo | Solo si la reserva fue creada |
| 3 | **Marcar** aplicada, solo si hubo respuesta | Un fallo de transporte deja la reserva sin cerrar, no en falso éxito |

Por qué esto es idempotente y no «reintentar con suerte»: la garantía no depende del número de
intentos ni de que el externo deduplique. Depende de que exista **un único registro por par,
escrito antes de la primera ejecución**.

## 2.5 Independencia del número de intentos

`NumeroIntentos` es un parámetro editado a mano en producción, con valores entre 1 y 10 según
la transaccionalidad (hallazgo de E6). El artefacto **no lo usa**:

| Elemento | Depende del número de intentos |
|---|---|
| Identidad de la operación (K1) | No |
| Idempotencia de la compensación (K5) | No |
| Idempotencia de la operación | No |
| Resolución del estado indeterminado (K7) | No |
| Tolerancia a `0003`/`0004` | **Sí** |

`MaxIntentosNegocios` solo modera los reintentos por error de negocio. Con valor 1 el método
sigue siendo correcto: pierde tolerancia, no corrección.

## 2.6 Reutilización de tablas y condición de unicidad

Dc4 manda reutilizar lo que existe y crear solo lo que falta:

| Elemento | Decisión |
|---|---|
| Estado de la operación | Reutilizar `T_QR_SOLICITUD` |
| Intentos | Reutilizar `T_QR_SOLICITUD_EJECUCION_INTENTOS` |
| **Idempotencia de compensaciones** | **Tabla nueva** `PAGO_SIMPLE_T_COMPENSACION`, clave `(TRAMITE_SECUENCIAL, T_PAR_SIMPLE_TRAMITE_FK, MOTIVO)` |
| Índice único de Dc10 | DDL **producido y comentado** |

El DDL está en `Sql/01_tabla_idempotencia_compensaciones.sql` y **no se ejecutó contra ninguna
base de datos**. El índice único de Dc10 va comentado porque rechazaría los 44 pares duplicados
que E6 encontró; hay que resolverlos antes de crearlo.

## 2.7 Ausencia de mensajería

Requisito C1. El artefacto no introduce cola, bus ni agente de mensajes. La reconsulta de
`PENDIENTE` y la reconciliación se diferiden al proceso programado existente
(`WinServiceSoatConsultaPagosQR`, Dc12). No hay ningún `RabbitMQ`, `ServiceBus` ni cola
introducida.

---

# 3. Verificación V1–V8

## 3.1 Diseño de la evaluación

### Doble de evaluación

| Propiedad | Cómo se satisface |
|---|---|
| **Neutro** | Cada invocación de `GenerarQR` / `AnularQR` **crea una unidad nueva** con su propio `IdQr`. Sin deduplicación, sin verificación |
| **No verifica ni compensa por el cliente** | Solo expone la consulta y aplica operaciones |
| **Contadores observables** | Por operación y por salto: `invocaciones`, `efectosAplicados`, `compensacionesAplicadas` |
| **Estados mapeables a K3** | `SOLICITADO`, `PAGADO`, `ANULADO`, `VENCIDO`, `PENDIENTE` |
| **Reloj inyectable** | Endpoint `admin/reloj/avanzar`; la ventana de 20 min se simula sin esperas reales |
| **Aislado** | `localhost:5098`. Datos sintéticos. BUSA real nunca invocado |

### Modos de fallo

| Modo | Efecto | Respuesta | Escenarios |
|---|---|---|---|
| `CaidaMitad` | No aplica | Aborta | 1 |
| `TimeoutPostEfecto` | **Aplica** | Retiene 30 s | 2 |
| `RespuestaPerdidaPostEfecto` | **Aplica** | Aborta | 3, 6 |
| `DejarPendiente` | No aplica | `PENDIENTE` | Ventana de 20 min (V4) |

## 3.2 V1 Exploratoria (K1, K2, K3) — escenarios 2, 3, 5

### Comprobación V1

**K3 distingue procesado / no procesado / indeterminado.**

| Escenario | Inyección | Fallo del cliente | Consulta K3 | Categoría | Recurso externo | Decisión de K4 |
|---|---|---|---|---|---|---|
| 2 | Timeout | `TIMEOUT` | `estado=SOLICITADO unidadesQr=1` | NoProcesado | **1** | `ConfirmarProcesada` |
| 3 | Respuesta perdida | `ERROR_TRANSPORTE` | `estado=SOLICITADO unidadesQr=1` | NoProcesado | **1** | `ConfirmarProcesada` |
| 5 | Reintento tras éxito | Ninguno | — | — | **1** | `ConfirmarProcesada` |

**CUMPLE.** Los tres estados se distinguen y, en los dos casos de ambigüedad, la categoría de
negocio (`NoProcesado`) contrasta con el recurso externo (`1`), y el método no reprocesa.

### Hallazgos de V1

| # | Hallazgo |
|---|---|
| H-V1-1 | La categoría de negocio y la existencia del recurso son hechos distintos. Normalizar solo por el estado lleva a duplicar |
| H-V1-2 | K3 detecta el duplicado pero no puede impedirlo. El impedimento requiere K5 |
| H-V1-3 | El método decidió correctamente sin depender del número de intentos: en 2 y 3 terminó en la primera verificación |

## 3.3 V2 Experimental (+ K4, K5, K6, K9) — escenarios 1, 4, 6

### Comprobación V2 (CRÍTICA)

**Repetir la compensación deja el estado final idéntico.**

Se repite la compensación N veces con K5 activo. El observable es el contador del **externo**.
Como el doble es neutro, cualquier compensación de más aparecería.

| N | Compensaciones efectivas (externo) | En bitácora `Aplicada` | Estado final externo | ¿Idéntico a N=1? |
|---|---|---|---|---|
| 1 | **1** | 0 | `ANULADO` | referencia |
| 2 | **1** | 1 | `ANULADO` | Sí |
| 5 | **1** | 1 | `ANULADO` | Sí |
| 10 | **1** | 1 | `ANULADO` | Sí |

**CUMPLE.** Exactamente una compensación efectiva para las cuatro valeurs de N.

La columna de bitácora explica un comportamiento correcto: con N=1 la respuesta de la
compensación se pierde y la reserva queda **sin confirmar** (`Aplicada = 0`). Con N ≥ 2, la
segunda invocación **consulta** el externo, lo confirma y cierra la reserva (`Aplicada = 1`)
sin volver a ejecutar. El efecto externo sigue siendo uno en ambos casos.

### Escenario 1 — caída a mitad

| Observable | Valor |
|---|---|
| Recursos externos | **0** |
| Decisión de K4 | `Reprocesar` |
| Estado en bitácora | `NoProcesada` |

Reprocesar es seguro porque el externo no tiene nada. Es el caso donde el sistema actual sí
reintentaría a ciegas.

### Escenario 4 — solicitud duplicada

Tres invocaciones de la misma identidad, sin fallo.

| Observable | Antes del defecto 4 | Después |
|---|---|---|
| Recursos externos | **3** | **1** |

Ver 4.3, defecto 4.

### Escenario 6 — fallo al repetir una anulación

| Observable | Valor |
|---|---|
| Compensaciones aplicadas en el externo | **1** |
| Compensaciones aplicadas en la bitácora | **1** |
| Estado final | `Compensada` |
| Estado externo | `ANULADO` |

### Comprobación V6 — telemetría

| Métrica | Valor |
|---|---|
| Total de eventos | 2 |
| Eventos con intento, causa y resultado completos | 2 / 2 |
| Eventos con salto identificado | 2 / 2 |

**CUMPLE.** Ejemplo de registro:

```
t=10:00:00 intento=1 salto=Externo causa=fallo de transporte ERROR_TRANSPORTE resultado=INDETERMINADA decision=Reconciliar
t=10:00:00 intento=1 salto=Externo causa=recurso confirmado en el externo resultado=CONFIRMADA_SIN_REPETIR decision=ConfirmarProcesada
```

### Comprobación V7 — sin espera bloqueante

Inspección del código del artefacto:

| Patrón | Presente |
|---|---|
| `Thread.Sleep` | No |
| `.Wait()` / `.Result` | No |
| `lock` | No |
| `ManualResetEvent` / `Semaphore` | No |

Contraste con el sistema real, que sí bloquea:

| Sistema | Patrón | Ubicación |
|---|---|---|
| RpcA (`CPagoSimple`) | `Thread.Sleep(milliseconds)` en el bucle | `CPagoSimple.cs` L186 |
| RpcB (`CNPagosQR`) | `Thread.Sleep(milliseconds)` en el `catch` | reintento de `Obtener` |

**CUMPLE.** El artefacto no introduce espera; difiere la reconsulta al proceso programado.

## 3.4 V3 Operacional (+ K7, K8) — escenarios 1 a 6

### Matriz completa

| # | Escenario | Inyección | Estado en bitácora | Categoría K3 | Recursos externos | Compensaciones externas | Decisión de K4 |
|---|---|---|---|---|---|---|---|
| 1 | Caída a mitad | aborta antes del efecto | `NoProcesada` | NoProcesado | 0 | 0 | `Reprocesar` |
| 2 | Timeout | aplica y retiene | `Procesada` | NoProcesado | **1** | 0 | `ConfirmarProcesada` |
| 3 | Respuesta perdida | aplica y aborta | `Procesada` | NoProcesado | **1** | 0 | `ConfirmarProcesada` |
| 4 | Solicitud duplicada | 3 invocaciones | `Procesada` | — | **1** | 0 | `ConfirmarProcesada` |
| 5 | Reintento tras éxito | 2 invocaciones | `Procesada` | NoProcesado | **1** | 0 | `ConfirmarProcesada` |
| 6 | Fallo al repetir anulación | aplica y aborta | `Compensada` | — | **1** | **1** | `Compensar` |

En ninguno de los seis escenarios hay duplicado. En el 1 no hay recurso porque el externo
abortó antes del efecto; en el 4 y el 5 hay exactamente uno pese a la repetición.

### Comprobación V3 (CRÍTICA)

**No se compensa dos veces una operación ya compensada.**

| Observable | Valor |
|---|---|
| Invocaciones de compensación | **10** |
| Registros `Aplicada` en bitácora | 1 |
| Compensaciones efectivas en el externo | **1** |
| Pares `(operación, motivo)` duplicados | **0** |

**CUMPLE.**

### Comprobación V4 — ventana de 20 minutos

| Reloj (min) | Estado externo | Categoría K3 | Minutos | Decisión K4 |
|---|---|---|---|---|
| 0 | `PENDIENTE` | Indeterminado | 0 | `Reconsultar` |
| 10 | `PENDIENTE` | Indeterminado | 10 | `Reconsultar` |
| 20 | `PENDIENTE` | Indeterminado | 20 | `Reconciliar` |
| 30 | `PENDIENTE` | Indeterminado | 30 | `Reconciliar` |
| 40 | `PENDIENTE` | Indeterminado | 40 | `Reconciliar` |

**CUMPLE.** En las cinco mediciones la categoría fue `Indeterminado`: no se asumió ningún
estado (Dc3). El reloj es virtual, por lo que la ventana se agotó sin esperar 40 minutos.

### Comprobación V5 — reconciliación idempotente

Dos corridas consecutivas sobre una operación con una reserva de compensación sin confirmar.

| Observable | Corrida 1 | Corrida 2 | ¿Igual? |
|---|---|---|---|
| Compensaciones efectivas en el externo | 1 | **1** | Sí |
| Estado final externo | `ANULADO` | `ANULADO` | Sí |
| Estado en bitácora | `Compensada` | `Compensada` | Sí |

**CUMPLE.** La primera corrida resolvió la reserva consultando el externo; la segunda no
produjo efectos nuevos.

### Comprobación V8 — consistencia interna

| Aserción | Evidencia | Resultado |
|---|---|---|
| Identidad única y reproducible | `llaveEnBitacora=7400\|1` = `llaveDerivada=7400\|1` | cumple |
| Consistencia bitácora ↔ externo | `bitacora=Compensada` = `externo=ANULADO` | cumple |

Matriz completa en `v8_trazabilidad.md`: 0 requisitos sin componente, 0 componentes sin
requisito, 0 componentes sin paso, 0 requisitos sin comprobación.

**CUMPLE, con salvedades** declaradas en la sección 5.

---

# 4. Registro de cambios

Detalle completo en `registro_cambios.md`. Resumen:

| Categoría | Cantidad |
|---|---|
| Cambios de construcción con razón ligada a hallazgo | 9 |
| Defectos del artefacto detectados por ejecución | 4 |
| Defectos del banco de pruebas detectados por ejecución | 4 |

## 4.1 Los cuatro defectos del artefacto

Ninguno se anticipó por revisión de código. Todos aparecieron porque una comprobación no se
cumplió o porque el contador externo contradecía la bitácora.

| # | Defecto | Detección | Corrección |
|---|---|---|---|
| 1 | K3 no veía la cantidad de recursos del externo | V1, escenarios 2 y 3: categoría `NoProcesado` con el recurso ya creado | K3 lee `UnidadesQr` y expone `RecursoExiste` |
| 2 | K5 **omitía a ciegas** ante cualquier reserva previa, incluso sin confirmar | **V5** reportó NO CUMPLE con 0 efectos: el plan llegaba vacío | Distingue reserva `Aplicada` (omite) de `Registrada` (**consulta**) |
| 3 | K4 confirmaba la operación como procesada sin marcar el salto con efecto confirmado, y K6 no tenía nada que planificar | **V5**: estado `Procesada` con 0 reservas pendientes y 0 efectos | `MarcarEfectoConfirmado` al confirmar |
| 4 | **Nada protegía la idempotencia de la operación**: la segunda invocación volvía a llamar al externo | Matriz de V3, escenario 4: 3 invocaciones → **3 recursos** | Guarda al inicio de `Procesar`: si el estado es final, se devuelve sin reinvocar |

## 4.2 El defecto 4 en detalle

Afecta al objetivo (a) del método —la idempotencia de la **operación**, no solo de la
compensación— y no lo cubría ninguna comprobación previa.

| Observable | Antes | Después |
|---|---|---|
| Escenario 4, recursos tras 3 invocaciones | 3 | **1** |
| Escenario 5, recursos tras 2 invocaciones | 2 | **1** |

Era invisible en V2 y V3 porque esas miden la compensación. Solo la matriz de los seis
escenarios lo hizo visible, y solo porque el escenario 4 existe para eso.

## 4.3 El defecto más grave del banco de pruebas

| Defecto | Detección | Impacto |
|---|---|---|
| `/busa/_QR_002` respondía 200 con cuerpo vacío | Toda verificación K3 devolvía «consulta falló por transporte» | **Las mediciones de V1 eran inválidas:** K3 nunca consultó de verdad. Un resultado falso habría pasado por evidencia válida |

## 4.4 La disciplina de detección

Los ocho defectos se(localizaron por **contradicción interna en la evidencia**: el contador del
externo frente a lo que la bitácora afirmaba, o una comprobación que no se cumplió. Es el
mismo criterio que se aplicó en E6 y que pueda volverse a aplicar en E8.

---

# 5. Revisión por expertos

## 5.1 Walkthrough con practicante independiente

**Estado: NO EJECUTADO.** El guion está preparado en `walkthrough_practicante.md` con ocho
tareas y tres puntos críticos de medición, pero **no hay practicante asignado**.

El criterio de cierre 4 **no está cumplido** y no puede cumplirse por quien construyó el
artefacto: la propiedad que se quiere medir es justamente que un tercero aplique el método sin
ayuda del autor, y esa propiedad no es observable si la cumple el propio constructor.

La sección 6 del guion queda vacía a propósito. Rellenarla antes de tiempo invalidaría la
medición.

## 5.2 Revisión expert

| Revisor | Perfil | Estado |
|---|---|---|
| — | Senior .NET con experiencia en transacciones distribuidas | **Pendiente** |
| — | Especialista en Sagas / consistencia eventual | **Pendiente** |

## 5.3 Preguntas abiertas para la revisión

| # | Pregunta |
|---|---|
| 1 | ¿La extensión de K3 que consulta `UnidadesQr` es coherente con el diseño de `07c`, o debería haber sido una decisión de diseño nueva y no una corrección? |
| 2 | ¿La reserva `Registrada` sin confirmar debería cerrarse por consulta en K5, o eso corresponde exclusivamente a K7? Hoy está en ambos, con la responsabilidad repartida |
| 3 | ¿`K6.EjecutarPlan` y el bucle de `CoordinadorMetodo.Compensar` deberían unificarse? Hoy hay dos rutas de ordenamiento |
| 4 | ¿El segundo exemplar de compensación (reversión de venta, `Ven06Revertir`) está en alcance de E7e, o pertenece a una etapa posterior? |

---

# 6. Limitaciones

## 6.1 Lo que la evidencia demuestra y lo que no

| Afirmación | Estado |
|---|---|
| El artefacto, con este doble, no produce efectos duplicados en los seis escenarios | **Demostrado** |
| La idempotencia proviene de K5 y no del doble | **Demostrado** por el control negativo |
| El método funciona con el doble neutro | **Demostrado** |
| El método funciona contra **BUSA real** | **No demostrado.** Nunca se invocó |


## 6.2 Lo que no se pudo hacer en esta estación

| Limitación | Causa | Consecuencia |
|---|---|---|
| **Compilar la solución real** | Los proyectos son `net462` con frameworks UVNet no disponibles aquí | El artefacto no está integrado en `PasarelaPagos.sln`. El `.sln` **no se modificó** |
| **Escribir el adaptador a `CNPagosQR`** | Requiere los tipos reales del framework | El puerto `IServicioExterno` no tiene implementación WCF |
| **Escribir el adaptador a `T_QR_SOLICITUD`** | Requiere la cadena de conexión y el acceso a datos | El puerto `IBitacora` solo tiene implementación en memoria |
| **Aplicar el DDL** | Regla de no modificar producción | La tabla de idempotencia no existe en ninguna base |
| **Crear el índice único de Dc10** | Rechazaría los 44 duplicados de E6 | Dc10 queda pendiente |
| **Ejecutar el walkthrough** | Requiere un practicante distinto del autor | Criterio de cierre 4 sin satisfacer |

## 6.3 Límites del diseño

| Límite | Detalle |
|---|---|
| Reversión de pago | **Fuera del alcance.** BUSA no la ofrece (Dc8). Para un QR pagado la acción correcta es continuar, no compensar |
| Reversión de venta | Declarada en el contrato `wsVentasV1` (`Ven06Revertir`) con **cero sitios de invocación**. Es el segundo exemplar del contrato de idempotencia, pero no se ejercitó |
| Reconciliación financiera | Fuera del alcance. El método cubre la operación de la pasarela, no la conciliación con el banco |
| Cadena de dos saltos | El método se describe para dos saltos; con más exige propagar identidad y orden |
| Concurrencia | Los escenarios son secuenciales. No se midió comportamiento bajo carga |

---

# 7. Reproducción

## 7.1 Requisitos

| Requisito | Versión |
|---|---|
| SDK .NET | 10.0.401 (verificado) |
| Sistema operativo | Windows |
| Acceso a red | Solo `localhost` |

## 7.2 Comandos

```powershell
# --- Terminal 1: doble de evaluación neutro ---
cd "C:\Users\lchavez\Documents\Proyecto predeterminado\e7e-testbed\busa-eval-stub"
dotnet build
dotnet run --urls "http://localhost:5098"

# --- Terminal 2: verificaciones ---
cd "C:\Users\lchavez\Documents\Proyecto predeterminado\e7e-testbed\coordinador"
dotnet build
dotnet run --no-build -- V1        "..\evidencia_e7e"
dotnet run --no-build -- V2        "..\evidencia_e7e"
dotnet run --no-build -- V3        "..\evidencia_e7e"
dotnet run --no-build -- NEGATIVO  "..\evidencia_e7e"
```

El código de salida del control negativo es 0 solo si el doble es neutro **y** K5 evita el
duplicado.

Para detener el doble: `Stop-Process -Name BusaEvalStub`

## 7.3 Artefacto

```powershell
cd "C:\Users\lchavez\Source\Workspaces\UNIVidaNetPasarelaPagos\UNIVidaNetPasarelaPagos\E7e_MetodoCompensacion"
dotnet build
```

## 7.4 Índice de la evidencia

| Archivo | Contenido |
|---|---|
| `v1_resultados.md` | Escenarios 2, 3, 5 y hallazgos de V1 |
| `v2_resultados.md` | Escenarios 1, 4, 6; comprobaciones V2, V6, V7 |
| `v3_resultados.md` | Escenarios 1–6; comprobaciones V3, V4, V5, V8 |
| `v8_trazabilidad.md` | Matriz paso ↔ requisito ↔ componente ↔ comprobación |
| `control_negativo.md` | Salida bruta del control negativo |
| `runbook_control_negativo.md` | Procedimiento y criterio de satisfacción |
| `registro_cambios.md` | Cambios, razones y defectos |
| `walkthrough_practicante.md` | Guion preparado; **sin ejecutar** |

## 7.5 Verificaciones de entorno

| Verificación | Resultado |
|---|---|
| Operaciones contra BUSA real | **0** |
| Escrituras en bases de datos de producción | **0** |
| Archivos de producción modificados | **0** |
| Acceso a datos personales o credenciales reales | **Ninguno** |
| Infraestructura de mensajería introducida | **Ninguna** (requisito C1) |
| Datos del doble | Sintéticos (`OP-*`) |

---

