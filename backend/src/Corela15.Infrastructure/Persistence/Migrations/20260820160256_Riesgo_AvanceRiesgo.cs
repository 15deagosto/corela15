using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Riesgo_AvanceRiesgo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "estado_avance_riesgo",
                schema: "riesgo",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_estado_avance_riesgo", x => x.codigo);
                });

            // Datos reales verificados contra RIESGOOPERATIVO.ESTADO_AVANCERIESGO
            // (6 filas reales). "AP" (Aprobada) figura ACTIVO=false en la
            // fuente real — se respeta tal cual, es un código en desuso en
            // Softbank, no un error de transcripción.
            migrationBuilder.Sql(@"
INSERT INTO riesgo.estado_avance_riesgo (codigo, nombre, activo) VALUES
('PRE', 'Preingresada', true),
('I', 'Ingresada', true),
('IN', 'Enviada a revisión', true),
('PR', 'Procesada', true),
('AN', 'Anulada', true),
('AP', 'Aprobada', false);
");

            migrationBuilder.CreateTable(
                name: "avance_riesgo",
                schema: "riesgo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_evento_riesgo = table.Column<Guid>(type: "uuid", nullable: false),
                    id_usuario_responsable = table.Column<Guid>(type: "uuid", nullable: false),
                    hora_inicio = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    hora_fin = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    codigo_estado = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_avance_riesgo", x => x.id);
                    table.ForeignKey(
                        name: "fk_avance_riesgo_estado_avance_riesgo_codigo_estado",
                        column: x => x.codigo_estado,
                        principalSchema: "riesgo",
                        principalTable: "estado_avance_riesgo",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_avance_riesgo_eventos_riesgo_id_evento_riesgo",
                        column: x => x.id_evento_riesgo,
                        principalSchema: "riesgo",
                        principalTable: "evento_riesgo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_avance_riesgo_usuarios_id_usuario_responsable",
                        column: x => x.id_usuario_responsable,
                        principalSchema: "seguridad",
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "avance_riesgo_detalle",
                schema: "riesgo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_avance_riesgo = table.Column<Guid>(type: "uuid", nullable: false),
                    evento_detectado = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    posible_causa = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    tratamiento = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    inconvenientes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    acciones_sugeridas = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_avance_riesgo_detalle", x => x.id);
                    table.ForeignKey(
                        name: "fk_avance_riesgo_detalle_avance_riesgo_id_avance_riesgo",
                        column: x => x.id_avance_riesgo,
                        principalSchema: "riesgo",
                        principalTable: "avance_riesgo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "avance_riesgo_etapa",
                schema: "riesgo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_avance_riesgo_detalle = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_estado = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    comentario = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    registrado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    fecha = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_avance_riesgo_etapa", x => x.id);
                    table.ForeignKey(
                        name: "fk_avance_riesgo_etapa_avance_riesgo_detalle_id_avance_riesgo_",
                        column: x => x.id_avance_riesgo_detalle,
                        principalSchema: "riesgo",
                        principalTable: "avance_riesgo_detalle",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_avance_riesgo_etapa_estado_avance_riesgo_codigo_estado",
                        column: x => x.codigo_estado,
                        principalSchema: "riesgo",
                        principalTable: "estado_avance_riesgo",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_avance_riesgo_codigo_estado",
                schema: "riesgo",
                table: "avance_riesgo",
                column: "codigo_estado");

            migrationBuilder.CreateIndex(
                name: "ix_avance_riesgo_id_evento_riesgo",
                schema: "riesgo",
                table: "avance_riesgo",
                column: "id_evento_riesgo");

            migrationBuilder.CreateIndex(
                name: "ix_avance_riesgo_id_usuario_responsable",
                schema: "riesgo",
                table: "avance_riesgo",
                column: "id_usuario_responsable");

            migrationBuilder.CreateIndex(
                name: "ix_avance_riesgo_detalle_id_avance_riesgo",
                schema: "riesgo",
                table: "avance_riesgo_detalle",
                column: "id_avance_riesgo");

            migrationBuilder.CreateIndex(
                name: "ix_avance_riesgo_etapa_codigo_estado",
                schema: "riesgo",
                table: "avance_riesgo_etapa",
                column: "codigo_estado");

            migrationBuilder.CreateIndex(
                name: "ix_avance_riesgo_etapa_id_avance_riesgo_detalle",
                schema: "riesgo",
                table: "avance_riesgo_etapa",
                column: "id_avance_riesgo_detalle");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "avance_riesgo_etapa",
                schema: "riesgo");

            migrationBuilder.DropTable(
                name: "avance_riesgo_detalle",
                schema: "riesgo");

            migrationBuilder.DropTable(
                name: "avance_riesgo",
                schema: "riesgo");

            migrationBuilder.DropTable(
                name: "estado_avance_riesgo",
                schema: "riesgo");
        }
    }
}
