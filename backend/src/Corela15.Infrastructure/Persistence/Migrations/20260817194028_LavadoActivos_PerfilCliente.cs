using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LavadoActivos_PerfilCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "perfil_comportamiento",
                schema: "lavadoactivos",
                table: "calificacion_cliente");

            migrationBuilder.DropColumn(
                name: "perfil_transaccional",
                schema: "lavadoactivos",
                table: "calificacion_cliente");

            migrationBuilder.AlterColumn<decimal>(
                name: "total_perfil",
                schema: "lavadoactivos",
                table: "calificacion_cliente",
                type: "numeric(9,4)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(9,4)");

            migrationBuilder.AddColumn<decimal>(
                name: "banda_ingreso",
                schema: "lavadoactivos",
                table: "calificacion_cliente",
                type: "numeric(9,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "banda_patrimonio",
                schema: "lavadoactivos",
                table: "calificacion_cliente",
                type: "numeric(9,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "categoria",
                schema: "lavadoactivos",
                table: "calificacion_cliente",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "rango_ingreso_lavado",
                schema: "lavadoactivos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    valor_inicial = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    valor_final = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(9,4)", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rango_ingreso_lavado", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rango_patrimonio_lavado",
                schema: "lavadoactivos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    valor_inicial = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    valor_final = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(9,4)", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rango_patrimonio_lavado", x => x.id);
                });

            // Los 5 rangos reales, verificados en vivo contra
            // LAVADOACTIVOS.INGRESOS_MENSUALES (todos ACTIVO=true, sin solape).
            migrationBuilder.InsertData(
                schema: "lavadoactivos", table: "rango_ingreso_lavado",
                columns: new[] { "id", "valor_inicial", "valor_final", "valor", "activo" },
                values: new object[,]
                {
                    { 1, 0.00m, 460.00m, 0m, true },
                    { 2, 460.01m, 920.00m, 1m, true },
                    { 3, 920.01m, 1380.00m, 2m, true },
                    { 4, 1380.01m, 1840.00m, 3m, true },
                    { 5, 1840.01m, 999999999.00m, 4m, true },
                });

            // Los 4 rangos reales limpios (no solapados), verificados en vivo
            // contra LAVADOACTIVOS.PATRIMONIO_NETO — ver RangoPatrimonioLavado.cs
            // para por qué se excluyeron las filas ACTIVO=true solapadas de
            // reconfiguraciones posteriores del motor real.
            migrationBuilder.InsertData(
                schema: "lavadoactivos", table: "rango_patrimonio_lavado",
                columns: new[] { "id", "valor_inicial", "valor_final", "valor", "activo" },
                values: new object[,]
                {
                    { 1, 100.00m, 8940.00m, 1m, true },
                    { 2, 8940.01m, 20000.00m, 2m, true },
                    { 3, 20000.01m, 50000.00m, 3m, true },
                    { 4, 50000.01m, 999999999.00m, 4m, true },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rango_ingreso_lavado",
                schema: "lavadoactivos");

            migrationBuilder.DropTable(
                name: "rango_patrimonio_lavado",
                schema: "lavadoactivos");

            migrationBuilder.DropColumn(
                name: "banda_ingreso",
                schema: "lavadoactivos",
                table: "calificacion_cliente");

            migrationBuilder.DropColumn(
                name: "banda_patrimonio",
                schema: "lavadoactivos",
                table: "calificacion_cliente");

            migrationBuilder.DropColumn(
                name: "categoria",
                schema: "lavadoactivos",
                table: "calificacion_cliente");

            migrationBuilder.AlterColumn<decimal>(
                name: "total_perfil",
                schema: "lavadoactivos",
                table: "calificacion_cliente",
                type: "numeric(9,4)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(9,4)",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "perfil_comportamiento",
                schema: "lavadoactivos",
                table: "calificacion_cliente",
                type: "numeric(9,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "perfil_transaccional",
                schema: "lavadoactivos",
                table: "calificacion_cliente",
                type: "numeric(9,4)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
