# Presentación de defensa de tesis de maestría (modelo ALSIE)

Documento rector para convertir el Informe Final de Investigación (IFI) ya traducido al modelo de ALSIE en una **defensa oral de 20 minutos**, centrada en lo que el tribunal evalúa. Este documento no es el `.pptx`: es la guía que ordena el contenido antes de construir las diapositivas. Las diapositivas se generan después, a partir de este guion.

## 0. Propósito y criterio de éxito

La defensa **no es un resumen del IFI, es una argumentación priorizada**. Un postulante que "cuenta todo" se pasa del tiempo y entierra lo importante; uno que cuenta solo la herramienta (la propuesta) olvida el aporte teórico, que es lo que ALSIE pondera para el nivel de maestría.

El criterio de éxito de la defensa es doble:

1. Que el tribunal pueda reconstruir, sin leer el documento, la **cadena de relaciones esenciales**: problema → objeto → campo → objetivo → hipótesis → modelo → propuesta → validación → aporte → conclusiones.
2. Que quede claro qué **conocimiento** nuevo se aporta (no qué herramienta se construyó), y que ese conocimiento está respaldado por evidencia.

La jerarquía de protagonismo es fija e innegociable: el **Modelo teórico** y el **Aporte teórico** son el corazón; la **Propuesta** es su concreción; el **Diagnóstico** y el **Marco teórico** son sustento, no protagonistas.

## 1. Reglas no negociables de la defensa

1. **20 minutos máximo, sin excepción.** Ensayar con cronómetro y ajustar; si un bloque se pasa, se recorta del siguiente, nunca se invade el tiempo del tribunal.
2. **No se lee.** Las diapositivas son apoyos visuales, no el texto a recitar. El postulante habla; la diapositiva muestra el dato, la figura o la relación.
3. **El gráfico del modelo teórico es irrenunciable.** La defensa se detiene en él; sin esa figura, el tribunal no puede juzgar el núcleo de la maestría.
4. **La hipótesis se enuncia completa y se conecta con la propuesta.** Es el puente entre lo que se propone y lo que se resuelve.
5. **El aporte teórico se expresa como conocimiento** (conceptos, relaciones, regularidades), nunca como "se desarrolló un sistema".
6. **Cada afirmación de resultado se respalda con un dato** de la evaluación, no con una opinión.
7. **Idioma y tono:** español académico formal, trato de "usted" hacia el tribunal; sin jerga innecesaria ni siglas no aclaradas la primera vez.
8. **Nada de términos de Design Science sin traducir** en la presentación: se habla de "propuesta", "modelo", "validación", no de "artefacto", "principio de diseño" ni "evaluación formativa" (la traducción ya se hizo en el IFI; la defensa la hereda).

## 2. Distribución de tiempo y diapositivas (guion de 20 minutos)

Regla práctica: **≈ 1 minuto por diapositiva**, con un total de **14 a 16 diapositivas**. La distribución que sigue suma exactamente 20 minutos e incorpora un bloque breve de demostración.

| # | Bloque | Tiempo | Diapositivas | Qué debe quedar claro |
|:--|:--|:--|:--|:--|
| 1 | Portada | — | 1 | Título, postulante, tutor, programa |
| 2 | Apertura: relevancia y problema | 1,5 min | 1 | Por qué importa, a quién afecta, cuánto cuesta |
| 3 | Problema científico, objeto y campo | 2 min | 1–2 | La cadena problema → objeto → campo, encadenada |
| 4 | Objetivo general, específicos e hipótesis | 2 min | 1–2 | Qué se propone, para qué, y la hipótesis que lo sostiene |
| 5 | Diseño metodológico | 1 min | 1 | Métodos y población/muestra, sin enumerar todo |
| 6 | Diagnóstico: línea base y hallazgos | 1,5 min | 1–2 | El estado actual con datos, no el proceso |
| 7 | Marco teórico esencial | 1 min | 1 | Solo lo que fundamenta el modelo, con toma de posición |
| 8 | **Modelo teórico (corazón)** | **3,5 min** | 2 | Gráfico + componentes + relaciones esenciales |
| 9 | Propuesta (concreción del modelo) | 2 min | 1–2 | Estructura oficial con autor y concreción del modelo |
| 10 | Demostración breve | 1,5 min | 1 | El artefacto o la propuesta aplicada en el caso real |
| 11 | Validación y resultados | 1,5 min | 1 | Evidencia de que resuelve el problema |
| 12 | Aporte teórico y significación práctica | 1,5 min | 1 | El conocimiento nuevo y su utilidad concreta |
| 13 | Conclusiones y recomendaciones | 1 min | 1 | Resultados por objetivo, en términos de productos |
| 14 | Cierre | — | 1 | Agradecimiento y apertura a preguntas |
| **Total** | | **20 min** | **≈ 16** | |

