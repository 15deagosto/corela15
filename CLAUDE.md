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
npm run dev   # http://localhost:5174 — puerto fijo (vite.config.ts,
               # server.port + strictPort:true) para no chocar con otras
               # apps propias en la misma máquina (SIGA, CredVault COAC,
               # etc.) que ya ocupan 5173/5180/otros. Con strictPort, si
               # 5174 ya está en uso Vite falla en vez de saltar de puerto
               # en silencio — revisar qué lo está usando antes de asumir
               # que cambió solo. El backend en Development acepta
               # CUALQUIER puerto de localhost/127.0.0.1 (ver CORS en
               # Program.cs) — no hace falta tocar nada por este cambio.
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
5. **Nivel 4** — Cobranzas + Cumplimiento/PLA ✅ **hecho** (registro de gestión de cobranza con UI real; falta motor de scoring PLA y cálculo de mora)
6. **Nivel 5** — Caja/Bóveda ✅ **hecho** (apertura/cierre de ventanilla con UI real; Bóveda y detalle de movimientos por denominación diferidos)
7. **Nivel 6** — Nómina propia ✅ **hecho** (generación de rol de pagos con UI real; décimos/fondos de reserva diferidos)
8. **Nivel 7** — Tesorería y activos internos ✅ **hecho** (registro/abono de cuentas por cobrar con UI real; detalle transaccional de los otros 4 submódulos diferido)
9. **Nivel 8** — Riesgo y reportería regulatoria (SEPS/BCE) ✅ **hecho** (registro de evento de riesgo con UI real; índice de reportes sin estructura de detalle, ver nota abajo)
10. **Periféricos** — sin orden obligatorio entre ellos ⚠️ **parcial**: `Auditoria`/`CallCenter`/`Marketing`/`Planificacion`/`HerramientaRural` tienen núcleo verificado; `Coactiva`/`Enlinea`/`SbkMovil` sin empezar (ver nota abajo)

Un nivel nunca depende de tablas/módulos de un nivel posterior. Si algo lo
necesita, está mal clasificado — revisar antes de seguir.

## Módulo Configuración (parametrización, transversal a todos los niveles)

Cada nivel construido hasta ahora sembró sus catálogos configurables
directo por migración (`tipo_transaccion`, `tipo_prestamo`, `rubro`,
`accion_gestion`, `denominacion`, `nivel_impacto`/`probabilidad`/`riesgo`,
etc.) — correcto para levantar el sistema, pero un core real no puede
depender de una migración para que alguien cambie una tasa o agregue una
agencia. **Decisión tomada**: un módulo `Configuración` centralizado en el
sidebar (no una pestaña "Parámetros" dentro de cada módulo existente) con
una pantalla CRUD por catálogo, empezando por Nivel 0 y avanzando en el
mismo orden 0→8 ya establecido para los casos de uso.

**Convención arquitectónica explícita para estos catálogos**: a diferencia
de los casos de uso de negocio (comprobantes, préstamos, ventanillas...),
que siempre van por `Application` con su propio servicio e interfaz, el
CRUD de un catálogo de configuración simple (código+nombre, sin más regla
que unicidad) se resuelve **directo en el controller contra el
`DbContext`**, lanzando `Corela15.Application.Common.CodigoDuplicadoException`
(422, vía el mismo `DomainExceptionHandler`) cuando corresponda. Escribir
una interfaz + servicio Application por catálogo sería una capa que solo
reenvía la llamada sin agregar lógica real — boilerplate puro. Si un
catálogo empieza a necesitar una regla de negocio de verdad (ej. no poder
desactivar una cuenta contable con saldo), ESE catálogo migra a
Application con su propio caso de uso; el resto se queda en el patrón
simple. No mezclar los dos patrones dentro del mismo catálogo.

**Nivel 0 — hecho**: `Corela15.Api.Controllers.ConfiguracionController`
(`api/configuracion/*`) con CRUD completo (crear + listar + actualizar,
sin borrado físico — coherente con que ninguna tabla del core permite
DELETE real, solo estados) de `paises`, `monedas`, `tipos-identificacion`,
`agencias` (con toggle `Activa`/`EsOperativa`), `roles`, y `empresa` (fila
única, solo `GET`/`PUT`, sin `POST`). Probado vía curl: creación,
duplicado de código rechazado (422), actualización. Pantalla real
(`Configuracion.tsx`) con pestañas por catálogo — `TabCatalogoCodigoNombre`
es un componente genérico reutilizado por Países y Tipos de identificación
(mismo shape código+nombre), evitando duplicar el formulario dos veces;
Monedas/Agencias/Roles/Empresa tienen su propia pestaña por tener campos
distintos. **Nivel 1 — hecho**: `plan-cuentas` y `tipos-comprobante`
agregados a `Configuracion.tsx`. A diferencia de los catálogos de Nivel 0,
el plan de cuentas sí tiene invariantes reales (versionado por trigger,
jerarquía padre/hijo, no se puede desactivar una cuenta con saldo
distinto de cero o en uso por un tipo de transacción activo) — por eso
`ICuentaContableAdminService`/`CuentaContableAdminService` pasan por
`Application`, rompiendo a propósito el patrón "directo contra el
DbContext" que sí aplica a los catálogos simples (documentado en el
propio código como la razón del quiebre de convención). `POST /api/
configuracion/plan-cuentas` crea una subcuenta nueva (código único,
grupo/naturaleza/es_mayor fijos desde la creación — cambiarlos después
rompería la integridad de asientos ya registrados); `PUT` solo permite
tocar nombre y estado activo. Probado end-to-end: creación de subcuenta
bajo un grupo válido, rechazo de padre inválido (una cuenta de detalle no
puede tener subcuentas), desactivación de una cuenta sin saldo (éxito),
desactivación de `1101` Caja rechazada (422, está en uso por
`DEP-EFEC`/`RET-EFEC`/etc). `tipo_comprobante_contable` sí es un catálogo
simple (Ingreso/Egreso/Diario/Apertura/Cierre, sin invariante más allá de
código único) — CRUD directo en `ConfiguracionController`, mismo patrón
que Países/Monedas. Pendiente: catálogos de Nivel 2 en adelante — seguir
en el mismo orden.

**Productos financieros — hecho** (agregado después de cerrar seguridad/
regulatorio, priorizado explícitamente por ser lo que un usuario real del
sistema — no solo el equipo de desarrollo — necesita tocar seguido):
`Configuracion.tsx` creció a 5 pestañas más, con los componentes movidos a
`ConfiguracionProductos.tsx` (el archivo ya rondaba 1000 líneas, separar
por responsabilidad en vez de seguir agregando a un solo archivo).

- **Productos de ahorro** (`tipos-cuenta`, Nivel 2) — catálogo simple,
  CRUD directo: código, nombre, saldo mínimo, tasa de interés anual (%,
  el formulario convierte automáticamente a fracción antes de mandar al
  API), débito automático de préstamo.
- **Productos de crédito** (`tipos-prestamo`, Nivel 3) — **sí pasa por
  Application** (`ITipoPrestamoAdminService`/`TipoPrestamoAdminService`):
  valida la tasa contra el techo BCE vigente del segmento elegido en el
  mismo momento de guardar el producto, reusando el mismo chequeo que
  `IPrestamoService.SolicitarAsync` — el error se detecta al configurar,
  no cuando un socio ya está esperando una solicitud. El selector de
  "Segmento BCE" en el formulario es una lista fija de los 10 segmentos
  reales (no texto libre, evita typos que romperían la validación).
- **Tasas techo BCE** (`tasas-techo-bce`) — hasta ahora solo eran
  editables por migración (documentado como pendiente en la sección de
  Tasas techo BCE más abajo) — ya no. El formulario reafirma la regla ya
  documentada en el código: nunca se edita una tasa histórica, se agrega
  una fila nueva con su fecha de vigencia (rechazo real de duplicado
  exacto segmento+fecha, probado).
- **Tablero de tasas DPF** (`tablero-tasas-dpf`, Nivel 3) — mismo patrón
  que tasas BCE: crear un rango nuevo (plazo/monto/tipo de persona/tasa/
  vigencia), editar solo toca la tasa y el estado activo de un rango
  existente.
- **Categorías de riesgo de cartera** (`categorias-riesgo-cartera`,
  motor de provisiones) — las 9 categorías (A1-E) son fijas, no se crean
  ni se borran desde acá (reflejan la norma, no son un catálogo abierto);
  el formulario solo permite editar rango de días de mora y % de
  provisión de una categoría existente.

Probado end-to-end contra la API real: edición de producto de ahorro,
creación de rango DPF nuevo, ambos verificados y limpiados después.

## Autenticación real (JWT + roles + menús)

Hasta este punto el core no tenía autenticación real: `hash_contrasena` era
un placeholder literal, no había login, y cada caso de uso recibía
`RegistradoPor` como un string libre que mandaba el frontend (`front:xxx`)
— cualquiera podía decir ser cualquiera, y no había forma real de auditar
quién hizo qué. Identificado como el gap más crítico (evaluación de
seguridad/SEPS) antes de seguir agregando módulos, porque cuanto más se
demore más caro sale retrofittearlo.

**Modelo de permisos**: `seguridad.menu` (código = `slug` de
`frontend/src/modules.ts`) + `seguridad.rol_menu` (N:M rol↔menú) —
pendiente documentado desde Nivel 0, implementado ahora. Migración
`Nivel0_MenuRolMenu`: sembró un menú por módulo y permisos por defecto
(ADMINISTRADOR ve todo; CAJERO → cajas/ahorros/socios; ASESOR DE CREDITO →
creditos/cobranzas-cumplimiento/socios; OFICIAL DE CAPTACIONES →
ahorros/creditos/socios) — ajustable después desde una pantalla de
Configuración > Roles todavía no construida (hoy solo por SQL/migración).

**Contraseñas reales**: `Nivel0_PasswordHashReal` reemplazó el placeholder
por un hash BCrypt real (`BCrypt.Net-Next`) para los usuarios de ejemplo
`admin`/`mguaman` — contraseña de desarrollo **`Corela15!Dev`**, documentada
acá a propósito porque son datos de ejemplo, nunca usar este patrón en un
ambiente real.

**JWT** (`Corela15.Application.Seguridad.IAuthService` /
`Corela15.Infrastructure.Services.AuthService`): `POST /api/auth/login`
valida contra el hash BCrypt, registra el intento en
`seguridad.accion_ingreso_usuario` **siempre** (éxito o fallo — antes esa
tabla no se usaba en ningún flujo real), y si es válido emite un JWT
(HMAC-SHA256, secreto en `.env.core` vía `Jwt__Secret`/`Issuer`/`Audience`/
`ExpiryMinutes`, nunca hardcodeado) con los roles y los **códigos de menú
permitidos** como claims — el permiso se calcula una sola vez al login, no
en cada request. `GET /api/auth/me` devuelve la sesión actual leyendo los
claims del token, sin volver a golpear la base.

**Autorización por menú**: en `Program.cs`, una policy `Menu:<código>` por
cada uno de los 11 menús (`RequireClaim("menu", codigo)`), y
`FallbackPolicy = RequireAuthenticatedUser()` — cualquier endpoint nuevo
queda protegido por default con solo heredar de `ControllerBase`, no hace
falta acordarse de agregarlo. Cada controller de negocio lleva
`[Authorize(Policy = "Menu:xxx")]` a nivel de clase; `AuthController.Login`
y `/health` son los únicos `[AllowAnonymous]`.

