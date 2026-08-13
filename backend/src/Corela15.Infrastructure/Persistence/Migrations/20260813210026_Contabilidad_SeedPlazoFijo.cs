using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Contabilidad_SeedPlazoFijo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Subcuenta de detalle real para depósitos a plazo fijo (grupo
            // CUC 2103, distinta de 2101 depósitos de ahorro a la vista).
            migrationBuilder.Sql(
                """
                INSERT INTO contabilidad.cuenta_contable (id, codigo, nombre, grupo, naturaleza, id_cuenta_padre, es_mayor, activa, creado_en, creado_por)
                SELECT gen_random_uuid(), '2103', 'Depósitos a plazo fijo', 'Pasivo', 'Acreedora', p.id, true, true, now(), 'seed:migracion'
                FROM contabilidad.cuenta_contable p WHERE p.codigo = '21';
                """);

            // Motor contable configurable para apertura/cancelación de DPF —
            // mismo patrón que DEP-EFEC/RET-EFEC/DESEMB-EFEC.
            migrationBuilder.Sql(
                """
                INSERT INTO contabilidad.tipo_transaccion
                    (codigo, nombre, id_cuenta_contable_debito, id_cuenta_contable_credito, id_tipo_comprobante, signo_saldo_cuenta, activo)
                SELECT 'APER-DPF', 'Apertura de depósito a plazo fijo', caja.id, dpf.id, tc.id, 1, true
                FROM contabilidad.cuenta_contable caja, contabilidad.cuenta_contable dpf, contabilidad.tipo_comprobante_contable tc
                WHERE caja.codigo = '1101' AND dpf.codigo = '2103' AND tc.codigo = 'DIA';

                INSERT INTO contabilidad.tipo_transaccion
                    (codigo, nombre, id_cuenta_contable_debito, id_cuenta_contable_credito, id_tipo_comprobante, signo_saldo_cuenta, activo)
                SELECT 'CANC-DPF', 'Cancelación de depósito a plazo fijo', dpf.id, caja.id, tc.id, -1, true
                FROM contabilidad.cuenta_contable caja, contabilidad.cuenta_contable dpf, contabilidad.tipo_comprobante_contable tc
                WHERE caja.codigo = '1101' AND dpf.codigo = '2103' AND tc.codigo = 'DIA';
                """);

            // Tablero de tasas real por rango de plazo (referencia de mercado
            // para cooperativas Segmento 2 — tasas nominales anuales).
            migrationBuilder.Sql(
                """
                INSERT INTO inversion.item_plazo_tasa (id, plazo_dias_min, plazo_dias_max, monto_min, monto_max, tipo_persona, tasa, fecha_vigencia_desde, activo) VALUES
                    (gen_random_uuid(), 30,  89,  50.00, NULL, 'Ambas', 0.0550, '2026-01-01', true),
                    (gen_random_uuid(), 90,  179, 50.00, NULL, 'Ambas', 0.0700, '2026-01-01', true),
                    (gen_random_uuid(), 180, 359, 50.00, NULL, 'Ambas', 0.0850, '2026-01-01', true),
                    (gen_random_uuid(), 360, 720, 50.00, NULL, 'Ambas', 0.1000, '2026-01-01', true);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM inversion.item_plazo_tasa;
                DELETE FROM contabilidad.tipo_transaccion WHERE codigo IN ('APER-DPF', 'CANC-DPF');
                DELETE FROM contabilidad.cuenta_contable WHERE codigo = '2103';
                """);
        }
    }
}
