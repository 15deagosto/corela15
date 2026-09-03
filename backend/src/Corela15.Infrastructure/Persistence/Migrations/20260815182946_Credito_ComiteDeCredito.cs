using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Credito_ComiteDeCredito : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "aprobado_por",
                schema: "credito",
                table: "solicitud_prestamo",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "comentario_aprobacion",
                schema: "credito",
                table: "solicitud_prestamo",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "fecha_aprobacion",
                schema: "credito",
                table: "solicitud_prestamo",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "aprobado_por",
                schema: "credito",
                table: "solicitud_prestamo");

            migrationBuilder.DropColumn(
                name: "comentario_aprobacion",
                schema: "credito",
                table: "solicitud_prestamo");

            migrationBuilder.DropColumn(
                name: "fecha_aprobacion",
                schema: "credito",
                table: "solicitud_prestamo");
        }
    }
}
