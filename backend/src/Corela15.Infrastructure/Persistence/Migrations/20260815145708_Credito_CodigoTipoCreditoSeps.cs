using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Credito_CodigoTipoCreditoSeps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "codigo_tipo_credito_seps",
                schema: "credito",
                table: "tipo_prestamo",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            // Códigos reales de la Tabla 13 "Tipo de Crédito" (Manual
            // Técnico de Tablas de Información v34.0, vigente desde
            // 01/03/2024) para los 3 productos ya sembrados, mapeados por
            // su SegmentoBce existente.
            migrationBuilder.Sql("UPDATE credito.tipo_prestamo SET codigo_tipo_credito_seps = 'CO' WHERE codigo = 'CONS';");
            migrationBuilder.Sql("UPDATE credito.tipo_prestamo SET codigo_tipo_credito_seps = 'AA' WHERE codigo = 'MICRO';");
            migrationBuilder.Sql("UPDATE credito.tipo_prestamo SET codigo_tipo_credito_seps = 'PY' WHERE codigo = 'PROD';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "codigo_tipo_credito_seps",
                schema: "credito",
                table: "tipo_prestamo");
        }
    }
}
