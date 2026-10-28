# Versión congelada para el ciclo sumativo

**Fecha de congelación:** 2026-10-27
**Artefacto:** `E7e_MetodoCompensacion` (netstandard2.0)
**Binario:** `E7e.MetodoCompensacion.dll`
**SHA-256:** `ec9b8b6e8d37374ff28f2eb816285571ede997c47ec44a235da829d13dbd10bf`
**Tamaño:** 43.520 bytes

## Alcance de la congelación

Esta versión incorpora los rediseños del ciclo formativo:

| Defecto | Cambio | Estado |
|---|---|---|
| D1 | Lock por operación en `CoordinadorMetodo.Procesar` | Resuelto para una instancia |
| D2 | K7 marca `Abandonada` la reserva huérfana; K5 y `ReservasPendientes` incluyen `Abandonada` | Resuelto y verificado |
| D3 | Ventana de 20 minutos completos, sin precisión de segundos | Declarado como condición de contorno |
| D4 | `MarcarAplicada` devuelve `bool`; K5 reintenta y deja la reserva para K7 si falla | Resuelto y verificado |

## Condición de contorno distribuida (D1)

El lock de D1 es **de proceso**. La prueba con 5 hilos demuestra concurrencia dentro de una instancia; **no** simula dos servidores. En un despliegue con dos o más instancias, cada una tiene su propio diccionario y el lock no las coordina. La garantía de idempotencia distribuida **no** proviene de este lock:

> Depende del índice único sobre la llave lógica y de la inserción atómica de **Dc10**. Sin Dc10 aplicado, el método no puede afirmar idempotencia distribuida.

Por tanto, **Dc10 deja de ser un pendiente y pasa a ser condición necesaria del método**. Además, los 44 duplicados de producción deben resolverse antes de crear el índice único.

## Alcance exacto de la afirmación

Los resultados prueban idempotencia **en una instancia** con el doble neutro. **No** prueban idempotencia distribuida. La afirmación correcta para el sumativo y las conclusiones es:

> Idempotencia verificada en una instancia; la garantía distribuida depende de Dc10 (índice único sobre la llave lógica e inserción atómica), que queda como **condición de despliegue**. Sin Dc10 aplicado y sin resolver los 44 duplicados de producción, no se afirma idempotencia distribuida.

No usar "idempotencia distribuida demostrada" ni equivalentes. El lock de D1 es de proceso; la prueba con 5 hilos no simula dos servidores.

## Evidencia de la corrida de congelación

- `tesis_ds/e8_evidencia/entregables_E8_formativo/ciclo_formativo_resultados.md`: 11 escenarios + control negativo, **0 defectos abiertos en los 11 escenarios, una instancia**.
- `tesis_ds/e8_evidencia/entregables_E8_formativo/v3_resultados.md`: regresión V1–V8, todo CUMPLE.
- `tesis_ds/e8_evidencia/entregables_E8_formativo/control_negativo.md`: sin K5 = 3 efectos; con K5 = 1.

## Reproducción

```powershell
# Terminal 1 (desde la raíz del proyecto; ajustar la ruta local del banco de pruebas si es distinta)
cd "tesis_ds\e7e-testbed"
dotnet run --project busa-eval-stub --urls "http://localhost:5098"

# Terminal 2 (desde tesis_ds/e7e-testbed; la evidencia se escribe en tesis_ds/e8_evidencia)
dotnet run --project coordinador -- FORMATIVO "..\e8_evidencia"
dotnet run --project coordinador -- VTODAS   "..\e8_evidencia"
```

El hash permite comprobar que la evidencia corresponde a esta versión. Si el artefacto cambia, la evidencia deja de coincidir y la versión debe volver a congelarse.
