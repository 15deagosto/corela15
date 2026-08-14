using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Nivel8_SeedProcesos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Macroprocesos/procesos de EJEMPLO (nombres genéricos reales de
            // una cooperativa, no un levantamiento formal) para que el
            // registro de evento de riesgo tenga sobre qué proceso aplicarse
            // — mismo espíritu que los seeds de socios/empleados.
            migrationBuilder.Sql(
                """
                INSERT INTO riesgo.macroproceso (id, nombre, activo) VALUES
                    (1, 'Captaciones', true),
                    (2, 'Colocación de crédito', true),
                    (3, 'Tecnología de la información', true);

                INSERT INTO riesgo.proceso (id, id_macro_proceso, nombre, critico, activo) VALUES
                    (1, 1, 'Apertura de cuentas de ahorro', false, true),
                    (2, 2, 'Otorgamiento de crédito', true, true),
                    (3, 2, 'Recuperación de cartera', true, true),
                    (4, 3, 'Respaldo y continuidad de datos', true, true);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM riesgo.proceso WHERE id IN (1, 2, 3, 4);
                DELETE FROM riesgo.macroproceso WHERE id IN (1, 2, 3);
                """);
        }
    }
}
