using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Riesgo_IndicadorLiquidez : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "indicador_liquidez",
                schema: "riesgo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    fondos_disponibles = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    depositos_corto_plazo = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    coeficiente = table.Column<decimal>(type: "numeric(9,4)", nullable: false),
                    minimo_regulatorio = table.Column<decimal>(type: "numeric(9,4)", nullable: false),
                    cumple_minimo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_indicador_liquidez", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "parametro_liquidez",
                schema: "riesgo",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    minimo_regulatorio = table.Column<decimal>(type: "numeric(9,4)", nullable: false),
                    actualizado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    actualizado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_parametro_liquidez", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_indicador_liquidez_fecha",
                schema: "riesgo",
                table: "indicador_liquidez",
                column: "fecha");

            // Valor de referencia (25%) mientras se verifica el mínimo
            // exacto de la Norma para la Administración de Riesgo de
            // Liquidez de la SEPS — ver nota en ParametroLiquidez.cs.
            migrationBuilder.Sql(
                """
                INSERT INTO riesgo.parametro_liquidez (minimo_regulatorio, actualizado_en, actualizado_por)
                VALUES (0.25, now(), 'seed:migracion');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "indicador_liquidez",
                schema: "riesgo");

            migrationBuilder.DropTable(
                name: "parametro_liquidez",
                schema: "riesgo");
        }
    }
}