**Retrofit de `RegistradoPor`**: en todos los casos de uso que escriben
(Ahorros, Créditos, Plazo Fijo, Cobranzas, Cajas, Tesorería), el valor ya
no lo manda el cliente — se deriva de `User.Identity.Name` (el claim del
JWT) en el controller. Esto obligó a introducir un DTO de *body* separado
del *request* de `Application` en los endpoints donde antes se bindeaba
directo (`AbrirCuentaBody`, `AbrirDepositoBody`, `SolicitarPrestamoBody`,
`RegistrarGestionBody`, `RegistrarCuentaPorCobrarBody`): si `RegistradoPor`
sigue siendo un campo no-nullable en el record de `Application` y el JSON
del cliente ya no lo manda, la validación automática de `[ApiController]`
lo rechaza con 400 **antes** de que el controller pueda sobrescribirlo —
bug real encontrado y corregido durante la prueba end-to-end, no una
elección de diseño. La cuenta de ventanilla (`AbrirVentanillaRequest`)
tuvo el mismo tratamiento: ya no se elige el cajero de una lista, la
ventanilla se abre para el usuario autenticado.

**Frontend**: `AuthContext`/`AuthProvider` (localStorage, no cookies —
simple para desarrollo local, revisar si se necesita httpOnly cookie en un
ambiente real expuesto a internet) + interceptor de axios que agrega
`Authorization: Bearer` a cada request y desloguea automáticamente en un
401. `RequireAuth` envuelve el `Layout` y redirige a `/login` si no hay
sesión. `Sidebar` filtra los módulos visibles según los menús permitidos
del usuario (`tieneMenu`), no solo según el catálogo `modules.ts` como
antes. `TopBar` muestra usuario/roles y logout. Los `registradoPor:
'front:xxx'` hardcodeados desaparecieron de las 5 pantallas que los tenían.

Probado end-to-end: request sin token → 401; login con contraseña
incorrecta → 401 (y se audita el intento fallido); login correcto → JWT
con roles/menús correctos; request con token a un menú no permitido → 403;
request a un menú permitido → 200; escritura real (registro de CxC,
apertura de cuenta, solicitud de crédito, apertura/cierre de ventanilla)
con `RegistradoPor`/usuario de la ventanilla verificado en la base como el
usuario autenticado real, no un string inventado por el cliente.

Pendiente (no bloquea, pero es lo que sigue de la lista de gaps
identificada): pantalla de administración de `rol_menu` ✅ **hecho** (ver
sección "Administración de permisos por rol" más abajo), y revocación
activa de tokens ✅ **hecho** (ver sección "Revocación de tokens" más
abajo). Concurrencia optimista e idempotencia — los dos puntos
que seguían en la lista de gaps críticos — ya están resueltos, ver
sección siguiente.

## Concurrencia optimista e idempotencia

Dos gaps críticos más de la evaluación de seguridad: sin control de
concurrencia, dos escrituras simultáneas sobre el mismo saldo podían
pisarse en silencio (el "problema del último que escribe gana" clásico);
sin idempotencia, un timeout de red o un doble-clic en el frontend podía
duplicar un depósito o un pago de cuota real.

**Concurrencia optimista real, sin migración de esquema**: Postgres ya
trae una columna de sistema `xmin` en toda tabla (el número de transacción
que escribió la fila más reciente) — en vez de agregar una columna
`RowVersion` propia, se mapeó `xmin` como concurrency token vía
`b.Property<uint>("xmin").HasColumnName("xmin").IsRowVersion()` en las 6
entidades financieras donde dos escrituras concurrentes son un riesgo
real: `Cuenta`/`CuentaItemSaldo` (Ahorros), `Prestamo` (Colocación),
`Deposito` (Plazo Fijo), `CuentaPorCobrar` (Tesorería), `Ventanilla`
(Cajas), y `SaldoContable` (Contabilidad — el punto de mayor contención
real del sistema, toda cuenta/período recibe comprobantes concurrentes
legítimos). **Cuidado real encontrado durante la implementación**: el
helper `UseXminAsConcurrencyToken()` de Npgsql (y también el patrón
"moderno" recomendado en su mensaje de obsolescencia) generan un
`AddColumn xmin` en el archivo C# de la migración — parece una migración
real y peligrosa (`xmin` es un nombre de columna reservado por Postgres,
un `ALTER TABLE ADD COLUMN xmin` normal fallaría). Se verificó con
`dotnet ef migrations script` antes de aplicar: el generador SQL de
Npgsql reconoce el nombre reservado y **no emite ningún DDL real** para
esa columna — la migración es histórica/bookkeeping puro. Nunca aplicar
una migración así sin verificar el script primero si el nombre de columna
suena sospechoso.

`Corela15.Application.Common.ConflictoConcurrenciaException` (409) es la
traducción estándar de `DbUpdateConcurrencyException`. Dos tratamientos
distintos, a propósito: **`ComprobanteContableService`** reintenta hasta 3
veces (contención esperada bajo carga normal en `saldo_contable` — dos
depósitos distintos a la misma hora no son un error, son el caso común),
destrackeando por completo las entidades de saldo tocadas en cada
reintento (nunca reutiliza estado a medio aplicar de un intento anterior,
evita duplicar el delta sobre una fila que nunca llegó a fallar). El resto
de servicios (`CuentaAhorroService`, `PrestamoService`, `DepositoService`,
`CuentaPorCobrarService`, `VentanillaService`) no reintentan — un choque
ahí sí es una señal real de dos usuarios operando la misma cuenta/préstamo
a la vez, se rechaza con 409 y el cliente decide si reintentar. Probado
con una carrera real (10 depósitos verdaderamente concurrentes —
`curl ... & ... & wait` — sobre la misma cuenta): exactamente 1 de 10 tuvo
éxito, los otros 9 devolvieron 409, y el saldo final quedó exacto (sin
pérdida ni duplicación de escritura).

**Idempotencia real en las operaciones que mueven dinero**:
`seguridad.solicitud_idempotente` (clave + usuario + ruta, único) +
`Corela15.Api.Idempotencia.IdempotenciaFilter` (un `IAsyncActionFilter`
global, activado solo en los endpoints marcados con
`[RequireIdempotencyKey]`): exige el header `Idempotency-Key`
(`FaltaIdempotencyKeyException`, 400, si falta), y si esa clave ya se
procesó para el mismo usuario y la misma ruta, devuelve la respuesta
original guardada **sin volver a ejecutar el caso de uso** — nunca
duplica el movimiento. Solo se guarda la respuesta de una ejecución
exitosa (2xx); un intento fallido no bloquea reintentar con la misma
clave. Aplicado a los endpoints que mueven dinero real: `POST /api/
ahorros/cuentas/{id}/movimientos`, `POST /api/creditos/solicitudes/{id}/
desembolsar`, `POST /api/creditos/prestamos/{id}/pagos`, `POST /api/
plazofijo/depositos` y `.../cancelar`, `POST /api/tesoreria/
cuentas-por-cobrar/{id}/abonos`. La respuesta guardada se serializa con
las mismas `JsonOptions` que usa MVC (`IOptions<Microsoft.AspNetCore.Mvc.
JsonOptions>`) — bug real encontrado y corregido en la prueba: sin esto,
la respuesta cacheada salía en PascalCase (`System.Text.Json` por
defecto) en vez de camelCase, inconsistente con toda otra respuesta de la
API. Probado end-to-end: sin header → 400; mismo header repetido dos veces
→ segunda respuesta idéntica byte a byte a la primera, saldo solo se
mueve una vez; header distinto → operación nueva de verdad, sí se aplica.
**Limitación conocida, no bloqueante**: si dos requests con la
**misma** clave llegan verdaderamente en simultáneo (no un reintento
secuencial, sino dos en paralelo exacto), ambos pueden pasar el chequeo
"¿existe ya?" antes de que cualquiera termine de escribir — el índice
único evitaría el registro duplicado en la tabla de idempotencia, pero
podría dejar pasar dos ejecuciones reales del caso de uso. Caso de
probabilidad muy baja (indica un bug del cliente, no un timeout/reintento
normal) — no se resolvió con un lock explícito, documentado a propósito.

**Bug real encontrado después de marcar los endpoints con
`[RequireIdempotencyKey]`**: el frontend nunca mandaba el header —
`Ahorros.tsx`, `Creditos.tsx` (desembolsar, pago de cuota, abrir/cancelar
DPF) y `Tesoreria.tsx` (abonar) hubieran quedado rotos en producción con
400 en cada intento. Corregido agregando `Idempotency-Key: crypto.
randomUUID()` a cada mutación afectada — una clave nueva por cada
`mutate()` (una acción real del usuario), no por render del componente,
así que un reintento interno de la misma llamada reutiliza la clave
correcta pero una acción nueva del usuario siempre genera una operación
nueva. Verificado end-to-end contra la API real después del fix.

## Motor de provisiones de cartera

Segundo punto del tier regulatorio (Art. 44, Norma para la Gestión del
Riesgo de Crédito en las COAC): toda cartera de crédito vigente debe
tener una provisión (reserva para incobrables) calculada según cuántos
días de mora tiene, no un valor fijo. `colocacion.categoria_riesgo_cartera`
(`Corela15.Domain.Colocacion.CategoriaRiesgoCartera`) es la matriz real de
9 categorías (A1-A3 riesgo normal, B1-B2 potencial, C1-C2 deficiente, D
dudoso recaudo, E pérdida) — **distinta de `ClasificacionCartera`** (esa
clasifica el balde de presentación del balance — por vencer/NDI/vencida —
no el % de provisión regulatorio; son dos clasificaciones relacionadas
pero no la misma cosa en la norma real, confusión fácil de cometer).
Porcentajes de provisión verificados por búsqueda (piso de cada rango
oficial: A1 1%, A2 2%, A3 3%, B1 6%, B2 10%, C1 20%, C2 40%, D 60%, E
100%). **Advertencia explícita**: los rangos de días de mora sembrados
(A2 1-8, A3 9-15, B1 16-30, B2 31-45, C1 46-70, C2 71-90, D 91-120, E
121+) son la convención estándar SEPS/Superbancos para Consumo/
Microcrédito — el PDF oficial (`Calificacion-activos-riesgo.pdf` y el
Manual Técnico de Operaciones de Cartera de SEPS) no se pudo extraer
programáticamente (streams binarios comprimidos) para verificar el rango
exacto columna por columna como se hizo con Softbank en Niveles 0-4;
verificar contra el PDF oficial antes de usar esto para un reporte
regulatorio real, no solo para uso interno.

Cuentas reales agregadas: `1499` Provisión para créditos incobrables
(contra-activo bajo el grupo 14, naturaleza Acreedora aunque vive bajo
Activo — así es como el CUC real trata las provisiones) y `4402`
Provisión para cartera de crédito (bajo el grupo 44 Provisiones, ya
sembrado desde Nivel 1). Motor `PROV-CART` (débito 4402 / crédito 1499),
mismo patrón que el resto.

**`IProvisionCarteraService.EjecutarCalculoAsync`** (`POST /api/creditos/
provision-cartera/calcular`): para cada préstamo vigente, calcula días de
mora reales (hoy − fecha de vencimiento de la cuota de capital impaga más
antigua, usando `PrestamoRubro.FechaFin` — la falta de este cálculo
estaba documentada como pendiente desde Nivel 4, ahora existe acá),
clasifica en su categoría, y aplica el % sobre el saldo. Solo registra el
asiento cuando la provisión requerida total **supera** la ya acumulada
(consulta `saldo_contable` de la cuenta `1499` del período) — un
incremento real. Si diera menor, **no reversa automáticamente**: una
reducción de provisión es una decisión que requiere revisión humana, no
algo que un batch deba decidir solo — limitación documentada a propósito,
no un olvido. Diseñado para correrse periódicamente (mensual como
mínimo), no por préstamo individual — de ahí que no lleve
`[RequireIdempotencyKey]`: correrlo dos veces el mismo día es seguro por
diseño (la segunda vez ve que ya no hace falta incremento), no necesita
protección adicional contra reintentos.

