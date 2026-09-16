using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Seguridad_TipoEstructuraB11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Segunda estructura real del módulo (ver "Otorgamiento directo
            // por usuario para Estructuras Financieras" — mismo patrón
            // exacto que OF01), calculada en vivo desde Softbank (ver
            // B11Service). Otorgada solo a ADMINISTRADOR por defecto —
            // acceso puntual a otra persona se da por rol o directo, mismo
            // criterio ya establecido para OF01.
            migrationBuilder.Sql("""
                INSERT INTO seguridad.tipo_estructura (codigo, nombre, activo) VALUES ('B11', 'Balance de Comprobación (B11)', true);

                INSERT INTO seguridad.rol_tipo_estructura (id_rol, codigo_tipo_estructura, activo)
                SELECT r.id, 'B11', true FROM seguridad.rol r WHERE r.nombre = 'ADMINISTRADOR';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM seguridad.rol_tipo_estructura WHERE codigo_tipo_estructura = 'B11';
                DELETE FROM seguridad.tipo_estructura WHERE codigo = 'B11';
                """);
        }
    }
}
