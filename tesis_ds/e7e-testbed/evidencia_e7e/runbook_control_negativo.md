# Control negativo — prueba de neutralidad del doble de evaluación

**Etapa:** E7e · Diseño del doble para E8
**Fecha real de ejecución:** 2026-10-13
**Artefacto ejecutado:** `E7e_MetodoCompensacion` (netstandard2.0)
**Servicio real BUSA invocado:** 0 veces
**Script reproducible:** `e7e-testbed/coordinador/Program.cs`, nivel `NEGATIVO`

---

## 1. Qué demuestra este control

Que el doble de evaluación **no es idempotente**. Cada invocación de compensación aplica efecto.

Si el doble deduplicara, la comprobación V2 no probaría nada: el estado final sería idéntico
porque el simulador lo garantiza, no porque el método lo evite. La diferencia entre las dos
columnas de la sección 4 es, por tanto, exactamente lo que aporta el método.

## 2. Procedimiento

Se repite la compensación N veces sobre una misma operación, con dos configuraciones del
artefacto:

| Parte | Configuración | Descripción |
|---|---|---|
| 1 | `Compensador.Activo = false` | K5 no registra ni consulta la tabla de idempotencia. Es el comportamiento sin método |
| 2 | `Compensador.Activo = true` | El método completo |

En ambos casos el doble externo es el mismo proceso, con los mismos parámetros. Lo único que
cambia es el artefacto.

## 3. Resultados

### Parte 1 — control negativo: K5 desactivado

| N | Compensaciones efectivas en el EXTERNO | Estado final | ¿Más de un efecto? |
|---|---|---|---|
| 1 | 1 | ANULADO | No |
| 2 | 2 | ANULADO | Sí |
| 5 | 5 | ANULADO | Sí |
| 10 | 10 | ANULADO | Sí |

**Resultado del control negativo: CONFIRMADO — el doble NO deduplica.**

Con K5 desactivado, N invocaciones producen N efectos de compensación en el externo. La
proporción es exactamente 1:1.

### Parte 2 — contraprobación: K5 activado

| N | Compensaciones efectivas en el EXTERNO | Estado final | ¿Un solo efecto? |
|---|---|---|---|
| 1 | 1 | ANULADO | Sí |
| 2 | 1 | ANULADO | Sí |
| 5 | 1 | ANULADO | Sí |
| 10 | 1 | ANULADO | Sí |

**Resultado de la contraprobación: CONFIRMADO — K5 evita el efecto duplicado.**

## 4. Comparación

| N | Sin K5 (efectos) | Con K5 (efectos) | Diferencia |
|---|---|---|---|
| 1 | 1 | 1 | 0 |
| 2 | 2 | 1 | −1 |
| 5 | 5 | 1 | −4 |
| 10 | 10 | 1 | −9 |

**Conclusión.** La diferencia entre ambas columnas es lo que aporta el método. El doble no
aporta nada de eso: es neutro por construcción, y por eso la diferencia no puede atribuirse
al simulador.

## 5. Condición de satisfactory

El control negativo se considera satisfecho cuando se cumplen las dos condiciones:

| Condición | Estado |
|---|---|
| Sin K5, N = 2, 5 y 10 producen **más de un** efecto en el externo | **Cumplida** (2, 5, 10) |
| Con K5, N = 1, 2, 5 y 10 producen **exactamente un** efecto | **Cumplida** (1, 1, 1, 1) |

Ambas se cumplen. El código de salida del ejecutor es 0 solo si se cumplen las dos.

## 6. Reproducción

```powershell
# Terminal 1 — doble de evaluación neutro
cd "C:\Users\lchavez\Documents\Proyecto predeterminado\e7e-testbed\busa-eval-stub"
dotnet build
dotnet run --urls "http://localhost:5098"

# Terminal 2 — control negativo
cd "C:\Users\lchavez\Documents\Proyecto predeterminado\e7e-testbed\coordinador"
dotnet build
dotnet run --no-build -- NEGATIVO "..\evidencia_e7e"
```

Para detener el doble: `Stop-Process -Name BusaEvalStub`

## 7. Advertencia de alcance

Este control demuestra la **neutralidad del doble**, que es un requisito del diseño de la
evaluación. **No demuestra** que el método sea correcto: esa demostración la dan las
comprobaciones V2 y V3 sobre los escenarios 1 a 6, y ambas están en `v2_resultados.md` y
`v3_resultados.md`.

Tampoco demuestra nada sobre el comportamiento de BUSA real, que nunca fue invocado.