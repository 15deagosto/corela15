using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Seguridad_MenuCredVault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // No es un módulo nativo de Corela15 -- es el ícono de enlace
            // externo a CredVault (bóveda de credenciales, app separada
            // con su propio backend/frontend/base, ver docker-compose.prod.yml
            // en su propio repo). El menú solo controla si el usuario ve el
            // ícono en el sidebar/Inicio; CredVault tiene su propio login y
            // roles internos (Jefe TI/Gerente/Asistente TI/Técnico/Auditor)
            // que gatean el acceso real una vez adentro.
            migrationBuilder.Sql("""
                INSERT INTO seguridad.menu (codigo, nombre, orden, activo) VALUES
                ('credvault', 'CredVault', 22, true);

                INSERT INTO seguridad.rol_menu (id_rol, id_menu, activo)
                SELECT r.id, m.id, true
                FROM seguridad.rol r, seguridad.menu m
                WHERE r.nombre = 'ADMINISTRADOR' AND m.codigo = 'credvault';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM seguridad.rol_menu WHERE id_menu = (SELECT id FROM seguridad.menu WHERE codigo = 'credvault');
                DELETE FROM seguridad.menu WHERE codigo = 'credvault';
                """);
        }
    }
}
