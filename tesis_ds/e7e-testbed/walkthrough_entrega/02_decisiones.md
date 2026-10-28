# Decisiones de diseño (Dc1–Dc13)

Estas decisiones ya están tomadas. El practicante no tiene que elegir entre alternativas ni
inventar criterios: solo aplicarlas al caso que se le asigne y decir si le resultaron claras.

| # | Decisión | Alternativa que se descartó | Por qué | Requisito |
|---|---|---|---|---|
| Dc1 | La llave de idempotencia se **deriva** de `(TRAMITE_NUMERO, GESTION)` | Asignar un UUID antes de llamar | El identificador ya es único por año y sobrevive a los reintentos del cliente. Un UUID habría que persistirlo antes de la llamada y se puede perder | F1 |
| Dc2 | Se verifica el estado **ante sospecha** (timeout o respuesta perdida), no siempre | Consultar en toda operación | Baja la latencia en el camino exitoso. El riesgo solo existe cuando falta la respuesta | F3, C5 |
| Dc3 | `PENDIENTE` se trata como **indeterminado**: reconsultar hasta 20 minutos y después reconciliar | Asumir que se procesó o que no | Asumir introduce justo el error que el método combate: duplicar, o dejar la operación sin entregar | F3, F4, F7 |
| Dc4 | Reutilizar `T_QR_SOLICITUD` (estado) y `T_QR_SOLICITUD_EJECUCION_INTENTOS` (intentos), y crear **solo** una tabla de idempotencia de compensaciones | Reutilizar las existentes tal cual, o crear toda la bitácora de cero | Las existentes cubren estado e intentos pero no la identidad `(operación, motivo)` de la compensación. Crear solo lo que falta evita duplicar datos | F2, F5 |
| Dc5 | La compensación se identifica por **`(operación, motivo)`** y se registra **antes** de ejecutar | Marcar por marca de tiempo | El par es determinista y reconoce la repetición. La marca de tiempo es frágil si hay reintentos casi simultáneos | F5 |
| Dc6 | **No bloquear** el hilo. Reemplazar la espera bloqueante por espera no bloqueante o por cola | Mantener `Thread.Sleep` | El bloqueo degrada el servidor bajo carga | C4 |
| Dc7 | Ordenar las compensaciones, respetando el orden inverso y las dependencias | Orden arbitrario | El orden arbitrario deja operaciones inconsistentes | F6 |
| Dc8 | La acción compensatoria disponible **en este contexto** es anular el QR no pagado. No existe reversión de pago | Suponer una reversión del pago | BUSA solo expone la anulación del QR antes de que se pague. Para un QR ya pagado lo correcto es continuar, no revertir | F5 |
| Dc9 | La reconciliación se ejecuta con los **servicios de Windows existentes** | Armar un planificador nuevo | La infraestructura de tareas programadas ya existe; reutilizarla baja el costo y el riesgo | F7, C5 |
| Dc10 | **Imponer unicidad en la base** sobre la llave lógica: índice único sobre `(TRAMITE_SECUENCIAL, T_PAR_SIMPLE_TRAMITE_FK)` | Confiar solo en que el servicio sea idempotente | Sin esta condición la idempotencia no se puede garantizar aunque el servicio lo sea. Hoy hay 44 pares duplicados y ningún índice único | F1 |
| Dc11 | **Cubrir las dos rutas de entrada** (RpcA `CPagoSimple` y RpcB `CNPagosQR`), o declarar y resolver su divergencia de reintentos | Cubrir una sola ruta | Si las dos rutas reintentan distinto, la idempotencia de una no protege a la otra | F4 |
| Dc12 | Apoyarse en el servicio de convergencia que ya existe (`WinServiceSoatConsultaPagosQR`, cada 10 s) para la reconciliación | Introducir un servicio nuevo | No se agrega infraestructura; el disparador ya está en operación | F7 |
| Dc13 | La compensación se define **agnóstica a la acción**: el método garantiza la idempotencia de cualquier acción compensatoria registrada en la bitácora. La anulación del QR no pagado es el exemplar evaluado | Limitar la compensación a la anulación, o definirla como reversión interna | Mantiene el nivel de método (transferible) sin fingir una reversión de pago que BUSA no ofrece | F5 |

## Condición de contorno

El método cubre compensaciones de operaciones **no pagadas**. No cubre la reversión de un pago
ya ejecutado, porque BUSA no la ofrece. Para un QR ya pagado, la resolución correcta es
continuar el flujo, no revertir.

---

**Procedencia**

| Campo | Valor |
|---|---|
| Fuente | `07c_diseno_artefacto.md` (cerrado el 2026-10-04), refinamientos Dc10–Dc12 del 2026-10-08 |
| Extraído para | paquete de entrega del walkthrough (E7e) |
