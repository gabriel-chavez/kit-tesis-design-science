# Doble de evaluación para E8 — diseño y especificación

**Etapa:** E7e (diseño) · **Destino:** E8 (evaluación)
**Fecha real:** 2026-10-13
**Implementación:** `e7e-testbed/busa-eval-stub/` (.NET 10, ASP.NET Core mínimo)
**Estado:** **implementado y verificado** por el control negativo

---

## 1. Requisito que motiva el diseño

> El doble de evaluación **no debe ser idempotente por defecto**.

Si el doble deduplicara, el método no podría demostrar su aporte: el estado final sería
idéntico porque el simulador lo garantiza, no porque K5 lo evite. Esta condición es la que
convierte la evaluación en una prueba y no en una tautología.

## 2. Propiedades obligatorias y cómo se cumplen

| # | Propiedad | Implementación | Verificación |
|---|---|---|---|
| 1 | **Neutro** | Cada `GenerarQR` crea una `UnidadQr` nueva con su propio `IdQr`. Cada `AnularQR` incrementa `compensacionesAplicadas`, sin importar el estado previo | Control negativo: N invocaciones → N efectos |
| 2 | **No verifica ni compensa por el cliente** | Solo expone `ConsultarQR` y aplica operaciones. No tiene clave de deduplicación ni memoria de invocaciones previas | Inspección del código |
| 3 | **Contadores observables** por operación y por salto | `invocaciones`, `efectosAplicados`, `compensacionesAplicadas`, agrupados por `Dictionary<string, Contadores>` | `admin/volcado/{id}` |
| 4 | **Estados mapeables a K3**, incluido `PENDIENTE` | `SOLICITADO`, `PAGADO`, `ANULADO`, `VENCIDO`, `PENDIENTE`; el último con `PendienteDesde` | Comprobación V4 |
| 5 | **Inyección de fallos reproducible** | `admin/fallo?modo=X&id=Y&veces=N`; el número de ocurrencias es determinista | Escenarios 1–6 |
| 6 | **Reloj inyectable** | `admin/reloj/avanzar?minutos=N`; la ventana de 20 min se agota sin esperar en tiempo real | Comprobación V4 |
| 7 | **Aislado y determinista** | `localhost:5098`; datos `OP-*`; BUSA real nunca invocado | Verificación de entorno |

## 3. Contrato del doble

### 3.1 Operaciones del servicio externo

| Endpoint | Equivalente real | Efecto | Respuesta |
|---|---|---|---|
| `POST /busa/_QR_001` | Generar QR | **Crea una unidad nueva** | `resultCode`, `idQr`, `estado` |
| `POST /busa/_QR_002` | Consultar QR | **Ninguno** | `estado`, `pendienteDesde`, `unidadesQr`, `anulacionesAplicadas` |
| `POST /busa/_QR_003` | Anular QR | **Aplica cancelación** | `resultCode`, `estado` |

### 3.2 Operaciones de control

| Endpoint | Efecto |
|---|---|
| `POST /admin/fallo?modo=X&id=Y&veces=N` | Programa N ocurrencias del modo X para la operación Y |
| `POST /admin/reloj/avanzar?minutos=N` | Adelanta el reloj virtual |
| `POST /admin/pagar?id=Y` | Marca la operación como pagada por el canal externo |
| `POST /admin/vencer?id=Y` | Lleva la operación a `VENCIDO` |
| `POST /admin/pendiente?id=Y` | Lleva la operación a `PENDIENTE` |
| `POST /admin/reiniciar` | Vacía el almacén y reinicia el reloj |
| `GET /admin/volcado/{id}` | Volcado legible: contadores, unidades e invocaciones |
| `GET /admin/todas` | Lista de identidades presentes |

## 4. Modos de fallo

| Modo | ¿Aplica efecto? | ¿Qué hace con la respuesta? | Escenario |
|---|---|---|---|
| `CaidaMitad` | **No** | `ctx.Abort()` | 1 |
| `TimeoutPostEfecto` | **Sí** | Retiene 30 s; el cliente agota su tiempo de espera | 2 |
| `RespuestaPerdidaPostEfecto` | **Sí** | `ctx.Abort()` | 3, 6 |
| `DejarPendiente` | No | Responde `PENDIENTE` con marca de tiempo | Ventana de 20 min |

Los tres primeros reproducen fallos **de transporte reales**, no excepciones simuladas en el
cliente: el socket se corta o la respuesta no llega. Eso es lo que hace fiable la inyección.

## 5. Modelo de neutralidad

```csharp
// GenerarQr: SIEMPRE crea una unidad nueva.
var unidad = new UnidadQr
{
    IdQr = $"{operacionLogica}-QR{op.Unidades.Count + 1}",
    OperacionLogica = operacionLogica
};
op.Unidades.Add(unidad);
c.EfectosAplicados++;

// AnularQr: NO mira el estado previo.
c.CompensacionesAplicadas++;
c.EfectosAplicados++;
```

Dos comentarios explican por qué el código es así:

