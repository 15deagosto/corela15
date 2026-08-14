using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Colocacion_AutoDebitoSpiLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "auto_debito_spi_log",
                schema: "colocacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    id_prestamo = table.Column<Guid>(type: "uuid", nullable: false),
                    numero_cuota = table.Column<int>(type: "integer", nullable: true),
                    id_cuenta = table.Column<Guid>(type: "uuid", nullable: true),
                    debitado = table.Column<bool>(type: "boolean", nullable: false),
                    motivo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    monto = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_auto_debito_spi_log", x => x.id);
                    table.ForeignKey(
                        name: "fk_auto_debito_spi_log_prestamos_id_prestamo",
                        column: x => x.id_prestamo,
                        principalSchema: "colocacion",
                        principalTable: "prestamo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_auto_debito_spi_log_fecha_id_prestamo",
                schema: "colocacion",
                table: "auto_debito_spi_log",
                columns: new[] { "fecha", "id_prestamo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_auto_debito_spi_log_id_prestamo",
                schema: "colocacion",
                table: "auto_debito_spi_log",
                column: "id_prestamo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "auto_debito_spi_log",
                schema: "colocacion");
        }
    }
}
