# E8. Evaluación

> **Estado:** en curso. Ciclo formativo **cerrado** el 2026-10-27 en tres corridas (dos de rediseño y una de lectura corregida): D1–D4 resueltos o declarados, control negativo confirmado y **versión congelada** (SHA-256 `ec9b8b6…`). Queda el ciclo sumativo.

## 1. Posición en el marco FEDS

FEDS combina dos ejes: **propósito** (formativo o sumativo) y **paradigma** (artificial o naturalista).

| Ciclo | Propósito | Paradigma | Qué produce |
|:--|:--|:--|:--|
| **Formativo** | Mejorar el método | Artificial (laboratorio, doble neutro) | Defectos, ambigüedades y **rediseño documentado** |
| **Sumativo** | Juzgar el método final | Artificial + naturalista (practicantes) | Evidencia contra los criterios congelados |

La evaluación de maestría exige **al menos un ciclo formativo con rediseño documentado y un ciclo sumativo**. Un walkthrough de aplicabilidad mide si el método se entiende; no mide si funciona.

## 2. Criterios congelados (E6, 2026-09-26)

No se modifican.

**Utilidad:**
1. Compensaciones con efecto duplicado = 0 en todos los escenarios.
2. Operaciones con estado final inconsistente = 0.
3. Repetir una compensación deja el estado final idéntico al de una ejecución única, en N de N escenarios.

**Contribución:**
4. Aplicabilidad: cobertura de la taxonomía de fallos y aplicación por practicantes independientes.

## 3. Ciclo formativo

### 3.1 Objetivo

Encontrar defectos y ambigüedades **del método** (no del doble), corregirlos y dejar constancia del rediseño. Si el ciclo no encuentra nada, no es que el método sea perfecto: es que los escenarios no fueron exigentes.

### 3.2 Escenarios

Además de los seis de E6, se añaden casos límite que atacan las partes no ejercitadas:

| # | Escenario | Qué pone a prueba |
|:--|:--|:--|
| 1 | Caída antes del efecto | Reprocesar sin duplicar |
| 2 | Timeout después del efecto | Verificación de estado |
| 3 | Respuesta perdida | Distinguir procesado de no procesado |
| 4 | Solicitud duplicada concurrente (misma llave, en paralelo) | Unicidad e idempotencia de la operación |
| 5 | Reintento tras éxito real | Guarda de estado final |
| 6 | Fallo durante la compensación | Reserva-Ejecución-Confirmación |
| 7 | Proceso muere entre la reserva y la ejecución | `K2.MarcarAbandonada` / reconciliación |
| 8 | `PENDIENTE` en 19:59 y en 20:01 | Frontera de la ventana |
| 9 | Estado externo desconocido | No asumir; reconciliar |
| 10 | Falla la escritura en la bitácora tras reservar | Consistencia bitácora ↔ externo |
| 11 | La compensación falla dos veces y luego responde | Idempotencia bajo fallo repetido |

### 3.3 Métricas por escenario

- Compensaciones efectivas en el externo.
- Operaciones con estado inconsistente.
- Estado final (bitácora y externo).
- Decisión de K4 y número de llamadas externas.
- Completitud de la telemetría (intento, causa, resultado, salto).
- Control negativo repetido en la corrida.

### 3.4 Procedimiento

1. Congelar la versión del método (hash).
2. Ejecutar los escenarios y guardar la salida cruda.
3. Comparar el observable externo con lo que declara la bitácora.
4. Registrar cada defecto o ambigüedad con su evidencia.
5. Rediseñar y anotar el cambio con su razón.
6. Re-ejecutar los escenarios afectados y los de regresión.
7. Declarar si el método queda listo para el ciclo sumativo.

### 3.5 Evidencia y criterio de salida

- Salidas crudas, scripts, hash del artefacto, registro de cambios antes/después.
- **Criterio de salida:** defectos críticos resueltos o declarados; al menos un rediseño documentado; control negativo confirmado; versión congelada para el sumativo.

