# E7b. Requisitos del artefacto

> **Estado:** cerrada el 2026-10-04. Requisitos, métricas, prioridades y trade-offs confirmados; los datos de BUSA, el identificador y el almacén quedaron confirmados.

## 1. Propósito

Derivar del problema (E2) y del diagnóstico (E6) los requisitos que el método debe cumplir, cada uno con su métrica y su fuente en el problema.

## 2. Requisitos funcionales (qué debe hacer el método)

| # | Requisito | Métrica | Fuente en el problema | Prioridad |
|:--|:--|:--|:--|:--|
| F1 | Asignar una **identidad determinista** a cada operación (llave de idempotencia) | Toda operación tiene identidad estable y reproducible | Duplicación posible (E6, riesgo alto) | Debe |
| F2 | **Registrar el estado** de cada operación y de cada compensación | Toda operación y compensación queda registrada con su estado | Sin instrumentación (E6, I3) | Debe |
| F3 | Ante respuesta perdida o timeout, **consultar el estado real en BUSA** antes de decidir | En N de N escenarios de respuesta perdida se consulta el estado | No distingue procesado de no procesado (E6, hallazgo central) | Debe |
| F4 | Decidir de forma determinista entre **reprocesar, compensar o reconciliar** según el estado consultado | Decisión correcta en N de N escenarios | Comportamiento por escenario (E6, sección 6) | Debe |
| F5 | Hacer **idempotente la compensación**: identificarla por (operación, motivo) y repetirla sin efecto adicional | 0 compensaciones con efecto duplicado | Criterio de éxito de utilidad (E6, sección 9) | Debe |
| F6 | **Ordenar** las compensaciones (orden inverso y dependencias) | Sin compensaciones fuera de orden en los escenarios | Sagas (R1, García-Molina y Salem) | Debe |
| F7 | **Reconciliar** operaciones cuyo estado quede indeterminado | 0 operaciones en estado desconocido al cierre | Consistencia eventual (R3, Vogels) | Debe |
| F8 | Producir **criterios y escenarios de verificación** de la idempotencia | Cobertura total de la taxonomía de fallos | D7 (Hummer et al., 2013) y criterio de contribución | Debe |
| F9 | **Registrar evidencia trazable**: número de intento, causa del reintento y resultado | Registro recuperable por operación | I3 no instrumentado (E6) | Debe |

## 3. Requisitos contextuales (en qué condiciones debe funcionar)

| # | Requisito | Métrica | Fuente | Prioridad |
|:--|:--|:--|:--|:--|
| C1 | Operar sobre la implementación propia (.NET/C#, cliente WCF, `Web.config`) sin exigir migrar a un marco de mensajería | Se aplica sin introducir infraestructura de mensajería nueva | Decisión E7a y contexto (E1–E2) | Debe |
| C2 | Depender solo de la **consulta de estado** de BUSA, no de que el servicio ofrezca transacciones | El método funciona con la interfaz de consulta disponible | Hecho habilitante de E7a | Debe |
| C3 | Cubrir los **seis escenarios** fijados en E6 | Cobertura 6 de 6 | Conjunto de escenarios (E6, sección 10) | Debe |
| C4 | **No bloquear** el hilo del servidor (evitar esperas bloqueantes) | Sin `Thread.Sleep` en el camino crítico | Riesgo "bloqueo de hilo" (E6) | Debe |
| C5 | Tener **costo de implementación razonable** | Un equipo puede adoptarlo con los recursos existentes | Criterio de aplicabilidad (E6, contribución) | Debería |
| C6 | Ser **aplicable por practicantes** distintos del autor | Practicantes independientes lo aplican sin ayuda | Criterio de contribución (E2, E6) | Debe |

## 4. Trade-offs declarados

- **Precisión frente a latencia:** consultar el estado siempre reduce el riesgo de duplicado, pero añade una llamada por operación. Decisión: consultar ante sospecha (timeout o respuesta perdida) y, para operaciones huérfanas, reconciliar; no consultar en el camino exitoso.
- **Generalidad frente a efectividad:** un método demasiado general pierde utilidad en el contexto; uno demasiado específico no es transferible. Decisión: mantener el método de nivel de clase (aplicable a integraciones con participantes externos no transaccionales) y documentar sus condiciones de contorno.
- **Cobertura frente a costo:** cubrir todos los modos de fallo exige más mecanismos. Decisión: priorizar los seis escenarios fijados y declarar los restantes como trabajo futuro.
- **Identidad derivada frente a identidad asignada:** derivar la llave del contenido es robusto ante reintentos, pero exige estabilidad del identificador; asignarla en el cliente da control, pero requiere persistirla.

## 5. Cobertura de la tipología

- **Método:** F1–F9 definen los pasos y artefactos del procedimiento.
- **Principios de diseño:** los trade-offs y las reglas de decisión (F4, F5, F7) se articularán como principios en E10.
- **Criterios de verificación:** F8 los hace explícitos.

## 6. Datos confirmados (puerta de E7b, cerrada el 2026-10-04)

- **Estados de BUSA:** PAGADO / EJECUTADO = procesada; SOLICITADO / ANULADO / VENCIDO = no procesada; PENDIENTE = indeterminado, fiable hasta **20 minutos**.
- **Identificador del cliente:** `TRAMITE_NUMERO` + `GESTION` (por ejemplo, `6901XUL` + `2025`), único por año. Es la base de la llave de idempotencia.
- **Almacén:** existe `BD_PASARELA_PAGO.PAGO_SIMPLE.T_QR_SOLICITUD_EJECUCION_INTENTOS`, pero registra la efectivización posterior a la confirmación del pago, proceso **externo a la pasarela**; se puede crear una tabla nueva para la bitácora del método.
- **Prioridades:** confirmadas (F1–F9, C1–C4 y C6 como "Debe"; C5 como "Debería").
- **Trade-offs:** confirmados.
