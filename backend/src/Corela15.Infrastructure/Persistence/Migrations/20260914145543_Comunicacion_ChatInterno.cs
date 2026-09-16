using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Comunicacion_ChatInterno : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "comunicacion");

            migrationBuilder.CreateTable(
                name: "canal",
                schema: "comunicacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    descripcion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    es_directo = table.Column<bool>(type: "boolean", nullable: false),
                    clave_directa = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    creado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_canal", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "canal_miembro",
                schema: "comunicacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_canal = table.Column<Guid>(type: "uuid", nullable: false),
                    id_usuario = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha_ultima_lectura = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_canal_miembro", x => x.id);
                    table.ForeignKey(
                        name: "fk_canal_miembro_canal_id_canal",
                        column: x => x.id_canal,
                        principalSchema: "comunicacion",
                        principalTable: "canal",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_canal_miembro_usuarios_id_usuario",
                        column: x => x.id_usuario,
                        principalSchema: "seguridad",
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "mensaje",
                schema: "comunicacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_canal = table.Column<Guid>(type: "uuid", nullable: false),
                    id_usuario_remitente = table.Column<Guid>(type: "uuid", nullable: false),
                    texto = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mensaje", x => x.id);
                    table.ForeignKey(
                        name: "fk_mensaje_canal_id_canal",
                        column: x => x.id_canal,
                        principalSchema: "comunicacion",
                        principalTable: "canal",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_mensaje_usuarios_id_usuario_remitente",
                        column: x => x.id_usuario_remitente,
                        principalSchema: "seguridad",
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_canal_clave_directa",
                schema: "comunicacion",
                table: "canal",
                column: "clave_directa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_canal_miembro_id_canal_id_usuario",
                schema: "comunicacion",
                table: "canal_miembro",
                columns: new[] { "id_canal", "id_usuario" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_canal_miembro_id_usuario",
                schema: "comunicacion",
                table: "canal_miembro",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "ix_mensaje_id_canal_creado_en",
                schema: "comunicacion",
                table: "mensaje",
                columns: new[] { "id_canal", "creado_en" });

            migrationBuilder.CreateIndex(
                name: "ix_mensaje_id_usuario_remitente",
                schema: "comunicacion",
                table: "mensaje",
                column: "id_usuario_remitente");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "canal_miembro",
                schema: "comunicacion");

            migrationBuilder.DropTable(
                name: "mensaje",
                schema: "comunicacion");

            migrationBuilder.DropTable(
                name: "canal",
                schema: "comunicacion");
        }
    }
}
