# Estado de la tesis

## Datos generales

- **Tesista:** Luis Gabriel Chavez Ramos
- **Programa:** Maestría en Ingeniería de Software (fijado)
- **Tutor:** Luis Roberto Pérez Rios, Ph.D. (fijado)
- **Institución de destino:** se define durante la traducción al modelo institucional
- **Fecha de inicio:** 2026-09-26
- **Título provisional:** por definir (E1–E2)

## Etapas

| Etapa | Estado | Archivo | Fecha de cierre |
|:--|:--|:--|:--|
| E0. Encuadre | cerrada | `00_estado.md` | 2026-09-26 |
| E1. Áreas y temas | cerrada | `01_areas_y_temas.md` | 2026-09-26 |
| E2. Delimitación | cerrada | `02_delimitacion.md` | 2026-09-26 |
| E3. Perfil de investigación DS | cerrada | `03_perfil_ds.md` | 2026-09-26 |
| E4. Marco teórico | cerrada | `04_marco_teorico.md` | 2026-09-26 |
| E5. Estado del arte | en curso | `05_estado_del_arte.md` | |
| E6. Diagnóstico con indicadores | en curso | `06_diagnostico.md` | |
| E7a. Alternativas de solución | pendiente | `07a_alternativas.md` | |
| E7b. Requisitos del artefacto | pendiente | `07b_requisitos.md` | |
| E7c. Diseño del artefacto | pendiente | `07c_diseno_artefacto.md` | |
| E7d. Plan de construcción y versiones | pendiente | `07d_plan_construccion.md` | |
| E7e. Construcción y verificación interna | pendiente | `07e_construccion_verificacion.md` | |
| E7f. Ficha del artefacto | pendiente | `07f_ficha_artefacto.md` | |
| E8. Evaluación | pendiente | `08_evaluacion.md` | |
| E9. Enlace propuesta ↔ solución | pendiente | `09_enlace_solucion.md` | |
| E10. Contribución y conclusiones | pendiente | `10_contribucion_conclusiones.md` | |

## Decisiones tomadas

| Fecha | Decisión | Justificación |
|:--|:--|:--|
| 2026-09-26 | Se adopta el paradigma Design Science como marco de la investigación y se abre el espacio de trabajo `tesis_ds/`. | El tesista inicia una tesis de maestría en Ingeniería de Software desde cero, sin tema definido, bajo el enfoque de Design Science. |
| 2026-09-26 | Se registra el perfil de partida: nombre, experiencia (8 años en desarrollo y mantenimiento de software), contexto real (sistemas empresariales e integraciones, con posibilidad de entorno controlado y datos sintéticos) e idea previa (Sagas, consistencia transaccional y compensaciones idempotentes). | Cierre de E0. Los datos provienen del propio tesista y aún no están verificados; la formulación del tema se trabaja en E1–E2. |
| 2026-09-26 | Se elige el tema T1 (compensaciones idempotentes en Sagas de pago) como columna vertebral, con componentes de T2 (criterios y mecanismo de verificación) y T3 (taxonomía de fallos). | El tema nace de un problema observado en el contexto real, admite un método con principios transferibles y aprovecha el entorno controlado para la evaluación. Cierre de E1. |
| 2026-09-26 | Se cierra la delimitación: problema de diseño con sus cuatro componentes, brecha reformulada y aceptada, objeto de estudio (el método con sus principios y criterios), campo de acción delimitado (seguros, QR, 2025) y propuesta. | Cierre de E2. Quedan trasladados a E6 el comportamiento actual del sistema y la operacionalización de la aplicabilidad. |
| 2026-09-26 | Se descarta, por ahora, el primer borrador sobre "diseño genérico de una arquitectura de microservicios". | Se detecta la señal de error de la calibración: una meta de construcción de sistema no constituye por sí sola una contribución de conocimiento de nivel maestría. Se conserva como posible vehículo del artefacto, no como objeto de la contribución. |
| 2026-09-26 | Se cierra el perfil de investigación: título, oración de contribución, pregunta prescriptiva-analítica, objetivos de construcción y de contribución, y cronograma con E4 y E5 incorporados. | Cierre de E3. La contribución es de nivel de clase (principios de diseño). El anclaje en fechas calendario queda pendiente. |
| 2026-09-26 | Se cierra el marco teórico con 22 fuentes verificadas (8 de 2020 en adelante), incluida Helland (2012) como fundamento de la idempotencia y Hummer et al. (2013) para la verificación de idempotencia. Se confirman las decisiones de diseño D1–D10. | Cierre de E4. Todas las decisiones de diseño tienen sustento verificado y el marco supera la prueba de eliminación. Quedan dos libros por verificar y la redacción extensa de la síntesis. |
| 2026-09-26 | Se adopta el **mapeo sistemático** como protocolo de E5 y se fijan los criterios de superioridad: cobertura de modos de fallo, garantía de idempotencia, verificabilidad y costo de implementación. | Cierre parcial de E5. El protocolo permite delimitar la novedad del artefacto frente a las soluciones existentes. |
| 2026-09-26 | Se registra la línea base de la práctica actual: la pasarela reintenta con una espera fija y, al agotar los intentos, produce un error no controlado; no hay diseño explícito de idempotencia ni procedimiento formal de compensación. E5 y E6 se trabajan en paralelo, según el cronograma. | Evidencia directa del problema; insumo de la caracterización de E6. |
| 2026-09-26 | Se adoptan **dos líneas base** en E6: retrospectiva (datos históricos de 2025, solo indicadores instrumentados) y experimental (reproducción de la conducta actual en el entorno controlado). Se distinguirá "no ocurrió" de "no se registró". Se fijan los seis escenarios de evaluación. | El sistema no registra compensaciones ni duplicados; sin la línea base experimental los criterios de idempotencia no serían evaluables. Los criterios de éxito quedaron congelados antes de construir (anti-HARKing). |

## Preguntas abiertas

- Caracterización de Temporal, Axon y Camunda para el estado del arte (E5).
- Registro de selección del mapeo sistemático (E5).
- Definición operacional de los indicadores, sus fuentes y su línea base (E6).
- Comportamiento actual del sistema ante cada escenario de fallo y su frecuencia (E6).
- Operacionalización de la aplicabilidad (E6/E8).

## Próximo paso

- E5 (en paralelo): completar la caracterización de herramientas y el mapeo académico.
- E6: documentar la evidencia del problema y fijar indicadores, fuentes, línea base y criterios de éxito antes de construir.