Probado end-to-end: cartera vacía → provisión $0; préstamo real de $1,000
con la cuota 1 forzada a 50 días de mora → clasificado correctamente en
C1 (20%), provisión de $200 registrada, `1499`/`4402` verificados en
$200 exactos; segunda corrida el mismo día → incremento $0 (no duplica).

## Devengo de interés y cierre de período contable

Últimos dos puntos del motor bancario real que faltaba: hasta acá todo se
calculaba solo en el momento de la transacción — un ahorro no ganaba
interés día a día como en un banco real, y se podía contabilizar
retroactivo sobre un mes ya cerrado y reportado sin que nada lo impidiera.

**Devengo de interés sobre ahorros** (`Corela15.Application.Ahorros.
IDevengoInteresService` / `DevengoInteresService`, `POST /api/ahorros/
devengo-interes/ejecutar`): `TipoCuenta.TasaInteresAnual` (nueva) — Ahorro
a la Vista sembrado con 2% nominal anual (tasa pasiva de referencia real
para una COAC Segmento 2, no una tasa techo regulatoria como la de
créditos, así que no exige la misma verificación exacta contra circular
oficial); Ahorro Infantil y Certificados de Aportación quedan en 0% por
decisión de producto (Certificados es capital social, no captación
remunerada en este diseño). Para cada cuenta activa con tasa > 0, calcula
`interés_día = saldo disponible × (tasa_anual / 365)` y lo acredita al
balde de saldo **"Interés por pagar"** (`item_saldo` código `INT`) — este
balde ya existía en el catálogo desde Nivel 2 (sembrado desde el
principio, sin usar hasta ahora), confirmando que el diseño original ya
había previsto este mecanismo. Un asiento consolidado por corrida
(débito `4101` Intereses causados en depósitos / crédito `2503`
Intereses por pagar sobre depósitos — ambas subcuentas nuevas, bajo los
grupos `41`/`25` ya sembrados desde Nivel 1), no uno por cuenta.
`ahorros.devengo_interes_log` (único por Fecha+Cuenta) es la bitácora real
del devengo y también lo que hace que correr el batch dos veces el mismo
día sea seguro por diseño — la segunda corrida no encuentra cuentas
pendientes. Probado end-to-end: cuenta de $10,000 → $0.55 devengado
exacto (10000 × 0.02 / 365), balde `INT` y cuentas `2503`/`4101`
verificados en $0.55; segunda corrida el mismo día → 0 cuentas procesadas
(no duplica). Pantalla real: botón "Ejecutar devengo de interés" en
`Ahorros.tsx`.

**Cierre de período contable** (`Corela15.Application.Contabilidad.
ICierrePeriodoService` / `CierrePeriodoService`, `POST /api/contabilidad/
periodos/cerrar`, `GET /api/contabilidad/periodos`): `contabilidad.
periodo_contable` (Periodo, Cerrado, FechaCierre, CerradoPor) — mientras
no exista una fila para un período se asume abierto, un cierre explícito
lo bloquea. `ComprobanteContableService.RegistrarAsync` valida en **cada**
registro si el período (año-mes) de la fecha del comprobante está
cerrado, y si lo está rechaza con 422 (`PeriodoContableCerradoException`)
— esto protege automáticamente a **todo** caso de uso que genera
comprobantes (depósitos, préstamos, provisión, devengo, lo que sea que se
construya después), no solo el endpoint directo de comprobantes, porque
todos pasan por el mismo servicio. Doble cierre del mismo período se
rechaza (`PeriodoYaCerradoException`, 422). **No implementa cierre de
resultados** (utilidad del ejercicio → patrimonio, el paso típico de un
cierre anual real) — solo el bloqueo operativo del período, que es lo que
impide contabilizar retroactivo; el cierre de resultados queda fuera de
alcance, documentado a propósito. Probado end-to-end: cierre de un
período pasado (julio 2026, elegido a propósito para no bloquear el mes
en curso durante la prueba), comprobante con fecha dentro de ese período
rechazado (422), comprobante con fecha del mes abierto aceptado (201),
doble cierre del mismo período rechazado (422). Pantalla real: sección
"Cierre de período contable" en `Contabilidad.tsx` con selector de mes,
botón de cierre, y tabla de períodos con su estado.

Con esto, los 5 puntos identificados en la evaluación de seguridad/
regulatoria de esta sesión (autenticación, concurrencia, idempotencia,
tasas techo BCE, provisiones) más devengo y cierre de período están
completos y probados de punta a punta. Pendiente real, documentado en
cada sección: estructura de detalle de reportes regulatorios (solo índice
hoy). El cierre de resultados del ejercicio ✅ **hecho**, ver sección
"Cierre de resultados del ejercicio" más abajo.

## Tests automatizados

`backend/src/Corela15.Tests` (xUnit, agregado a `Corela15.sln`) — corre
contra el Postgres real de desarrollo (`CORELA15_CONNECTION`, mismo patrón
que el resto del proyecto: nunca mockear la base, ver la regla de oro del
proyecto en espíritu aunque acá aplica a la base propia, no a Softbank),
sin proveedor en memoria. Cada clase de test limpia sus propios datos en
`DisposeAsync` (`IAsyncLifetime`) — se verificó explícitamente con `psql`
que no queda ningún residuo tras correr la suite completa.

Cobertura: `ComprobanteContableServiceTests` (balance de líneas, mínimo de
2 líneas, cuenta inválida, actualización de `saldo_contable`),
`ProvisionCarteraServiceTests` (las 9 combinaciones día-de-mora→categoría→
porcentaje de la matriz A1-E, vía `[Theory]`/`[InlineData]`),
`TipoPrestamoAdminServiceTests` (tasa excede techo BCE, segmento inválido,
código duplicado), `AuthServiceTests` (login correcto con roles/menús,
contraseña incorrecta auditada, usuario inexistente — con `IConfiguration`
en memoria solo para el JWT, no para la base), `CuentaContableAdminServiceTests`
(alta/baja de cuenta contable, incluyendo el caso real de que `1101` Caja
General no se puede desactivar — con saldo real de actividad de sesión, el
test acepta `CuentaContableConSaldoException` o `CuentaContableEnUsoException`
según cuál de las dos validaciones dispare primero, ambas son 422 legítimos
para el mismo caso de negocio). `dotnet test` desde `backend/`: 25/25 en
verde.

## Coeficiente de liquidez

Esquema `riesgo`: `indicador_liquidez` (snapshot histórico: Fecha,
FondosDisponibles, DepositosCortoPlazo, Coeficiente, MinimoRegulatorio,
CumpleMinimo — una fila por corrida, nunca se sobreescribe) y
`parametro_liquidez` (fila única configurable, sembrada en 25% — **valor
de referencia, pendiente de verificación oficial contra la norma SEPS**;
la extracción de los PDFs oficiales de SEPS/BCE volvió a fallar —binario/
codificado, no texto legible— así que el porcentaje exacto vigente no se
pudo confirmar, se documenta así en vez de presentarlo como dato oficial).
Migración `Riesgo_IndicadorLiquidez`.

`Corela15.Application.Riesgo.IIndicadorLiquidezService` /
`IndicadorLiquidezService`: `POST /api/riesgo/liquidez/calcular` suma
`saldo_contable` del período vigente para cuentas de grupo CUC `11`
(Fondos disponibles) sobre grupo `21` (Depósitos a corto plazo) — el mismo
`saldo_contable` que ya alimenta cada asiento del sistema, no una tabla
paralela — y guarda el snapshot con `CumpleMinimo` calculado contra el
parámetro vigente. `GET /api/riesgo/liquidez` lista el histórico. `GET`/
`PUT /api/riesgo/liquidez/parametro` para consultar/actualizar el mínimo
regulatorio. Probado: con saldos en cero → coeficiente 0 (no divide por
cero, correcto: no cumple); con una cuenta de ahorro de prueba de $1,000
abierta → fondos disponibles $1,000 / depósitos corto plazo $1,000 =
coeficiente 1.0 (100%), verificado contra un mínimo temporal de 30% →
cumple. Todos los datos de prueba limpiados después (cuenta, comprobante,
saldo_contable, indicador_liquidez, parámetro repuesto a 0.25/seed).

Pantalla real: pestaña "Liquidez" nueva en `Riesgo.tsx` (antes solo tenía
registro de eventos de riesgo, ahora tiene navegación por pestañas —
`SeccionEventos`/`SeccionLiquidez`). Botón "Calcular ahora", tarjetas de
coeficiente actual (con badge de cumple/no cumple), fondos/depósitos en
dólares, y el mínimo regulatorio con **edición inline** (ícono de lápiz
abre un formulario pequeño en el mismo lugar, sin navegar a
Configuración) — por el requerimiento explícito de no forzar cambio de
pantalla para completar un proceso. Tabla histórica de corridas debajo.

No se implementó el aporte/prima de COSEDE (seguro de depósitos) como
cálculo separado — es un cargo regulatorio distinto al coeficiente de
liquidez operativo, con su propia fórmula sobre el total de depósitos
asegurados; queda fuera de esta ronda, a construir cuando haga falta el
caso de uso real de reporte a COSEDE.

## Motor de scoring de crédito

`credito.score_crediticio` (`Corela15.Domain.Credito.ScoreCrediticio`,
migración `Credito_ScoreCrediticio`): snapshot histórico por cliente
(nunca se sobreescribe, cada cálculo agrega una fila) con Puntaje (0-100),
Categoría (RiesgoBajo/RiesgoMedio/RiesgoAlto), los dos ratios financieros,
si tiene algún préstamo `Castigado` en su historial, cuántos préstamos
`Cancelado` tiene, y si es PEP. **Deliberadamente no es el motor histórico
completo de Softbank** (`CALIFICACIONCLIENTE_TRANSACCION`/`_DETALLE`,
6.3M/4M filas, fuera de alcance por complejidad — mismo criterio que ya
se aplicó en Nivel 4 con `lavadoactivos.calificacion_cliente`) — pero
tampoco un número inventado: cada componente del puntaje sale de una
columna real ya existente en el dominio (`Persona.Ingresos/Egresos/
Activos/Pasivos`, `Prestamo.Estado` vía `PrestamoCliente`,
`PersonaNatural.EsPep`), nunca de una tabla nueva fabricada para la
ocasión.

Fórmula (`Corela15.Application.Colocacion.IScoreCrediticioService` /
`ScoreCrediticioService`, base 50 puntos): ratio ingreso neto
`(Ingresos-Egresos)/Ingresos` suma hasta +30 (≥50%) o resta -20 (negativo);
ratio de endeudamiento `Pasivos/Activos` suma +10 (<30%) o resta -15
(≥60%); préstamo castigado en el historial resta -40; cada préstamo
cancelado con éxito suma +10 (tope +20). Resultado acotado a [0,100].
Categoría: ≥70 RiesgoBajo, ≥40 RiesgoMedio, <40 RiesgoAlto. Cuando
`Persona` no tiene Ingresos/Activos capturados (caso real de los socios de
prueba sembrados), esos componentes simplemente no aplican — no es un
error, el score queda en la base + ajustes por historial de préstamos.
Probado end-to-end con datos reales: socio sin datos financieros →
puntaje 50/RiesgoMedio; mismo socio con Ingresos=$1000/Egresos=$400
(ratio 60% → +30) y Activos=$5000/Pasivos=$1000 (ratio 20% → +10) →
puntaje 90/RiesgoBajo, confirmando que la fórmula responde correctamente a
datos reales y no es un valor fijo. Datos de prueba limpiados después
(score y campos financieros de la persona de prueba revertidos a null).

