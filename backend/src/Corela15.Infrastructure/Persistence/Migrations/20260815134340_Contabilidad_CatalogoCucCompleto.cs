using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    /// <summary>
    /// Siembra el Catálogo Único de Cuentas oficial completo (1.130 cuentas
    /// nuevas, ~1.192 en total con las ya sembradas) desde
    /// Persistence/Seeds/CatalogoCucCompleto.sql — generado a partir de
    /// "Catálogo-B11-y-B13.xlsx", provisto por el usuario y descargado
    /// directamente de la página oficial de manuales técnicos de SEPS.
    /// Ver CLAUDE.md, sección "Catálogo de cuentas oficial completo (CUC)",
    /// para la metodología completa (jerarquía, herencia de naturaleza,
    /// detección de hojas) y las cuentas ya sembradas que NO se tocan.
    /// </summary>
    public partial class Contabilidad_CatalogoCucCompleto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(
                "Corela15.Infrastructure.Persistence.Seeds.CatalogoCucCompleto.sql");
            using var reader = new StreamReader(stream!);
            var sql = reader.ReadToEnd();

            migrationBuilder.Sql(sql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DELETE FROM contabilidad.cuenta_contable WHERE creado_por = 'seed:catalogo_seps_oficial';");
        }
    }
}
