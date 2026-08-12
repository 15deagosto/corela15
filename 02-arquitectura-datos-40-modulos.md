# Arquitectura de datos — mapeo para core financiero propio

Documento de trabajo para ir construyendo, módulo a módulo y en orden de
dependencias, un core financiero propio. Cada nivel se documenta **después de
verificarlo contra la base real** (`Softbank`, solo lectura) — no son
suposiciones, son tablas y columnas confirmadas con consultas reales.

**Regla de secuencia**: un nivel nunca depende de tablas/módulos de un nivel
posterior. Si algo de Nivel 2 necesita algo que solo existe en Nivel 3, está
mal clasificado — hay que revisar.

## Convención de nombres (decisión tomada, no solo referencia)

A diferencia de la nota original de este documento, **la decisión final es
reusar los nombres de Softbank para toda entidad que mapee 1:1** —
`PERSONA`, `CLIENTE`, `CUENTA`, `PRESTAMO`, `DEPOSITO`, los nombres de
esquema (`AHORROS`, `COLOCACION`, `CREDITO`, `INVERSION`, `CONTABILIDAD`,
`COBRANZA`, `LAVADOACTIVOS`, etc.) y los códigos de catálogo que vienen de
norma externa (códigos de cuenta CUC, códigos de agencia, códigos de tipo
de identificación). Motivo: facilita un futuro script de migración
dato-a-dato y evita ambigüedad de "¿esto es lo mismo que aquello?" al
comparar ambos sistemas.

**Lo que NO se hereda** — la deuda técnica documentada en cada nivel, no el
nombre:
- Tablas de respaldo manual tipo `CUENTACONTABLE_2022A` (copiar toda la
  tabla a mano en vez de versionar) → usar versionado nativo (`FOR SYSTEM_TIME`
  o equivalente) desde el día uno, en toda tabla que cambie de estado en el
  tiempo — no solo en las que Softbank versionó bien (`CUENTA`,
  `CUENTA_ITEMSALDO`, `PRESTAMO`, `PRESTAMO_RUBRO`, `ITEMPLAZO_TASA`).
- Ausencia de llaves foráneas declaradas (confirmado: `CLIENTES.CLIENTE` no
  tiene ninguna FK real en el motor, las relaciones son por convención de
  nombre de columna) → declarar FKs reales.
- Configuración sin historial de cambios (patrón `EMPRESA_*`, más de 40
  tablas de una sola fila) → versionar también la configuración, no solo
  los datos transaccionales.
- Auditoría que existe en el esquema pero nunca se activó
  (`SEGURIDAD.LOGAUDITORIA` vacía desde siempre, `NOMBRETABLA_GENERA_LOG`
  con una sola fila y `ACTIVO=False`) → si se declara una tabla de
  auditoría, se activa y se usa, o no se declara.

**Nombres en español**: se mantiene el español para todo lo visible a
usuario final o a la norma (igual que en SIGA), siguiendo la misma
convención que ya usa Softbank.

---

## Nivel 0 — Cimientos (identidad, seguridad, catálogos)

Nada del resto del sistema puede existir sin esto. Mapeado contra 4 esquemas
de Softbank: `SUJETO`, `CLIENTES`, `SEGURIDAD`, `GENERAL` (297 tablas en
total entre los cuatro; acá solo se documentan las núcleo — el resto son
variantes/detalle que se pueden sumar después sin bloquear nada).

### 1.1 Identidad de personas (`SUJETO`)

Cadena real en Softbank: `PERSONA` (datos comunes) → `PERSONA_NATURAL` **o**
`PERSONA_JURIDICA` (mutuamente excluyentes, mismo `ID`/`IDPERSONA`).

**`SUJETO.PERSONA`** (14.526 filas) — el núcleo de identidad, comparte todo
lo común a natural/jurídica:
| Columna | Qué es |
|---|---|
| `ID` | PK |
| `IDENTIFICACION` | Cédula/RUC/pasaporte |
| `IDTIPOIDENTIFICACION` | FK a tipo de identificación |
| `NOMBRE` | Nombre completo o razón social ya concatenado |
| `EMAIL` | — |
| `IDPAIS`, `IDACTIVIDADECONOMICA`, `IDRESIDENCIA` | FKs a catálogos |
| `ACTIVOS`, `PASIVOS`, `INGRESOS`, `EGRESOS` | Declarados, para análisis financiero/buró |
| `NUMEROCASA`, `BARRIO`, `CALLEPRINCIPAL`, etc. | Dirección |

**`SUJETO.PERSONA_NATURAL`** (13.911 filas) — extensión 1:1 vía `IDPERSONA`:
`PRIMERNOMBRE`/`SEGUNDONOMBRE`/`APELLIDOPATERNO`/`APELLIDOMATERNO` por
separado (a diferencia de `PERSONA.NOMBRE` que ya viene concatenado),
`FECHANACIMIENTO`, `ESMASCULINO`, estado civil, educación, vivienda,
profesión, huella dactilar (`TIENEHUELLA`/`CODIGODACTILAR`), `ESPEPS`
(persona expuesta políticamente — relevante para cumplimiento/UAF desde el
día uno del diseño, no como añadido posterior).

