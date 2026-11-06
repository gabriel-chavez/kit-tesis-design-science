# ALSIE — Informe Final de Investigación (IFI)

Documentos normativos del modelo institucional de **ALSIE** para el Informe Final de Investigación, convertidos a Markdown. Son la fuente de la forma que aplica el agente `traductor-alsie`.

## Documentos

| Archivo | Contenido | Uso |
|:--|:--|:--|
| `guia_ifi_contenido.md` | Qué debe contener, punto a punto, cada apartado del IFI: carátula, resumen, índice, introducción, capítulos I–IV, conclusiones, recomendaciones, referencias y anexos | Estructura y contenido de cada apartado |
| `rubrica_maestria.md` | Rúbrica puntuada de maestría (100 puntos, noviembre de 2026): criterios con puntos, BIEN/MAL, transversales y escala. Prevalece sobre los indicadores de 2024 | Redacción y autoevaluación del IFI |
| `indicadores_revision_ifi.md` | Indicadores de evaluación por apartado (junio de 2024) y relaciones esenciales del documento | Complemento de la rúbrica |

## Notas

- El agente aplica **solo el nivel de maestría**: desarrolla el Capítulo III (Modelo teórico) y el Capítulo IV (Propuesta), e incluye en el perfil Hipótesis, Aporte teórico y Significación práctica.
- Quedan **excluidas** las variantes de Diplomado, Especialidad y Monografía, donde no se desarrolla el Modelo teórico y el perfil omite Hipótesis, Aporte teórico y Significación práctica.
- Los archivos `.docx` originales no se versionan. Para regenerar los Markdown:

`rubrica_maestria.md` no sale de un `.docx` del repositorio: se genera desde el instrumento de TesIA (`thesis-writer/tesiaBackend/knowledge/institutions/uab/alsie/maestria-ifi.instrument.yaml`), que incorpora la rúbrica de ALSIE y sus modificaciones; ver su sección «Procedencia».

```bash
pandoc "Guía  IFI_Contenido_2da Edición_2023_Cuerpo.docx" -f docx -t gfm --wrap=none -o guia_ifi_contenido.md
pandoc "Indicadores para la revisión del IFI_1ra Rev_junio 2024.docx" -f docx -t gfm --wrap=none -o indicadores_revision_ifi.md
```
