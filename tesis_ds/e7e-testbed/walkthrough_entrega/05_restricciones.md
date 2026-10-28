# Restricciones del entorno

Lo que se puede y lo que no se puede hacer durante el walkthrough.

## Sí

- Trabajar en un entorno **aislado**.
- Usar los datos que ya existan en el entorno de pruebas.
- Cortar la red, cerrar procesos, apagar el servicio simulado, lo que haga falta para provocar
  el fallo.
- Modificar el entorno **de pruebas**.
- Preguntarle al autor solo por **este paquete**: si algo de acá no se entiende, eso es
  precisamente un dato que hay que anotar.

## No

| Prohibido | Por qué |
|---|---|
| Invocar el servicio real de BUSA | El entorno real se toca solo en producción, y esto es una prueba |
| Escribir en la base de producción | Igual que arriba |
| Usar datos de personas reales | Se trabaja con datos del entorno de pruebas |
| Introducir una infraestructura de mensajería | El sistema no usa mensajería y el método no la necesita |
| Pedirle al autor que explique cómo funciona por dentro | Si hay que preguntar, entonces el método no se explica solo, y eso hay que registrarlo, no taparlo |

## Una condición sobre el material

El paquete que estás leyendo contiene **el método**, no su implementación.

No vas a recibir el código del artefacto ni los resultados de las pruebas internas. La razón es
simple: si ves la implementación, lo que estás evaluando es el código, y lo que se quiere medir
es si el **método** se puede aplicar sin que el autor esté al lado.

Si en algún momento te encontrás queriendo ver cómo está implementado para poder avanzar, anotalo.
Es una observación válida y probablemente la más útil de todo el ejercicio.

## Dato sobre el sistema real, para ubicarte

- La cadena tiene **dos saltos propios** y termina en un servicio externo (BUSA) que no participa
  de ninguna transacción global.
- El sistema es **.NET Framework 4.6.2** con servicios **WCF**.
- Las dos rutas de entrada que hoy existen son RpcA (`CPagoSimple`) y RpcB (`CNPagosQR`).
- No hay mensajería en ninguna parte del camino.
- Ya existe un servicio programado que revisa operaciones cada 10 segundos
  (`WinServiceSoatConsultaPagosQR`).

---

**Procedencia**

| Campo | Valor |
|---|---|
| Fuente de las restricciones | Guion del walkthrough (E7e) y decisiones Dc1–Dc13 |
| Fecha | 2026-10-14 |
