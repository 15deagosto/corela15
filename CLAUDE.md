# Corela15 — Core financiero propio

Core financiero propio para la Cooperativa de Ahorro y Crédito 15 de Agosto
de Pilacoto (Ecuador, Segmento 2 SEPS). Repo nuevo, identidad de seguridad
nueva — **no es SIGA**. SIGA es de solo lectura sobre `Softbank` (SQL
Server); este proyecto **sí escribe** (su propia base, Postgres, nueva).

Contexto completo del dominio y la razón de ser de este proyecto:
[00-EMPEZAR-AQUI.md](00-EMPEZAR-AQUI.md), [01-contexto-origen.md](01-contexto-origen.md),
[02-arquitectura-datos-40-modulos.md](02-arquitectura-datos-40-modulos.md).

## Regla de oro

`.env` y `.env.nominal` son credenciales de **solo lectura** contra la base
real de producción (`Softbank`, SIGA). Se usan únicamente como referencia de
lectura del dominio (consultas exploratorias, nunca desde este backend). El
backend de este proyecto **nunca** se conecta a esas credenciales — solo a
Postgres (`.env.core`).

## Stack

- **Backend**: ASP.NET Core Web API (.NET 8 LTS), Clean Architecture
  (`Domain` → `Application` → `Infrastructure` → `Api`), EF Core + Npgsql,
  convención snake_case (`EFCore.NamingConventions`), Serilog.
- **Frontend**: React + TypeScript + Vite, Tailwind CSS v4, TanStack Query,
  Recharts (dashboards/análisis de datos).
- **DB**: PostgreSQL 16, Docker local (`docker-compose.yml` +
  `.env.core`, gitignored).

## Estructura

```
backend/
  Corela15.sln
  src/
    Corela15.Domain/          # entidades, sin dependencias
    Corela15.Application/     # casos de uso, interfaces (vacío por ahora)
    Corela15.Infrastructure/  # EF Core, DbContext, migraciones, configs Fluent API
    Corela15.Api/             # controllers, Program.cs
frontend/
  src/
    App.tsx, main.tsx, modules.ts, lib/api.ts
    components/   # Sidebar, TopBar, Layout, PageHeader
    pages/         # Home, ModuloPagina (placeholder genérico por módulo)
docker-compose.yml            # Postgres local
.env.core                     # credenciales del Postgres NUEVO (gitignored)
```

## Cómo levantar el entorno local

```bash
# 1. Postgres
docker compose --env-file .env.core up -d

# 2. Backend (desde backend/src/Corela15.Api) — ASPNETCORE_ENVIRONMENT=Development
# es necesario: sin eso corre en modo Production (Swagger apagado, y aunque el
# CORS de dev igual detecta el ambiente por builder.Environment, dejarlo
# explícito evita sorpresas).
CORELA15_CONNECTION="Host=localhost;Port=5432;Database=corela15_core;Username=corela15_admin;Password=<ver .env.core>" \
ASPNETCORE_URLS="http://localhost:5080" \
ASPNETCORE_ENVIRONMENT="Development" \
dotnet run

# 3. Frontend (desde frontend/)
npm run dev   # http://localhost:5173 — si ya hay otro Vite corriendo (SIGA,
               # otras apps propias en la misma máquina), Vite salta al
               # próximo puerto libre (5174, 5175...); revisar el log de
               # `npm run dev`. El backend en Development acepta CUALQUIER
               # puerto de localhost/127.0.0.1 (ver CORS en Program.cs) —
               # no hace falta tocar nada cuando cambia el puerto.
```

Migraciones (desde `backend/`):
```bash
export CORELA15_CONNECTION="Host=localhost;Port=5432;Database=corela15_core;Username=corela15_admin;Password=<ver .env.core>"
dotnet ef migrations add <Nombre> --project src/Corela15.Infrastructure --startup-project src/Corela15.Infrastructure -o Persistence/Migrations
dotnet ef database update --project src/Corela15.Infrastructure --startup-project src/Corela15.Infrastructure
```

## Convención de nombres (decisión tomada, no solo referencia)

