using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Nivel8_RiesgoYReporteria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "riesgo");

            migrationBuilder.EnsureSchema(
                name: "reportecontrol");

            migrationBuilder.CreateTable(
                name: "macroproceso",
                schema: "riesgo",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_macroproceso", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "nivel_impacto",
                schema: "riesgo",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    nivel = table.Column<int>(type: "integer", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_nivel_impacto", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "nivel_probabilidad",
                schema: "riesgo",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    nivel = table.Column<int>(type: "integer", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_nivel_probabilidad", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "nivel_riesgo",
                schema: "riesgo",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    nivel = table.Column<int>(type: "integer", nullable: false),
                    rango_inicio = table.Column<decimal>(type: "numeric(9,4)", nullable: false),
                    rango_fin = table.Column<decimal>(type: "numeric(9,4)", nullable: false),
                    color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_nivel_riesgo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "reporte_regulatorio",
                schema: "reportecontrol",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    entidad = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reporte_regulatorio", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "proceso",
                schema: "riesgo",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_macro_proceso = table.Column<int>(type: "integer", nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    critico = table.Column<bool>(type: "boolean", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_proceso", x => x.id);
                    table.ForeignKey(
                        name: "fk_proceso_macroproceso_id_macro_proceso",
                        column: x => x.id_macro_proceso,
                        principalSchema: "riesgo",
                        principalTable: "macroproceso",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "evento_riesgo",
                schema: "riesgo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_proceso = table.Column<int>(type: "integer", nullable: false),
                    descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    id_nivel_impacto = table.Column<int>(type: "integer", nullable: false),
                    id_nivel_probabilidad = table.Column<int>(type: "integer", nullable: false),
                    id_nivel_riesgo = table.Column<int>(type: "integer", nullable: false),
                    fecha_identificacion = table.Column<DateOnly>(type: "date", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_evento_riesgo", x => x.id);
                    table.ForeignKey(
                        name: "fk_evento_riesgo_niveles_impacto_id_nivel_impacto",
                        column: x => x.id_nivel_impacto,
                        principalSchema: "riesgo",
                        principalTable: "nivel_impacto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_evento_riesgo_niveles_probabilidad_id_nivel_probabilidad",
                        column: x => x.id_nivel_probabilidad,
                        principalSchema: "riesgo",
                        principalTable: "nivel_probabilidad",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_evento_riesgo_niveles_riesgo_id_nivel_riesgo",
                        column: x => x.id_nivel_riesgo,
                        principalSchema: "riesgo",
                        principalTable: "nivel_riesgo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_evento_riesgo_procesos_id_proceso",
                        column: x => x.id_proceso,
                        principalSchema: "riesgo",
                        principalTable: "proceso",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_evento_riesgo_id_nivel_impacto",
                schema: "riesgo",
                table: "evento_riesgo",
                column: "id_nivel_impacto");

            migrationBuilder.CreateIndex(
                name: "ix_evento_riesgo_id_nivel_probabilidad",
                schema: "riesgo",
                table: "evento_riesgo",
                column: "id_nivel_probabilidad");

            migrationBuilder.CreateIndex(
                name: "ix_evento_riesgo_id_nivel_riesgo",
                schema: "riesgo",
                table: "evento_riesgo",
                column: "id_nivel_riesgo");

            migrationBuilder.CreateIndex(
                name: "ix_evento_riesgo_id_proceso",
                schema: "riesgo",
                table: "evento_riesgo",
                column: "id_proceso");

            migrationBuilder.CreateIndex(
                name: "ix_proceso_id_macro_proceso",
                schema: "riesgo",
                table: "proceso",
                column: "id_macro_proceso");

            migrationBuilder.CreateIndex(
                name: "ix_reporte_regulatorio_codigo",
                schema: "reportecontrol",
                table: "reporte_regulatorio",
                column: "codigo",
                unique: true);

            // --- Seed de catálogos ---
            migrationBuilder.Sql(
                """
                INSERT INTO riesgo.nivel_impacto (nombre, nivel, activo) VALUES
                    ('Insignificante', 1, true), ('Menor', 2, true), ('Moderado', 3, true),
                    ('Mayor', 4, true), ('Catastrófico', 5, true);

                INSERT INTO riesgo.nivel_probabilidad (nombre, nivel, activo) VALUES
                    ('Rara vez', 1, true), ('Improbable', 2, true), ('Posible', 3, true),
                    ('Probable', 4, true), ('Casi certeza', 5, true);

                INSERT INTO riesgo.nivel_riesgo (nombre, nivel, rango_inicio, rango_fin, color, activo) VALUES
                    ('Bajo',     1, 1,  6,  '#2c5670', true),
                    ('Moderado', 2, 7,  12, '#c9a665', true),
                    ('Alto',     3, 13, 19, '#b58e4a', true),
                    ('Extremo',  4, 20, 25, '#8b2f2f', true);
                """);

            // Índice de reportes regulatorios confirmados con filas reales en
            // Softbank hoy (REPORTECONTROL.CABECERA_*) — ver advertencia en
            // ReporteRegulatorio.cs: esto es solo el índice, no la estructura
            // de cada reporte.
            migrationBuilder.Sql(
                """
                INSERT INTO reportecontrol.reporte_regulatorio (codigo, nombre, entidad, activo) VALUES
                    ('B13',   'Balance mensual',                              'Seps', true),
                    ('D01',   'Reporte D01',                                  'Seps', true),
                    ('BCE01', 'Reporte Banco Central 01',                     'Bce',  true),
                    ('BCE02', 'Reporte Banco Central 02',                     'Bce',  true),
                    ('S01',   'Reporte S01',                                  'Seps', true),
                    ('L01',   'Liquidez L01',                                 'Seps', true),
                    ('L02',   'Liquidez L02',                                 'Seps', true),
                    ('IG01',  'Indicador de gestión IG01',                    'Seps', true),
                    ('TIN',   'Tasas de interés',                             'Bce',  true),
                    ('UAF',   'Reporte de operaciones a la UAF',              'Uaf',  true),
                    ('RFD',   'Fondo de liquidez',                            'Seps', true),
                    ('ROTEF', 'Reporte ROTEF',                                'Seps', true),
                    ('CRS',   'Intercambio de información fiscal internacional', 'Sri', true);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "evento_riesgo",
                schema: "riesgo");

            migrationBuilder.DropTable(
                name: "reporte_regulatorio",
                schema: "reportecontrol");

            migrationBuilder.DropTable(
                name: "nivel_impacto",
                schema: "riesgo");

            migrationBuilder.DropTable(
                name: "nivel_probabilidad",
                schema: "riesgo");

            migrationBuilder.DropTable(
                name: "nivel_riesgo",
                schema: "riesgo");

            migrationBuilder.DropTable(
                name: "proceso",
                schema: "riesgo");

            migrationBuilder.DropTable(
                name: "macroproceso",
                schema: "riesgo");
        }
    }
}
