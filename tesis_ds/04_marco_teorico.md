# E4. Marco teórico (sustentación del objeto de estudio)

> **Estado:** cerrada el 2026-09-26, desde el punto de vista de su puerta. Queda pendiente la redacción extensa de la síntesis (tarea de escritura, no de fundamento) y la verificación de dos libros.

## 1. Protocolo de búsqueda teórica

**Pregunta de búsqueda:** ¿qué teorías, principios y resultados empíricos de la ingeniería de software y de los sistemas distribuidos sustentan el diseño y la verificación de compensaciones idempotentes en transacciones distribuidas basadas en Sagas con servicios externos no transaccionales?

**Criterios de inclusión:**

- Libros y fuentes primarias (artículos de revista o de conferencia revisados).
- Teoría aplicada: preferencia por obras de los últimos cinco años.
- Obras fundacionales: se cita la edición vigente y se indica el año original.
- **Acceso abierto** (restricción del tesista): se priorizan obras recuperables sin suscripción.

**Criterios de exclusión:**

- Fuentes secundarias no verificables, blogs sin respaldo y trabajos sin evaluación.
- Publicaciones en medios de dudosa revisión que repiten resultados sin aportar evidencia.

## 2. Fuentes verificadas (APA 7)

### 2.1 Fundacionales (objeto de estudio)

| # | Referencia | Medio |
|:--|:--|:--|
| R1 | García-Molina, H., & Salem, K. (1987). Sagas. *Proceedings of the 1987 ACM SIGMOD International Conference on Management of Data*, 249–259. https://doi.org/10.1145/38713.38742 | Conferencia |
| R2 | Helland, P. (2012). Idempotence is not a medical condition. *ACM Queue, 10*(4). https://doi.org/10.1145/2181796.2187821 | Revista |
| R3 | Vogels, W. (2009). Eventually consistent. *Communications of the ACM, 52*(1), 40–44. https://doi.org/10.1145/1435417.1435432 | Revista |
| R4 | Cristian, F. (1991). Understanding fault-tolerant distributed systems. *Communications of the ACM, 34*(2), 56–78. https://doi.org/10.1145/102792.102801 | Revista |

### 2.2 Verificación y evaluación

| # | Referencia | Medio |
|:--|:--|:--|
| R5 | Hummer, W., Rosenberg, F., Oliveira, F., & Eilam, T. (2013). Testing idempotence for infrastructure as code. *Service-Oriented Computing (ICSOC 2013)*. https://doi.org/10.1007/978-3-642-45065-5_19 | Conferencia |
| R6 | Claessen, K., & Hughes, J. (2000). QuickCheck: A lightweight tool for random testing of Haskell programs. *Proceedings of the Fifth ACM SIGPLAN International Conference on Functional Programming*, 268–279. https://doi.org/10.1145/357766.351266 | Conferencia |
| R7 | Basiri, A., Behnam, N., de Rooij, R., Hochstein, L., Kosewski, L., Reynolds, J., & Rosenthal, C. (2016). Chaos engineering. *IEEE Software, 33*(3), 35–41. https://doi.org/10.1109/MS.2016.60 | Revista |
| R8 | Venable, J., Pries-Heje, J., & Baskerville, R. (2016). FEDS: A framework for evaluation in design science research. *European Journal of Information Systems, 25*(1), 77–89. https://doi.org/10.1057/ejis.2014.36 | Revista |

### 2.3 Sagas en microservicios (recientes)

| # | Referencia | Medio |
|:--|:--|:--|
| R9 | Štefanko, M., Chaloupka, O., & Rossi, B. (2019). The saga pattern in a reactive microservices environment. *Proceedings of the 14th International Conference on Software Technologies*, 483–490. https://doi.org/10.5220/0007918704830490 | Conferencia |
| R10 | Daraghmi, E., Zhang, C.-P., & Yuan, S.-M. (2022). Enhancing saga pattern for distributed transactions within a microservices architecture. *Applied Sciences, 12*(12), 6242. https://doi.org/10.3390/app12126242 | Revista (OA) |
| R11 | Reza, K., & Rahman, N. (2022). Explication and extension of Saga and microservice patterns to enable resilient distributed transaction. *Lecture Notes in Networks and Systems*. https://doi.org/10.1007/978-981-19-4960-9_18 | Capítulo |
| R12 | Aydın, Ş., & Çebi, C. B. (2022). Comparison of choreography vs orchestration based Saga patterns in microservices. *2022 International Conference on Electrical, Computer and Energy Technologies (ICECET)*. https://doi.org/10.1109/ICECET55527.2022.9872665 | Conferencia |
| R13 | Laigner, R., Zhou, Y., Salles, M. A. V., Liu, Y., & Kalinowski, M. (2021). Data management in microservices: State of the practice, challenges, and research directions. *Proceedings of the VLDB Endowment, 14*(13). https://doi.org/10.14778/3484224.3484232 | Revista |
| R14 | Šöylemez, M., Tekinerdogan, B., & Tarhan, A. (2022). Challenges and solution directions of microservice architectures: A systematic literature review. *Applied Sciences, 12*(11), 5507. https://doi.org/10.3390/app12115507 | Revista (OA) |
| R15 | Blinowski, G., Ojdowska, A., & Przybyłek, A. (2022). Monolithic vs. microservice architecture: A performance and scalability evaluation. *IEEE Access, 10*. https://doi.org/10.1109/ACCESS.2022.3152803 | Revista (OA) |
| R16 | Kaloudis, M. (2024). Evolving software architectures from monolithic systems to resilient microservices: Best practices, challenges and future trends. *International Journal of Advanced Computer Science and Applications, 15*(9). https://doi.org/10.14569/IJACSA.2024.0150901 | Revista (OA) |
| R21 | Velepucha, V., & Flores, P. (2023). A survey on microservices architecture: Principles, patterns and migration challenges. *IEEE Access, 11*. https://doi.org/10.1109/ACCESS.2023.3305687 | Revista (OA) |

