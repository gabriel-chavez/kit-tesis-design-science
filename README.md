# Kit de Tesis con Design Science

Kit de trabajo para tesistas de **Maestría en Ingeniería de Software**. Incluye agentes de IA para [opencode](https://opencode.ai) que acompañan dos momentos distintos del trabajo:

1. **Construir** la investigación bajo el paradigma de **Design Science** (agente `asesor-tesis-ds`).
2. **Traducir** ese trabajo al modelo institucional de una universidad (agente `traductor-alsie`, para ALSIE).

El kit está diseñado para dar soporte a **diversas instituciones educativas**. Cada institución tiene su propia carpeta normativa y su propio agente traductor. En la versión actual solo está implementado el traductor para **ALSIE**.

---

## ¿Qué resuelve?

La mayoría de las tesis de Ingeniería de Software fracasan científicamente por una razón que no es técnica: el tesista construye un artefacto (sistema, herramienta, modelo) sin articular qué conocimiento transferible genera construirlo. El código compila, el sistema funciona, y el capítulo de conclusiones no puede decir qué se aprendió más allá de que "el sistema funciona".

El kit ataca el problema en dos frentes: primero asegura que la investigación se construya como Design Science (con problema de diseño, problema de investigación, artefacto, evaluación y contribución); después traduce ese trabajo al formato que la institución exige para el Informe Final de Investigación.

Los agentes no escriben la tesis por el estudiante. Lo guían, lo interrogan y lo obligan a tomar decisiones fundamentadas.

---

## Los dos agentes del kit

### `asesor-tesis-ds` — construcción de la investigación

Actúa como director de tesis. Recorre **11 etapas** (E0–E10) desde la elección del tema hasta las conclusiones con principios de diseño validados, y no avanza sin cerrar cada etapa. Todos los productos se escriben en la carpeta `tesis_ds/`.

| Etapa | Qué se trabaja |
|:--|:--|
| E0 | Encuadre: estándares de maestría y reglas de trabajo |
| E1 | Área de experiencia mayor y propuesta de temas |
| E2 | Delimitación: problema de diseño, problema de investigación, objeto de estudio, campo de acción y propuesta |
| E3 | Perfil de investigación en Design Science |
| E4 | Marco teórico que sustenta el objeto de estudio |
| E5 | Estado del arte sobre la propuesta y los artefactos existentes |
| E6 | Diagnóstico con indicadores y línea base |
| E7a | Alternativas de solución y decisión con trade-offs |
| E7b | Requisitos del artefacto |
| E7c | Diseño del artefacto (componentes, estructura, diagrama) |
| E7d | Plan de construcción y versiones (producto mínimo viable, suficiencia) |
| E7e | Construcción y verificación interna |
| E7f | Ficha reproducible del artefacto |
| E8 | Evaluación (formativa con rediseño + sumativa) |
| E9 | Enlace propuesta ↔ solución (matriz de trazabilidad) |
| E10 | Contribución y conclusiones (principios de diseño) |

La propuesta no tiene que ser software. El asesor admite como artefacto un método o metodología, un marco, un modelo, un constructo, un principio de diseño o una composición, y adapta la construcción y la evaluación a la tipología. Cuando no es software, "construir" significa formalizar y aplicar, y se exige al menos una **exemplar**: una aplicación real que produzca la evidencia.

### `traductor-alsie` — traducción al modelo institucional

Se usa **solo cuando la investigación ya está terminada** (todas las etapas E0–E10 cerradas). Toma los productos de `tesis_ds/`, los mapea y los reescribe en la estructura y el lenguaje del **Informe Final de Investigación (IFI) de ALSIE**, redactando cada apartado para la rúbrica de maestría de ALSIE (100 puntos) y autoevaluando el resultado con ella. No resume: vuelca el desarrollo completo de cada fase al apartado que le corresponde (solo el Resumen y la Introducción están acotados por la norma).

Salida: el documento final en Markdown, `entrega_alsie/IFI_ALSIE.md`, con todos los apartados del modelo de ALSIE. Además produce un **anexo de mapeo** que documenta la equivalencia entre los elementos de Design Science y los del modelo de ALSIE, incluido en el propio IFI y también como archivo de trabajo `entrega_alsie/mapeo_ds_alsie.md`. Al cerrar, ofrece generar el **guion de defensa oral de 20 minutos** (`entrega_alsie/guion_defensa.md`) a partir del documento rector `plantillas/presentacion_defensa_alsie.md`. El agente consulta al tesista cuando le falta información y nunca inventa datos.

---

## Requisitos

- [opencode](https://opencode.ai) instalado.
- Acceso a un modelo de lenguaje configurado en opencode.
- `git` (para clonar y actualizar).

---

## Instalación

```bash
git clone <URL-DEL-REPOSITORIO> mi-tesis
cd mi-tesis
opencode
```

Al abrir opencode dentro de la carpeta, el agente `asesor-tesis-ds` queda seleccionado por defecto (así lo define `opencode.json`). Para la traducción institucional, cambia al agente **traductor-alsie**.

No hay que instalar nada más ni configurar rutas: los agentes y su biblioteca de referencia viajan dentro del repositorio.

Video adicional de ejemplo: [OpenCode+Agente Tutor](https://youtu.be/pBNbl9Z-pD0)

---

## Flujo completo de trabajo

```text
1. Investigación (agente asesor-tesis-ds)
   tesis_ds/00_estado.md ... tesis_ds/10_contribucion_conclusiones.md

2. Traducción institucional (agente traductor-alsie)
   entrega_alsie/IFI_ALSIE.md
```

**Cuándo pasar al traductor:** únicamente cuando la investigación esté terminada. La señal es `tesis_ds/00_estado.md` con las etapas E0–E10 cerradas y la contribución articulada. Si se traduce antes, el IFI quedará con apartados vacíos o con contenido provisional.

---

## Soporte para diversas instituciones educativas

El kit está pensado para crecer. La estructura separa el paradigma (Design Science) de la forma institucional:

```text
instituciones/
└── alsie/                        Normativa de ALSIE (solo en Markdown)
    ├── guia_ifi_contenido.md      Qué debe contener el IFI, punto a punto
    ├── rubrica_maestria.md        Rúbrica puntuada de maestría (norma de evaluación vigente)
    ├── indicadores_revision_ifi.md Indicadores de 2024 (complemento)
    └── README.md

.opencode/agent/
├── asesor-tesis-ds.md             El asesor de Design Science
└── traductor-alsie.md             El traductor al modelo de ALSIE
```

Para agregar otra institución:

1. Crear `instituciones/<institucion>/` y convertir sus documentos normativos a Markdown (con `pandoc`, por ejemplo).
2. Crear `.opencode/agent/traductor-<institucion>.md`, tomando `traductor-alsie.md` como plantilla y ajustando el mapa de apartados, los indicadores y las salidas.
3. Actualizar este README.

En la versión actual, **solo está implementado el traductor para ALSIE**.

---

## Ejemplo de uso del traductor

**Situación.** Un tesista terminó su investigación de Design Science sobre priorización de deuda técnica en un sistema legado. En `tesis_ds/` tiene la delimitación, el marco teórico, el estado del arte, el diagnóstico con indicadores, el diseño y las evaluaciones, y las conclusiones con los principios de diseño. Debe presentar el documento ante ALSIE.

**Paso 1.** Cierra la investigación con el asesor y verifica el estado:

```text
Lee tesis_ds/00_estado.md y confirma que todas las etapas están cerradas.
```

**Paso 2.** Cambia al agente `traductor-alsie` y pide la traducción:

```text
Traduce mi investigación de tesis_ds/ al modelo IFI de ALSIE para maestría.
Lee primero instituciones/alsie/guia_ifi_contenido.md y
instituciones/alsie/indicadores_revision_ifi.md. Genera el documento final en
Markdown con todos los apartados del modelo. Si te falta información, pregúntame
antes de continuar.
```

**Paso 3.** El agente construye el mapa Design Science → ALSIE (equivalencias terminológicas y mapa apartado por apartado), detecta vacíos (datos de carátula, hipótesis, aporte teórico, significación práctica, población y muestra) y le pregunta al tesista. Con las respuestas, redacta `entrega_alsie/IFI_ALSIE.md` volcando el desarrollo completo de cada fase, e incluye el mapa como **anexo del IFI** referenciado en el cuerpo; también lo entrega como archivo de trabajo `entrega_alsie/mapeo_ds_alsie.md` y genera `entrega_alsie/autoevaluacion_indicadores.md`.

**Paso 4.** El tesista revisa, responde las preguntas abiertas y repite hasta que la autoevaluación no tenga indicadores incumplidos sin justificación. El archivo Markdown es la versión ALSIE del documento; su conversión a otro formato se hace después, a partir de ese archivo.

---

## Prompts específicos (copiar y pegar)

### Agente asesor (`asesor-tesis-ds`)

**Arranque en frío**

```
Actúa como mi asesor de tesis. Quiero iniciar mi tesis de maestría en Ingeniería
de Software con el enfoque de Design Science. Estoy empezando de cero y no tengo
tema definido. Guíame.
```

**Etapa 1 — Área de experiencia**

```
Mi área de experiencia mayor es [por ejemplo, pruebas de software]. Trabajo desde
hace [N] años en [tipo de organización], con [tecnologías y roles]. Tengo acceso a
[contexto real] durante [tiempo]. Ayúdame a explorar temas de investigación.
```

**Etapa 2 — Delimitación**

```
Ya me interesa el tema [tema]. Ayúdame a delimitar con claridad el problema de
diseño, el problema de investigación, el objeto de estudio, el campo de acción y la
propuesta, y a distinguir cada uno.
```

**Etapa 4 — Marco teórico**

```
Construye el marco teórico que sustenta el objeto de estudio. Busca teoría real y
existente, preferentemente libros y fuentes de los últimos 5 años, y verifica cada
referencia antes de citarla.
```

**Etapa 6 — Diagnóstico**

```
Ayúdame a definir los indicadores del diagnóstico (definición, fuente, línea base y
valor esperado) y a fijar los criterios de éxito antes de construir el artefacto.
```

**Etapa 8 — Evaluación**

```
Diseña la estrategia de evaluación con el marco FEDS: un ciclo formativo con
rediseño y un ciclo sumativo. Define la muestra, los instrumentos y las amenazas a
la validez.
```

**Etapa 9 — Enlace propuesta ↔ solución**

```
Ayúdame a enlazar la propuesta con la solución del problema mediante la matriz de
trazabilidad, y dime qué afirmaciones no tienen evidencia suficiente.
```

**Continuar una sesión**

```
Lee tesis_ds/00_estado.md y dime en qué etapa estamos, qué se cerró y qué sigue.
```

### Agente traductor (`traductor-alsie`)

**Traducción completa**

```
Traduce mi investigación de tesis_ds/ al modelo IFI de ALSIE para maestría. Lee
instituciones/alsie/guia_ifi_contenido.md e
instituciones/alsie/indicadores_revision_ifi.md. Genera el documento final en
Markdown con todos los apartados del modelo. Pregúntame si falta información.
```

**Solo el perfil de investigación (introducción)**

```
Traduce únicamente el perfil de investigación (problema científico, objeto de
estudio, objetivo general, objetivos específicos, campo de acción, hipótesis,
aporte teórico, significación práctica, métodos, población y muestra) a partir de
tesis_ds/02_delimitacion.md y tesis_ds/03_perfil_ds.md.
```

**Verificar coherencia antes de cerrar**

```
Revisa entrega_alsie/IFI_ALSIE.md contra los indicadores de maestría y las
relaciones esenciales de ALSIE. Dime qué indicadores no se cumplen y qué debo
corregir.
```

**Guion de defensa oral (20 minutos)**

```
Genera el guion de mi defensa oral de 20 minutos a partir de
plantillas/presentacion_defensa_alsie.md y de mi IFI en entrega_alsie/IFI_ALSIE.md.
Escribe el resultado en entrega_alsie/guion_defensa.md, priorizando lo que el
documento rector marca como irrenunciable.
```

---

## Estructura del repositorio

```text
.
├── README.md                             Este documento
├── opencode.json                         Activa el agente asesor por defecto
├── .gitignore                            Ignora tesis_ds/, entrega_*/ y binarios
├── .opencode/
│   └── agent/
│       ├── asesor-tesis-ds.md            Agente de construcción (Design Science)
│       └── traductor-alsie.md            Agente de traducción al modelo de ALSIE
├── referencias/                          Libro de Design Science (16 capítulos .md)
│   ├── ds_ch00_prefacio.md
│   ├── ds_ch01_fundamentos.md
│   └── ... ds_ch15_referencias.md
├── instituciones/
│   └── alsie/
│       ├── README.md                     Nota sobre el modelo de ALSIE
│       ├── guia_ifi_contenido.md         Contenido del IFI, punto a punto
│       └── indicadores_revision_ifi.md   Criterios de evaluación (maestría)
└── plantillas/                           Esqueletos opcionales de trabajo
    ├── 00_estado.md
    ├── matriz_coherencia.md
    ├── matriz_alternativas.md
    ├── ficha_artefacto.md
    ├── ficha_metodo.md
    ├── plan_aplicacion_exemplar.md
    ├── presentacion_defensa_alsie.md     Documento rector de la defensa oral (20 min)
    └── referencias.md
```

Los documentos normativos se guardan **solo en Markdown**. Los `.docx` originales no se versionan (están ignorados por Git).

---

## Reglas de los agentes (resumen)

- Hablan en **español académico de Bolivia**, con trato formal de "usted".
- Calibran todo al **nivel de maestría**.
- Todos los entregables y artefactos se producen **siempre en archivos Markdown (`.md`)**.
- Las citas y referencias siguen **APA 7 de forma obligatoria**.
- Nunca inventan referencias ni datos: lo no verificable se marca como `[por verificar]` y lo faltante se pregunta al tesista.
- El asesor trabaja el **problema de investigación** y anota su traducción institucional a **"problema científico"**.
- El asesor hace explícita la resignificación de Design Science: el **objeto de estudio es el artefacto propuesto**; el traductor la revierte al sentido que ALSIE exige.
- El traductor aplica **solo los indicadores de maestría** de ALSIE y verifica las relaciones esenciales del documento.
- El traductor **no resume**: vuelca el desarrollo completo de cada fase al apartado correspondiente, y entrega el **mapa de equivalencias Design Science → ALSIE** como anexo del IFI y como archivo de trabajo.

---

## Preguntas frecuentes

**¿Cuándo uso cada agente?** Primero `asesor-tesis-ds` para construir la investigación; después, `traductor-alsie` para presentarla ante ALSIE. No se usa el traductor con la investigación a medias.

**¿En qué formato entregan los agentes?** Siempre en Markdown (`.md`), con citas y referencias en APA 7. No generan Word ni PDF; si necesitas otro formato, se convierte después a partir del `.md`.

**¿Sirve para doctorado?** No. Los agentes están calibrados exclusivamente para maestría.

**¿El traductor reemplaza al asesor?** No. Son momentos distintos: uno construye, el otro traduce. Traducir sin investigación terminada produce un documento vacío.

**¿Puedo usarlo con otra universidad?** Sí. Agrega su carpeta en `instituciones/` y su agente `traductor-<institucion>`. Hoy solo existe el de ALSIE.

**¿Cómo actualizo el kit?** Con `git pull`. Tus carpetas `tesis_ds/` y `entrega_alsie/` no se ven afectadas porque están ignoradas por Git.

---

## Licencia y derechos

El contenido del libro de Design Science incluido en `referencias/` es obra de Luis Roberto Pérez Rios, Ph.D., y se distribuye con fines académicos y educativos. Los documentos normativos de ALSIE pertenecen a esa institución. La configuración y los agentes pueden reutilizarse citando la fuente.

© Luis Roberto Pérez Rios, Ph.D.
