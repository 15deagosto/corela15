using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class General_ProvinciaSeps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "codigo_provincia_domicilio",
                schema: "sujeto",
                table: "persona",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "provincia",
                schema: "general",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_provincia", x => x.codigo);
                });

            // Los 24 códigos reales de INEC/SEPS (Tabla 05: Código de
            // Provincia, Manual Técnico de Tablas de Información v34.0).
            migrationBuilder.InsertData(
                schema: "general", table: "provincia", columns: new[] { "codigo", "nombre" },
                values: new object[,]
                {
                    { "01", "Azuay" }, { "02", "Bolívar" }, { "03", "Cañar" }, { "04", "Carchi" },
                    { "05", "Cotopaxi" }, { "06", "Chimborazo" }, { "07", "El Oro" }, { "08", "Esmeraldas" },
                    { "09", "Guayas" }, { "10", "Imbabura" }, { "11", "Loja" }, { "12", "Los Ríos" },
                    { "13", "Manabí" }, { "14", "Morona Santiago" }, { "15", "Napo" }, { "16", "Pastaza" },
                    { "17", "Pichincha" }, { "18", "Tungurahua" }, { "19", "Zamora Chinchipe" }, { "20", "Galápagos" },
                    { "21", "Sucumbíos" }, { "22", "Orellana" }, { "23", "Santo Domingo de los Tsáchilas" }, { "24", "Santa Elena" },
                });

            migrationBuilder.CreateIndex(
                name: "ix_persona_codigo_provincia_domicilio",
                schema: "sujeto",
                table: "persona",
                column: "codigo_provincia_domicilio");

            migrationBuilder.AddForeignKey(
                name: "fk_persona_provincia_codigo_provincia_domicilio",
                schema: "sujeto",
                table: "persona",
                column: "codigo_provincia_domicilio",
                principalSchema: "general",
                principalTable: "provincia",
                principalColumn: "codigo",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_persona_provincia_codigo_provincia_domicilio",
                schema: "sujeto",
                table: "persona");

            migrationBuilder.DropTable(
                name: "provincia",
                schema: "general");

            migrationBuilder.DropIndex(
                name: "ix_persona_codigo_provincia_domicilio",
                schema: "sujeto",
                table: "persona");

            migrationBuilder.DropColumn(
                name: "codigo_provincia_domicilio",
                schema: "sujeto",
                table: "persona");
        }
    }
}
