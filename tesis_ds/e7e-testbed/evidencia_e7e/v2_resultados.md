# V2 Experimental — resultados con evidencia

| Aspecto | Detalle |
|---|---|
| Versión | V2 Experimental |
| Componentes activos | K1, K2, K3, K4, K5, K6, K9 |
| Componentes inactivos | K7, K8 |
| Escenarios | 1, 4, 6 |
| Comprobaciones | V2 (crítica), V6, V7 |
| Doble | neutro (no deduplica) |
| Artefacto ejecutado | `E7e_MetodoCompensacion` (netstandard2.0) |
| Fecha real de ejecución | 2026-10-13 |

================================================================
ESCENARIO 1 — CAÍDA A MITAD DE TRANSACCIÓN (aborta antes del efecto)
================================================================
  **Traza del método:**
    - intento 1: externo respondio=False resultCode=ERROR_TRANSPORTE falloTransporte=Unable to read data from the transport connection: Se ha forzado la interrupción de una conexión existente por el host remoto..
    - K3: estado=SOLICITADO resultCode=0000 unidadesQr=0
    - K4 decide: Reprocesar
    - el externo no tiene recurso: reprocesar es seguro

  - Estado final en bitácora : NoProcesada
  - Categoría K3             : NoProcesado
  - Intentos                 : 1
  - Consultas de estado      : 1
  - Decisiones de K4         : Reprocesar

  **Telemetría K9:**
      t=10:00:00 intento=1 salto=Externo causa=fallo de transporte ERROR_TRANSPORTE resultado=INDETERMINADA decision=Reconciliar obs=Unable to read data from the transport connection: Se ha forzado la interrupción de una conexión existente por el host remoto..
      t=10:00:00 intento=1 salto=Externo causa=sin recurso en el externo resultado=REPROCESABLE decision=Reprocesar obs=estado=SOLICITADO resultCode=0000 unidadesQr=0

  **Verdad externa (observable, lo que el cliente no ve):**
    operacion=OP-3001-G1
      llaveMetodo=3001|1
      estado=SOLICITADO
      unidadesQr=0 efectosGenerarQr=0 compensacionesAplicadas=0
      [salto externo] invocaciones=1;efectosAplicados=0;compensacionesAplicadas=0
        #1 GenerarQR modo=CaidaMitad efecto=False ABORT_ANTES_DE_EFECTO
    

  Recursos en el externo: 0  (0 = el efecto NO se aplicó)
  >> Decisión esperada: `Reprocesar`. El externo no tiene recurso, así que
     repetir no puede duplicar nada.

================================================================
ESCENARIO 4 — SOLICITUD DUPLICADA (misma llave, sin fallo)
================================================================
  Se invoca la MISMA operación lógica 5 veces. Sin K5 el doble neutro
  crearía 5 recursos. Con el método, el comportamiento depende de K3:
  la 2.ª invocación ya encuentra el recurso y debe confirmarlo sin repetir.

    invocación #1: exito=True idQr=OP-3004-G1-QR1 recursos=1 decisiones=Ninguna
    invocación #2: exito=True idQr=OP-3004-G1-QR1 recursos=1 decisiones=ConfirmarProcesada
    invocación #3: exito=True idQr=OP-3004-G1-QR1 recursos=1 decisiones=ConfirmarProcesada
    invocación #4: exito=True idQr=OP-3004-G1-QR1 recursos=1 decisiones=ConfirmarProcesada
    invocación #5: exito=True idQr=OP-3004-G1-QR1 recursos=1 decisiones=ConfirmarProcesada

    operacion=OP-3004-G1
      llaveMetodo=3004|1
      estado=SOLICITADO
      unidadesQr=1 efectosGenerarQr=1 compensacionesAplicadas=0
      [salto externo] invocaciones=1;efectosAplicados=1;compensacionesAplicadas=0
        unidad OP-3004-G1-QR1 estado=SOLICITADO efectos=[]
        #1 GenerarQR modo=Ninguno efecto=True EFECTO_GENERAR_QR idQr=OP-3004-G1-QR1
    

  >> Recursos totales en el externo: 1

