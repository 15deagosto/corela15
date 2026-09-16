using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Seguridad_OpcionesBibliotecaDocumentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Tercer nivel de permiso (ver "Opción" en Créditos) aplicado a
            // Biblioteca de Documentos -- pedido explícito del usuario: el
            // menú da lectura/descarga a cualquiera con acceso al módulo,
            // pero cada acción de escritura (y la matriz de cobertura, que
            // expone info de TODAS las áreas a la vez) se otorga aparte.
            migrationBuilder.Sql("""
                INSERT INTO seguridad.opcion (codigo, nombre, codigo_menu, activo) VALUES
                ('biblioteca-documentos.crear', 'Subir documento nuevo', 'biblioteca-documentos', true),
                ('biblioteca-documentos.editar', 'Editar metadatos de un documento', 'biblioteca-documentos', true),
                ('biblioteca-documentos.nueva-version', 'Subir nueva versión de un documento', 'biblioteca-documentos', true),
                ('biblioteca-documentos.desactivar', 'Desactivar un documento', 'biblioteca-documentos', true),
                ('biblioteca-documentos.matriz', 'Ver matriz de cobertura documental (todas las áreas)', 'biblioteca-documentos', true);

                INSERT INTO seguridad.rol_opcion (id_rol, codigo_opcion, activo)
                SELECT r.id, o.codigo, true
                FROM seguridad.rol r, seguridad.opcion o
                WHERE r.nombre = 'ADMINISTRADOR' AND o.codigo_menu = 'biblioteca-documentos';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM seguridad.rol_opcion WHERE codigo_opcion LIKE 'biblioteca-documentos.%';
                DELETE FROM seguridad.opcion WHERE codigo_menu = 'biblioteca-documentos';
                """);
        }
    }
}
