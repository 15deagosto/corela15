using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Seguridad_MenuMensajeriaWhatsapp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // A diferencia de Financiero/Proveeduría (solo ADMINISTRADOR), este
            // menú se otorga también a CAJERO desde el día uno -- es el caso de
            // uso real que originó el módulo: una cajera sin acceso a WhatsApp
            // que necesita avisarle algo puntual a un socio.
            migrationBuilder.Sql("""
                INSERT INTO seguridad.menu (codigo, nombre, orden, activo) VALUES
                ('mensajeria-whatsapp', 'Mensajería (WhatsApp)', 24, true);

                INSERT INTO seguridad.rol_menu (id_rol, id_menu, activo)
                SELECT r.id, m.id, true
                FROM seguridad.rol r, seguridad.menu m
                WHERE r.nombre IN ('ADMINISTRADOR', 'CAJERO') AND m.codigo = 'mensajeria-whatsapp';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM seguridad.rol_menu WHERE id_menu = (SELECT id FROM seguridad.menu WHERE codigo = 'mensajeria-whatsapp');
                DELETE FROM seguridad.menu WHERE codigo = 'mensajeria-whatsapp';
                """);
        }
    }
}
