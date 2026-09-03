using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Credito_TipoConvenio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "codigo_tipo_convenio",
                schema: "credito",
                table: "solicitud_prestamo",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "codigo_tipo_convenio",
                schema: "colocacion",
                table: "prestamo",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tipo_convenio",
                schema: "credito",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    id_agencia = table.Column<int>(type: "integer", nullable: false),
                    es_cooperativa = table.Column<bool>(type: "boolean", nullable: false),
                    valor_ahorro = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipo_convenio", x => x.codigo);
                    table.ForeignKey(
                        name: "fk_tipo_convenio_agencia_id_agencia",
                        column: x => x.id_agencia,
                        principalSchema: "general",
                        principalTable: "agencia",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_solicitud_prestamo_codigo_tipo_convenio",
                schema: "credito",
                table: "solicitud_prestamo",
                column: "codigo_tipo_convenio");

            migrationBuilder.CreateIndex(
                name: "ix_prestamo_codigo_tipo_convenio",
                schema: "colocacion",
                table: "prestamo",
                column: "codigo_tipo_convenio");

            migrationBuilder.CreateIndex(
                name: "ix_tipo_convenio_id_agencia",
                schema: "credito",
                table: "tipo_convenio",
                column: "id_agencia");

            migrationBuilder.AddForeignKey(
                name: "fk_prestamo_tipo_convenio_codigo_tipo_convenio",
                schema: "colocacion",
                table: "prestamo",
                column: "codigo_tipo_convenio",
                principalSchema: "credito",
                principalTable: "tipo_convenio",
                principalColumn: "codigo",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_solicitud_prestamo_tipo_convenio_codigo_tipo_convenio",
                schema: "credito",
                table: "solicitud_prestamo",
                column: "codigo_tipo_convenio",
                principalSchema: "credito",
                principalTable: "tipo_convenio",
                principalColumn: "codigo",
                onDelete: ReferentialAction.Restrict);

            // Catálogo real, verificado en vivo contra CREDITO.TIPO_CONVENIO
            // (204 filas reales) — solo se sembraron los 2 códigos con uso
            // real confirmado (COLOCACION.PRESTAMO_TIPOCONVENIO, 8 filas
            // reales, solo referencian estos 2 códigos; los 202 restantes
            // del catálogo real están inactivos y sin ningún préstamo real
            // asociado). IdAgencia real de Softbank (2, 4) colapsado a la
            // única agencia real de este core (1, Matriz) — misma
            // simplificación ya aplicada a GrupoContable (ver CLAUDE.md).
            migrationBuilder.InsertData(
                schema: "credito",
                table: "tipo_convenio",
                columns: new[] { "codigo", "nombre", "id_agencia", "es_cooperativa", "valor_ahorro", "activo" },
                values: new object[,]
                {
                    { "001", "COOP.COMERCIO LTDA.", 1, true, 5.00m, false },
                    { "171", "SINDICATO DE TRABAJADORES DE LA DIRECCIÓN PROVINCIAL DE SALUD DE COTOPAXI", 1, false, 0.00m, true },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_prestamo_tipo_convenio_codigo_tipo_convenio",
                schema: "colocacion",
                table: "prestamo");

            migrationBuilder.DropForeignKey(
                name: "fk_solicitud_prestamo_tipo_convenio_codigo_tipo_convenio",
                schema: "credito",
                table: "solicitud_prestamo");

            migrationBuilder.DropTable(
                name: "tipo_convenio",
                schema: "credito");

            migrationBuilder.DropIndex(
                name: "ix_solicitud_prestamo_codigo_tipo_convenio",
                schema: "credito",
                table: "solicitud_prestamo");

            migrationBuilder.DropIndex(
                name: "ix_prestamo_codigo_tipo_convenio",
                schema: "colocacion",
                table: "prestamo");

            migrationBuilder.DropColumn(
                name: "codigo_tipo_convenio",
                schema: "credito",
                table: "solicitud_prestamo");

            migrationBuilder.DropColumn(
                name: "codigo_tipo_convenio",
                schema: "colocacion",
                table: "prestamo");
        }
    }
}
