# E8 - Resultados de la evaluación sumativa

**Fecha de sesiones:** 2026-10-28
**Artefacto:** `E7e_MetodoCompensacion` (netstandard2.0)
**SHA-256:** `ec9b8b6e8d37374ff28f2eb816285571ede997c47ec44a235da829d13dbd10bf`
**Doble:** `busa-eval-stub`, neutro, `localhost:5098`
**Observador:** Jose Luis Quispe
**Participantes:** P1 (Leonel Chamaca Lima, CASO-A) y P2 (Anahi Quisbert, CASO-B)
**Documentos firmados:** consentimientos y declaración del observador, 2026-10-30 (`firmas/escaneos/`); salvedad §4, campo «¿Pidió ayuda?» no evaluable.


## 1. Medición técnica

### 1.1 Método congelado

| Caso | Escenario | Recursos | Compensaciones | Estado bitácora | Estado externo | Decisión K4 | Cumple |
|---|---|---|---|---|---|---|---|
| A | E1 Caída a mitad | 0 | 0 | NoProcesada | SOLICITADO | Reprocesar | Sí |
| A | E2 Timeout | 1 | 0 | Procesada | SOLICITADO | ConfirmarProcesada | Sí |
| A | E3 Respuesta perdida | 1 | 0 | Procesada | SOLICITADO | ConfirmarProcesada | Sí |
| A | E4 Solicitud duplicada | 1 | 0 | Procesada | SOLICITADO | ConfirmarProcesada | Sí |
| A | E5 Reintento tras éxito | 1 | 0 | Procesada | SOLICITADO | ConfirmarProcesada | Sí |
| A | E6 Repetir anulación | 1 | 1 | Compensada | ANULADO | Compensar | Sí |
| B | E1 Caída a mitad | 0 | 0 | NoProcesada | SOLICITADO | Reprocesar | Sí |
| B | E2 Timeout | 1 | 0 | Procesada | SOLICITADO | ConfirmarProcesada | Sí |
| B | E3 Respuesta perdida | 1 | 0 | Procesada | SOLICITADO | ConfirmarProcesada | Sí |
| B | E4 Solicitud duplicada | 1 | 0 | Procesada | SOLICITADO | ConfirmarProcesada | Sí |
| B | E5 Reintento tras éxito | 1 | 0 | Procesada | SOLICITADO | ConfirmarProcesada | Sí |
| B | E6 Repetir anulación | 1 | 1 | Compensada | ANULADO | Compensar | Sí |

**Resultado:** 12/12. 0 recursos duplicados; 0 compensaciones duplicadas; 0 estados inconsistentes.

### 1.2 Línea base re-ejecutada

| Caso | Escenario | Recursos | Compensaciones | Estado registrado | Cumple |
|---|---|---|---|---|---|
| A | E1 Caída a mitad | 0 | 0 | Error | No |
| A | E2 Timeout | 2 | 0 | Procesada | No |
| A | E3 Respuesta perdida | 2 | 0 | Procesada | No |
| A | E4 Solicitud duplicada | 3 | 0 | Procesada | No |
| A | E5 Reintento tras éxito | 2 | 0 | Procesada | No |
| A | E6 Repetir anulación | 1 | 4 | Compensada | No |
| B | E1 Caída a mitad | 0 | 0 | Error | No |
| B | E2 Timeout | 2 | 0 | Procesada | No |
| B | E3 Respuesta perdida | 2 | 0 | Procesada | No |
| B | E4 Solicitud duplicada | 3 | 0 | Procesada | No |
| B | E5 Reintento tras éxito | 2 | 0 | Procesada | No |
| B | E6 Repetir anulación | 1 | 4 | Compensada | No |

**Resultado:** 0/12.

> El flag `inconsistente` es 0 en las 24 filas. La definición y los casos sintéticos que prueban que la métrica discrimina están en `prueba_metrica_inconsistencia.md`. Los duplicados se reportan aparte.

### 1.3 Comparación

| Métrica | Método | Línea base |
|---|---|---|
| Escenario-caso que cumplen | 12/12 | 0/12 |
| Recursos duplicados | 0 | 10 |
| Compensaciones duplicadas | 0 | 6 |
| Estados inconsistentes | 0 | 0 |
| Consultas de estado | 6 | 0 |
| Llamadas externas | 14 | 38 |

### 1.4 Control negativo

| Caso | Sin K5 | Con K5 | Resultado |
|---|---|---|---|
| CASO-A | 3 | 1 | Confirmado |
| CASO-B | 3 | 1 | Confirmado |

**Limitación:** el control se ejecutó por caso, no dentro de cada sesión humana. Es un control técnico post-hoc; no sustituye el control de sesión.

## 2. Sesiones

