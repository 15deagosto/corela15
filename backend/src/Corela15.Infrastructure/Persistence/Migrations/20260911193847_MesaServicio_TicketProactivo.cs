using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MesaServicio_TicketProactivo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "es_proactivo",
                schema: "mesaservicio",
                table: "ticket",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "es_proactivo",
                schema: "mesaservicio",
                table: "ticket");
        }
    }
}
