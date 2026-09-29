using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Documentos_EliminarAreasFijasYAplanarPersonales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Pedido explícito del usuario: nada de catálogo fijo
            // compartido por defecto en Biblioteca de Documentos -- cada
            // usuario ve solo su carpeta personal, la comparte él mismo si
            // quiere. Se elimina el árbol de las 15 áreas originales
            // (Gerencia General, Crédito, Cajas/Ventanilla, etc.) y se
            // aplanan las carpetas personales (antes hijas de un agrupador
            // "Carpetas personales") para que cada una sea su propia raíz.
            //
            // El único documento real que había dentro de las 15 áreas
            // ("Informe renovacion impresoras", agencias/sucursales) se
            // borra acá a nivel de registro -- el archivo físico real en
            // el NAS se borra aparte, por fuera de esta migración (una
            // migración de EF no debe tocar el filesystem/NAS).

            // 1) Documentos, accesos y subcarpetas (a CUALQUIER profundidad
            //    real, no solo 1 nivel -- se encontró en desarrollo local
            //    un caso de 2 niveles de una ronda de pruebas anterior)
            //    dentro de las 15 áreas fijas. En producción son solo 1
            //    documento y 0 subcarpetas (verificado antes de escribir
            //    esto), pero la migración queda robusta para cualquier
            //    ambiente real. Bucle real en vez de un DELETE plano
            //    porque el FK padre->hijo es RESTRICT: hay que borrar
            //    siempre las hojas primero, sin importar cuántos niveles
            //    reales tenga el árbol.
            migrationBuilder.Sql("""
                DO $$
                DECLARE
                    objetivo uuid[];
                BEGIN
                    WITH RECURSIVE objetivo_cte AS (
                        SELECT id FROM documentos.carpeta WHERE id_carpeta_padre IS NULL AND nombre != 'Carpetas personales'
                        UNION ALL
                        SELECT c.id FROM documentos.carpeta c JOIN objetivo_cte o ON c.id_carpeta_padre = o.id
                    )
                    SELECT array_agg(id) INTO objetivo FROM objetivo_cte;

                    IF objetivo IS NOT NULL THEN
                        DELETE FROM documentos.documento WHERE id_carpeta = ANY(objetivo);
                        DELETE FROM documentos.carpeta_acceso WHERE id_carpeta = ANY(objetivo);

                        WHILE EXISTS (SELECT 1 FROM documentos.carpeta WHERE id = ANY(objetivo)) LOOP
                            DELETE FROM documentos.carpeta
                            WHERE id = ANY(objetivo)
                              AND id NOT IN (
                                  SELECT id_carpeta_padre FROM documentos.carpeta
                                  WHERE id_carpeta_padre IS NOT NULL
                              );
                        END LOOP;
                    END IF;
                END $$;
                """);

            // 2) Aplanar: las carpetas personales (hijas de "Carpetas
            //    personales") pasan a ser raíz ellas mismas.
            migrationBuilder.Sql("""
                UPDATE documentos.carpeta
                SET id_carpeta_padre = NULL
                WHERE id_carpeta_padre = (
                    SELECT id FROM documentos.carpeta
                    WHERE nombre = 'Carpetas personales' AND id_carpeta_padre IS NULL
                );
                """);

            // 3) El acceso de Lectura sobre la raíz "Carpetas personales"
            //    ya no tiene sentido -- cada carpeta personal es su propia
            //    raíz ahora, visible por su propio ACL directo.
            migrationBuilder.Sql("""
                DELETE FROM documentos.carpeta_acceso
                WHERE id_carpeta = (
                    SELECT id FROM documentos.carpeta
                    WHERE nombre = 'Carpetas personales' AND id_carpeta_padre IS NULL
                );
                """);

            // 4) Borrar la raíz "Carpetas personales" -- las 15 áreas fijas
            //    ya se borraron en el paso 1 (el DO block de arriba las
            //    incluye directo, no solo a sus descendientes).
            migrationBuilder.Sql("""
                DELETE FROM documentos.carpeta
                WHERE id_carpeta_padre IS NULL AND nombre = 'Carpetas personales';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Irreversible a propósito -- borra un documento real (y su
            // archivo físico, aparte) y aplana la jerarquía original.
            // Nunca simular una reversión que no puede devolver lo borrado.
        }
    }
}
