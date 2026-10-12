using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Planificacion_CumplimientoDisposicionPorArea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "calificado_por",
                schema: "planificacion",
                table: "disposicion_gerencia_area",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "comentario_cumplimiento",
                schema: "planificacion",
                table: "disposicion_gerencia_area",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            // defaultValue real explícito -- EF Core no propaga el valor
            // C# del enum (SinEvaluar) al generar la migración, mismo gap
            // ya documentado varias veces en el proyecto.
            migrationBuilder.AddColumn<string>(
                name: "cumplimiento",
                schema: "planificacion",
                table: "disposicion_gerencia_area",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "SinEvaluar");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "fecha_calificacion",
                schema: "planificacion",
                table: "disposicion_gerencia_area",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "calificado_por",
                schema: "planificacion",
                table: "disposicion_gerencia_area");

            migrationBuilder.DropColumn(
                name: "comentario_cumplimiento",
                schema: "planificacion",
                table: "disposicion_gerencia_area");

            migrationBuilder.DropColumn(
                name: "cumplimiento",
                schema: "planificacion",
                table: "disposicion_gerencia_area");

            migrationBuilder.DropColumn(
                name: "fecha_calificacion",
                schema: "planificacion",
                table: "disposicion_gerencia_area");
        }
    }
}
