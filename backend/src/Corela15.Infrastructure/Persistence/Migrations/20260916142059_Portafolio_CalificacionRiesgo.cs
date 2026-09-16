using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Portafolio_CalificacionRiesgo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "codigo_calificacion_riesgo",
                schema: "portafolio",
                table: "inversion_portafolio",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "codigo_calificadora_riesgo",
                schema: "portafolio",
                table: "inversion_portafolio",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "fecha_ultima_calificacion",
                schema: "portafolio",
                table: "inversion_portafolio",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "provision_constituida",
                schema: "portafolio",
                table: "inversion_portafolio",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "calificacion_riesgo",
                schema: "portafolio",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_calificacion_riesgo", x => x.codigo);
                });

            migrationBuilder.CreateTable(
                name: "calificadora_riesgo",
                schema: "portafolio",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_calificadora_riesgo", x => x.codigo);
                });

            // Catálogos reales, verificados en vivo contra PORTAFOLIO.
            // CALIFICACIONRIESGO (26 filas) y PORTAFOLIO.CALIFICADORARIESGO
            // (12 filas), confirmados contra el I02 real de referencia
            // (agosto 2026) que asigna una calificación+calificadora a
            // cada inversión del portafolio.
            migrationBuilder.Sql(@"
INSERT INTO portafolio.calificacion_riesgo (codigo, nombre, activo) VALUES
('001', 'Optimo', true),
('002', 'Alta Calidad Crediticia', true),
('003', 'Alta Calidad Crediticia', true),
('004', 'Alta Calidad Crediticia', true),
('005', 'Buena Calidad Crediticia', true),
('006', 'Buena Calidad Crediticia', true),
('007', 'Buena Calidad Crediticia', true),
('008', 'Factores de proteccion inferiores al promedio', true),
('009', 'Factores de proteccion inferiores al promedio', true),
('010', 'Factores de proteccion inferiores al promedio', true),
('011', 'Emisiones cerca del grado de inversion', true),
('012', 'Emisiones cerca del grado de inversion', true),
('013', 'Emisiones cerca del grado de inversion', true),
('014', 'Emisiones por debajo del grado de inversion', true),
('015', 'Emisiones por debajo del grado de inversion', true),
('016', 'Emisiones por debajo del grado de inversion', true),
('017', 'Emisiones muy por debajo del grado de inversion', true),
('018', 'Incumplimiento de pago u obligacion', true),
('019', 'Sin suficiente informacion para calificar', true),
('020', 'Sin suficiente informacion para calificar', true),
('021', 'Sin suficiente informacion para calificar', true),
('022', 'Sin suficiente informacion para calificar', false),
('023', 'Sin suficiente informacion para calificar', false),
('024', 'Sin suficiente informacion para calificar', false),
('025', 'Sin suficiente informacion para calificar', true),
('026', 'No Disponible', true);

INSERT INTO portafolio.calificadora_riesgo (codigo, nombre, activo) VALUES
('000', 'No Disponible', true),
('001', 'Standard & Poor''s (S & P)', true),
('002', 'Moody''s', true),
('003', 'Fitch', true),
('004', 'Bank Watch Ratings', true),
('005', 'Ecuability', true),
('006', 'Humphreys', true),
('007', 'Pcr Pacific', true),
('008', 'Soc. Cal. Riesgo Latinoamericana SCR LA', true),
('009', 'Class International Rating', true),
('010', 'Microfinanza Rating', true),
('011', 'Otras', true);
");

            migrationBuilder.CreateIndex(
                name: "ix_inversion_portafolio_codigo_calificacion_riesgo",
                schema: "portafolio",
                table: "inversion_portafolio",
                column: "codigo_calificacion_riesgo");

            migrationBuilder.CreateIndex(
                name: "ix_inversion_portafolio_codigo_calificadora_riesgo",
                schema: "portafolio",
                table: "inversion_portafolio",
                column: "codigo_calificadora_riesgo");

            migrationBuilder.AddForeignKey(
                name: "fk_inversion_portafolio_calificacion_riesgo_codigo_calificacio",
                schema: "portafolio",
                table: "inversion_portafolio",
                column: "codigo_calificacion_riesgo",
                principalSchema: "portafolio",
                principalTable: "calificacion_riesgo",
                principalColumn: "codigo",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_inversion_portafolio_calificadora_riesgo_codigo_calificador",
                schema: "portafolio",
                table: "inversion_portafolio",
                column: "codigo_calificadora_riesgo",
                principalSchema: "portafolio",
                principalTable: "calificadora_riesgo",
                principalColumn: "codigo",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_inversion_portafolio_calificacion_riesgo_codigo_calificacio",
                schema: "portafolio",
                table: "inversion_portafolio");

            migrationBuilder.DropForeignKey(
                name: "fk_inversion_portafolio_calificadora_riesgo_codigo_calificador",
                schema: "portafolio",
                table: "inversion_portafolio");

            migrationBuilder.Sql("DELETE FROM portafolio.calificadora_riesgo; DELETE FROM portafolio.calificacion_riesgo;");

            migrationBuilder.DropTable(
                name: "calificacion_riesgo",
                schema: "portafolio");

            migrationBuilder.DropTable(
                name: "calificadora_riesgo",
                schema: "portafolio");

            migrationBuilder.DropIndex(
                name: "ix_inversion_portafolio_codigo_calificacion_riesgo",
                schema: "portafolio",
                table: "inversion_portafolio");

            migrationBuilder.DropIndex(
                name: "ix_inversion_portafolio_codigo_calificadora_riesgo",
                schema: "portafolio",
                table: "inversion_portafolio");

            migrationBuilder.DropColumn(
                name: "codigo_calificacion_riesgo",
                schema: "portafolio",
                table: "inversion_portafolio");

            migrationBuilder.DropColumn(
                name: "codigo_calificadora_riesgo",
                schema: "portafolio",
                table: "inversion_portafolio");

            migrationBuilder.DropColumn(
                name: "fecha_ultima_calificacion",
                schema: "portafolio",
                table: "inversion_portafolio");

            migrationBuilder.DropColumn(
                name: "provision_constituida",
                schema: "portafolio",
                table: "inversion_portafolio");
        }
    }
}
