# Metodo congelado - salida cruda
SHA-256: `ec9b8b6e8d37374ff28f2eb816285571ede997c47ec44a235da829d13dbd10bf`
Fecha: 2026-10-28 20:38:58 -04:00

## CASO-A - E1 Caida a mitad
- estadoBitacora: NoProcesada
- estadoExterno: SOLICITADO
- recursos: 0
- compensaciones: 0
- decision: Reprocesar
- llamadas: 1
- consultas: 1
- latenciaMs: 33
- inconsistente: False
- cumple: True

## CASO-A - E2 Timeout
- estadoBitacora: Procesada
- estadoExterno: SOLICITADO
- recursos: 1
- compensaciones: 0
- decision: ConfirmarProcesada
- llamadas: 1
- consultas: 1
- latenciaMs: 2016
- inconsistente: False
- cumple: True

## CASO-A - E3 Respuesta perdida
- estadoBitacora: Procesada
- estadoExterno: SOLICITADO
- recursos: 1
- compensaciones: 0
- decision: ConfirmarProcesada
- llamadas: 1
- consultas: 1
- latenciaMs: 2
- inconsistente: False
- cumple: True

## CASO-A - E4 Solicitud duplicada
- estadoBitacora: Procesada
- estadoExterno: SOLICITADO
- recursos: 1
- compensaciones: 0
- decision: ConfirmarProcesada
- llamadas: 1
- consultas: 0
- latenciaMs: 1
- inconsistente: False
- cumple: True

## CASO-A - E5 Reintento tras exito
- estadoBitacora: Procesada
- estadoExterno: SOLICITADO
- recursos: 1
- compensaciones: 0
- decision: ConfirmarProcesada
- llamadas: 1
- consultas: 0
- latenciaMs: 0
- inconsistente: False
- cumple: True

## CASO-A - E6 Fallo al repetir anulacion
- estadoBitacora: Compensada
- estadoExterno: ANULADO
- recursos: 1
- compensaciones: 1
- decision: Compensar
- llamadas: 2
- consultas: 0
- latenciaMs: 5
- inconsistente: False
- cumple: True

## CASO-B - E1 Caida a mitad
- estadoBitacora: NoProcesada
- estadoExterno: SOLICITADO
- recursos: 0
- compensaciones: 0
- decision: Reprocesar
- llamadas: 1
- consultas: 1
- latenciaMs: 3
- inconsistente: False
- cumple: True

## CASO-B - E2 Timeout
- estadoBitacora: Procesada
- estadoExterno: SOLICITADO
- recursos: 1
- compensaciones: 0
- decision: ConfirmarProcesada
- llamadas: 1
- consultas: 1
- latenciaMs: 2012
- inconsistente: False
- cumple: True

## CASO-B - E3 Respuesta perdida
- estadoBitacora: Procesada
- estadoExterno: SOLICITADO
- recursos: 1
- compensaciones: 0
- decision: ConfirmarProcesada
- llamadas: 1
- consultas: 1
- latenciaMs: 2
- inconsistente: False
- cumple: True

## CASO-B - E4 Solicitud duplicada
- estadoBitacora: Procesada
- estadoExterno: SOLICITADO
- recursos: 1
- compensaciones: 0
- decision: ConfirmarProcesada
- llamadas: 1
- consultas: 0
- latenciaMs: 0
- inconsistente: False
- cumple: True

## CASO-B - E5 Reintento tras exito
- estadoBitacora: Procesada
- estadoExterno: SOLICITADO
- recursos: 1
- compensaciones: 0
- decision: ConfirmarProcesada
- llamadas: 1
- consultas: 0
- latenciaMs: 0
- inconsistente: False
- cumple: True

## CASO-B - E6 Fallo al repetir anulacion
- estadoBitacora: Compensada
- estadoExterno: ANULADO
- recursos: 1
- compensaciones: 1
- decision: Compensar
- llamadas: 2
- consultas: 0
- latenciaMs: 2
- inconsistente: False
- cumple: True

