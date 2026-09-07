using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Planificacion_MotorReal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "area",
                schema: "planificacion",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_area", x => x.codigo);
                });

            migrationBuilder.CreateTable(
                name: "etiqueta",
                schema: "planificacion",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    color_hex = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_etiqueta", x => x.codigo);
                });

            migrationBuilder.CreateTable(
                name: "plan_semanal",
                schema: "planificacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_area = table.Column<string>(type: "character varying(20)", nullable: false),
                    fecha_inicio_semana = table.Column<DateOnly>(type: "date", nullable: false),
                    nombre_responsable = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    cargo_responsable = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    modificado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    modificado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_plan_semanal", x => x.id);
                    table.ForeignKey(
                        name: "fk_plan_semanal_area_codigo_area",
                        column: x => x.codigo_area,
                        principalSchema: "planificacion",
                        principalTable: "area",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "plan_semanal_bloque",
                schema: "planificacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_plan_semanal = table.Column<Guid>(type: "uuid", nullable: false),
                    dia_semana = table.Column<int>(type: "integer", nullable: false),
                    hora_inicio = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    hora_fin = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    codigo_etiqueta = table.Column<string>(type: "character varying(20)", nullable: false),
                    descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_plan_semanal_bloque", x => x.id);
                    table.ForeignKey(
                        name: "fk_plan_semanal_bloque_etiqueta_codigo_etiqueta",
                        column: x => x.codigo_etiqueta,
                        principalSchema: "planificacion",
                        principalTable: "etiqueta",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_plan_semanal_bloque_planes_semanales_id_plan_semanal",
                        column: x => x.id_plan_semanal,
                        principalSchema: "planificacion",
                        principalTable: "plan_semanal",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_plan_semanal_codigo_area_fecha_inicio_semana",
                schema: "planificacion",
                table: "plan_semanal",
                columns: new[] { "codigo_area", "fecha_inicio_semana" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_plan_semanal_bloque_codigo_etiqueta",
                schema: "planificacion",
                table: "plan_semanal_bloque",
                column: "codigo_etiqueta");

            migrationBuilder.CreateIndex(
                name: "ix_plan_semanal_bloque_id_plan_semanal",
                schema: "planificacion",
                table: "plan_semanal_bloque",
                column: "id_plan_semanal");

            migrationBuilder.CreateIndex(
                name: "ix_plan_semanal_creado_por",
                schema: "planificacion",
                table: "plan_semanal",
                column: "creado_por");

            // Menús reales del módulo -- planificacion (rol-gated, no
            // universal: solo quien tenga el rol lo ve) y planificacion-
            // gerencia (visibilidad de solo lectura sobre todas las áreas).
            // Sembrados directo a ADMINISTRADOR; el resto de roles se
            // asigna después desde Configuración → Roles → Permisos.
            migrationBuilder.Sql("""
                INSERT INTO seguridad.menu (codigo, nombre, orden, activo) VALUES
                ('planificacion', 'Planificación', 20, true),
                ('planificacion-gerencia', 'Planificación (Gerencia)', 21, true);

                INSERT INTO seguridad.rol_menu (id_rol, id_menu, activo)
                SELECT r.id, m.id, true
                FROM seguridad.rol r, seguridad.menu m
                WHERE r.nombre = 'ADMINISTRADOR' AND m.codigo IN ('planificacion', 'planificacion-gerencia');
                """);

            // Área real sembrada (la que ya usa el propio Jefe de TI en su
            // planificación de referencia) -- el resto de áreas se agregan
            // desde Configuración cuando cada jefatura empiece a usarlo.
            migrationBuilder.Sql("""
                INSERT INTO planificacion.area (codigo, nombre, activo) VALUES
                ('TI', 'Tecnologías de la Información', true);
                """);

            // Las 7 categorías reales ya usadas en la planificación de
            // referencia (ver PDF del usuario), con colores distintos y
            // reales para pintar cada bloque en la grilla.
            migrationBuilder.Sql("""
                INSERT INTO planificacion.etiqueta (codigo, nombre, color_hex, activo) VALUES
                ('SOPORTE', 'Soporte y continuidad del negocio', '#c9a227', true),
                ('CANALES', 'Proyecto: Canales Digitales', '#16213e', true),
                ('DESARROLLO', 'Desarrollo de Soluciones (bases de datos y programación)', '#64748b', true),
                ('INFRA', 'Infraestructura y respaldos', '#1d4e5f', true),
                ('SEGURIDAD', 'Seguridad y revisión de accesos/roles', '#7a1f2b', true),
                ('DOCUMENTACION', 'Documentación y cumplimiento', '#6b4226', true),
                ('MEJORACONTINUA', 'Planificación y mejora continua', '#a67c1f', true);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM seguridad.rol_menu WHERE id_menu IN (SELECT id FROM seguridad.menu WHERE codigo IN ('planificacion', 'planificacion-gerencia'));
                DELETE FROM seguridad.menu WHERE codigo IN ('planificacion', 'planificacion-gerencia');
                """);

            migrationBuilder.DropTable(
                name: "plan_semanal_bloque",
                schema: "planificacion");

            migrationBuilder.DropTable(
                name: "etiqueta",
                schema: "planificacion");

            migrationBuilder.DropTable(
                name: "plan_semanal",
                schema: "planificacion");

            migrationBuilder.DropTable(
                name: "area",
                schema: "planificacion");
        }
    }
}
