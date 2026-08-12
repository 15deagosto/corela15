# Core financiero propio — semilla del proyecto

Esta carpeta es todo lo que hace falta para arrancar el proyecto nuevo (repo
nuevo, sesión nueva de Claude Code). Copiala completa a la raíz del proyecto
nuevo y empezá desde ahí — no hace falta releer nada de la sesión de SIGA.

## Por qué es un proyecto aparte y no una carpeta dentro de SIGA

SIGA tiene una regla inviolable: **solo emite `SELECT`**, nunca escribe en
la base del core (`Softbank`). Un core financiero propio es, por
definición, un sistema que sí escribe (`INSERT`/`UPDATE`/`DELETE` todo el
día). Mezclar ambas identidades de seguridad en el mismo repo, aunque sea
en carpetas separadas, es exactamente el tipo de confusión que esa regla de
SIGA existe para evitar. Por eso: repo nuevo, `CLAUDE.md` nuevo, sesión
nueva.

## Qué contiene esta carpeta

- **`01-contexto-origen.md`** (este resumen) — de dónde viene todo esto y
  cómo se construyó, para que la sesión nueva tenga contexto sin tener que
  releer la conversación original.
- **`02-arquitectura-datos-40-modulos.md`** — el mapeo completo de los 40
  módulos (esquemas) del core actual (`Softbank`, SQL Server), verificado
  tabla por tabla contra la base real, con:
  - Los 5 niveles núcleo (Personas/Seguridad, Contabilidad, Ahorros,
    Plazo Fijo/Crédito/Colocación, Cobranzas/Cumplimiento) documentados a
    fondo: tablas, columnas, filas reales, y grounding regulatorio real
    (Catálogo Único de Cuentas de la SEPS, Norma de Riesgo de Crédito JPRF,
    Ley de Prevención de Lavado de Activos).
  - Los 27 módulos restantes (Caja, Nómina, Tesorería, Riesgo/Reportería
    regulatoria, periféricos e infraestructura técnica) a nivel de
    inventario, listos para profundizar cuando les toque el turno.
  - La convención de nombres: **reusar los nombres de Softbank** para toda
    entidad que mapee 1:1 (facilita migración futura), **sin heredar su
    deuda técnica** (versionado real desde el día uno, FKs declaradas,
    configuración auditada de verdad).

## El orden de construcción recomendado (dependencias, no preferencia)

Un nivel nunca depende de algo de un nivel posterior — si en algún momento
parece que sí, es que algo está mal clasificado y hay que revisar antes de
seguir.

1. **Nivel 0** — Identidad de personas, seguridad/usuarios, catálogos generales
2. **Nivel 1** — Motor contable (plan de cuentas SEPS, asientos, saldos)
3. **Nivel 2** — Ahorros (captación a la vista)
4. **Nivel 3** — Plazo Fijo + Crédito/Colocación
5. **Nivel 4** — Cobranzas + Cumplimiento/Prevención de Lavado de Activos
6. **Nivel 5** — Caja/Bóveda
7. **Nivel 6** — Nómina propia (empleados de la cooperativa, no socios)
8. **Nivel 7** — Tesorería y activos internos (Obligaciones financieras, Cuentas por Cobrar, Activo Fijo, Proveeduría, Portafolio propio)
9. **Nivel 8** — Riesgo (liquidez/operativo) y reportería regulatoria (SEPS/BCE)
10. **Periféricos** — cuando se necesiten, sin orden obligatorio entre ellos

## Primer paso sugerido en la sesión nueva

1. Pegar/copiar esta carpeta como semilla del `CLAUDE.md` del proyecto nuevo
   (o referenciarla directamente).
2. Definir el stack técnico del proyecto nuevo (puede ser el mismo de SIGA
   — .NET + React — u otro; es una decisión nueva, no heredada).
3. Empezar por **Nivel 0**, con el detalle de tablas que ya está en
   `02-arquitectura-datos-40-modulos.md`, diseñando el esquema propio
   (mismos nombres de entidad, estructura propia y moderna).
4. Recién ahí Nivel 1, y así en orden.

## Una aclaración importante para la sesión nueva

Todo lo de `02-arquitectura-datos-40-modulos.md` es **documentación de qué
información hace falta modelar**, sacada de observar un sistema en
producción real (con autorización, de solo lectura, para este propósito).
No es ni debe tratarse como código o diseño interno del proveedor —
ninguna consulta SQL de exploración capturó lógica de aplicación compilada
ni se decompiló nada. Es el equivalente a documentar qué reportes pide la
SEPS y qué datos hacen falta para producirlos, no una copia del software de
terceros.
