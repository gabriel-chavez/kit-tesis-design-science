# E7c. Diseño del artefacto

> **Estado:** cerrada el 2026-10-04. El diseño cubre todos los requisitos, cada decisión tiene alternativa considerada, hay diagrama, definición de reconciliación y trazabilidad requisito → componente → decisión.

## 1. Artefacto y tipología

**Método** para el diseño y la verificación de compensaciones idempotentes en transacciones distribuidas basadas en Sagas con servicios externos no transaccionales, con **principios de diseño** y **criterios de verificación**. Alternativa A (E7a).

## 2. Principio rector

Como el servicio externo no participa de una transacción global, **el estado del servicio externo es la fuente de verdad**, no la memoria del proceso. La bitácora propia acelera y ordena, pero ante cualquier ambigüedad se resuelve consultando a BUSA. Esta es la regla que hace posible la idempotencia cuando la escritura local y la llamada externa no pueden ser atómicas.

## 3. Componentes

| # | Componente | Qué hace | Requisito |
|:--|:--|:--|:--|
| K1 | **Generador de identidad** | Deriva la llave de idempotencia `(TRAMITE_NUMERO, GESTION)` | F1 |
| K2 | **Bitácora de estado** | Registra el estado de cada operación y de cada compensación | F2, F5 |
| K3 | **Verificador de estado** | Consulta BUSA y mapea a procesado / no procesado / indeterminado | F3 |
| K4 | **Motor de decisión** | Decide entre reprocesar, compensar o reconciliar | F4 |
| K5 | **Compensador idempotente** | Ejecuta la compensación identificada por `(operación, motivo)` sin efecto repetido | F5 |
| K6 | **Ordenador de compensaciones** | Aplica orden inverso y respeta dependencias | F6 |
| K7 | **Reconciliador** | Resuelve operaciones de estado indeterminado | F7 |
| K8 | **Verificador de idempotencia** | Escenarios y aserciones que comprueban la propiedad | F8 |
| K9 | **Telemetría de intentos** | Registra intento, causa y resultado | F9 |

## 4. Estructura (diagrama)

```mermaid
flowchart TD
    A[Operación] --> B[K1: llave = TRAMITE_NUMERO + GESTION]
    B --> C[K2: registrar estado en bitácora]
    C --> D[Ejecutar en BUSA]
    D --> E{¿Hay respuesta?}
    E -- Sí, éxito --> F[Estado: procesada]
    E -- Sí, 0003/0004 --> G[K4/K9: reintento con reglas y telemetría]
    E -- No: timeout o pérdida --> H[K3: consultar estado en BUSA]
    H --> I{Estado consultado}
    I -- PAGADO / EJECUTADO --> F
    I -- SOLICITADO / ANULADO / VENCIDO --> J[Estado: no procesada]
    I -- PENDIENTE --> K{¿Más de 20 min?}
    K -- No --> H
    K -- Sí --> L[K7: reconciliar]
    J --> G
    G --> D
    F --> M{¿Requiere compensación?}
    M -- Sí --> N[K5: compensar idempotente por (operación, motivo)]
    N --> O[K6: ordenar compensaciones]
    O --> P[K8: verificar idempotencia y consistencia]
    M -- No --> P
    L --> H
```

## 5. Decisiones de diseño

| # | Decisión | Alternativa considerada | Justificación | Requisito |
|:--|:--|:--|:--|:--|
| Dc1 | Llave de idempotencia **derivada** de `(TRAMITE_NUMERO, GESTION)` | Asignar un UUID antes de llamar | El identificador ya es único por año y sobrevive a los reintentos del cliente; el UUID exigiría persistirlo antes de la llamada y podría perderse | F1 |
| Dc2 | Verificar el estado **ante sospecha** (timeout o respuesta perdida), no siempre | Consultar en toda operación | Reduce latencia en el camino exitoso; el riesgo solo existe cuando falta la respuesta | F3, C5 |
| Dc3 | Tratar PENDIENTE como **indeterminado**, reconsultar hasta 20 minutos y luego reconciliar | Asumir procesado o no procesado | Asumir introduce exactamente el error que la tesis combate: duplicar o dejar sin entregar | F3, F4, F7 |
| Dc4 | Reutilizar `T_QR_SOLICITUD` (estado) y `T_QR_SOLICITUD_EJECUCION_INTENTOS` (intentos) y crear solo una **tabla de idempotencia de compensaciones** | Reutilizar tal cual las existentes / crear toda la bitácora desde cero | Las existentes cubren estado e intentos, pero no la identidad `(operación, motivo)` de la compensación; crear solo lo que falta evita duplicar datos. La tabla se puede crear en desarrollo de inmediato | F2, F5 |
| Dc5 | Compensación identificada por **`(operación, motivo)`** con registro previo | Marcar por marca de tiempo | El par es determinista y reconoce la repetición; la marca de tiempo es frágil ante reintentos casi simultáneos | F5 |
| Dc6 | **No bloquear** el hilo: reemplazar la espera bloqueante por espera no bloqueante o por cola | Mantener `Thread.Sleep` | El bloqueo degrada el servidor bajo carga | C4 |
| Dc7 | Ordenar las compensaciones (inverso y dependencias) | Orden arbitrario | El orden arbitrario deja operaciones inconsistentes; Camunda, por ejemplo, no garantiza orden | F6 |
| Dc8 | La **acción compensatoria disponible en el contexto** es anular el QR no pagado; no existe reversión de pago (condición de contorno) | Suponer una reversión del pago | BUSA solo expone la anulación del QR antes de ser pagado; para un QR ya pagado la acción correcta es continuar (efectivizar), no compensar | F5 |
| Dc9 | La **reconciliación** se ejecuta con los **servicios de Windows** existentes | Construir un planificador nuevo | Ya existe la infraestructura de tareas programadas (`WinServiceSoatConsultaAnulaPagosQR`, `WinServiceSoatAnulaQR`); reutilizarla reduce costo y riesgo | F7, C5 |
| Dc13 | La **compensación se define como agnóstica a la acción**: el método garantiza la idempotencia de cualquier acción compensatoria registrada en la bitácora, y la **anulación del QR no pagado es la exemplar evaluada** | Limitar la compensación a la anulación (A) o definirla como reversión interna (B) | Preserva el nivel de clase (transferibilidad) sin fingir una reversión de pago que BUSA no ofrece; la anulación es la instancia concreta disponible para evaluar | F5 |

