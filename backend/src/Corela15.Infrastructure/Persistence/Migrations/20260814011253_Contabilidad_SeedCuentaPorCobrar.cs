using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Contabilidad_SeedCuentaPorCobrar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Subcuentas de detalle reales para cuentas por cobrar internas
            // (grupo CUC 16, no confundir con cartera de crédito de socios) y
            // el ingreso que las origina (grupo 56, Otros ingresos).
            migrationBuilder.Sql(
                """
                INSERT INTO contabilidad.cuenta_contable (id, codigo, nombre, grupo, naturaleza, id_cuenta_padre, es_mayor, activa, creado_en, creado_por)
                SELECT gen_random_uuid(), '1601', 'Cuentas por cobrar varias', 'Activo', 'Deudora', p.id, true, true, now(), 'seed:migracion'
                FROM contabilidad.cuenta_contable p WHERE p.codigo = '16';

                INSERT INTO contabilidad.cuenta_contable (id, codigo, nombre, grupo, naturaleza, id_cuenta_padre, es_mayor, activa, creado_en, creado_por)
                SELECT gen_random_uuid(), '5601', 'Otros ingresos varios', 'Ingresos', 'Acreedora', p.id, true, true, now(), 'seed:migracion'
                FROM contabilidad.cuenta_contable p WHERE p.codigo = '56';
                """);

            // Motor contable configurable para registro/abono de CxC — mismo
            // patrón que DEP-EFEC/RET-EFEC/APER-DPF/CANC-DPF.
            migrationBuilder.Sql(
                """
                INSERT INTO contabilidad.tipo_transaccion
                    (codigo, nombre, id_cuenta_contable_debito, id_cuenta_contable_credito, id_tipo_comprobante, signo_saldo_cuenta, activo)
                SELECT 'REG-CXC', 'Registro de cuenta por cobrar', cxc.id, ing.id, tc.id, 1, true
                FROM contabilidad.cuenta_contable cxc, contabilidad.cuenta_contable ing, contabilidad.tipo_comprobante_contable tc
                WHERE cxc.codigo = '1601' AND ing.codigo = '5601' AND tc.codigo = 'DIA';

                INSERT INTO contabilidad.tipo_transaccion
                    (codigo, nombre, id_cuenta_contable_debito, id_cuenta_contable_credito, id_tipo_comprobante, signo_saldo_cuenta, activo)
                SELECT 'ABONO-CXC', 'Abono a cuenta por cobrar', caja.id, cxc.id, tc.id, -1, true
                FROM contabilidad.cuenta_contable caja, contabilidad.cuenta_contable cxc, contabilidad.tipo_comprobante_contable tc
                WHERE caja.codigo = '1101' AND cxc.codigo = '1601' AND tc.codigo = 'DIA';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM contabilidad.tipo_transaccion WHERE codigo IN ('REG-CXC', 'ABONO-CXC');
                DELETE FROM contabilidad.cuenta_contable WHERE codigo IN ('1601', '5601');
                """);
        }
    }
}
