# Trazabilidad de la evidencia humana del ciclo sumativo

**Fecha de registro:** 2026-10-30
**Alcance:** sesiones P1 (2026-10-28, 09:00–09:50, CASO-A) y P2 (2026-10-28, 11:00–11:55, CASO-B)
**Documentos firmados:** `firmas/escaneos/` (consentimientos y declaración del observador)

Este documento responde a tres objeciones sobre la evidencia humana: la coincidencia de las respuestas con un documento previo, la disponibilidad de notas crudas y la independencia del observador. Para cada una se distingue lo **declarado** de lo **documentado**, y se identifica qué sostiene y qué no.

## 1. Fuentes efectivamente disponibles

| Resultado documentado | Fuente disponible | Cómo se produjo | Trazabilidad |
|:--|:--|:--|:--|
| Decisiones por escenario (E1–E6) | `hoja_observacion_P1_CASO-A.md`, `hoja_observacion_P2_CASO-B.md` | Registro completado por el observador durante o inmediatamente después de la sesión (declaración §3) | Directa a la hoja; sin nota primaria independiente |
| Tareas respondidas (6/6) y puntos críticos E2/E6 (2/2) | Mismas hojas | Derivados de las decisiones registradas | Directa a la hoja; la corrección la juzgó el observador |
| Tiempos por escenario y duración total | Mismas hojas (columnas inicio/fin) | Anotados por el observador | Directa a la hoja; sin registro de reloj independiente |
| Cuestionario de ocho ítems y entrevista | `evaluacion_P1_P2.md` §6, hojas | Respuestas transcritas por el observador | Directa a la hoja; sin audio ni formulario original |
| «¿Pidió ayuda?» | Hojas, columna correspondiente | No se registró durante la sesión | **No evaluable** (salvedad §4 de la declaración) |
| Consentimiento y presencia | `firmas/escaneos/01…`, `02…`, `03…` | Firmados el 2026-10-30 | Documento firmado |
| Resultados técnicos | `evidencia_tecnica/` (CSV, crudos, hash) | Corrida del 2026-10-28, 20:38, sobre el doble neutro | Reproducible con el arnés, salvo que no existe modo `SUMATIVO` |

**Fuente primaria disponible:** las hojas de observación. No hay una segunda fuente que permita contrastarlas.

## 2. Notas crudas: ausencia declarada

**No existen notas crudas independientes** de las sesiones P1 y P2: el único registro es la hoja de observación.

Consecuencias que se declaran:

- Las hojas de observación son el **único registro** de lo ocurrido en las sesiones.
- Su contemporaneidad se apoya **solo** en la declaración del observador (§3: «completé el registro yo mismo durante o inmediatamente después de la sesión») y en la nota de la hoja («transcrita el mismo día de la sesión»). No hay traza primaria que lo verifique de forma independiente.
- Las hojas **no** se presentan como notas contemporáneas independientes, sino como el registro del observador, completado en la sesión o inmediatamente después.
- Cualquier reconstrucción posterior del contenido no puede acreditarse como registro contemporáneo y no se usa como tal.

Esta ausencia no invalida el registro, pero limita la verificación: no permite contrastar lo anotado contra una fuente anterior.

## 3. Coincidencia con el documento previo: declaración y brecha

**Declaración del tesista (2026-10-30).** La coincidencia entre las respuestas de las versiones anteriores y el documento previo —retirado del expediente el 2026-10-27 por no constituir evidencia— se explica porque los ajustes realizados estuvieron orientados a mejorar el **formato, la estructura y la presentación** de las respuestas, sin modificar el contenido sustantivo ni los resultados previamente obtenidos. Las coincidencias responderían, por tanto, a la conservación de la información de base durante los cambios de presentación.

**Brecha declarada.** La afirmación anterior **no está documentada**: no se conserva un diff entre versiones ni un registro de cambios que permita distinguir las modificaciones de formato de las que pudieran afectar el contenido. Por tanto:

- Se registra como **declaración del tesista**, no como hecho verificado.
- No se usa para elevar el grado de la evidencia humana.
- La trazabilidad formato/contenido queda como limitación abierta. Cerrarla exige un anexo con el diff entre versiones y la lista de cambios de presentación.

## 4. Independencia del observador: alcance y límites

**Lo que la declaración acredita.** El observador firmó (2026-10-30) que no participó en el diseño del método, que no es autor del artefacto, que no recibió instrucciones sobre el resultado esperado, que es **colega en la misma organización** del tesista y que no existe relación jerárquica que condicione su registro.

**Límite.** La pertenencia a la misma organización impide sostener **independencia plena**. Puede introducir sesgos de familiaridad, de expectativa o de conveniencia, aunque no se identifica una relación jerárquica.

**Distinción que se adopta.** La función del observador fue **registrar y verificar el desarrollo de la actividad**, no **validar independientemente los resultados**. Son dos funciones distintas y no se equiparan:

| Función | Quién la ejerció | Qué sostiene |
|:--|:--|:--|
| Observación y registro | Jose Luis Quispe (misma organización) | Que el registro corresponde a lo que presenció |
| Validación independiente de resultados | **Nadie** | No se afirma |

La independencia se evalúa por responsabilidades, participación efectiva y conflictos de interés, no solo por la ausencia de relación jerárquica. Con la información disponible, la independencia es **parcial** y así se declara.

## 5. Qué sostiene y qué no

**Sostiene:**

- Que hubo dos sesiones con dos practicantes, con consentimiento firmado y con un observador que declara haber estado presente y haber completado el registro.
- Que las decisiones, tiempos y respuestas del cuestionario constan en las hojas y son la base de los conteos reportados.
- Que el registro de ayuda no es evaluable.

**No sostiene:**

- Que las respuestas no provengan total o parcialmente del documento previo: la explicación es una declaración sin traza documental.
- La contemporaneidad verificada de las hojas: no hay notas crudas independientes.
- Independencia plena del observador: es colega de la misma organización.
- Validación independiente de los resultados: esa función no se ejerció.
- El criterio de aplicabilidad: sigue **no alcanzado** con n = 2 y sin medición de la ayuda.

## 6. Pendientes que permanecen

1. Anexar el diff entre versiones del documento previo y la lista de cambios de presentación, si se quiere cerrar la brecha de §3.
2. Resolver la salvedad §4 de la declaración (fe de errata o re-firma con una sola opción); mientras tanto el campo queda «no evaluable».
3. Si se busca independencia plena, incorporar un observador externo a la organización en una réplica futura.
