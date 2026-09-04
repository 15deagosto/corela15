using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Seguridad_UsuarioCodigoSoftbank : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "codigo_usuario_softbank",
                schema: "seguridad",
                table: "usuario",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_usuario_codigo_usuario_softbank",
                schema: "seguridad",
                table: "usuario",
                column: "codigo_usuario_softbank");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_usuario_codigo_usuario_softbank",
                schema: "seguridad",
                table: "usuario");

            migrationBuilder.DropColumn(
                name: "codigo_usuario_softbank",
                schema: "seguridad",
                table: "usuario");
        }
    }
}
