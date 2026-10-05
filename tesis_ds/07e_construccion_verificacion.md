# E7e. Construcción y verificación interna

> **Estado:** en curso. Cierra cuando el artefacto esté construido y verificado técnicamente, con evidencia de la verificación, cambios documentados con su razón, y listo para la evaluación formativa. Al ser un método, "construir" significa **formalizar y aplicar**, y la verificación combina comprobaciones técnicas de la exemplar con revisión por expertos.

## 1. Construcción

| Componente | Cómo se construye | Versión | Fecha |
|:--|:--|:--|:--|
| K1 Identidad `(TRAMITE_NUMERO, GESTION)` | Derivación determinista desde `T_QR_SOLICITUD` | V1 | 2026-10-05 |
| K2 Bitácora | Reutilizar `T_QR_SOLICITUD`, `T_QR_SOLICITUD_EJECUCION_INTENTOS` + tabla de idempotencia | V1 | 2026-10-05 |
| K3 Verificador de estado | Consulta al doble de BUSA y mapeo de estados | V1 | 2026-10-05 |
| K4 Motor de decisión | Reglas reprocesar / compensar / reconciliar | V2 | 2026-10-06 |
| K5 Compensador idempotente | Anulación por `(operación, motivo)` | V2 | 2026-10-06 |
| K6 Orden de compensaciones | Orden inverso y dependencias | V2 | 2026-10-06 |
| K9 Telemetría | Registro de intento, causa y resultado | V2 | 2026-10-06 |
| K7 Reconciliador | Integración con el servicio de Windows | V3 | 2026-10-07 |
| K8 Verificador de idempotencia | Escenarios y aserciones | V3 | 2026-10-07 |

Cierre de construcción y verificación interna: **2026-10-08**.

## 2. Entorno de verificación

- **Doble de BUSA** sobre datos sintéticos: reproduce los estados PAGADO/EJECUTADO, SOLICITADO, ANULADO, VENCIDO y PENDIENTE, y provoca los seis fallos.
- El servicio BUSA real **no** se usa durante los experimentos.
- Reproducción de los seis escenarios (E6) con inyección de fallos (Basiri et al., R6).

## 3. Verificación técnica interna

| # | Comprobación | Método | Evidencia esperada |
|:--|:--|:--|:--|
| V1 | La verificación de estado distingue procesado / no procesado / indeterminado | Escenarios 2, 3, 5 | Tabla de resultados por escenario |
| V2 | La repetición de una compensación no produce efecto adicional | Ejecutar la compensación N veces (2, 5, 10) y comparar el estado final | Estado final idéntico en N de N |
| V3 | No se compensa dos veces una operación ya compensada | Reintento y reconciliación sobre la misma operación | Una sola anulación registrada |
| V4 | El estado PENDIENTE se respeta durante la ventana de 20 minutos | Forzar PENDIENTE y observar la reconsulta | Reconsulta dentro de la ventana y sin suposición |
| V5 | La reconciliación es idempotente | Ejecutarla dos veces | Sin efecto adicional |
| V6 | La telemetría registra intento, causa y resultado | Inspección del registro | Registro recuperable por operación |
| V7 | No hay espera bloqueante en el camino crítico | Revisión de código | Ausencia de `Thread.Sleep` bloqueante |
| V8 | La consistencia del método | Revisión de que cada paso cubre un requisito | Trazabilidad sin eslabones huérfanos |

Para inyectar y observar fallos se puede complementar con pruebas basadas en propiedades (Claessen y Hughes, R5) sobre las aserciones de idempotencia.

## 4. Registro de cambios (design rationale)

| Fecha | Versión | Cambio | Razón |
|:--|:--|:--|:--|
| | | | |

## 5. Revisión por expertos (verificación del método)

La verificación técnica comprueba que la exemplar funciona; la revisión por expertos comprueba que el **método es seguible**. Se documentará un recorrido (*walkthrough*) en el que un practicante distinto del autor recorre los pasos con un caso nuevo y reporta ambigüedades.

## 6. Criterio de suficiencia (E7d) para pasar a evaluación

- Tres versiones construidas y verificadas.
- V1–V8 con evidencia y sin hallazgos abiertos sin justificación.
- Un practicante independiente aplica el método sin ayuda.
- Telemetría recuperable.

## 7. Pendientes de E7e

- Reportar el resultado de la construction de V1, V2 y V3 con sus verificaciones.
- Registrar los cambios con su razón.
- Aplicar la revisión por expertos.
- **Pendientes de E6 aún abiertos:** registros de 2025 para I2, línea base experimental de I3, confirmar 1 frente a 2 intentos y justificar el proxy de I1/I4 (fecha comprometida: 2026-10-05).
