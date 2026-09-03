using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Cajas_VentanillaItemCaja : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "item_caja",
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
                    table.PrimaryKey("pk_item_caja", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ventanilla_item_caja",
                schema: "cajas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_ventanilla = table.Column<Guid>(type: "uuid", nullable: false),
                    id_item_caja = table.Column<int>(type: "integer", nullable: false),
                    id_moneda = table.Column<int>(type: "integer", nullable: false),
                    saldo = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    saldo_cuadre = table.Column<decimal>(type: "numeric(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ventanilla_item_caja", x => x.id);
                    table.ForeignKey(
                        name: "fk_ventanilla_item_caja_item_caja_id_item_caja",
                        column: x => x.id_item_caja,
                        principalSchema: "cajas",
                        principalTable: "item_caja",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_ventanilla_item_caja_moneda_id_moneda",
                        column: x => x.id_moneda,
                        principalSchema: "general",
                        principalTable: "moneda",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_ventanilla_item_caja_ventanilla_id_ventanilla",
                        column: x => x.id_ventanilla,
                        principalSchema: "cajas",
                        principalTable: "ventanilla",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ventanilla_item_caja_movimiento",
                schema: "cajas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_ventanilla_item_caja = table.Column<Guid>(type: "uuid", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    saldo_resultante = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    descripcion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    id_comprobante_contable = table.Column<Guid>(type: "uuid", nullable: true),
                    registrado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    fecha = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ventanilla_item_caja_movimiento", x => x.id);
                    table.ForeignKey(
                        name: "fk_ventanilla_item_caja_movimiento_comprobante_contable_id_com",
                        column: x => x.id_comprobante_contable,
                        principalSchema: "contabilidad",
                        principalTable: "comprobante_contable",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_ventanilla_item_caja_movimiento_ventanilla_item_caja_id_ven",
                        column: x => x.id_ventanilla_item_caja,
                        principalSchema: "cajas",
                        principalTable: "ventanilla_item_caja",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Datos reales verificados contra CAJAS.ITEMCAJA (4 filas
            // reales). Solo "EFE" (Efectivo) tiene un enganche real hoy
            // (ver ComprobanteContableService) — el resto queda sembrado
            // como catálogo real completo, listo para cuando exista el
            // caso de uso de cheques/caja chica.
            migrationBuilder.Sql(@"
INSERT INTO cajas.item_caja (codigo, nombre, activo) VALUES
('EFE', 'Efectivo', true),
('CHI', 'Cheque Ingreso', true),
('CHE', 'Cheque Egreso', true),
('EFC', 'Efectivo Caja Chica', true);
");

            migrationBuilder.CreateIndex(
                name: "ix_item_caja_codigo",
                schema: "cajas",
                table: "item_caja",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ventanilla_item_caja_id_item_caja",
                schema: "cajas",
                table: "ventanilla_item_caja",
                column: "id_item_caja");

            migrationBuilder.CreateIndex(
                name: "ix_ventanilla_item_caja_id_moneda",
                schema: "cajas",
                table: "ventanilla_item_caja",
                column: "id_moneda");

            migrationBuilder.CreateIndex(
                name: "ix_ventanilla_item_caja_id_ventanilla_id_item_caja_id_moneda",
                schema: "cajas",
                table: "ventanilla_item_caja",
                columns: new[] { "id_ventanilla", "id_item_caja", "id_moneda" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ventanilla_item_caja_movimiento_id_comprobante_contable",
                schema: "cajas",
                table: "ventanilla_item_caja_movimiento",
                column: "id_comprobante_contable");

            migrationBuilder.CreateIndex(
                name: "ix_ventanilla_item_caja_movimiento_id_ventanilla_item_caja",
                schema: "cajas",
                table: "ventanilla_item_caja_movimiento",
                column: "id_ventanilla_item_caja");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ventanilla_item_caja_movimiento",
                schema: "cajas");

            migrationBuilder.DropTable(
                name: "ventanilla_item_caja",
                schema: "cajas");

            migrationBuilder.DropTable(
                name: "item_caja",
                schema: "cajas");
        }
    }
}
