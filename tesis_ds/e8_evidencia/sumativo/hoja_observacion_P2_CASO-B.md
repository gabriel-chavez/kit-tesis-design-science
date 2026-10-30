# Hoja de observación — P2

## Datos de la sesión

| Campo | Valor |
|:--|:--|
| Participante | Anahi Quisbert |
| Código | P2 |
| Caso | CASO-B (`1598416 / 1`) |
| Fecha de la sesión | 2026-10-28 |
| Hora inicio / fin | 11:00 / 11:55 |
| Duración | 55 min 00 s |
| Observador | Jose Luis Quispe |
| Versión evaluada | `ec9b8b6e8d37…` |
| Fecha de llenado de esta hoja | 2026-10-28 |
| Resultados técnicos anexados el | 2026-10-28 |

## Registro por escenario

| # | Hora inicio | Hora fin | Duración | Decisión del participante | ¿Pidió ayuda? (Sí / No / no evaluable) | ¿Correcta a la primera? | Punto por precisar |
|:--|:--|:--|:--|:--|:--|:--|:--|
| E1 | 11:00 | 11:09 | 9 min | Confirmar si procesó y reintentar con el identificador original | no evaluable | Sí | Dónde consultar la evidencia |
| E2 | 11:09 | 11:20 | 11 min | Consultar estado; no reintentar | no evaluable | Sí | Segundo timeout / estado indeterminado |
| E3 | 11:20 | 11:28 | 8 min | Buscar el efecto por la misma llave | no evaluable | Sí | — |
| E4 | 11:28 | 11:38 | 10 min | Una sola operación; revisar bitácora | no evaluable | Sí | — |
| E5 | 11:38 | 11:45 | 7 min | Reutilizar el éxito previo | no evaluable | Sí | — |
| E6 | 11:45 | 11:55 | 10 min | Confirmar si la compensación ya se hizo | no evaluable | Sí | Ambigüedad de “reserva” |


## Puntos críticos del protocolo

| Punto crítico | ¿Resuelto a la primera? (Sí / No / No registrado) | Evidencia |
|:--|:--|:--|
| **E2** — consulta de estado antes de reintentar | Sí | E2: corrige el impulso y consulta |
| **E6** — verificar reserva/compensación antes de repetir | Sí | E6: confirma si ya se hizo |

**Observaciones adicionales (no son los dos puntos críticos del protocolo):** identidad estable, distinción respuesta/efecto, idempotencia de la compensación.

## Control negativo de la sesión

Control negativo: no ejecutado dentro de la sesión. Se ejecutó por caso en la corrida técnica (28/10 20:38): sin K5 = 3, con K5 = 1.


## Cuestionario

| Ítem | Puntaje |
|:--|:--|
| 1 Claridad | 3 |
| 2 Decisión | 4 |
| 3 Bitácora/estado externo | 3 |
| 4 Idempotencia | 3 |
| 5 Esfuerzo | 4 |
| 6 Confianza en producción | 2 |
| 7 Ambigüedades menores | 3 |
| 8 Recomendación | 4 |
| **Promedio** | **3,25** |

## Observaciones cualitativas

- E1: duda operativa sobre dónde consultar la evidencia.
- E2: falta instrucción para el segundo timeout.
- E6: ambigüedad conceptual en “reserva”.
- Pide una tabla de decisión con ejemplos de bitácora.

## Firmas

| Rol | Nombre | C.I. | Firma | Fecha |
|:--|:--|:--|:--|:--|
| Participante | Anahi Quisbert | | | |
| Observador | Jose Luis Quispe | | | |
| Tesista (responsable) | Luis Gabriel Chavez Ramos | | | |

Al firmar, cada firmante declara haber leído el registro de esta hoja y que las respuestas y decisiones que se le atribuyen corresponden a lo que manifestó u observó en la sesión del 2026-10-28. La hoja fue transcrita el mismo día de la sesión.

