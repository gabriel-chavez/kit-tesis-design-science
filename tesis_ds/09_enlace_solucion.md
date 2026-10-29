# E9. Enlace propuesta ↔ solución

> **Estado:** cerrada el 2026-10-28 **con brechas declaradas**. La matriz está completa y cada eslabón sin evidencia suficiente está listado en §6 y §8; no queda ningún eslabón huérfano por omisión. Las afirmaciones que la evidencia no sostiene se retiran en §8.

## 1. Propósito

Enlazar, eslabón por eslabón, el problema de diseño con la solución construida y con la evidencia de evaluación. La pregunta que responde este documento no es "¿el artefacto funciona?", sino **"¿qué afirmaciones sobre la solución están respaldadas y cuáles no?"**.

Cadena que se recorre:

```text
problema de diseño → requisito → decisión de diseño → componente
→ indicador/criterio → evidencia → grado de cumplimiento → limitación
```

## 2. Regla de lectura

### 2.1 Demostración no es evaluación

- **Demostración:** el artefacto se aplica a un caso y produce un resultado observable.
- **Evaluación:** comparación sistemática contra criterios fijados antes de construir (E6).

Este documento usa ambas, y las distingue en cada fila. Una demostración sobre un doble **no** es un caso resuelto en producción.

### 2.2 Escala de cumplimiento

| Grado | Significado |
|:--|:--|
| **Cumple** | Hay evidencia ejecutada, reproducible y del escenario que corresponde |
| **Cumple con salvedad** | Hay evidencia, pero el alcance es menor que el enunciado del requisito |
| **Parcial** | Hay implementación y alguna evidencia, pero falta el escenario que la pondría a prueba |
| **No verificado** | No hay evidencia ejecutada |
| **No alcanzado** | Hay evidencia y el resultado es negativo o insuficiente |

### 2.3 Regla de evidencia

Un eslabón cuenta como respaldado solo si existe un archivo con salida cruda o un resultado registrado. Una afirmación sostenida únicamente por el diseño o por inspección de código se marca como tal.

## 3. Problemas de diseño enlazados

Derivados de `02_delimitacion.md` §1.2 y de `06_diagnostico.md` §5–§7:

| # | Problema | Fuente |
|:--|:--|:--|
| P1 | Falla parcial de comunicación deja la operación en estado inconsistente | E2 §1.2 |
| P2 | Ante timeout o respuesta perdida no se distingue "no procesado" de "procesado con respuesta perdida" | E6 §7 (hallazgo central) |
| P3 | La compensación no es idempotente: repetirla produce efecto duplicado | E2 §1.2; E6 §9 |
| P4 | El reintento se hace a ciegas: no verifica estado y duplica operaciones | E6 §5 (dos rutas divergentes) |
| P5 | Operaciones huérfanas o indeterminadas sin cierre | E6 §7; E7c §7.bis |
| P6 | El reintento no deja rastro (intento, causa, resultado) | E6 I3 no instrumentado |
| P7 | La base admite duplicados: 44 pares sin índice único | E6 §7 (2026-10-08) |
| P8 | Dos rutas de entrada (RpcA/RpcB) con reintentos divergentes | E6 §5 |
| P9 | No existen criterios de verificación de la idempotencia | E5; E2 §1.3 |

## 4. Matriz de trazabilidad — requisitos funcionales

| Req. | Problema | Decisión | Componente | Indicador / criterio | Evidencia | Grado | Limitación |
|:--|:--|:--|:--|:--|:--|:--|:--|
| **F1** Identidad determinista | P4 | Dc1 | K1 | Toda operación con identidad estable | V8 (aserción de identidad); sumativo E4: 3 solicitudes → 1 recurso | **Cumple** (doble) | `K1.ExigirLlaveCorrecta` no se invocó; supone unicidad de `(TRAMITE_NUMERO, GESTION)` por año |
| **F2** Registrar estado | P5, P6 | Dc4 | K2 | Operación y compensación con estado | V6, V8; formativo S10 (fallo de escritura manejado) | **Cumple con salvedad** | K2 corrió sobre bitácora en memoria; no se probó sobre `T_QR_SOLICITUD` real |
| **F3** Consultar estado antes de decidir | P2 | Dc2, Dc3, Dc14 | K3 | I1/I3; criterio 2 | V1; sumativo E2/E3 (`ConfirmarProcesada`); formativo S8/S9 | **Cumple** (doble) | Los estados de BUSA están simulados; no se consultó el servicio real |
| **F4** Decidir de forma determinista | P1, P2 | Dc3, Dc14 | K4 | Decisión correcta en 6/6 escenarios | V1; sumativo 6/6 por caso; formativo 11 escenarios | **Cumple** (doble) | Depende de K3: si la consulta falla, el método no decide (laguna del segundo fallo) |
| **F5** Compensación idempotente | P3 | Dc5, Dc13 | K5 | Criterio 1: 0 compensaciones duplicadas | V2, V3; control negativo 3/1; formativo S1–S11; sumativo E6 (4 → 1) | **Cumple** (una instancia) | La idempotencia distribuida depende de Dc10, que no se aplicó |
| **F6** Ordenar compensaciones | P1 | Dc7 | K6 | Sin compensaciones fuera de orden | V2 escenario 6; V8 | **Parcial** | `K6.EjecutarPlan` no se invocó; el coordinador usa un bucle equivalente; no hay escenario con varias compensaciones dependientes |
| **F7** Reconciliar indeterminados | P5 | Dc3, Dc9, Dc12 | K7 | 0 operaciones en estado desconocido | V5 (dos pasadas idempotentes); formativo S7 (0 pendientes) | **Cumple** (doble) | No se ejecutó con el servicio de Windows real ni con huérfanas de producción |
| **F8** Criterios de verificación | P9 | — | K8 | I7 = 6/6 | V2, V3, V8 | **Cumple con salvedad** | La taxonomía la definió el investigador; V8 se cumple con cuatro miembros no ejercitados |
| **F9** Telemetría trazable | P6 | Dc6 | K9 | I3 instrumentado | V6; formativo S10 | **Cumple** (doble) | Telemetría en memoria; no persistida en producción |

