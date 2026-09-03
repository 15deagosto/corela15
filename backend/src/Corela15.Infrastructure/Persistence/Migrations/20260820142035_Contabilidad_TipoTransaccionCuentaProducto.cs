using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Contabilidad_TipoTransaccionCuentaProducto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tipo_transaccion_cuenta_producto",
                schema: "contabilidad",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_tipo_transaccion = table.Column<int>(type: "integer", nullable: false),
                    id_tipo_cuenta = table.Column<int>(type: "integer", nullable: false),
                    id_cuenta_contable_debito = table.Column<Guid>(type: "uuid", nullable: false),
                    id_cuenta_contable_credito = table.Column<Guid>(type: "uuid", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipo_transaccion_cuenta_producto", x => x.id);
                    table.ForeignKey(
                        name: "fk_tipo_transaccion_cuenta_producto_cuenta_contable_id_cuenta_",
                        column: x => x.id_cuenta_contable_credito,
                        principalSchema: "contabilidad",
                        principalTable: "cuenta_contable",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_tipo_transaccion_cuenta_producto_cuenta_contable_id_cuenta_1",
                        column: x => x.id_cuenta_contable_debito,
                        principalSchema: "contabilidad",
                        principalTable: "cuenta_contable",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_tipo_transaccion_cuenta_producto_tipo_cuenta_id_tipo_cuenta",
                        column: x => x.id_tipo_cuenta,
                        principalSchema: "ahorros",
                        principalTable: "tipo_cuenta",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_tipo_transaccion_cuenta_producto_tipo_transaccion_id_tipo_t",
                        column: x => x.id_tipo_transaccion,
                        principalSchema: "contabilidad",
                        principalTable: "tipo_transaccion",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Override real: DEP-EFEC/RET-EFEC sobre una cuenta CERT
            // (Certificados de Aportación) deben afectar 3103 Aportes de
            // socios (capital social), no 2101 Depósitos de ahorro a la
            // vista — mismo bug ya corregido en CuentaAhorroService.AbrirAsync
            // para la apertura, acá se cierra para movimientos posteriores.
            migrationBuilder.Sql(@"
INSERT INTO contabilidad.tipo_transaccion_cuenta_producto
    (id_tipo_transaccion, id_tipo_cuenta, id_cuenta_contable_debito, id_cuenta_contable_credito, activo)
SELECT tt.id, tc.id,
    (SELECT id FROM contabilidad.cuenta_contable WHERE codigo = '1101'),
    (SELECT id FROM contabilidad.cuenta_contable WHERE codigo = '3103'),
    true
FROM contabilidad.tipo_transaccion tt, ahorros.tipo_cuenta tc
WHERE tt.codigo = 'DEP-EFEC' AND tc.codigo = 'CERT';

INSERT INTO contabilidad.tipo_transaccion_cuenta_producto
    (id_tipo_transaccion, id_tipo_cuenta, id_cuenta_contable_debito, id_cuenta_contable_credito, activo)
SELECT tt.id, tc.id,
    (SELECT id FROM contabilidad.cuenta_contable WHERE codigo = '3103'),
    (SELECT id FROM contabilidad.cuenta_contable WHERE codigo = '1101'),
    true
FROM contabilidad.tipo_transaccion tt, ahorros.tipo_cuenta tc
WHERE tt.codigo = 'RET-EFEC' AND tc.codigo = 'CERT';
");

            migrationBuilder.CreateIndex(
                name: "ix_tipo_transaccion_cuenta_producto_id_cuenta_contable_credito",
                schema: "contabilidad",
                table: "tipo_transaccion_cuenta_producto",
                column: "id_cuenta_contable_credito");

            migrationBuilder.CreateIndex(
                name: "ix_tipo_transaccion_cuenta_producto_id_cuenta_contable_debito",
                schema: "contabilidad",
                table: "tipo_transaccion_cuenta_producto",
                column: "id_cuenta_contable_debito");

            migrationBuilder.CreateIndex(
                name: "ix_tipo_transaccion_cuenta_producto_id_tipo_cuenta",
                schema: "contabilidad",
                table: "tipo_transaccion_cuenta_producto",
                column: "id_tipo_cuenta");

            migrationBuilder.CreateIndex(
                name: "ix_tipo_transaccion_cuenta_producto_id_tipo_transaccion_id_tip",
                schema: "contabilidad",
                table: "tipo_transaccion_cuenta_producto",
                columns: new[] { "id_tipo_transaccion", "id_tipo_cuenta" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tipo_transaccion_cuenta_producto",
                schema: "contabilidad");
        }
    }
}