| En el código | Razón |
|---|---|
| «NEUTRO: SIEMPRE crea una unidad nueva» | Refleja que el estado previo no altera el efecto |
| «NEUTRO: sin importacion del estado previo. Siempre aplica» | Documenta la ausencia deliberada de una rama condicional que anularía el control negativo |

## 6. Contraste con el doble de la línea base (E6)

**El doble de E6 no puede reutilizarse para E8.** La diferencia es sustantiva:

| Aspecto | Doble de E6 (línea base) | Doble de E8 (evaluación) |
|---|---|---|
| Deduplicación | **Sí** — `NO_EFFECT_ALREADY_CANCELLED` | **No** — cada invocación aplica |
| Estados | `NoProcesada` / `Generada` / `Pagada` / `Anulada` | `SOLICITADO` / `PAGADO` / `ANULADO` / `VENCIDO` / `PENDIENTE` |
| Reloj | Reloj real | **Virtual**, inyectable |
| Ventana de `PENDIENTE` | No modelada | 20 minutos, simulado |
| Contadores | `efectosGenerarQr`, `anulacionesAplicadas` | `invocaciones`, `efectosAplicados`, `compensacionesAplicadas` por salto |
| Uso previsto | Caracterizar el sistema **sin** método | Medir el **método** |

Con el doble de E6, repetir una compensación dejaría el contador en 1 aunque el método
funcionara. La comprobación V2 habría pasado por la razón equivocada.

## 7. Verificación de la neutralidad

### 7.1 Procedimiento

Repetir la compensación N veces, una vez con K5 desactivado y otra con K5 activado.

### 7.2 Resultado

| N | Sin K5 (efectos en el externo) | Con K5 (efectos en el externo) | Diferencia |
|---|---|---|---|
| 1 | 1 | 1 | 0 |
| 2 | 2 | 1 | −1 |
| 5 | 5 | 1 | −4 |
| 10 | 10 | 1 | −9 |

**Conclusión:** el doble es neutro (la columna izquierda crece con N) y el método evita el
duplicado (la columna derecha es constante). La diferencia es exactamente la contribución
del método, y no puede atribuirse al simulador.

Detalle en `runbook_control_negativo.md`.

## 8. Desviaciones respecto del servicio real

Declaradas para que la evaluación no se presente como más fiel de lo que es.

| # | Desviación | Consecuencia para E8 |
|---|---|---|
| 1 | **JSON en lugar de XML firmado.** El servicio real usa XML con `SignatureValue` y `X509Certificate` | El doble no reproduce la validación de firma. Esa validación queda fuera del alcance del método |
| 2 | **Transporte HTTP en lugar de WCF sobre HTTPS** | No se reproduce el comportamiento de `basicHttpBinding` ni los valores por defecto de timeout |
| 3 | **Estado local en memoria**, sin persistencia | El doble se reinicia entre corridas. Determinista, pero sin resistencia a reinicios |
| 4 | **Sin control de concurrencia** | Los escenarios son secuenciales |
| 5 | **Un solo servicio externo** | No modela competencia entre réplicas ni particionado de clave |

## 9. Qué NO debe usarse el doble para

| Uso | Motivo |
|---|---|
| Concluir sobre el comportamiento de BUSA real | Es un simulador neutral, no una reproducción |
| Medir volumen o tasa de fallos reales | Los fallos se inyectan; no emergen |
| Validar la firma digital | No hay firma |
| Sustituir el entorno de pruebas autorizado | No reproduce WCF, HTTPS ni la base de datos |

## 10. Consideraciones para E8

| # | Consideración |
|---|---|
| 1 | **Mantener la neutralidad.** Cualquier cambio que añada deduplicación al doble invalidaría V2 y V3. Si se necesita un simulador idempotente, debe ser un **segundo** doble, no una modificación de este |
| 2 | **El control negativo debe repetirse en cada corrida de E8.** Es la única prueba de que el doble sigue siendo neutro |
| 3 | **Añadir el segundo exemplar de compensación** (reversión de venta, `Ven06Revertir`) cuando se implemente en el artefacto |
| 4 | **Reproducir la matriz de V8 sobre un doble más fiel** (con firma y WCF) para acotar la desviación 1 y 2 |

---

## 11. Trazabilidad con los requisitos de evaluación

| Requisito de la evaluación | Dónde se satisface |
|---|---|
| Doble neutro | `AlmacenExterno.GenerarQr` / `AnularQr` |
| No verifica ni compensa por el cliente | `AlmacenExterno` sin clave de deduplicación |
| Contadores por operación y por salto | `OperacionExterna.ContadoresPorSalto` |
| Estados mapeables a K3, con `PENDIENTE` y ventana | `EstadoExterno` + `PendienteDesde` + `admin/reloj` |
| Inyección de fallos reproducible | `ModoFallo` + `admin/fallo` |
| Reloj inyectable | `AlmacenExterno.Avanzar` + `admin/reloj` |
| Aislado y determinista | `localhost:5098`, datos `OP-*`, reloj virtual |
| **Control negativo** | `runbook_control_negativo.md`, resultado en `control_negativo.md` |

---