## 5. Matriz de trazabilidad — requisitos contextuales

| Req. | Decisión | Componente | Criterio | Evidencia | Grado | Limitación |
|:--|:--|:--|:--|:--|:--|:--|
| **C1** Sin marco de mensajería | — | Puertos | Se aplica sin infraestructura nueva | Inspección de código; artefacto `netstandard2.0` sin dependencias fuera de la BCL | **Cumple** (inspección) | No se probó dentro de la solución real |
| **C2** Solo consulta de estado | Dc2 | K3 | Funciona con la interfaz de consulta | Sumativo: 6 consultas frente a 0 de la línea base | **Cumple** (doble) | La interfaz real de BUSA no se ejercitó |
| **C3** Cubrir los seis escenarios | — | — | 6 de 6 | Sumativo 6/6 en dos casos; formativo 11 escenarios | **Cumple** | Los escenarios los fijó el investigador |
| **C4** No bloquear el hilo | Dc6 | K4, K9 | Sin `Thread.Sleep` en el camino crítico | V7 (inspección de código) | **Cumple** (inspección) | Sin prueba de carga ni de concurrencia sostenida |
| **C5** Costo razonable | — | — | Un equipo puede adoptarlo | **Sin medición** | **No verificado** | No se midió esfuerzo de implementación ni costo operativo |
| **C6** Aplicable por terceros | — | — | Practicantes independientes sin ayuda | Walkthrough E7e (n = 2); sumativo P1/P2 | **No alcanzado** | Sin registro de ayuda; hojas sin firmar; aplicabilidad exploratoria |

## 6. Verificación de eslabones

### 6.1 Búsqueda de huérfanos

| Búsqueda | Resultado |
|:--|:--|
| Requisito sin componente | **0** (F1–F9 y C1–C6 tienen componente o se declaran sin él) |
| Componente sin requisito | **0** (K1–K9 mapean a F1–F9) |
| Requisito sin evidencia | **1**: C5 |
| Requisito con evidencia insuficiente | **2**: F6, C6 |
| Decisión sin materialización | **1**: Dc10 (DDL producido, no ejecutado) |
| Decisión parcialmente materializada | **1**: Dc11 (solo se ejercitó el salto interno forzado) |
| Componente no ejercitado | **1**: `K6.EjecutarPlan` |
| Miembro no invocado | **1**: `K1.ExigirLlaveCorrecta` |

### 6.2 Qué significa

- **Dc10** es el eslabón crítico: sin índice único y sin resolver los 44 pares duplicados, el método no puede afirmar idempotencia entre instancias. Está declarado desde E7c y se sostiene como condición de despliegue.
- **Dc11** queda como deuda de integración: el salto interno real (`CNPagosQR`) está compilado pero no ejecutado.
- **F6** no puede pasar a "cumple" mientras no exista un escenario con dos o más compensaciones dependientes y una sola ruta de ordenamiento.
- **C5** no se midió y no se puede inferir del diseño.

## 7. Demostración frente a evaluación

### 7.1 Demostración

| Aspecto | Estado |
|:--|:--|
| Casos usados | CASO-A (`63442 / 4`) y CASO-B (`1598416 / 1`) |
| Medio | Doble neutro `busa-eval-stub`; BUSA real nunca invocado |
| Cadena real | Cliente WCF de BUSA enlaza con el doble (2/2); saltos internos compilados, **no ejecutados**; cadena completa **no ejecutada** |
| Resultado | 12/12 escenario-caso; 0 duplicados; 0 inconsistencias |