## 4. Ciclo sumativo

### 4.1 Objetivo

Juzgar la versión final contra los criterios congelados y medir la aplicabilidad con practicantes independientes.

### 4.2 Diseño

**Comparación método vs. conducta actual** sobre los mismos escenarios y el mismo doble neutro. La línea base ya está medida (E6): reintentos con espera fija, sin verificación de estado. La comparación es la que demuestra el aporte.

- **Muestra:** ≥2–3 practicantes independientes, con **casos distintos**; al menos uno externo a la organización si es posible.
- **Unidad de análisis:** escenario × participante.
- **Asignación:** orden aleatorizado de escenarios para controlar aprendizaje.
- **Observación:** un observador independiente registra; el autor no evalúa su propio método.

### 4.3 Instrumentos

- Guion de escenarios con criterios de éxito por escenario.
- Hoja de observación (decisiones, errores, tiempos, ayudas).
- Telemetría del artefacto y contadores del doble.
- Cuestionario posterior (claridad, esfuerzo, confianza) y entrevista semiestructurada.
- Control negativo en cada corrida.

### 4.4 Métricas

**Utilidad:** compensaciones duplicadas, operaciones inconsistentes, invariancia del estado final.
**Aplicabilidad:** tareas resueltas sin ayuda, puntos críticos a la primera, tiempo, ambigüedades.
**Eficiencia (opcional):** llamadas externas por operación, latencia.

### 4.5 Análisis

- Tabla por escenario y por participante, con conteos exactos.
- Análisis cualitativo de ambigüedades y decisiones.
- **Generalización analítica, no estadística:** con n pequeño no se estiman tasas poblacionales; se documentan mecanismos y condiciones.
- Reportar la dirección del efecto y los casos donde el método no mejora.

### 4.6 Amenazas a la validez

| Tipo | Amenaza | Mitigación |
|:--|:--|:--|
| Constructo | El doble no es BUSA real | Declararlo; triangular con el cliente WCF real y la inspección de código |
| Interna | Aprendizaje, orden, sesgo del observador | Orden aleatorizado; observador independiente; criterios pre-registrados |
| Externa | Una organización, un caso | Varios casos; declarar condiciones de contorno |
| Conclusión | n pequeño | Conteos exactos; no sobreafirmar; generalización analítica |

### 4.7 Criterio de éxito

- Los tres criterios de utilidad se cumplen en todos los escenarios.
- Aplicabilidad: al menos el 80 % de las tareas sin ayuda y ningún punto crítico fallado por todos los participantes.
- Ningún defecto crítico sin resolver o sin declarar.

## 5. Cronograma sugerido

| Fase | Duración | Producto |
|:--|:--|:--|
| Ciclo formativo | 1 semana | Resultados, defectos, rediseño |
| Rediseño y re-ejecución | 2–3 días | Versión congelada |
| Ciclo sumativo | 1–2 semanas | Resultados por participante |
| Análisis | 3 días | Tablas, amenazas, conclusión |

## 6. Evidencia de aplicabilidad ya disponible

| Evaluador | Fecha | Caso | Resultado | Estatus |
|:--|:--|:--|:--|:--|
| Leonel Chamaca Lima | 2026-10-16 | SOAT 3147 / 2026 | 6/8 tareas; 0/2 puntos críticos a la primera | Evidencia real |
| Anahi Quisbert | 2026-10-20 | SOAT 3147 / 2026 | 8/8 tareas; 2/2 puntos críticos a la primera | Evidencia real |

**Limitaciones:** mismo caso, misma organización, ambos junior. Se declaran; no se ocultan. Estas dos hojas miden aplicabilidad, no efectividad.

## 7. Ciclo formativo: resultados (2026-10-27)

**11 escenarios** (6 base + 5 límite) sobre el doble neutro. Evidencia en `e8_evidencia/ciclo_formativo_resultados.md` y `ciclo_formativo_rediseno.md`.

