# Prueba de enlace con el cliente WCF real de BUSA — resultado

**Etapa:** E7e, requisito A de la opción A (cadena real)
**Fecha real de ejecución:** 2026-10-14
**Servicio real BUSA (`172.30.140.139`):** 0 invocaciones
**Servicio real de firma digital:** 0 invocaciones
**Invoca el doble, nunca el servicio real:** confirmado

---

## 1. Qué se probó

Que el **cliente WCF real de BUSA** enlace con el doble. Es la misma clase que el código real
instancia, sin una línea propia en la capa de transporte:

| Cliente real | Dónde lo usa producción | Doble |
|---|---|---|
| `wsUniQrService.QRServiceClient.BunApi(string)` | `CPagoSimple.cs:1096` | `localhost:5098/UNIQRService/UNIQRService.svc` |

La prueba compila los **mismos archivos generados** que usa producción
(`Connected Services/wsUniQrService/Reference.cs`), por `Compile Include` enlazado al original.
No hay réplica ni reimplementación del contrato.

## 2. Resultado

### 2.1 BUSA — ENLAZA Y FUNCIONA

Salida cruda de la corrida (`crudo/enlace_busa_salida_cruda.txt`, código de salida 0):

```
=================================================================
PRUEBA DE ENLACE CON EL CLIENTE WCF REAL DE BUSA
BUSA  : http://localhost:5098/UNIQRService/UNIQRService.svc
=================================================================


--- 2. wsUniQrService.QRServiceClient.BunApi (2 invocaciones) ---
  invocacion #1: OK  <ns2:Response xmlns:ns2="http://www.bancounion.com.bo/"><ns2:ResultCode>0000</ns2:ResultCode>...
  invocacion #2: OK  <ns2:Response xmlns:ns2="http://www.bancounion.com.bo/"><ns2:ResultCode>0000</ns2:ResultCode>...
  exitosas: 2 / 2

RESULTADO GLOBAL: el cliente real de BUSA enlaza
```

Contadores del doble (verdad externa, `admin/volcado/ENLACE-1`):

```
operacion=ENLACE-1
  unidadesQr=2 efectosGenerarQr=2 compensacionesAplicadas=0
  [salto externo] invocaciones=2;efectosAplicados=2;compensacionesAplicadas=0
    unidad ENLACE-1-QR1 estado=SOLICITADO efectos=[GENERAR_QR#1]
    unidad ENLACE-1-QR2 estado=SOLICITADO efectos=[GENERAR_QR#1]
```

**2 invocaciones → 2 unidades → 2 efectos.** Cada invocación creó un recurso: el doble **no
deduplica**. Queda confirmada con el cliente real la neutralidad que ya se había establecido
con el arnés en proceso.

Esto importa para el método: si el doble deduplicara, la no-duplicación podría atribuirse al
simulador. Como no deduplica, cualquier repetición que se evite es aporte del método.

## 3. Compilación de los proyectos reales

Verificada el 2026-10-13 con el MSBuild de Visual Studio 18 (`18.9.1`):

| Proyecto | DLL de salida | Tamaño |
|---|---|---|
| `NegociosPasarelaPagos` (contiene `CNPagosQR`) | `bin\Debug\NegociosPasarelaPagos.dll` | 28 KB |
| `WebServicePasarelaPagos` (host WCF coordinador) | `bin\WebServicePasarelaPagos.dll` | 5 KB |
| `NegociosPagoSimple` (contiene `CPagoSimple`) | `bin\Debug\NegociosPagoSimple.dll` | 55 KB |
| `WebServicePagoSimple` (host WCF de pago) | `bin\WebServicePagoSimple.dll` | 6 KB |