## 3. Mapa defensa ↔ IFI ↔ `tesis_ds/`

Cada bloque de la defensa sale de un apartado del IFI y, en última instancia, de un producto de `tesis_ds/`. No se "reinventa" contenido: se prioriza el que ya existe.

| Bloque de la defensa | Apartado del IFI (ALSIE) | Fuente en `tesis_ds/` |
|:--|:--|:--|
| Apertura: relevancia y problema | Introducción (contexto histórico social) | `01_areas_y_temas.md`, `06_diagnostico.md` |
| Problema científico, objeto y campo | Introducción (problema científico, objeto, campo) | `02_delimitacion.md`, `03_perfil_ds.md` |
| Objetivos e hipótesis | Introducción (objetivo general, específicos, hipótesis) | `03_perfil_ds.md`, `10_contribucion_conclusiones.md` |
| Diseño metodológico | Introducción (métodos, población y muestra) | `06_diagnostico.md`, `08_evaluacion.md` |
| Diagnóstico | Capítulo I. Diagnóstico | `06_diagnostico.md` |
| Marco teórico esencial | Capítulo II. Marco teórico | `04_marco_teorico.md`, `05_estado_del_arte.md` |
| Modelo teórico | Capítulo III. Modelo teórico | `10_contribucion_conclusiones.md`, `07c_diseno_artefacto.md` |
| Propuesta | Capítulo IV. Propuesta | `07a_alternativas.md`, `07c_diseno_artefacto.md`, `07e_construccion_verificacion.md` |
| Demostración breve | Capítulo IV (metodología de aplicación) | `07f_ficha_artefacto.md`, `09_enlace_solucion.md` |
| Validación y resultados | Capítulo IV (evaluación) y Conclusiones | `08_evaluacion.md`, `09_enlace_solucion.md` |
| Aporte y significación práctica | Introducción (aporte teórico, significación práctica) | `10_contribucion_conclusiones.md` |
| Conclusiones y recomendaciones | Conclusiones y Recomendaciones | `10_contribucion_conclusiones.md` |

## 4. Guía por bloque: qué decir, qué mostrar, qué evitar

### Bloque 2 — Apertura: relevancia y problema (1,5 min)

- **Qué decir:** una oración que sitúe el problema en su contexto real y cuantifique su costo. No la historia completa.
- **Qué mostrar:** una cifra de impacto (costo, tiempo, defectos) o el dato que justifica por qué importa.
- **Qué evitar:** la biografía de la empresa, el organigrama, o un repaso histórico de la tecnología.

### Bloque 3 — Problema científico, objeto y campo (2 min)

- **Qué decir:** la cadena encadenada: situación real → brecha de conocimiento → objeto (área del conocimiento) → campo (parte esencial).
- **Qué mostrar:** un diagrama o tres viñetas que visualicen problema → objeto → campo como una sola cadena, no como tres cosas sueltas.
- **Qué evitar:** enunciar el objeto sin conectarlo con el problema; confundir el objeto (área) con la herramienta (propuesta).

### Bloque 4 — Objetivos e hipótesis (2 min)

- **Qué decir:** objetivo general (verbo de conocimiento + objeto + para qué), objetivos específicos (uno por capítulo) y la hipótesis completa.
- **Qué mostrar:** la hipótesis destacada, con la flecha "si se propone X con características Y, entonces se logra Z, porque M".
- **Qué evitar:** objetivos redactados como tareas ("desarrollar un módulo") en lugar de como producción de conocimiento; una hipótesis que no mencione el mecanismo.

### Bloque 5 — Diseño metodológico (1 min)

