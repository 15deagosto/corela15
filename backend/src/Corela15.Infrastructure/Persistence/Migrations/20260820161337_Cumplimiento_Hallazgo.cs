using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Cumplimiento_Hallazgo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "cumplimiento");

            migrationBuilder.CreateTable(
                name: "estado_hallazgo",
                schema: "cumplimiento",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_estado_hallazgo", x => x.codigo);
                });

            // Datos reales verificados contra CUMPLIMIENTO.ESTADO_HALLAZGO.
            // El código "111" de la fuente real (NOMBRE = nombre de una
            // persona, dato corrupto, ACTIVO=false) se excluyó a propósito
            // — mismo criterio que la anomalía "671" del catálogo CUC.
            migrationBuilder.Sql(@"
INSERT INTO cumplimiento.estado_hallazgo (codigo, nombre, activo) VALUES
('IN', 'Ingresada', true),
('PR', 'Procesada', true),
('FI', 'Finalizada', true);
");

            migrationBuilder.CreateTable(
                name: "hallazgo",
                schema: "cumplimiento",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    detalle = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    id_usuario_reporta = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_estado = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hallazgo", x => x.id);
                    table.ForeignKey(
                        name: "fk_hallazgo_estado_hallazgo_codigo_estado",
                        column: x => x.codigo_estado,
                        principalSchema: "cumplimiento",
                        principalTable: "estado_hallazgo",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_hallazgo_usuarios_id_usuario_reporta",
                        column: x => x.id_usuario_reporta,
                        principalSchema: "seguridad",
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hallazgo_etapa",
                schema: "cumplimiento",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_hallazgo = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_estado = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    comentario = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    registrado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    fecha = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hallazgo_etapa", x => x.id);
                    table.ForeignKey(
                        name: "fk_hallazgo_etapa_estado_hallazgo_codigo_estado",
                        column: x => x.codigo_estado,
                        principalSchema: "cumplimiento",
                        principalTable: "estado_hallazgo",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_hallazgo_etapa_hallazgo_id_hallazgo",
                        column: x => x.id_hallazgo,
                        principalSchema: "cumplimiento",
                        principalTable: "hallazgo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hallazgo_usuario",
                schema: "cumplimiento",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_hallazgo = table.Column<Guid>(type: "uuid", nullable: false),
                    id_usuario = table.Column<Guid>(type: "uuid", nullable: false),
                    estado_respondido = table.Column<bool>(type: "boolean", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hallazgo_usuario", x => x.id);
                    table.ForeignKey(
                        name: "fk_hallazgo_usuario_hallazgo_id_hallazgo",
                        column: x => x.id_hallazgo,
                        principalSchema: "cumplimiento",
                        principalTable: "hallazgo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_hallazgo_usuario_usuarios_id_usuario",
                        column: x => x.id_usuario,
                        principalSchema: "seguridad",
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hallazgo_usuario_respuesta",
                schema: "cumplimiento",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_hallazgo_usuario = table.Column<Guid>(type: "uuid", nullable: false),
                    respuesta = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    fecha = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hallazgo_usuario_respuesta", x => x.id);
                    table.ForeignKey(
                        name: "fk_hallazgo_usuario_respuesta_hallazgo_usuario_id_hallazgo_usu",
                        column: x => x.id_hallazgo_usuario,
                        principalSchema: "cumplimiento",
                        principalTable: "hallazgo_usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_hallazgo_codigo_estado",
                schema: "cumplimiento",
                table: "hallazgo",
                column: "codigo_estado");

            migrationBuilder.CreateIndex(
                name: "ix_hallazgo_id_usuario_reporta",
                schema: "cumplimiento",
                table: "hallazgo",
                column: "id_usuario_reporta");

            migrationBuilder.CreateIndex(
                name: "ix_hallazgo_etapa_codigo_estado",
                schema: "cumplimiento",
                table: "hallazgo_etapa",
                column: "codigo_estado");

            migrationBuilder.CreateIndex(
                name: "ix_hallazgo_etapa_id_hallazgo",
                schema: "cumplimiento",
                table: "hallazgo_etapa",
                column: "id_hallazgo");

            migrationBuilder.CreateIndex(
                name: "ix_hallazgo_usuario_id_hallazgo",
                schema: "cumplimiento",
                table: "hallazgo_usuario",
                column: "id_hallazgo");

            migrationBuilder.CreateIndex(
                name: "ix_hallazgo_usuario_id_usuario",
                schema: "cumplimiento",
                table: "hallazgo_usuario",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "ix_hallazgo_usuario_respuesta_id_hallazgo_usuario",
                schema: "cumplimiento",
                table: "hallazgo_usuario_respuesta",
                column: "id_hallazgo_usuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hallazgo_etapa",
                schema: "cumplimiento");

            migrationBuilder.DropTable(
                name: "hallazgo_usuario_respuesta",
                schema: "cumplimiento");

            migrationBuilder.DropTable(
                name: "hallazgo_usuario",
                schema: "cumplimiento");

            migrationBuilder.DropTable(
                name: "hallazgo",
                schema: "cumplimiento");

            migrationBuilder.DropTable(
                name: "estado_hallazgo",
                schema: "cumplimiento");
        }
    }
}
