using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Seguridad_SolicitudIdempotente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "solicitud_idempotente",
                schema: "seguridad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    clave = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    id_usuario = table.Column<Guid>(type: "uuid", nullable: false),
                    ruta = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    codigo_estado = table.Column<int>(type: "integer", nullable: false),
                    cuerpo_respuesta = table.Column<string>(type: "jsonb", nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_solicitud_idempotente", x => x.id);
                    table.ForeignKey(
                        name: "fk_solicitud_idempotente_usuarios_id_usuario",
                        column: x => x.id_usuario,
                        principalSchema: "seguridad",
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_solicitud_idempotente_clave_id_usuario_ruta",
                schema: "seguridad",
                table: "solicitud_idempotente",
                columns: new[] { "clave", "id_usuario", "ruta" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_solicitud_idempotente_id_usuario",
                schema: "seguridad",
                table: "solicitud_idempotente",
                column: "id_usuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "solicitud_idempotente",
                schema: "seguridad");
        }
    }
}
