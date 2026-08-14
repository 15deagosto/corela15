using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Credito_ScoreCrediticio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "score_crediticio",
                schema: "credito",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_cliente = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    puntaje = table.Column<int>(type: "integer", nullable: false),
                    categoria = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ratio_ingreso_egreso = table.Column<decimal>(type: "numeric(9,4)", nullable: true),
                    ratio_endeudamiento = table.Column<decimal>(type: "numeric(9,4)", nullable: true),
                    tiene_prestamo_castigado = table.Column<bool>(type: "boolean", nullable: false),
                    prestamos_cancelados = table.Column<int>(type: "integer", nullable: false),
                    es_pep = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_score_crediticio", x => x.id);
                    table.ForeignKey(
                        name: "fk_score_crediticio_cliente_id_cliente",
                        column: x => x.id_cliente,
                        principalSchema: "clientes",
                        principalTable: "cliente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_score_crediticio_id_cliente_fecha",
                schema: "credito",
                table: "score_crediticio",
                columns: new[] { "id_cliente", "fecha" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "score_crediticio",
                schema: "credito");
        }
    }
}
