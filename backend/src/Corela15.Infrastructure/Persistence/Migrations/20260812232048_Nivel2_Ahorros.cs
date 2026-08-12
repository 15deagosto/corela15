using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Nivel2_Ahorros : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ahorros");

            migrationBuilder.CreateTable(
                name: "item_saldo",
                schema: "ahorros",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_item_saldo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tipo_cuenta",
                schema: "ahorros",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    permite_debito_prestamo = table.Column<bool>(type: "boolean", nullable: false),
                    saldo_minimo_con_prestamo = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipo_cuenta", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cuenta",
                schema: "ahorros",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    id_tipo_cuenta = table.Column<int>(type: "integer", nullable: false),
                    id_agencia = table.Column<int>(type: "integer", nullable: false),
                    fecha_apertura = table.Column<DateOnly>(type: "date", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    modificado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    modificado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cuenta", x => x.id);
                    table.ForeignKey(
                        name: "fk_cuenta_agencia_id_agencia",
                        column: x => x.id_agencia,
                        principalSchema: "general",
                        principalTable: "agencia",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_cuenta_tipos_cuenta_id_tipo_cuenta",
                        column: x => x.id_tipo_cuenta,
                        principalSchema: "ahorros",
                        principalTable: "tipo_cuenta",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tipo_cuenta_item_saldo",
                schema: "ahorros",
                columns: table => new
                {
                    id_tipo_cuenta = table.Column<int>(type: "integer", nullable: false),
                    id_item_saldo = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipo_cuenta_item_saldo", x => new { x.id_tipo_cuenta, x.id_item_saldo });
                    table.ForeignKey(
                        name: "fk_tipo_cuenta_item_saldo_item_saldo_id_item_saldo",
                        column: x => x.id_item_saldo,
                        principalSchema: "ahorros",
                        principalTable: "item_saldo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_tipo_cuenta_item_saldo_tipo_cuenta_id_tipo_cuenta",
                        column: x => x.id_tipo_cuenta,
                        principalSchema: "ahorros",
                        principalTable: "tipo_cuenta",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cuenta_cliente",
                schema: "ahorros",
                columns: table => new
                {
                    id_cuenta = table.Column<Guid>(type: "uuid", nullable: false),
                    id_cliente = table.Column<Guid>(type: "uuid", nullable: false),
                    principal = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cuenta_cliente", x => new { x.id_cuenta, x.id_cliente });
                    table.ForeignKey(
                        name: "fk_cuenta_cliente_cliente_id_cliente",
                        column: x => x.id_cliente,
                        principalSchema: "clientes",
                        principalTable: "cliente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_cuenta_cliente_cuentas_id_cuenta",
                        column: x => x.id_cuenta,
                        principalSchema: "ahorros",
                        principalTable: "cuenta",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cuenta_item_saldo",
                schema: "ahorros",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_cuenta = table.Column<Guid>(type: "uuid", nullable: false),
                    id_item_saldo = table.Column<int>(type: "integer", nullable: false),
                    saldo = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    acredita_prestamo = table.Column<bool>(type: "boolean", nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    modificado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    modificado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cuenta_item_saldo", x => x.id);
                    table.ForeignKey(
                        name: "fk_cuenta_item_saldo_cuenta_id_cuenta",
                        column: x => x.id_cuenta,
                        principalSchema: "ahorros",
                        principalTable: "cuenta",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_cuenta_item_saldo_items_saldo_id_item_saldo",
                        column: x => x.id_item_saldo,
                        principalSchema: "ahorros",
                        principalTable: "item_saldo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_cuenta_id_agencia",
                schema: "ahorros",
                table: "cuenta",
                column: "id_agencia");

            migrationBuilder.CreateIndex(
                name: "ix_cuenta_id_tipo_cuenta",
                schema: "ahorros",
                table: "cuenta",
                column: "id_tipo_cuenta");

            migrationBuilder.CreateIndex(
                name: "ix_cuenta_numero",
                schema: "ahorros",
                table: "cuenta",
                column: "numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cuenta_cliente_id_cliente",
                schema: "ahorros",
                table: "cuenta_cliente",
                column: "id_cliente");

            migrationBuilder.CreateIndex(
                name: "ix_cuenta_item_saldo_id_cuenta_id_item_saldo",
                schema: "ahorros",
                table: "cuenta_item_saldo",
                columns: new[] { "id_cuenta", "id_item_saldo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cuenta_item_saldo_id_item_saldo",
                schema: "ahorros",
                table: "cuenta_item_saldo",
                column: "id_item_saldo");

            migrationBuilder.CreateIndex(
                name: "ix_item_saldo_codigo",
                schema: "ahorros",
                table: "item_saldo",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tipo_cuenta_codigo",
                schema: "ahorros",
                table: "tipo_cuenta",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tipo_cuenta_item_saldo_id_item_saldo",
                schema: "ahorros",
                table: "tipo_cuenta_item_saldo",
                column: "id_item_saldo");

            // --- Versionado real (mismo patrón que Nivel1_MotorContable) ---
            // Softbank ya versiona CUENTA y CUENTA_ITEMSALDO de forma nativa (temporal
            // tables de SQL Server) — es el único caso bueno del núcleo original. Acá
            // se replica con el mismo mecanismo trigger+historico usado en
            // contabilidad.cuenta_contable, para no depender de una feature específica
            // del motor y mantener un solo patrón en todo el proyecto.
            migrationBuilder.Sql(
                """
                CREATE TABLE ahorros.cuenta_historico (
                    historico_id    bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                    operacion       varchar(10) NOT NULL,
                    modificado_en   timestamptz NOT NULL DEFAULT now(),
                    id              uuid NOT NULL,
                    numero          varchar(20) NOT NULL,
                    id_tipo_cuenta  integer NOT NULL,
                    id_agencia      integer NOT NULL,
                    fecha_apertura  date NOT NULL,
                    estado          varchar(20) NOT NULL,
                    creado_en       timestamptz NOT NULL,
                    creado_por      varchar(100) NOT NULL,
                    modificado_por  varchar(100)
                );
                CREATE INDEX ix_cuenta_historico_id ON ahorros.cuenta_historico (id);

                CREATE FUNCTION ahorros.fn_versionar_cuenta()
                RETURNS trigger AS $$
                BEGIN
                    IF (TG_OP = 'DELETE') THEN
                        INSERT INTO ahorros.cuenta_historico
                            (operacion, id, numero, id_tipo_cuenta, id_agencia, fecha_apertura,
                             estado, creado_en, creado_por, modificado_por)
                        VALUES ('DELETE', OLD.id, OLD.numero, OLD.id_tipo_cuenta, OLD.id_agencia,
                                OLD.fecha_apertura, OLD.estado, OLD.creado_en, OLD.creado_por, OLD.modificado_por);
                        RETURN OLD;
                    ELSE
                        INSERT INTO ahorros.cuenta_historico
                            (operacion, id, numero, id_tipo_cuenta, id_agencia, fecha_apertura,
                             estado, creado_en, creado_por, modificado_por)
                        VALUES (TG_OP, NEW.id, NEW.numero, NEW.id_tipo_cuenta, NEW.id_agencia,
                                NEW.fecha_apertura, NEW.estado, NEW.creado_en, NEW.creado_por, NEW.modificado_por);
                        RETURN NEW;
                    END IF;
                END;
                $$ LANGUAGE plpgsql;

                CREATE TRIGGER trg_versionar_cuenta
                    AFTER INSERT OR UPDATE OR DELETE ON ahorros.cuenta
                    FOR EACH ROW EXECUTE FUNCTION ahorros.fn_versionar_cuenta();

                CREATE TABLE ahorros.cuenta_item_saldo_historico (
                    historico_id      bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                    operacion         varchar(10) NOT NULL,
                    modificado_en     timestamptz NOT NULL DEFAULT now(),
                    id                uuid NOT NULL,
                    id_cuenta         uuid NOT NULL,
                    id_item_saldo     integer NOT NULL,
                    saldo             numeric(18,2) NOT NULL,
                    acredita_prestamo boolean NOT NULL,
                    creado_en         timestamptz NOT NULL,
                    creado_por        varchar(100) NOT NULL,
                    modificado_por    varchar(100)
                );
                CREATE INDEX ix_cuenta_item_saldo_historico_id ON ahorros.cuenta_item_saldo_historico (id);

                CREATE FUNCTION ahorros.fn_versionar_cuenta_item_saldo()
                RETURNS trigger AS $$
                BEGIN
                    IF (TG_OP = 'DELETE') THEN
                        INSERT INTO ahorros.cuenta_item_saldo_historico
                            (operacion, id, id_cuenta, id_item_saldo, saldo, acredita_prestamo,
                             creado_en, creado_por, modificado_por)
                        VALUES ('DELETE', OLD.id, OLD.id_cuenta, OLD.id_item_saldo, OLD.saldo,
                                OLD.acredita_prestamo, OLD.creado_en, OLD.creado_por, OLD.modificado_por);
                        RETURN OLD;
                    ELSE
                        INSERT INTO ahorros.cuenta_item_saldo_historico
                            (operacion, id, id_cuenta, id_item_saldo, saldo, acredita_prestamo,
                             creado_en, creado_por, modificado_por)
                        VALUES (TG_OP, NEW.id, NEW.id_cuenta, NEW.id_item_saldo, NEW.saldo,
                                NEW.acredita_prestamo, NEW.creado_en, NEW.creado_por, NEW.modificado_por);
                        RETURN NEW;
                    END IF;
                END;
                $$ LANGUAGE plpgsql;

                CREATE TRIGGER trg_versionar_cuenta_item_saldo
                    AFTER INSERT OR UPDATE OR DELETE ON ahorros.cuenta_item_saldo
                    FOR EACH ROW EXECUTE FUNCTION ahorros.fn_versionar_cuenta_item_saldo();
                """);

            // --- Seed de catálogos (productos y baldes de saldo) ---
            migrationBuilder.Sql(
                """
                INSERT INTO ahorros.tipo_cuenta (codigo, nombre, permite_debito_prestamo, saldo_minimo_con_prestamo, activo) VALUES
                    ('AHV',  'Ahorro a la Vista',            true,  20.00, true),
                    ('AHI',  'Ahorro Infantil',               false, NULL,  true),
                    ('CERT', 'Certificados de Aportación',    false, NULL,  true);

                INSERT INTO ahorros.item_saldo (codigo, nombre) VALUES
                    ('DISP', 'Disponible'),
                    ('ENC',  'Encaje'),
                    ('BLOQ', 'Bloqueado'),
                    ('INT',  'Interés por pagar');

                INSERT INTO ahorros.tipo_cuenta_item_saldo (id_tipo_cuenta, id_item_saldo)
                SELECT tc.id, i.id FROM ahorros.tipo_cuenta tc, ahorros.item_saldo i
                WHERE tc.codigo = 'AHV' AND i.codigo IN ('DISP', 'ENC', 'BLOQ', 'INT');

                INSERT INTO ahorros.tipo_cuenta_item_saldo (id_tipo_cuenta, id_item_saldo)
                SELECT tc.id, i.id FROM ahorros.tipo_cuenta tc, ahorros.item_saldo i
                WHERE tc.codigo IN ('AHI', 'CERT') AND i.codigo IN ('DISP', 'INT');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DROP TRIGGER IF EXISTS trg_versionar_cuenta_item_saldo ON ahorros.cuenta_item_saldo;
                DROP FUNCTION IF EXISTS ahorros.fn_versionar_cuenta_item_saldo();
                DROP TABLE IF EXISTS ahorros.cuenta_item_saldo_historico;

                DROP TRIGGER IF EXISTS trg_versionar_cuenta ON ahorros.cuenta;
                DROP FUNCTION IF EXISTS ahorros.fn_versionar_cuenta();
                DROP TABLE IF EXISTS ahorros.cuenta_historico;
                """);

            migrationBuilder.DropTable(
                name: "cuenta_cliente",
                schema: "ahorros");

            migrationBuilder.DropTable(
                name: "cuenta_item_saldo",
                schema: "ahorros");

            migrationBuilder.DropTable(
                name: "tipo_cuenta_item_saldo",
                schema: "ahorros");

            migrationBuilder.DropTable(
                name: "cuenta",
                schema: "ahorros");

            migrationBuilder.DropTable(
                name: "item_saldo",
                schema: "ahorros");

            migrationBuilder.DropTable(
                name: "tipo_cuenta",
                schema: "ahorros");
        }
    }
}
