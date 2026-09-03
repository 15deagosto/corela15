using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Nomina_SueldoActualYDatosPersonaSocio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "nuevo_sueldo",
                schema: "nomina",
                table: "solicitud_accion_personal",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "sueldo_actual",
                schema: "nomina",
                table: "empleado",
                type: "numeric(18,2)",
                nullable: true);

            // Backfill real: el sueldo actual es una caché del último rol de
            // pagos de cada empleado (la fuente real, ver CLAUDE.md) — nunca
            // un valor inventado.
            migrationBuilder.Sql(@"
UPDATE nomina.empleado e SET sueldo_actual = sub.ingresos
FROM (
    SELECT DISTINCT ON (rpe.id_empleado) rpe.id_empleado, rpe.ingresos
    FROM nomina.rol_pagos_empleado rpe
    JOIN nomina.rol_pagos rp ON rp.id = rpe.id_rol_pagos
    WHERE NOT rpe.anulado
    ORDER BY rpe.id_empleado, rp.periodo DESC
) sub
WHERE e.id = sub.id_empleado;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "nuevo_sueldo",
                schema: "nomina",
                table: "solicitud_accion_personal");

            migrationBuilder.DropColumn(
                name: "sueldo_actual",
                schema: "nomina",
                table: "empleado");
        }
    }
}
