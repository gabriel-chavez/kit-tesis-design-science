# Matriz de análisis del ciclo sumativo

**Versión:** `E7e_MetodoCompensacion`, SHA-256 `ec9b8b6e8d37374ff28f2eb816285571ede997c47ec44a235da829d13dbd10bf`
**Fecha de sesiones:** 2026-10-28
**Observador:** Jose Luis Quispe
**Participantes:** P1 (Leonel Chamaca Lima, CASO-A) y P2 (Anahi Quisbert, CASO-B)


## 1. Matriz técnica

### 1.1 Método vs. línea base

| Métrica | Método | Línea base | Diferencia |
|---|---|---|---|
| Escenario-caso que cumplen | 12/12 | 0/12 | +12 |
| Recursos duplicados | 0 | 10 | −10 |
| Compensaciones duplicadas | 0 | 6 | −6 |
| Estados inconsistentes | 0 | 0 | 0 |
| Consultas de estado | 6 | 0 | +6 |
| Llamadas externas | 14 | 38 | −24 |

### 1.2 Detalle por escenario (valores idénticos en CASO-A y CASO-B)

| Escenario | Método recursos / compensaciones | Decisión método | Línea base recursos / compensaciones | Cumple método | Cumple línea base |
|---|---|---|---|---|---|
| E1 Caída a mitad | 0 / 0 | Reprocesar | 0 / 0 | Sí | No |
| E2 Timeout | 1 / 0 | ConfirmarProcesada | 2 / 0 | Sí | No |
| E3 Respuesta perdida | 1 / 0 | ConfirmarProcesada | 2 / 0 | Sí | No |
| E4 Solicitud duplicada | 1 / 0 | ConfirmarProcesada | 3 / 0 | Sí | No |
| E5 Reintento tras éxito | 1 / 0 | ConfirmarProcesada | 2 / 0 | Sí | No |
| E6 Repetir anulación | 1 / 1 | Compensar | 1 / 4 | Sí | No |

## 2. Matriz humana (piloto reportado, no verificado)

| Escenario | P1 decisión | P1 tiempo | P1 punto por precisar | P2 decisión | P2 tiempo | P2 punto por precisar |
|---|---|---|---|---|---|---|
| E1 Caída a mitad | Confirmar registro local y reprocesar con la misma llave | 8 min | — | Confirmar si procesó y reintentar con el identificador original | 9 min | Dónde consultar la evidencia |
| E2 Timeout | Consultar estado; no reintentar a ciegas | 10 min | Qué hacer si la consulta también falla | Consultar estado; no reintentar | 11 min | Segundo timeout / estado indeterminado |
| E3 Respuesta perdida | Consultar por la misma llave; confirmar si hay efecto | 7 min | — | Buscar el efecto por la misma llave | 8 min | — |
| E4 Solicitud duplicada | Una operación lógica; exige restricción persistente | 9 min | Límite del doble: en memoria no basta | Una sola operación; revisar bitácora | 10 min | — |
| E5 Reintento tras éxito | Devolver el resultado exitoso conocido | 6 min | — | Reutilizar el éxito previo | 7 min | — |
| E6 Repetir anulación | Revisar reserva y estado; llave de compensación idempotente | 10 min | Atomicidad de la llave | Confirmar si la compensación ya se hizo | 10 min | Ambigüedad de “reserva” |

## 3. Resumen humano

| Métrica | P1 | P2 | Total / promedio |
|---|---|---|---|
| Tareas respondidas | 6/6 | 6/6 | 12/12 |
| Tareas sin ayuda | **No medido** | **No medido** | **No evaluable** |
| Ayudas solicitadas | No registradas | No registradas | **No medido** |
| Puntos críticos E2/E6 | 2/2 | 2/2 | 4/4 |
| Duración total | 50 min | 55 min | 52,5 min |
| Cuestionario (promedio) | 3,375 | 3,25 | 3,31 |
| Ambigüedades declaradas | 2 | 3 | 5 |

**Regla de conteo de ambigüedades.** Se cuenta como ambigüedad la laguna de guía o de terminología. El «límite del doble» (P1-E4) se registra como hallazgo, no como ambigüedad. Resultado: P1 = 2, P2 = 3, total 5.

**Puntos críticos del protocolo:** E2 (consulta de estado antes de reintentar) y E6 (verificar reserva/compensación antes de repetir). P1 y P2: 2/2 cada uno. La identidad estable y la distinción respuesta/efecto se observaron, pero no se cuentan como los dos puntos críticos del protocolo.

## 4. Hallazgos

1. Las hojas reportan identidad estable y consulta de estado en ambos.
2. P1 identifica que la garantía en memoria no basta para producción; refuerza Dc10.
3. P2 pide apoyo operativo (dónde consultar, qué es “reserva”); es mejora de material.
4. Ambos comparten la laguna del segundo fallo de consulta; la guía debe explicitar la regla.
5. Confianza baja en producción (2/5 en ambos), coherente con Dc10 no aplicado.
6. Tiempos 50 y 55 min; la ayuda no se midió.
7. **Orden real:** ambas sesiones E1→E6; no hubo aleatorización; el orden fijo se declara como amenaza a la validez interna.
8. **CASO-A:** la corrida parte de 0 recursos; las 2 filas históricas no se sembraron; Dc10 no se ejercitó.

## 5. Regla de análisis

Con n = 2 no se estiman tasas poblacionales. Se reportan conteos exactos, dirección del efecto y mecanismos. La generalización es analítica. La idempotencia distribuida no se afirma: depende de Dc10.