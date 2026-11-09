# Linea base (conducta actual) - salida cruda
SHA-256 del metodo (no usado por la linea base): `ec9b8b6e8d37374ff28f2eb816285571ede997c47ec44a235da829d13dbd10bf`
Fecha: 2026-10-28 20:38:58 -04:00

## CASO-A - E1 Caida a mitad
- estadoRegistrado: Error
- estadoExterno: SOLICITADO
- recursos: 0
- compensaciones: 0
- llamadas: 5
- consultas: 0 (la linea base no verifica estado)
- latenciaMs: 5070
- inconsistente: False
- cumple: False

## CASO-A - E2 Timeout
- estadoRegistrado: Procesada
- estadoExterno: SOLICITADO
- recursos: 2
- compensaciones: 0
- llamadas: 2
- consultas: 0 (la linea base no verifica estado)
- latenciaMs: 3016
- inconsistente: False
- cumple: False

## CASO-A - E3 Respuesta perdida
- estadoRegistrado: Procesada
- estadoExterno: SOLICITADO
- recursos: 2
- compensaciones: 0
- llamadas: 2
- consultas: 0 (la linea base no verifica estado)
- latenciaMs: 1011
- inconsistente: False
- cumple: False

## CASO-A - E4 Solicitud duplicada
- estadoRegistrado: Procesada
- estadoExterno: SOLICITADO
- recursos: 3
- compensaciones: 0
- llamadas: 3
- consultas: 0 (la linea base no verifica estado)
- latenciaMs: 1
- inconsistente: False
- cumple: False

## CASO-A - E5 Reintento tras exito
- estadoRegistrado: Procesada
- estadoExterno: SOLICITADO
- recursos: 2
- compensaciones: 0
- llamadas: 2
- consultas: 0 (la linea base no verifica estado)
- latenciaMs: 0
- inconsistente: False
- cumple: False

## CASO-A - E6 Fallo al repetir anulacion
- estadoRegistrado: Compensada
- estadoExterno: ANULADO
- recursos: 1
- compensaciones: 4
- llamadas: 5
- consultas: 0 (la linea base no verifica estado)
- latenciaMs: 1018
- inconsistente: False
- cumple: False

## CASO-B - E1 Caida a mitad
- estadoRegistrado: Error
- estadoExterno: SOLICITADO
- recursos: 0
- compensaciones: 0
- llamadas: 5
- consultas: 0 (la linea base no verifica estado)
- latenciaMs: 5069
- inconsistente: False
- cumple: False

## CASO-B - E2 Timeout
- estadoRegistrado: Procesada
- estadoExterno: SOLICITADO
- recursos: 2
- compensaciones: 0
- llamadas: 2
- consultas: 0 (la linea base no verifica estado)
- latenciaMs: 3030
- inconsistente: False
- cumple: False

## CASO-B - E3 Respuesta perdida
- estadoRegistrado: Procesada
- estadoExterno: SOLICITADO
- recursos: 2
- compensaciones: 0
- llamadas: 2
- consultas: 0 (la linea base no verifica estado)
- latenciaMs: 1015
- inconsistente: False
- cumple: False

## CASO-B - E4 Solicitud duplicada
- estadoRegistrado: Procesada
- estadoExterno: SOLICITADO
- recursos: 3
- compensaciones: 0
- llamadas: 3
- consultas: 0 (la linea base no verifica estado)
- latenciaMs: 1
- inconsistente: False
- cumple: False

## CASO-B - E5 Reintento tras exito
- estadoRegistrado: Procesada
- estadoExterno: SOLICITADO
- recursos: 2
- compensaciones: 0
- llamadas: 2
- consultas: 0 (la linea base no verifica estado)
- latenciaMs: 0
- inconsistente: False
- cumple: False

## CASO-B - E6 Fallo al repetir anulacion
- estadoRegistrado: Compensada
- estadoExterno: ANULADO
- recursos: 1
- compensaciones: 4
- llamadas: 5
- consultas: 0 (la linea base no verifica estado)
- latenciaMs: 1005
- inconsistente: False
- cumple: False

