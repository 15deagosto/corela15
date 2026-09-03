using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Seguridad_AccionUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "accion_usuario",
                schema: "seguridad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_accion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    usuario_registro = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    fecha = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_accion_usuario", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_accion_usuario_fecha",
                schema: "seguridad",
                table: "accion_usuario",
                column: "fecha");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "accion_usuario",
                schema: "seguridad");
        }
    }
}
