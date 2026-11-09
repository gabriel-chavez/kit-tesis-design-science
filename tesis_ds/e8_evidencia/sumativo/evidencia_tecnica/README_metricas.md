# Lectura de `metricas.csv`

**Origen:** corrida técnica del arnés sobre el doble neutro `busa-eval-stub`.
**Fecha de la corrida:** 2026-10-28 20:38:58 -04:00 (ver `metodo_crudo.md` y `linea_base_crudo.md`).
**Artefacto evaluado:** `E7e_MetodoCompensacion`, SHA-256 `ec9b8b6e8d37374ff28f2eb816285571ede997c47ec44a235da829d13dbd10bf` (verificado en `hash_verificacion.txt`).

## 1. Qué contiene

24 filas = 2 casos × 6 escenarios × 2 enfoques (`metodo` y `linea_base`).

| Columna | Significado |
|:--|:--|
| `caso` | CASO-A (`63442 / 4`) y CASO-B (`1598416 / 1`) |
| `participante` | Titular del caso (P1 ↔ CASO-A, P2 ↔ CASO-B). **No indica que la medición se haya hecho en la sesión humana.** |
| `enfoque` | `metodo` (artefacto congelado) o `linea_base` (conducta actual: 5 reintentos con espera fija de 1000 ms, sin verificación de estado) |
| `escenario` | E1–E6 del protocolo de E6 |
| `recursos` | Recursos creados en el doble (efectos externos) |
| `compensaciones` | Compensaciones efectivas ejecutadas en el doble |
| `estado_bitacora` | Estado que declara el cliente (`NoProcesada`, `Procesada`, `Compensada`); en la línea base se registra como `Error` cuando no converge |
| `estado_externo` | Verdad del doble (`SOLICITADO`, `ANULADO`) |
| `decision` | Decisión de K4 en el método; `sin K4` en la línea base |
| `llamadas` | Llamadas externas realizadas |
| `consultas` | Consultas de estado (K3); la línea base no consulta |
| `latencia_ms` | Latencia observada en el doble local; **no** es un SLA |
| `inconsistente` | 1 si el estado declarado no es válido frente a la verdad externa; definición y casos sintéticos en `../prueba_metrica_inconsistencia.md` |
| `cumple` | 1 = el escenario-caso satisface el criterio; 0 = no lo satisface |

## 2. Aclaraciones necesarias

1. **La corrida es técnica y posterior a las sesiones humanas.** Las sesiones de P1 y P2 se realizaron el 2026-10-28 entre 09:00 y 11:55; esta corrida se ejecutó a las 20:38 del mismo día. La columna `participante` identifica el caso, no una observación en sesión.
2. **El control negativo no se ejecutó dentro de las sesiones.** Se ejecutó por caso en esta corrida (`control_negativo_sumativo.md`): sin K5 = 3, con K5 = 1.
3. **`inconsistente` es 0 en las 24 filas.** Eso no significa que la métrica no discrimine, sino que no hubo caso positivo observado. La prueba está en los casos sintéticos de `../prueba_metrica_inconsistencia.md`.
4. **Los duplicados se miden aparte.** Recursos duplicados = 10 y compensaciones duplicadas = 6 en la línea base; no se suman a la inconsistencia de estado.

## 3. Conciliación de totales

| Métrica | Método | Línea base |
|:--|--:|--:|
| Filas | 12 | 12 |
| Recursos | 10 (0 duplicados) | 20 (10 duplicados) |
| Compensaciones | 2 (0 duplicadas) | 8 (6 duplicadas) |
| Consultas de estado | 6 | 0 |
| Llamadas externas | 14 | 38 |
| Escenario-caso que cumplen | 12/12 | 0/12 |

Los "duplicados" se calculan como el exceso sobre un recurso o una compensación por operación; por eso no coinciden con el total de recursos o compensaciones.

## 4. Límites de reproducibilidad

Desde este archivo **no** se puede reproducir la corrida. Faltan:

- marca de tiempo por fila;
- identificador de corrida;
- versión del arnés que generó el CSV;
- versión y estado inicial del doble (semilla);
- el propio arnés: el testbed del repositorio no tiene modo `SUMATIVO` (solo `V1`, `V2`, `V3`, `NEGATIVO`, `VTODAS` y `FORMATIVO`).

Se deja constancia para que la evidencia se lea como observación registrada y no como resultado replicable con el material disponible.

## 5. Advertencia

`metricas.csv`, `metodo_crudo.md`, `linea_base_crudo.md`, `control_negativo_sumativo.md` y `hash_verificacion.txt` son evidencia cruda. No deben editarse: cualquier corrección posterior invalida su valor probatorio.