- **Qué decir:** qué métodos (teóricos, empíricos, técnicas) se usaron y para qué objetivo cada uno; población y muestra en una línea.
- **Qué mostrar:** una tabla compacta método → objetivo específico.
- **Qué evitar:** enumerar todos los métodos sin decir para qué sirvieron; detallar instrumentos (eso va al diagnóstico).

### Bloque 6 — Diagnóstico (1,5 min)

- **Qué decir:** la línea base con sus indicadores y los hallazgos que evidencian el problema; las regularidades del estado actual.
- **Qué mostrar:** una tabla o figura con los valores de línea base frente a los esperados.
- **Qué evitar:** narrar cómo se aplicó cada instrumento; repetir el mismo dato en tabla y figura.

### Bloque 7 — Marco teórico esencial (1 min)

- **Qué decir:** solo las teorías que sustentan el modelo, con la **toma de posición** del investigador.
- **Qué mostrar:** un mapa mínimo "teoría → componente del modelo".
- **Qué evitar:** el listado de autores sin valoración; citar a todos los autores del Capítulo II.

### Bloque 8 — Modelo teórico (3,5 min) — el corazón

- **Qué decir:** la fundamentación epistemológica breve, luego los componentes (dimensiones o variables), luego las relaciones entre ellos, y qué explican del comportamiento del objeto.
- **Qué mostrar:** **el gráfico del modelo**, grande y legible; descomponerlo: primero la estructura completa, luego cada relación.
- **Qué evitar:** saltarse el gráfico; describir el modelo sin mostrar su representación; confundir modelo con la herramienta que lo implementa.

### Bloque 9 — Propuesta (2 min)

- **Qué decir:** la definición asumida, la estructura oficial con autor sobre la que se elabora, y cómo la propuesta concreta el modelo del Capítulo III.
- **Qué mostrar:** la estructura de la propuesta y la traza "componente del modelo → elemento de la propuesta".
- **Qué evitar:** describir la propuesta sin anclarla al modelo; detalles técnicos de implementación.

### Bloque 10 — Demostración breve (1,5 min)

Ver la sección 5: el formato depende de la tipología del artefacto.

### Bloque 11 — Validación y resultados (1,5 min)

- **Qué decir:** cómo se puso a prueba la propuesta y qué datos confirman que resuelve el problema; la comparación línea base → resultado.
- **Qué mostrar:** una tabla o gráfico de resultados con los indicadores de éxito, antes y después.
- **Qué evitar:** afirmar éxito sin dato; confundir "el sistema compila" con "resuelve el problema".

### Bloque 12 — Aporte teórico y significación práctica (1,5 min)

- **Qué decir:** el aporte como **conocimiento nuevo** (conceptos, relaciones, regularidades que explican el comportamiento del objeto) y su utilidad concreta, con el vínculo entre ambos.
- **Qué mostrar:** la regularidad o principio formulado, con su contexto y su mecanismo.
- **Qué evitar:** presentar la herramienta como si fuera el aporte; inflar la generalización más allá de la evidencia.

### Bloque 13 — Conclusiones y recomendaciones (1 min)

- **Qué decir:** una conclusión por objetivo específico, en términos de resultados; una recomendación viable.
- **Qué mostrar:** una lista numerada que mapee objetivo → resultado.
- **Qué evitar:** conclusiones en términos de sugerencias; recomendaciones inviables.

## 5. Demostración breve del artefacto o propuesta

La demostración no reemplaza a la validación: muestra que la propuesta es aplicable; la validación muestra que resuelve el problema. Son bloques distintos (9–10 y 11). El formato depende de la tipología de la propuesta:

| Tipología de la propuesta | Qué se demuestra | Cómo |
|:--|:--|:--|
| Instanciación (software) | El sistema funcionando sobre el caso real | Captura de pantalla, video corto o ejecución en vivo de la función clave (≤ 1,5 min) |
| Método / proceso | La aplicación del método en un caso (la exemplar) | Mostrar los pasos ejecutados y el producto de trabajo resultante |
| Marco | La cobertura y articulación de sus componentes en el caso | Diagrama del marco aplicado al contexto |
| Modelo | Las predicciones del modelo frente a datos reales | Tabla predicción vs. observado |
| Constructo | La taxonomía o definición aplicada para clasificar el caso | Ejemplo de clasificación real |
| Principio / teoría de diseño | La instanciación o aplicación que lo soporta | Mostrar la instanciación concreta que evidencia el principio |

