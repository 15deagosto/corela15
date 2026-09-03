using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Colocacion_IndicesRealesReporteria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_prestamo_rubro_id_prestamo_estado_fecha_fin",
                schema: "colocacion",
                table: "prestamo_rubro",
                columns: new[] { "id_prestamo", "estado", "fecha_fin" });

            migrationBuilder.CreateIndex(
                name: "ix_prestamo_codigo_usuario_asesor",
                schema: "colocacion",
                table: "prestamo",
                column: "codigo_usuario_asesor");

            migrationBuilder.CreateIndex(
                name: "ix_prestamo_estado",
                schema: "colocacion",
                table: "prestamo",
                column: "estado");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_prestamo_rubro_id_prestamo_estado_fecha_fin",
                schema: "colocacion",
                table: "prestamo_rubro");

            migrationBuilder.DropIndex(
                name: "ix_prestamo_codigo_usuario_asesor",
                schema: "colocacion",
                table: "prestamo");

            migrationBuilder.DropIndex(
                name: "ix_prestamo_estado",
                schema: "colocacion",
                table: "prestamo");
        }
    }
}