================================================================
ESCENARIO 6 — FALLO AL REPETIR UNA ANULACIÓN (aplica y aborta la respuesta)
================================================================
 Paso 1 — generación: exito=True idQr=OP-3006-G1-QR1

 Paso 2 — compensación: el externo aplica la anulación y aborta la respuesta.

   compensación #1: ejecutada=True omitidaPorIdempotencia=False respondio=False falloTransporte=True
   compensación #2: ejecutada=False omitidaPorIdempotencia=True respondio=False falloTransporte=False
   compensación #3: ejecutada=False omitidaPorIdempotencia=True respondio=False falloTransporte=False

    operacion=OP-3006-G1
      llaveMetodo=3006|1
      estado=ANULADO
      unidadesQr=1 efectosGenerarQr=1 compensacionesAplicadas=1
      [salto externo] invocaciones=2;efectosAplicados=2;compensacionesAplicadas=1
        unidad OP-3006-G1-QR1 estado=ANULADO efectos=[CANCEL_QR#1]
        #1 GenerarQR modo=Ninguno efecto=True EFECTO_GENERAR_QR idQr=OP-3006-G1-QR1
        #2 AnularQR modo=RespuestaPerdidaPostEfecto efecto=True EFECTO_ANULAR_QR idQr=OP-3006-G1-QR1 (invocacionAnulacion=1)
    

  >> Compensaciones aplicadas EN EL EXTERNO: 1
  >> Compensaciones aplicadas EN LA BITÁCORA: 1

================================================================
COMPROBACIÓN V2 (CRÍTICA) — repetir la compensación deja el estado final idéntico
================================================================

Se repite la compensación N veces **con K5 activo**. El observable es el contador
del externo: si el doble deduplicara, la comprobación no probaría nada. Como el doble
es neutro, cualquier compensación que se ejecute de más aparece en el contador.

| N | Compensaciones efectivas (externo) | Compensaciones en bitácora | Estado final externo | ¿Idéntico a N=1? |
|---|---|---|---|---|
| 1 | 1 | 0 | ANULADO | referencia |
| 2 | 1 | 1 | ANULADO | Sí |
| 5 | 1 | 1 | ANULADO | Sí |
| 10 | 1 | 1 | ANULADO | Sí |

**Resultado de V2:** CUMPLE — con K5 activo, el externo registra exactamente 1 compensación efectiva para N = 1, 2, 5 y 10, y el estado final es idéntico.

================================================================
COMPROBACIÓN V6 — la telemetría registra intento, causa y resultado
================================================================

  Eventos registrados para OP-6006-G1:
      t=10:00:00 intento=1 salto=Externo causa=fallo de transporte ERROR_TRANSPORTE resultado=INDETERMINADA decision=Reconciliar obs=Unable to read data from the transport connection: Se ha forzado la interrupción de una conexión existente por el host remoto..
      t=10:00:00 intento=1 salto=Externo causa=recurso confirmado en el externo resultado=CONFIRMADA_SIN_REPETIR decision=ConfirmarProcesada
      t=10:00:00 intento=1 salto=Externo causa=compensacion AnularQR ejecutada resultado=APLICADA resultCode=0000 decision=Compensar

  Total de eventos: 3
  Eventos con intento, causa y resultado completos: True/3
  Eventos con salto identificado: 3/3
  **Resultado de V6:** CUMPLE

================================================================
COMPROBACIÓN V7 — sin espera bloqueante en el camino crítico
================================================================

Revisión de código del artefacto `E7e_MetodoCompensacion`:

| Patrón bloqueante | Presente | Ubicación |
|---|---|---|
| `Thread.Sleep` | **No** | — |
| `.Wait()` / `.Result` sobre tarea | **No** | — |
| `lock` | **No** | — |
| `ManualResetEvent` / `Semaphore` | **No** | — |

Contraste con el sistema real, que sí bloquea:

| Sistema | Patrón | Ubicación |
|---|---|---|
| `CPagoSimple` (RpcA) | `Thread.Sleep(milliseconds)` en el bucle de reintento | `CPagoSimple.cs` L186 |
| `CNPagosQR` (RpcB) | `Thread.Sleep(milliseconds)` en el `catch` | `CNPagosQR.cs` reintento |

El artefacto **no** introduce espera: la reconsulta de `PENDIENTE` y la
reconciliación se diferieren al proceso programado existente (Dc12).

**Resultado de V7:** CUMPLE (verificado por inspección del código fuente del artefacto)

---

**Procedencia de esta evidencia**

| Campo | Valor |
|---|---|
| Generado por | arnés `coordinador` (`Program.cs`), sin lógica del método |
| Fecha y hora de ejecución | 2026-10-14 22:14:49 -04:00 |
| Comando | `dotnet run -- VTODAS ..\evidencia_e7e` |
| Artefacto ejecutado | `E7e.MetodoCompensacion` |
| SHA-256 del artefacto | `d00087332fddb581af8617bbcbcb7f07189d9f24221b6812fc9a6c8e5725c612` |
| Doble | `busa-eval-stub` en `http://localhost:5098`, neutro (no deduplica) |

El SHA-256 permite comprobar que esta evidencia corresponde a esta versión del
artefacto. Si el artefacto cambia, el hash cambia y la evidencia deja de coincidir.