### 2.4 Metodología de Design Science

| # | Referencia | Medio |
|:--|:--|:--|
| R17 | Hevner, A. R., March, S. T., Park, J., & Ram, S. (2004). Design science in information systems research. *MIS Quarterly, 28*(1), 75–105. https://doi.org/10.5555/2017212.2017217 | Revista |
| R18 | Peffers, K., Tuunanen, T., Rothenberger, M. A., & Chatterjee, S. (2007). A design science research methodology for information systems research. *Journal of Management Information Systems, 24*(3), 45–77. https://doi.org/10.2753/MIS0742-1222240302 | Revista |
| R19 | Gregor, S., & Hevner, A. R. (2013). Positioning and presenting design science research for maximum impact. *MIS Quarterly, 37*(2), 337–355. https://doi.org/10.25300/MISQ/2013/37.2.01 | Revista |
| R20 | Wieringa, R. J. (2014). *Design science methodology for information systems and software engineering*. Springer. https://doi.org/10.1007/978-3-662-43839-8 | Libro |

## 3. Fuentes pendientes de verificación

- Gray, J., & Reuter, A. (1993). *Transaction processing: Concepts and techniques*. Morgan Kaufmann. `[por verificar]`
- Richardson, C. (2018). *Microservices patterns: With examples in Java*. Manning. `[por verificar]`

Verificado sin DOI (libro): Hohpe, G., & Woolf, B. (2003). *Enterprise integration patterns: Designing, building, and deploying messaging solutions*. Addison-Wesley.

## 4. Síntesis y relación con las decisiones de diseño

- **D1. Modelar la operación de pago como una saga con transacciones compensables.** García-Molina y Salem (R1) definen la saga como una secuencia de transacciones locales con compensaciones que deshacen los efectos de las ya confirmadas. Reza y Rahman (R11) y Daraghmi et al. (R10) trasladan el patrón a microservicios. Sin R1, el método no tendría la noción de compensación que lo vertebra.

- **D2 y D10. Identidad de la operación y llave de idempotencia.** Helland (R2) explica por qué, con reintentos y mensajería al-menos-una-vez, la idempotencia y la deduplicación son requisitos estructurales, no un detalle. Es la fuente que faltaba para el núcleo del método; sin ella, la idempotencia quedaría como una afirmación sin respaldo.

- **D3. Registrar el estado de la compensación para reconocer ejecuciones previas.** Daraghmi et al. (R10) documentan el registro del estado de la saga como mecanismo para decidir si una compensación ya ocurrió.

- **D4. Orden inverso y deduplicación de las compensaciones.** García-Molina y Salem (R1) establecen la ejecución en orden inverso al de la transacción original.

- **D5. Reconciliar operaciones huérfanas.** Vogels (R3) aporta el marco de consistencia eventual que hace aceptable y necesario reconciliar el estado final.

- **D6. Sustentar la tolerancia a fallos y los modos de fallo.** Cristian (R4) ofrece la taxonomía de fallos sobre la que se construye la taxonomía de fallos de compensación (componente heredado de T3).

- **D7. Verificar por inyección de fallos y pruebas de propiedades.** Hummer et al. (R5) es el antecedente directo de **verificar idempotencia**; Basiri et al. (R7) aportan la inyección de fallos controlada; Claessen y Hughes (R6) sustentan las pruebas basadas en propiedades.

- **D8. Evaluar de forma formativa y sumativa.** Venable et al. (R8) dan el marco FEDS con la secuencia formativa → sumativa que exige la calibración de maestría.

- **D9. Conducir el proceso de investigación.** Peffers et al. (R18) aportan el DSRM; Hevner et al. (R17), Gregor y Hevner (R19) y Wieringa (R20) enmarcan la contribución y el rigor.

## 5. Prueba de eliminación

El marco la supera en sus fuentes troncales: eliminar R1 suprime la noción de compensación; eliminar R2 suprime el fundamento de la idempotencia y de la deduplicación; eliminar R3 suprime la justificación de la reconciliación; eliminar R5 y R7 suprime el mecanismo de verificación. Las fuentes R13 a R16 se conservan por contexto y estado del arte, no por decisión de diseño.

## 6. Conteo de fuentes recientes

Fuentes de 2020 en adelante verificadas: **ocho** (R10–R16 y R21). Meta cumplida.

## 7. Pendientes de esta etapa

- Redactar la síntesis extensa (párrafos propios) para el documento final, sobre las fuentes ya verificadas.
- Verificar por ISBN los dos libros aún pendientes (Gray y Reuter; Richardson). No se citan hasta confirmarlos.
- La prueba de eliminación y la cobertura de decisiones están completas; las fuentes R13 a R16 y R21 son de contexto y estado del arte.
