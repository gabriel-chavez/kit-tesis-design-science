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
| E5. Estado del arte | cerrada | `05_estado_del_arte.md` | 2026-09-30 |
| E6. Diagnóstico con indicadores | cerrada | `06_diagnostico.md` | 2026-10-04 |
| E7a. Alternativas de solución | cerrada | `07a_alternativas.md` | 2026-10-04 |
| E7b. Requisitos del artefacto | cerrada | `07b_requisitos.md` | 2026-10-04 |
| E7c. Diseño del artefacto | cerrada | `07c_diseno_artefacto.md` | 2026-10-04 |
| E7d. Plan de construcción y versiones | cerrada | `07d_plan_construccion.md` | 2026-10-05 |
| E7e. Construcción y verificación interna | en curso | `07e_construccion_verificacion.md` | |
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
| 2026-09-30 | Se cierra el estado del arte con la comparación de MassTransit, NServiceBus, Camunda 8, Temporal y Axon. Hallazgo: ningún marco garantiza ni verifica la idempotencia de la compensación; todos la dejan a la aplicación. | Cierre de E5. La novedad del artefacto queda delimitada en el diseño y la verificación de la idempotencia, no en la coordinación. Quedan por citar las filas de Temporal y Axon (afirmaciones del tesista) y registrar el protocolo del mapeo. |
| 2026-10-04 | Se cierra el diagnóstico con evidencia de código, configuración, tickets (69 casos EFECTIVIZAR en 2025), tiempo medio de resolución (9,67 h) y reproducción de la conducta actual. Se documentan seis riesgos y el comportamiento por escenario. | Cierre de E6. Hallazgo central: el sistema no distingue "no procesado" de "procesado con respuesta perdida", no reintenta ante timeout y no verifica después del error. Pendientes que no bloquean E7: registros 2025 para I2, línea base experimental de I3, resolver 1 vs 2 intentos y justificar el proxy de I1/I4. Fecha comprometida: 2026-10-05. |
| 2026-10-04 | Se elige la **Alternativa A** (compensación idempotente con llave de idempotencia, bitácora de estado y verificación por consulta). Se confirma que BUSA permite consultar el estado de una operación. | Cierre de E7a. La verificación permite distinguir, ante respuesta perdida, entre operación procesada, no procesada y estado por reconciliar. Se descartan B, C y D. |
| 2026-10-04 | Se cierran los requisitos: llave derivada de `(TRAMITE_NUMERO, GESTION)`; estados de BUSA confirmados (procesado / no procesado / PENDIENTE indeterminado hasta 20 min); bitácora en tabla nueva; prioridades y trade-offs confirmados. | Cierre de E7b. Todo requisito es rastreable al problema, tiene métrica y los trade-offs están declarados. |
| 2026-10-04 | Se cierra el diseño con nueve componentes, diagrama, siete más dos decisiones y la definición de reconciliación. Se acota la compensación a **anular el QR no pagado** (BUSA no ofrece reversión de pago) y la reconciliación se apoya en los **servicios de Windows** existentes. | Cierre de E7c. La bitácora reutiliza `T_QR_SOLICITUD` y `T_QR_SOLICITUD_EJECUCION_INTENTOS` y añade una tabla de idempotencia de compensaciones. Condición de contorno: no cubre reversión de pagos ya ejecutados. |
| 2026-10-05 | Se cierra el plan de construcción: MVP con K1–K9, versiones V1 (05/10), V2 (06/10) y V3 (07/10), cierre de construcción y verificación el 08/10. El medio de verificación es un **doble de BUSA** sobre datos sintéticos; el servicio real no se usa en los experimentos. | Cierre de E7d. Criterio de suficiencia explícito y matriz de riesgos con mitigación. Se advierte que el calendario es muy ajustado. |
| 2026-10-05 | Se salda la deuda de E6: número real de intentos (5, hasta 10 en meses pico), I2 de 2025 (4.302 errores y 226 timeouts), justificación del proxy I1/I4 y entrevista al personal de soporte. Se registra el hallazgo de la reversión a nivel de banco, fuera de TI, como condición de contorno. Se confirma el calendario de construcción y evaluación. Se descarta la cifra de 10.000 ocurrencias por error de redacción. | E6 consolidada. Queda la línea base experimental (2026-10-06). |
| 2026-10-08 | Se ejecuta la línea base experimental (réplica del algoritmo + doble HTTP; sin contacto con BUSA real ni cambios en producción). I3 por rama, I6 sin efectos adicionales en el doble, I7 6/6. Se descubren dos rutas de reintento divergentes (RpcA/RpcB), 44 pares duplicados sin índice único, y que el servicio de convergencia existe pero no se invoca tras el fallo. | E6 completa. Refina el diseño (Dc10–Dc12) y exige que el doble de evaluación no sea idempotente por defecto. Contradicción a resolver: `NumeroIntentos` de producción (1 por inspección frente a 5/10 por testimonio). |
| 2026-10-08 | Se resuelve la contradicción de `NumeroIntentos`: el valor de producción es **dinámico**, editado a mano hasta 10 según la transaccionalidad; la prueba usó 5. Se adopta la definición de compensación **agnóstica a la acción (C), con la anulación del QR no pagado como exemplar (A)**. | Cierre de la delimitación del artefacto. La definición agnóstica preserva el nivel de clase sin fingir una reversión de pago que BUSA no ofrece. La variabilidad no registrada del parámetro refuerza el requisito F9 (telemetría). |

## Preguntas abiertas

- Aclarar si los 226 timeouts son un subconjunto de los 4.302 errores no controlados.
- Adjuntar las URL de las secciones citadas de Temporal y Axon (E5).
- Registro de selección del mapeo sistemático (E5).

## Próximo paso

- E7e: construir V1, V2 y V3 y reportar la evidencia de V1–V8 con el registro de cambios; en paralelo, la sesión de línea base experimental del 2026-10-06.
