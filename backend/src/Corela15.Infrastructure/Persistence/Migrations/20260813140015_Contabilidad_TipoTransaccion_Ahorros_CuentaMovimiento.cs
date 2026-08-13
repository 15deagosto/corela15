using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Contabilidad_TipoTransaccion_Ahorros_CuentaMovimiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "saldo_minimo",
                schema: "ahorros",
                table: "tipo_cuenta",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "cuenta_movimiento",
                schema: "ahorros",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_cuenta = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    monto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    saldo_resultante = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    id_comprobante_contable = table.Column<Guid>(type: "uuid", nullable: true),
                    fecha_hora = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    registrado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cuenta_movimiento", x => x.id);
                    table.ForeignKey(
                        name: "fk_cuenta_movimiento_comprobante_contable_id_comprobante_conta",
                        column: x => x.id_comprobante_contable,
                        principalSchema: "contabilidad",
                        principalTable: "comprobante_contable",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_cuenta_movimiento_cuenta_id_cuenta",
                        column: x => x.id_cuenta,
                        principalSchema: "ahorros",
                        principalTable: "cuenta",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tipo_transaccion",
                schema: "contabilidad",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    id_cuenta_contable_debito = table.Column<Guid>(type: "uuid", nullable: false),
                    id_cuenta_contable_credito = table.Column<Guid>(type: "uuid", nullable: false),
                    id_tipo_comprobante = table.Column<int>(type: "integer", nullable: false),
                    signo_saldo_cuenta = table.Column<int>(type: "integer", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipo_transaccion", x => x.id);
                    table.CheckConstraint("ck_tipo_transaccion_signo", "signo_saldo_cuenta IN (-1, 1)");
                    table.ForeignKey(
                        name: "fk_tipo_transaccion_cuenta_contable_id_cuenta_contable_credito",
                        column: x => x.id_cuenta_contable_credito,
                        principalSchema: "contabilidad",
                        principalTable: "cuenta_contable",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_tipo_transaccion_cuenta_contable_id_cuenta_contable_debito",
                        column: x => x.id_cuenta_contable_debito,
                        principalSchema: "contabilidad",
                        principalTable: "cuenta_contable",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_tipo_transaccion_tipo_comprobante_contable_id_tipo_comproba",
                        column: x => x.id_tipo_comprobante,
                        principalSchema: "contabilidad",
                        principalTable: "tipo_comprobante_contable",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_cuenta_movimiento_id_comprobante_contable",
                schema: "ahorros",
                table: "cuenta_movimiento",
                column: "id_comprobante_contable");

            migrationBuilder.CreateIndex(
                name: "ix_cuenta_movimiento_id_cuenta_fecha_hora",
                schema: "ahorros",
                table: "cuenta_movimiento",
                columns: new[] { "id_cuenta", "fecha_hora" });

            migrationBuilder.CreateIndex(
                name: "ix_tipo_transaccion_codigo",
                schema: "contabilidad",
                table: "tipo_transaccion",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tipo_transaccion_id_cuenta_contable_credito",
                schema: "contabilidad",
                table: "tipo_transaccion",
                column: "id_cuenta_contable_credito");

            migrationBuilder.CreateIndex(
                name: "ix_tipo_transaccion_id_cuenta_contable_debito",
                schema: "contabilidad",
                table: "tipo_transaccion",
                column: "id_cuenta_contable_debito");

            migrationBuilder.CreateIndex(
                name: "ix_tipo_transaccion_id_tipo_comprobante",
                schema: "contabilidad",
                table: "tipo_transaccion",
                column: "id_tipo_comprobante");

            // Saldo mínimo real de la cooperativa por producto (falencia
            // corregida respecto a Softbank, ver TipoCuenta.SaldoMinimo).
            migrationBuilder.Sql(
                """
                UPDATE ahorros.tipo_cuenta SET saldo_minimo = 5.00 WHERE codigo = 'AHV';
                UPDATE ahorros.tipo_cuenta SET saldo_minimo = 1.00 WHERE codigo = 'AHI';
                UPDATE ahorros.tipo_cuenta SET saldo_minimo = 50.00 WHERE codigo = 'CERT';
                """);

            // Motor contable configurable: qué transacción dispara qué asiento,
            // una sola fila legible en vez de las tres tablas cruzadas de
            // Softbank (ver TipoTransaccion.cs).
            migrationBuilder.Sql(
                """
                INSERT INTO contabilidad.tipo_transaccion
                    (codigo, nombre, id_cuenta_contable_debito, id_cuenta_contable_credito, id_tipo_comprobante, signo_saldo_cuenta, activo)
                SELECT 'DEP-EFEC', 'Depósito en efectivo', caja.id, depositos.id, tc.id, 1, true
                FROM contabilidad.cuenta_contable caja, contabilidad.cuenta_contable depositos, contabilidad.tipo_comprobante_contable tc
                WHERE caja.codigo = '1101' AND depositos.codigo = '2101' AND tc.codigo = 'DIA';

                INSERT INTO contabilidad.tipo_transaccion
                    (codigo, nombre, id_cuenta_contable_debito, id_cuenta_contable_credito, id_tipo_comprobante, signo_saldo_cuenta, activo)
                SELECT 'RET-EFEC', 'Retiro en efectivo', depositos.id, caja.id, tc.id, -1, true
                FROM contabilidad.cuenta_contable caja, contabilidad.cuenta_contable depositos, contabilidad.tipo_comprobante_contable tc
                WHERE caja.codigo = '1101' AND depositos.codigo = '2101' AND tc.codigo = 'DIA';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cuenta_movimiento",
                schema: "ahorros");

            migrationBuilder.DropTable(
                name: "tipo_transaccion",
                schema: "contabilidad");

            migrationBuilder.DropColumn(
                name: "saldo_minimo",
                schema: "ahorros",
                table: "tipo_cuenta");
        }
    }
}
