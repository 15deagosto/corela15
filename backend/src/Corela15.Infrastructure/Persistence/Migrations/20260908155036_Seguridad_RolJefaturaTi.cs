using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Seguridad_RolJefaturaTi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rol real pedido explícitamente por el usuario para poder
            // otorgar el ícono de CredVault (bóveda de credenciales) a la
            // jefatura de TI sin que dependan de ser ADMINISTRADOR --
            // arranca solo con el menú credvault otorgado; se le pueden
            // sumar más menús después desde Configuración → Roles →
            // Permisos, sin migración nueva.
            migrationBuilder.Sql("""
                INSERT INTO seguridad.rol (nombre, nivel) VALUES ('JEFATURA DE TI', 50);

                INSERT INTO seguridad.rol_menu (id_rol, id_menu, activo)
                SELECT r.id, m.id, true
                FROM seguridad.rol r, seguridad.menu m
                WHERE r.nombre = 'JEFATURA DE TI' AND m.codigo = 'credvault';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM seguridad.rol_menu WHERE id_rol = (SELECT id FROM seguridad.rol WHERE nombre = 'JEFATURA DE TI');
                DELETE FROM seguridad.rol WHERE nombre = 'JEFATURA DE TI';
                """);
        }
    }
}
