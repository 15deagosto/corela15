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
    App.tsx, main.tsx, lib/api.ts
docker-compose.yml            # Postgres local
.env.core                     # credenciales del Postgres NUEVO (gitignored)
```

## Cómo levantar el entorno local

```bash
# 1. Postgres
docker compose --env-file .env.core up -d

# 2. Backend (desde backend/src/Corela15.Api)
CORELA15_CONNECTION="Host=localhost;Port=5432;Database=corela15_core;Username=corela15_admin;Password=<ver .env.core>" \
ASPNETCORE_URLS="http://localhost:5080" \
dotnet run

# 3. Frontend (desde frontend/)
npm run dev   # http://localhost:5173 — si ya hay otro Vite corriendo (ej. SIGA
               # en la misma máquina), Vite salta automáticamente al próximo
               # puerto libre (5174, 5175...); revisar el log de `npm run dev`.
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
2. **Nivel 1** — Motor contable (plan de cuentas SEPS, asientos, saldos) ✅ **hecho** (estructura + versionado; falta UI y casos de uso de registro de asientos)
3. **Nivel 2** — Ahorros (captación a la vista)
4. **Nivel 3** — Plazo Fijo + Crédito/Colocación
5. **Nivel 4** — Cobranzas + Cumplimiento/PLA
6. **Nivel 5** — Caja/Bóveda
7. **Nivel 6** — Nómina propia
8. **Nivel 7** — Tesorería y activos internos
9. **Nivel 8** — Riesgo y reportería regulatoria (SEPS/BCE)
10. **Periféricos** — sin orden obligatorio entre ellos

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

Pendiente dentro de Nivel 1: casos de uso en `Corela15.Application` para
registrar comprobantes (validando que suma de débitos = suma de créditos
antes de persistir — ese invariante no está en la base de datos, es
responsabilidad de la capa de aplicación), actualización de `saldo_contable`
al contabilizar, y pantallas en el frontend.

## Frontend — módulos visibles al usuario

`frontend/src/modules.ts` define los módulos con nombre/ícono/estado
amigables para quien usa el sistema — la jerga interna de "Nivel 0/1/2..."
vive solo en este documento y en los `.md` de arquitectura, nunca en la UI.
Al completar un nivel, actualizar el `estado` del módulo correspondiente
(`disponible` / `en-construccion` / `proximamente`).