**`SUJETO.PERSONA_JURIDICA`** (615 filas) — extensión 1:1 vía `IDPERSONA`:
`RAZONSOCIAL`, `FECHACREACION`, `ESGRUPO`, `ESINTITUCIONBANCARIA`,
`PAISCONSTITUCION`, `ESPUBLICA`.

Tablas de soporte identificadas (no bloquean el diseño inicial, se agregan
cuando haga falta ese detalle): `CONYUGE`, `REPRESENTANTE` /
`REPRESENTANTE_JURIDICO` (firmantes autorizados de personas jurídicas),
`PERSONA_TELEFONO`, `REFERENCIA` / `REFERENCIA_BANCARIA` /
`REFERENCIA_COMERCIAL`, `PERSONA_INGRESOGASTO` (192.525 filas — detalle
histórico de ingresos/gastos declarados), listas de control para
cumplimiento (`LISTACONTROL_DETALLE_PERSONA_PEPS`: 445.066 filas,
`LISTACONTROL_DETALLE_SENTENCIADO`: 988.626 filas — screening AML/OFAC, se
integran en el módulo de Cumplimiento del Nivel 4, pero el *gancho* en el
modelo de personas debe preverse desde ahora).

### 1.2 Vínculo persona → socio de la cooperativa (`CLIENTES`)

**`CLIENTES.CLIENTE`** (13.346 filas) — la persona *se vuelve* socio acá:
| Columna | Qué es |
|---|---|
| `ID` | PK — este es el ID que usan Ahorros/Crédito/Inversión para referenciar al socio |
| `NUMERO` | Número de socio visible |
| `IDPERSONA` | FK a `SUJETO.PERSONA` |
| `IDAGENCIA` | En qué agencia se afilió/opera |
| `USUARIOOFICIAL` | Asesor asignado |
| `CODIGOCALIFICACIONINTERNA`, `CODIGOCAUSAVINCULACION` | — |
| `CODIGOESTADO` | Activo/inactivo |

Nota de diseño (confirmada en la investigación de hoy): una persona puede
tener **cero, uno, o varios** registros `CLIENTE` a lo largo del tiempo (no
es estrictamente 1:1) — el core propio debería decidir explícitamente si
permite eso o fuerza 1:1 desde el diseño, en vez de heredarlo por accidente.

### 1.3 Seguridad y usuarios (`SEGURIDAD`) — ya construido en SIGA, referencia

Este nivel **ya lo tenemos resuelto** en SIGA (`/admin/permisos`, construido
hoy): usuario → rol → menú/módulo. Mapeo de referencia por si el core propio
lo necesita desde cero:

- **`USUARIO`** (254 filas, 47 activas): usuario, nombre, agencia, flags de
  acceso (`PUEDEINGRESARSISTEMA`, `TIENEBLOQUEO`, `ACTIVO`).
- **`ROL`** (135 filas): catálogo de roles, con `NIVEL` (jerarquía numérica).
- **`USUARIO_ROL`**: N:M usuario↔rol, con `ACTIVO` (un usuario puede tener
  varios roles a la vez, confirmado — hasta 13 roles en un caso real).
- **`MENU`** / **`MENU_PADRE`** / **`ROL_MENU`**: catálogo jerárquico de
  opciones (módulo → submódulo → opción) y qué rol accede a cuál.
- **`ACCION_INGRESO_USUARIO`** (82.166 filas): log de inicios de sesión —
  esto sí es un ejemplo real de auditoría *que funciona* en Softbank (a
  diferencia de `LOGAUDITORIA`, que está vacía siempre — ver hallazgo del
  incidente SPI). Si se diseña un core propio, este es el patrón a copiar
  (loggear en la tabla correcta desde el día uno), no el patrón vacío.

### 1.4 Catálogos generales (`GENERAL`)

- **`EMPRESA`** (1 fila) — la cooperativa misma: código, nombre, moneda,
  país. Todo lo demás cuelga de `IDEMPRESA`.
- **`AGENCIA`** (4 filas, 3 operativas + 1 consolidador a excluir — Regla #2
  de CLAUDE.md) — código, nombre, si es operativa/activa, zona.
- Catálogos de apoyo confirmados: `MONEDA` (3), `PAIS` (228),
  `ACTIVIDAD_ECONOMICA` (3.063, con jerarquía padre-hijo), `DIVISION_POLITICA`
  (provincia/cantón/parroquia, 2.139 filas), `TIPO_IDENTIFICACION` (8),
  `CALENDARIO_SISTEMA` (16.802 filas — días hábiles/feriados, necesario para
  cualquier cálculo de plazos/vencimientos).
- Patrón `EMPRESA_*` (más de 40 tablas, todas de 1 fila): configuración
  global por módulo, una fila por empresa. **Ya documentamos el problema de
  este patrón hoy**: son campos de configuración sin historial de cambios
  (no son tablas temporales, no hay auditoría real detrás) — si el core
  propio reusa esta idea de "una fila de parámetros por módulo", debe
  agregarle versionado desde el diseño (tabla temporal de SQL Server o
  equivalente), no como parche después.

---

## Nivel 1 — Motor contable

