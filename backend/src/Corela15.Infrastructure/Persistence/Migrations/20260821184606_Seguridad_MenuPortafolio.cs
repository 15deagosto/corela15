using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Seguridad_MenuPortafolio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO seguridad.menu (codigo, nombre, orden, activo) VALUES
                ('portafolio', 'Portafolio', 13, true);

                INSERT INTO seguridad.rol_menu (id_rol, id_menu, activo)
                SELECT r.id, m.id, true
                FROM seguridad.rol r, seguridad.menu m
                WHERE r.nombre = 'ADMINISTRADOR' AND m.codigo = 'portafolio';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM seguridad.rol_menu WHERE id_menu = (SELECT id FROM seguridad.menu WHERE codigo = 'portafolio');
                DELETE FROM seguridad.menu WHERE codigo = 'portafolio';
                """);
        }
    }
}
