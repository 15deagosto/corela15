using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Tesoreria_ObligacionFinancieraIndiceUnico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_obligacion_financiera_tipo_identificacion_acreedor_identifi",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.CreateIndex(
                name: "ix_obligacion_financiera_tipo_identificacion_acreedor_identifi",
                schema: "obligacion",
                table: "obligacion_financiera",
                columns: new[] { "tipo_identificacion_acreedor", "identificacion_acreedor", "numero_obligacion", "id_cuenta_contable" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_obligacion_financiera_tipo_identificacion_acreedor_identifi",
                schema: "obligacion",
                table: "obligacion_financiera");

            migrationBuilder.CreateIndex(
                name: "ix_obligacion_financiera_tipo_identificacion_acreedor_identifi",
                schema: "obligacion",
                table: "obligacion_financiera",
                columns: new[] { "tipo_identificacion_acreedor", "identificacion_acreedor", "numero_obligacion", "id_cuenta_contable" });
        }
    }
}
