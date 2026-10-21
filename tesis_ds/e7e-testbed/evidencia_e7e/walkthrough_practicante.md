# Cómo probar el método sin que te ayude yo

**Etapa:** E7e
**Estado:** guion listo, todavía sin ejecutar
**Preparado el:** 2026-10-13

---

## 1. Antes de empezar

Este documento no es un informe. Es lo que le entregaría a alguien para que pruebe el método
por su cuenta.

La idea de fondo es sencilla. Si el método solo lo sabe usar quien lo escribió, entonces no
sirve como método, sirve como código mío. Necesito que lo aplique otra persona, sin que yo le
explique nada, y que después me cuente dónde se trabó y qué le pareció ambiguo.

Yo no puedo hacerlo. Si lo hago yo, no se prueba nada.

| | |
|---|---|
| Quién lo hace | (falta poner el nombre) |
| Qué caso le toca | (se elige en el punto 3) |
| Cuándo | (falta) |
| Cuánto tardó | (falta) |

El apartado 6 está en blanco a propósito. Se llena cuando alguien lo haga de verdad. Si lo
llenamos antes, no vale.

## 2. Qué le pido a quien lo haga

- Que no sea yo. Si lo hago yo, la prueba no mide nada.
- Que sepa .NET y WCF. El sistema es .NET Framework 4.6.2 y hay servicios WCF de por medio.
- Que pueda entrar al código de `PasarelaPagos` y de `UNIVidaNetPagosSimple`.
- Que arme un entorno aparte. Nada contra BUSA de verdad y nada contra datos de producción.
- Y lo más importante: que **no** vea cómo está implementado el método hasta que termine.

Esa última condición es la que le da valor a todo. Si ve el código, va a evaluar mi código y
no el método. Lo que quiero saber es si el método se entiende solo, no si el código compila.

## 3. El caso que le toca

Tiene que ser un caso nuevo, que no sea ninguno de los seis escenarios que ya probé. Si le doy
uno de esos, va a sacar la respuesta mirando los ejemplos y no me sirve.

Lo que sugiero:

| Elemento | Qué usar |
|---|---|
| Operación | Una venta de SOAT ya registrada en el entorno de pruebas, con su `TRAMITE_SECUENCIAL` y su `GESTION` de verdad, los de ese entorno |
| Falla | Un corte de red entre el servicio de pago y el externo, y que el corte ocurra **después** de que el externo ya aplicó el efecto |
| Dónde corta | Como prefiera: `TcpClient`, el firewall, o cerrando el proceso del doble. Da igual mientras el corte sea real |
| Cuándo | En el camino de integración, no en el de ejecución |
| Datos | Los del entorno de pruebas, no inventados. Acá quiero medir si el método se puede aplicar, no cómo se porta un simulador |

## 4. Qué le entrego

| # | Material | Qué contiene |
|---|---|---|
| 1 | `metodo.md` | El método completo: los componentes, los pasos y la regla de normalización |
| 2 | Referencia de decisiones | Dc1 a Dc13 |
| 3 | Consulta del servicio externo | El nombre y el contrato de `ConsultarQR` (`UNIVIDA_*_QR_002`) |
| 4 | Tablas disponibles | Cómo están armadas `T_QR_SOLICITUD` y `T_QR_SOLICITUD_EJECUCION_INTENTOS` |
| 5 | Restricciones | Entorno aislado, sin mensajería, sin tocar producción |

**No le entrego** el código del artefacto ni los resultados de V1 a V8. Tampoco este guion más
allá de las tareas. Si le paso los resultados, ya sabe para dónde va.

## 5. Las tareas

Para cada paso quiero que anote tres cosas: si lo entendió sin preguntarme, qué le resultó
ambiguo, y qué cambiaría. Si algo le parece obvio, que igual lo anote, porque a mí lo obvio me
sirve para saber qué no hace falta explicar.

### Tarea 0. Entender el problema, sin código todavía

| Paso | Qué le pido | Qué miro |
|---|---|---|
| 0.1 | «Decime qué se rompe cuando una llamada al externo se pierde después de que el efecto ya se aplicó» | ¿Se da cuenta de que no se puede distinguir entre "no se procesó" y "se procesó"? |
| 0.2 | «¿Y qué debería hacer el sistema en ese caso?» | ¿Propone verificar antes de reintentar? |
| 0.3 | «¿Dónde pondrías esa solución? ¿En la capa de integración o en el servicio de pago?» | ¿Ve la ventaja de tener un solo lugar donde se decide? |

### Tarea 1. Identidad

