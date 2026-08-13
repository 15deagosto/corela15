using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Contabilidad_SeedCuentaInteresesGanados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Subcuenta de detalle real para el ingreso por intereses de
            // cartera (grupo CUC 51) — necesaria para que el pago de cuota
            // pueda separar capital (reduce 1401) de interés (ingreso 5101).
            migrationBuilder.Sql(
                """
                INSERT INTO contabilidad.cuenta_contable (id, codigo, nombre, grupo, naturaleza, id_cuenta_padre, es_mayor, activa, creado_en, creado_por)
                SELECT gen_random_uuid(), '5101', 'Intereses ganados de cartera de crédito', 'Ingresos', 'Acreedora', p.id, true, true, now(), 'seed:migracion'
                FROM contabilidad.cuenta_contable p WHERE p.codigo = '51';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM contabilidad.cuenta_contable WHERE codigo = '5101';");
        }
    }
}
