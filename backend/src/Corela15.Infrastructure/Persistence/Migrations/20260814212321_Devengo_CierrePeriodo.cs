using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Devengo_CierrePeriodo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "tasa_interes_anual",
                schema: "ahorros",
                table: "tipo_cuenta",
                type: "numeric(9,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "devengo_interes_log",
                schema: "ahorros",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    id_cuenta = table.Column<Guid>(type: "uuid", nullable: false),
                    saldo_base = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    tasa_anual_aplicada = table.Column<decimal>(type: "numeric(9,4)", nullable: false),
                    monto_devengado = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_devengo_interes_log", x => x.id);
                    table.ForeignKey(
                        name: "fk_devengo_interes_log_cuenta_id_cuenta",
                        column: x => x.id_cuenta,
                        principalSchema: "ahorros",
                        principalTable: "cuenta",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "periodo_contable",
                schema: "contabilidad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    periodo = table.Column<DateOnly>(type: "date", nullable: false),
                    cerrado = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_cierre = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    cerrado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_periodo_contable", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_devengo_interes_log_fecha_id_cuenta",
                schema: "ahorros",
                table: "devengo_interes_log",
                columns: new[] { "fecha", "id_cuenta" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_devengo_interes_log_id_cuenta",
                schema: "ahorros",
                table: "devengo_interes_log",
                column: "id_cuenta");

            migrationBuilder.CreateIndex(
                name: "ix_periodo_contable_periodo",
                schema: "contabilidad",
                table: "periodo_contable",
                column: "periodo",
                unique: true);

            // Subcuentas de detalle reales para el devengo de interés de
            // ahorros: 2503 (pasivo — lo que la cooperativa le debe al
            // socio por interés ya devengado pero no pagado) bajo el grupo
            // 25 Cuentas por pagar, y 4101 (gasto) bajo el grupo 41
            // Intereses causados — ambos grupos ya sembrados desde Nivel 1,
            // solo faltaba el detalle.
            migrationBuilder.Sql(
                """
                INSERT INTO contabilidad.cuenta_contable (id, codigo, nombre, grupo, naturaleza, id_cuenta_padre, es_mayor, activa, creado_en, creado_por)
                SELECT gen_random_uuid(), '2503', 'Intereses por pagar sobre depósitos', 'Pasivo', 'Acreedora', p.id, true, true, now(), 'seed:migracion'
                FROM contabilidad.cuenta_contable p WHERE p.codigo = '25';

                INSERT INTO contabilidad.cuenta_contable (id, codigo, nombre, grupo, naturaleza, id_cuenta_padre, es_mayor, activa, creado_en, creado_por)
                SELECT gen_random_uuid(), '4101', 'Intereses causados en depósitos', 'Gastos', 'Deudora', p.id, true, true, now(), 'seed:migracion'
                FROM contabilidad.cuenta_contable p WHERE p.codigo = '41';

                INSERT INTO contabilidad.tipo_transaccion
                    (codigo, nombre, id_cuenta_contable_debito, id_cuenta_contable_credito, id_tipo_comprobante, signo_saldo_cuenta, activo)
                SELECT 'DEVENGO-INT-AHO', 'Devengo de interés sobre depósitos de ahorro', gasto.id, pasivo.id, tc.id, 1, true
                FROM contabilidad.cuenta_contable gasto, contabilidad.cuenta_contable pasivo, contabilidad.tipo_comprobante_contable tc
                WHERE gasto.codigo = '4101' AND pasivo.codigo = '2503' AND tc.codigo = 'DIA';
                """);

            // Tasa de referencia real para Ahorro a la Vista (2% nominal
            // anual — tasa pasiva típica de una COAC Segmento 2 para este
            // producto). Ahorro Infantil y Certificados de Aportación
            // quedan en 0 (no devengan) — decisión de producto, no un
            // olvido: Certificados de Aportación es capital social, no
            // captación remunerada en este diseño.
            migrationBuilder.Sql(
                """
                UPDATE ahorros.tipo_cuenta SET tasa_interes_anual = 0.0200 WHERE codigo = 'AHV';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM contabilidad.tipo_transaccion WHERE codigo = 'DEVENGO-INT-AHO';
                DELETE FROM contabilidad.cuenta_contable WHERE codigo IN ('2503', '4101');
                """);

            migrationBuilder.DropTable(
                name: "devengo_interes_log",
                schema: "ahorros");

            migrationBuilder.DropTable(
                name: "periodo_contable",
                schema: "contabilidad");

            migrationBuilder.DropColumn(
                name: "tasa_interes_anual",
                schema: "ahorros",
                table: "tipo_cuenta");
        }
    }
}