Reglas de la demostración: **una sola función o un solo caso**, preparada de antemano, con respaldo por si falla la conexión en vivo (captura o video). Si la propuesta no es software, la "demostración" es la **exemplar** de aplicación, no una excusa para describir de nuevo el diseño.

## 6. Priorización y errores que penaliza el tribunal

### Qué es irrenunciable (nunca se corta)

1. La cadena problema → objeto → campo → objetivo (relaciones esenciales).
2. La hipótesis completa, con su mecanismo.
3. El gráfico del modelo teórico y su explicación.
4. El aporte teórico formulado como conocimiento.
5. Al menos un dato de la validación que respalde la solución.
6. Las conclusiones en términos de resultados.

### Qué se corta si el tiempo aprieta (en este orden)

1. El detalle histórico del contexto (Bloque 2).
2. La enumeración de métodos sin propósito (Bloque 5).
3. La lista de autores del marco (Bloque 7) — se conserva solo la toma de posición.
4. El detalle de construcción de la propuesta (Bloque 9).
5. La segunda figura del diagnóstico, si una tabla ya lo muestra.

### Errores que el tribunal penaliza

1. Empezar por la tecnología o la herramienta en lugar del problema.
2. Confundir el objeto de estudio (área del conocimiento) con la propuesta (herramienta).
3. No mostrar el gráfico del modelo, o mostrarlo ilegible.
4. Presentar la herramienta como si fuera el aporte teórico.
5. Afirmar éxito sin dato de validación.
6. Leer las diapositivas palabra por palabra.
7. Pasarse del tiempo y no dejar margen para las preguntas.
8. Usar términos de Design Science ("artefacto", "principio de diseño") en lugar del lenguaje de ALSIE.
9. Conclusiones redactadas como sugerencias ("se debería…") en lugar de resultados ("se obtuvo…").
10. Recomendaciones que no son viables o no conectan con las conclusiones.

## 7. Lista de verificación previa a la defensa

Antes de ensayar, confirme cada punto:

- [ ] La presentación suma 20 minutos o menos, medida con cronómetro en el último ensayo.
- [ ] El gráfico del modelo teórico es legible desde el fondo de la sala.
- [ ] La hipótesis aparece completa y vinculada a la propuesta.
- [ ] El aporte teórico está redactado como conocimiento, no como herramienta.
- [ ] Cada resultado citado tiene su dato de validación a la vista.
- [ ] Las relaciones esenciales (problema–objetivo–objeto–campo; objetivos–conclusiones) se ven en la secuencia de diapositivas.
- [ ] No queda ningún término de Design Science sin traducir.
- [ ] La demostración tiene respaldo (captura o video) por si falla la conexión en vivo.
- [ ] Las conclusiones son resultados por objetivo, no sugerencias.
- [ ] El número de diapositivas está entre 14 y 16.
- [ ] La portada y el cierre incluyen los datos institucionales que exige ALSIE.

## 8. Ejemplos de diapositivas redactadas

Los ejemplos usan el caso recurrente del kit: priorización de deuda técnica en un sistema bancario legado en Bolivia. Son ilustrativos; el postulante los adapta a su propio trabajo. El texto entre corchetes es lo que **se dice**; el resto es lo que **se muestra** en la diapositiva.

### Diapositiva 2 — Apertura (1,5 min)

**Título:** Priorización de deuda técnica en un sistema bancario legado

- Equipo de 8 desarrolladores; sistema de 180.000 líneas de código.
- Cobertura de pruebas: 23 %.
- Las decisiones de refactorización se toman sin criterio estructurado.

> [En el sistema que mantiene el equipo, cada cambio de integración cuesta más que el anterior porque no existe un criterio que ordene qué deuda técnica atacar primero. La cobertura de pruebas es baja y el riesgo de regresión es alto.]

### Diapositiva 3 — Problema, objeto y campo (2 min)

**Título:** Problema científico, objeto de estudio y campo de acción