| # | Escenario | Resultado | Defecto |
|:--|:--|:--|:--|
| S1 | Caída antes del efecto | 0 recursos | — |
| S2 | Timeout después del efecto | 1 recurso; `ConfirmarProcesada` | — |
| S3 | Respuesta perdida | 1 recurso; `ConfirmarProcesada` | — |
| S4 | Duplicada concurrente (5) | **5 recursos en la 1.ª iteración → 1 tras el rediseño** | **D1 (resuelto)** |
| S5 | Reintento tras éxito | 1 recurso | — |
| S6 | Fallo en la compensación | 1 compensación | — |
| S7 | Muerte entre reserva y ejecución | 0 reservas pendientes tras K7 | **D2 (sin cablear)** |
| S8 | `PENDIENTE` 19/20 min | Reconsultar / Reconciliar | **D3 (granularidad)** |
| S9 | Estado desconocido | Reconciliar | — |
| S10 | Falla escritura en bitácora | `InvalidOperationException` | **D4 (sin manejo)** |
| S11 | Compensación falla dos veces | 1 compensación | — |
| CN | Control negativo | 3 / 1 | — |

### D1 — Concurrencia (resuelto, con una condición)

La primera iteración produjo **5 recursos** con 5 invocaciones concurrentes. Se añadió un lock por operación (`ConcurrentDictionary` estático) y S4 pasó a 1 recurso.

**Condición crítica:** ese lock es **de proceso**. Garantiza la idempotencia dentro de una instancia, pero **no entre instancias o servidores**. En un despliegue real con varias réplicas, la garantía debe venir de la **base de datos** (índice único + inserción atómica, Dc10) o de un lock distribuido. Debe declararse como límite y probarse con dos procesos, o el método no puede afirmar idempotencia distribuida.

### D2, D3 y D4 (declarados, no resueltos)

- **D2:** `K2.MarcarAbandonada` no se invoca desde K5 ni K7. Asignar a **K7 como responsable único** y cablearlo. Afecta la garantía ante muerte entre reserva y ejecución.
- **D3:** la ventana se evalúa en minutos enteros; no distingue 19:59 de 20:01. O se corrige a segundos, o se declara la ventana en minutos enteros en el método.
- **D4:** `MarcarAplicada` es `void`; un fallo de persistencia propaga excepción y deja la reserva colgada. Convertir a resultado, reintentar y dejar la reserva `Registrada` para K7.

### Criterio de salida

- D1 resuelto y verificado.
- **D2 y D4 deben resolverse antes de congelar**: afectan la garantía del método. D3 puede declararse como condición de contorno si se documenta.
- Re-ejecutar S7, S8 y S10, más la regresión de S1–S6, S9 y S11.
- Congelar la versión con su hash.
- Control negativo confirmado.

## 8. Segunda iteración y congelación (2026-10-27)

- **D2 resuelto y verificado:** K7 es el responsable único; marca `Abandonada` y la resuelve en la siguiente pasada (S7: dos pasadas, 1 compensación, 0 pendientes).
- **D4 resuelto y verificado:** `MarcarAplicada` devuelve `bool`; K5 reintenta y deja la reserva a K7 sin propagar excepción (S10: sin excepción, reserva final `Aplicada`, 0 pendientes).
- **D3 declarado:** ventana de **20 minutos completos**; no se afirma precisión de segundos.
- **D1:** resuelto para una instancia; la garantía distribuida depende de **Dc10** (índice único + inserción atómica), que pasa a ser **condición necesaria**.
- **Resultado:** 11 escenarios sin defectos abiertos; control negativo 3 / 1.
- **Versión congelada:** `ec9b8b6e8d37374ff28f2eb816285571ede997c47ec44a235da829d13dbd10bf` (43.520 bytes).

### Inconsistencia de reporte (resuelta el 2026-10-27)

