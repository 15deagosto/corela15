using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Credito_SeguroDesgravamenFlat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "codigo_tipo_seguro",
                schema: "credito",
                table: "tipo_prestamo",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tipo_seguro",
                schema: "credito",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    valor_mensual = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    beneficiario_adicional = table.Column<int>(type: "integer", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipo_seguro", x => x.codigo);
                });

            migrationBuilder.CreateIndex(
                name: "ix_tipo_prestamo_codigo_tipo_seguro",
                schema: "credito",
                table: "tipo_prestamo",
                column: "codigo_tipo_seguro");

            migrationBuilder.AddForeignKey(
                name: "fk_tipo_prestamo_tipo_seguro_codigo_tipo_seguro",
                schema: "credito",
                table: "tipo_prestamo",
                column: "codigo_tipo_seguro",
                principalSchema: "credito",
                principalTable: "tipo_seguro",
                principalColumn: "codigo",
                onDelete: ReferentialAction.Restrict);

            // Catálogo real, verificado en vivo contra CREDITO.TIPO_SEGURO
            // (3 filas reales) — ver TipoSeguro.cs.
            migrationBuilder.InsertData(
                schema: "credito",
                table: "tipo_seguro",
                columns: new[] { "codigo", "nombre", "valor_mensual", "beneficiario_adicional", "activo" },
                values: new object[,]
                {
                    { "I", "Individual", 1.00m, 0, true },
                    { "A", "Deudor y Adicional", 2.00m, 1, true },
                    { "F", "Deudor y Familiares", 3.00m, 4, true },
                });

            // Producto Consumo (CONS) queda con seguro Individual por
            // defecto — el más simple/común de los 3 tipos reales, para
            // que el flujo de desembolso genere el rubro real desde ya.
            migrationBuilder.Sql(
                "UPDATE credito.tipo_prestamo SET codigo_tipo_seguro = 'I' WHERE codigo = 'CONS';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE credito.tipo_prestamo SET codigo_tipo_seguro = NULL WHERE codigo = 'CONS';");

            migrationBuilder.DropForeignKey(
                name: "fk_tipo_prestamo_tipo_seguro_codigo_tipo_seguro",
                schema: "credito",
                table: "tipo_prestamo");

            migrationBuilder.DropTable(
                name: "tipo_seguro",
                schema: "credito");

            migrationBuilder.DropIndex(
                name: "ix_tipo_prestamo_codigo_tipo_seguro",
                schema: "credito",
                table: "tipo_prestamo");

            migrationBuilder.DropColumn(
                name: "codigo_tipo_seguro",
                schema: "credito",
                table: "tipo_prestamo");
        }
    }
}