A diferencia de los niveles anteriores, este **no se basa solo en lo que
Softbank implementó** — se contrastó contra la norma real: el
**Catálogo Único de Cuentas (CUC)** de la SEPS, de uso obligatorio para
cooperativas de ahorro y crédito, cajas centrales, CONAFIPS y mutualistas
([Resolución SEPS-IGT-IGS-INSESF-INR-INFMR-INGINT-2022-0194, codificado con
reforma octubre 2023](https://www.seps.gob.ec/wp-content/uploads/CUC-codificado-2023_reforma-octubre-2023.pdf)).
El diseño de un core propio **debe partir del CUC**, no de cómo Softbank
decidió mapearlo — el CUC es la norma; la implementación de Softbank es una
interpretación posible de esa norma (y hoy encontramos varios lugares donde
esa interpretación tiene huecos: `LOGAUDITORIA` vacía, config sin historial).

### 1.1 Estructura oficial del plan de cuentas (SEPS)

Grupos de primer nivel (dígito 1) y segundo nivel (2 dígitos) confirmados
directamente del CUC:

| Código | Grupo |
|---|---|
| **1** | **ACTIVO** |
| 11 | Fondos disponibles (caja, bancos, efectos de cobro inmediato) |
| 12 | Operaciones interfinancieras |
| 13 | Inversiones |
| 14 | **Cartera de créditos** (ver detalle abajo — clave para el módulo de Crédito) |
| 16 | Cuentas por cobrar |
| 17 | Bienes realizables, adjudicados por pago y bienes no utilizados |
| 18 | Propiedades y equipo |
| 19 | Otros activos |
| **2** | **PASIVO** |
| 21 | Obligaciones con el público (depósitos — clave para Ahorros/DPF) |
| 22 | Operaciones interfinancieras |
| 23 | Obligaciones inmediatas |
| 25 | Cuentas por pagar |
| 26 | Obligaciones financieras |
| 27 | Valores en circulación |
| 28 | Aportes para futuras capitalizaciones |
| 29 | Otros pasivos |
| **3** | **PATRIMONIO** |
| 31 | Capital social |
| 32 | Prima en colocación de certificados de aportación |
| 33 | Reservas |
| 34 | Otros aportes patrimoniales |
| 35 | Superávit por valuaciones |
| 36 | Resultados |
| **4** | **GASTOS** |
| 41 | Intereses causados |
| 42 | Comisiones causadas |
| 43 | Pérdidas financieras |
| 44 | Provisiones |
| 45 | Gastos de operación |
| 46 | Otras pérdidas operacionales |
| 47 | Otros gastos y pérdidas |
| **5** | **INGRESOS** |
| 51 | Intereses y descuentos ganados |
| 52 | Comisiones ganadas |
| 53 | Utilidades financieras |
| 54 | Ingresos por servicios |
| 55 | Otros ingresos operacionales |
| 56 | Otros ingresos |
| **6** | **CUENTAS CONTINGENTES** (61 deudoras, 62/63 por el contrario, 64 acreedoras) |
| **7** | **CUENTAS DE ORDEN** (71 deudoras, 73 por el contrario, 74 acreedoras) |

**Grupo 14 — Cartera de créditos**, el que más importa para el módulo de
Crédito (Nivel 3): la SEPS exige clasificar cada operación por **tipo de
crédito** (productivo, consumo, inmobiliario, microcrédito, vivienda de
interés social, educativo) **cruzado con estado** (por vencer / refinanciada
/ reestructurada / que no devenga intereses / vencida). Ejemplo real:
`1401` cartera productiva por vencer, `1425` cartera productiva que no
devenga intereses, `1449` cartera productiva vencida, `1409` cartera
productiva refinanciada por vencer, `1417` reestructurada por vencer — y así
cruzado para cada uno de los 5 tipos de crédito. **Esto confirma, con la
norma en mano, la Regla #10 de CLAUDE.md** (Mora SEPS = (VENCIDO + NDI) /
TOTAL): la SEPS no deja esa fórmula a interpretación de cada cooperativa —
la cartera *ya nace clasificada* en esos tres baldes por diseño del catálogo.

### 1.2 Lo que Softbank construyó encima del CUC (`CONTABILIDAD`, `FINANCIERO`)

179 tablas mapeadas entre los dos esquemas. Núcleo real (verificado, con
filas actuales):

| Tabla | Filas | Rol |
|---|---|---|
| `CONTABILIDAD.CUENTACONTABLE` | 2.199 | El plan de cuentas de la cooperativa — instancia del CUC con sus propias subcuentas de detalle |
| `CONTABILIDAD.CUENTACONTABLE_PADRE` | 2.190 | Jerarquía (cuenta → cuenta padre), para poder sumar de detalle a grupo |
| `CONTABILIDAD.CUENTACONTABLE_ACTIVOSCONTIGENTES` | 9 | Ponderación de riesgo por cuenta — requisito de Basilea/SEPS para patrimonio técnico |
| `CONTABILIDAD.PATRIMONIO_TECNICO` / `_DETALLE` | 19 / 224 | Cálculo regulatorio de patrimonio técnico (solvencia) |
| `CONTABILIDAD.CAUSAL` | 671 | Plantilla de asiento: qué cuenta debita/acredita cada tipo de operación (ya lo exploramos hoy a fondo) |
| `CONTABILIDAD.GENERADOR_CONTABLE` | 113 | El motor que decide, para cada transacción, qué causal/asiento generar |
| `CONTABILIDAD.COMPROBANTECONTABLE` | 246.468 | El asiento contable (cabecera) |
| `CONTABILIDAD.MOVIMIENTOCOMPROBANTECONTABLE` | 838.081 | Las líneas del asiento (débito/crédito por cuenta) |
| `CONTABILIDAD.MOVIMIENTOCOMPROBANTECONTABLE_TRANSACCION` | 11.972.538 | Vínculo detallado línea de asiento ↔ transacción origen — la tabla más grande del núcleo contable |
| `CONTABILIDAD.SALDOCONTABLE` | 1.804.727 | Saldo por cuenta y período — lo que consulta cualquier reporte financiero, para no tener que sumar movimientos cada vez |
| `FINANCIERO.TRANSACCION` | 166 | Catálogo maestro de tipos de transacción (distinto del causal contable — esta es "qué operación de negocio ocurrió", ya la vimos con el caso del abono automático SPI) |
| `FINANCIERO.MOVIMIENTO_AFECTACION` | 35.727.114 | El libro mayor real, a nivel más bajo — la tabla más grande de toda la base entre las revisadas hoy |
| `FINANCIERO.MOVIMIENTO_TRANSACCION` / `_DETALLE` | 563.305 / 1.139.180 | La transacción de negocio (lo que ve el cajero/usuario) y su detalle |
| `CONTABILIDAD.TIPO_COMPROBANTECONTABLE` | 5 | Tipos de comprobante (ingreso, egreso, diario, etc.) |
| `CONTABILIDAD.CUC_CARGA` | 0 (vacía) | Existe una tabla dedicada a cargar el CUC oficial — reafirma que el catálogo de Softbank está pensado para calzar 1:1 con la norma SEPS |

### 1.3 Lo que hay que hacer distinto (aprendizajes de hoy, no repetir)

1. **Versionar el plan de cuentas de verdad.** Existen `CUENTACONTABLE_2022A`
   y `CUENTACONTABLE_ESTRUCTURA_2022A` como *copias manuales* de una
   estructura anterior — el patrón "copio la tabla entera cada vez que
   cambia algo importante" es exactamente lo opuesto a versionado real
   (`FOR SYSTEM_TIME`). El core propio debería usar tablas temporales nativas
   para `CUENTACONTABLE` desde el día uno.
2. **Auditoría real, no una tabla vacía.** Cualquier cambio a `CAUSAL`,
   `GENERADOR_CONTABLE` o parámetros de la cuenta contable debe quedar
   loggeado con usuario y motivo — el patrón que sí funciona en Softbank es
   `SEGURIDAD.ACCION_INGRESO_USUARIO` (82.166 filas, con datos reales), no
   `SEGURIDAD.LOGAUDITORIA` (vacía siempre).
3. **El generador contable debe ser explícito y trazable.** Hoy nos costó
   bastante entender qué transacción dispara qué asiento (tuvimos que cruzar
   `GENERADOR_CONTABLE`, `CAUSAL` y `FINANCIERO.TRANSACCION` a mano). Un
   diseño propio debería dejar esa relación como configuración de primera
   clase, visible y documentada, no reconstruible solo por ingeniería
   inversa de datos.

---

## Nivel 2 — Ahorros (captaciones a la vista)

Esquema `AHORROS`, 130 tablas mapeadas. Grupo CUC de referencia: **21
Obligaciones con el público** — subcuentas `2101` depósitos a la vista,
`2102` operaciones de reporto, `2103` depósitos a plazo (Nivel 3), `2104`
depósitos de garantía, `2105` depósitos restringidos. Toda cuenta de
ahorros de la cooperativa cae, contablemente, en `2101` o en alguna de sus
variantes restringidas/de garantía.

### 2.1 Núcleo real

| Tabla | Filas | Rol |
|---|---|---|
| `CUENTA` | 20.725 | La cuenta de ahorros — **es tabla temporal nativa** (`SYSTEM_VERSIONED_TEMPORAL_TABLE`, con `CUENTA_HISTORICO` de 20,1 millones de filas). Este es el patrón bueno a copiar — a diferencia de `CUENTACONTABLE` en Nivel 1, que NO está versionada. |
| `CUENTA_CLIENTE` | 20.991 | Bridge cuenta ↔ socio, con `PRINCIPAL` (para no duplicar montos entre cotitulares — Regla #1 de CLAUDE.md) |
| `TIPO_CUENTA` | 12 | Catálogo de productos (Ahorro Vista, Certificados, Ahorro Infantil, etc.) — el que revisamos a fondo hoy con `DEBITOPRESTAMO` y `SALDOMINIMOCONPRESTAMO` |
| `ITEMSALDO` | 54 | Los "baldes" de saldo dentro de una cuenta (disponible, encaje, bloqueo, interés...) |
| `CUENTA_ITEMSALDO` | 85.462 | Saldo real por cuenta × ítem — **también temporal nativa** (con 9,7 millones de filas de historial). El campo `ACREDITAPRESTAMO` que investigamos hoy vive acá. |
| `TIPO_CUENTA_ITEMSALDO` | 63 | Qué ítems de saldo aplican a cada producto |
| `ITEMSALDO_TASA` | 18 | Tasa pasiva por ítem — **también temporal nativa** |
| `TRANSFERENCIA` / `TRANSFERENCIA_DEBITOCUENTA` | 2.286 | Transferencias entre cuentas |
| `CUENTA_TRANSFERENCIARECIBIDA` / `_ABONOPRESTAMO` | 3.396 / 399 | El mecanismo de auto-débito por SPI que investigamos a fondo — la tabla que conecta un depósito recibido con el abono automático al préstamo |
| `CUENTA_PROVISION_MOVIMIENTOAFECTACION` | 18.146.300 | Provisión de intereses día a día — la tabla más grande del esquema |
| `CUENTA_MOVIMIENTOTRANSACCIONDETALLE` | 668.441 | Detalle de movimientos (depósitos/retiros) por cuenta |

### 2.2 Aprendizajes de hoy aplicables al diseño

- **`CUENTA` y `CUENTA_ITEMSALDO` sí están bien versionadas** (temporal
  nativo) — a diferencia del plan de cuentas contable. Es la prueba de que
  Softbank *sabe* hacerlo bien cuando quiere; el core propio debería aplicar
  ese mismo estándar a *todo* lo que cambia de estado con el tiempo, no
  solo a lo transaccional.
- **El auto-débito de préstamo desde una cuenta de ahorro cruza tres
  configuraciones independientes** (`ITEMSALDO.ACREDITAPRESTAMO`,
  `TIPO_CUENTA.DEBITOPRESTAMO`/`SALDOMINIMOCONPRESTAMO`, y el campo
  `COLOCACION.PRESTAMO.DEBITOSPI` de Nivel 3) que hoy **no se validan entre
  sí** — encontramos que el préstamo decía explícitamente "no aplica débito
  SPI" y el proceso lo hizo igual. Si el core propio implementa este
  mecanismo, debe ser **una sola fuente de verdad**, no tres flags
  independientes que un proceso puede o no consultar.

---

## Nivel 3 — Plazo Fijo, Crédito y Colocación

### 3.1 Plazo Fijo (`INVERSION`) — grupo CUC 2103

Ya mapeado a fondo hoy durante la investigación de tasas DPF:
`DEPOSITO` (el certificado, con `TASA`/`VARIACION_TASA`/`PLAZO`),
`DEPOSITO_RENOVACION` (origen→destino de cada renovación),
`ITEMPLAZO_TASA`/`ITEMPLAZO_TASA_DETALLE` (el tablero de tasas oficial,
por rango de saldo/plazo/persona natural o jurídica),
`APROBACION_USUARIOTASAS` (rango de variación de tasa permitido por rol —
el que encontramos en 0,00-0,00 para el asesor).
**Aprendizaje ya documentado**: la renovación automática debe tomar la tasa
*vigente al momento de renovar*, no una versión vieja del tablero — y el
campo `TIEMPOCAMBIOTASAS` (días de gracia para aplicar un cambio de tasa)
es la pieza de diseño correcta para esto, que el core propio debería
implementar de forma explícita y con pruebas, no como lo encontramos hoy
(aplicando una tasa de un segmento de cliente equivocado).

### 3.2 Crédito y Colocación (`CREDITO`, `COLOCACION`) — grupo CUC 14

Esquema más grande de los mapeados hoy: **257 tablas** entre los dos.
División de responsabilidad real en Softbank (útil para el diseño propio):
**`CREDITO`** es *originación* (antes de desembolsar) y **`COLOCACION`** es
*el préstamo ya vivo* (después de desembolsado).

**`CREDITO` (originación)** — núcleo:
| Tabla | Filas | Rol |
|---|---|---|
| `SOLICITUD_PRESTAMO` | 4.170 | La solicitud, antes de aprobarse |
| `SOLICITUD_PRESTAMO_TABLA_AMORTIZACION` | 117.041 | Tabla de amortización propuesta |
| `SOLICITUD_PRESTAMO_CALIFICACION` + `_TIPO_ITEM` + `_ITEMCALIFICACION` | 3.372 / 20.232 / 133.624 | El scoring/análisis crediticio — motor de reglas configurable (`ITEMCALIFICACION`, `PLANTILLA_CALIFICACION`) |
| `COMITE_CREDITO_MAESTRO` | 199 | Aprobación colegiada — gobierno corporativo exigido por la norma |
| `TIPO_PRESTAMO` | 60 | Catálogo de productos de crédito |
| `TIPO_PRESTAMO_ITEMAHORRO_AFECTACUENTA` | 11 | La tabla que encontramos hoy — vincula tipo de préstamo con el ítem de ahorro que puede auto-debitarlo |
| `TABLA_AMORTIZACION_PRESUNTIVA` | 1.636.735 | Simulaciones de tabla de amortización (calculadora, antes de la solicitud real) |
| `CALIFICACION_CONTABLE_SEGMENTO` | 53 | La cadena de calificación de riesgo documentada en CLAUDE.md — 28 segmentos, unión por `(CODIGOCALIFICACIONCONTABLE, CODIGODESTINO_FINANCIERO)`, nunca por `ID` |

**`COLOCACION` (el préstamo ya vivo)** — núcleo:
| Tabla | Filas | Rol |
|---|---|---|
| `PRESTAMO` | 4.982 | El préstamo — **temporal nativa** |
| `PRESTAMO_CONSOLIDADO` | 4.982 | Estado consolidado (días de mora actual) — **temporal nativa**, con 1,88 millones de filas de historial |
| `PRESTAMO_RUBRO` | 490.237 | Capital/interés/mora/seguro por cuota — **temporal nativa**, con 9,8 millones de filas de historial (la que usamos para reconstruir el incidente SPI minuto a minuto) |
| `PRESTAMO_CLIENTE` | 4.982 | Bridge préstamo↔socio (deudor + codeudores, `ESPRINCIPAL`) |
| `PRESTAMO_CALIFICACION` | 1.789.192 | Histórico de corridas de calificación de riesgo (no el estado actual — para eso es `PRESTAMO_CONSOLIDADO.DIASMORAACTUAL`, ya documentado en CLAUDE.md) |
| `CLASIFICACION_CARTERA` | 441 | VIGENTE/VENCIDO/NDI — **temporal nativa** |
| `TIPO_VENCIMIENTO` | 3 | Los tres baldes de la Regla #10 |
| `RANGO_TASAMORA` | 112 | Tasas de mora por rango |
| `RUBRO` | 46 | Catálogo de rubros (capital=1, interés, mora, seguro...) |

**Grounding regulatorio**: la escalera de calificación (A-1 a E, con rangos
de mora y % de provisión) documentada en CLAUDE.md corresponde a la **Norma
para la Gestión del Riesgo de Crédito** de la Junta de Política y
Regulación Monetaria y Financiera (JPRF) — es la misma norma la que exige
que el `CALIFICACION_CONTABLE_SEGMENTO` tenga esos 28 segmentos con escala
estricta idéntica, no una decisión de diseño de Softbank.

---

## Nivel 4 — Cobranzas y Cumplimiento/Prevención de Lavado de Activos

### 4.1 Cobranzas (`COBRANZA`) — depende de que exista Crédito/Colocación

56 tablas. Núcleo: `GESTION_PRESTAMO_COBRANZA` (10.128, cada gestión de
cobro registrada), `PERIODO_MORA`/`PERIODO_PREVENTIVO` (los tramos de días
de mora con su `ACCION_GESTION` correspondiente — coincide con lo que vimos
en el manual de Cobranzas: Gestión Preventiva → Gestión Cobranza → Comité
Mora I → Comité Mora II/III → Judicial), `COMITE_MORA`/`COMITE_DEMANDA`
(gobierno del proceso de mora), `HONORARIOS_COBRANZA` (honorarios de
abogados externos), `ABOGADO`/`PROCESO_JUDICIAL_ETAPA` (vía judicial).

### 4.2 Cumplimiento y prevención de lavado de activos (`CUMPLIMIENTO`, `LAVADOACTIVOS`)

Hallazgo real: el esquema **`CUMPLIMIENTO`** está casi vacío (5 tablas, solo
`HALLAZGO`/`HALLAZGO_ETAPA`, 6-12 filas — parece un módulo de seguimiento de
hallazgos de auditoría interna, poco usado). **El motor real de
cumplimiento/PLA vive en `LAVADOACTIVOS`** (42 tablas) y se apoya en las
tablas de listas de control de `SUJETO` que ya vimos en el Nivel 0:

| Tabla | Filas | Rol |
|---|---|---|
| `LAVADOACTIVOS.CALIFICACIONCLIENTE_TRANSACCION` | 6.322.407 | Scoring de riesgo LA/FT por transacción — la tabla más grande del módulo |
| `LAVADOACTIVOS.CALIFICACIONCLIENTEDETALLE` | 4.041.528 | Detalle del cálculo de la calificación |
| `LAVADOACTIVOS.CALIFICACIONCLIENTE` / `_RESUMEN` | 336.794 / 390.061 | Calificación de riesgo por cliente (no por transacción) |
| `SUJETO.LISTACONTROL_DETALLE_PERSONA_PEPS` | 445.066 | Screening de personas expuestas políticamente |
| `SUJETO.LISTACONTROL_DETALLE_SENTENCIADO` | 988.626 | Screening contra sentenciados/listas de control |
| `LAVADOACTIVOS.ALERTA_TRANSACCIONAL` | 2 | Alertas generadas (motor configurado pero con muy poca actividad real registrada) |

**Grounding regulatorio**: este módulo responde a la **Ley Orgánica de
Prevención, Detección y Erradicación del Delito de Lavado de Activos y del
Financiamiento de Delitos** y a la obligación de reportar a la **UAF**
(Unidad de Análisis Financiero) — es la razón de ser de campos que vimos en
Nivel 0 como `SUJETO.PERSONA_NATURAL.ESPEPS` y de la sección "Anexo 2"
(`FINANCIERO.ANEXO2`, 4.973 filas) que apareció varias veces hoy sin que
profundizáramos en ella — es, justamente, el anexo regulatorio de
notificación de operaciones a la UAF.

---

## Inventario completo — los 40 módulos (esquemas) de Softbank

Confirmado por consulta directa: **exactamente 40 esquemas**. Los 13 de
arriba (Niveles 0-4) son la columna vertebral. Los 27 restantes se mapean
acá, agrupados por dependencia, para que "poco a poco" cubra **todo**, no
solo lo obvio.

## Nivel 5 — Operación de caja (depende de Ahorros + Colocación + Contabilidad)

**`CAJAS`** (114 tablas). Es la capa de ventanilla/bóveda por donde pasa
físicamente cada transacción — el cajero recibe/entrega efectivo, y de ahí
se dispara el movimiento real en Ahorros/Colocación/Contabilidad.

| Tabla | Filas | Rol |
|---|---|---|
| `VENTANILLA_ITEMCAJA_MOVIMIENTOTRANSACCION_EFECTIVO` | 2.222.203 | Cada movimiento de efectivo en ventanilla — la tabla más grande del esquema |
| `VENTANILLA_ITEMCAJA_MOVIMIENTOTRANSACCION` | 171.784 | Transacciones de ventanilla |
| `BOVEDA_ITEMBOVEDA_MOVIMIENTOTRANSACCION_EFECTIVO` | 117.572 | Movimientos de bóveda (la reserva de efectivo detrás de las ventanillas) |
| `VENTANILLA_CUADRE` / `_EFECTIVO` | 3.650 / 42.510 | El cuadre de caja diario por cajero — control operativo obligatorio |
| `SOLICITUD_REVERSO_CAJA` | 133 | Reversos de transacciones de caja — vinculado al `PERMITEREVERSO` que vimos en Nivel 1 |
| `PAGO_EXTERNO_TRANSACCION` | 679 | Pagos de servicios externos (luz, agua, etc.) por ventanilla |
| `DENOMINACION` | 13 | Catálogo de billetes/monedas — control de especies, relevante para el hallazgo `ESPECIEFALSIFICADA` de Nivel 1 |

## Nivel 6 — Nómina propia (depende de Personas + Contabilidad)

**`NOMINA`** (186 tablas). Nómina de los **empleados de la cooperativa**
(no de socios) — sueldos, décimos, fondos de reserva, IESS, vacaciones.
Núcleo: `EMPLEADO` (127), `ROLPAGOS`/`ROLPAGOS_EMPLEADO` (rol de pagos
mensual), `EMPLEADO_DECIMOTERCERO`/`_DECIMOCUARTO` (obligatorios en
Ecuador), `EMPLEADO_FONDOSRESERVA`, `SOLICITUD_ACCIONPERSONAL` (altas,
bajas, cambios de cargo — flujo de RRHH). Es un módulo grande pero
autocontenido: no lo necesita nada de la cadena de negocio financiero, solo
Personas (Nivel 0) y el motor contable (Nivel 1) para generar los asientos
de gasto de personal.

## Nivel 7 — Tesorería y activos internos (dependen de Contabilidad)

Módulos de "casa adentro" — la cooperativa gestionando su propio dinero y
bienes, no el de los socios:

- **`OBLIGACION`** (47 tablas) — deuda que la cooperativa toma con terceros
  (bancos, CONAFIPS). Corresponde al grupo CUC **26 Obligaciones
  financieras**. Núcleo: `OBLIGACION_FINANCIERA` + `_RUBRO` +
  `_TABLAAMORTIZACION` — estructuralmente un espejo de `COLOCACION.PRESTAMO`
  pero para cuando la cooperativa es la deudora, no el socio.
- **`CUENTASPORCOBRAR`** (45 tablas) — cuentas por cobrar/pagar internas
  (`CUENTAPORCOBRAR`, `CUENTAPORPAGAR`, `PAGO_PROCESADO`).
- **`ACTIVOFIJO`** (35 tablas) — activos fijos de la cooperativa
  (`ACTIVO`, depreciación, traslados, bajas). Corresponde al grupo CUC **18
  Propiedades y equipo**.
- **`PROVEEDURIA`** (23 tablas) — compras e inventario de bodega
  (`ARTICULO`, `BODEGA_ARTICULO`, `SOLICITUD_PEDIDO`).
- **`PORTAFOLIO`** (63 tablas) — el portafolio de **inversiones propias**
  de la cooperativa (no las DPF de los socios, que son Nivel 3) — dónde la
  coop coloca su propia liquidez excedente. Núcleo: `INVERSIONPORTAFOLIO`,
  `INVERSION_ITEMINVERSION`, `CALIFICACIONRIESGO` (de la contraparte, no del
  socio). Corresponde al grupo CUC **13 Inversiones**.

## Nivel 8 — Riesgo y reportería regulatoria (dependen de que exista todo lo anterior)

Este nivel **no puede construirse antes que el resto** — por definición,
reporta sobre datos que ya deben existir.

- **`RIESGOLIQUIDEZ`** (76 tablas) y **`RIESGOOPERATIVO`** (62 tablas) —
  gestión de riesgo exigida por la norma. `RIESGOOPERATIVO` en particular
  modela un framework completo de gestión de riesgo operacional
  (`MACROPROCESO` → `PROCESO` → `SUBPROCESO` → `ACTIVIDAD`, con
  `EVENTO`/`TIPOEVENTO`, matriz de `IMPACTO` × `PROBABILIDAD` →
  `NIVELRIESGO`) — un patrón de gestión de riesgo genérico, reutilizable
  para cualquier tipo de riesgo, no solo operativo.
- **`REPORTECONTROL`** (135 tablas) — **el hallazgo más importante de este
  pase**: acá viven, con nombre y apellido, los **reportes regulatorios
  obligatorios reales** que la cooperativa envía a la SEPS y al Banco
  Central: `B13`, `D01`, `BCE01`/`BCE02`, `S01`, `C01` a `C07`, `L01`/`L02`
  (liquidez), `IG01`, `TIN` (tasas), `UAF` (lavado de activos), `RFD`
  (fondo de liquidez), `ROTEF`, `R01` a `R22`, `CRS` (intercambio de
  información fiscal internacional). Cada uno tiene su tabla `CABECERA_*` y
  `DETALLE_*`. **Este esquema es, en la práctica, el índice de todos los
  formularios regulatorios que un core financiero real tiene que poder
  producir** — vale la pena, cuando se llegue a este nivel, tomar esta
  lista de códigos y buscar cada uno en el sitio de la SEPS/BCE para saber
  exactamente qué exige cada reporte, en vez de inferirlo del nombre de la
  tabla.

## Periféricos (no bloquean nada, se construyen cuando se necesiten)

Módulos chicos, cada uno independiente de los demás:

`PLANIFICACION` (11 tablas, planificación estratégica/metas) ·
`MARKETING` (16, rifas/promociones a socios) ·
`ENLINEA` (16, banca en línea — sesiones y transacciones de clientes vía
web/app) · `CALLCENTER` (3, mínimo) · `COACTIVA` (6, cobro coactivo/judicial
— apéndice de Cobranzas) · `AUDITORIA` (19, auditoría interna — gestión de
hallazgos, ya lo vimos hoy) · `HERRAMIENTARURAL` (13, calculadora de flujo
de caja agrícola para crédito rural — apéndice de Crédito) · `SBK_MOVIL`
(7, configuración de la app móvil) · `CS_CAT` (27) y `CS_CE` (12) —
facturación electrónica ante el **SRI** (Servicio de Rentas Internas,
catálogos y comprobantes electrónicos — obligación tributaria, no
financiera) · `CC_SW` (13) — control de billetes de alta denominación y
licitud de documentos, apéndice de `LAVADOACTIVOS`.

## Infraestructura técnica (no son módulos de negocio — no diseñar, solo saberlo)

`MIGRACION` (148 tablas) — staging de la migración de datos desde el core
anterior (`15DEAGOSTO*`, `FACES_*`, `MIGRACION*`) — es historia de una
migración pasada, no lógica de negocio viva. `HangFire` (11) y
`SEGUNDOPLANO` (11) — colas de trabajos en segundo plano (dos instancias
del mismo motor, `Hangfire.SqlServer`). `FLUJOTRABAJO` (8) — motor de
aprobación genérico por etapas (lo usa `CONTABILIDAD.GRUPO_CONTABLE`, entre
otros). `dbo` (8) — misceláneo sin esquema propio, prácticamente vacío.

---

## Cómo seguir

Los 40 esquemas de Softbank quedaron todos identificados y clasificados por
nivel de dependencia; los 13 del núcleo (Niveles 0-4) están documentados a
fondo, con tablas núcleo, filas reales y grounding regulatorio. Los 27
restantes (Niveles 5-8 + periféricos + infraestructura) están mapeados a
nivel de inventario (qué tablas existen, cuántas filas, para qué sirven a
grandes rasgos) — cuando llegue el turno de diseñar cada uno, se profundiza
igual que se hizo con los primeros cinco: tablas núcleo columna por columna,
y norma oficial contrastada cuando exista (particularmente `REPORTECONTROL`,
que amerita ir formulario por formulario contra la web de la SEPS/BCE
cuando se llegue a Nivel 8).

- [x] Nivel 0 — Personas, Seguridad, Catálogos Generales
- [x] Nivel 1 — Motor contable (CUC de la SEPS + `CONTABILIDAD`/`FINANCIERO`)
- [x] Nivel 2 — Ahorros (`AHORROS`, grupo CUC 21)
- [x] Nivel 3 — Plazo Fijo (`INVERSION`) y Crédito/Colocación (`CREDITO`/`COLOCACION`, grupo CUC 14, Norma JPRF de riesgo de crédito)
- [x] Nivel 4 — Cobranzas (`COBRANZA`) y Cumplimiento/PLA (`LAVADOACTIVOS`, Ley de Prevención de Lavado de Activos, UAF)
- [x] Nivel 5 — Caja/Bóveda (`CAJAS`) — inventario, falta detalle de columnas
- [x] Nivel 6 — Nómina propia (`NOMINA`) — inventario, falta detalle de columnas
- [x] Nivel 7 — Tesorería y activos internos (`OBLIGACION`, `CUENTASPORCOBRAR`, `ACTIVOFIJO`, `PROVEEDURIA`, `PORTAFOLIO`) — inventario, falta detalle de columnas
- [x] Nivel 8 — Riesgo y reportería regulatoria (`RIESGOLIQUIDEZ`, `RIESGOOPERATIVO`, `REPORTECONTROL`) — inventario, falta el cruce formulario por formulario contra SEPS/BCE
- [x] Periféricos y infraestructura técnica — inventariados, sin necesidad de más detalle salvo que se decida construir alguno

No modificar este documento sin volver a verificar contra la base — es un
mapa de lo que existe, no un diseño aspiracional.