`K7.InformeCorrida.ReservasResueltas` cuenta solo las reservas cerradas **por ejecución**. Las cerradas **por confirmación** (el externo ya estaba `ANULADO`) se contabilizan aparte. La evidencia ahora reporta `resueltasPorEjecucion`, `cerradasPorConfirmacion` y `Reservas resueltas (observado por transición de estado)`, que vale 1 en S7 y S10. El campo del artefacto no cambió para preservar el hash; la corrección es de lectura y así debe explicarse en la tesis.

### Alcance de "0 defectos abiertos"

Se refiere a los escenarios probados en una instancia. **Dc10 sigue abierto**: sin índice único y sin resolver los 44 duplicados, la idempotencia distribuida no está garantizada. No se puede afirmar más que lo probado.

### Paquete consolidado

Los entregables están en `e8_evidencia/entregables_E8_formativo/`: LEEME, versión congelada, rediseño, resultados, control negativo y regresión. La cronología (iteración 1: detección; 2: rediseño; 3: corrección de lectura) y las rutas quedaron corregidas.

**Dos ajustes menores:** calificar "0 defectos abiertos" como "en los 11 escenarios, una instancia" en `version_congelada.md`; y unificar las rutas de reproducción (`..\evidencia_e7e` frente a `tesis_ds/e8_evidencia`), para que un tercero no falle al reproducir.

## 9. Pendientes

- Ajustar el calificador de "0 defectos abiertos" y las rutas de reproducción del paquete.
- Aplicar Dc10 (resolver los 44 duplicados y crear el índice único) o declararlo como condición abierta del alcance.
- Ejecutar las sesiones humanas del ciclo sumativo (P1 y P2) con observador independiente.
- Repetir el control negativo en cada corrida.
- Analizar y declarar las amenazas a la validez.

## 10. Ciclo sumativo: estado (2026-10-28)

**Medición técnica ejecutada** con la misma versión congelada `ec9b8b6…` y el doble neutro:

- Método **12/12** vs. línea base **0/12**.
- **0** recursos duplicados vs. **10**; **0** compensaciones duplicadas vs. **6**.
- Control negativo: **3 / 1** en ambos casos.
- Consultas K3: 6 (método) vs. 0 (línea base); llamadas externas: 14 vs. 38.
- La línea base se modeló con **150 ms** de espera (no 1 s) para no penalizar; la latencia no es una comparación de SLA.

**Sesiones humanas: reporte corregido recibido, no verificado (2026-10-27).** Dos sesiones formales (P1 Leonel, `CASO-A`; P2 Ernesto Gutierrez, `CASO-B`) con observadora Micaela Machicado, fechas, tiempos, respuestas, cuestionario y entrevista. **No se acepta aún como evidencia**: las firmas escaneadas están pendientes, faltan consentimientos y notas crudas, y no se reportan el conteo de tareas sin ayuda, los puntos críticos, la versión congelada, el orden ni el control negativo. **E8 no está cerrado.**

**Corrección de identidad (Opción A, 2026-10-28).** La llave es `(TRAMITE_SECUENCIAL, T_PAR_SIMPLE_TRAMITE_FK)`; `GESTION` no es el año, es el canal. Se propagó a `07b`, `07c`, `07d` y `metodo.md`. La unicidad de la llave es condición de Dc10, no una propiedad actual: hay duplicados históricos (`CASO-A` tiene 2 filas).

**Decisión:** no cerrar E8 hasta ejecutar las sesiones humanas y completar la aplicabilidad. La medición técnica no sustituye al observador ni a los practicantes.

**Paquete consolidado:** `e8_evidencia/entregables_E8_sumativo/`, con el detalle en `e8_evidencia/DETALLE_ENTREGABLES_E8.md`. Incluye informe, resultados, matriz, amenazas, mapeo de identidad, asignación de participantes y evidencia cruda (`control_negativo_sumativo.md`, `hash_verificacion.txt`, `metodo_crudo.md`, `linea_base_crudo.md`, `metricas.csv`).
