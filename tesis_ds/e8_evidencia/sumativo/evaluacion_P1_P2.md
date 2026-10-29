# Evaluación del ciclo sumativo - P1 y P2

**Fecha de las sesiones:** 2026-10-28
**Observadora:** Micaela Machicado
**Versión evaluada:** `E7e_MetodoCompensacion`, SHA-256 `ec9b8b6e8d37374ff28f2eb816285571ede997c47ec44a235da829d13dbd10bf`
**Doble:** `busa-eval-stub`, neutro, `localhost:5098`
**Participantes:** P1 (Leonel Chamaca Lima, CASO-A) y P2 (Ernesto Gutierrez, CASO-B)


## 1. Datos generales

| Dato | P1 | P2 |
|---|---|---|
| Participante | Leonel Chamaca Lima | Ernesto Gutierrez |
| Perfil | Desarrollador .NET/WCF | Analista/desarrollador sin exposición previa |
| Caso | CASO-A (`63442 / 4`) | CASO-B (`1598416 / 1`) |
| Fecha | 2026-10-28 | 2026-10-28 |
| Inicio | 09:00 | 11:00 |
| Fin | 09:50 | 11:55 |
| Duración | 50 min 00 s | 55 min 00 s |
| Observadora | Micaela Machicado | Micaela Machicado |

## 2. Registro temporal

### P1

| Escenario | Inicio | Fin | Duración |
|---|---|---|---|
| E1 Caída a mitad | 09:00 | 09:08 | 8 min 00 s |
| E2 Timeout | 09:08 | 09:18 | 10 min 00 s |
| E3 Respuesta perdida | 09:18 | 09:25 | 7 min 00 s |
| E4 Solicitud duplicada | 09:25 | 09:34 | 9 min 00 s |
| E5 Reintento tras éxito | 09:34 | 09:40 | 6 min 00 s |
| E6 Repetir anulación | 09:40 | 09:50 | 10 min 00 s |

### P2

| Escenario | Inicio | Fin | Duración |
|---|---|---|---|
| E1 Caída a mitad | 11:00 | 11:09 | 9 min 00 s |
| E2 Timeout | 11:09 | 11:20 | 11 min 00 s |
| E3 Respuesta perdida | 11:20 | 11:28 | 8 min 00 s |
| E4 Solicitud duplicada | 11:28 | 11:38 | 10 min 00 s |
| E5 Reintento tras éxito | 11:38 | 11:45 | 7 min 00 s |
| E6 Repetir anulación | 11:45 | 11:55 | 10 min 00 s |

## 3. Decisiones y observaciones por escenario


### P1 - CASO-A

| Escenario | Respuesta registrada | Resultado técnico del doble | Punto por precisar |
|---|---|---|---|
| E1 | “Primero miraría si quedó algo registrado localmente. Si confirmo que el externo no aplicó nada, reprocesaría con la misma llave. No generaría otra identidad por el simple hecho de reintentar.” | 0 recursos, 0 compensaciones; `Reprocesar` | Ninguno |
| E2 | “No lo tomaría como rechazo. Buscaría el estado por la llave de la operación. Si el externo dice que ya está procesada, cierro con ese resultado; si no puedo determinarlo, lo dejo pendiente en vez de disparar otra llamada a ciegas.” | 1 recurso; `ConfirmarProcesada` | Qué hacer si la consulta de estado también falla |
| E3 | “Aquí sí asumiría que la respuesta se perdió, no necesariamente el efecto. Consulto el estado y correlaciono con la misma identidad. Si encuentro el efecto, confirmo; no duplico.” | 1 recurso; `ConfirmarProcesada` | Ninguno |
| E4 | “Las tres entregas deberían resolver a una operación lógica. Quiero ver que la llave tenga una restricción persistente; en memoria no me bastaría para producción.” | 1 recurso (método); 3 (línea base) | Identifica el límite del doble: la garantía persistente es Dc10 |
| E5 | “Si el éxito está guardado y corresponde a esa llave, devuelvo el resultado conocido. Otra llamada externa no agrega valor.” | 1 recurso; `ConfirmarProcesada` | Ninguno |
| E6 | “Antes de compensar, comprobaría la reserva y el estado de la compensación. Si la respuesta anterior se perdió pero la compensación quedó aplicada, confirmo ese estado. Aquí me importa que la llave de compensación también sea idempotente.” | 1 recurso y 1 compensación; línea base 4 compensaciones | Atomicidad de la llave de compensación |

### P2 - CASO-B

| Escenario | Respuesta registrada | Resultado técnico del doble | Punto por precisar |
|---|---|---|---|
| E1 | “Entiendo que primero tengo que confirmar si alcanzó a procesar. Si no lo hizo, vuelvo a intentar con el identificador original. ¿La bitácora me muestra eso?” | 0 recursos; `Reprocesar` | Dónde consultar la evidencia (operativa) |
| E2 | “Mi primera reacción sería reintentar, pero la guía dice que no sabemos si se aplicó. Entonces paro y consulto el estado. Me falta saber qué hago cuando esa consulta queda también sin respuesta.” | 1 recurso; `ConfirmarProcesada` | Segundo timeout / estado indeterminado |
| E3 | “Si el externo hizo el cambio, debería aparecer en la consulta aunque mi sistema no recibió la respuesta. Buscaría por la misma llave y no por una nueva.” | 1 recurso; `ConfirmarProcesada` | Ninguno |
| E4 | “Si recibo tres veces la misma solicitud, espero una sola operación. Revisaría la bitácora para asegurar que el sistema no creó tres recursos.” | 1 recurso (método); 3 (línea base) | Ninguno |
| E5 | “Con un resultado exitoso anterior, devolvería ese resultado. No veo por qué llamar otra vez al servicio.” | 1 recurso; `ConfirmarProcesada` | Ninguno |
| E6 | “Confirmaría si la compensación ya se hizo antes de repetirla. Me confunde un poco la palabra reserva: quisiera ver un ejemplo de cómo cambia su estado.” | 1 recurso y 1 compensación; línea base 4 compensaciones | Ambigüedad conceptual en “reserva” |

