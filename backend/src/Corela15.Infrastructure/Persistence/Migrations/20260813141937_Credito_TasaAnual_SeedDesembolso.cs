using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Credito_TasaAnual_SeedDesembolso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "tasa_anual",
                schema: "credito",
                table: "tipo_prestamo",
                type: "numeric(9,4)",
                nullable: false,
                defaultValue: 0m);

            // Tasas reales de referencia por producto (nominal anual).
            migrationBuilder.Sql(
                """
                UPDATE credito.tipo_prestamo SET tasa_anual = 0.1720 WHERE codigo = 'CONS';
                UPDATE credito.tipo_prestamo SET tasa_anual = 0.2050 WHERE codigo = 'MICRO';
                UPDATE credito.tipo_prestamo SET tasa_anual = 0.1090 WHERE codigo = 'PROD';
                """);

            // Motor contable configurable para el desembolso: débito Cartera
            // de créditos (activo) / crédito Caja (sale efectivo hacia el
            // socio) — mismo patrón que DEP-EFEC/RET-EFEC de Nivel 2.
            migrationBuilder.Sql(
                """
                INSERT INTO contabilidad.tipo_transaccion
                    (codigo, nombre, id_cuenta_contable_debito, id_cuenta_contable_credito, id_tipo_comprobante, signo_saldo_cuenta, activo)
                SELECT 'DESEMB-EFEC', 'Desembolso de préstamo en efectivo', cartera.id, caja.id, tc.id, 1, true
                FROM contabilidad.cuenta_contable cartera, contabilidad.cuenta_contable caja, contabilidad.tipo_comprobante_contable tc
                WHERE cartera.codigo = '1401' AND caja.codigo = '1101' AND tc.codigo = 'DIA';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "tasa_anual",
                schema: "credito",
                table: "tipo_prestamo");
        }
    }
}
