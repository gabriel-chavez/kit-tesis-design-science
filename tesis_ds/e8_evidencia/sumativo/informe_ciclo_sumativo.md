# Informe del ciclo sumativo

**Tesis:** «Método para el diseño y la verificación de compensaciones idempotentes en transacciones distribuidas basadas en Sagas»
**Etapa:** E8 - Evaluación sumativa
**Fecha de sesiones:** 2026-10-28
**Versión evaluada:** `E7e_MetodoCompensacion` (netstandard2.0), SHA-256 `ec9b8b6e8d37374ff28f2eb816285571ede997c47ec44a235da829d13dbd10bf`
**Observador:** Jose Luis Quispe
**Documentos firmados:** consentimientos de P1 y P2 y declaración del observador, 2026-10-30 (`firmas/escaneos/`). Salvedad en §4 de la declaración: campo «¿Pidió ayuda?» no evaluable.

## Resumen

Se ejecutó el ciclo sumativo con dos practicantes y un observador. En la medición técnica, el método congelado cumplió 12/12 escenario-caso y la línea base 0/12; el método no duplicó recursos ni compensaciones, y el control negativo se confirmó (sin K5 = 3; con K5 = 1). En las hojas aportadas por el tesista (piloto con consentimiento y declaración firmados), P1 (Leonel Chamaca Lima, CASO-A) y P2 (Anahi Quisbert, CASO-B) reportan los seis escenarios, con 2/2 puntos críticos E2/E6 cada uno, 50 y 55 minutos, y un promedio de cuestionario de 3,31/5. Ambos identificaron la misma laguna de guía: qué hacer cuando la consulta de estado también falla. La confianza en producción fue baja (2/5), coherente con que Dc10 no está aplicado. La evidencia **técnica** sostiene idempotencia verificada en una instancia; la evidencia **humana** está parcialmente acreditada —hay consentimientos y declaración del observador firmados—, pero el registro de ayuda no es evaluable y no se usa para afirmar aplicabilidad. No se sostiene idempotencia distribuida.

## 1. Propósito y marco

El ciclo sumativo juzga la versión final contra criterios congelados. Se apoya en la evaluación formativa/sumativa de Venable et al. (2016), en la lógica de diseño y evaluación de Hevner et al. (2004) y Peffers et al. (2007), y en la separación entre artefacto y evaluación de Wieringa (2014). El artefacto es un método (K1-K9); el software es vehículo.

## 2. Diseño

- Comparación método vs. conducta actual (reintentos con espera fija, sin verificación ni idempotencia) sobre el mismo doble neutro.
- Seis escenarios de E6; dos casos: CASO-A (`63442 / 4`, duplicado histórico) y CASO-B (`1598416 / 1`, control). El doble parte de cero y no siembra las 2 filas históricas de CASO-A; por tanto, **Dc10 no se ejercitó**. CASO-A se usa solo como llave lógica.
- Muestra humana: P1 (Leonel Chamaca Lima) y P2 (Anahi Quisbert), casos distintos.
- Observador: Jose Luis Quispe.
- P3 (CASO-C) se descartó por decisión del tesista y no se reporta.
- La línea base se re-ejecutó (no se reutilizaron los números de E6), con 5 reintentos y espera fija de 1000 ms, equivalente a la producción.

## 3. Resultados técnicos

| Métrica | Método | Línea base |
|---|---|---|
| Escenario-caso que cumplen | 12/12 | 0/12 |
| Recursos duplicados | 0 | 10 |
| Compensaciones duplicadas | 0 | 6 |
| Estados inconsistentes | 0 | 0 |
| Consultas de estado | 6 | 0 |
| Llamadas externas | 14 | 38 |

El método confirmó el recurso en E2/E3, reutilizó el estado final en E4/E5 y compensó una sola vez en E6. La línea base duplicó en E2-E5 y acumuló cuatro compensaciones en E6. El control negativo se confirmó en ambos casos, pero se ejecutó por caso y no dentro de cada sesión humana; se declara como limitación.

## 4. Resultados

