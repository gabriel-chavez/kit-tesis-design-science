# Verificación del paquete `e7e-testbed.rar`

**Fecha de verificación:** 2026-10-27
**Archivo:** `tesis_ds/e8_evidencia/e7e-testbed_frozen.rar` (575.154 bytes)
**Extracción:** en carpeta temporal de inspección; no se sobrescribió el banco de pruebas del proyecto.

## Hash del artefacto

| Campo | Valor |
|:--|:--|
| Archivo | `coordinador/bin/Debug/net10.0/E7e.MetodoCompensacion.dll` |
| Tamaño | 43.520 bytes |
| SHA-256 | `ec9b8b6e8d37374ff28f2eb816285571ede997c47ec44a235da829d13dbd10bf` |
| Esperado (versión congelada) | `ec9b8b6e8d37374ff28f2eb816285571ede997c47ec44a235da829d13dbd10bf` |
| Resultado | **Coincide** |

## Contenido

- `busa-eval-stub/`: doble neutro (ASP.NET Core) con modos de fallo y reloj inyectable.
- `coordinador/`: arnés de verificación y DLL del artefacto.
- `prueba-enlace/`: prueba con el cliente WCF real de BUSA.
- `evidencia_e7e/`: `v1_resultados.md`, `v2_resultados.md`, `v3_resultados.md`, `control_negativo.md`, `v8_trazabilidad.md`, `registro_cambios.md`, `walkthrough_practicante.md`, `prueba_enlace_cliente_real.md` y `crudo/`.
- `walkthrough_entrega/`: `00_LEEME`, `01_metodo`, `02_decisiones`, `03_consulta_estado_qr`, `04_tablas_bitacora`, `05_restricciones`, `guion_walkthrough`.
- `07e_construccion_verificacion.md`, `08_diseno_doble_evaluacion.md`.

## Integración en el proyecto (2026-10-27)

- `tesis_ds/e7e-testbed` fue reemplazado por el contenido del RAR, **sin `obj/` ni `bin/`**.
- El DLL congelado se conserva en `tesis_ds/e7e-testbed/artefacto_congelado/E7e.MetodoCompensacion.dll`, con su hash en `SHA256.txt`.
- El testbed anterior quedó respaldado en `tesis_ds/e8_evidencia/_backup/e7e-testbed_previo.zip`.
- **Nota de compilación:** `coordinador/Coordinador.csproj` mantiene la referencia al proyecto original (`..\..\..\..\Source\Workspaces\...\E7e_MetodoCompensacion.csproj`). Para compilar en otro entorno hay que ajustar esa referencia o usar el DLL congelado.
