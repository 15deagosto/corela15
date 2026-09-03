using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ActivoFijo_ValidacionesReales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_activo_codigo",
                schema: "activofijo",
                table: "activo",
                column: "codigo",
                unique: true,
                filter: "codigo IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_activo_codigo",
                schema: "activofijo",
                table: "activo");
        }
    }
}
