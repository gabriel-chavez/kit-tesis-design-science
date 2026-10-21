# Registro de cambios del artefacto (E7e)

Cada entrada declara la **razón** y el hallazgo o comprobación que la origina. Sin esa
vinculación, el cambio no cuenta como iteración.

**Artefacto:** `E7e_MetodoCompensacion`
**Fecha real:** 2026-10-13

---

## 1. Cambios de construcción

| Fecha | Versión | Cambio | Razón |
|---|---|---|---|
| 2026-10-13 | V1 | Se crea `E7e_MetodoCompensacion` como librería `netstandard2.0` con K1, K2, K3 | Dc3 de `07c`: el coordinador debe poder referenciarse desde `net462` (sistema real) y desde el arnés de verificación. Una copia del método en el arnés habría hecho que la evidencia fuera de la copia y no del artefacto |
| 2026-10-13 | V1 | El método se define contra **puertos** (`IServicioExterno`, `IBitacora`, `IReloj`, `ITelemetria`) en lugar de contra un transporte | Dc13: la garantía de idempotencia no puede depender de que el externo sea WCF ni de SQL Server. Los puertos hacen que el artefacto sea el mismo en producción y en evaluación |
| 2026-10-13 | V1 | K3 consulta **además del estado, cuántos recursos tiene el externo** para la identidad (`UnidadesQr`) | **H-V1-1**, observado ejecutando V1: un estado `SOLICITADO` puede significar «el QR ya existe» o «nunca se creó». Normalizar solo por el estado llevaba a `Reprocesar`, que en un doble neutro crea un segundo QR |
| 2026-10-13 | V2 | Se añaden K4 (motor de decisión), K5 (compensador idempotente), K6 (ordenador) y K9 (telemetría) | Componentes faltantes para poder decidir, compensar sin duplicar y registrar. Sin ellos no había forma de sostener la propiedad que el artefacto afirma |
| 2026-10-13 | V2 | K5 reserva en la tabla de idempotencia **antes** de ejecutar | Dc5. Un registro posterior dejaría sin reconocer una repetición cuando el fallo de transporte ocurre entre el efecto y el registro |
| 2026-10-13 | V2 | Se añade `InyectorSaltoInterno` y el parámetro `forzarFalloSaltoInterno` | Dc11: cubrir las dos rutas de entrada (RpcA y RpcB), no solo el salto externo |
| 2026-10-13 | V2 | El arnés pasa a `ProjectReference` del artefacto; se retiran sus copias de K1, K2, K3 y del modelo | Razón metodológica: la evidencia de V1–V8 debe salir del código que vive en el repositorio real |
| 2026-10-13 | V3 | Se añaden K7 (reconciliador) y K8 (verificador) | Requeridos por las comprobaciones V5 y por la emisión de aserciones sobre observables externos, no sobre estado interno |
| 2026-10-13 | V3 | K7 resuelve las **reservas de compensación sin confirmar**, no solo las operaciones indeterminadas | Detectado en la primera corrida de V3: una reserva `Registrada` que nunca se cierra deja la operación sin estado final, que es la condición de contorno que el método dice resolver |
| 2026-10-13 | — | DDL de `PAGO_SIMPLE_T_COMPENSACION` y del índice único de Dc10, **sin ejecutar** | Requisito de no alterar producción. El DDL se entrega; aplicarlo requiere autorización |

## 2. Defectos detectados por la ejecución y corregidos

Cada defecto se detectó porque una comprobación **no se cumplió** o porque el observable
externo contradecía lo que la bitácora afirmaba. Ninguno se anticipó por revisión de código.

| # | Defecto | Cómo se detectó | Corrección | Comprobación que lo detecta |
|---|---|---|---|---|
| 1 | K3 no veía la cantidad de recursos del externo, solo el estado de negocio | Ejecución de V1, escenarios 2 y 3: la categoría era `NoProcesado` con el recurso ya creado | K3 lee `UnidadesQr` y expone `RecursoExiste` | V1 |
| 2 | `K5.Compensar` **omitía a ciegas** ante cualquier reserva previa, incluso sin confirmar | **V5** reportó `NO CUMPLE` con 0 compensaciones externas: el plan llegaba vacío | K5 distingue reserva `Aplicada` (omite) de reserva `Registrada` (**consulta** al externo) | V2, V3, V5 |
| 3 | K4 confirmaba la operación como procesada pero **no marcaba el salto con efecto confirmado**, por lo que K6 no tenía nada que planificar | **V5**: `Estado antes de reconciliar: Procesada` con `Reservas pendientes: 0` y 0 efectos externos | `CoordenadorMetodo.MarcarEfectoConfirmado` al confirmar | V5 |
| 4 | Nada protegía la **idempotencia de la operación**: una segunda invocación de la misma identidad volvía a llamar al externo | **Matriz de V3**, escenario 4: 3 invocaciones produjeron **3 recursos** externos | Guarda al inicio de `Procesar`: si el estado ya es final confirmado, se devuelve sin reinvocar | Escenario 4 |

### 2.1 Detalle del defecto 4

Es el más relevante porque afecta al objetivo (a) del método —la idempotencia de la
**operación**, no solo de la compensación— y no lo cubría ninguna comprobación previa.

