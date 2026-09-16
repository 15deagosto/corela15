using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Seguridad_MenuComunicacionInterna : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Universal (ver AuthService.LoginAsync) -- el catálogo real de
            // menús se siembra igual, por consistencia de sidebar/listados
            // de Configuración, pero nunca se otorga por rol_menu (mismo
            // criterio exacto que "mesa-servicio").
            migrationBuilder.Sql("""
                INSERT INTO seguridad.menu (codigo, nombre, orden, activo) VALUES
                ('comunicacion-interna', 'Comunicación interna', 25, true);
                """);

            // Los 2 canales reales que originaron el módulo -- abiertos desde
            // el día uno a TODOS los usuarios activos de hoy (autoservicio
            // real para cualquier usuario nuevo desde acá en adelante: ver
            // GET /api/comunicacion/canales/descubrir).
            migrationBuilder.Sql("""
                INSERT INTO comunicacion.canal (id, nombre, descripcion, es_directo, activo, creado_por, creado_en)
                VALUES
                    (gen_random_uuid(), 'Cajas', 'Comunicación real entre ventanillas/cajeros', false, true, 'seed:comunicacion_interna', now()),
                    (gen_random_uuid(), 'Balcón de Servicio', 'Comunicación real del balcón de atención al socio', false, true, 'seed:comunicacion_interna', now());

                INSERT INTO comunicacion.canal_miembro (id, id_canal, id_usuario, activo, creado_en)
                SELECT gen_random_uuid(), c.id, u.id, true, now()
                FROM comunicacion.canal c
                CROSS JOIN seguridad.usuario u
                WHERE c.creado_por = 'seed:comunicacion_interna' AND u.activo;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM comunicacion.canal_miembro
                    WHERE id_canal IN (SELECT id FROM comunicacion.canal WHERE creado_por = 'seed:comunicacion_interna');
                DELETE FROM comunicacion.canal WHERE creado_por = 'seed:comunicacion_interna';
                DELETE FROM seguridad.menu WHERE codigo = 'comunicacion-interna';
                """);
        }
    }
}
