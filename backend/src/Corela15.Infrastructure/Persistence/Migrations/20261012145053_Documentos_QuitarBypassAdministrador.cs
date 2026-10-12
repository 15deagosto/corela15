using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Documentos_QuitarBypassAdministrador : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Pedido explícito del usuario: "así yo sea admin no quiero
            // poder ver todos los datos de todos los usuarios... que cada
            // usuario decida quién ve cada cosa" -- el código ya dejó de
            // dar bypass automático a ADMINISTRADOR (ver DocumentosController.
            // VeTodo), pero ADMINISTRADOR seguía teniendo el menú
            // `biblioteca-documentos-gerencia` otorgado por defecto desde
            // su creación, que igual le daría visibilidad total. Se
            // revoca acá -- solo estado (activo=false), nunca DELETE
            // real, mismo criterio de todo el core. El menú en sí sigue
            // existiendo como capacidad real, por si en el futuro se
            // arma un rol dedicado de auditoría/cumplimiento documental.
            migrationBuilder.Sql("""
                UPDATE seguridad.rol_menu SET activo = false
                WHERE activo = true
                  AND id_rol = (SELECT id FROM seguridad.rol WHERE nombre = 'ADMINISTRADOR')
                  AND id_menu = (SELECT id FROM seguridad.menu WHERE codigo = 'biblioteca-documentos-gerencia');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE seguridad.rol_menu SET activo = true
                WHERE id_rol = (SELECT id FROM seguridad.rol WHERE nombre = 'ADMINISTRADOR')
                  AND id_menu = (SELECT id FROM seguridad.menu WHERE codigo = 'biblioteca-documentos-gerencia');
                """);
        }
    }
}
