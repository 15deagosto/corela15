using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Nivel3_PlazoFijoYCredito : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "colocacion");

            migrationBuilder.EnsureSchema(
                name: "inversion");

            migrationBuilder.EnsureSchema(
                name: "credito");

            migrationBuilder.CreateTable(
                name: "deposito",
                schema: "inversion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    id_agencia = table.Column<int>(type: "integer", nullable: false),
                    monto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    tasa = table.Column<decimal>(type: "numeric(9,4)", nullable: false),
                    variacion_tasa = table.Column<decimal>(type: "numeric(9,4)", nullable: false),
                    plazo_dias = table.Column<int>(type: "integer", nullable: false),
                    pago_periodico_interes = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_creacion = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_vencimiento = table.Column<DateOnly>(type: "date", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    modificado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    modificado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_deposito", x => x.id);
                    table.ForeignKey(
                        name: "fk_deposito_agencia_id_agencia",
                        column: x => x.id_agencia,
                        principalSchema: "general",
                        principalTable: "agencia",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "item_plazo_tasa",
                schema: "inversion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    plazo_dias_min = table.Column<int>(type: "integer", nullable: false),
                    plazo_dias_max = table.Column<int>(type: "integer", nullable: false),
                    monto_min = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    monto_max = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    tipo_persona = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    tasa = table.Column<decimal>(type: "numeric(9,4)", nullable: false),
                    fecha_vigencia_desde = table.Column<DateOnly>(type: "date", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_item_plazo_tasa", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rubro",
                schema: "colocacion",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    es_cuenta_por_cobrar = table.Column<bool>(type: "boolean", nullable: false),
                    orden_de_cobro = table.Column<int>(type: "integer", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rubro", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tipo_prestamo",
                schema: "credito",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    monto_minimo = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    monto_maximo = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    plazo_minimo_dias = table.Column<int>(type: "integer", nullable: false),
                    plazo_maximo_dias = table.Column<int>(type: "integer", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipo_prestamo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tipo_vencimiento",
                schema: "colocacion",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    es_vigente = table.Column<bool>(type: "boolean", nullable: false),
                    es_no_devenga_interes = table.Column<bool>(type: "boolean", nullable: false),
                    es_vencido = table.Column<bool>(type: "boolean", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipo_vencimiento", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "deposito_cliente",
                schema: "inversion",
                columns: table => new
                {
                    id_deposito = table.Column<Guid>(type: "uuid", nullable: false),
                    id_cliente = table.Column<Guid>(type: "uuid", nullable: false),
                    principal = table.Column<bool>(type: "boolean", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_deposito_cliente", x => new { x.id_deposito, x.id_cliente });
                    table.ForeignKey(
                        name: "fk_deposito_cliente_cliente_id_cliente",
                        column: x => x.id_cliente,
                        principalSchema: "clientes",
                        principalTable: "cliente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_deposito_cliente_depositos_id_deposito",
                        column: x => x.id_deposito,
                        principalSchema: "inversion",
                        principalTable: "deposito",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "deposito_renovacion",
                schema: "inversion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_deposito_origen = table.Column<Guid>(type: "uuid", nullable: false),
                    id_deposito_destino = table.Column<Guid>(type: "uuid", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    valor_incremento = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    fecha_renovacion = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_deposito_renovacion", x => x.id);
                    table.ForeignKey(
                        name: "fk_deposito_renovacion_deposito_id_deposito_destino",
                        column: x => x.id_deposito_destino,
                        principalSchema: "inversion",
                        principalTable: "deposito",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_deposito_renovacion_deposito_id_deposito_origen",
                        column: x => x.id_deposito_origen,
                        principalSchema: "inversion",
                        principalTable: "deposito",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "prestamo",
                schema: "colocacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    id_tipo_prestamo = table.Column<int>(type: "integer", nullable: false),
                    id_agencia = table.Column<int>(type: "integer", nullable: false),
                    cuotas = table.Column<int>(type: "integer", nullable: false),
                    deuda_inicial = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    saldo = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    tasa = table.Column<decimal>(type: "numeric(9,4)", nullable: false),
                    tea = table.Column<decimal>(type: "numeric(9,4)", nullable: false),
                    fecha_adjudicacion = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_vencimiento = table.Column<DateOnly>(type: "date", nullable: false),
                    debito_spi = table.Column<bool>(type: "boolean", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    modificado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    modificado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_prestamo", x => x.id);
                    table.ForeignKey(
                        name: "fk_prestamo_agencia_id_agencia",
                        column: x => x.id_agencia,
                        principalSchema: "general",
                        principalTable: "agencia",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_prestamo_tipos_prestamo_id_tipo_prestamo",
                        column: x => x.id_tipo_prestamo,
                        principalSchema: "credito",
                        principalTable: "tipo_prestamo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "solicitud_prestamo",
                schema: "credito",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    id_cliente = table.Column<Guid>(type: "uuid", nullable: false),
                    id_tipo_prestamo = table.Column<int>(type: "integer", nullable: false),
                    id_agencia = table.Column<int>(type: "integer", nullable: false),
                    monto_solicitado = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    monto_aprobado = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    cuotas = table.Column<int>(type: "integer", nullable: false),
                    fecha_solicitud = table.Column<DateOnly>(type: "date", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    modificado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    modificado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_solicitud_prestamo", x => x.id);
                    table.ForeignKey(
                        name: "fk_solicitud_prestamo_agencia_id_agencia",
                        column: x => x.id_agencia,
                        principalSchema: "general",
                        principalTable: "agencia",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_solicitud_prestamo_cliente_id_cliente",
                        column: x => x.id_cliente,
                        principalSchema: "clientes",
                        principalTable: "cliente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_solicitud_prestamo_tipos_prestamo_id_tipo_prestamo",
                        column: x => x.id_tipo_prestamo,
                        principalSchema: "credito",
                        principalTable: "tipo_prestamo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "clasificacion_cartera",
                schema: "colocacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    dias_inicio = table.Column<int>(type: "integer", nullable: false),
                    dias_fin = table.Column<int>(type: "integer", nullable: false),
                    id_cuenta_contable = table.Column<Guid>(type: "uuid", nullable: false),
                    id_tipo_vencimiento = table.Column<int>(type: "integer", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_clasificacion_cartera", x => x.id);
                    table.CheckConstraint("ck_clasificacion_cartera_rango", "dias_fin >= dias_inicio");
                    table.ForeignKey(
                        name: "fk_clasificacion_cartera_cuentas_contables_id_cuenta_contable",
                        column: x => x.id_cuenta_contable,
                        principalSchema: "contabilidad",
                        principalTable: "cuenta_contable",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_clasificacion_cartera_tipos_vencimiento_id_tipo_vencimiento",
                        column: x => x.id_tipo_vencimiento,
                        principalSchema: "colocacion",
                        principalTable: "tipo_vencimiento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "prestamo_cliente",
                schema: "colocacion",
                columns: table => new
                {
                    id_prestamo = table.Column<Guid>(type: "uuid", nullable: false),
                    id_cliente = table.Column<Guid>(type: "uuid", nullable: false),
                    principal = table.Column<bool>(type: "boolean", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_prestamo_cliente", x => new { x.id_prestamo, x.id_cliente });
                    table.ForeignKey(
                        name: "fk_prestamo_cliente_cliente_id_cliente",
                        column: x => x.id_cliente,
                        principalSchema: "clientes",
                        principalTable: "cliente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_prestamo_cliente_prestamos_id_prestamo",
                        column: x => x.id_prestamo,
                        principalSchema: "colocacion",
                        principalTable: "prestamo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prestamo_rubro",
                schema: "colocacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_prestamo = table.Column<Guid>(type: "uuid", nullable: false),
                    id_rubro = table.Column<int>(type: "integer", nullable: false),
                    numero_cuota = table.Column<int>(type: "integer", nullable: false),
                    fecha_inicio = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_fin = table.Column<DateOnly>(type: "date", nullable: false),
                    proyectado = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    calculado = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    cobrado = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_prestamo_rubro", x => x.id);
                    table.ForeignKey(
                        name: "fk_prestamo_rubro_prestamo_id_prestamo",
                        column: x => x.id_prestamo,
                        principalSchema: "colocacion",
                        principalTable: "prestamo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_prestamo_rubro_rubros_id_rubro",
                        column: x => x.id_rubro,
                        principalSchema: "colocacion",
                        principalTable: "rubro",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_clasificacion_cartera_id_cuenta_contable",
                schema: "colocacion",
                table: "clasificacion_cartera",
                column: "id_cuenta_contable");

            migrationBuilder.CreateIndex(
                name: "ix_clasificacion_cartera_id_tipo_vencimiento",
                schema: "colocacion",
                table: "clasificacion_cartera",
                column: "id_tipo_vencimiento");

            migrationBuilder.CreateIndex(
                name: "ix_deposito_codigo",
                schema: "inversion",
                table: "deposito",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_deposito_id_agencia",
                schema: "inversion",
                table: "deposito",
                column: "id_agencia");

            migrationBuilder.CreateIndex(
                name: "ix_deposito_cliente_id_cliente",
                schema: "inversion",
                table: "deposito_cliente",
                column: "id_cliente");

            migrationBuilder.CreateIndex(
                name: "ix_deposito_renovacion_id_deposito_destino",
                schema: "inversion",
                table: "deposito_renovacion",
                column: "id_deposito_destino");

            migrationBuilder.CreateIndex(
                name: "ix_deposito_renovacion_id_deposito_origen",
                schema: "inversion",
                table: "deposito_renovacion",
                column: "id_deposito_origen");

            migrationBuilder.CreateIndex(
                name: "ix_prestamo_id_agencia",
                schema: "colocacion",
                table: "prestamo",
                column: "id_agencia");

            migrationBuilder.CreateIndex(
                name: "ix_prestamo_id_tipo_prestamo",
                schema: "colocacion",
                table: "prestamo",
                column: "id_tipo_prestamo");

            migrationBuilder.CreateIndex(
                name: "ix_prestamo_numero",
                schema: "colocacion",
                table: "prestamo",
                column: "numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_prestamo_cliente_id_cliente",
                schema: "colocacion",
                table: "prestamo_cliente",
                column: "id_cliente");

            migrationBuilder.CreateIndex(
                name: "ix_prestamo_rubro_id_prestamo_numero_cuota_id_rubro",
                schema: "colocacion",
                table: "prestamo_rubro",
                columns: new[] { "id_prestamo", "numero_cuota", "id_rubro" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_prestamo_rubro_id_rubro",
                schema: "colocacion",
                table: "prestamo_rubro",
                column: "id_rubro");

            migrationBuilder.CreateIndex(
                name: "ix_rubro_codigo",
                schema: "colocacion",
                table: "rubro",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_solicitud_prestamo_id_agencia",
                schema: "credito",
                table: "solicitud_prestamo",
                column: "id_agencia");

            migrationBuilder.CreateIndex(
                name: "ix_solicitud_prestamo_id_cliente",
                schema: "credito",
                table: "solicitud_prestamo",
                column: "id_cliente");

            migrationBuilder.CreateIndex(
                name: "ix_solicitud_prestamo_id_tipo_prestamo",
                schema: "credito",
                table: "solicitud_prestamo",
                column: "id_tipo_prestamo");

            migrationBuilder.CreateIndex(
                name: "ix_solicitud_prestamo_numero",
                schema: "credito",
                table: "solicitud_prestamo",
                column: "numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tipo_prestamo_codigo",
                schema: "credito",
                table: "tipo_prestamo",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tipo_vencimiento_codigo",
                schema: "colocacion",
                table: "tipo_vencimiento",
                column: "codigo",
                unique: true);

            // --- Versionado real (mismo patrón de Nivel1/Nivel2) ---
            // Confirmado contra la base real: DEPOSITO y PRESTAMO tienen columnas
            // de sistema de temporal table en Softbank (ya versionan de verdad).
            // PRESTAMO_RUBRO también. Se replica con trigger+historico.
            migrationBuilder.Sql(
                """
                CREATE TABLE inversion.deposito_historico (
                    historico_id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                    operacion varchar(10) NOT NULL, modificado_en timestamptz NOT NULL DEFAULT now(),
                    id uuid NOT NULL, codigo varchar(20) NOT NULL, id_agencia integer NOT NULL,
                    monto numeric(18,2) NOT NULL, tasa numeric(9,4) NOT NULL, variacion_tasa numeric(9,4) NOT NULL,
                    plazo_dias integer NOT NULL, pago_periodico_interes boolean NOT NULL,
                    fecha_creacion date NOT NULL, fecha_vencimiento date NOT NULL, estado varchar(20) NOT NULL,
                    creado_en timestamptz NOT NULL, creado_por varchar(100) NOT NULL, modificado_por varchar(100)
                );
                CREATE INDEX ix_deposito_historico_id ON inversion.deposito_historico (id);
                CREATE FUNCTION inversion.fn_versionar_deposito() RETURNS trigger AS $$
                BEGIN
                    IF (TG_OP = 'DELETE') THEN
                        INSERT INTO inversion.deposito_historico (operacion, id, codigo, id_agencia, monto, tasa,
                            variacion_tasa, plazo_dias, pago_periodico_interes, fecha_creacion, fecha_vencimiento,
                            estado, creado_en, creado_por, modificado_por)
                        VALUES ('DELETE', OLD.id, OLD.codigo, OLD.id_agencia, OLD.monto, OLD.tasa, OLD.variacion_tasa,
                            OLD.plazo_dias, OLD.pago_periodico_interes, OLD.fecha_creacion, OLD.fecha_vencimiento,
                            OLD.estado, OLD.creado_en, OLD.creado_por, OLD.modificado_por);
                        RETURN OLD;
                    ELSE
                        INSERT INTO inversion.deposito_historico (operacion, id, codigo, id_agencia, monto, tasa,
                            variacion_tasa, plazo_dias, pago_periodico_interes, fecha_creacion, fecha_vencimiento,
                            estado, creado_en, creado_por, modificado_por)
                        VALUES (TG_OP, NEW.id, NEW.codigo, NEW.id_agencia, NEW.monto, NEW.tasa, NEW.variacion_tasa,
                            NEW.plazo_dias, NEW.pago_periodico_interes, NEW.fecha_creacion, NEW.fecha_vencimiento,
                            NEW.estado, NEW.creado_en, NEW.creado_por, NEW.modificado_por);
                        RETURN NEW;
                    END IF;
                END;
                $$ LANGUAGE plpgsql;
                CREATE TRIGGER trg_versionar_deposito AFTER INSERT OR UPDATE OR DELETE ON inversion.deposito
                    FOR EACH ROW EXECUTE FUNCTION inversion.fn_versionar_deposito();

                CREATE TABLE colocacion.prestamo_historico (
                    historico_id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                    operacion varchar(10) NOT NULL, modificado_en timestamptz NOT NULL DEFAULT now(),
                    id uuid NOT NULL, numero varchar(20) NOT NULL, id_tipo_prestamo integer NOT NULL,
                    id_agencia integer NOT NULL, cuotas integer NOT NULL, deuda_inicial numeric(18,2) NOT NULL,
                    saldo numeric(18,2) NOT NULL, tasa numeric(9,4) NOT NULL, tea numeric(9,4) NOT NULL,
                    fecha_adjudicacion date NOT NULL, fecha_vencimiento date NOT NULL, debito_spi boolean NOT NULL,
                    estado varchar(20) NOT NULL, creado_en timestamptz NOT NULL, creado_por varchar(100) NOT NULL,
                    modificado_por varchar(100)
                );
                CREATE INDEX ix_prestamo_historico_id ON colocacion.prestamo_historico (id);
                CREATE FUNCTION colocacion.fn_versionar_prestamo() RETURNS trigger AS $$
                BEGIN
                    IF (TG_OP = 'DELETE') THEN
                        INSERT INTO colocacion.prestamo_historico (operacion, id, numero, id_tipo_prestamo, id_agencia,
                            cuotas, deuda_inicial, saldo, tasa, tea, fecha_adjudicacion, fecha_vencimiento, debito_spi,
                            estado, creado_en, creado_por, modificado_por)
                        VALUES ('DELETE', OLD.id, OLD.numero, OLD.id_tipo_prestamo, OLD.id_agencia, OLD.cuotas,
                            OLD.deuda_inicial, OLD.saldo, OLD.tasa, OLD.tea, OLD.fecha_adjudicacion,
                            OLD.fecha_vencimiento, OLD.debito_spi, OLD.estado, OLD.creado_en, OLD.creado_por, OLD.modificado_por);
                        RETURN OLD;
                    ELSE
                        INSERT INTO colocacion.prestamo_historico (operacion, id, numero, id_tipo_prestamo, id_agencia,
                            cuotas, deuda_inicial, saldo, tasa, tea, fecha_adjudicacion, fecha_vencimiento, debito_spi,
                            estado, creado_en, creado_por, modificado_por)
                        VALUES (TG_OP, NEW.id, NEW.numero, NEW.id_tipo_prestamo, NEW.id_agencia, NEW.cuotas,
                            NEW.deuda_inicial, NEW.saldo, NEW.tasa, NEW.tea, NEW.fecha_adjudicacion,
                            NEW.fecha_vencimiento, NEW.debito_spi, NEW.estado, NEW.creado_en, NEW.creado_por, NEW.modificado_por);
                        RETURN NEW;
                    END IF;
                END;
                $$ LANGUAGE plpgsql;
                CREATE TRIGGER trg_versionar_prestamo AFTER INSERT OR UPDATE OR DELETE ON colocacion.prestamo
                    FOR EACH ROW EXECUTE FUNCTION colocacion.fn_versionar_prestamo();

                CREATE TABLE colocacion.prestamo_rubro_historico (
                    historico_id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                    operacion varchar(10) NOT NULL, modificado_en timestamptz NOT NULL DEFAULT now(),
                    id uuid NOT NULL, id_prestamo uuid NOT NULL, id_rubro integer NOT NULL, numero_cuota integer NOT NULL,
                    fecha_inicio date NOT NULL, fecha_fin date NOT NULL, proyectado numeric(18,2) NOT NULL,
                    calculado numeric(18,2) NOT NULL, cobrado numeric(18,2) NOT NULL, estado varchar(20) NOT NULL
                );
                CREATE INDEX ix_prestamo_rubro_historico_id ON colocacion.prestamo_rubro_historico (id);
                CREATE FUNCTION colocacion.fn_versionar_prestamo_rubro() RETURNS trigger AS $$
                BEGIN
                    IF (TG_OP = 'DELETE') THEN
                        INSERT INTO colocacion.prestamo_rubro_historico (operacion, id, id_prestamo, id_rubro,
                            numero_cuota, fecha_inicio, fecha_fin, proyectado, calculado, cobrado, estado)
                        VALUES ('DELETE', OLD.id, OLD.id_prestamo, OLD.id_rubro, OLD.numero_cuota, OLD.fecha_inicio,
                            OLD.fecha_fin, OLD.proyectado, OLD.calculado, OLD.cobrado, OLD.estado);
                        RETURN OLD;
                    ELSE
                        INSERT INTO colocacion.prestamo_rubro_historico (operacion, id, id_prestamo, id_rubro,
                            numero_cuota, fecha_inicio, fecha_fin, proyectado, calculado, cobrado, estado)
                        VALUES (TG_OP, NEW.id, NEW.id_prestamo, NEW.id_rubro, NEW.numero_cuota, NEW.fecha_inicio,
                            NEW.fecha_fin, NEW.proyectado, NEW.calculado, NEW.cobrado, NEW.estado);
                        RETURN NEW;
                    END IF;
                END;
                $$ LANGUAGE plpgsql;
                CREATE TRIGGER trg_versionar_prestamo_rubro AFTER INSERT OR UPDATE OR DELETE ON colocacion.prestamo_rubro
                    FOR EACH ROW EXECUTE FUNCTION colocacion.fn_versionar_prestamo_rubro();
                """);

            // --- Seed de catálogos ---
            migrationBuilder.Sql(
                """
                INSERT INTO credito.tipo_prestamo (codigo, nombre, monto_minimo, monto_maximo, plazo_minimo_dias, plazo_maximo_dias, activo) VALUES
                    ('CONS', 'Consumo',    300.00, 20000.00, 90,  1800, true),
                    ('MICRO','Microcrédito', 200.00, 30000.00, 90,  1800, true),
                    ('PROD', 'Productivo', 1000.00, 100000.00, 180, 2555, true);

                INSERT INTO colocacion.rubro (codigo, nombre, es_cuenta_por_cobrar, orden_de_cobro, activo) VALUES
                    ('CAP',  'Capital',  false, 1, true),
                    ('INT',  'Interés',  false, 2, true),
                    ('MORA', 'Mora',     false, 3, true),
                    ('SEG',  'Seguro',   true,  4, true);

                INSERT INTO colocacion.tipo_vencimiento (codigo, nombre, es_vigente, es_no_devenga_interes, es_vencido, activo) VALUES
                    ('VIG', 'Por vencer',            true,  false, false, true),
                    ('NDI', 'No devenga interés',    false, true,  false, true),
                    ('VEN', 'Vencido',               false, false, true,  true);

                -- Subcuentas de detalle del grupo 14 (Cartera de créditos) necesarias
                -- para que clasificacion_cartera tenga a qué apuntar — cartera
                -- productiva, los tres baldes de la Regla #10 (ver CLAUDE.md).
                INSERT INTO contabilidad.cuenta_contable (id, codigo, nombre, grupo, naturaleza, id_cuenta_padre, es_mayor, activa, creado_en, creado_por)
                SELECT gen_random_uuid(), d.codigo, d.nombre, 'Activo', 'Deudora', p.id, true, true, now(), 'seed:migracion'
                FROM (VALUES
                    ('1401', 'Cartera de crédito productivo por vencer'),
                    ('1425', 'Cartera de crédito productivo que no devenga intereses'),
                    ('1449', 'Cartera de crédito productivo vencida')
                ) AS d(codigo, nombre)
                JOIN contabilidad.cuenta_contable p ON p.codigo = '14';

                INSERT INTO colocacion.clasificacion_cartera (id, dias_inicio, dias_fin, id_cuenta_contable, id_tipo_vencimiento, activo)
                SELECT gen_random_uuid(), r.dias_inicio, r.dias_fin, cc.id, tv.id, true
                FROM (VALUES
                    (0, 0, '1401', 'VIG'),
                    (1, 30, '1425', 'NDI'),
                    (31, 999999, '1449', 'VEN')
                ) AS r(dias_inicio, dias_fin, codigo_cuenta, codigo_tipo_vencimiento)
                JOIN contabilidad.cuenta_contable cc ON cc.codigo = r.codigo_cuenta
                JOIN colocacion.tipo_vencimiento tv ON tv.codigo = r.codigo_tipo_vencimiento;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DROP TRIGGER IF EXISTS trg_versionar_prestamo_rubro ON colocacion.prestamo_rubro;
                DROP FUNCTION IF EXISTS colocacion.fn_versionar_prestamo_rubro();
                DROP TABLE IF EXISTS colocacion.prestamo_rubro_historico;
                DROP TRIGGER IF EXISTS trg_versionar_prestamo ON colocacion.prestamo;
                DROP FUNCTION IF EXISTS colocacion.fn_versionar_prestamo();
                DROP TABLE IF EXISTS colocacion.prestamo_historico;
                DROP TRIGGER IF EXISTS trg_versionar_deposito ON inversion.deposito;
                DROP FUNCTION IF EXISTS inversion.fn_versionar_deposito();
                DROP TABLE IF EXISTS inversion.deposito_historico;
                """);

            migrationBuilder.DropTable(
                name: "clasificacion_cartera",
                schema: "colocacion");

            migrationBuilder.DropTable(
                name: "deposito_cliente",
                schema: "inversion");

            migrationBuilder.DropTable(
                name: "deposito_renovacion",
                schema: "inversion");

            migrationBuilder.DropTable(
                name: "item_plazo_tasa",
                schema: "inversion");

            migrationBuilder.DropTable(
                name: "prestamo_cliente",
                schema: "colocacion");

            migrationBuilder.DropTable(
                name: "prestamo_rubro",
                schema: "colocacion");

            migrationBuilder.DropTable(
                name: "solicitud_prestamo",
                schema: "credito");

            migrationBuilder.DropTable(
                name: "tipo_vencimiento",
                schema: "colocacion");

            migrationBuilder.DropTable(
                name: "deposito",
                schema: "inversion");

            migrationBuilder.DropTable(
                name: "prestamo",
                schema: "colocacion");

            migrationBuilder.DropTable(
                name: "rubro",
                schema: "colocacion");

            migrationBuilder.DropTable(
                name: "tipo_prestamo",
                schema: "credito");
        }
    }
}
