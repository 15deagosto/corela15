using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CuentasPorPagar_FormaCancelacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "cuotas",
                schema: "cuentasporcobrar",
                table: "cuenta_por_pagar",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "cuentasporcobrar",
                table: "cuenta_por_pagar",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.CreateTable(
                name: "forma_cancelacion",
                schema: "contabilidad",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    es_efectivo = table.Column<bool>(type: "boolean", nullable: false),
                    es_cheque = table.Column<bool>(type: "boolean", nullable: false),
                    es_transferencia = table.Column<bool>(type: "boolean", nullable: false),
                    es_causal = table.Column<bool>(type: "boolean", nullable: false),
                    es_cuenta = table.Column<bool>(type: "boolean", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_forma_cancelacion", x => x.codigo);
                });

            // Datos reales verificados contra CONTABILIDAD.FORMA_CANCELACION
            // (6 filas reales). "6 TRANSFERENCIA BANCARIA" figura
            // ACTIVO=false en la fuente real — se respeta tal cual.
            migrationBuilder.Sql(@"
INSERT INTO contabilidad.forma_cancelacion
    (codigo, nombre, es_efectivo, es_cheque, es_transferencia, es_causal, es_cuenta, activo) VALUES
('1', 'Efectivo', true, false, false, false, false, true),
('2', 'Cheque', false, true, false, false, false, true),
('3', 'Causal', false, false, false, true, false, true),
('4', 'Acreditación a cuenta', false, false, false, false, true, true),
('5', 'Factura servicios profesionales', false, false, false, false, false, true),
('6', 'Transferencia bancaria', false, false, true, false, false, false);
");

            // Motor de asientos real para Cuentas por Pagar — mismo patrón
            // que REG-CXC/ABONO-CXC (ver Contabilidad_SeedCuentaPorCobrar):
            // registro debita el gasto genérico (4507→450790 Otros gastos,
            // ya sembrado en el CUC completo) y acredita el pasivo genérico
            // (2590→259090 Otras cuentas por pagar); el pago hace la reversa
            // desde Caja.
            migrationBuilder.Sql(
                """
                INSERT INTO contabilidad.tipo_transaccion
                    (codigo, nombre, id_cuenta_contable_debito, id_cuenta_contable_credito, id_tipo_comprobante, signo_saldo_cuenta, activo)
                SELECT 'REG-CXP', 'Registro de cuenta por pagar', gasto.id, cxp.id, tc.id, 1, true
                FROM contabilidad.cuenta_contable gasto, contabilidad.cuenta_contable cxp, contabilidad.tipo_comprobante_contable tc
                WHERE gasto.codigo = '450790' AND cxp.codigo = '259090' AND tc.codigo = 'DIA';

                INSERT INTO contabilidad.tipo_transaccion
                    (codigo, nombre, id_cuenta_contable_debito, id_cuenta_contable_credito, id_tipo_comprobante, signo_saldo_cuenta, activo)
                SELECT 'PAGO-CXP', 'Pago de cuenta por pagar', cxp.id, caja.id, tc.id, -1, true
                FROM contabilidad.cuenta_contable cxp, contabilidad.cuenta_contable caja, contabilidad.tipo_comprobante_contable tc
                WHERE cxp.codigo = '259090' AND caja.codigo = '1101' AND tc.codigo = 'DIA';

                INSERT INTO contabilidad.tipo_transaccion
                    (codigo, nombre, id_cuenta_contable_debito, id_cuenta_contable_credito, id_tipo_comprobante, signo_saldo_cuenta, activo)
                SELECT 'ANULA-CXP', 'Anulación de cuenta por pagar', cxp.id, gasto.id, tc.id, -1, true
                FROM contabilidad.cuenta_contable cxp, contabilidad.cuenta_contable gasto, contabilidad.tipo_comprobante_contable tc
                WHERE cxp.codigo = '259090' AND gasto.codigo = '450790' AND tc.codigo = 'DIA';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM contabilidad.tipo_transaccion WHERE codigo IN ('REG-CXP', 'PAGO-CXP', 'ANULA-CXP');");

            migrationBuilder.DropTable(
                name: "forma_cancelacion",
                schema: "contabilidad");

            migrationBuilder.DropColumn(
                name: "cuotas",
                schema: "cuentasporcobrar",
                table: "cuenta_por_pagar");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "cuentasporcobrar",
                table: "cuenta_por_pagar");
        }
    }
}
