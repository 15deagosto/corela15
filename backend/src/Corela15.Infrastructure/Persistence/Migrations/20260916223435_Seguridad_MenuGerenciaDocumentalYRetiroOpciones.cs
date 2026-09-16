using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Seguridad_MenuGerenciaDocumentalYRetiroOpciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Reemplaza el modelo de Opciones globales (crear/editar/nueva-
            // versión/desactivar/matriz) por el ACL real por área
            // (AreaAccesoUsuario) -- pedido explícito del usuario ("como TI
            // no quiero que alguien más vea lo que subo ahí"): las Opciones
            // globales no distinguían por área, así que quien tuviera
            // "crear" podía crear en CUALQUIER área. El cascade de
            // rol_opcion/usuario_opcion ya está declarado con ON DELETE
            // CASCADE sobre opcion.codigo, así que borrar acá las 5 filas
            // limpia también sus otorgamientos.
            migrationBuilder.Sql("""
                DELETE FROM seguridad.opcion WHERE codigo_menu = 'biblioteca-documentos';

                INSERT INTO seguridad.menu (codigo, nombre, orden, activo) VALUES
                ('biblioteca-documentos-gerencia', 'Biblioteca de Documentos (ver todas las áreas)', 27, true);

                INSERT INTO seguridad.rol_menu (id_rol, id_menu, activo)
                SELECT r.id, m.id, true
                FROM seguridad.rol r, seguridad.menu m
                WHERE r.nombre = 'ADMINISTRADOR' AND m.codigo = 'biblioteca-documentos-gerencia';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM seguridad.rol_menu WHERE id_menu = (SELECT id FROM seguridad.menu WHERE codigo = 'biblioteca-documentos-gerencia');
                DELETE FROM seguridad.menu WHERE codigo = 'biblioteca-documentos-gerencia';
                """);
        }
    }
}
