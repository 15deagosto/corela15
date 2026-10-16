using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Planificacion_EntregablesYAsistenteGerencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "disposicion_entregable",
                schema: "planificacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_disposicion_area = table.Column<Guid>(type: "uuid", nullable: false),
                    comentario = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ruta_archivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    nombre_archivo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    content_type = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    tamano_bytes = table.Column<long>(type: "bigint", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_disposicion_entregable", x => x.id);
                    table.ForeignKey(
                        name: "fk_disposicion_entregable_disposicion_gerencia_area_id_disposi",
                        column: x => x.id_disposicion_area,
                        principalSchema: "planificacion",
                        principalTable: "disposicion_gerencia_area",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_disposicion_entregable_id_disposicion_area",
                schema: "planificacion",
                table: "disposicion_entregable",
                column: "id_disposicion_area");

            // Rol real de la asistente de gerencia: mismos permisos de
            // gerencia en Planificación (ver todas las planificaciones,
            // disposiciones y entregables), más el chat interno que tienen
            // todos los roles de empleados. Mesa de Servicio es universal.
            migrationBuilder.Sql("""
                INSERT INTO seguridad.rol (nombre, nivel, dias_cambio_clave, permite_consolidado_cliente)
                VALUES ('ASISTENTE DE GERENCIA', 40, 30, false)
                ON CONFLICT (nombre) DO NOTHING;

                INSERT INTO seguridad.rol_menu (id_rol, id_menu, activo)
                SELECT r.id, m.id, true
                FROM seguridad.rol r, seguridad.menu m
                WHERE r.nombre = 'ASISTENTE DE GERENCIA'
                  AND m.codigo IN ('planificacion', 'planificacion-gerencia', 'comunicacion-interna')
                ON CONFLICT (id_rol, id_menu) DO NOTHING;
                """);

            // Usuario MIZA (Melany Iza, asistente de gerencia) -- no existe en
            // Softbank, se crea acá. Clave temporal = nombre de usuario con
            // cambio obligatorio en el primer ingreso, mismo criterio que los
            // 42 usuarios importados. Agencia 2 = Pilacoto (matriz).
            var hash = BCrypt.Net.BCrypt.HashPassword("MIZA");
            migrationBuilder.Sql($"""
                INSERT INTO seguridad.usuario (id, nombre_usuario, hash_contrasena, id_agencia, puede_ingresar_sistema, tiene_bloqueo, activo,
                    creado_en, creado_por, cambia_clave, dias_cambio_clave, permite_consulta_empleados, permite_riesgo_operativo,
                    usa_dispositivo_movil, valida_ip, nombre_completo)
                SELECT gen_random_uuid(), 'MIZA', '{hash}', 2, true, false, true,
                    now(), 'migracion:asistente-gerencia', true, 30, false, false, false, false, 'MELANY IZA'
                WHERE NOT EXISTS (SELECT 1 FROM seguridad.usuario WHERE nombre_usuario = 'MIZA');

                INSERT INTO seguridad.usuario_rol (id_usuario, id_rol, activo, asignado_en)
                SELECT u.id, r.id, true, now()
                FROM seguridad.usuario u, seguridad.rol r
                WHERE u.nombre_usuario = 'MIZA' AND r.nombre = 'ASISTENTE DE GERENCIA'
                ON CONFLICT (id_usuario, id_rol) DO NOTHING;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // El usuario MIZA y el rol no se borran al revertir: un usuario
            // real con historial de auditoría nunca se elimina, solo se
            // desactiva (mismo criterio de todo el core).
            migrationBuilder.Sql("""
                UPDATE seguridad.usuario SET activo = false, puede_ingresar_sistema = false
                WHERE nombre_usuario = 'MIZA' AND creado_por = 'migracion:asistente-gerencia';
                """);

            migrationBuilder.DropTable(
                name: "disposicion_entregable",
                schema: "planificacion");
        }
    }
}
