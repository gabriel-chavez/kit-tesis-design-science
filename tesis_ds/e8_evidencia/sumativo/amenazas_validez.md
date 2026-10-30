# Amenazas a la validez del ciclo sumativo

**Versión:** `E7e_MetodoCompensacion`, SHA-256 `ec9b8b6e8d37374ff28f2eb816285571ede997c47ec44a235da829d13dbd10bf`
**Fecha:** 2026-10-28
**Observador:** Jose Luis Quispe
**Participantes:** P1 (Leonel Chamaca Lima) y P2 (Anahi Quisbert)
**Documentos firmados:** consentimientos y declaración del observador, 2026-10-30 (`firmas/escaneos/`).

## 1. Constructo

| Amenaza | Mitigación aplicada | Impacto residual |
|---|---|---|
| El doble no es BUSA real. | Se declara; el cliente WCF real se enlazó con el doble en E7e; la lógica se prueba contra el doble neutro. | No se afirma el comportamiento exacto de BUSA (timeouts, códigos, estados). |
| La compensación del exemplar es anular un QR no pagado, no revertir un pago. | Se declara Dc8; el contrato de idempotencia es agnóstico a la acción. | El pago ya ejecutado queda fuera del exemplar. |
| El doble es neutro por diseño. | Control negativo en la corrida: sin K5 = 3; con K5 = 1. | Si el doble cambiara, la comparación se invalidaría. |
| Puntos críticos definidos después del protocolo | E2/E6 se operacionalizaron al construir el guion de E8; no estaban en el protocolo congelado de E6. | Criterio de aplicabilidad no pre-registrado; se declara. |

## 2. Validez interna

| Amenaza | Mitigación aplicada | Impacto residual |
|---|---|---|
| Aprendizaje entre escenarios. | Ambas sesiones siguieron E1→E6; no hubo aleatorización. Se declara. | El orden fijo puede favorecer la familiarización; no se contrabalanceó. |
| Sesgo de la observadora. | Observadora no autora; hoja pre-estructurada; declaración de independencia firmada (2026-10-30). | Una sola observadora y colega de la misma organización: independencia parcial. Su función fue registrar, no validar de forma independiente. |
| §4 de la declaración con dos opciones marcadas («Marque una»). | Se registra como salvedad; el campo «¿Pidió ayuda?» se mantiene «no evaluable» y no se adopta la opción alternativa. | La medición de la ayuda queda inhabilitada con este material. |
| Ausencia de notas crudas. | Se declara expresamente (2026-10-30): no hay notas originales independientes. Las hojas son el único registro y no se presentan como notas contemporáneas independientes. | La contemporaneidad de las hojas se apoya solo en la declaración del observador. |
| Coincidencia literal con un documento previo ficticio. | El tesista declara que los ajustes fueron de formato, estructura y presentación, sin alterar el contenido sustantivo. | No hay diff entre versiones: la explicación no está documentada y el origen de las respuestas sigue sin acreditar. |
| El autor conoce el método. | El autor no observa ni completa hojas. | La preparación del entorno sigue a su cargo. |
| P1 y P2 comparten la duda del segundo fallo de consulta. | Se declara como laguna de la guía, no como fallo de decisión. | La guía queda incompleta en ese punto. |
| Reutilizar la línea base de E6. | Se re-ejecutó sobre el mismo doble neutro. | La línea base es una simplificación de 5 reintentos con espera fija de 1000 ms. |

## 3. Validez externa

| Amenaza | Mitigación aplicada | Impacto residual |
|---|---|---|
| Una sola organización. | Se declara; dos casos distintos. | No se generaliza a otras organizaciones. |
| Una sola cadena QR/BUSA. | Se describe la cadena y sus condiciones. | Otros externos pueden comportarse distinto. |
| Una sola instancia. | Se declara el límite del lock de D1. | La idempotencia distribuida depende de Dc10. |
| Dos participantes. | Se declaran perfiles y experiencia. | No representan a expertos senior. |

## 4. Validez de conclusión

| Amenaza | Mitigación aplicada | Impacto residual |
|---|---|---|
| n = 2. | Conteos exactos; generalización analítica, no estadística. | No se estiman tasas poblacionales. |
| Pocas repeticiones técnicas. | 6 escenarios × 2 casos × 2 enfoques. | No se mide variabilidad de red ni carga. |
| Métricas dependientes del doble. | Contadores externos, telemetría y control negativo. | El doble no es BUSA real. |
| Latencia local. | Se reporta como referencia, no como SLA. | No representa producción. |

## 5. Declaración de alcance

> Idempotencia verificada en una instancia; la garantía distribuida depende de Dc10 (índice único e inserción atómica), que queda como condición de despliegue.