`POST /api/creditos/clientes/{idCliente}/score` calcula y guarda un nuevo
snapshot; `GET /api/creditos/clientes/{idCliente}/score/historial` lista
los anteriores. **Integrado inline en el flujo de solicitud de crédito**
(`Creditos.tsx`, componente `ScoreCrediticioPanel` dentro de
`SolicitarForm`): en cuanto el asesor selecciona el socio en el
desplegable, el score se calcula automáticamente si no existe uno
reciente (sin que el asesor tenga que hacer nada ni cambiar de pantalla),
mostrando puntaje/categoría con badge de color, alertas visuales si el
socio es PEP o tiene un préstamo castigado, y los dos ratios — toda la
información para decidir la solicitud aparece en el mismo formulario,
antes de completar el envío. Botón "Recalcular" disponible por si los
datos de la persona cambiaron. El "PLA" del ítem original (antilavado de
activos) se cubre solo parcialmente acá vía el flag `EsPep` expuesto en el
score — el motor transaccional de PLA real
(`lavadoactivos.calificacion_cliente`, perfil ya modelado en Nivel 4) no
se construyó en esta ronda, sigue pendiente.

## Reportes contables (balance de comprobación)

Último ítem pendiente de la ronda de evaluación de seguridad/regulatoria.
**Decisión consciente de alcance**: no se implementó la estructura de
detalle de ningún reporte regulatorio codificado (B13, D01, BCE01/02,
etc. — el índice de `reportecontrol.reporte_regulatorio` de Nivel 8) por
la misma razón ya documentada ahí: verificar la estructura de tablas de
Softbank no sustituye verificar la norma oficial SEPS/BCE, y esa
verificación volvió a fallar (PDFs oficiales devuelven binario/codificado,
no texto legible, mismo problema que con la matriz de provisión y el
mínimo de liquidez). Inventar la estructura de un reporte regulatorio
real sin esa verificación sería peor que no construirlo. En su lugar se
construyó el **Balance de Comprobación** — un reporte contable estándar
universal (no un formulario SEPS/BCE codificado), 100% derivable de datos
que el sistema ya calcula, sin fabricar estructura nueva.

`GET /api/contabilidad/reportes/balance-comprobacion?periodo=YYYY-MM-DD`
(`Corela15.Api.Controllers.ReportesController`, patrón de lectura directa
sobre `Corela15DbContext` sin pasar por Application, igual que
`CuentasContablesController` — no hay lógica de negocio que validar, solo
agregación): para cada cuenta de detalle (`es_mayor=true`), calcula saldo
inicial como la suma de `SaldoFinal` de todos los períodos anteriores al
solicitado (`contabilidad.saldo_contable` ya guarda un `SaldoFinal`
correctamente firmado según la naturaleza de la cuenta desde
`ComprobanteContableService`, así que sumarlos a través de períodos da un
acumulado válido sin tabla nueva), más los débitos/créditos/saldo final
del período consultado. Omite cuentas sin ningún movimiento (inicial o
del período) para no listar las ~30 cuentas del plan que nunca se han
usado. Expone `cuadrado` (total débitos == total créditos del período) —
la misma verificación de partida doble que ya hace
`ComprobanteContableService` por comprobante, acá agregada por período
completo, así que si algún día hay una inconsistencia real sería visible
acá primero. `GET /api/contabilidad/reportes/periodos` lista los períodos
con algún movimiento real, para poblar accesos rápidos en el selector.

Probado end-to-end: cuenta de ahorro de prueba con depósito inicial de
$1,500 → balance de agosto 2026 mostró `1101` Caja General (débito
$1,500, saldo final $1,500) y `2101` Depósitos de ahorro a la vista
(crédito $1,500, saldo final $1,500), `cuadrado: true`, totales
$1,500/$1,500 exactos. Datos de prueba limpiados después (cuenta,
comprobante, saldo_contable). Pantalla real: sección "Balance de
comprobación" en `Contabilidad.tsx`, debajo del plan de cuentas — selector
de período (`<input type="month">`) más atajos de un clic a los períodos
con movimientos reales, badge Cuadrado/Descuadrado, tabla con fila de
totales.

## Auto-débito de cuota por SPI

Este es el incidente que originó todo el proyecto (ver
[01-contexto-origen.md](01-contexto-origen.md)): el proceso real de
Softbank descontaba la cuota de un préstamo por SPI/Banco Central sin
cruzar correctamente tres configuraciones independientes, e ignoraba en
la práctica `PRESTAMO.DEBITOSPI` en algunos casos. El aprendizaje ya
estaba documentado desde Nivel 2/3 en los comentarios de
`CuentaItemSaldo.AcreditaPrestamo`, `TipoCuenta.PermiteDebitoPrestamo` y
`Prestamo.DebitoSpi`: las tres deben validarse como una sola fuente de
verdad, nunca como flags independientes que un proceso puede ignorar.

`colocacion.auto_debito_spi_log` (`Corela15.Domain.Colocacion.
AutoDebitoSpiLog`, migración `Colocacion_AutoDebitoSpiLog`): un registro
por préstamo por día, **se debite o se omita**, siempre con el motivo
explícito — la corrección directa del incidente real, donde no había
forma de saber por qué un préstamo sí o no fue debitado sin cruzar el
historial temporal nativo de SQL Server a mano. El índice único (Fecha,
IdPrestamo) hace que correr el batch dos veces el mismo día sea seguro
por diseño, mismo patrón que `DevengoInteresLog`.

**`IAutoDebitoSpiService`/`AutoDebitoSpiService`** (`Corela15.Infrastructure.
Services`): `PATCH /api/creditos/prestamos/{id}/debito-spi` activa/
desactiva `Prestamo.DebitoSpi` (antes quedaba fijo en `false` desde el
desembolso, sin forma de encenderlo — gap real encontrado al construir
este caso de uso). `POST /api/creditos/auto-debito-spi/ejecutar` corre el
batch: para cada préstamo vigente con `DebitoSpi=true` no procesado hoy,
busca la próxima cuota pendiente y valida en orden:
1. `TipoCuenta.PermiteDebitoPrestamo` en alguna cuenta activa del socio.
2. `CuentaItemSaldo.AcreditaPrestamo=true` en el balde Disponible de esa
   cuenta (tercera configuración — no tenía pantalla para activarla; se
   agregó `PATCH /api/ahorros/cuentas/{id}/acredita-prestamo`, directo
   contra el DbContext por ser un flag sin invariante de negocio, mismo
   patrón que los catálogos simples de Configuración).
3. Saldo disponible suficiente para cubrir la cuota sin bajar del mínimo
   del producto (`TipoCuenta.SaldoMinimoConPrestamo ?? SaldoMinimo`).

Si las tres se cumplen, debita la cuenta del socio, marca la cuota
pagada, reduce `Prestamo.Saldo` (cancela el préstamo si era la última
cuota) — mismo tratamiento de `PagarCuotaAsync`, pero financiado desde el
pasivo del socio (`2101` Depósitos de ahorro a la vista) en vez de
efectivo (`1101` Caja). Un solo comprobante consolidado por corrida
(débito `2101` / crédito `1401` Cartera por el total de capital / crédito
`5101` Intereses ganados por el total de interés), mismo patrón que
`DevengoInteresService`. `GET /api/creditos/auto-debito-spi/historial`
lista la bitácora completa.

Probado end-to-end contra Postgres real, cubriendo las tres causas de
omisión y el caso exitoso: préstamo Consumo de $300 a 3 cuotas con
`DebitoSpi=true` sobre una cuenta AHV con `AcreditaPrestamo=false` →
omitido con motivo explícito; activado `AcreditaPrestamo` y reejecutado
→ debitado $102.88 exacto (cuota 1), saldo de la cuenta $500→$397.12,
saldo del préstamo $300→$201.42, comprobante verificado cuadrado en el
balance de comprobación ($902.88 = $902.88 incluyendo el resto de la
prueba). Reejecución el mismo día → 0 procesados (idempotente). Datos de
prueba limpiados después (préstamo, solicitud, cuenta, comprobantes,
saldo_contable, log).

Pantalla real: en `Creditos.tsx`, columna "Débito SPI" con badge
interactivo por préstamo vigente (toggle directo, sin navegar a otra
pantalla) y sección "Auto-débito de cuota por SPI" con botón "Ejecutar
auto-débito SPI" + tabla de bitácora (préstamo, cuenta, cuota, resultado,
motivo, monto). En `Ahorros.tsx`, columna "Débito de préstamo (SPI)" con
el mismo patrón de badge interactivo, deshabilitada visualmente cuando el
producto no admite débito de préstamo.

Pendiente, no bloqueante: batch diseñado para correr una vez al día
(no hay todavía un job programado — se ejecuta manualmente desde la
pantalla, igual que devengo de interés y provisión de cartera); si dos
socios comparten cuenta y ambos tienen préstamos con `DebitoSpi=true`,
la elegibilidad de cuenta usa la primera cuenta activa con
`PermiteDebitoPrestamo=true` encontrada, sin priorización explícita entre
varias cuentas elegibles del mismo socio (caso poco común, no probado).

## Renovación de DPF con tasa vigente

