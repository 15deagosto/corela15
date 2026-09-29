using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Documentos_CarpetasPersonales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Raíz real "Carpetas personales" -- una sola vez, idempotente.
            // Cada usuario activo real recibe, debajo, una carpeta propia
            // (Escritura) para que decida él mismo con quién la comparte,
            // sin depender de que un administrador se la arme a mano (ver
            // CarpetaService.AsegurarCarpetaPersonalAsync, que corre el
            // mismo alta desde ahora en cada usuario nuevo que se cree).
            migrationBuilder.Sql("""
                INSERT INTO documentos.carpeta (id, nombre, id_carpeta_padre, activa, creado_en, creado_por)
                SELECT gen_random_uuid(), 'Carpetas personales', NULL, true, now(), 'migracion:carpetas_personales'
                WHERE NOT EXISTS (
                    SELECT 1 FROM documentos.carpeta
                    WHERE nombre = 'Carpetas personales' AND id_carpeta_padre IS NULL AND activa
                );
                """);

            // Lectura sobre la raíz para cada usuario activo -- sin esto su
            // carpeta propia queda huérfana en el árbol del frontend (se
            // arma recorriendo desde las carpetas raíz visibles, no solo
            // por las filas de ACL de la hoja).
            migrationBuilder.Sql("""
                INSERT INTO documentos.carpeta_acceso (id, id_carpeta, id_usuario, nivel_acceso, creado_en, creado_por)
                SELECT gen_random_uuid(), r.id, u.id, 'Lectura', now(), 'migracion:carpetas_personales'
                FROM documentos.carpeta r
                CROSS JOIN seguridad.usuario u
                WHERE r.nombre = 'Carpetas personales' AND r.id_carpeta_padre IS NULL AND r.activa
                  AND u.activo
                ON CONFLICT (id_carpeta, id_usuario) DO NOTHING;
                """);

            // Carpeta personal, una por NOMBRE real distinto (no por
            // usuario) -- hallazgo real durante la prueba: dos usuarios
            // activos pueden compartir el mismo nombre real (dos cuentas
            // reales de Softbank para la misma persona, ej. una cuenta
            // departamental + una personal). Precalcular los nombres
            // distintos en una subconsulta evita el problema real de
            // "INSERT...SELECT no ve sus propias filas recién insertadas
            // dentro de la misma sentencia" -- sin este paso, dos usuarios
            // con el mismo nombre real terminaban con dos carpetas
            // separadas en vez de una compartida (bug real encontrado y
            // corregido en la misma ronda, nunca llegó a desplegarse).
            migrationBuilder.Sql("""
                INSERT INTO documentos.carpeta (id, nombre, id_carpeta_padre, activa, creado_en, creado_por)
                SELECT gen_random_uuid(), nombres.nombre_calculado, r.id, true, now(), 'migracion:carpetas_personales'
                FROM (
                    SELECT DISTINCT COALESCE(p.nombre, u.nombre_completo, u.nombre_usuario) AS nombre_calculado
                    FROM seguridad.usuario u
                    LEFT JOIN sujeto.persona p ON p.id = u.id_persona
                    WHERE u.activo
                ) nombres
                CROSS JOIN documentos.carpeta r
                WHERE r.nombre = 'Carpetas personales' AND r.id_carpeta_padre IS NULL AND r.activa
                  AND NOT EXISTS (
                      SELECT 1 FROM documentos.carpeta c2
                      WHERE c2.id_carpeta_padre = r.id AND c2.nombre = nombres.nombre_calculado AND c2.activa
                  );
                """);

            // Escritura de cada usuario activo sobre SU carpeta personal --
            // si dos usuarios comparten el mismo nombre real, ambos quedan
            // con Escritura sobre la misma carpeta compartida (correcto:
            // son la misma persona real con dos accesos al sistema).
            migrationBuilder.Sql("""
                INSERT INTO documentos.carpeta_acceso (id, id_carpeta, id_usuario, nivel_acceso, creado_en, creado_por)
                SELECT gen_random_uuid(), c.id, u.id, 'Escritura', now(), 'migracion:carpetas_personales'
                FROM seguridad.usuario u
                JOIN documentos.carpeta r ON r.nombre = 'Carpetas personales' AND r.id_carpeta_padre IS NULL AND r.activa
                LEFT JOIN sujeto.persona p ON p.id = u.id_persona
                JOIN documentos.carpeta c ON c.id_carpeta_padre = r.id
                    AND c.nombre = COALESCE(p.nombre, u.nombre_completo, u.nombre_usuario) AND c.activa
                WHERE u.activo
                ON CONFLICT (id_carpeta, id_usuario) DO NOTHING;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM documentos.carpeta_acceso WHERE creado_por = 'migracion:carpetas_personales';
                DELETE FROM documentos.carpeta WHERE creado_por = 'migracion:carpetas_personales';
                """);
        }
    }
}
