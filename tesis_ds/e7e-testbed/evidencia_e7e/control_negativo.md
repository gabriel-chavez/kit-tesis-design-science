# Control negativo — prueba de neutralidad del doble

**Propiedad que se quiere demostrar:** el doble de evaluación NO es idempotente.
Cada invocación de compensación aplica efecto. Si el doble deduplicara, el método
no podría demostrar su aporte, porque no habría nada que evitar.

**Fecha real de ejecución:** 2026-10-13
**Artefacto:** `E7e_MetodoCompensacion`


================================================================
PARTE 1 — CONTROL NEGATIVO: K5 DESACTIVADO
================================================================

Se repite la compensación N veces con `Compensador.Activo = false` (K5 no registra
ni consulta la tabla de idempotencia: es el comportamiento sin método).

| N | Compensaciones efectivas en el EXTERNO | Estado final | ¿Más de un efecto? |
|---|---|---|---|
| 1 | 1 | ANULADO | No |
| 2 | 2 | ANULADO | Sí |
| 5 | 5 | ANULADO | Sí |
| 10 | 10 | ANULADO | Sí |

**Resultado del control negativo:** CONFIRMADO — el doble NO deduplica


================================================================
PARTE 2 — CONTRAPROBACIÓN: K5 ACTIVADO
================================================================

Misma operación, mismas N repeticiones, con K5 activo.

| N | Compensaciones efectivas en el EXTERNO | Estado final | ¿Un solo efecto? |
|---|---|---|---|
| 1 | 1 | ANULADO | Sí |
| 2 | 1 | ANULADO | Sí |
| 5 | 1 | ANULADO | Sí |
| 10 | 1 | ANULADO | Sí |

**Resultado de la contraprobación:** CONFIRMADO — K5 evita el efecto duplicado


================================================================
COMPARACIÓN DE AMBAS PARTES
================================================================

| N | Sin K5 (efectos) | Con K5 (efectos) | Diferencia |
|---|---|---|---|
| 1 | 1 | 1 | 0 |
| 2 | 2 | 1 | -1 |
| 5 | 5 | 1 | -4 |
| 10 | 10 | 1 | -9 |

**Conclusión:** la diferencia entre ambas columnas es exactamente lo que aporta
el método. El doble no aporta nada de eso: es neutro por construcción, y por eso
la diferencia no puede atribuirse al simulador.

---

**Procedencia de esta evidencia**

| Campo | Valor |
|---|---|
| Generado por | arnés `coordinador` (`Program.cs`), sin lógica del método |
| Fecha y hora de ejecución | 2026-10-14 22:14:52 -04:00 |
| Comando | `dotnet run -- VTODAS ..\evidencia_e7e` |
| Artefacto ejecutado | `E7e.MetodoCompensacion` |
| SHA-256 del artefacto | `d00087332fddb581af8617bbcbcb7f07189d9f24221b6812fc9a6c8e5725c612` |
| Doble | `busa-eval-stub` en `http://localhost:5098`, neutro (no deduplica) |

El SHA-256 permite comprobar que esta evidencia corresponde a esta versión del
artefacto. Si el artefacto cambia, el hash cambia y la evidencia deja de coincidir.
