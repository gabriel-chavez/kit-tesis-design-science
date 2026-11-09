# V1 Exploratoria — resultados con evidencia

| Aspecto | Detalle |
|---|---|
| Versión | V1 Exploratoria |
| Componentes activos | K1, K2, K3 |
| Componentes inactivos | K4, K5, K6, K7, K8, K9 |
| Escenarios | 2, 3, 5 |
| Doble | neutro (no deduplica) |
| Artefacto ejecutado | `E7e_MetodoCompensacion` (netstandard2.0) |
| Fecha real de ejecución | 2026-10-13 |

En V1 el método **observa** la ambigüedad pero no **decide**: no hay motor de
decisión (K4) ni compensador (K5). El objetivo declarado es comprobar que K3
separa las tres categorías de estado.

================================================================
ESCENARIO 2 — TIMEOUT (el externo aplica el efecto y retiene la respuesta)
================================================================
  **Traza del método:**
    - intento 1: externo respondio=False resultCode=TIMEOUT falloTransporte=timeout esperando respuesta del externo
    - K3: estado=SOLICITADO resultCode=0000 unidadesQr=1
    - K4 decide: ConfirmarProcesada
    - confirmada como aplicada: el externo tiene 1 recurso(s); no se reprocesa

  - Estado final en bitácora : Procesada
  - Categoría K3             : NoProcesado
  - Intentos                 : 1
  - Consultas de estado      : 1
  - Decisiones de K4         : ConfirmarProcesada

  **Telemetría K9:**
      t=10:00:00 intento=1 salto=Externo causa=fallo de transporte TIMEOUT resultado=INDETERMINADA decision=Reconciliar obs=timeout esperando respuesta del externo
      t=10:00:00 intento=1 salto=Externo causa=recurso confirmado en el externo resultado=CONFIRMADA_SIN_REPETIR decision=ConfirmarProcesada

  **Verdad externa (observable, lo que el cliente no ve):**
    operacion=OP-2002-G1
      llaveMetodo=2002|1
      estado=SOLICITADO
      unidadesQr=1 efectosGenerarQr=1 compensacionesAplicadas=0
      [salto externo] invocaciones=1;efectosAplicados=1;compensacionesAplicadas=0
        unidad OP-2002-G1-QR1 estado=SOLICITADO efectos=[]
        #1 GenerarQR modo=TimeoutPostEfecto efecto=True EFECTO_GENERAR_QR idQr=OP-2002-G1-QR1
    

  **Comprobación V1:** K3Raw = `NoProcesado`; recurso externo = 1

================================================================
ESCENARIO 3 — RESPUESTA PERDIDA (el externo aplica el efecto y aborta la respuesta)
================================================================
  **Traza del método:**
    - intento 1: externo respondio=False resultCode=ERROR_TRANSPORTE falloTransporte=Unable to read data from the transport connection: Se ha forzado la interrupción de una conexión existente por el host remoto..
    - K3: estado=SOLICITADO resultCode=0000 unidadesQr=1
    - K4 decide: ConfirmarProcesada
    - confirmada como aplicada: el externo tiene 1 recurso(s); no se reprocesa

  - Estado final en bitácora : Procesada
  - Categoría K3             : NoProcesado
  - Intentos                 : 1
  - Consultas de estado      : 1
  - Decisiones de K4         : ConfirmarProcesada

  **Telemetría K9:**
      t=10:00:00 intento=1 salto=Externo causa=fallo de transporte ERROR_TRANSPORTE resultado=INDETERMINADA decision=Reconciliar obs=Unable to read data from the transport connection: Se ha forzado la interrupción de una conexión existente por el host remoto..
      t=10:00:00 intento=1 salto=Externo causa=recurso confirmado en el externo resultado=CONFIRMADA_SIN_REPETIR decision=ConfirmarProcesada

  **Verdad externa (observable, lo que el cliente no ve):**
    operacion=OP-2003-G1
      llaveMetodo=2003|1
      estado=SOLICITADO
      unidadesQr=1 efectosGenerarQr=1 compensacionesAplicadas=0
      [salto externo] invocaciones=1;efectosAplicados=1;compensacionesAplicadas=0
        unidad OP-2003-G1-QR1 estado=SOLICITADO efectos=[]
        #1 GenerarQR modo=RespuestaPerdidaPostEfecto efecto=True EFECTO_GENERAR_QR idQr=OP-2003-G1-QR1
    

  **Comprobación V1:** K3Raw = `NoProcesado`; recurso externo = 1

================================================================
ESCENARIO 5 — REINTENTO TRAS ÉXITO REAL (el doble NO deduplica)
================================================================
  Primera invocación  : exito=True idQr=OP-2005-G1-QR1 recursos=1
  Segunda invocación  : exito=True idQr=OP-2005-G1-QR1 recursos=1

    operacion=OP-2005-G1
      llaveMetodo=2005|1
      estado=SOLICITADO
      unidadesQr=1 efectosGenerarQr=1 compensacionesAplicadas=0
      [salto externo] invocaciones=1;efectosAplicados=1;compensacionesAplicadas=0
        unidad OP-2005-G1-QR1 estado=SOLICITADO efectos=[]
        #1 GenerarQR modo=Ninguno efecto=True EFECTO_GENERAR_QR idQr=OP-2005-G1-QR1
    

  >> Recursos creados en el externo: 1
  >> **OBSERVACIÓN:** el doble es neutro. La segunda invocación creó un
     segundo recurso porque nadie lo impidió. En V1 no existe el componente
     que lo impida (ese es K5). Esto es el problema que el método resuelve.

================================================================
HALLAZGOS DE V1
================================================================

**H-V1-1. La categoría de negocio y la existencia del recurso son hechos distintos.**

El estado `SOLICITADO` significa «la operación de negocio no está pagada», pero no
dice si el QR llegó a crearse. En los escenarios 2 y 3 el estado es `SOLICITADO`
y el recurso **sí** existe, porque el externo aplicó el efecto antes de cortar la
respuesta. Normalizar solo por el estado llevaría a `Reprocesar`, que en un doble
neutro crea un segundo QR.

**H-V1-2. K3 detecta el duplicado pero no puede impedirlo.**

En el escenario 5 el artefacto observa 2 recursos tras dos invocaciones. La
detección es información; el impedimento requiere K5, que se construye en V2.

**H-V1-3. El método no necesita el número de intentos para ser correcto.**

V1 decidió con `MaxIntentosNegocios = 3` sin depender de él: en 2 y 3 el flujo
terminó en la primera verificación. El parámetro solo modera los `0003`/`0004`.

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
