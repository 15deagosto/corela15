using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MesaServicio_Ticket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "mesaservicio");

            migrationBuilder.CreateTable(
                name: "categoria_incidencia",
                schema: "mesaservicio",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categoria_incidencia", x => x.codigo);
                });

            migrationBuilder.CreateTable(
                name: "estado_ticket",
                schema: "mesaservicio",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_estado_ticket", x => x.codigo);
                });

            migrationBuilder.CreateTable(
                name: "prioridad_ticket",
                schema: "mesaservicio",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    horas_sla = table.Column<int>(type: "integer", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_prioridad_ticket", x => x.codigo);
                });

            migrationBuilder.CreateTable(
                name: "ticket",
                schema: "mesaservicio",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    codigo_categoria = table.Column<string>(type: "character varying(20)", nullable: false),
                    codigo_prioridad = table.Column<string>(type: "character varying(20)", nullable: false),
                    codigo_estado = table.Column<string>(type: "character varying(20)", nullable: false),
                    id_agencia = table.Column<int>(type: "integer", nullable: false),
                    id_usuario_asignado = table.Column<Guid>(type: "uuid", nullable: true),
                    fecha_limite_sla = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    fecha_cierre = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    modificado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    modificado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ticket", x => x.id);
                    table.ForeignKey(
                        name: "fk_ticket_agencia_id_agencia",
                        column: x => x.id_agencia,
                        principalSchema: "general",
                        principalTable: "agencia",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_ticket_categoria_incidencia_codigo_categoria",
                        column: x => x.codigo_categoria,
                        principalSchema: "mesaservicio",
                        principalTable: "categoria_incidencia",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_ticket_estado_ticket_codigo_estado",
                        column: x => x.codigo_estado,
                        principalSchema: "mesaservicio",
                        principalTable: "estado_ticket",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_ticket_prioridad_ticket_codigo_prioridad",
                        column: x => x.codigo_prioridad,
                        principalSchema: "mesaservicio",
                        principalTable: "prioridad_ticket",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_ticket_usuarios_id_usuario_asignado",
                        column: x => x.id_usuario_asignado,
                        principalSchema: "seguridad",
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ticket_comentario",
                schema: "mesaservicio",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_ticket = table.Column<Guid>(type: "uuid", nullable: false),
                    comentario = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    registrado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    fecha = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ticket_comentario", x => x.id);
                    table.ForeignKey(
                        name: "fk_ticket_comentario_tickets_id_ticket",
                        column: x => x.id_ticket,
                        principalSchema: "mesaservicio",
                        principalTable: "ticket",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ticket_etapa_hist",
                schema: "mesaservicio",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_ticket = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_estado_anterior = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    codigo_estado_nuevo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    comentario = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    registrado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    fecha = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ticket_etapa_hist", x => x.id);
                    table.ForeignKey(
                        name: "fk_ticket_etapa_hist_ticket_id_ticket",
                        column: x => x.id_ticket,
                        principalSchema: "mesaservicio",
                        principalTable: "ticket",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_ticket_codigo_categoria",
                schema: "mesaservicio",
                table: "ticket",
                column: "codigo_categoria");

            migrationBuilder.CreateIndex(
                name: "ix_ticket_codigo_estado",
                schema: "mesaservicio",
                table: "ticket",
                column: "codigo_estado");

            migrationBuilder.CreateIndex(
                name: "ix_ticket_codigo_prioridad",
                schema: "mesaservicio",
                table: "ticket",
                column: "codigo_prioridad");

            migrationBuilder.CreateIndex(
                name: "ix_ticket_id_agencia",
                schema: "mesaservicio",
                table: "ticket",
                column: "id_agencia");

            migrationBuilder.CreateIndex(
                name: "ix_ticket_id_usuario_asignado",
                schema: "mesaservicio",
                table: "ticket",
                column: "id_usuario_asignado");

            migrationBuilder.CreateIndex(
                name: "ix_ticket_numero",
                schema: "mesaservicio",
                table: "ticket",
                column: "numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ticket_comentario_id_ticket",
                schema: "mesaservicio",
                table: "ticket_comentario",
                column: "id_ticket");

            migrationBuilder.CreateIndex(
                name: "ix_ticket_etapa_hist_id_ticket",
                schema: "mesaservicio",
                table: "ticket_etapa_hist",
                column: "id_ticket");

            // Catálogos reales de Mesa de Servicio — control de incidencias
            // exigido por la SEPS. Estado fijo (ciclo de vida del ticket, no
            // editable desde la app); prioridad y categoría sí editables
            // desde Configuración.
            migrationBuilder.Sql(@"
                INSERT INTO mesaservicio.estado_ticket (codigo, nombre, activo) VALUES
                    ('ABIERTO', 'Abierto', true),
                    ('EN_PROGRESO', 'En progreso', true),
                    ('ESPERANDO_USUARIO', 'Esperando respuesta del usuario', true),
                    ('RESUELTO', 'Resuelto', true),
                    ('CERRADO', 'Cerrado', true),
                    ('CANCELADO', 'Cancelado', true);

                INSERT INTO mesaservicio.prioridad_ticket (codigo, nombre, horas_sla, activo) VALUES
                    ('BAJA', 'Baja', 72, true),
                    ('MEDIA', 'Media', 24, true),
                    ('ALTA', 'Alta', 8, true),
                    ('CRITICA', 'Crítica', 2, true);

                INSERT INTO mesaservicio.categoria_incidencia (codigo, nombre, activo) VALUES
                    ('FALLA_SISTEMA', 'Falla del sistema / aplicación', true),
                    ('ACCESO', 'Solicitud de acceso o permisos', true),
                    ('DATOS', 'Error o inconsistencia de datos', true),
                    ('SEGURIDAD', 'Incidente de seguridad', true),
                    ('INFRAESTRUCTURA', 'Infraestructura / red / hardware', true),
                    ('CONSULTA', 'Consulta general', true),
                    ('OTRO', 'Otro', true);

                -- Registro en el catálogo real de menús, por consistencia
                -- (sidebar, listados de Configuración) — el acceso en sí NO
                -- depende de rol_menu para este menú puntual (ver
                -- AuthService.LoginAsync: se agrega siempre, transversal a
                -- todo usuario autenticado, sin excepción por rol).
                INSERT INTO seguridad.menu (codigo, nombre, orden, activo) VALUES
                    ('mesa-servicio', 'Mesa de Servicio', 17, true);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM mesaservicio.estado_ticket;
                DELETE FROM mesaservicio.prioridad_ticket;
                DELETE FROM mesaservicio.categoria_incidencia;
                DELETE FROM seguridad.menu WHERE codigo = 'mesa-servicio';
            ");

            migrationBuilder.DropTable(
                name: "ticket_comentario",
                schema: "mesaservicio");

            migrationBuilder.DropTable(
                name: "ticket_etapa_hist",
                schema: "mesaservicio");

            migrationBuilder.DropTable(
                name: "ticket",
                schema: "mesaservicio");

            migrationBuilder.DropTable(
                name: "categoria_incidencia",
                schema: "mesaservicio");

            migrationBuilder.DropTable(
                name: "estado_ticket",
                schema: "mesaservicio");

            migrationBuilder.DropTable(
                name: "prioridad_ticket",
                schema: "mesaservicio");
        }
    }
}
