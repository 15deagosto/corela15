using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Nivel1_MotorContable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "contabilidad");

            migrationBuilder.CreateTable(
                name: "cuenta_contable",
                schema: "contabilidad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    grupo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    naturaleza = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    id_cuenta_padre = table.Column<Guid>(type: "uuid", nullable: true),
                    es_mayor = table.Column<bool>(type: "boolean", nullable: false),
                    activa = table.Column<bool>(type: "boolean", nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    modificado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    modificado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cuenta_contable", x => x.id);
                    table.ForeignKey(
                        name: "fk_cuenta_contable_cuenta_contable_id_cuenta_padre",
                        column: x => x.id_cuenta_padre,
                        principalSchema: "contabilidad",
                        principalTable: "cuenta_contable",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tipo_comprobante_contable",
                schema: "contabilidad",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipo_comprobante_contable", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "saldo_contable",
                schema: "contabilidad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_cuenta_contable = table.Column<Guid>(type: "uuid", nullable: false),
                    periodo = table.Column<DateOnly>(type: "date", nullable: false),
                    total_debitos = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    total_creditos = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    saldo_final = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_saldo_contable", x => x.id);
                    table.ForeignKey(
                        name: "fk_saldo_contable_cuenta_contable_id_cuenta_contable",
                        column: x => x.id_cuenta_contable,
                        principalSchema: "contabilidad",
                        principalTable: "cuenta_contable",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "comprobante_contable",
                schema: "contabilidad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero = table.Column<long>(type: "bigint", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    id_tipo_comprobante = table.Column<int>(type: "integer", nullable: false),
                    id_agencia = table.Column<int>(type: "integer", nullable: false),
                    descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    modificado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    modificado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_comprobante_contable", x => x.id);
                    table.ForeignKey(
                        name: "fk_comprobante_contable_agencia_id_agencia",
                        column: x => x.id_agencia,
                        principalSchema: "general",
                        principalTable: "agencia",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_comprobante_contable_tipos_comprobante_contable_id_tipo_com",
                        column: x => x.id_tipo_comprobante,
                        principalSchema: "contabilidad",
                        principalTable: "tipo_comprobante_contable",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "movimiento_comprobante_contable",
                schema: "contabilidad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_comprobante = table.Column<Guid>(type: "uuid", nullable: false),
                    id_cuenta_contable = table.Column<Guid>(type: "uuid", nullable: false),
                    numero_linea = table.Column<int>(type: "integer", nullable: false),
                    debito = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    credito = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    descripcion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_movimiento_comprobante_contable", x => x.id);
                    table.CheckConstraint("ck_movimiento_debito_o_credito", "(debito = 0 OR credito = 0) AND (debito + credito) > 0");
                    table.ForeignKey(
                        name: "fk_movimiento_comprobante_contable_comprobante_contable_id_com",
                        column: x => x.id_comprobante,
                        principalSchema: "contabilidad",
                        principalTable: "comprobante_contable",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_movimiento_comprobante_contable_cuenta_contable_id_cuenta_c",
                        column: x => x.id_cuenta_contable,
                        principalSchema: "contabilidad",
                        principalTable: "cuenta_contable",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_comprobante_contable_id_agencia",
                schema: "contabilidad",
                table: "comprobante_contable",
                column: "id_agencia");

            migrationBuilder.CreateIndex(
                name: "ix_comprobante_contable_id_tipo_comprobante_numero",
                schema: "contabilidad",
                table: "comprobante_contable",
                columns: new[] { "id_tipo_comprobante", "numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cuenta_contable_codigo",
                schema: "contabilidad",
                table: "cuenta_contable",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cuenta_contable_id_cuenta_padre",
                schema: "contabilidad",
                table: "cuenta_contable",
                column: "id_cuenta_padre");

            migrationBuilder.CreateIndex(
                name: "ix_movimiento_comprobante_contable_id_comprobante_numero_linea",
                schema: "contabilidad",
                table: "movimiento_comprobante_contable",
                columns: new[] { "id_comprobante", "numero_linea" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_movimiento_comprobante_contable_id_cuenta_contable",
                schema: "contabilidad",
                table: "movimiento_comprobante_contable",
                column: "id_cuenta_contable");

            migrationBuilder.CreateIndex(
                name: "ix_saldo_contable_id_cuenta_contable_periodo",
                schema: "contabilidad",
                table: "saldo_contable",
                columns: new[] { "id_cuenta_contable", "periodo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tipo_comprobante_contable_codigo",
                schema: "contabilidad",
                table: "tipo_comprobante_contable",
                column: "codigo",
                unique: true);

            // --- Versionado real de cuenta_contable (historico + trigger) ---
            // Regla de diseño explícita (02-arquitectura-datos-40-modulos.md, Nivel 1.3):
            // Softbank versiona el plan de cuentas a mano (CUENTACONTABLE_2022A, copia
            // manual de la tabla entera). Acá el historial se llena solo, en cada
            // INSERT/UPDATE/DELETE, vía trigger — nunca una copia manual. Este es el
            // patrón a replicar cuando toque versionar CUENTA/PRESTAMO en niveles futuros.
            migrationBuilder.Sql(
                """
                CREATE TABLE contabilidad.cuenta_contable_historico (
                    historico_id      bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                    operacion         varchar(10) NOT NULL,
                    modificado_en     timestamptz NOT NULL DEFAULT now(),
                    id                uuid NOT NULL,
                    codigo            varchar(20) NOT NULL,
                    nombre            varchar(200) NOT NULL,
                    grupo             varchar(30) NOT NULL,
                    naturaleza        varchar(20) NOT NULL,
                    id_cuenta_padre   uuid,
                    es_mayor          boolean NOT NULL,
                    activa            boolean NOT NULL,
                    creado_en         timestamptz NOT NULL,
                    creado_por        varchar(100) NOT NULL,
                    modificado_por    varchar(100)
                );

                CREATE INDEX ix_cuenta_contable_historico_id
                    ON contabilidad.cuenta_contable_historico (id);

                CREATE FUNCTION contabilidad.fn_versionar_cuenta_contable()
                RETURNS trigger AS $$
                BEGIN
                    IF (TG_OP = 'DELETE') THEN
                        INSERT INTO contabilidad.cuenta_contable_historico
                            (operacion, id, codigo, nombre, grupo, naturaleza, id_cuenta_padre,
                             es_mayor, activa, creado_en, creado_por, modificado_por)
                        VALUES ('DELETE', OLD.id, OLD.codigo, OLD.nombre, OLD.grupo, OLD.naturaleza,
                                OLD.id_cuenta_padre, OLD.es_mayor, OLD.activa, OLD.creado_en,
                                OLD.creado_por, OLD.modificado_por);
                        RETURN OLD;
                    ELSE
                        INSERT INTO contabilidad.cuenta_contable_historico
                            (operacion, id, codigo, nombre, grupo, naturaleza, id_cuenta_padre,
                             es_mayor, activa, creado_en, creado_por, modificado_por)
                        VALUES (TG_OP, NEW.id, NEW.codigo, NEW.nombre, NEW.grupo, NEW.naturaleza,
                                NEW.id_cuenta_padre, NEW.es_mayor, NEW.activa, NEW.creado_en,
                                NEW.creado_por, NEW.modificado_por);
                        RETURN NEW;
                    END IF;
                END;
                $$ LANGUAGE plpgsql;

                CREATE TRIGGER trg_versionar_cuenta_contable
                    AFTER INSERT OR UPDATE OR DELETE ON contabilidad.cuenta_contable
                    FOR EACH ROW EXECUTE FUNCTION contabilidad.fn_versionar_cuenta_contable();
                """);

            SeedCatalogoCuc(migrationBuilder);
            SeedTiposComprobante(migrationBuilder);
        }

        /// <summary>
        /// Grupos 1er y 2do nivel del CUC de la SEPS (Resolución
        /// SEPS-IGT-IGS-INSESF-INR-INFMR-INGINT-2022-0194). Son cuentas de
        /// agrupación (es_mayor = false) — las subcuentas de detalle donde
        /// realmente se contabiliza se agregan cuenta por cuenta cuando cada
        /// módulo (Ahorros, Crédito...) las necesite.
        /// </summary>
        private static void SeedCatalogoCuc(MigrationBuilder migrationBuilder)
        {
            var now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
            const string sistema = "seed:migracion";

            // (codigo, nombre, grupo, naturaleza, codigoPadre)
            var grupos1 = new (string Codigo, string Nombre, string Grupo, string Naturaleza)[]
            {
                ("1", "ACTIVO", "Activo", "Deudora"),
                ("2", "PASIVO", "Pasivo", "Acreedora"),
                ("3", "PATRIMONIO", "Patrimonio", "Acreedora"),
                ("4", "GASTOS", "Gastos", "Deudora"),
                ("5", "INGRESOS", "Ingresos", "Acreedora"),
                ("6", "CUENTAS CONTINGENTES", "CuentasContingentes", "Deudora"),
                ("7", "CUENTAS DE ORDEN", "CuentasDeOrden", "Deudora"),
            };

            var grupos2 = new (string Codigo, string Nombre, string Grupo, string Naturaleza, string Padre)[]
            {
                ("11", "Fondos disponibles", "Activo", "Deudora", "1"),
                ("12", "Operaciones interfinancieras", "Activo", "Deudora", "1"),
                ("13", "Inversiones", "Activo", "Deudora", "1"),
                ("14", "Cartera de créditos", "Activo", "Deudora", "1"),
                ("16", "Cuentas por cobrar", "Activo", "Deudora", "1"),
                ("17", "Bienes realizables, adjudicados por pago y bienes no utilizados", "Activo", "Deudora", "1"),
                ("18", "Propiedades y equipo", "Activo", "Deudora", "1"),
                ("19", "Otros activos", "Activo", "Deudora", "1"),

                ("21", "Obligaciones con el público", "Pasivo", "Acreedora", "2"),
                ("22", "Operaciones interfinancieras", "Pasivo", "Acreedora", "2"),
                ("23", "Obligaciones inmediatas", "Pasivo", "Acreedora", "2"),
                ("25", "Cuentas por pagar", "Pasivo", "Acreedora", "2"),
                ("26", "Obligaciones financieras", "Pasivo", "Acreedora", "2"),
                ("27", "Valores en circulación", "Pasivo", "Acreedora", "2"),
                ("28", "Aportes para futuras capitalizaciones", "Pasivo", "Acreedora", "2"),
                ("29", "Otros pasivos", "Pasivo", "Acreedora", "2"),

                ("31", "Capital social", "Patrimonio", "Acreedora", "3"),
                ("32", "Prima en colocación de certificados de aportación", "Patrimonio", "Acreedora", "3"),
                ("33", "Reservas", "Patrimonio", "Acreedora", "3"),
                ("34", "Otros aportes patrimoniales", "Patrimonio", "Acreedora", "3"),
                ("35", "Superávit por valuaciones", "Patrimonio", "Acreedora", "3"),
                ("36", "Resultados", "Patrimonio", "Acreedora", "3"),

                ("41", "Intereses causados", "Gastos", "Deudora", "4"),
                ("42", "Comisiones causadas", "Gastos", "Deudora", "4"),
                ("43", "Pérdidas financieras", "Gastos", "Deudora", "4"),
                ("44", "Provisiones", "Gastos", "Deudora", "4"),
                ("45", "Gastos de operación", "Gastos", "Deudora", "4"),
                ("46", "Otras pérdidas operacionales", "Gastos", "Deudora", "4"),
                ("47", "Otros gastos y pérdidas", "Gastos", "Deudora", "4"),

                ("51", "Intereses y descuentos ganados", "Ingresos", "Acreedora", "5"),
                ("52", "Comisiones ganadas", "Ingresos", "Acreedora", "5"),
                ("53", "Utilidades financieras", "Ingresos", "Acreedora", "5"),
                ("54", "Ingresos por servicios", "Ingresos", "Acreedora", "5"),
                ("55", "Otros ingresos operacionales", "Ingresos", "Acreedora", "5"),
                ("56", "Otros ingresos", "Ingresos", "Acreedora", "5"),

                ("61", "Cuentas contingentes deudoras", "CuentasContingentes", "Deudora", "6"),
                ("62", "Cuentas contingentes por el contrario (deudoras)", "CuentasContingentes", "Acreedora", "6"),
                ("63", "Cuentas contingentes por el contrario (acreedoras)", "CuentasContingentes", "Deudora", "6"),
                ("64", "Cuentas contingentes acreedoras", "CuentasContingentes", "Acreedora", "6"),

                ("71", "Cuentas de orden deudoras", "CuentasDeOrden", "Deudora", "7"),
                ("73", "Cuentas de orden por el contrario", "CuentasDeOrden", "Acreedora", "7"),
                ("74", "Cuentas de orden acreedoras", "CuentasDeOrden", "Acreedora", "7"),
            };

            var idPorCodigo = new Dictionary<string, Guid>();
            foreach (var g in grupos1)
            {
                idPorCodigo[g.Codigo] = Guid.NewGuid();
            }
            foreach (var g in grupos2)
            {
                idPorCodigo[g.Codigo] = Guid.NewGuid();
            }

            var columns = new[]
            {
                "id", "codigo", "nombre", "grupo", "naturaleza", "id_cuenta_padre",
                "es_mayor", "activa", "creado_en", "creado_por"
            };

            foreach (var g in grupos1)
            {
                migrationBuilder.InsertData(
                    schema: "contabilidad",
                    table: "cuenta_contable",
                    columns: columns,
                    values: new object[]
                    {
                        idPorCodigo[g.Codigo], g.Codigo, g.Nombre, g.Grupo, g.Naturaleza,
                        null, false, true, now, sistema
                    });
            }

            foreach (var g in grupos2)
            {
                migrationBuilder.InsertData(
                    schema: "contabilidad",
                    table: "cuenta_contable",
                    columns: columns,
                    values: new object[]
                    {
                        idPorCodigo[g.Codigo], g.Codigo, g.Nombre, g.Grupo, g.Naturaleza,
                        idPorCodigo[g.Padre], false, true, now, sistema
                    });
            }
        }

        private static void SeedTiposComprobante(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "contabilidad",
                table: "tipo_comprobante_contable",
                columns: new[] { "codigo", "nombre" },
                values: new object[,]
                {
                    { "ING", "Ingreso" },
                    { "EGR", "Egreso" },
                    { "DIA", "Diario" },
                    { "APE", "Apertura" },
                    { "CIE", "Cierre" },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DROP TRIGGER IF EXISTS trg_versionar_cuenta_contable ON contabilidad.cuenta_contable;
                DROP FUNCTION IF EXISTS contabilidad.fn_versionar_cuenta_contable();
                DROP TABLE IF EXISTS contabilidad.cuenta_contable_historico;
                """);

            migrationBuilder.DropTable(
                name: "movimiento_comprobante_contable",
                schema: "contabilidad");

            migrationBuilder.DropTable(
                name: "saldo_contable",
                schema: "contabilidad");

            migrationBuilder.DropTable(
                name: "comprobante_contable",
                schema: "contabilidad");

            migrationBuilder.DropTable(
                name: "cuenta_contable",
                schema: "contabilidad");

            migrationBuilder.DropTable(
                name: "tipo_comprobante_contable",
                schema: "contabilidad");
        }
    }
}