Segundo de los tres incidentes reales que originaron el proyecto (ver
[01-contexto-origen.md](01-contexto-origen.md), hallazgo #2): "la tasa
aplicada al renovar un certificado a menudo no coincidía con el tablero
de tasas vigente". El aprendizaje ya estaba documentado desde Nivel 3 en
`Deposito.cs`: la renovación debe leer la tasa vigente de
`ItemPlazoTasa` **en el momento de renovar**, nunca copiar la tasa del
depósito origen.

**`IDepositoService.RenovarAsync`** (`POST /api/plazofijo/depositos/{id}/
renovar`): cierra el depósito origen con `Estado=Renovado` (nunca
`Cancelado` — sigue siendo capital del socio, no una redención), y abre
un depósito nuevo (mismo código consecutivo `DPFxxxxxxxxx` que
`AbrirAsync`) leyendo la tasa vigente del tablero con el mismo criterio
de `AbrirAsync` (plazo/monto/tipo de persona), nunca la tasa que tenía el
origen. Copia los titulares (`DepositoCliente`) del origen al destino.
Admite **incremento de capital** opcional — si el socio aporta más dinero
al renovar, se registra el asiento de la diferencia (débito Caja /
crédito `2103` Depósitos a plazo fijo); si no hay incremento, no genera
comprobante porque el saldo de la subcuenta contable no cambia, solo se
re-papela bajo un código de DPF nuevo. `inversion.deposito_renovacion`
(ya modelada desde Nivel 3, sin caso de uso hasta ahora) registra la
cadena origen→destino con el valor total y el incremento, para
trazabilidad completa de cuántas veces se renovó un certificado y a qué
tasa cada vez — la auditoría que el incidente original no tenía sin
cruzar el historial temporal de SQL Server a mano.

Probado end-to-end contra Postgres real, reproduciendo el incidente
original a propósito: se abrió un DPF de $1,000 a 180 días con la tasa
vigente (8.50%), luego se insertó una fila nueva en el tablero
(`item_plazo_tasa`) con una tasa distinta (9.20%) para el mismo rango de
plazo — simulando que la Junta/el consejo cambió el tablero después de
la apertura original, el escenario real del incidente. Un segundo DPF
abierto tras el cambio confirmó que `AbrirAsync` ya toma la tasa nueva
correctamente (9.20%); renovado ese segundo DPF con un incremento de
capital de $200 → nuevo DPF de $1,200 a la tasa vigente (9.20%, NO la
tasa original), origen marcado `Renovado`, asiento del incremento
verificado ($200 exactos), y el balance de comprobación del período
cuadrado ($2,200 = $2,200 incluyendo el resto de la prueba). Datos de
prueba limpiados después (depósitos, renovaciones, fila de tasa de
prueba, comprobantes, saldo_contable).

Pantalla real: botón "Renovar" por DPF vigente en `Creditos.tsx` (junto
a "Cancelar", inline en la misma fila — sin cambiar de pantalla), abre un
modal (`RenovarDpfModal`) con plazo nuevo, incremento de capital opcional
y tipo de persona, muestra el resultado (código nuevo, monto, tasa
aplicada, vencimiento) en el mismo modal sin recargar. Sección
"Renovaciones de DPF" debajo de la cartera de plazo fijo con el
historial completo origen→destino.

Cálculo de interés devengado proporcional en cancelación ✅ **hecho** (ver
sección siguiente). Pendiente real que sigue: al renovar un DPF, el
depósito origen no paga el interés devengado hasta la fecha de
renovación — el capital simplemente se re-papela bajo el destino nuevo
sin liquidar el interés del período que sí corrió sobre el origen.

## Interés devengado proporcional en cancelación anticipada de DPF

Último punto pendiente documentado desde Nivel 3: `CancelarAsync`
devolvía solo el capital nominal del depósito, sin importar cuántos días
habían transcurrido desde la apertura — cancelar al día 1 o al día 89 de
un DPF a 90 días pagaba exactamente lo mismo (nada de interés), lo cual
no es correcto financieramente.

Fórmula (`DepositoService.CancelarAsync`): `interés = Monto × Tasa ×
díasTranscurridos / 365`, con `díasTranscurridos` acotado entre 0 y
`PlazoDias` del depósito (`Math.Clamp`) — protección explícita para que
una cancelación después del vencimiento nominal (depósito vigente que
nunca se renovó ni se canceló a tiempo) no pague más que el interés
completo del plazo contratado. Mismo convención `/365` que
`DevengoInteresService` (ver esa sección para la nota de que es una
convención estándar, no una tasa techo regulatoria que exija
verificación exacta contra circular oficial). Si el interés calculado es
mayor a cero, el comprobante de cancelación pasa de 2 a 4 líneas: además
de la reversión de capital (débito `2103` / crédito `1101`, sin cambios),
se agrega débito `4101` Intereses causados en depósitos / crédito `1101`
Caja por el interés — pagado en efectivo de inmediato junto con el
capital, no diferido. Si el interés calculado es exactamente cero (ej.
depósito cancelado el mismo día que se abrió), el comprobante se queda en
2 líneas — no se agrega una línea con monto cero.

`DepositoCanceladoResult` ahora expone `InteresPagado` además de
`ValorDevuelto`. Probado end-to-end contra Postgres real, reproduciendo
un escenario de cancelación anticipada real: DPF de $2,000 a 90 días
(tasa 7%), fecha de apertura retrocedida 45 días por SQL para simular el
paso del tiempo → cancelación devolvió `interesPagado: 17.26`, cálculo
exacto (`2000 × 0.07 × 45 / 365 = 17.26`), balance de comprobación
verificado cuadrado tras la prueba. **Cuidado real durante la prueba**:
el ambiente de desarrollo compartido ya tenía actividad real del usuario
(una cuenta y un préstamo abiertos manualmente desde el frontend, no
datos de prueba) — la limpieza posterior fue quirúrgica (solo los
comprobantes y saldo_contable generados por el DPF de prueba, verificado
por `creado_en`/`creado_por` antes de borrar nada), en vez del `DELETE
FROM saldo_contable` sin filtrar que se usó en rondas anteriores cuando
la base estaba vacía — nunca asumir que la base de desarrollo está vacía
solo porque lo estaba la última vez.

Pantalla real: el mensaje de éxito al cancelar un DPF en `Creditos.tsx`
(sección Plazo fijo) ahora muestra el capital devuelto y, si aplica, el
interés devengado proporcional pagado, en el mismo lugar sin navegar.

## Administración de permisos por rol

Tercer hallazgo real que originó el proyecto (ver
[01-contexto-origen.md](01-contexto-origen.md)): en SIGA se construyó
`/admin/permisos` para ver, filtrable, qué rol tiene qué acceso —
reemplazando la revisión manual uno por uno en Softbank. Acá el
equivalente escribe, no solo lee: hasta ahora `seguridad.rol_menu` (qué
módulos ve cada rol) solo se podía tocar por migración
(`Nivel0_MenuRolMenu`) — cualquier ajuste real requería una migración
nueva y un despliegue, no una pantalla.

`GET /api/configuracion/roles/{id}/menus` / `PUT /api/configuracion/
roles/{id}/menus` (`ConfiguracionController`, mismo patrón directo contra
el DbContext que el resto de catálogos simples — la asignación N:M no
tiene invariante de negocio más allá de no duplicar la fila): el `PUT`
recibe la lista completa de `idsMenu` que debe tener el rol y reconcilia
`rol_menu` completo (activa las filas presentes, desactiva las que ya no
están, crea las que faltan) en una sola llamada — el frontend no necesita
mandar diffs.

**Limitación real, documentada explícitamente en el código y en la UI, no
oculta**: los permisos de una sesión ya iniciada se calculan una sola vez
al login (claims del JWT, ver "Autenticación real" más arriba) — cambiar
`rol_menu` acá no actualiza automáticamente una sesión activa, sus claims
siguen siendo los de cuando inició sesión. Ya no es un callejón sin
salida: con la revocación de tokens (ver esa sección más abajo), un
administrador puede forzar el refresco cerrando las sesiones activas del
usuario afectado desde `Usuarios y roles` — el siguiente login trae los
permisos nuevos. Probado end-to-end: permisos del rol CAJERO editados (quitar
`cajas`, agregar `riesgo`), verificado por `GET`; un nuevo login del
usuario `mguaman` (que tiene CAJERO + OFICIAL DE CAPTACIONES) confirmó
`menus: ["ahorros","creditos","riesgo","socios"]` en `/api/auth/me` — sin
`cajas`, con `riesgo` — probando que el cambio se propaga correctamente
al siguiente login. Permisos de CAJERO restaurados al estado original
sembrado después de la prueba.

Pantalla real: dentro de la pestaña "Roles" en `Configuracion.tsx`, botón
"Permisos" por rol que expande una fila inline (`PermisosDelRol`) con los
11 módulos como chips seleccionables (clic para activar/desactivar,
mismo patrón visual que los tipos de transacción en `Ahorros.tsx`) y un
botón "Guardar permisos" — todo en el mismo lugar, sin navegar a otra
pantalla ni abrir un modal separado.

## Revocación de tokens

Último gap crítico documentado desde la implementación original de
autenticación: un JWT es válido por firma y fecha de expiración
únicamente — sin nada más, un token seguía siendo aceptado durante las 8h
de `Jwt__ExpiryMinutes` sin importar qué pasara después con la cuenta
(usuario deshabilitado, contraseña cambiada por sospecha de robo, alguien
que deja la cooperativa). No había forma real de "desconectar" a alguien.

`seguridad.sesion_usuario` (`Corela15.Domain.Seguridad.SesionUsuario`,
migración `Seguridad_SesionUsuario`): una fila por token emitido, con
`Id` = el claim `jti` del JWT (no un id nuevo — el jti ya viaja en el
token, reusarlo como PK evita una tabla de mapeo aparte). Cada login
(`AuthService.LoginAsync`) inserta la fila junto con el token. En
`Program.cs`, `AddJwtBearer().Events.OnTokenValidated` agrega un chequeo
extra después de que .NET ya validó firma/emisor/expiración: busca el
`jti` en `sesion_usuario` y si no existe o `Revocada=true`, falla la
autenticación (401) — sin este chequeo, revocar no tendría ningún efecto
real hasta que el token expirara solo.

Tres operaciones nuevas en `IAuthService`:
- **`LogoutAsync`** (`POST /api/auth/logout`) — revoca la sesión actual
  (su propio `jti`, leído del token). Logout real, no solo
  `localStorage.removeItem` del lado del cliente.
- **`ListarSesionesAsync`** (`GET /api/usuarios/{id}/sesiones`) —
  historial completo de tokens emitidos a un usuario (emitida, expira,
  IP, vigente/revocada/expirada), más reciente primero.
- **`RevocarTodasLasSesionesAsync`** (`POST /api/usuarios/{id}/sesiones/
  revocar-todas`) — cierra TODAS las sesiones activas de un usuario de
  una sola vez, el caso real de "cortar todos los accesos ya" cuando
  alguien deja la cooperativa o se sospecha de una cuenta comprometida.
  **Efecto colateral esperado, no un bug**: si el administrador ejecuta
  esto sobre su propio usuario, también cierra su propia sesión actual —
  probado a propósito, es el comportamiento correcto (revocar todas es
  todas, sin excepción tácita para quien lo ejecuta).

Probado end-to-end contra Postgres real: login → token nuevo funciona en
`/api/auth/me`; logout → el mismo token, reutilizado inmediatamente
después, devuelve 401; login de una segunda sesión → activa; `GET .../
sesiones` lista ambas (una vigente, la ya cerrada marcada revocada con
`revocadaPor`); `POST .../revocar-todas` → la sesión que seguía activa
también queda revocada, verificado con un request inmediato posterior
(401). Las filas de `sesion_usuario` generadas durante la prueba se
dejaron como quedan — es una bitácora de auditoría real (mismo criterio
que `accion_ingreso_usuario`: nunca se borra, ni siquiera la de pruebas
con credenciales reales).

**Advertencia real para cualquier sesión de desarrollo abierta ANTES de
este cambio**: un token emitido antes de que existiera `sesion_usuario`
no tiene fila en la tabla → el chequeo `OnTokenValidated` lo trata igual
que revocado (`revocada != false` cuando no hay fila es `null`, y
`null != false` es verdadero) y lo rechaza con 401. Es el comportamiento
correcto (más estricto: preferible perder sesiones válidas viejas a que
la revocación tenga huecos), pero significa que **cualquier usuario con
sesión activa en el navegador al momento de desplegar esto necesita
volver a iniciar sesión** — no es un bug, es la transición esperada de
"sin tracking de sesiones" a "con tracking de sesiones".

Pantalla real: `TopBar` ya no solo limpia `localStorage` al cerrar
sesión — `AuthContext.logout` ahora llama a `POST /api/auth/logout`
antes de limpiar el token local (best-effort: si falla por red, igual
limpia localmente). En `UsuariosRoles.tsx`, botón "Sesiones" por usuario
que expande una fila inline con la tabla de historial y el botón "Cerrar
todas las sesiones" — mismo patrón de expansión inline que "Permisos" en
`Configuracion.tsx`, sin navegar a otra pantalla.

Pendiente, no bloqueante: no hay un job que purgue filas viejas de
`sesion_usuario` (crece sin límite, mismo criterio de "no borrar
auditoría" que `accion_ingreso_usuario` — revisar si hace falta un
archivado en un ambiente real con volumen alto de logins).

## Cierre de resultados del ejercicio

Último pendiente documentado desde la implementación de cierre de
período (Nivel 1): ese cierre solo bloquea contabilizar retroactivo mes
a mes — nunca liquidó la utilidad o pérdida del año contra patrimonio,
el paso típico de un cierre anual real. Sin esto, las cuentas de
Ingresos (grupo CUC 5) y Gastos (grupo CUC 4) seguían acumulando sin
límite de un año a otro dentro del mismo diseño de `saldo_contable`.

Subcuentas reales agregadas bajo `36` Resultados (ya sembrado desde
Nivel 1 pero sin hijos): `3603` Utilidad del ejercicio y `3604`
`(Pérdida del ejercicio)` — códigos reales del CUC, mismo criterio que
`1499`/`4402` en el motor de provisiones (naturaleza Acreedora, la que
trae el grupo Patrimonio, no una naturaleza "intuitiva" según si es
utilidad o pérdida).

**`ICierreEjercicioService.CerrarAsync`** (`POST /api/contabilidad/
cierre-ejercicio/cerrar`): agrupa `saldo_contable` por cuenta, sumando
`SaldoFinal` de todos los períodos (meses) del año a cerrar, solo para
cuentas de grupo Ingresos o Gastos. Para cada cuenta con saldo acumulado
distinto de cero genera una línea que la deja en cero (débito si es
Ingresos — su saldo positivo es un crédito acumulado; crédito si es
Gastos — su saldo positivo es un débito acumulado), y agrega una última
línea a `3603` (crédito, si `Ingresos > Gastos`) o `3604` (débito, si
`Gastos > Ingresos`) por la diferencia — un solo comprobante, partida
doble real, mismo motor `ComprobanteContableService` de siempre, fechado
31 de diciembre del año que se cierra. Único por año
(`contabilidad.cierre_ejercicio`, índice único en `Anio`) — un segundo
intento de cerrar el mismo año se rechaza (`EjercicioYaCerradoException`,
422); un año sin ningún movimiento de ingresos/gastos también se rechaza
(`EjercicioSinMovimientosException`, 422) en vez de generar un
comprobante vacío o un cierre en cero sin sentido.

**Interacción real con el cierre de período, documentada a propósito**:
si diciembre del año a cerrar ya está bloqueado vía "Cierre de período"
(Nivel 1), `ComprobanteContableService` rechaza el comprobante de cierre
de resultados con `PeriodoContableCerradoException` — el orden operativo
correcto es cerrar resultados primero, período de diciembre después,
documentado en la propia pantalla. **No implementa cierre de resultados
acumulados de años anteriores** (`3601`/`3602`, típico de un segundo
paso donde la utilidad del año recién cerrado se traslada a "utilidades
acumuladas") — cada corrida liquida solo el ejercicio que se cierra,
fuera de alcance a propósito.

Probado end-to-end contra Postgres real con datos sintéticos aislados en
un año lejano sin ningún riesgo de tocar actividad real (2099, nunca
puede chocar con un año calendario legítimo): ingresos $500 en `5101`,
gastos $200 en `4101` insertados directo en `saldo_contable` para
simular un año con actividad real acumulada → cierre calculó utilidad
exacta de $300, `5101`/`4101` verificados en cero tras sumar sus dos
períodos (el de la actividad original + el de la reversión del cierre),
`3603` acreditada en $300, balance de comprobación de diciembre 2099
cuadrado ($500 = $500). Reintentar el mismo año → 422. Año sin
movimientos (2025) → 422. Datos sintéticos limpiados por completo
después (cierre_ejercicio, comprobante, saldo_contable de 2099) sin
tocar ningún dato real del ambiente de desarrollo compartido.

Pantalla real: pestaña "Cierre de resultados" nueva en `Contabilidad.tsx`
— selector de año, botón "Cerrar ejercicio", mensaje de resultado
inmediato (ingresos/gastos/utilidad o pérdida), y tabla histórica de
ejercicios cerrados.

## Estados financieros B11/B13 (SEPS) — primer reporte con estructura oficial real

Primer reporte regulatorio construido a partir de la fuente real (no
inferido de Softbank ni inventado): el usuario descargó directamente los
manuales técnicos oficiales desde la página de SEPS
(`https://www.seps.gob.ec/manuales-para-la-gestion-de-envio-de-informacion-esfps/`)
a `C:\Users\ksantana\Documents\manuales seps\` — a diferencia de los PDF
de la web pública (binarios/escaneados, `pdftotext`/WebFetch fallaban
siempre), estos son PDF con texto real extraíble. Fuente exacta usada acá:
**"Manual Técnico de Estructuras de Datos - Sistema de Acopio de
Información 'Estados Financieros'", versión 10.0, actualizado al
15/07/2025** (sección 3: Definición de Estructuras, sección 4.1:
Controles de Validación).

**Hallazgo operativo real, no de código**: los nombres de archivo
originales (con tildes/eñes) no se podían leer ni con el `Read` tool ni
con PowerShell (`Copy-Item`/`Get-ChildItem` con el nombre tipeado
fallaban con "no se encuentra la ruta") — normalización Unicode NFD vs
NFC en el sistema de archivos. Se resolvió renombrando todos los
PDF/XLSX de esa carpeta a nombres sin tildes vía `Get-ChildItem` +
`Rename-Item` (que sí puede iterar el objeto `FileInfo` sin re-tipear el
nombre acentuado). El contenido de los archivos no cambió, solo el
nombre. `pdftotext` (de `mingw64/bin`, ya presente en el entorno) con
`-enc UTF-8` extrae el texto correctamente.

Estructura real B11 (mensual) / B13 (diario) — idéntica entre ambas,
solo cambia la periodicidad de la fecha de corte:

- **Cabecera**: Código de estructura (Carácter 3, "B11"/"B13"), Número de
  RUC (Numérico 13), Fecha de corte (dd/mm/aaaa), Número total de
  registros (Numérico 6), Valor de cuadre (Numérico 15.2 — suma
  algebraica de todos los saldos reportados).
- **Detalle** (una fila por cuenta): Código de cuenta contable (Carácter
  6), Nombre de la cuenta contable (Carácter 200), Saldo de la cuenta
  contable (Numérico 15.2).

Controles de validación reales implementados en
`ReportesController.GenerarEstadoFinancieroAsync` (mismo patrón directo
contra `Corela15DbContext` que el resto de `ReportesController`, sin
Application — es agregación de lectura, no un caso de uso con
invariante): excluye los grupos CUC `62`/`63`/`72`/`73` (contrapartidas
de cuentas de orden/contingentes, el manual las excluye explícitamente);
valida que el saldo de cada cuenta sea positivo salvo la lista real de
excepciones del manual (elemento 3 Patrimonio, grupos `35`/`36`, cuentas
`3502`/`3504`, y la lista textual completa de subcuentas de provisión/
depreciación que sí pueden ser negativas — `1399`, `1499` y sus
subcuentas, `1699`, `1899`, `1999`, `3602`, `3604`, etc., transcrita tal
cual del manual) — si una cuenta viola esto, se reporta como advertencia
explícita, no se oculta ni se bloquea el reporte.

**Limitación real, documentada en el código y en la UI, no oculta**: el
catálogo completo ya está sembrado (ver sección "Catálogo de cuentas
oficial completo (CUC)" más abajo — 983 cuentas de detalle), pero el
número de registros todavía no coincide exacto con el oficial (1.192)
porque ese catálogo no está filtrado por aplicabilidad de segmento
(Segmento 2, el real de esta cooperativa) — sigue incluyendo cuentas de
los 5 segmentos + Caja Central + CONAFIPS. B13 (diario) solo puede
generarse **a la fecha de hoy**: el modelo de saldos de este core es
mensual (`saldo_contable` por período-mes), no diario, así que no existe
una foto exacta reconstruible de un día pasado arbitrario — limitación
real del diseño de datos, no del reporte.

**No se generó el archivo de envío real** (XML + TXT-hash de seguridad,
comprimidos en `.zip`, con el nombre obligatorio `B11_RUC_dd-mm-aaaa.zip`
según el manual) — el manual describe el formato de contenido pero no
incluye el XSD con los nombres de tag XML exactos entre los archivos que
se descargaron (si aparece en otro manual/paquete, se puede agregar
después); construir un XML con nombres de tag inventados sería el mismo
error que se viene evitando con reportes regulatorios desde el principio
del proyecto. Lo que sí se construyó es correcto y verificable: los
datos, la estructura de campos, y los controles de validación reales.

Probado end-to-end contra Postgres real: `GET /api/contabilidad/
reportes/b11?periodo=2026-08-01` y `GET /api/contabilidad/reportes/b13`
sobre la actividad real del usuario en el ambiente de desarrollo (no
datos sintéticos) — devolvió cabecera correcta (RUC de la empresa
sembrada, fecha de corte = último día de agosto para B11 / hoy para
B13), 983 registros de detalle (todas las cuentas de detalle del
catálogo completo, con saldo 0 donde no hay actividad — solo 3 con saldo
real: `1101`, `1401`, `2101`), y **encontró una advertencia real, no
fabricada para la prueba**: `1101` Caja General tenía saldo `-900.00`
(más desembolsos de préstamo que depósitos reales en el ambiente de
prueba), y el manual no autoriza esa cuenta a reportarse en negativo —
exactamente el tipo de inconsistencia que este control está diseñado
para atrapar antes de un envío real a SEPS. Verificado también que el
Balance de Comprobación (que sí sigue omitiendo cuentas sin movimiento,
a propósito, es un reporte distinto) no se vio afectado por la siembra
del catálogo completo — mismas 3 líneas, mismo cuadre exacto.

Pantalla real: pestaña "B11 / B13 SEPS" en `Contabilidad.tsx` — selector
de estructura (B11 mensual / B13 diario), selector de mes para B11,
tarjeta de cabecera, panel de advertencias de validación (destacado,
nunca oculto), y tabla de detalle con los tres campos exactos del
manual.

**Pendiente real para retomar esta línea de trabajo** (documentado a
propósito, no una lista aspiracional): sembrar el CUC completo ✅ **hecho**
(ver sección siguiente); procesar el resto de manuales ya descargados en
`manuales seps/` (Depósitos, Socios, Cartera de Créditos y Contingentes,
Servicios Financieros, Riesgo de Liquidez L02, Indicadores de Género
IG01, Cobros Indebidos CI01, Obligaciones Financieras, Tablas de
Información) con el mismo método (`pdftotext -enc UTF-8`, evitando el
problema de nombres de archivo con tildes); conseguir o construir el XSD
real para el empaquetado XML+hash+zip final si se necesita enviar de
verdad a SEPS.

## Catálogo de cuentas oficial completo (CUC)

Sembrado desde `Catálogo-B11-y-B13.xlsx` (provisto por el usuario,
descargado directo de SEPS) — 1.130 cuentas nuevas agregadas a las ~64
que ya existían desde Nivel 1 (que cubrían solo elemento + grupo +
un puñado de cuentas de detalle realmente usadas por los casos de uso
construidos). Total actual: **1.194 cuentas**, de las cuales **983** son
de detalle (`es_mayor=true`).

**Metodología real** (no una simple carga de Excel): el archivo `.xlsx`
no se pudo leer con herramientas estándar de Node/Python (no hay Python
en el entorno) — se parseó directo el XML interno (`xl/sharedStrings.xml`
+ `xl/worksheets/sheet1.xml`) vía PowerShell, forzando lectura UTF-8
explícita (`[System.IO.File]::ReadAllText(..., [System.Text.Encoding]::UTF8)`
— sin esto, los caracteres acentuados salían corruptos). Con 1.191 filas
(código + nombre) extraídas, se calculó en PowerShell:
- **Jerarquía**: nivel por longitud de código (1=elemento, 2=grupo,
  4=cuenta, 6=subcuenta — confirmado contra la sección 4 del Manual
  Técnico), padre = código truncado al nivel anterior.
- **Naturaleza**: heredada del padre (no del elemento) — necesario porque
  los grupos de contrapartida (`62`/`63` bajo Cuentas Contingentes, ya
  sembrados desde Nivel 1) invierten la naturaleza real respecto a su
  elemento, y la herencia por padre lo respeta automáticamente.
- **`EsMayor` (hoja)**: `true` si ningún otro código del catálogo
  combinado (existente + nuevo) lo tiene como padre inmediato.

**Cuentas ya operativas (1101, 1401, 1425, 1449, 1499, 1601, 2101, 2103,
2503, 3603, 3604, 4101, 4402, 5101, 5601) nunca se tocaron** — la
migración usa `ON CONFLICT (codigo) DO NOTHING`, así que si el catálogo
oficial dice que una de ellas ahora tiene subcuentas hijas (ej. `1101`
Caja tiene `110105` Efectivo, `110110` Caja chica en el catálogo real),
esa cuenta operativa se queda exactamente como está (`es_mayor=true`,
usada activamente por todo el motor contable) y sus "hijas oficiales" se
siembran igual como hojas nuevas en `$0`, nunca reciben posteos reales
todavía. **Inconsistencia estructural aceptada a propósito**: es la
opción más segura — la alternativa (migrar todo el motor contable a
postear a nivel de subcuenta de 6 dígitos) es un cambio de diseño
mucho más grande, fuera de alcance de esta sesión.

**Anomalía real encontrada y excluida**: el código `671` ("Contingentes")
aparece en el Excel con 3 dígitos — no encaja en ningún nivel válido
(1/2/4/6) de la jerarquía documentada. Se excluyó de la siembra en vez de
adivinar su jerarquía real (¿es un típo de `67`? ¿un código legítimo de
3 dígitos que el manual no documenta?) — un solo código, impacto
mínimo, pendiente de aclarar contra la fuente si se necesita.

`B11`/`B13` ya no filtran cuentas con saldo cero (a diferencia del
Balance de Comprobación) — el manual exige el conteo fijo de registros,
no solo las cuentas con actividad. **La brecha contra el "1.192" oficial
sigue sin cerrar del todo** (983 vs. 1.192, ~209 de diferencia): la causa
más probable es que el catálogo sembrado incluye cuentas de los 5
segmentos + Caja Central + CONAFIPS sin filtrar por la columna de
aplicabilidad real (`SEG 1`...`SEG 5`, `CAJA CENTRAL`, `CONAFIPS` — sí
están en el Excel, no se procesaron todavía). Filtrar el catálogo a
Segmento 2 específicamente (el segmento real de esta cooperativa) es el
siguiente paso lógico para cerrar esa brecha, documentado como pendiente
explícito en la propia respuesta del endpoint, no oculto.

Migración: `Contabilidad_CatalogoCucCompleto` — el SQL generado
(~1.130 `INSERT ... ON CONFLICT DO NOTHING`) se guardó como recurso
embebido en `Corela15.Infrastructure/Persistence/Seeds/
CatalogoCucCompleto.sql` (referenciado en el `.csproj` como
`EmbeddedResource`) en vez de inline en el archivo de migración — 422KB
de SQL generado no es practicable como string literal C#. La migración
lee el recurso embebido vía `Assembly.GetManifestResourceStream` y lo
ejecuta con `migrationBuilder.Sql(...)`. `Down()` revierte por
`creado_por = 'seed:catalogo_seps_oficial'`, no por lista de IDs.

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
**Pago de cuota implementado y probado end-to-end**
(`IPrestamoService.PagarCuotaAsync`): paga la próxima cuota pendiente (la de
menor número aún no pagada), separa capital e interés, actualiza
`Prestamo.Saldo`, y registra un asiento de **tres líneas** (débito Caja /
crédito Cartera de créditos por el capital / crédito `5101` Intereses
ganados por el interés — sembrada para esto) — la primera vez que un
comprobante en el proyecto usa más de 2 líneas, confirmando que
`IComprobanteContableService` ya soportaba N líneas desde el diseño
original de Nivel 1, sin cambios. Si era la última cuota, el préstamo pasa
a `Cancelado` automáticamente. Probado un ciclo de vida completo real:
solicitud → desembolso → 3 pagos de cuota → saldo exacto en `$0.00` →
préstamo cancelado → un 4º intento de pago rechazado correctamente (422,
`PrestamoInvalidoException`) porque ya no está vigente. Verificado en la
base que `1401` Cartera queda neteada en cero y `5101` Intereses ganados
acumula exactamente el interés total esperado.

Pantalla real en el frontend (`Creditos.tsx`: solicitar, listar, botón
"Desembolsar", cartera de préstamos con botón "Pagar cuota").

**Apertura y cancelación de DPF implementadas y probadas end-to-end**
(`Corela15.Application.Inversion.IDepositoService`, `DepositoService`):
`POST /api/plazofijo/depositos` busca la tasa vigente en el tablero
(`inversion.item_plazo_tasa`, sembrado con 4 rangos reales por plazo:
30-89d 5.5%, 90-179d 7%, 180-359d 8.5%, 360-720d 10%) por
monto/plazo/tipo de persona — nunca hardcodeada — y registra el asiento
(débito Caja / crédito `2103` Depósitos a plazo fijo, sembrada para esto)
vía `APER-DPF`. `POST /api/plazofijo/depositos/{id}/cancelar` revierte el
asiento vía `CANC-DPF` y marca el depósito `Cancelado`. Probado: apertura
con tasa correcta según el tablero (180 días → 8.50% exacto), cancelación,
doble cancelación rechazada, y verificado en la base que `1101`/`2103`
netean en cero tras el ciclo completo. Pantalla real integrada en
`Creditos.tsx` (mismo módulo que Crédito, ya que `modules.ts` los agrupa
como "Créditos y Plazo Fijo").

**Tasas techo BCE (regulatorio real, no inventado)**: `credito.tasa_techo_bce`
(`Corela15.Domain.Credito.TasaTechoBce`) guarda la tasa de interés activa
efectiva **máxima** por segmento, publicada mensualmente por la Junta de
Política y Regulación Monetaria y Financiera (BCE) — con vigencia real
(`FechaVigenciaDesde`), no un valor fijo. Sembrado con los 10 segmentos
reales (Productivo Corporativo/Empresarial/PYMES, Consumo Ordinario/
Prioritario, Vivienda/Vivienda de Interés Público, Microcrédito Minorista/
Acumulación Simple/Acumulación Ampliada) con las tasas **máximas** reales
vigentes a marzo 2026 (fuente: circulares BCE, compiladas en
verifacturaec.com — no un placeholder, ver migración
`Credito_TasaTechoBce` para el detalle). `TipoPrestamo.SegmentoBce` liga
cada producto sembrado a su segmento real: `CONS`→Consumo Prioritario,
`PROD`→Productivo PYMES (el segmento realista para una cooperativa
Segmento 2, no Corporativo), `MICRO`→Microcrédito Acumulación Ampliada
(el techo más conservador de los 3 subsegmentos de microcrédito, porque
el catálogo hoy no distingue subsegmento — simplificación explícita).
`IPrestamoService.SolicitarAsync` valida `TipoPrestamo.TasaAnual` contra
el techo **vigente a la fecha de la solicitud** (no el que existía cuando
se sembró el producto — si el BCE baja un techo, un producto que era
válido se detecta como inválido en la próxima solicitud, no requiere
revisar productos uno por uno). Probado: solicitud normal (dentro del
techo) sigue funcionando; producto de prueba con tasa 50% rechazado con
422 y mensaje explícito (`TasaExcedeTechoBceException`). **Las tasas techo
cambian mes a mes — hasta que exista una pantalla de Configuración para
mantenerlas (pendiente, mismo patrón que el resto de catálogos), actualizar
con una fila nueva (nunca editar la existente, se conserva el historial de
vigencia) cuando el BCE publique una circular nueva.**

Pendiente dentro de Nivel 3: renovación de DPF con tasa vigente ✅ **hecho**
(ver sección "Renovación de DPF con tasa vigente" más abajo), cálculo de
interés devengado proporcional en cancelación anticipada (hoy devuelve
solo el capital nominal), motor de scoring de `SolicitudPrestamo`
(`SOLICITUD_PRESTAMO_CALIFICACION` en Softbank, fuera de alcance),
microsegmentación real de Microcrédito (hoy
un solo producto/segmento, sin distinguir Minorista/Acum. Simple/Acum.
Ampliada), y el auto-débito de cuota por SPI ya documentado arriba.

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

**Apertura/cierre de ventanilla implementado y probado end-to-end**
(`Corela15.Application.Cajas.IVentanillaService`): `POST /api/cajas/
ventanillas` abre la sesión de caja del día para un cajero (rechaza si ya
tiene una abierta hoy — `VentanillaYaAbiertaException`, 422). `POST /api/
cajas/ventanillas/{id}/cerrar` cierra la ventanilla y registra el cuadre
(`VentanillaCuadre`) con el efectivo/cheque contado por el cajero (rechaza
si ya está cerrada — `VentanillaInvalidaException`, 422). **Limitación real,
documentada explícitamente en la interfaz, no oculta**: el cuadre no compara
contra un monto esperado calculado — registra el conteo declarado como
"cuadrado" por definición, porque el detalle de movimientos de efectivo por
transacción vinculado a una ventanilla (ver arriba) todavía no existe; la
reconciliación automática real requiere construir antes ese vínculo.
Probado vía curl contra Postgres: apertura, doble apertura del mismo cajero
en el mismo día rechazada (422), cierre, doble cierre rechazado (422),
listado refleja `cerrada`/`cuadrada` correctamente. Pantalla real
(`Cajas.tsx`, reemplaza el placeholder "Próximamente"): formulario de
apertura por cajero, tabla de ventanillas con acción "Cerrar" por fila que
abre un formulario de cuadre (efectivo + cheques).

**Nivel 6** — esquema `nomina` (`empleado`, `rol_pagos`, `rol_pagos_empleado`)
— verificado contra Softbank. Décimo tercero/cuarto, fondos de reserva y
provisión de vacaciones (obligatorios en Ecuador, `EMPLEADO_DECIMOTERCERO`/
`_DECIMOCUARTO`/`_FONDOSRESERVA`/`_PROVISION_VACACION`) quedan fuera de
alcance inicial — se agregan cuando se construya el cálculo real de rol de
pagos, no antes. Migración: `Nivel6_Nomina`. `Nivel6_SeedEmpleados` sembró
2 empleados de **ejemplo** (nombres/cédulas ficticios, mismo patrón que
`Nivel0_SeedDatosPrueba`) para que la pantalla tenga contenido real.

Hallazgo real de la verificación contra Softbank antes de diseñar el caso
de uso: `NOMINA.CARGO` **no** guarda un sueldo base, y no existe ninguna
otra tabla de sueldo-por-cargo — el ingreso se digita directamente en
`NOMINA.ROLPAGOS_EMPLEADO.INGRESOS` cada período. Por eso `Empleado` acá
tampoco tiene un campo `SueldoBase`: hubiera sido un campo inventado que no
existe en el sistema real que se está reemplazando.

**Generación de rol de pagos implementada y probada end-to-end**
(`Corela15.Application.Nomina.IRolPagosService.GenerarAsync`): `POST /api/
nomina/roles-pagos` recibe período + tipo (Mensual/Quincenal) + una línea
por empleado (ingresos, egresos, días laborados, digitados por quien genera
el rol, no derivados), valida que cada empleado exista y esté activo
(`EmpleadoInvalidoException`, 422), rechaza período+tipo duplicado a nivel
de aplicación además del índice único de `RolPagos(Periodo, Tipo)` en la
base (`RolPagosDuplicadoException`, 422), y rechaza un rol sin líneas
(`RolPagosSinLineasException`, 400). Cada línea calcula `Total = Ingresos -
Egresos` y el rol queda `Procesado`. Sin asiento contable: como en
Softbank, el rol de pagos es el registro fuente, no el pago en sí (el pago
real y su asiento contable — débito Gasto sueldos / crédito Caja o Banco —
quedan fuera de alcance, se agregan cuando se construya el caso de uso de
pago de rol). Probado vía curl: generación con 2 empleados (total exacto
$888.90 sobre $460+$520 de ingresos menos $42.50+$48.60 de egresos),
duplicado de período+tipo rechazado, empleado inexistente rechazado, rol
sin líneas rechazado (400). Pantalla real (`Nomina.tsx`, reemplaza el
placeholder "Próximamente"): formulario para agregar líneas de empleado
dinámicamente, tabla de roles de pago generados con total y estado.

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

**Registro y abono de cuenta por cobrar implementado y probado
end-to-end** (`Corela15.Application.CuentasPorCobrar.ICuentaPorCobrarService`):
`Contabilidad_SeedCuentaPorCobrar` agregó las subcuentas de detalle reales
`1601` Cuentas por cobrar varias (bajo el grupo 16) y `5601` Otros ingresos
varios (bajo el grupo 56), y el motor configurable `REG-CXC`/`ABONO-CXC`
— mismo patrón que `DEP-EFEC`/`APER-DPF`. `POST /api/tesoreria/
cuentas-por-cobrar` registra la cuenta (débito `1601` / crédito `5601`) y
rechaza monto ≤ 0 (400). `POST /api/tesoreria/cuentas-por-cobrar/{id}/
abonos` registra el abono (débito Caja / crédito `1601`), rechaza un abono
que exceda el saldo pendiente (`AbonoExcedeSaldoException`, 422), y marca
la cuenta `Cancelada` automáticamente cuando el saldo llega exacto a cero
— mismo patrón que el cierre de préstamo (Nivel 3). Un abono a una cuenta
ya cancelada se rechaza (`CuentaPorCobrarInvalidaException`, 422). Probado
vía curl: registro de $150.00, abono parcial de $50.00, abono que excede
saldo rechazado, abono final de $100.00 que cancela la cuenta, abono
posterior rechazado, y verificado en la base que `1601` netea en cero
mientras `5601` mantiene el ingreso reconocido ($150.00 en crédito, como
corresponde). Pantalla real (`Tesoreria.tsx`, reemplaza el placeholder
"Próximamente"): formulario de registro, tabla de cuentas por cobrar con
acción "Abonar" por fila mientras estén vigentes. `CuentaPorPagar` queda
modelada (misma estructura, grupo CUC 25) pero sin caso de uso todavía —
se agrega cuando haga falta un flujo real de pago a terceros.

**Nivel 8** — esquema `riesgo` (`macroproceso` → `proceso`, `nivel_impacto`,
`nivel_probabilidad`, `nivel_riesgo` —sembrados con escalas 1-5 y matriz de
rangos—, `evento_riesgo`): patrón genérico de gestión de riesgo verificado
contra RIESGOOPERATIVO, confirmado reutilizable para cualquier tipo de
riesgo (operativo, liquidez), no solo operativo. Esquema `reportecontrol`
(`reporte_regulatorio`): sembrado con 13 códigos de reportes reales
confirmados activos en Softbank hoy (B13, D01, BCE01/02, S01, L01/L02,
IG01, TIN, UAF, RFD, ROTEF, CRS). Migración: `Nivel8_RiesgoYReporteria`.
`Nivel8_SeedProcesos` sembró 3 macroprocesos y 4 procesos de **ejemplo**
para que el registro de evento de riesgo tenga sobre qué aplicarse.

**Registro de evento de riesgo implementado y probado end-to-end**
(`Corela15.Application.Riesgo.IEventoRiesgoService`): `POST /api/riesgo/
eventos` recibe proceso + descripción + nivel de impacto + nivel de
probabilidad, calcula el puntaje como `NivelImpacto.Nivel ×
NivelProbabilidad.Nivel` (rango 1-25) y ubica automáticamente el
`NivelRiesgo` correspondiente según el rango sembrado (Bajo 1-6, Moderado
7-12, Alto 13-19, Extremo 20-25) — el operador nunca elige el nivel a
mano, se deriva de la matriz, siguiendo el mismo patrón verificado en
RIESGOOPERATIVO. Valida que el proceso, el nivel de impacto y el nivel de
probabilidad existan y estén activos (422). Sin asiento contable: es un
registro de identificación de riesgo, no un movimiento financiero. Probado
vía curl con los 4 extremos de la matriz (1×1=1→Bajo, 3×3=9→Moderado,
4×4=16→Alto, 5×5=25→Extremo, cada uno cae exacto en su rango) y proceso
inexistente rechazado (422). Pantalla real (`Riesgo.tsx`, reemplaza el
placeholder "Próximamente"): formulario de registro con selección de
proceso/impacto/probabilidad, tabla de eventos con el nivel de riesgo
resultante coloreado según `NivelRiesgo.Color`. Los planes de acción y
etapas de avance (`EVENTO_PLANACCION` en Softbank) quedan fuera de
alcance — se agregan cuando se construya el caso de uso real de
seguimiento, esto modela solo el registro base del evento.

**Actualización real, ver sección "Estados financieros B11/B13 (SEPS)"
más abajo**: la advertencia de arriba seguía siendo válida mientras la
única fuente disponible eran los PDFs de la web pública de SEPS
(binarios/escaneados, no legibles). El usuario descargó directamente los
manuales técnicos reales desde
`https://www.seps.gob.ec/manuales-para-la-gestion-de-envio-de-informacion-esfps/`
— esos SÍ son texto extraíble, y con ellos ya se implementó B11/B13 con
estructura, tipos de dato y controles de validación reales, citados con
su fuente exacta. El resto de reportes del índice (D01, BCE01/02, S01,
L01/L02, IG01, TIN, UAF, RFD, ROTEF, CRS) sigue pendiente de la misma
verificación — los manuales de varios de ellos (Depósitos, Socios,
Cartera de Créditos y Contingentes, Servicios Financieros, Riesgo de
Liquidez L02, Indicadores de Género IG01, Cobros Indebidos CI01,
Obligaciones Financieras, Tablas de Información) ya están descargados en
`C:\Users\ksantana\Documents\manuales seps\` (con nombres renombrados sin
tildes por un problema de normalización Unicode en las rutas — el
contenido es el original), a procesar de la misma forma cuando se
retome esta línea de trabajo.

**Registro de gestión de cobranza implementado y probado end-to-end**
(`Corela15.Application.Cobranza.IGestionCobranzaService`): `POST /api/
cobranzas/gestiones` valida que el préstamo, el cliente y la acción de
gestión existan y estén activos, y registra el contacto (llamada, visita,
acuerdo de pago...) con `TieneCompromisoPago` y observación libre — sin
asiento contable, es un registro de seguimiento, no un movimiento de
dinero. Pantalla real (`CobranzasCumplimiento.tsx`): pestañas "Cartera en
mora" / "Gestiones" (ver sección "Motor de mora real" más abajo — el
selector de préstamo vigente ya no muestra todos los vigentes por
igual).

Pendiente dentro de Nivel 4: motor de scoring PLA (`CalificacionCliente`
tiene la entidad pero no el caso de uso de cálculo). Días de mora reales
✅ **hecho**, ver sección siguiente.

## Motor de mora real (`IMoraCarteraService`)

Gap operativo real identificado en la auditoría de profundidad del
sistema (no un incidente puntual como los tres de
`01-contexto-origen.md`, sino una revisión completa módulo por módulo):
el cálculo de días de mora existía, pero **solo vivía escondido dentro
de `ProvisionCarteraService`**, calculado inline y nunca expuesto a
nada más. Cobranza tenía su propio catálogo de tramos
(`cobranza.periodo_mora`, sembrado desde Nivel 4: Preventiva/Gestión/
Comité I/Comité II/Judicial) sin ningún caso de uso que lo alimentara —
el selector de préstamos para registrar una gestión mostraba **todos**
los préstamos vigentes mezclados, vencidos o no, porque no había forma
de saber cuáles realmente estaban en mora.

`Corela15.Application.Colocacion.IMoraCarteraService` /
`MoraCarteraService.CalcularAsync()`: única fuente de verdad para "días
de mora de un préstamo" en todo el sistema — hoy menos la fecha de
vencimiento (`PrestamoRubro.FechaFin`) de la cuota de capital impaga más
antigua, 0 si no tiene ninguna vencida. `ProvisionCarteraService` se
refactorizó para consumir este mismo servicio en vez de tener su propio
cálculo duplicado (mismo resultado exacto, verificado: préstamo con
cuota vencida hace 45 días → categoría B2, $100 de provisión sobre
$1,000, idéntico antes y después del refactor).

`CobranzasController.PrestamosVigentes` (`GET /api/cobranzas/prestamos`)
ahora filtra a solo préstamos con `DiasMora > 0` — el fix real del gap
documentado — y expone el tramo (`cobranza.periodo_mora`) de cada uno.
`GET /api/cobranzas/mora/resumen` agrega cantidad de préstamos y saldo
total por tramo, incluyendo los que están al día (tramo `PREV`), para
una vista completa de la distribución de cartera.

Probado end-to-end contra Postgres real usando el préstamo real del
ambiente de desarrollo (no datos sintéticos aislados esta vez, porque
`Prestamo`/`PrestamoRubro` no se pueden fabricar sin pasar por el ciclo
completo de solicitud→desembolso, y el préstamo real ya existía):
backdate temporal de la fecha de vencimiento de la cuota 1 a 45 días
atrás → `GET /api/cobranzas/prestamos` mostró el préstamo con
`diasMora: 45`, tramo "Comité de mora I"; `GET /api/cobranzas/mora/
resumen` lo contó correctamente en `CM1`; `POST /api/creditos/
provision-cartera/calcular` clasificó el mismo préstamo en B2 ($100 de
provisión), confirmando que ambos consumidores ven exactamente la misma
mora. Fecha de vencimiento restaurada a su valor original después.

**Bug real encontrado y corregido durante esta prueba, más serio que el
motor de mora en sí**: `ComprobanteContableServiceTests.DisposeAsync()`
borraba la fila **completa** de `saldo_contable` de las cuentas
`1101`/`2101` del período actual al terminar, asumiendo que el ambiente
de desarrollo siempre arranca vacío para esas cuentas — una asunción que
dejó de ser cierta en cuanto hubo actividad real de un usuario probando
la app en paralelo. Correr `dotnet test` borró en silencio el saldo real
de esas dos cuentas (detectado porque el balance de comprobación pasó de
cuadrado a descuadrado sin que nadie tocara nada). Mismo problema en
`ProvisionCarteraServiceTests`: `EjecutarCalculoAsync_SinCarteraVigente_
NoRequiereProvision` corre contra **toda** la cartera vigente real del
ambiente compartido (no solo préstamos de prueba) y no revertía el
comprobante que podía generar. Ambos tests se corrigieron para revertir
solo el delta exacto que ellos mismos aportaron (calculado a partir de
sus propias líneas de movimiento), nunca la fila completa — el mismo
principio de limpieza quirúrgica que se viene aplicando manualmente en
cada sesión de pruebas contra este ambiente, ahora también dentro de los
tests automatizados. Datos reales del usuario reconstruidos exactamente
después de detectar el borrado accidental, verificados contra las líneas
de los comprobantes reales que seguían intactos.

Pantalla real: nueva pestaña "Cartera en mora" en
`CobranzasCumplimiento.tsx` (antes solo tenía "Gestiones") con tarjetas
resumen por tramo (cantidad + saldo) y tabla de préstamos vencidos con
días de mora y badge de tramo coloreado; el selector de préstamo dentro
de "Registrar gestión" ahora muestra días de mora y tramo en cada
opción, y avisa explícitamente si no hay ningún préstamo vencido en vez
de mostrar un selector vacío sin explicación.

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