## 6. Trazabilidad requisito → componente → decisión

| Requisito | Componente | Decisión |
|:--|:--|:--|
| F1 | K1 | Dc1 |
| F2 | K2 | Dc4 |
| F3 | K3 | Dc2, Dc3 |
| F4 | K4 | Dc3 |
| F5 | K2, K5 | Dc5 |
| F6 | K6 | Dc7 |
| F7 | K7 | Dc3 |
| F8 | K8 | — |
| F9 | K9 | Dc6 |
| C4 | K4, K9 | Dc6 |

## 7. Fundamentación en la base de conocimiento

- La regla "el estado externo es la fuente de verdad" se apoya en la idempotencia y la deduplicación de Helland (R2).
- La estructura de la operación y la compensación proviene de García-Molina y Salem (R1); el registro del estado, de Daraghmi et al. (R10).
- La reconciliación de los indeterminados se apoya en la consistencia eventual de Vogels (R3).
- La verificación de la idempotencia se apoya en Hummer et al. (R4) y en la inyección de fallos de Basiri et al. (R6).

## 7.bis. Reconciliación (definición)

**Diferencia con la verificación:** la verificación es puntual e inmediata, en el camino crítico, sobre una operación que no recibió respuesta. La reconciliación es **masiva y diferida**: revisa el conjunto de operaciones sin estado final confiable (huérfanas) y hace converger el estado local con el de BUSA.

**Operaciones huérfanas:** las que quedaron indeterminadas, las que superaron la ventana de 20 minutos en estado PENDIENTE y las que el proceso abandonó al morir entre la escritura de la bitácora y la llamada a BUSA.

**Contrato de la reconciliación:**

| Estado en BUSA | Acción |
|:--|:--|
| PAGADO / EJECUTADO | Marcar la operación como procesada y, si el paso siguiente no ocurrió, dispararlo de forma idempotente. **No compensar** (no hay reversión de pago). |
| SOLICITADO / ANULADO / VENCIDO | Marcar como no procesada; verificar consistencia de una posible anulación previa; habilitar el reproceso. |
| PENDIENTE (tras la ventana) | No asumir: mantener en la cola con alerta. |

**Propiedades exigidas:** la reconciliación debe ser **idempotente** (ejecutarla dos veces no produce efectos nuevos) y **nunca asumir** un estado que la consulta no confirma. Se apoya en la consistencia eventual (Vogels, R3).

**Disparador:** los servicios de Windows existentes (Dc9).

**Alcance de la compensación (Dc8):** anular el QR no pagado. Para un QR pagado, la resolución es continuar el flujo, no revertir.

## 7.ter. Refinamientos tras la línea base experimental (2026-10-08)

- **Dc10. Imponer unicidad en la base** sobre la llave lógica (índice único sobre `(TRAMITE_SECUENCIAL, T_PAR_SIMPLE_TRAMITE_FK)`), porque hoy hay 44 pares duplicados y ningún índice único. Sin esta condición, la idempotencia no puede garantizarse aunque el servicio lo sea.
- **Dc11. Cubrir las dos rutas de entrada** (RpcA `CPagoSimple` y RpcB `CNPagosQR`), o declarar y resolver su divergencia de reintentos.
- **Dc12. Apoyarse en el servicio de convergencia existente** (`WinServiceSoatConsultaPagosQR`, cada 10 s) para la reconciliación, en lugar de introducir uno nuevo.
- **Nota para E8:** el doble de evaluación **no debe ser idempotente por defecto**, o debe permitir medir el efecto duplicado; de lo contrario el método no puede demostrar su aporte.

## 8. Pendientes de E7c

Resueltos el 2026-10-04:

- BUSA **sí** expone operación de anulación, **solo del QR no pagado**; no hay reversión de pago (nota: `WinServiceSoatEfectivizacion` anula QR no pagados).
- La bitácora se apoya en `T_QR_SOLICITUD` y `T_QR_SOLICITUD_EJECUCION_INTENTOS`; la tabla de idempotencia de compensaciones se puede crear en desarrollo de inmediato.
- La reconciliación se dispara con **servicios de Windows**.
- El cliente **sí** puede formar `(TRAMITE_NUMERO, GESTION)`: `TRAMITE_SECUENCIAL` y `T_PAR_SIMPLE_TRAMITE_FK` están en `T_QR_SOLICITUD`, y la combinación es única por año.

**Condición de contorno a declarar:** el método cubre compensaciones de operaciones **no pagadas**; no cubre reversión de pagos ya ejecutados, porque BUSA no la ofrece.
