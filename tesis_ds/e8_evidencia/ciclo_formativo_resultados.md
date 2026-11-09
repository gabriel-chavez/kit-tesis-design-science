# Ciclo formativo - resultados con evidencia

Escenarios: 11 (6 base de E6 + 5 casos limite).
Doble: neutro (no deduplica). Artefacto: E7e_MetodoCompensacion.
Fecha real de ejecucion: 2026-10-27 21:59:25 -04:00


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
  K7: operaciones revisadas=1 reservas resueltas=1 omitidas=0
  Reservas pendientes despues: 0
  NOTA: K2.MarcarAbandonada esta implementada pero NO se invoca en K5/K7 (defecto de cableado).

================================================================
S8 - PENDIENTE en frontera de ventana (19 vs 20 min)
================================================================
  (ver bloque dedicado S8b)

================================================================
S8b - PENDIENTE frontera 19 y 20 minutos
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
  Excepcion al compensar: InvalidOperationException: fallo de escritura en bitacora (simulado)
  Compensaciones externas: 1
  NOTA: MarcarAplicada es void y no hay manejo de fallo de persistencia; la reserva queda Registrada sin cerrar.

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
HALLAZGOS Y REDISENO (primer rediseño)
================================================================

Total de defectos detectados: 2

- S7: K2.MarcarAbandonada implementada pero no invocada por ningun componente (cableado faltante)
- S10: fallo de escritura en bitacora no esta manejado (propaga excepcion o deja reserva colgada)

## Rediseños aplicados en este ciclo

1. **Concurrencia (S4):** el guard de idempotencia de la operacion en `CoordinadorMetodo.Procesar` lee/escribe la bitacora sin sincronizacion. Rediseño: proteger `Abrir`/consulta de estado con un lock por operacion (o transaccion en BD).
2. **K2.MarcarAbandonada sin invocar (S7):** cablear en K5 (cuando una reserva queda Registrada y el proceso muere) o en K7. Se asigna a K7 como responsable unico (pregunta abierta E8).
3. **Frontera de ventana (S8):** la ventana se evalua en minutos enteros; no hay precision de segundos. Rediseño: usar comparacion de timestamps con precision de segundos (o definir ventana en segundos).
4. **Fallo de persistencia en bitacora (S10):** `MarcarAplicada` es void y no hay reintento. Rediseño: convertir a resultado, reintentar y dejar la reserva en Registrada para K7 si falla.
5. **Estado desconocido (S9):** ya reconcilia; mantener. Sin cambio funcional, solo telemetria.