Reusar los nombres de Softbank para toda entidad que mapee 1:1 (`Persona`,
`Cliente`, `Cuenta`, `Prestamo`, nombres de esquema en minúscula:
`sujeto`, `clientes`, `seguridad`, `general`, `contabilidad`, `ahorros`,
`colocacion`, `credito`, `inversion`, `cobranza`, `lavadoactivos`, etc.).
Motivo: facilita un futuro script de migración dato-a-dato y evita
ambigüedad al comparar ambos sistemas. Ver el detalle completo en
[02-arquitectura-datos-40-modulos.md](02-arquitectura-datos-40-modulos.md).

**Lo que NO se hereda** — la deuda técnica documentada en el mapeo, no el
nombre:
- Versionado real (tabla `*_historico` + trigger) en toda tabla que cambie
  de estado en el tiempo, desde el día uno — no como parche.
- FKs declaradas siempre (Postgres, `OnDelete` explícito).
- Auditoría real y activa desde el diseño (`seguridad.accion_ingreso_usuario`
  es el patrón bueno a replicar), nunca una tabla de auditoría declarada
  pero vacía.
- Configuración versionada, no solo datos transaccionales.

## Orden de construcción (dependencias, no preferencia)

1. **Nivel 0** — Identidad de personas, seguridad/usuarios, catálogos ✅ **hecho**
2. **Nivel 1** — Motor contable (plan de cuentas SEPS, asientos, saldos) ✅ **hecho** (estructura + versionado + caso de uso de registro de comprobantes; falta UI)
3. **Nivel 2** — Ahorros (captación a la vista) ✅ **hecho** (estructura + versionado; falta caso de uso de apertura/movimientos y UI)
4. **Nivel 3** — Plazo Fijo + Crédito/Colocación ✅ **hecho** (estructura + versionado, verificado contra Softbank; falta UI y casos de uso)
5. **Nivel 4** — Cobranzas + Cumplimiento/PLA ✅ **hecho** (núcleo verificado y simplificado; falta UI y motor de scoring PLA)
6. **Nivel 5** — Caja/Bóveda ✅ **hecho** (núcleo de Ventanilla; Bóveda y detalle de movimientos por denominación diferidos)
7. **Nivel 6** — Nómina propia ✅ **hecho** (núcleo: empleado, rol de pagos; décimos/fondos de reserva diferidos)
8. **Nivel 7** — Tesorería y activos internos ✅ **hecho** (núcleo de los 5 submódulos, verificado; detalle transaccional diferido)
9. **Nivel 8** — Riesgo y reportería regulatoria (SEPS/BCE) ✅ **hecho** (marco de riesgo genérico + índice de reportes; estructura de cada reporte pendiente de verificación regulatoria real, ver nota abajo)
10. **Periféricos** — sin orden obligatorio entre ellos ⚠️ **parcial**: `Auditoria`/`CallCenter`/`Marketing`/`Planificacion`/`HerramientaRural` tienen núcleo verificado; `Coactiva`/`Enlinea`/`SbkMovil` sin empezar (ver nota abajo)

Un nivel nunca depende de tablas/módulos de un nivel posterior. Si algo lo
necesita, está mal clasificado — revisar antes de seguir.

## Estado actual

**Nivel 0** — esquemas `sujeto` (`persona`, `persona_natural`,
`persona_juridica`), `clientes` (`cliente`), `seguridad` (`usuario`, `rol`,
`usuario_rol`, `accion_ingreso_usuario`), `general` (`empresa`, `agencia`,
`moneda`, `pais`, `tipo_identificacion`). Migración: `Nivel0_Cimientos`.

Pendiente dentro de Nivel 0 (no bloquea Nivel 1): tablas de soporte de
`SUJETO` (`CONYUGE`, `REPRESENTANTE`, `PERSONA_TELEFONO`, listas de control
PEP/sentenciados), catálogo `MENU`/`ROL_MENU` completo de seguridad,
versionado real (`*_historico`) sobre `Persona` y `Cliente` (mismo patrón
que `cuenta_contable`, ver abajo).