| Métrica | P1 | P2 | Total / promedio |
|---|---|---|---|
| Tareas respondidas | 6/6 | 6/6 | 12/12 |
| Tareas sin ayuda | **No medido** | **No medido** | **No evaluable** |
| Ayudas solicitadas | No registradas | No registradas | **No medido** |
| Puntos críticos E2/E6 | 2/2 | 2/2 | 4/4 |
| Duración | 50 min | 55 min | 52,5 min |
| Cuestionario | 3,375 | 3,25 | 3,31 |
| Ambigüedades | 2 | 3 | 5 |

**Regla de conteo de ambigüedades.** Se cuenta como ambigüedad la laguna de guía o de terminología. El «límite del doble» (P1-E4) se registra como hallazgo, no como ambigüedad. Resultado: P1 = 2, P2 = 3, total 5.

> Los datos de esta sección provienen de las hojas aportadas por el tesista. Están respaldados por los consentimientos de P1 y P2 y por la declaración del observador, firmados el 2026-10-30. No existen notas crudas independientes; la coincidencia con el documento previo se explica como cambio de formato, sin traza documental (§4.1); y el campo «¿Pidió ayuda?» es **no evaluable** por la salvedad §4 de la declaración. Se reportan con esos límites; no se certifica la medición de la ayuda.

**Decisiones (según las hojas).** P1 y P2 reportan identidad estable, consulta de estado antes de reintentar, distinción entre respuesta y efecto, y compensación idempotente. P1 enfatizó la restricción persistente (“en memoria no me bastaría para producción”); P2 pidió apoyo operativo para ubicar la evidencia y entender “reserva”.

**Ambigüedades compartidas (según las hojas).** Ambos preguntaron qué hacer cuando la consulta de estado también falla. Es una laguna de la guía, no un fallo de decisión en los seis escenarios. Se declara como mejora del material.

**Confianza (según las hojas).** Ambos puntuaron 2/5 en “confiaría en el método para operar un pago real”. La causa declarada es la persistencia y la concurrencia, no la lógica. Esto es consistente con Dc10 como condición de despliegue.

### 4.1 Trazabilidad de la evidencia humana (2026-10-30)

El detalle está en `trazabilidad_evidencia_humana.md`. Tres declaraciones, ninguna de las cuales eleva el grado de la evidencia:

- **Coincidencia con el documento previo.** El tesista declara que los ajustes fueron de formato, estructura y presentación, sin modificar el contenido sustantivo ni los resultados. No se conserva un diff entre versiones: la afirmación no está documentada y se registra como declaración.
- **Notas crudas.** **No existen notas originales independientes.** Las hojas de observación son el único registro; su contemporaneidad se apoya solo en la declaración del observador (§3) y en la nota de la hoja. No se presentan como notas contemporáneas independientes.
- **Independencia del observador.** Es colega de la misma organización y no existe relación jerárquica; la independencia es **parcial**. Se distingue la función de observación y registro de la validación independiente de resultados, que **no se ejerció**.

## 5. Criterios de éxito

| Criterio | Estado |
|---|---|
| Compensaciones duplicadas = 0 | Cumple (técnico) |
| Operaciones con estado inconsistente = 0 | Cumple (técnico) |
| Estado final invariante | No re-medido en el sumativo; la invariancia se probó en el ciclo formativo (S11) y se remite a esa evidencia. |
| ≥ 80 % de tareas sin ayuda | **No evaluable:** la ayuda no se midió; la hoja dice “no evaluable”. |
| Ningún punto crítico fallado por todos | **Reportado y atestiguado por el observador:** E2/E6 4/4 (n = 2). |
| Ningún defecto crítico sin resolver o declarar | Cumple: segundo fallo y Dc10 declarados |

## 6. Análisis

Los mecanismos que explican la diferencia son la consulta de estado (K3), la guarda de estado final (K2) y la reserva previa de la compensación (K5). La línea base no consulta y reintenta a ciegas; por eso duplica. El coste del método es explícito: más consultas y una tabla de idempotencia. En E1 ambos tuvieron 0 recursos; la diferencia está en la decisión (`Reprocesar` vs. `Error` tras agotar reintentos), no en el duplicado.

