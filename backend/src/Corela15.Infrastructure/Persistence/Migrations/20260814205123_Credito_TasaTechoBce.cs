using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Credito_TasaTechoBce : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "segmento_bce",
                schema: "credito",
                table: "tipo_prestamo",
                type: "character varying(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "tasa_techo_bce",
                schema: "credito",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    segmento = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    tasa_maxima = table.Column<decimal>(type: "numeric(9,4)", nullable: false),
                    fecha_vigencia_desde = table.Column<DateOnly>(type: "date", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tasa_techo_bce", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_tasa_techo_bce_segmento_fecha_vigencia_desde",
                schema: "credito",
                table: "tasa_techo_bce",
                columns: new[] { "segmento", "fecha_vigencia_desde" },
                unique: true);

            // Tasas de interés activas efectivas MÁXIMAS reales publicadas
            // por la Junta de Política y Regulación Monetaria y Financiera
            // (BCE) — vigentes marzo 2026 (última fuente verificable con el
            // desglose completo por segmento al momento de esta migración).
            // Estas tasas cambian mes a mes: hasta que exista una pantalla
            // de Configuración para mantenerlas, actualizar acá con una
            // nueva fila (nunca editar la existente — se conserva el
            // historial de vigencia) cuando el BCE publique una nueva
            // circular. No inventadas — ver CLAUDE.md para las fuentes.
            migrationBuilder.Sql(
                """
                -- tasa_maxima como fracción (0.0933 = 9.33%), mismo formato
                -- que credito.tipo_prestamo.tasa_anual, para que sean
                -- directamente comparables sin conversión.
                INSERT INTO credito.tasa_techo_bce (segmento, tasa_maxima, fecha_vigencia_desde, activo) VALUES
                    ('Productivo Corporativo', 0.0933, '2026-03-01', true),
                    ('Productivo Empresarial', 0.1021, '2026-03-01', true),
                    ('Productivo PYMES', 0.1183, '2026-03-01', true),
                    ('Consumo Ordinario', 0.1730, '2026-03-01', true),
                    ('Consumo Prioritario', 0.1730, '2026-03-01', true),
                    ('Vivienda de Interés Público', 0.0499, '2026-03-01', true),
                    ('Vivienda', 0.1133, '2026-03-01', true),
                    ('Microcrédito Minorista', 0.2850, '2026-03-01', true),
                    ('Microcrédito Acumulación Simple', 0.2550, '2026-03-01', true),
                    ('Microcrédito Acumulación Ampliada', 0.2350, '2026-03-01', true);

                -- Mapeo de los 3 productos ya sembrados (Credito_TasaAnual_SeedDesembolso)
                -- a su segmento BCE real. Microcrédito usa el techo más
                -- conservador de sus 3 subsegmentos (Acumulación Ampliada)
                -- porque el catálogo hoy no distingue subsegmento de
                -- microcrédito — simplificación explícita, no un olvido.
                UPDATE credito.tipo_prestamo SET segmento_bce = 'Consumo Prioritario' WHERE codigo = 'CONS';
                UPDATE credito.tipo_prestamo SET segmento_bce = 'Microcrédito Acumulación Ampliada' WHERE codigo = 'MICRO';
                UPDATE credito.tipo_prestamo SET segmento_bce = 'Productivo PYMES' WHERE codigo = 'PROD';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tasa_techo_bce",
                schema: "credito");

            migrationBuilder.DropColumn(
                name: "segmento_bce",
                schema: "credito",
                table: "tipo_prestamo");
        }
    }
}