**Nivel 1** — esquema `contabilidad`: `cuenta_contable` (plan de cuentas,
jerárquico vía `id_cuenta_padre`, sembrado con los 7 grupos de 1er nivel y
23 subgrupos de 2do nivel del CUC de la SEPS), `tipo_comprobante_contable`
(sembrado: Ingreso/Egreso/Diario/Apertura/Cierre), `comprobante_contable`
(cabecera del asiento), `movimiento_comprobante_contable` (líneas, con
CHECK que obliga a que cada línea sea débito **o** crédito, nunca ambos ni
ninguno), `saldo_contable` (saldo por cuenta/período). Migración:
`Nivel1_MotorContable`.

**Patrón de versionado real, ya implementado y probado** (aplica el
aprendizaje de Softbank — nunca copiar la tabla a mano):
`cuenta_contable` tiene una tabla gemela `cuenta_contable_historico` +
función `contabilidad.fn_versionar_cuenta_contable()` + trigger
`trg_versionar_cuenta_contable` que graba automáticamente cada
INSERT/UPDATE/DELETE. Replicar este mismo patrón (tabla `*_historico` +
función + trigger `AFTER INSERT OR UPDATE OR DELETE`) en toda tabla futura
que necesite historial real (`CUENTA` en Nivel 2, `PRESTAMO` en Nivel 3,
etc.) — no reinventarlo cada vez, copiar la estructura de
`Nivel1_MotorContable.cs`.

Caso de uso implementado y probado end-to-end: `POST /api/contabilidad/comprobantes`
(`Corela15.Application.Contabilidad.IComprobanteContableService`, implementado
en `Corela15.Infrastructure.Services.ComprobanteContableService`) — valida
suma de débitos = suma de créditos (ese invariante no está en la base de
datos, solo el CHECK por línea lo está), valida que las cuentas existan y
sean de detalle (`es_mayor`), numera el comprobante por talonario de tipo,
y actualiza `saldo_contable` del período. Falta: pantallas en el frontend.

Nota: `Nivel0_SeedCatalogosGenerales` sembró país (Ecuador), moneda (USD),
tipos de identificación y una `Empresa`/`Agencia` mínimas — el RUC de la
empresa es un placeholder de desarrollo, actualizar antes de cualquier
ambiente real.

**Nivel 2** — esquema `ahorros`: `tipo_cuenta` (catálogo de productos,
sembrado: Ahorro a la Vista/Ahorro Infantil/Certificados de Aportación,
con `permite_debito_prestamo`/`saldo_minimo_con_prestamo`), `cuenta` (la
cuenta de ahorros, **versionada** con `cuenta_historico` + trigger),
`cuenta_cliente` (bridge cuenta↔socio con `principal` para no duplicar
montos entre cotitulares), `item_saldo` (catálogo de "baldes" de saldo:
Disponible/Encaje/Bloqueado/Interés), `tipo_cuenta_item_saldo` (qué ítems
aplican a cada producto), `cuenta_item_saldo` (saldo real por cuenta×ítem,
**también versionada**, con `acredita_prestamo`). Migración: `Nivel2_Ahorros`.

Nota de diseño importante para cuando se construya Nivel 3 (Crédito): el
campo `cuenta_item_saldo.acredita_prestamo` es una de tres configuraciones
independientes del auto-débito de cuota por SPI (junto con
`tipo_cuenta.permite_debito_prestamo` acá y `COLOCACION.PRESTAMO.DEBITOSPI`
que vivirá en Nivel 3) — el hallazgo real de la investigación original en
Softbank fue que un préstamo con `DEBITOSPI=false` igual se debitaba porque
el proceso no cruzaba las tres tablas. Cuando se implemente el caso de uso
de auto-débito, debe validar las tres como una sola fuente de verdad, no
tratarlas como flags independientes.