| Observable | Antes del arreglo | Después |
|---|---|---|
| Escenario 4, recursos externos tras 3 invocaciones | **3** | **1** |
| Escenario 5, recursos externos tras 2 invocaciones | 2 | **1** |

El defecto era invisible en las comprobaciones V2 y V3 porque estas miden la compensación.
Solo la matriz de escenarios 1-6 lo hizo visible, y solo porque el escenario 4 existe para eso.

## 3. Defectos del banco de pruebas corregidos

Distintos de los anteriores: afectan a la infraestructura, no al artefacto.

| # | Defecto | Detección | Impacto |
|---|---|---|---|
| 5 | El endpoint `/busa/_QR_002` respondía 200 con cuerpo vacío: firma `Task<IResult>` con `Results.Json` no escribía la respuesta | Toda verificación K3 devolvía «consulta falló por transporte» | **Las mediciones de V1 eran inválidas**: K3 nunca consultó de verdad |
| 6 | El parser de contadores del arnés separaba por espacio, pero el volcado del doble usa `;` | Excepción `Sequence contains no matching element` al leer `compensacionesAplicadas` | Impedía leer los observables externos en V2 y V3 |
| 7 | El arnés resolvía la ruta de `test-config` con dos niveles de upwards | Mensaje `NO ENCONTRADO` (heredado de E6) | El experimento se omitía en silencio |
| 8 | El método de la comprobación V4 consultaba una operación no registrada en la bitácora | Excepción `Operacion no registrada: OP-7200-G1` | V4 no se ejecutaba |

El defecto **5** es el más grave de esta tanda: sin él, la primera corrida de V1 habría
«verificado» que K3 fallaba siempre, cuando en realidad K3 nunca llegó a ejecutarse. Un
resultado falso habría pasado por evidencia válida.

## 4. Elementos identificados y NO resueltos

Se registran para que no se confundan con trabajo completado.

| # | Elemento | Estado | Por qué |
|---|---|---|---|
| 1 | Índice único de Dc10 contra `T_QR_SOLICITUD` | **DDL producido, no ejecutado** | Rejectaría los 44 pares duplicados existentes. Requiere resolverlos primero y autorización |
| 2 | `K6.EjecutarPlan` vs. el bucle equivalente en `CoordinadorMetodo.Compensar` | **Dos rutas de ordenamiento** | Divergencia latente. No detectada por las comprobaciones porque el arnés ejercita una sola |
| 3 | `K2.MarcarAbandonada` y `K1.ExigirLlaveCorrecta` | **Implementadas, no ejercitadas** | El escenario «el proceso muere entre el registro y la llamada» no está en la taxonomía de 6 escenarios |
| 4 | Adaptador a `CNPagosQR` y a `T_QR_SOLICITUD` reales | **No escrito** | Requiere compilar la solución real (`net462` + frameworks UVNet), no disponible en esta estación |
| 5 | Práctica del walkthrough por persona independiente | **Pendiente** | No puede ejecutarla quien construyó el artefacto |

## 5. Re-ejecución de verificación (2026-10-14)

**Sin cambios en el artefacto.** Se repitió la corrida completa para dejar evidencia
auditable con salida cruda y bloque de procedencia.

| Aspecto | Detalle |
|---|---|
| Artefacto | `E7e.MetodoCompensacion.dll`, `netstandard2.0` |
| Ruta | `...\UNIVidaNetPasarelaPagos\E7e_MetodoCompensacion\bin\Debug\netstandard2.0\` |
| SHA-256 | `d00087332fddb581af8617bbcbcb7f07189d9f24221b6812fc9a6c8e5725c612` |
| Fecha en disco del binario | 2026-10-13 03:13 — **no se recompiló** |
| Comando | `dotnet run -- VTODAS ..\evidencia_e7e` (desde `e7e-testbed\coordinador`) |
| Código de salida | 0 |
| Comprobaciones | V1, V2, V3, V4, V5, V6, V7, V8 → todas **CUMPLE** |
| Control negativo | **CONFIRMADO** — sin K5: N efectos (1, 2, 5, 10); con K5: 1 efecto siempre |
| Salida cruda | `crudo/vtodas_salida_cruda.txt` |
| Enlace con el cliente real de BUSA | 2/2 OK — `crudo/enlace_busa_salida_cruda.txt` |

El binario es el mismo archivo del 2026-10-13: la re-ejecución reprodujo los resultados
**sin necesidad de tocar el artefacto**. Cada evidencia generada incorpora ahora su propio
bloque de procedencia (fecha, comando y SHA-256), de modo que puede comprobarse contra qué
versión del artefacto se produjo.

## 6. Resumen

| Categoría | Cantidad |
|---|---|
| Cambios de construcción con razón ligada a hallazgo | 9 |
| Defectos del artefacto detectados por ejecución | 4 |
| Defectos del banco de pruebas detectados por ejecución | 4 |
| Elementos identificados y no resueltos | 5 |

Ningún cambio de este registro se hizo por motivaciones estéticas o por gusto de refactorizar. Los ocho
defectos se(localizaron) por contradicción entre lo que afirmaba la bitácora y lo que
mostraba el contador externo, o por una comprobación que no se cumplió.