### 2.1 Datos generales

| Dato | P1 | P2 |
|---|---|---|
| Participante | Leonel Chamaca Lima | Anahi Quisbert |
| Caso | CASO-A (`63442 / 4`) | CASO-B (`1598416 / 1`) |
| Fecha | 2026-10-28 | 2026-10-28 |
| Inicio / fin | 09:00 / 09:50 | 11:00 / 11:55 |
| Duración | 50 min | 55 min |
| Observador | Jose Luis Quispe | Jose Luis Quispe |

### 2.2 Decisiones por escenario

| Escenario | P1 | P2 |
|---|---|---|
| E1 | Confirmar registro local y reprocesar con la misma llave | Confirmar si procesó y reintentar con el identificador original |
| E2 | Consultar estado; no reintentar a ciegas | Consultar estado; no reintentar |
| E3 | Consultar por la misma llave; confirmar efecto | Buscar el efecto por la misma llave |
| E4 | Una operación lógica; exigir restricción persistente | Una sola operación; revisar bitácora |
| E5 | Devolver el resultado exitoso conocido | Reutilizar el éxito previo |
| E6 | Revisar reserva y estado; llave de compensación idempotente | Confirmar si la compensación ya se hizo |

### 2.3 Métricas

| Métrica | P1 | P2 | Total / promedio |
|---|---|---|---|
| Tareas respondidas | 6/6 | 6/6 | 12/12 |
| Tareas sin ayuda | **No medido** | **No medido** | **No evaluable** |
| Ayudas solicitadas | No registradas | No registradas | **No medido** |
| Puntos críticos E2/E6 | 2/2 | 2/2 | 4/4 |
| Duración | 50 min | 55 min | 52,5 min |
| Cuestionario (promedio) | 3,375 | 3,25 | 3,31 |
| Ambigüedades | 2 | 3 | 5 |

**Regla de conteo de ambigüedades.** Se cuenta como ambigüedad la laguna de guía o de terminología. El «límite del doble» (P1-E4) se registra como hallazgo, no como ambigüedad. Resultado: P1 = 2, P2 = 3, total 5.


### 2.4 Cuestionario

| Ítem | P1 | P2 |
|---|---|---|
| 1 Claridad | 4 | 3 |
| 2 Decisión | 4 | 4 |
| 3 Bitácora/estado externo | 3 | 3 |
| 4 Idempotencia | 4 | 3 |
| 5 Esfuerzo | 3 | 4 |
| 6 Confianza en producción | 2 | 2 |
| 7 Ambigüedades menores | 3 | 3 |
| 8 Recomendación | 4 | 4 |
| **Promedio** | **3,375** | **3,25** |

## 3. Hallazgos

> Los hallazgos 1–6 provienen de un **piloto con consentimiento y declaración firmados** (2026-10-30). No existen notas crudas independientes: las hojas de observación son el único registro. La coincidencia con el documento previo se explica por el tesista como cambio de formato sin traza documental, y la independencia del observador es parcial (misma organización; observó y registró, no validó de forma independiente). El campo «¿Pidió ayuda?» es **no evaluable** por la salvedad §4. Se leen como reporte atestiguado por el observador, no como resultado certificado. Detalle en `trazabilidad_evidencia_humana.md`.

1. Ambos aplican identidad estable y consulta de estado.
2. P1 identifica que la garantía en memoria no basta para producción; refuerza Dc10.
3. P2 pide apoyo operativo (dónde consultar, qué es “reserva”); es mejora de material.
4. Ambos comparten la laguna del segundo fallo de consulta; la guía debe explicitar la regla.
5. Confianza baja en producción (2/5 en ambos), coherente con Dc10 no aplicado.
6. Tiempos 50 y 55 min; la ayuda no se midió (la hoja dice “no evaluable”).
7. **Orden real:** ambas sesiones siguieron E1→E6; no hubo aleatorización; el orden fijo se declara como amenaza a la validez interna.
8. **CASO-A y Dc10:** la corrida arranca con 0 recursos; las 2 filas históricas no se sembraron en el doble. El caso se usó como llave lógica, no como prueba de Dc10.


## 4. Salidas crudas y hash

- `evidencia_tecnica/metricas.csv`
- `evidencia_tecnica/metodo_crudo.md`
- `evidencia_tecnica/linea_base_crudo.md`
- `evidencia_tecnica/control_negativo_sumativo.md`
- `evidencia_tecnica/hash_verificacion.txt`

## 5. Alcance

Los resultados prueban el comportamiento en una instancia con el doble neutro. No prueban idempotencia distribuida. La garantía distribuida depende de **Dc10** (índice único e inserción atómica), que queda como condición de despliegue. No se afirma “idempotencia distribuida demostrada”.
