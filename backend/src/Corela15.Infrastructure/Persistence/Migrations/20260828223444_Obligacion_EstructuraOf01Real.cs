using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Obligacion_EstructuraOf01Real : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_obligacion_financiera_codigo",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.DropColumn(
                name: "codigo",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.DropColumn(
                name: "deuda_inicial",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.DropColumn(
                name: "estado",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.DropColumn(
                name: "numero_pagare",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.DropColumn(
                name: "saldo_actual",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.DropColumn(
                name: "valor_entregado",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.AddColumn<string>(
                name: "codigo_clase",
                schema: "obligacion",
                table: "obligacion_financiera",
                type: "character varying(1)",
                maxLength: 1,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "codigo_estado",
                schema: "obligacion",
                table: "obligacion_financiera",
                type: "character varying(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "codigo_forma_cancelacion",
                schema: "obligacion",
                table: "obligacion_financiera",
                type: "character varying(1)",
                maxLength: 1,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "codigo_pais_acreedor",
                schema: "obligacion",
                table: "obligacion_financiera",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "codigo_periodicidad_pago",
                schema: "obligacion",
                table: "obligacion_financiera",
                type: "character varying(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "creado_en",
                schema: "obligacion",
                table: "obligacion_financiera",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "creado_por",
                schema: "obligacion",
                table: "obligacion_financiera",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "destino_linea_credito",
                schema: "obligacion",
                table: "obligacion_financiera",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateOnly>(
                name: "fecha_concesion",
                schema: "obligacion",
                table: "obligacion_financiera",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<DateOnly>(
                name: "fecha_vencimiento",
                schema: "obligacion",
                table: "obligacion_financiera",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<Guid>(
                name: "id_cuenta_contable",
                schema: "obligacion",
                table: "obligacion_financiera",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "identificacion_acreedor",
                schema: "obligacion",
                table: "obligacion_financiera",
                type: "character varying(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "intereses_por_pagar",
                schema: "obligacion",
                table: "obligacion_financiera",
                type: "numeric(15,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "modificado_en",
                schema: "obligacion",
                table: "obligacion_financiera",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "modificado_por",
                schema: "obligacion",
                table: "obligacion_financiera",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "monto_linea_credito",
                schema: "obligacion",
                table: "obligacion_financiera",
                type: "numeric(15,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "monto_por_utilizar",
                schema: "obligacion",
                table: "obligacion_financiera",
                type: "numeric(15,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "numero_obligacion",
                schema: "obligacion",
                table: "obligacion_financiera",
                type: "character varying(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "numero_obligacion_anterior",
                schema: "obligacion",
                table: "obligacion_financiera",
                type: "character varying(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "numero_periodos_gracia",
                schema: "obligacion",
                table: "obligacion_financiera",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "paga_comision",
                schema: "obligacion",
                table: "obligacion_financiera",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "saldo",
                schema: "obligacion",
                table: "obligacion_financiera",
                type: "numeric(15,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "tasa_interes",
                schema: "obligacion",
                table: "obligacion_financiera",
                type: "numeric(6,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "tasa_interes_comision",
                schema: "obligacion",
                table: "obligacion_financiera",
                type: "numeric(6,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "tiene_periodo_gracia",
                schema: "obligacion",
                table: "obligacion_financiera",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "tipo_identificacion_acreedor",
                schema: "obligacion",
                table: "obligacion_financiera",
                type: "character varying(1)",
                maxLength: 1,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "valor_comision",
                schema: "obligacion",
                table: "obligacion_financiera",
                type: "numeric(15,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_vencido",
                schema: "obligacion",
                table: "obligacion_financiera",
                type: "numeric(15,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "clase_obligacion_financiera",
                schema: "obligacion",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_clase_obligacion_financiera", x => x.codigo);
                });

            migrationBuilder.CreateTable(
                name: "estado_obligacion_financiera",
                schema: "obligacion",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_estado_obligacion_financiera", x => x.codigo);
                });

            migrationBuilder.CreateTable(
                name: "forma_cancelacion_obligacion",
                schema: "obligacion",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_forma_cancelacion_obligacion", x => x.codigo);
                });

            migrationBuilder.CreateTable(
                name: "periodicidad_pago",
                schema: "obligacion",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_periodicidad_pago", x => x.codigo);
                });

            migrationBuilder.CreateIndex(
                name: "ix_obligacion_financiera_codigo_clase",
                schema: "obligacion",
                table: "obligacion_financiera",
                column: "codigo_clase");

            migrationBuilder.CreateIndex(
                name: "ix_obligacion_financiera_codigo_estado",
                schema: "obligacion",
                table: "obligacion_financiera",
                column: "codigo_estado");

            migrationBuilder.CreateIndex(
                name: "ix_obligacion_financiera_codigo_forma_cancelacion",
                schema: "obligacion",
                table: "obligacion_financiera",
                column: "codigo_forma_cancelacion");

            migrationBuilder.CreateIndex(
                name: "ix_obligacion_financiera_codigo_pais_acreedor",
                schema: "obligacion",
                table: "obligacion_financiera",
                column: "codigo_pais_acreedor");

            migrationBuilder.CreateIndex(
                name: "ix_obligacion_financiera_codigo_periodicidad_pago",
                schema: "obligacion",
                table: "obligacion_financiera",
                column: "codigo_periodicidad_pago");

            migrationBuilder.CreateIndex(
                name: "ix_obligacion_financiera_id_cuenta_contable",
                schema: "obligacion",
                table: "obligacion_financiera",
                column: "id_cuenta_contable");

            migrationBuilder.CreateIndex(
                name: "ix_obligacion_financiera_tipo_identificacion_acreedor_identifi",
                schema: "obligacion",
                table: "obligacion_financiera",
                columns: new[] { "tipo_identificacion_acreedor", "identificacion_acreedor", "numero_obligacion", "id_cuenta_contable" });

            migrationBuilder.AddForeignKey(
                name: "fk_obligacion_financiera_clase_obligacion_financiera_codigo_cl",
                schema: "obligacion",
                table: "obligacion_financiera",
                column: "codigo_clase",
                principalSchema: "obligacion",
                principalTable: "clase_obligacion_financiera",
                principalColumn: "codigo",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_obligacion_financiera_cuenta_contable_id_cuenta_contable",
                schema: "obligacion",
                table: "obligacion_financiera",
                column: "id_cuenta_contable",
                principalSchema: "contabilidad",
                principalTable: "cuenta_contable",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_obligacion_financiera_estado_obligacion_financiera_codigo_e",
                schema: "obligacion",
                table: "obligacion_financiera",
                column: "codigo_estado",
                principalSchema: "obligacion",
                principalTable: "estado_obligacion_financiera",
                principalColumn: "codigo",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_obligacion_financiera_forma_cancelacion_obligacion_codigo_f",
                schema: "obligacion",
                table: "obligacion_financiera",
                column: "codigo_forma_cancelacion",
                principalSchema: "obligacion",
                principalTable: "forma_cancelacion_obligacion",
                principalColumn: "codigo",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_obligacion_financiera_nacionalidad_codigo_pais_acreedor",
                schema: "obligacion",
                table: "obligacion_financiera",
                column: "codigo_pais_acreedor",
                principalSchema: "sujeto",
                principalTable: "nacionalidad",
                principalColumn: "codigo",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_obligacion_financiera_periodicidad_pago_codigo_periodicidad",
                schema: "obligacion",
                table: "obligacion_financiera",
                column: "codigo_periodicidad_pago",
                principalSchema: "obligacion",
                principalTable: "periodicidad_pago",
                principalColumn: "codigo",
                onDelete: ReferentialAction.Restrict);

            // Catálogos reales OF01 (SEPS, Manual Técnico v1.0, 13/03/2026),
            // verificados vía pdftotext -table contra el Manual Técnico de
            // Tablas de Información v34.0.
            migrationBuilder.Sql(@"
                INSERT INTO obligacion.estado_obligacion_financiera (codigo, nombre, activo) VALUES
                    ('NV', 'Nueva', true),
                    ('VG', 'Vigente', true),
                    ('VN', 'Vencida', true),
                    ('CN', 'Cancelada', true);

                INSERT INTO obligacion.periodicidad_pago (codigo, nombre, activo) VALUES
                    ('DI', 'Diario', true),
                    ('SA', 'Semanal', true),
                    ('QU', 'Quincenal', true),
                    ('ME', 'Mensual', true),
                    ('BM', 'Bimensual', true),
                    ('TR', 'Trimestral', true),
                    ('CT', 'Cuatrimestral', true),
                    ('SE', 'Semestral', true),
                    ('NM', 'Nueve meses', true),
                    ('AN', 'Anual', true),
                    ('VC', 'Al vencimiento', true),
                    ('NA', 'No aplica', true);

                INSERT INTO obligacion.clase_obligacion_financiera (codigo, nombre, activo) VALUES
                    ('N', 'Original', true),
                    ('V', 'Novada', true),
                    ('E', 'Reestructurada', true),
                    ('F', 'Refinanciada', true);

                INSERT INTO obligacion.forma_cancelacion_obligacion (codigo, nombre, activo) VALUES
                    ('N', 'Efectivo', true),
                    ('E', 'Efectivización de garantías', true),
                    ('W', 'Con otra operación de la misma institución', true),
                    ('Y', 'Otros', true);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM obligacion.estado_obligacion_financiera;
                DELETE FROM obligacion.periodicidad_pago;
                DELETE FROM obligacion.clase_obligacion_financiera;
                DELETE FROM obligacion.forma_cancelacion_obligacion;
            ");

            migrationBuilder.DropForeignKey(
                name: "fk_obligacion_financiera_clase_obligacion_financiera_codigo_cl",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.DropForeignKey(
                name: "fk_obligacion_financiera_cuenta_contable_id_cuenta_contable",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.DropForeignKey(
                name: "fk_obligacion_financiera_estado_obligacion_financiera_codigo_e",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.DropForeignKey(
                name: "fk_obligacion_financiera_forma_cancelacion_obligacion_codigo_f",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.DropForeignKey(
                name: "fk_obligacion_financiera_nacionalidad_codigo_pais_acreedor",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.DropForeignKey(
                name: "fk_obligacion_financiera_periodicidad_pago_codigo_periodicidad",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.DropTable(
                name: "clase_obligacion_financiera",
                schema: "obligacion");

            migrationBuilder.DropTable(
                name: "estado_obligacion_financiera",
                schema: "obligacion");

            migrationBuilder.DropTable(
                name: "forma_cancelacion_obligacion",
                schema: "obligacion");

            migrationBuilder.DropTable(
                name: "periodicidad_pago",
                schema: "obligacion");

            migrationBuilder.DropIndex(
                name: "ix_obligacion_financiera_codigo_clase",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.DropIndex(
                name: "ix_obligacion_financiera_codigo_estado",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.DropIndex(
                name: "ix_obligacion_financiera_codigo_forma_cancelacion",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.DropIndex(
                name: "ix_obligacion_financiera_codigo_pais_acreedor",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.DropIndex(
                name: "ix_obligacion_financiera_codigo_periodicidad_pago",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.DropIndex(
                name: "ix_obligacion_financiera_id_cuenta_contable",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.DropIndex(
                name: "ix_obligacion_financiera_tipo_identificacion_acreedor_identifi",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.DropColumn(
                name: "codigo_clase",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.DropColumn(
                name: "codigo_estado",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.DropColumn(
                name: "codigo_forma_cancelacion",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.DropColumn(
                name: "codigo_pais_acreedor",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.DropColumn(
                name: "codigo_periodicidad_pago",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.DropColumn(
                name: "creado_en",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.DropColumn(
                name: "creado_por",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.DropColumn(
                name: "destino_linea_credito",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.DropColumn(
                name: "fecha_concesion",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.DropColumn(
                name: "fecha_vencimiento",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.DropColumn(
                name: "id_cuenta_contable",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.DropColumn(
                name: "identificacion_acreedor",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.DropColumn(
                name: "intereses_por_pagar",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.DropColumn(
                name: "modificado_en",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.DropColumn(
                name: "modificado_por",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.DropColumn(
                name: "monto_linea_credito",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.DropColumn(
                name: "monto_por_utilizar",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.DropColumn(
                name: "numero_obligacion",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.DropColumn(
                name: "numero_obligacion_anterior",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.DropColumn(
                name: "numero_periodos_gracia",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.DropColumn(
                name: "paga_comision",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.DropColumn(
                name: "saldo",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.DropColumn(
                name: "tasa_interes",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.DropColumn(
                name: "tasa_interes_comision",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.DropColumn(
                name: "tiene_periodo_gracia",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.DropColumn(
                name: "tipo_identificacion_acreedor",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.DropColumn(
                name: "valor_comision",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.DropColumn(
                name: "valor_vencido",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.AddColumn<string>(
                name: "codigo",
                schema: "obligacion",
                table: "obligacion_financiera",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "deuda_inicial",
                schema: "obligacion",
                table: "obligacion_financiera",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "estado",
                schema: "obligacion",
                table: "obligacion_financiera",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "numero_pagare",
                schema: "obligacion",
                table: "obligacion_financiera",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "saldo_actual",
                schema: "obligacion",
                table: "obligacion_financiera",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_entregado",
                schema: "obligacion",
                table: "obligacion_financiera",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "ix_obligacion_financiera_codigo",
                schema: "obligacion",
                table: "obligacion_financiera",
                column: "codigo",
                unique: true);
        }
    }
}