## 4. Cuestionario posterior

| Ítem | P1 | P2 |
|---|---|---|
| 1. Claridad desde el primer escenario | 4 | 3 |
| 2. Supe qué decisión tomar | 4 | 4 |
| 3. Bitácora y estado externo fáciles de interpretar | 3 | 3 |
| 4. Regla de idempotencia se entendió sin explicación adicional | 4 | 3 |
| 5. Esfuerzo razonable | 3 | 4 |
| 6. Confiaría en el método para operar un pago real | 2 | 2 |
| 7. Ambigüedades menores | 3 | 3 |
| 8. Recomendaría el método a otro equipo | 4 | 4 |
| **Promedio** | **3,375** | **3,25** |

**Promedio conjunto:** 3,31 / 5.

## 5. Entrevista breve

| Pregunta | P1 | P2 |
|---|---|---|
| Comprensión | “Consultar el estado y conservar una identidad estable; repetir solo cuando la evidencia indique que no hubo efecto.” | “Reviso si el pago ya ocurrió y uso la misma llave para que el reintento no cree otro.” |
| Decisión | “El timeout no me dice qué pasó del otro lado.” | “No puedo inferir que falló solo porque mi llamada venció.” |
| Confianza | “Índice único, inserción atómica y pruebas concurrentes contra un entorno controlado.” | “Ver un registro de ejemplo y confirmar que no se crean duplicados bajo concurrencia.” |
| Límites | “No compensaría si el estado no permite demostrar qué operación se aplicó.” | “Si no puedo encontrar la operación o la llave no es confiable, no avanzaría automáticamente.” |
| Doble | “Es determinista; no reproduce concurrencia, latencia variable ni garantías del proveedor.” | “El doble responde igual cada vez; el proveedor real puede tardar o fallar de otra manera.” |
| Coste | “Más consultas y trazabilidad, a cambio de no duplicar efectos.” | “Hay más pasos que un reintento, pero entiendo el motivo.” |
| Mejora | “Ejemplos de estados desconocidos y qué hacer cuando la consulta también falla.” | “Definir qué significa reserva y mostrar qué hacer cuando el estado queda desconocido.” |

## 6. Métricas humanas

| Métrica | P1 | P2 | Total / promedio |
|---|---|---|---|
| Tareas respondidas | 6/6 | 6/6 | 12/12 |
| Tareas sin ayuda | **No medido** | **No medido** | **No evaluable** |
| Ayudas solicitadas | No registradas | No registradas | **No medido** |
| Puntos críticos E2/E6 | 2/2 | 2/2 | 4/4 |
| Tiempo total | 50 min | 55 min | 52,5 min |
| Cuestionario (promedio) | 3,375 | 3,25 | 3,31 |
| Ambigüedades declaradas | 2 | 3 | 5 |

**Regla de conteo de ambigüedades.** Se cuenta como ambigüedad la laguna de guía o de terminología. El «límite del doble» (P1-E4) se registra como hallazgo, no como ambigüedad. Resultado: P1 = 2, P2 = 3, total 5.

**Puntos críticos del protocolo:** E2 (consulta de estado antes de reintentar) y E6 (verificar reserva/compensación antes de repetir). P1 y P2: 2/2 cada uno. La identidad estable y la distinción respuesta/efecto se observaron, pero no se cuentan como los dos puntos críticos del protocolo. Las ambigüedades son de guía o terminología, no fallos de decisión en los seis escenarios.

**Orden real:** ambas sesiones siguieron E1→E6. No hubo aleatorización; el orden fijo se declara como amenaza a la validez interna.

**CASO-A y Dc10:** la corrida parte de 0 recursos; las 2 filas históricas no se sembraron en el doble. El caso se usó como llave lógica, no como prueba de Dc10.


## 7. Hallazgos

1. **Ambos aplican los dos principios centrales:** identidad estable y consulta de estado antes de repetir.
2. **P1 identifica el límite del doble:** “en memoria no me bastaría para producción”. Refuerza Dc10 como condición de despliegue.
3. **P2 necesita apoyo operativo:** dónde consultar la evidencia y qué significa “reserva”. Es una mejora de material, no del método.
4. **Ambos comparten una laguna:** qué hacer cuando la consulta de estado también falla (segundo fallo). La guía debe explicitar la regla de estado indeterminado.
5. **Confianza baja en producción (2/5 en ambos):** coherente con que Dc10 no está aplicado. No es un fallo del método; es una condición de despliegue declarada.
6. **Tiempo:** 50 y 55 min; dentro del rango previsto. La ayuda no se midió; la hoja dice “no evaluable”.