Caso de uso implementado y probado end-to-end: `POST /api/ahorros/cuentas`
(`Corela15.Application.Ahorros.ICuentaAhorroService`, implementado en
`Corela15.Infrastructure.Services.CuentaAhorroService`) — abre la cuenta,
crea sus `cuenta_item_saldo` según el producto, y si hay depósito inicial
**registra el comprobante contable correspondiente** vía
`IComprobanteContableService` (débito `1101` Caja / crédito `2101`
Depósitos a la vista, sembradas en `Contabilidad_SeedCuentasCajaYDepositos`)
— la integración real entre Ahorros y Contabilidad, probada de punta a
punta. Pantalla real en el frontend (`Ahorros.tsx`, formulario "Abrir
cuenta"). Nota de diseño pendiente: apertura y comprobante son dos
transacciones separadas (el servicio de comprobantes abre la suya propia)
— si el comprobante falla, la cuenta queda creada sin su asiento; aceptable
por ahora sin saga/outbox, revisar si se vuelve un problema real.

Casos de uso de depósito/retiro implementados, con un **motor contable
configurable** que corrige una falencia real de Softbank: en vez de las
tres tablas cruzadas y opacas (`FINANCIERO.TRANSACCION` →
`CONTABILIDAD.GENERADOR_CONTABLE` → `CONTABILIDAD.CAUSAL`, que la propia
investigación original documentó como difíciles de entender sin cruzar a
mano), acá `contabilidad.tipo_transaccion` es **una sola fila configurable**
por tipo de transacción: qué cuenta debita, qué cuenta acredita, qué tipo
de comprobante usa, y si suma o resta al saldo de la cuenta afectada
(`signo_saldo_cuenta`, explícito, no inferido). Sembrado: `DEP-EFEC`
(depósito en efectivo) y `RET-EFEC` (retiro en efectivo). Para agregar un
nuevo tipo de movimiento (transferencia, pago de servicio...) no hace falta
código nuevo — se inserta una fila.

`POST /api/ahorros/cuentas/{id}/movimientos`
(`ICuentaAhorroService.RegistrarMovimientoAsync`) valida saldo mínimo del
producto (`ahorros.tipo_cuenta.saldo_minimo` — falencia corregida: Softbank
solo valida mínimo si hay préstamo asociado, acá se valida siempre),
actualiza `cuenta_item_saldo`, genera el asiento vía
`IComprobanteContableService`, y deja constancia en
`ahorros.cuenta_movimiento` (bitácora con referencia directa al comprobante,
para auditar un movimiento sin cruzar tablas). Probado end-to-end: depósito,
retiro, y rechazo correcto de un retiro que dejaría el saldo bajo el
mínimo. Pantalla real en el frontend (botón "Movimiento" por cuenta).

**Nivel 3** — verificado columna por columna contra Softbank en vivo (solo
lectura, ver metodología abajo), no solo contra el resumen de
`02-arquitectura-datos-40-modulos.md`. Correcciones reales que salieron de
esa verificación: `Deposito` NO tiene FK directa a cliente (es vía
`DepositoCliente`, igual que `Ahorros.CuentaCliente`); `ClasificacionCartera`
es una tabla de **reglas** (rango de días de mora → cuenta contable → balde
de vencimiento), no un registro por préstamo — esa fue una hipótesis inicial
equivocada, corregida antes de migrar.

Esquemas `inversion` (`deposito` —versionado—, `deposito_cliente`,
`deposito_renovacion`, `item_plazo_tasa`), `credito` (`tipo_prestamo`,
`solicitud_prestamo`), `colocacion` (`prestamo` —versionado—,
`prestamo_cliente`, `prestamo_rubro` —versionado—, `rubro`,
`tipo_vencimiento`, `clasificacion_cartera`). Migración:
`Nivel3_PlazoFijoYCredito`. Sembrado: 3 tipos de préstamo, 4 rubros, 3 tipos
de vencimiento, 3 subcuentas de detalle reales del grupo CUC 14 (`1401`
por vencer / `1425` NDI / `1449` vencida) y sus 3 reglas de clasificación.

`Prestamo.DebitoSpi` es el campo real del incidente documentado en
01-contexto-origen.md — confirmado que existe tal cual en Softbank. Sigue
pendiente el caso de uso de auto-débito que cruce este campo con
`ahorros.tipo_cuenta.permite_debito_prestamo` y
`ahorros.cuenta_item_saldo.acredita_prestamo` como una sola fuente de
verdad (ver nota en Nivel 2).

**Solicitud y desembolso implementados y probados end-to-end**
(`Corela15.Application.Colocacion.IPrestamoService`, `PrestamoService`):
`POST /api/creditos/solicitudes` valida rango de monto contra el producto
(`tipo_prestamo.monto_minimo/monto_maximo`); `POST /api/creditos/
solicitudes/{id}/desembolsar` crea el `Prestamo`, genera la tabla de
amortización completa por **sistema francés** (cuota fija, capital
creciente/interés decreciente, redondeo del último período para que la
suma de capital cuadre exacto con el monto — verificado: $1000 a 12 cuotas
sumó exacto) en `PrestamoRubro`, y registra el asiento (débito `1401`
Cartera de créditos / crédito `1101` Caja) vía `DESEMB-EFEC` — mismo motor
`TipoTransaccion` de Ahorros, reusado sin cambios. Todo en una sola
transacción atómica (mismo patrón de la sección de infraestructura
transversal). `TipoPrestamo.TasaAnual` sembrada con tasas reales de
referencia (Consumo 17.20%, Microcrédito 20.50%, Productivo 10.90%).
Pantalla real en el frontend (`Creditos.tsx`: solicitar, listar, botón
"Desembolsar", cartera de préstamos).

**Nivel 4** — esquema `cobranza` (`periodo_mora` —sembrado con los 5 tramos
Preventiva→Judicial—, `accion_gestion`, `gestion_prestamo_cobranza`) y
`lavadoactivos` (`calificacion_cliente`). Simplificación consciente:
Softbank separa la gestión de cobranza en tablas de lote/asignación de
trabajo (`PRESTAMO_GESTIONAR` + `_DETALLE`) antes de llegar al registro de
contacto — acá `gestion_prestamo_cobranza` referencia el préstamo
directamente, sin heredar esa capa de asignación de trabajo (es mecanismo
de UI de Softbank, no estructura de dominio esencial). El motor de scoring
transaccional PLA (`CALIFICACIONCLIENTE_TRANSACCION`/`_DETALLE`, 6.3M/4M
filas en Softbank) queda fuera de alcance — se modeló solo el resumen de
perfil (`calificacion_cliente`), a agregar cuando se construya ese motor.
Migración: `Nivel4_CobranzasYCumplimiento`.

**Nivel 5** — esquema `cajas` (`denominacion` —sembrado con billetes/monedas
reales de USD—, `ventanilla` —sesión de caja por cajero/día—,
`ventanilla_cuadre` —cuadre diario—). Fuera de alcance: `Boveda` (espejo de
`Ventanilla` para la reserva detrás de las cajas — mismo patrón, se agrega
cuando haga falta) y el detalle de movimientos de efectivo por denominación
(`VENTANILLA_ITEMCAJA_MOVIMIENTOTRANSACCION_EFECTIVO`, 2,2M filas en
Softbank, la tabla más grande del esquema) — eso se modela junto al caso de
uso real de transacción de ventanilla, vinculado a
`FINANCIERO.MOVIMIENTO_AFECTACION` de Nivel 1. Migración: `Nivel5_Caja`.

**Nivel 6** — esquema `nomina` (`empleado`, `rol_pagos`, `rol_pagos_empleado`)
— verificado contra Softbank. Décimo tercero/cuarto, fondos de reserva y
provisión de vacaciones (obligatorios en Ecuador, `EMPLEADO_DECIMOTERCERO`/
`_DECIMOCUARTO`/`_FONDOSRESERVA`/`_PROVISION_VACACION`) quedan fuera de
alcance inicial — se agregan cuando se construya el cálculo real de rol de
pagos, no antes. Migración: `Nivel6_Nomina`.

**Nivel 7** — cinco esquemas, cada uno con su tabla núcleo verificada contra
Softbank: `obligacion` (`obligacion_financiera` — 0 filas en Softbank hoy,
la coop no tiene deuda vigente, pero la estructura la exige el grupo CUC 26),
`activofijo` (`activo`, grupo CUC 18), `cuentasporcobrar`
(`cuenta_por_cobrar`/`cuenta_por_pagar`, grupos CUC 16/25),
`proveeduria` (`articulo`), `portafolio` (`inversion_portafolio`, grupo CUC
13 — inversión propia de la coop, no confundir con los DPF de los socios
de Nivel 3). En los 5 casos se modeló solo la tabla cabecera/catálogo
núcleo, no el detalle transaccional (depreciación, traslados, tabla de
amortización, movimientos de bodega) — se agrega cuando se construya el
caso de uso real de cada uno. Migración: `Nivel7_TesoreriaYActivos`.

**Nivel 8** — esquema `riesgo` (`macroproceso` → `proceso`, `nivel_impacto`,
`nivel_probabilidad`, `nivel_riesgo` —sembrados con escalas 1-5 y matriz de
rangos—, `evento_riesgo`): patrón genérico de gestión de riesgo verificado
contra RIESGOOPERATIVO, confirmado reutilizable para cualquier tipo de
riesgo (operativo, liquidez), no solo operativo. Esquema `reportecontrol`
(`reporte_regulatorio`): sembrado con 13 códigos de reportes reales
confirmados activos en Softbank hoy (B13, D01, BCE01/02, S01, L01/L02,
IG01, TIN, UAF, RFD, ROTEF, CRS). Migración: `Nivel8_RiesgoYReporteria`.

**Advertencia explícita, no un detalle menor**: `reporte_regulatorio` es
solo un **índice** de qué reportes existen — NO se modeló la estructura de
datos de ningún reporte individual (qué campos exige, con qué fórmula, en
qué periodicidad). El propio `02-arquitectura-datos-40-modulos.md` ya
advertía esto: hay que ir "formulario por formulario contra la web de la
SEPS/BCE" antes de diseñar el `CABECERA_*`/`DETALLE_*` real de cada uno —
verificar la estructura de tablas de Softbank NO sustituye verificar la
norma oficial. No implementar el detalle de ningún reporte sin esa
verificación regulatoria explícita primero.

**Periféricos con núcleo modelado** (`Perifericos_Auditoria_CallCenter_Marketing_Planificacion_Rural`):
- `auditoria` (`area_auditoria`, `seguimiento`) — **reusa `riesgo.nivel_riesgo` de Nivel 8** en vez de duplicar el catálogo de impacto/probabilidad que Softbank sí duplica entre `AUDITORIA` y `RIESGOOPERATIVO` (mismo patrón matriz, declarado una sola vez acá).
- `callcenter` (`tipo_comentario`, `comentario`)
- `marketing` (`rifa`, `rifa_premio`)
- `planificacion` (`indicador`, `planificacion_anual` — sin el detalle por asesor/mensual)
- `herramientarural` (`tipo_producto_agrario` — solo el catálogo, sin el motor de cálculo)

**Periféricos NO modelados, a propósito**: `Coactiva` (cobro coactivo, 0 filas reales en Softbank hoy — apéndice de Cobranzas, esperar a tener un caso real), `Enlinea` (banca en línea — necesita infraestructura de autenticación/sesión de socios que este proyecto todavía no tiene, construir cuando exista ese login), `SbkMovil` (configuración de app móvil, prácticamente vacía en Softbank — 0 filas en casi todas sus tablas). `CS_CAT`/`CS_CE` (facturación electrónica SRI) tampoco se tocaron: es un subsistema de cumplimiento tributario aparte, con su propia complejidad regulatoria, fuera del alcance de "core financiero" hasta que haga falta.

### Nota operativa: incidente real en el servidor de Softbank (2026-08-13)

El servicio de SQL Server del servidor de Softbank (`192.168.0.68`, instancia `MSSQLSERVER`, SQL Server 2019) dejó de arrancar tras un reinicio por Windows Update — `master` no podía completar un script de actualización interna pendiente (`msdb110_upgrade.sql`, conflicto de collation). Se resolvió arrancando con el trace flag **`-T902`** (salta el script de actualización), que quedó configurado de forma **permanente** en SQL Server Configuration Manager → Startup Parameters. Se tomó backup de `master`, `msdb` y `Softbank` en ese momento (no existía ninguno antes). Si en el futuro Softbank no responde, revisar primero si el servicio está corriendo antes de asumir que es un problema de red/firewall.

### Metodología de verificación contra Softbank (usar para Coactiva/Enlinea/SbkMovil/CS_CAT/CS_CE, si se retoman)

`02-arquitectura-datos-40-modulos.md` documenta a fondo los Niveles 0-4
(columna por columna, contra la base real) pero los Niveles 5-8 quedaron
**solo a nivel de inventario** (qué tablas existen, cuántas filas, para qué
sirven a grandes rasgos — el propio documento lo dice explícitamente: "no
es un diseño aspiracional"). Antes de diseñar cada nivel nuevo, verificar en
vivo contra Softbank (nunca inventar estructura):

```bash
cd backend  # o cualquier carpeta temporal — es una herramienta de consulta,
            # no vive en el repo permanentemente
dotnet run --project <ruta-a-un-proyecto-console-con-Microsoft.Data.SqlClient> -- \
    "SELECT s.name, t.name, p.rows FROM sys.tables t JOIN sys.schemas s ON s.schema_id=t.schema_id JOIN sys.partitions p ON p.object_id=t.object_id AND p.index_id IN (0,1) WHERE s.name='<ESQUEMA>' ORDER BY p.rows DESC" \
    Softbank
```

Reglas: conexión con `siga_ro` (la de `.env`, `ApplicationIntent=ReadOnly`),
**solo SELECT/WITH, nunca escribir nada** — es la misma regla de oro de
siempre, ver arriba. Base real: `Softbank` (no `master`, que es solo el
default de la cadena de conexión de SIGA). Verificar columnas con
`sys.columns`/`sys.types` antes de dar por buena cualquier entidad nueva del
core propio.

## Infraestructura transversal (arquitectura, no un nivel)

**Atomicidad real entre módulos.** `ComprobanteContableService.RegistrarAsync`
detecta si ya hay una transacción ambiente en el `DbContext`
(`db.Database.CurrentTransaction`) y, si la hay, no abre la suya propia —
se suma a la del caller. Todo caso de uso que necesite hacer un cambio de
dominio + un asiento contable en la misma operación (como
`CuentaAhorroService.AbrirAsync`/`RegistrarMovimientoAsync`) abre **una
sola transacción** que envuelve ambas partes. Probado explícitamente:
forzar el fallo del comprobante (desactivando una cuenta contable) confirma
que la operación de dominio tampoco queda persistida. Replicar este mismo
patrón en desembolso de préstamo y cualquier caso de uso futuro que cruce
dos módulos — nunca dos `SaveChanges`/transacciones sueltas para una sola
operación de negocio.

**Manejo de errores centralizado.** `Corela15.Application.Common.DomainException`
(con `ReglaDeNegocioException` → HTTP 422 y `SolicitudInvalidaException` →
HTTP 400) es la base de toda excepción de caso de uso. `Corela15.Api.
ExceptionHandling.DomainExceptionHandler` (`IExceptionHandler` de .NET 8,
registrado en `Program.cs`) las traduce a `ProblemDetails` automáticamente
— los controllers **no** llevan `try/catch` por excepción; un caso de uso
nuevo hereda el manejo correcto solo con heredar de la base adecuada.

## Frontend — módulos visibles al usuario

`frontend/src/modules.ts` define los módulos con nombre/ícono/estado
amigables para quien usa el sistema — la jerga interna de "Nivel 0/1/2..."
vive solo en este documento y en los `.md` de arquitectura, nunca en la UI.
Al completar un nivel, actualizar el `estado` del módulo correspondiente
(`disponible` / `en-construccion` / `proximamente`).

Pantallas reales ya construidas (tabla + búsqueda, estilo SIGA):
`Socios`, `UsuariosRoles`, `Contabilidad` (plan de cuentas),
`Ahorros` (productos + cuentas) — cada una consume un endpoint `GET` de
solo lectura (`SociosController`, `UsuariosController`,
`CuentasContablesController`, `AhorrosController`) que consulta
`Corela15DbContext` directo desde el controller, sin pasar por
`Application` — es la convención para lecturas simples sin lógica de
negocio; los casos de uso que escriben sí van por Application (ver
`ComprobanteContableService`). El resto de los slugs de `modules.ts` caen
en `ModuloPagina`, la pantalla placeholder genérica.

`Nivel0_SeedDatosPrueba` sembró 5 socios y 2 usuarios de **ejemplo**
(nombres/cédulas ficticios) solo para que estas pantallas tengan contenido
real en desarrollo local — no son datos de producción, no hay flujo de
login todavía (`hash_contrasena` es un placeholder literal).
