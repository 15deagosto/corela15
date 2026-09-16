using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Seguridad_ComunicacionInternaPorPermiso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Corrige un error real: "comunicacion-interna" se había hecho
            // universal (mismo trato que "mesa-servicio"), pero solo Mesa de
            // Servicio es transversal por requisito SEPS — el resto de
            // módulos, incluido este chat, se maneja con permisos reales.
            // Otorgado por rol a los roles reales de empleados (no al rol
            // externo GENERADOR OF01, que por diseño queda acotado a Mesa de
            // Servicio + su estructura, mismo criterio ya documentado para
            // los usuarios importados de Softbank sin rol todavía).
            migrationBuilder.Sql("""
                INSERT INTO seguridad.rol_menu (id_rol, id_menu, activo)
                SELECT r.id, m.id, true
                FROM seguridad.rol r, seguridad.menu m
                WHERE r.nombre IN ('ADMINISTRADOR', 'CAJERO', 'ASESOR DE CREDITO', 'OFICIAL DE CAPTACIONES', 'JEFATURA DE TI')
                  AND m.codigo = 'comunicacion-interna'
                ON CONFLICT (id_rol, id_menu) DO NOTHING;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM seguridad.rol_menu
                WHERE id_menu = (SELECT id FROM seguridad.menu WHERE codigo = 'comunicacion-interna')
                  AND id_rol IN (SELECT id FROM seguridad.rol WHERE nombre IN ('ADMINISTRADOR', 'CAJERO', 'ASESOR DE CREDITO', 'OFICIAL DE CAPTACIONES', 'JEFATURA DE TI'));
                """);
        }
    }
}
