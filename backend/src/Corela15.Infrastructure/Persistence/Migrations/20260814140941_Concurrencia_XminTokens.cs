using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Concurrencia_XminTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "cajas",
                table: "ventanilla",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "contabilidad",
                table: "saldo_contable",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "colocacion",
                table: "prestamo",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "inversion",
                table: "deposito",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "cuentasporcobrar",
                table: "cuenta_por_cobrar",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "ahorros",
                table: "cuenta_item_saldo",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "cajas",
                table: "ventanilla");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "contabilidad",
                table: "saldo_contable");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "colocacion",
                table: "prestamo");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "inversion",
                table: "deposito");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "cuentasporcobrar",
                table: "cuenta_por_cobrar");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "ahorros",
                table: "cuenta_item_saldo");
        }
    }
}
