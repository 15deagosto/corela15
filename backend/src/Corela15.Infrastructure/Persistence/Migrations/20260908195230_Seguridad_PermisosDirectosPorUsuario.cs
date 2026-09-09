using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Seguridad_PermisosDirectosPorUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "usuario_dataset_reporteria",
                schema: "seguridad",
                columns: table => new
                {
                    id_usuario = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_dataset = table.Column<string>(type: "character varying(30)", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuario_dataset_reporteria", x => new { x.id_usuario, x.codigo_dataset });
                    table.ForeignKey(
                        name: "fk_usuario_dataset_reporteria_dataset_reporteria_codigo_dataset",
                        column: x => x.codigo_dataset,
                        principalSchema: "seguridad",
                        principalTable: "dataset_reporteria",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_usuario_dataset_reporteria_usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalSchema: "seguridad",
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuario_menu",
                schema: "seguridad",
                columns: table => new
                {
                    id_usuario = table.Column<Guid>(type: "uuid", nullable: false),
                    id_menu = table.Column<int>(type: "integer", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuario_menu", x => new { x.id_usuario, x.id_menu });
                    table.ForeignKey(
                        name: "fk_usuario_menu_menu_id_menu",
                        column: x => x.id_menu,
                        principalSchema: "seguridad",
                        principalTable: "menu",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_usuario_menu_usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalSchema: "seguridad",
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_usuario_dataset_reporteria_codigo_dataset",
                schema: "seguridad",
                table: "usuario_dataset_reporteria",
                column: "codigo_dataset");

            migrationBuilder.CreateIndex(
                name: "ix_usuario_menu_id_menu",
                schema: "seguridad",
                table: "usuario_menu",
                column: "id_menu");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "usuario_dataset_reporteria",
                schema: "seguridad");

            migrationBuilder.DropTable(
                name: "usuario_menu",
                schema: "seguridad");
        }
    }
}
