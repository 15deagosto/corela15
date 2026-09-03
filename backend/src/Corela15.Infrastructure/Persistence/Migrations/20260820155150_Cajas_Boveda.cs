using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Cajas_Boveda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "boveda",
                schema: "cajas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_agencia = table.Column<int>(type: "integer", nullable: false),
                    id_usuario_responsable = table.Column<Guid>(type: "uuid", nullable: false),
                    existencia_minima = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    existencia_maxima = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    existencia_minima_caja = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    existencia_maxima_caja = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    existencia_minima_caja_chica = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    existencia_maxima_caja_chica = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    activa = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_boveda", x => x.id);
                    table.ForeignKey(
                        name: "fk_boveda_agencia_id_agencia",
                        column: x => x.id_agencia,
                        principalSchema: "general",
                        principalTable: "agencia",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_boveda_usuarios_id_usuario_responsable",
                        column: x => x.id_usuario_responsable,
                        principalSchema: "seguridad",
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "item_boveda",
                schema: "cajas",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_item_boveda", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "boveda_item_boveda",
                schema: "cajas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_boveda = table.Column<Guid>(type: "uuid", nullable: false),
                    id_item_boveda = table.Column<int>(type: "integer", nullable: false),
                    id_moneda = table.Column<int>(type: "integer", nullable: false),
                    saldo = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_boveda_item_boveda", x => x.id);
                    table.ForeignKey(
                        name: "fk_boveda_item_boveda_boveda_id_boveda",
                        column: x => x.id_boveda,
                        principalSchema: "cajas",
                        principalTable: "boveda",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_boveda_item_boveda_items_boveda_id_item_boveda",
                        column: x => x.id_item_boveda,
                        principalSchema: "cajas",
                        principalTable: "item_boveda",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_boveda_item_boveda_monedas_id_moneda",
                        column: x => x.id_moneda,
                        principalSchema: "general",
                        principalTable: "moneda",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Datos reales verificados contra CAJAS.ITEMBOVEDA (2 filas reales).
            migrationBuilder.Sql(@"
INSERT INTO cajas.item_boveda (codigo, nombre, activo) VALUES
('EFE', 'Efectivo', true),
('CHE', 'Cheque', true);
");

            migrationBuilder.CreateIndex(
                name: "ix_boveda_id_agencia",
                schema: "cajas",
                table: "boveda",
                column: "id_agencia",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_boveda_id_usuario_responsable",
                schema: "cajas",
                table: "boveda",
                column: "id_usuario_responsable");

            migrationBuilder.CreateIndex(
                name: "ix_boveda_item_boveda_id_boveda_id_item_boveda_id_moneda",
                schema: "cajas",
                table: "boveda_item_boveda",
                columns: new[] { "id_boveda", "id_item_boveda", "id_moneda" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_boveda_item_boveda_id_item_boveda",
                schema: "cajas",
                table: "boveda_item_boveda",
                column: "id_item_boveda");

            migrationBuilder.CreateIndex(
                name: "ix_boveda_item_boveda_id_moneda",
                schema: "cajas",
                table: "boveda_item_boveda",
                column: "id_moneda");

            migrationBuilder.CreateIndex(
                name: "ix_item_boveda_codigo",
                schema: "cajas",
                table: "item_boveda",
                column: "codigo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "boveda_item_boveda",
                schema: "cajas");

            migrationBuilder.DropTable(
                name: "boveda",
                schema: "cajas");

            migrationBuilder.DropTable(
                name: "item_boveda",
                schema: "cajas");
        }
    }
}
