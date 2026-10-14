using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Repara el bug real de compartir una subcarpeta (oct-2026): al darle
    /// acceso a alguien a una carpeta que venía heredando permisos, la
    /// herencia se cortaba y el propio dueño (y todos los editores de la
    /// carpeta padre) perdían el acceso. El código ya copia los permisos
    /// heredados antes de cortar; esta migración repara las carpetas que se
    /// compartieron antes del fix: a cada carpeta con ACL propio le agrega
    /// los usuarios con Escritura de su carpeta autoritativa más cercana
    /// hacia arriba (los dueños/editores reales) que le falten. Solo
    /// Escritura -- nunca reabre lecturas que el dueño pudo haber
    /// restringido a propósito. Idempotente.
    /// </summary>
    public partial class Documentos_RepararDuenoCarpetasCompartidas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                WITH RECURSIVE sub AS (
                    SELECT c.id AS carpeta, c.id_carpeta_padre AS anc, 1 AS nivel
                    FROM documentos.carpeta c
                    WHERE c.id_carpeta_padre IS NOT NULL
                      AND EXISTS (SELECT 1 FROM documentos.carpeta_acceso a WHERE a.id_carpeta = c.id)
                    UNION ALL
                    SELECT s.carpeta, p.id_carpeta_padre, s.nivel + 1
                    FROM sub s JOIN documentos.carpeta p ON p.id = s.anc
                    WHERE p.id_carpeta_padre IS NOT NULL
                      AND NOT EXISTS (SELECT 1 FROM documentos.carpeta_acceso a WHERE a.id_carpeta = s.anc)
                      AND s.nivel < 50
                ),
                autoritativa AS (
                    SELECT DISTINCT ON (carpeta) carpeta, anc
                    FROM sub
                    WHERE EXISTS (SELECT 1 FROM documentos.carpeta_acceso a WHERE a.id_carpeta = sub.anc)
                    ORDER BY carpeta, nivel
                )
                INSERT INTO documentos.carpeta_acceso (id, id_carpeta, id_usuario, nivel_acceso, creado_en, creado_por)
                SELECT gen_random_uuid(), au.carpeta, a.id_usuario, 'Escritura', now(), 'migracion:reparar-herencia'
                FROM autoritativa au
                JOIN documentos.carpeta_acceso a ON a.id_carpeta = au.anc AND a.nivel_acceso = 'Escritura'
                WHERE NOT EXISTS (SELECT 1 FROM documentos.carpeta_acceso x WHERE x.id_carpeta = au.carpeta AND x.id_usuario = a.id_usuario);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM documentos.carpeta_acceso WHERE creado_por = 'migracion:reparar-herencia';");
        }
    }
}