**Conclusión de la demostración:** el método se aplica y produce el resultado previsto **sobre el doble**. No hay ningún caso resuelto contra el servicio real; la demostración es de laboratorio.

### 7.2 Evaluación

| Ciclo | Qué mide | Resultado |
|:--|:--|:--|
| Formativo (11 escenarios) | Defectos y rediseño | D1–D4 resueltos o declarados; versión congelada |
| Sumativo técnico | Utilidad frente a la línea base | 12/12 frente a 0/12; 0 duplicados frente a 10 recursos y 6 compensaciones duplicadas |
| Sumativo humano | Aplicabilidad | **No alcanzado** (ayuda no registrada; hojas sin firmar) |

## 8. Afirmaciones: respaldo y decisión

| # | Afirmación | Respaldo | Decisión |
|:--|:--|:--|:--|
| 1 | Evita compensaciones con efecto duplicado | Sí: V2, V3, control negativo, formativo, sumativo | **Se mantiene**, con alcance "una instancia, doble neutro" |
| 2 | Evita duplicados de operación ante reintento | Sí: sumativo E2–E5 frente a la línea base | **Se mantiene**, mismo alcance |
| 3 | Distingue procesado / no procesado / indeterminado | Sí: V1, V4, sumativo E2/E3 | **Se mantiene**, con estados simulados |
| 4 | Garantiza idempotencia distribuida | **No**: Dc10 sin aplicar | **Se retira**. Queda como condición de despliegue |
| 5 | Funciona dentro de la cadena real de pagos | **No**: cadena no ejecutada | **Se retira**. Solo se afirma el enlace del cliente externo (2/2) |
| 6 | Es aplicable por practicantes sin ayuda | **No**: ayuda no registrada, hojas sin firmar | **Se degrada** a "aplicabilidad exploratoria, n = 2 piloto" |
| 7 | Reduce el tiempo de resolución (9,67 h) | **No medido** después del método | **Se retira**. El dato es del diagnóstico, no del resultado |
| 8 | Reduce el costo operativo | **No medido** | **Se retira** |
| 9 | Cubre los seis modos de fallo | Sí: 6/6 con taxonomía propia | **Se mantiene** con la salvedad de que la taxonomía la definió el investigador |
| 10 | El método no depende del número de intentos | Sí: 07e §2.5 | **Se mantiene** |

## 9. Limitaciones y condiciones de contorno

1. **Instancia única.** La garantía de idempotencia se probó dentro de un proceso. Entre instancias depende de Dc10, no aplicado.
2. **Doble, no BUSA.** Los estados, tiempos y códigos son simulados. El comportamiento real del proveedor no está caracterizado.
3. **Compensación acotada.** La acción compensatoria disponible es anular el QR no pagado (Dc8). No hay reversión de pagos ejecutados.
4. **Integración parcial.** Los saltos internos reales no se ejecutaron; la cadena completa tampoco.
5. **Reconciliación no operada.** Se probó el componente, no su ejecución programada con el servicio de Windows.
6. **Aplicabilidad exploratoria.** Dos practicantes, un piloto no verificado, ayuda no registrada.
7. **Costo no medido.** C5 no tiene evidencia.
8. **Fuera de alcance.** Conciliación financiera con el banco, canales de tarjeta y Tigo Money.

## 10. Puerta de E9

| Condición | Estado |
|:--|:--|
| Matriz completa, de problema a limitación | **Cumple** |
| Ningún eslabón huérfano por omisión | **Cumple**: 6 eslabones sin evidencia suficiente, todos declarados en §6 |
| Cada afirmación de solución respaldada o retirada | **Cumple**: §8 retira 4 afirmaciones y degrada 1 |
| Limitaciones declaradas | **Cumple**: §9 |
| Demostración distinguida de evaluación | **Cumple**: §7 |

## 11. Lo que E9 traslada a E10

1. La contribución debe formularse **solo** sobre las afirmaciones 1, 2, 3, 9 y 10 de §8.
2. El principio de diseño central es: *el estado del participante externo es la fuente de verdad, y la identidad de la operación y de la compensación debe ser previa al efecto*.
3. Las condiciones de contorno de §9 son parte del principio, no notas al pie.
4. Dc10 no es una limitación menor: es la frontera entre "idempotencia verificada" e "idempotencia garantizada en despliegue".

## 12. Deudas de coherencia detectadas

- `07f_ficha_artefacto.md` sigue en estado "borrador", cita el hash `d0008733…` (anterior a la congelación) y declara el walkthrough como pendiente, cuando se cerró el 2026-10-20. Debe corregirse antes de citar la ficha en la tesis.
- `07e_construccion_verificacion.md` §1.2 afirma que el comportamiento con WCF "no se probó", mientras `evidencia_e7e/prueba_enlace_cliente_real.md` registra el enlace del cliente real (2/2). Conviene precisar: lo probado es el **cliente** contra el doble, no el servicio real.
