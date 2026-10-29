# E8. Evaluación

> **Estado:** **cerrado en la dimensión técnica; abierto en la de aplicabilidad.** Ciclo formativo cerrado el 2026-10-27 (D1–D4 resueltos o declarados, control negativo confirmado, versión congelada `ec9b8b6…`) y ciclo sumativo ejecutado el 2026-10-28 sobre la misma versión (§9). Los tres criterios de utilidad se cumplen en la medición técnica; el criterio de aplicabilidad **no se alcanzó** y se declara como evidencia exploratoria. Las hojas de las sesiones están pendientes de firma.

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

> **Resultado (2026-10-28):** el criterio de aplicabilidad **no se alcanzó**. El criterio no se modifica —se congeló en E6 para evitar HARKing—; el resultado se registra en §9.4.

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

> **Nota (2026-10-28):** estas dos hojas pertenecen al *walkthrough* de E7e. No forman parte del ciclo sumativo: en §9 los participantes son P1 y P2, con casos distintos, y sus hojas se clasifican como **piloto no verificado**. La hoja de Anahi Quisbert (2026-10-20) no se usa en el sumativo.

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

### D2, D3 y D4 (estado tras la primera iteración)

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

## 9. Ciclo sumativo: ejecución y resultados (2026-10-28)

### 9.1 Condiciones

- **Versión evaluada:** `E7e_MetodoCompensacion`, SHA-256 `ec9b8b6e8d37374ff28f2eb816285571ede997c47ec44a235da829d13dbd10bf`. Hash verificado antes de la corrida.
- **Medio:** doble neutro `busa-eval-stub` en `localhost:5098`. Sin contacto con BUSA real.
- **Casos:** CASO-A (`63442 / 4`, con duplicado histórico) y CASO-B (`1598416 / 1`, control). El doble parte de **cero recursos**: las dos filas históricas de CASO-A no se sembraron, por lo que **Dc10 no se ejercitó**. CASO-A se usa como llave lógica.
- **Comparación:** método vs. conducta actual (5 reintentos, espera fija de 1000 ms, sin consulta de estado), sobre los mismos seis escenarios.
- **Evidencia:** paquete del ciclo sumativo (14 archivos, 2026-10-28): `metricas.csv`, `metodo_crudo.md`, `linea_base_crudo.md`, `control_negativo_sumativo.md`, `hash_verificacion.txt` y `README_metricas.md`, más el informe, los resultados, la matriz, las amenazas, la prueba de la métrica y las dos hojas. Incorporado en `e8_evidencia/sumativo/`.

### 9.2 Resultados técnicos

| Métrica | Método | Línea base |
|:--|--:|--:|
| Escenario-caso que cumplen | 12/12 | 0/12 |
| Recursos duplicados | 0 | 10 |
| Compensaciones duplicadas | 0 | 6 |
| Estados inconsistentes | 0 | 0 |
| Consultas de estado | 6 | 0 |
| Llamadas externas | 14 | 38 |

Totales de referencia: el método creó 10 recursos y 2 compensaciones (0 duplicados); la línea base, 20 recursos y 8 compensaciones, es decir, 10 recursos y 6 compensaciones en exceso.

**Control negativo:** por caso, sin K5 = 3 y con K5 = 1 (CASO-A y CASO-B). **No se ejecutó dentro de las sesiones humanas**; es un control técnico y se declara como limitación.

**Invariancia del estado final (criterio 3):** no se re-midió en el sumativo, porque cada caso ejecuta E6 una sola vez. Se remite a la evidencia del ciclo formativo (S11).

**Sobre "estados inconsistentes = 0":** el valor es 0 en ambos enfoques. No indica que la métrica no discrimine, sino que no hubo caso positivo observado; la definición y los casos sintéticos están en la evidencia del sumativo.

### 9.3 Sesiones humanas

Dos practicantes, casos distintos, una observadora; orden fijo E1→E6, sin aleatorización.

| Participante | Caso | Duración | Tareas | Puntos críticos E2/E6 | Cuestionario |
|:--|:--|--:|:--|:--|--:|
| P1 (Leonel Chamaca Lima) | CASO-A | 50 min | 6/6 | 2/2 | 3,375 |
| P2 (Ernesto Gutierrez) | CASO-B | 55 min | 6/6 | 2/2 | 3,25 |

**Estatus: reportados, no verificados.** Las hojas no están firmadas (pendiente), no hay consentimientos, notas crudas ni declaración de independencia de la observadora, y no se explicó la coincidencia de las respuestas con un documento previo. El registro de "tareas sin ayuda" quedó como **no evaluable**. Por eso estos datos **no se usan para afirmar aplicabilidad**.

### 9.4 Criterios congelados: resultado

| Criterio (E6) | Resultado |
|:--|:--|
| Compensaciones con efecto duplicado = 0 | **Cumple** (técnico, 12/12) |
| Operaciones con estado final inconsistente = 0 | **Cumple** (técnico) |
| Estado final invariante en N de N | **No re-medido** en el sumativo; probado en el formativo (S11) |
| Aplicabilidad: ≥ 80 % de tareas sin ayuda y ningún punto crítico fallado por todos | **NO ALCANZADO.** La ayuda no se registró (no evaluable) y los puntos críticos provienen de hojas sin firmar |
| Ningún defecto crítico sin resolver o declarar | **Cumple:** el segundo fallo de consulta y Dc10 quedan declarados |

### 9.5 Declaración de alcance

> Idempotencia verificada en una instancia, con el doble neutro y en los seis escenarios de E6. La garantía distribuida depende de Dc10 (índice único e inserción atómica), que sigue como condición de despliegue. La aplicabilidad es **exploratoria**: dos participantes, piloto no verificado y sin medición de la ayuda.

No se afirma idempotencia distribuida demostrada ni eficacia de aplicación por practicantes independientes.

## 10. Pendientes

- Ajustar el calificador de "0 defectos abiertos" y las rutas de reproducción del paquete.
- Aplicar Dc10 (resolver los 44 duplicados y crear el índice único) o declararlo como condición abierta del alcance.
- **Firmar las hojas de P1 y P2** y anexar los escaneos; sin firma, los datos humanos siguen siendo piloto no verificado.
- Aportar consentimientos, declaración de independencia de la observadora y notas crudas, o declarar su ausencia definitiva.
- Explicar la coincidencia de las respuestas con un documento previo.
- Incorporar un modo `SUMATIVO` al arnés, o declarar la corrida como no replicable con el material actual.
- Repetir el control negativo en cada corrida.
- Analizar y declarar las amenazas a la validez.
