# E8. Evaluación

> **Estado:** en curso. Este documento define los dos ciclos que faltan: **formativo** (mejorar el método con evidencia) y **sumativo** (juzgarlo contra los criterios congelados). Los walkthroughs de aplicabilidad ya registrados **no** sustituyen ninguno de los dos.

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

## 7. Pendientes

- Ejecutar el ciclo formativo con los once escenarios.
- Documentar el rediseño y congelar la versión.
- Definir la muestra y ejecutar el ciclo sumativo con casos distintos.
- Repetir el control negativo en cada corrida.
- Analizar y declarar las amenazas a la validez.
