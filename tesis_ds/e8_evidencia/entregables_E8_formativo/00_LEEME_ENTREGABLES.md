# Entregables E8 - Ciclo formativo

**Fecha:** 2026-10-27
**Versión congelada:** `E7e_MetodoCompensacion` (netstandard2.0)
**SHA-256:** `ec9b8b6e8d37374ff28f2eb816285571ede997c47ec44a235da829d13dbd10bf`

## Contenido

| Archivo | Qué contiene |
|---|---|
| `version_congelada.md` | Hash, alcance, cambios D1-D4 y condición de despliegue Dc10 |
| `ciclo_formativo_rediseño.md` | Hallazgos, rediseño y alcance exacto de la afirmación |
| `ciclo_formativo_resultados.md` | Evidencia de los 11 escenarios + control negativo, con cronología de iteraciones |
| `v3_resultados.md` | Regresión V1-V8 |
| `control_negativo.md` | Sin K5 = 3 efectos; con K5 = 1 |

## Reproducción

Rutas canónicas (detalle en `version_congelada.md` § Reproducción):

```powershell
# Terminal 1
cd "tesis_ds\e7e-testbed"
dotnet run --project busa-eval-stub --urls "http://localhost:5098"

# Terminal 2, desde tesis_ds\e7e-testbed\coordinador
dotnet run -- FORMATIVO "..\..\e8_evidencia\reproduccion"
dotnet run -- VTODAS   "..\..\e8_evidencia\reproduccion"
```

La salida va a `tesis_ds/e8_evidencia/reproduccion/` y no sobrescribe este paquete. La línea «Comando» que aparece dentro de `control_negativo.md` y `v3_resultados.md` registra la corrida original (`..\evidencia_e7e`, desde `coordinador`) y se conserva como parte de la evidencia; no es la ruta de reproducción.

## Frase para tesis y defensa

> Idempotencia verificada en una instancia; la garantía distribuida depende de Dc10 (índice único e inserción atómica), que queda como condición de despliegue.

No usar "idempotencia distribuida demostrada".

## Estado

- Ciclo formativo cerrado.
- 0 defectos abiertos en los 11 escenarios, una instancia.
- Dc10 abierto: resolver los 44 duplicados y crear el índice único.
- Siguiente: ciclo sumativo con 2-3 practicantes, casos distintos y comparación contra la línea base de E6.
