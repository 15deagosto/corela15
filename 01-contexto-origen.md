# Contexto de origen — de dónde sale este mapeo

Resumen para que la sesión nueva entienda el "por qué" sin releer la
conversación original completa.

## El proyecto que originó todo esto: SIGA

La Cooperativa de Ahorro y Crédito 15 de Agosto de Pilacoto (Ecuador,
Segmento 2 SEPS) construyó **SIGA**, una herramienta de reportería y
tableros de solo lectura sobre su core financiero real, **Softbank**
(Microsoft SQL Server, ~2.500 tablas en 40 esquemas). SIGA nunca escribe en
Softbank — es estrictamente `SELECT`, con un login de base de datos
`db_datareader` exclusivo.

Durante el desarrollo y uso de SIGA salieron varios hallazgos técnicos
reales trabajando con datos de producción (con autorización, con las
protecciones de solo lectura del caso):

1. **Incidente de auto-débito SPI**: el proceso que descuenta
   automáticamente la cuota de un préstamo cuando llega el sueldo del socio
   por transferencia interbancaria (SPI/Banco Central) tenía comportamiento
   inconsistente — se investigó a fondo cruzando el historial temporal
   nativo de SQL Server (`FOR SYSTEM_TIME ALL`) de las tablas de préstamo,
   y se encontró que el proceso ignoraba un campo diseñado exactamente para
   controlar esto (`PRESTAMO.DEBITOSPI`), entre otras inconsistencias de
   configuración cruzada entre tres tablas de parámetros distintas.
2. **Inconsistencia de tasas en renovación de Plazo Fijo (DPF)**: la tasa
   aplicada al renovar un certificado a menudo no coincidía con el tablero
   de tasas vigente, y las variaciones de tasa autorizadas por asesores no
   siempre se reflejaban correctamente.
3. **Mapa de permisos de usuarios**: se construyó una pantalla en SIGA
   (`/admin/permisos`) para ver, de forma filtrable, qué rol y qué acceso
   granular tiene cada uno de los usuarios activos del core — reemplazando
   la revisión manual uno por uno en la interfaz de Softbank.

Estos tres hallazgos llevaron a mapear en profundidad el modelo de datos
real de Softbank (identidad de personas, seguridad, contabilidad, ahorros,
crédito, cobranzas, cumplimiento) — y de ahí surgió la idea de este
proyecto nuevo: usar todo ese conocimiento de negocio ya verificado para
construir, poco a poco y en orden de dependencias, un **core financiero
propio moderno**, sin partir de cero en el entendimiento del dominio.

## Grounding regulatorio ya verificado

Durante el mapeo se contrastó contra normativa oficial real, no solo contra
lo que Softbank implementó:

- **Catálogo Único de Cuentas (CUC)** de la SEPS — descargado y verificado
  directamente de `seps.gob.ec` (Resolución
  SEPS-IGT-IGS-INSESF-INR-INFMR-INGINT-2022-0194, codificado con reforma de
  octubre 2023). Confirma la estructura de cuentas 1-Activo, 2-Pasivo,
  3-Patrimonio, 4-Gastos, 5-Ingresos, 6-Contingentes, 7-Orden, y el detalle
  completo del grupo 14 (Cartera de Créditos) que valida la fórmula de Mora
  SEPS = (Vencido + No Devenga Interés) / Total.
- **Norma de Gestión del Riesgo de Crédito (JPRF)** — referenciada para la
  escalera de calificación de riesgo (A-1 a E) y el esquema de 28 segmentos
  de calificación contable.
- **Ley de Prevención de Lavado de Activos y UAF** — referenciada para el
  módulo de cumplimiento/PLA.

## Reglas que se mantuvieron durante toda la investigación (aplicables también al proyecto nuevo)

- Nunca se generó ni ejecutó SQL de escritura contra Softbank.
- Nunca se decompiló ni se hizo ingeniería inversa del software del
  proveedor (cliente WPF, servidor de aplicación) — toda la investigación
  fue contra la base de datos (solo lectura) y las pantallas visibles de la
  aplicación, con autorización.
- Los hallazgos con datos personales de socios (el incidente SPI puntual)
  se investigaron con la conexión de datos elevada específica para ese
  propósito, auditada, nunca con el acceso general de solo lectura — y esa
  información puntual de socios **no forma parte de este mapeo de
  arquitectura** (que es solo estructura de tablas/columnas, sin datos).
