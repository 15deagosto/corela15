using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Seguridad_ExclusionesDirectasPorUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "excluido",
                schema: "seguridad",
                table: "usuario_opcion",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "excluido",
                schema: "seguridad",
                table: "usuario_menu",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "excluido",
                schema: "seguridad",
                table: "usuario_dataset_reporteria",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "excluido",
                schema: "seguridad",
                table: "usuario_opcion");

            migrationBuilder.DropColumn(
                name: "excluido",
                schema: "seguridad",
                table: "usuario_menu");

            migrationBuilder.DropColumn(
                name: "excluido",
                schema: "seguridad",
                table: "usuario_dataset_reporteria");
        }
    }
}
