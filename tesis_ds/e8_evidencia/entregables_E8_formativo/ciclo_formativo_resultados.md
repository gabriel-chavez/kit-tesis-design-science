# Ciclo formativo - resultados con evidencia

Escenarios: 11 (6 base de E6 + 5 casos limite).
Doble: neutro (no deduplica). Artefacto: E7e_MetodoCompensacion.
SHA-256 del artefacto: `ec9b8b6e8d37374ff28f2eb816285571ede997c47ec44a235da829d13dbd10bf`
Fecha real de ejecucion: 2026-10-27 23:43:36 -04:00


================================================================
S1 - Caida antes del efecto (E1)
================================================================
  Estado bitacora: NoProcesada
  Categoria K3: NoProcesado
  Intentos: 1  Consultas: 1
  Decisiones: Reprocesar
  K4 decision final esperada: Reprocesar
  Recursos externos: 0 (esperado 0)
  Compensaciones externas: 0 (esperado 0)

================================================================
S2 - Timeout despues del efecto (E2)
================================================================
  Estado bitacora: Procesada
  Categoria K3: NoProcesado
  Intentos: 1  Consultas: 1
  Decisiones: ConfirmarProcesada
  K4 decision final esperada: ConfirmarProcesada
  Recursos externos: 1 (esperado 1)
  K4 decision final: ConfirmarProcesada

================================================================
S3 - Respuesta perdida (E3)
================================================================
  Estado bitacora: Procesada
  Categoria K3: NoProcesado
  Intentos: 1  Consultas: 1
  Decisiones: ConfirmarProcesada
  K4 decision final esperada: ConfirmarProcesada
  Recursos externos: 1 (esperado 1)

================================================================
S4 - Solicitud duplicada CONCURRENTE (misma llave, en paralelo)
================================================================
  Invocaciones concurrentes: 5
  Recursos externos creados: 1 (esperado 1)
  Estado bitacora: Procesada
  LIMITE DECLARADO (D1): el lock es de PROCESO. Esta corrida prueba una sola instancia.
  La garantia distribuida depende del indice unico y de la insercion atomica de Dc10.

================================================================
S5 - Reintento tras exito real (E5)
================================================================
  1ra invocacion: exito=True recursos=1
  2da invocacion: exito=True recursos=1
  Recursos externos: 1 (esperado 1)

================================================================
S6 - Fallo durante la compensacion
================================================================
   compensacion #1: ejecutada=True omitida=False respondio=False falloTransporte=True
   compensacion #2: ejecutada=False omitida=True respondio=False falloTransporte=False
   compensacion #3: ejecutada=False omitida=True respondio=False falloTransporte=False
  Compensaciones externas: 1 (esperado 1)
  Registros Aplicada bitacora: 1

================================================================
S7 - Proceso muere entre reserva y ejecucion (K2.MarcarAbandonada)
================================================================
  Reserva creada: True
  Reservas pendientes antes de reconciliar: 1
  K7 pasada 1: resueltasPorEjecucion=0 cerradasPorConfirmacion=0 estadoReserva=Abandonada
  K7 pasada 2: resueltasPorEjecucion=0 cerradasPorConfirmacion=1 estadoReserva=Aplicada
  Reservas resueltas (observado por transicion de estado): 1
  Reservas pendientes despues: 0
  Compensaciones externas: 1 (esperado 1)
  Registros Aplicada bitacora: 1
  Eventos de telemetria RESERVA_ABANDONADA: 1

================================================================
S8 - PENDIENTE frontera 19 y 20 minutos (granularidad declarada)
================================================================
  t=19min: categoria=Indeterminado minutos=19 decision=Reconsultar
  t=20min: categoria=Indeterminado minutos=20 decision=Reconciliar
  NOTA: la ventana se mide en minutos enteros; no hay precision de segundos (19:59/20:01 no distinguibles).

================================================================
S9 - Estado externo desconocido (no asumir, reconciliar)
================================================================
  Consulta fallida: True
  Categoria: Indeterminado
  Decision K4: Reconciliar (esperado Reconciliar)

================================================================
S10 - Falla la escritura en bitacora tras reservar (consistencia)
================================================================
  Excepcion al compensar: ninguna (esperado: ninguna)
  Estado reserva tras K5: Registrada
  Compensaciones externas: 1 (esperado 1)
  K7 pasada 1: resueltasPorEjecucion=0 cerradasPorConfirmacion=1
  K7 pasada 2: resueltasPorEjecucion=0 cerradasPorConfirmacion=0
  Reservas resueltas (observado por transicion de estado): 1
  Estado reserva final: Aplicada
  Reservas pendientes finales: 0
  Compensaciones externas finales: 1 (esperado 1)

================================================================
S11 - Compensacion falla dos veces y luego responde
================================================================
   compensacion #1: ejecutada=True omitida=False respondio=False falloTransporte=True
   compensacion #2: ejecutada=False omitida=True respondio=False falloTransporte=False
   compensacion #3: ejecutada=False omitida=True respondio=False falloTransporte=False
  Compensaciones externas: 1 (esperado 1)

================================================================
Control negativo (sin K5 debe duplicar; con K5 no)
================================================================
  Sin K5 (3 invocaciones): 3 efectos (esperado 3)
  Con K5 (3 invocaciones): 1 efectos (esperado 1)

================================================================
HALLAZGOS Y REDISEÑO - Iteración 3 (corrección de lectura)
================================================================

| Iteración | Qué se buscaba | Resultado |
|---|---|---|
| 1. Detección | Ejecutar los 11 escenarios y encontrar defectos del método | D1 concurrencia, D2 MarcarAbandonada sin cablear, D3 frontera de ventana, D4 persistencia sin manejo, D5 estado desconocido (sin cambio) |
| 2. Rediseño y verificación | Aplicar D1, D2 y D4; declarar D3; re-ejecutar S7, S8, S10 y regresión | D1-D4 resueltos o declarados; 0 defectos en la corrida |
| 3. Corrección de lectura | Separar cierres por ejecución de cierres por confirmación; reemitir evidencia sin tocar el artefacto | S7 y S10: resueltasPorEjecucion=0, cerradasPorConfirmacion=1, resueltas observadas=1; hash intacto |

Total de defectos detectados en esta corrida: 0
La iteración 1 detectó D1-D5; la iteración 2 los rediseñó o declaró; esta iteración solo corrige la lectura del contador.


## Rediseños aplicados en este ciclo

1. **D1 - Concurrencia (S4):** lock por operacion en `CoordinadorMetodo.Procesar` (atomiza el guard). LIMITE: es un lock de proceso; la garantia distribuida depende de Dc10 (indice unico + insercion atomica).
2. **D2 - MarcarAbandonada (S7):** K7 es el responsable unico. Marca Abandonada la reserva huerfana, y `ReservasPendientes`/K5 incluyen Abandonada para resolverla en la siguiente pasada. Verificado con dos pasadas de K7.
3. **D3 - Frontera de ventana (S8):** se declara condicion de contorno: la ventana es de 20 minutos completos y no distingue 19:59 de 20:01. No se afirma precision de segundos.
4. **D4 - Persistencia (S10):** `MarcarAplicada` devuelve bool; K5 reintenta y, si falla, deja la reserva Registrada sin propagar excepcion. K7 la resuelve en pasadas posteriores. Verificado.
5. **S9 - Estado desconocido:** ya reconcilia; sin cambio funcional.

