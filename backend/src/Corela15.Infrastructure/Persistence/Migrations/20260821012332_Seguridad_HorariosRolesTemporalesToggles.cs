using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Seguridad_HorariosRolesTemporalesToggles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "cambia_clave",
                schema: "seguridad",
                table: "usuario",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "dias_cambio_clave",
                schema: "seguridad",
                table: "usuario",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "permite_consulta_empleados",
                schema: "seguridad",
                table: "usuario",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "permite_riesgo_operativo",
                schema: "seguridad",
                table: "usuario",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "usa_dispositivo_movil",
                schema: "seguridad",
                table: "usuario",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "valida_ip",
                schema: "seguridad",
                table: "usuario",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "dias_cambio_clave",
                schema: "seguridad",
                table: "rol",
                type: "integer",
                nullable: false,
                defaultValue: 30);

            migrationBuilder.AddColumn<bool>(
                name: "permite_consolidado_cliente",
                schema: "seguridad",
                table: "rol",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "horario_acceso_usuario",
                schema: "seguridad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_usuario = table.Column<Guid>(type: "uuid", nullable: false),
                    dia_semana = table.Column<int>(type: "integer", nullable: false),
                    hora_inicio = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    hora_fin = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    es_receso = table.Column<bool>(type: "boolean", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_horario_acceso_usuario", x => x.id);
                    table.ForeignKey(
                        name: "fk_horario_acceso_usuario_usuarios_id_usuario",
                        column: x => x.id_usuario,
                        principalSchema: "seguridad",
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuario_agencia_temporal",
                schema: "seguridad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_usuario = table.Column<Guid>(type: "uuid", nullable: false),
                    id_agencia_origen = table.Column<int>(type: "integer", nullable: false),
                    id_agencia_actual = table.Column<int>(type: "integer", nullable: false),
                    fecha_caducidad = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuario_agencia_temporal", x => x.id);
                    table.ForeignKey(
                        name: "fk_usuario_agencia_temporal_agencia_id_agencia_actual",
                        column: x => x.id_agencia_actual,
                        principalSchema: "general",
                        principalTable: "agencia",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_usuario_agencia_temporal_agencia_id_agencia_origen",
                        column: x => x.id_agencia_origen,
                        principalSchema: "general",
                        principalTable: "agencia",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_usuario_agencia_temporal_usuarios_id_usuario",
                        column: x => x.id_usuario,
                        principalSchema: "seguridad",
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuario_rol_temporal",
                schema: "seguridad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_usuario = table.Column<Guid>(type: "uuid", nullable: false),
                    id_rol = table.Column<int>(type: "integer", nullable: false),
                    fecha_caducidad = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuario_rol_temporal", x => x.id);
                    table.ForeignKey(
                        name: "fk_usuario_rol_temporal_rol_id_rol",
                        column: x => x.id_rol,
                        principalSchema: "seguridad",
                        principalTable: "rol",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_usuario_rol_temporal_usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalSchema: "seguridad",
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_horario_acceso_usuario_id_usuario_dia_semana",
                schema: "seguridad",
                table: "horario_acceso_usuario",
                columns: new[] { "id_usuario", "dia_semana" });

            migrationBuilder.CreateIndex(
                name: "ix_usuario_agencia_temporal_id_agencia_actual",
                schema: "seguridad",
                table: "usuario_agencia_temporal",
                column: "id_agencia_actual");

            migrationBuilder.CreateIndex(
                name: "ix_usuario_agencia_temporal_id_agencia_origen",
                schema: "seguridad",
                table: "usuario_agencia_temporal",
                column: "id_agencia_origen");

            migrationBuilder.CreateIndex(
                name: "ix_usuario_agencia_temporal_id_usuario_activo",
                schema: "seguridad",
                table: "usuario_agencia_temporal",
                columns: new[] { "id_usuario", "activo" });

            migrationBuilder.CreateIndex(
                name: "ix_usuario_rol_temporal_id_rol",
                schema: "seguridad",
                table: "usuario_rol_temporal",
                column: "id_rol");

            migrationBuilder.CreateIndex(
                name: "ix_usuario_rol_temporal_id_usuario_activo",
                schema: "seguridad",
                table: "usuario_rol_temporal",
                columns: new[] { "id_usuario", "activo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "horario_acceso_usuario",
                schema: "seguridad");

            migrationBuilder.DropTable(
                name: "usuario_agencia_temporal",
                schema: "seguridad");

            migrationBuilder.DropTable(
                name: "usuario_rol_temporal",
                schema: "seguridad");

            migrationBuilder.DropColumn(
                name: "cambia_clave",
                schema: "seguridad",
                table: "usuario");

            migrationBuilder.DropColumn(
                name: "dias_cambio_clave",
                schema: "seguridad",
                table: "usuario");

            migrationBuilder.DropColumn(
                name: "permite_consulta_empleados",
                schema: "seguridad",
                table: "usuario");

            migrationBuilder.DropColumn(
                name: "permite_riesgo_operativo",
                schema: "seguridad",
                table: "usuario");

            migrationBuilder.DropColumn(
                name: "usa_dispositivo_movil",
                schema: "seguridad",
                table: "usuario");

            migrationBuilder.DropColumn(
                name: "valida_ip",
                schema: "seguridad",
                table: "usuario");

            migrationBuilder.DropColumn(
                name: "dias_cambio_clave",
                schema: "seguridad",
                table: "rol");

            migrationBuilder.DropColumn(
                name: "permite_consolidado_cliente",
                schema: "seguridad",
                table: "rol");
        }
    }
}