Las DLL de UVNet estaban disponibles en `packages\`. El bloqueo no eran las dependencias.

## 4. Obstáculos encontrados y cómo se resolvieron

| # | Obstáculo | Resolución |
|---|---|---|
| 1 | `dotnet build` pide la carga `android` | Es `UNIVidaNetClienteQr`, cliente móvil ajeno a la cadena. Se compilan solo los proyectos de la cadena |
| 2 | Falta `Microsoft.WebApplication.targets` | Usar el **MSBuild de Visual Studio 18**, que sí lo incluye |
| 3 | **No hay elevación** (`Administrador: False`) | `ServiceHost` de WCF usa `HttpListener` con prefijo comodín `http://+:PUERTO`, que exige reserva de URL. El doble se aloja en ASP.NET Core respondiendo el sobre SOAP |
| 4 | `BunApi` y `BunApiAsync` comparten Action: válido como cliente, inválido como servicio | Se reprodujo el Action exacto. El enlace del cliente se resuelve por Action y namespace, no por tipo |
| 5 | **Namespace del sobre de respuesta** | El `ServiceContract` generado no declara `Namespace`, así que WCF espera `http://tempuri.org/`, no `http://tempuri.org/IQRService` |
| 6 | Resultado escapado como texto | WCF espera un **elemento hijo**, no texto. Escaparlo produce `NetDispatcherFaultException` |

El obstáculo **5** y el **6** merecen registro: en ambos casos la petición llegaba, el doble
aplicaba el efecto correctamente y solo el sobre de respuesta estaba mal. Un observador que
mirara únicamente los contadores del doble habría concluido que todo funcionaba.

## 5. Desviaciones declaradas

| # | Desviación | Consecuencia |
|---|---|---|
| 1 | Doble alojado en ASP.NET Core, no en `ServiceHost` de WCF | El cliente sigue enviando y recibiendo SOAP sobre HTTP real. No se reproduce el *stack de servidor WCF* |
| 2 | HTTP sin TLS | Producción usa `basicHttpBinding` con `security mode="Transport"` (HTTPS) |
| 3 | Dirección por constructor, no por `Web.config` | Evita modificar configuración de producción |
| 4 | **El paso de firma digital se sustituye por un paso simulado** | El método evaluado (K1–K9) **no depende** de la firma: decide por verificación de estado (K3) e idempotencia (K5). El alcance de E7e es la cadena de pago QR, no la firma |

La desviación 4 acota el alcance: la firma digital es una preocupación transversal ajena a la
pregunta de investigación. Ningún componente del método lee, escribe ni decide en función de
la firma.

## 6. Estado de la cadena real

| Salto | Componente real | Estado |
|---|---|---|
| Interno | `CNPagosQR` (RpcB) | **Compilado**, no ejecutado |
| Interno | `CPagoSimple` (RpcA) | **Compilado**, no ejecutado |
| Externo | `QRServiceClient` → BUSA | **Enlaza y funciona**, 2/2 |
| Hosts | `WebServicePagoSimple`, `WebServicePasarelaPagos` | **Compilados**, no alojados |
| Alojamiento | IISExpress | **Disponible**, no configurado |
| Cadena completa | `CNPagosQR → CPagoSimple → BUSA` | **NO EJECUTADA** |

## 7. Reproducción

```powershell
# Terminal 1 — doble BUSA
cd "C:\Users\lchavez\Documents\Proyecto predeterminado\e7e-testbed\busa-eval-stub"
dotnet run --urls "http://localhost:5098"

# Terminal 2 — cliente real de BUSA contra el doble
cd "C:\Users\lchavez\Documents\Proyecto predeterminado\e7e-testbed\prueba-enlace"
dotnet run -- "http://localhost:5098/UNIQRService/UNIQRService.svc" "ENLACE-1" 2
```

Detener: `Stop-Process -Name BusaEvalStub`

---

**Procedencia de esta evidencia**

| Campo | Valor |
|---|---|
| Generado por | arnés `prueba-enlace`, que usa los clientes WCF reales |
| Fecha y hora de ejecución | 2026-10-14 | 
| Comando | `dotnet run -- http://localhost:5098/UNIQRService/UNIQRService.svc ENLACE-1 2` |
| Código de salida | 0 |
| Salida cruda | `crudo/enlace_busa_salida_cruda.txt` |
| Servicios reales invocados | **ninguno** |
