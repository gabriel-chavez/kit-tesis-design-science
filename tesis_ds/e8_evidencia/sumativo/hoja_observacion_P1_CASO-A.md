# Hoja de observación — P1


## Datos de la sesión

| Campo | Valor |
|:--|:--|
| Participante | Leonel Chamaca Lima |
| Código | P1 |
| Caso | CASO-A (`63442 / 4`) |
| Fecha de la sesión | 2026-10-28 |
| Hora inicio / fin | 09:00 / 09:50 |
| Duración | 50 min 00 s |
| Observadora | Micaela Machicado |
| Versión evaluada | `ec9b8b6e8d37…` |
| Fecha de llenado de esta hoja | 2026-10-28 |
| Resultados técnicos anexados el | 2026-10-28 |

## Registro por escenario

| # | Hora inicio | Hora fin | Duración | Decisión del participante | ¿Pidió ayuda? (Sí / No / no evaluable) | ¿Correcta a la primera? | Punto por precisar |
|:--|:--|:--|:--|:--|:--|:--|:--|
| E1 | 09:00 | 09:08 | 8 min | Confirmar registro local y reprocesar con la misma llave si no hubo efecto | no evaluable | Sí | — |
| E2 | 09:08 | 09:18 | 10 min | Consultar estado; no reintentar a ciegas | no evaluable | Sí | Qué hacer si la consulta también falla |
| E3 | 09:18 | 09:25 | 7 min | Consultar por la misma llave; confirmar si hay efecto | no evaluable | Sí | — |
| E4 | 09:25 | 09:34 | 9 min | Una operación lógica; exigir restricción persistente | no evaluable | Sí | Límite del doble: en memoria no basta |
| E5 | 09:34 | 09:40 | 6 min | Devolver el resultado exitoso conocido | no evaluable | Sí | — |
| E6 | 09:40 | 09:50 | 10 min | Revisar reserva y estado; llave de compensación idempotente | no evaluable | Sí | Atomicidad de la llave |


## Puntos críticos del protocolo

| Punto crítico | ¿Resuelto a la primera? (Sí / No / No registrado) | Evidencia |
|:--|:--|:--|
| **E2** — consulta de estado antes de reintentar | Sí | E2: consulta en lugar de reintentar |
| **E6** — verificar reserva/compensación antes de repetir | Sí | E6: revisa reserva y estado |


**Observaciones adicionales (no son los dos puntos críticos del protocolo):** identidad estable, distinción respuesta/efecto, idempotencia de la compensación.

## Control negativo de la sesión

No ejecutado dentro de la sesión. Se ejecutó por caso en la corrida técnica (28/10 20:38): sin K5 = 3, con K5 = 1.



## Cuestionario

| Ítem | Puntaje |
|:--|:--|
| 1 Claridad | 4 |
| 2 Decisión | 4 |
| 3 Bitácora/estado externo | 3 |
| 4 Idempotencia | 4 |
| 5 Esfuerzo | 3 |
| 6 Confianza en producción | 2 |
| 7 Ambigüedades menores | 3 |
| 8 Recomendación | 4 |
| **Promedio** | **3,375** |

## Observaciones cualitativas

- Enfatiza identidad estable.
- E2: falta precisar qué hacer si la consulta también falla.
- E4: “en memoria no me bastaría para producción”.
- E6: duda implícita sobre la atomicidad de la llave de compensación.

## Firmas

| Rol | Nombre | C.I. | Firma | Fecha |
|:--|:--|:--|:--|:--|
| Participante | Leonel Chamaca Lima | | | |
| Observadora | Micaela Machicado | | | |
| Tesista (responsable) | Luis Gabriel Chavez Ramos | | | |

Al firmar, cada firmante declara haber leído el registro de esta hoja y que las respuestas y decisiones que se le atribuyen corresponden a lo que manifestó u observó en la sesión del 2026-10-28. La hoja fue transcrita el mismo día de la sesión.

