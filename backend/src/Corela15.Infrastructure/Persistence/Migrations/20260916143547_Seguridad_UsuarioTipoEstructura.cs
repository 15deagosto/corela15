using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Seguridad_UsuarioTipoEstructura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "usuario_tipo_estructura",
                schema: "seguridad",
                columns: table => new
                {
                    id_usuario = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_tipo_estructura = table.Column<string>(type: "character varying(20)", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    excluido = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuario_tipo_estructura", x => new { x.id_usuario, x.codigo_tipo_estructura });
                    table.ForeignKey(
                        name: "fk_usuario_tipo_estructura_tipo_estructura_codigo_tipo_estruct",
                        column: x => x.codigo_tipo_estructura,
                        principalSchema: "seguridad",
                        principalTable: "tipo_estructura",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_usuario_tipo_estructura_usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalSchema: "seguridad",
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_usuario_tipo_estructura_codigo_tipo_estructura",
                schema: "seguridad",
                table: "usuario_tipo_estructura",
                column: "codigo_tipo_estructura");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "usuario_tipo_estructura",
                schema: "seguridad");
        }
    }
}