Con n = 2 no se estiman tasas poblacionales. Se documentan mecanismos y condiciones; la generalización es analítica.



## 7. Amenazas a la validez

| Tipo | Amenaza | Mitigación | Impacto residual |
|---|---|---|---|
| Constructo | El doble no es BUSA real. | Se declara; enlace WCF verificado en E7e. | No se afirma el comportamiento exacto de BUSA. |
| Interna | Aprendizaje y sesgo de la observadora. | Casos distintos; observadora no autora; registro pre-estructurado. | n = 2 y una sola observadora limitan el contrabalanceo. |
| Interna | Declaración del observador con dos opciones marcadas en §4 («¿Pidió ayuda?»). | Se registra como salvedad; el campo se mantiene «no evaluable» y no se adopta la opción alternativa. | El criterio de ayuda no es medible con este material. |
| Interna | P1 y P2 comparten la laguna del segundo fallo. | Se declara y se propone mejorar la guía. | La guía queda incompleta en ese punto. |
| Externa | Una organización, una cadena. | Dos casos; se describe la cadena. | No se generaliza a otras organizaciones. |
| Conclusión | n = 2. | Conteos exactos; generalización analítica. | No se estiman tasas. |
| Específica | Lock D1 de proceso; Dc10 no aplicado. | Se declara. | La idempotencia distribuida no está probada. |

## 8. Conclusión

La evidencia **técnica** sostiene que el método evita duplicados y compensaciones repetidas en los seis escenarios, en una instancia y con el doble neutro. La evidencia **humana** está parcialmente acreditada: los consentimientos de P1 y P2 y la declaración del observador están firmados (2026-10-30) y la declaración atestigua presencia, independencia declarada y autoría del registro. Persisten tres límites declarados: la salvedad §4, que deja el registro de ayuda como **no evaluable**; la ausencia de notas crudas independientes, que hace de las hojas el único registro; y la coincidencia con un documento previo, explicada por el tesista como cambio de formato sin traza documental que lo acredite. La independencia del observador es **parcial** (misma organización) y la validación independiente de resultados no se ejerció. Por tanto, no se afirma que se haya cumplido el criterio de aplicabilidad ni que los puntos críticos estén superados; solo se reporta lo que las hojas dicen, atestiguado por el observador. La laguna del segundo fallo de consulta y la baja confianza en producción son condiciones a mejorar y declarar, no fracasos del método.

La afirmación correcta es:

> Idempotencia verificada en una instancia; la garantía distribuida depende de Dc10 (índice único e inserción atómica), que queda como condición de despliegue.

No se afirma idempotencia distribuida demostrada ni generalización estadística con n = 2.

## Referencias

García-Molina, H., & Salem, K. (1987). Sagas. *Proceedings of the 1987 ACM SIGMOD International Conference on Management of Data*, 249-259. https://doi.org/10.1145/38713.38742

Helland, P. (2012). Idempotence is not a medical condition. *ACM Queue, 10*(4). https://doi.org/10.1145/2181796.2187821

Hevner, A. R., March, S. T., Park, J., & Ram, S. (2004). Design science in information systems research. *MIS Quarterly, 28*(1), 75-105. https://doi.org/10.5555/2017212.2017217

Hummer, W., Rosenberg, F., Oliveira, F., & Eilam, T. (2013). Testing idempotence for infrastructure as code. *Service-Oriented Computing (ICSOC 2013)*. https://doi.org/10.1007/978-3-642-45065-5_19

Peffers, K., Tuunanen, T., Rothenberger, M. A., & Chatterjee, S. (2007). A design science research methodology for information systems research. *Journal of Management Information Systems, 24*(3), 45-77. https://doi.org/10.2753/MIS0742-1222240302

Venable, J., Pries-Heje, J., & Baskerville, R. (2016). FEDS: A framework for evaluation in design science research. *European Journal of Information Systems, 25*(1), 77-89. https://doi.org/10.1057/ejis.2014.36

Wieringa, R. J. (2014). *Design science methodology for information systems and software engineering*. Springer. https://doi.org/10.1007/978-3-662-43839-8
