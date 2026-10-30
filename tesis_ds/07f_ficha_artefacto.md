# E7f. Ficha del artefacto

> **Estado:** cerrada el 2026-10-20, con el walkthrough independiente. Un investigador sin acceso al tesista puede entender y reproducir el artefacto a partir de esta ficha, del `metodo.md` y de la evidencia. Documento de referencia completo: `metodo.md`.

## 1. Identificación y tipología

- **Nombre:** Método de compensación idempotente con verificación de estado para Sagas sobre servicios externos no transaccionales.
- **Tipología:** método, con principios de diseño y criterios de verificación asociados.
- **Vehículo:** librería `E7e_MetodoCompensacion` (`netstandard2.0`, sin dependencias fuera de la BCL) y una exemplar sobre un doble de servicio.

## 2. Problema que resuelve

Ante un timeout o una respuesta perdida en cualquier salto de la cadena, el sistema no distingue una operación **no procesada** de una **procesada cuya respuesta se perdió**, y reintenta sin verificar; esto admite duplicados y deja operaciones sin estado final confiable.

## 3. Requisitos (F1–F9, C1–C6)

Identidad determinista (F1); bitácora de estado (F2); verificación del estado externo (F3); motor de decisión (F4); compensación idempotente (F5); orden de compensaciones (F6); reconciliación (F7); criterios de verificación (F8); telemetría (F9). Contextuales: sin migrar de marco (C1); solo con consulta de estado (C2); seis escenarios (C3); sin espera bloqueante (C4); costo razonable (C5); aplicable por terceros (C6).

## 4. Diseño y componentes

Nueve componentes (K1–K9) sobre cuatro puertos (`IServicioExterno`, `IBitacora`, `IReloj`, `ITelemetria`). El mismo código sirve al sistema real y a la evaluación. Coordinador en la capa de integración (opción A). Diagrama y detalle en `07c_diseno_artefacto.md` y `metodo.md`.

## 5. Decisiones y alternativas descartadas

Dc1–Dc14 en `07c_diseno_artefacto.md`. Principales: llave derivada (Dc1); verificar ante sospecha (Dc2); PENDIENTE = indeterminado (Dc3); compensación por `(operación, motivo)` con registro previo (Dc5); unicidad en base (Dc10); cubrir ambas rutas (Dc11); reconciliación con el servicio existente (Dc12); compensación agnóstica a la acción (Dc13); decidir con dos hechos (Dc14). Descartadas: apoyarse en un marco de mensajería (B) y reconciliación sin compensación explícita (C).

## 6. Versiones

V1 exploratoria (K1–K3), V2 experimental (+K4, K5, K6, K9) y V3 operacional (+K7, K8). Construidas y verificadas el 2026-10-13; versión congelada el 2026-10-27 (SHA-256 `ec9b8b6e…`, 43.520 bytes).

## 7. Verificación interna

V1–V8 **CUMPLE** (V8 con salvedades); control negativo confirmado (sin K5: N efectos; con K5: 1). Artefacto con SHA-256 `ec9b8b6e8d37374ff28f2eb816285571ede997c47ec44a235da829d13dbd10bf` (43.520 bytes). Detalle en `07e_construccion_verificacion.md` y `e7e-testbed/evidencia_e7e/`. **Walkthrough independiente: cumplido** (2026-10-16, informe 2026-10-20; 6/8 tareas y 0/2 puntos críticos a la primera). **Revisión por expertos: pendiente.**

**Evaluación (E8, 2026-10-28):** sobre el doble neutro, método 12/12 frente a 0/12 de la línea base; 0 recursos y 0 compensaciones duplicadas; control negativo 3/1 por caso. Ver `08_evaluacion.md` §9 y `e8_evidencia/sumativo/`.

## 8. Instrucciones de uso y reproducción

Requiere SDK .NET, Windows y acceso solo a `localhost`. Comandos del doble, de las verificaciones y del artefacto en `07e_construccion_verificacion.md` §7.

## 9. Limitaciones conocidas

La evidencia verifica el método, **no su despliegue**: la cadena completa no se ejecutó, el DDL de Dc10 no se aplicó y los adaptadores reales no se escribieron. El cliente WCF real de BUSA enlaza con el doble (2/2), sin invocar el servicio real. El walkthrough independiente está cumplido; la revisión por expertos sigue pendiente. No cubre la reversión de pagos ni la conciliación con el banco.

## 10. Relación con la base de conocimiento

Sagas (R1); idempotencia (R2); consistencia eventual (R3); verificación de idempotencia (R4); inyección de fallos (R6); registro de la saga (R10). Metodología: DSRM (R18), FEDS (R8), Wieringa (R20).
