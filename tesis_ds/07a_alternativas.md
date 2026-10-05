# E7a. Solución conceptual y alternativas

> **Estado:** borrador. Cierra cuando haya al menos dos alternativas reales con trade-offs explícitos y una decisión justificada del tesista.

## 1. Objetivo

Convertir la solución conceptual de E2 en dos o más **alternativas reales de solución** (no variantes cosméticas de la misma) y elegir una con trade-offs documentados, coherente con la tipología declarada (**método + principios + criterios de verificación**) y con la base de conocimiento (E4).

## 2. Alternativas

### Alternativa A — Método de compensación idempotente con llave de idempotencia, bitácora y verificación de estado

- **Idea:** cada operación recibe una identidad determinista; antes de ejecutar o compensar se consulta una bitácora de estado; la compensación se identifica por el mismo par (operación, motivo) y es reconocible/repetible sin efecto adicional; ante respuesta perdida, se **verifica el estado real** consultando al servicio antes de decidir.
- **Principios que la sustentan:** Sagas y compensación (R1); idempotencia y deduplicación (R2, Helland); registro de estado de la saga (R10, Daraghmi).
- **Supuestos:** el servicio externo expone (o permite construir) una consulta de estado; la operación tiene un identificador estable.
- **Costo:** medio (diseño de identidad, bitácora y consulta de verificación).
- **Riesgo:** depende de que exista una operación de consulta; si no, hay que deducir el estado.
- **Cobertura:** alta sobre los seis escenarios, en especial "procesado con respuesta perdida".
- **Novedad:** alta; ataca justo el vacío identificado en E5 (diseño y verificación de la idempotencia).

### Alternativa B — Método apoyado en la semántica de un marco de mensajería (máquina de estados, outbox, deduplicación de eventos)

- **Idea:** reutilizar los mecanismos del marco (MassTransit, NServiceBus o similar): máquina de estados de saga, outbox y deduplicación por identificador de mensaje, y añadir la compensación como paso explícito.
- **Principios:** saga y outbox (R7, R8, MassTransit/NServiceBus); idempotencia de receptor (R2).
- **Supuestos:** se adopta el marco y se migra el flujo; el servicio externo sigue siendo no transaccional.
- **Costo:** alto (introducir infraestructura de mensajería); riesgo de sobrealcance sobre el sistema legado.
- **Cobertura:** alta para duplicados internos; **baja** para la respuesta perdida del servicio externo, que el outbox no resuelve.
- **Novedad:** baja; es adoptar tecnología existente, no generar conocimiento nuevo.

### Alternativa C — Método de reconciliación periódica sin compensación explícita

- **Idea:** no prevenir el duplicado en el momento; detectar y corregir divergencias por reconciliación posterior (consulta masiva de estados y ajuste).
- **Principios:** consistencia eventual (R3, Vogels).
- **Supuestos:** existe consulta de estado y un proceso de reconciliación por lotes.
- **Costo:** bajo en diseño, alto en operación (conciliación continua).
- **Riesgo:** **no evita el efecto duplicado**, solo lo repara después; el criterio de utilidad "0 compensaciones con efecto duplicado" no se cumpliría en el momento del incidente.
- **Novedad:** media; la reconciliación es conocida.

### Alternativa D — No diseñar; adoptar un marco existente tal cual (descartada)

- **Idea:** configurar Temporal, Camunda o Axon y delegar todo en el marco.
- **Motivo de descarte:** no genera conocimiento transferible; es resolución de ingeniería, no investigación. Se conserva como referencia de comparación, no como alternativa de tesis.

## 3. Matriz de decisión

| Criterio | A | B | C |
|:--|:--|:--|:--|
| Factibilidad | Alta | Media | Alta |
| Novedad | Alta | Baja | Media |
| Costo | Medio | Alto | Bajo/Medio |
| Riesgo | Medio | Medio-alto | Alto (no previene) |
| Sustento teórico | Alto (R1, R2, R10) | Alto (R7, R8) | Medio (R3) |
| Cobertura de los seis escenarios | Alta | Media | Baja en el momento |
| Coherencia con la tipología (método) | Alta | Media | Media |

## 4. Decisión (puerta de E7a, cerrada el 2026-10-04)

El tesista elige la **Alternativa A**, con la verificación de estado como componente explícito.

**Justificación (palabras del tesista):** es la alternativa que aborda directamente el problema identificado en E6 —el sistema no distingue una operación no procesada de una operación procesada cuya respuesta se perdió—; consultar el estado real antes de reintentar evita la ejecución duplicada.

**Hecho habilitante:** se confirma que BUSA **permite consultar el estado** de una operación. La verificación permitirá diferenciar, ante una respuesta perdida, entre una operación **procesada**, una **no procesada** y una cuyo estado **requiere reconciliación**.

**Descartadas:** B (novedad baja y no resuelve la respuesta perdida del servicio externo), C (no previene el duplicado, solo lo repara) y D (no genera conocimiento). Se conservan como referencia de comparación.