- **Problema:** decidir qué deuda técnica priorizar se hace sin criterio, con costos de integración crecientes.
- **Objeto:** la gestión de la deuda técnica en sistemas legados.
- **Campo:** los criterios y relaciones que determinan la priorización — impacto de cambio y riesgo de falla.

> [La brecha es que la literatura ofrece modelos de priorización por métricas técnicas, pero no evidencia sobre qué combinación es más predictiva cuando la cobertura de pruebas es baja. De esa brecha se deriva el objeto y, de él, el campo de acción.]

### Diapositiva 4 — Objetivos e hipótesis (2 min)

**Título:** Objetivo e hipótesis

- **Objetivo general:** fundamentar un modelo de priorización de deuda técnica que reduzca el tiempo de decisión de refactorización en sistemas legados con baja cobertura de pruebas.
- **Hipótesis:** *Si el equipo prioriza con un modelo de dos dimensiones — impacto de cambio y riesgo de falla —, entonces reduce el tiempo de decisión y mejora la cobertura de los módulos intervenidos, porque concentra el esfuerzo en los módulos con mayor frecuencia de cambio y mayor probabilidad de regresión.*

### Diapositiva 8 — Modelo teórico (3,5 min, dos diapositivas)

**Diapositiva 8a — Estructura del modelo**

**Título:** Modelo teórico: componentes y relaciones

- **Componentes:** impacto de cambio (frecuencia de modificación y acoplamiento) y riesgo de falla (cobertura de pruebas y densidad de defectos).
- **Relación esencial:** la prioridad es función del producto de ambas dimensiones.

*(Gráfico del modelo en bloque, grande y legible.)*

**Diapositiva 8b — Explicación**

> [El modelo propone que la prioridad no se decide por una sola métrica, sino por la interacción de dos: cuánto cambia el módulo y cuánto riesgo tiene de fallar. La relación nueva que el estudio encontró es que, en sistemas con baja cobertura, el riesgo de falla domina sobre el impacto de cambio cuando la cobertura cae por debajo del umbral observado.]

### Diapositiva 10 — Demostración breve (1,5 min)

**Título:** Aplicación del modelo en el caso real

- Clasificación de 5 módulos del sistema según impacto y riesgo.
- La prioridad resultante coincide con los módulos que luego concentraron las regresiones.

> [Aplicamos el modelo a cinco módulos del sistema real: ordenó los módulos de mayor a menor prioridad, y ese orden coincidió con los módulos donde las regresiones se concentraron en los meses siguientes.]

### Diapositiva 11 — Validación y resultados (1,5 min)

**Título:** Validación: línea base frente a resultado

| Indicador | Línea base | Resultado |
|:--|:--|:--|
| Tiempo de decisión de refactorización | 4 días por módulo | 1,5 días por módulo |
| Cobertura de pruebas en módulos intervenidos | 23 % | 41 % |

> [La comparación muestra que el modelo redujo el tiempo de decisión y elevó la cobertura en los módulos priorizados, que era el objetivo del estudio.]

### Diapositiva 12 — Aporte y significación práctica (1,5 min)

**Título:** Aporte teórico y significación práctica

- **Aporte:** la regularidad de que, en sistemas legados con baja cobertura de pruebas, el riesgo de falla domina sobre el impacto de cambio en la determinación de la prioridad.
- **Significación práctica:** un criterio de priorización aplicable que concentra el esfuerzo de refactorización donde más reduce la regresión.

> [El aporte no es la herramienta, sino la relación entre las dos dimensiones y su comportamiento bajo baja cobertura, que antes no estaba documentada para este tipo de contexto.]

### Diapositiva 13 — Conclusiones (1 min)

**Título:** Conclusiones

1. Se caracterizó el estado de la deuda técnica en el sistema: cobertura 23 % y decisiones sin criterio.
2. Se sistematizaron los fundamentos de priorización por métricas técnicas.
3. Se modeló la priorización como interacción de impacto de cambio y riesgo de falla.
4. Se elaboró y validó la propuesta, reduciendo el tiempo de decisión de 4 a 1,5 días.

> [Cada objetivo específico tiene su resultado; el conjunto evidencia que el modelo resuelve el problema planteado y aporta la relación nueva que sustenta el trabajo.]
