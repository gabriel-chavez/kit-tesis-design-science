# E7d. Plan de construcción y versiones

> **Estado:** cerrada el 2026-10-05. MVP, versiones y criterio de suficiencia confirmados; el doble de BUSA queda establecido como medio de verificación.

## 1. Producto mínimo viable (MVP)

El MVP **no es un sistema completo**: es el mínimo artefacto que permite probar los principios de diseño y resolver el problema.

1. **Formalización del método:** etapas, entradas y salidas, reglas de decisión (reprocesar / compensar / reconciliar), precondiciones y criterios de entrada/salida.
2. **Una exemplar aplicada** en el entorno controlado que cubra los seis escenarios e implemente los componentes K1–K9:
   - K1 identidad `(TRAMITE_NUMERO, GESTION)`;
   - K2 bitácora (reutilizando `T_QR_SOLICITUD` y `T_QR_SOLICITUD_EJECUCION_INTENTOS` más la tabla de idempotencia de compensaciones);
   - K3 verificador de estado de BUSA;
   - K4 motor de decisión;
   - K5 compensador idempotente por `(operación, motivo)`;
   - K6 orden de compensaciones;
   - K7 reconciliador disparado por servicio de Windows;
   - K8 verificador de idempotencia (escenarios y aserciones);
   - K9 telemetría de intentos.
3. **Criterio del MVP:** debe permitir concluir si (a) la verificación de estado elimina la ambigüedad procesado / no procesado / PENDIENTE y (b) la repetición de una compensación no produce efecto adicional.

## 2. Versiones y pregunta de diseño de cada una

| Versión | Contenido | Pregunta de diseño | Escenarios |
|:--|:--|:--|:--|
| V1 Exploratoria | Identidad, bitácora y verificador de estado | ¿La consulta de estado resuelve la ambigüedad ante timeout y respuesta perdida? | 2, 3, 5 |
| V2 Experimental | V1 + compensación idempotente, orden y telemetría | ¿La compensación repetida deja el estado final idéntico al de una ejecución única? | 1, 4, 6 |
| V3 Operacional | V2 + reconciliación por servicio de Windows y verificación completa | ¿El método es aplicable por un practicante y suficiente ante los seis escenarios? | 1–6 |

**Incluido:** todo lo anterior. **Excluido:** la reversión de pagos ya ejecutados, porque BUSA no la ofrece (condición de contorno de Dc8).

## 3. Criterio de suficiencia

El artefacto está listo para la evaluación sumativa cuando:

1. las tres versiones están construidas y verificadas internamente;
2. las aserciones de idempotencia y consistencia pasan en los seis escenarios;
3. un practicante independiente aplica el método sin ayuda del autor;
4. la telemetría de intentos queda registrada y es recuperable.

## 4. Matriz de riesgos de construcción

| Riesgo | Probabilidad | Impacto | Mitigación |
|:--|:--|:--|:--|
| No poder usar BUSA real en el entorno controlado | Alta | Alto | Simulador de BUSA que reproduzca sus estados y fallos sobre datos sintéticos |
| Permisos para crear la tabla de idempotencia | Baja | Medio | Creación habilitada en desarrollo; declarar el requisito de producción |
| Tiempo insuficiente (una semana en el cronograma) | Media | Alto | Priorizar V1 y V2; V3 puede recortarse a la reconciliación mínima |
| Practicantes independientes no disponibles | Media | Alto | Identificarlos ahora y reservar fecha (E8) |
| Sesgo del constructor | Media | Alto | Separar diseño y evaluación; el autor no evalúa su propio método |

## 5. Trazabilidad versión → pregunta → escenarios

La secuencia V1 → V2 → V3 respeta la exigencia de maestría: un ciclo formativo con rediseño (V1 y V2 con sus hallazgos) antes del ciclo sumativo (V3 evaluado).

## 6. Pendientes de E7d

- Confirmar el MVP y el plan de versiones.
- Confirmar el enfoque del simulador de BUSA para el entorno controlado.
- Confirmar la prioridad y el alcance mínimo de V3.
- Fijar fechas de V1, V2 y V3.
