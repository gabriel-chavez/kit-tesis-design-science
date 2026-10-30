# Definición y prueba de la métrica “estado inconsistente”

## Definición

Una operación tiene **estado inconsistente** cuando el estado final que declara el cliente no es válido frente a la verdad externa:

| Estado declarado | Verdad externa | ¿Inconsistente? |
|---|---|---|
| `Procesada` | 0 recursos | **Sí** |
| `NoProcesada` | ≥ 1 recurso | **Sí** |
| `Compensada` | externo ≠ `ANULADO` | **Sí** |
| `Error` | 0 recursos | No |
| `Procesada` | ≥ 1 recurso | No (puede haber duplicado, que se mide aparte) |
| `Compensada` | `ANULADO` | No (puede haber compensación duplicada, que se mide aparte) |

Los duplicados y las compensaciones repetidas son métricas distintas de la inconsistencia de estado. No se suman.

## Lo observado

En las 24 filas de `metricas.csv`, el flag `inconsistente` es 0. Es correcto con la definición anterior:

- La línea base E1 declara `Error` y el externo tiene 0 recursos: consistente, aunque no convergió.
- La línea base E2/E3 declara `Procesada` y el externo tiene 2 recursos: consistente, pero duplicada.
- La línea base E6 declara `Compensada` y el externo está `ANULADO`: consistente, pero con 4 compensaciones.

Por tanto, **0/0 no significa que la métrica no discrimine**, sino que las corridas observadas no produjeron un caso positivo. El positivo se prueba con casos sintéticos.

## Casos sintéticos de prueba

| # | Estado declarado | Verdad externa | Resultado esperado | Resultado |
|---|---|---|---|---|
| 1 | `Procesada` | 0 recursos | Inconsistente | **Sí** |
| 2 | `NoProcesada` | 1 recurso | Inconsistente | **Sí** |
| 3 | `Compensada` | `SOLICITADO` | Inconsistente | **Sí** |
| 4 | `Error` | 0 recursos | Consistente | No |
| 5 | `Procesada` | 2 recursos | Consistente (duplicado) | No |

**Caso positivo de línea base:** si el externo devolviera `0000` sin aplicar efecto (fallo del proveedor), la línea base registraría `Procesada` con 0 recursos y el flag sería **1**. Ese caso no ocurrió con el doble neutro, que solo responde `0000` cuando aplica efecto. Se documenta como prueba de la métrica, no como resultado observado.

## Conclusión

La métrica discrimina por definición y por los casos sintéticos. En la corrida observada, la línea base no incurrió en inconsistencia de estado: incurrió en duplicados. Ambas cosas se reportan por separado.
