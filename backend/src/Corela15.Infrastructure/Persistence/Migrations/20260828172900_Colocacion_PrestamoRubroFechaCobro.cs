using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Colocacion_PrestamoRubroFechaCobro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "fecha_cobro",
                schema: "colocacion",
                table: "prestamo_rubro",
                type: "date",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_prestamo_rubro_fecha_cobro",
                schema: "colocacion",
                table: "prestamo_rubro",
                column: "fecha_cobro");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_prestamo_rubro_fecha_cobro",
                schema: "colocacion",
                table: "prestamo_rubro");

            migrationBuilder.DropColumn(
                name: "fecha_cobro",
                schema: "colocacion",
                table: "prestamo_rubro");
        }
    }
}