| Paso | Qué le pido | Qué miro |
|---|---|---|
| 1.1 | Abrir la operación y sacar la llave | ¿Llega a `(TRAMITE_NUMERO, GESTION)` sin que le diga? |
| 1.2 | Comprobar que la misma operación da la misma llave | ¿Sospecha que hace falta una clave estable? |

### Tarea 2. Verificar el estado

| Paso | Qué le pido | Qué miro |
|---|---|---|
| 2.1 | Con el corte hecho, averiguar el estado real | ¿Sabe que hay que preguntarle al externo y no deducirlo? |
| 2.2 | Clasificar el estado que le devuelven | ¿Consulta el estado o se queda con `SOLICITADO`? **Acá se juega la medición** |
| 2.3 | Si le devuelve `PENDIENTE` | ¿Asume un estado o espera? |

### Tarea 3. Decidir

| Paso | Qué le pido | Qué miro |
|---|---|---|
| 3.1 | Decidir qué hacer con el estado ya verificado | ¿Distingue "no se procesó" de "el recurso ya está hecho"? |
| 3.2 | Justificar la decisión | ¿Usa el principio o decide por intuición? |

### Tarea 4. Compensar sin repetir

| Paso | Qué le pido | Qué miro |
|---|---|---|
| 4.1 | Identificar la compensación por `(operación, motivo)` | ¿Propone un par? ¿Un contador? ¿Una fecha? |
| 4.2 | Definir qué se registra primero, si el registro o la ejecución | **Acá se juega la medición otra vez** |
| 4.3 | Repetir la compensación dos veces | ¿Entiende que no puede duplicar sin consultar antes? |

### Tarea 5. Reconciliar

| Paso | Qué le pido | Qué miro |
|---|---|---|
| 5.1 | Resolver la operación que quedó huérfana | ¿Le pregunta al externo o asume? |
| 5.2 | Correrla dos veces | ¿Ve que tiene que ser idempotente? |

### Tarea 6. Registrar

| Paso | Qué le pido | Qué miro |
|---|---|---|
| 6.1 | Definir qué se guarda | ¿Incluye el salto? ¿Incluye la causa? |

### Tarea 7. Cerrar

| Paso | Qué le pido | Qué miro |
|---|---|---|
| 7.1 | Declarar cómo quedó la operación | ¿Queda algo sin cerrar? |
| 7.2 | Declarar qué no pudo verificar | ¿Distingue "no pasó" de "no quedó registrado"? |

## 6. Lo que hay que registrar

**En blanco hasta que se haga.** No se completa antes.

### 6.1 Cómo salió cada tarea

| Tarea | Lo entendió solo | Ambigüedad | Qué cambiaría | Notas de quien lo hizo |
|---|---|---|---|---|
| 0 | | | | |
| 1 | | | | |
| 2 | | | | |
| 3 | | | | |
| 4 | | | | |
| 5 | | | | |
| 6 | | | | |
| 7 | | | | |

### 6.2 Cuánto se pudo aplicar

| Métrica | Valor | De dónde sale |
|---|---|---|
| Tareas resueltas sin ayuda | | Contar los «Sí» de 6.1 |
| Puntos críticos resueltos a la primera | | Los pasos 2.2 y 4.2 |
| Ambigüedades que frenaron el trabajo | | Descripción libre |
| Cambios que propone al método | | Lista de 6.1 |
| Su veredicto | | ¿El método se puede aplicar sin el autor? |

### 6.3 Cambios al método que salieron del walkthrough

Cada fila tiene que venir de una ambigüedad concreta de 6.1. Si no, no cuenta como mejora.

| # | Ambigüedad | Cambio propuesto | Por qué | Estado |
|---|---|---|---|---|
| | | | | |

## 7. Cuándo se puede decir que está hecho

| # | Criterio | Estado |
|---|---|---|
| 1 | Lo hizo alguien distinto del autor | **No cumplido** |
| 2 | El caso era nuevo, no uno de los seis escenarios | **No cumplido** |
| 3 | Se entregó el método sin su implementación | **No cumplido** |
| 4 | Todas las tareas tienen resultado registrado | **No cumplido** |
| 5 | Al menos un punto crítico falló en el primer intento | **No evaluable** |
| 6 | Los cambios propuestos salen de ambigüedades concretas | **No evaluable** |

Mientras los puntos 1 a 4 no se cumplan, esto sigue en «No cumplido» y el criterio de cierre 4
de la etapa no está satisfecho. Lo dejo escrito así en vez de dar por bueno un walkthrough
informal hecho por el propio autor.
