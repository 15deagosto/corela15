using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Cajas_PagoExterno : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pago_externo_producto",
                schema: "cajas",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    nombre = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    titulo_referencia = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    requiere_datos_factura = table.Column<bool>(type: "boolean", nullable: false),
                    tiene_comision = table.Column<bool>(type: "boolean", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pago_externo_producto", x => x.id);
                });

            // Catálogo real — verificado contra CAJAS.PAGO_EXTERNO_PRODUCTO
            // (629 filas activas del switch nacional), sembrado solo con
            // los 29 productos con actividad transaccional real de esta
            // cooperativa (679 transacciones reales en CAJAS.
            // PAGO_EXTERNO_TRANSACCION) — ver CLAUDE.md.
            migrationBuilder.Sql(@"
INSERT INTO cajas.pago_externo_producto (id, nombre, titulo_referencia, requiere_datos_factura, tiene_comision, activo) VALUES
(1, 'CNEL- REGIONAL - MANABI', 'CODIGO UNICO ELECTRICO', false, false, true),
(2, 'AGUA PORTOVIEJO - PORTOAGUAS', 'NUMERO DE CUENTA', false, false, true),
(3, 'CNT - FIJA-TELEVISION-INTERNET', 'TELEFONO/CONTRATO', false, false, true),
(4, 'CLARO PLANES MOVIL', 'NUMERO CELULAR', false, false, true),
(5, 'MOVISTAR', 'NUMERO CELULAR', false, false, true),
(6, 'IESS (TODOS)', 'CEDULA/RUC/COMPROBANTE', false, false, true),
(7, 'CNT - MOVIL', 'NUMERO CELULAR', false, false, true),
(8, 'IESS (PLANI)-PAGO DE PLANILLAS (Empleadores/Afiliados)', 'NUMERO DE PLANILLA', false, false, true),
(9, 'NETLIFE', 'CEDULA/RUC', false, false, true),
(10, 'CATALOGO LEONISA', 'CEDULA/RUC', false, false, true),
(11, 'TVCABLE', 'NUMERO DE CONTRATO', false, false, true),
(12, 'ROCAFUERTE EPAPAR - AGUA', 'CEDULA/RUC', false, false, true),
(13, 'BOMBEROS PORTOVIEJO', 'CEDULA/RUC', false, false, true),
(14, 'PYCCA', 'CEDULA/RUC', false, false, true),
(15, 'MATRICULACION - VEHICULAR', 'PLACA/RAMV', false, false, true),
(16, 'AZZORTI', 'CEDULA/RUC', false, false, true),
(17, 'YANBAL', 'NUMERO DE CUENTA', false, false, true),
(18, 'CNEL- REGIONAL - SANTO DOMINGO', 'CODIGO UNICO ELECTRICO', false, false, true),
(19, 'EPAM - MANTA - CONTRATO', 'MEDIDOR', false, false, true),
(20, 'PLANES ALFANET', 'CEDULA/RUC', false, false, true),
(21, 'PORTOVIEJO', 'CEDULA/RUC', false, false, true),
(22, 'BANCO SOLIDARIO ALIA TARJETA DE CREDITO', 'NUMERO DE TARJETA', false, false, true),
(23, 'CONSEJO JUDICATURA PENSION ALIMENTICIA - PERSONA', 'NUMERO TARJETA SUPA', false, false, true),
(24, 'SAITEL', 'CEDULA/RUC', false, false, true),
(25, 'CHONE - AGUA', 'CEDULA/RUC', false, false, true),
(26, 'UNICOMER - ARTEFACTA', 'CEDULA/RUC/FACTURA', false, false, true),
(27, 'MANCOMUNIDAD EMMAPEP', 'CEDULA/RUC/FACTURA', false, false, true),
(28, 'MANABI VIAL', 'CEDULA/RUC', false, false, true),
(29, 'CNEL GUAYAQUIL - (03305)', 'CUENTA CONTRATO', false, false, true);
");

            migrationBuilder.CreateTable(
                name: "pago_externo_transaccion",
                schema: "cajas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_producto = table.Column<int>(type: "integer", nullable: false),
                    referencia = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    documento = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    valor = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    comision = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    id_agencia = table.Column<int>(type: "integer", nullable: false),
                    fecha_proceso = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    registrado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    reversada = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_reverso = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    reversada_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    id_comprobante = table.Column<Guid>(type: "uuid", nullable: true),
                    id_comprobante_reverso = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pago_externo_transaccion", x => x.id);
                    table.ForeignKey(
                        name: "fk_pago_externo_transaccion_pago_externo_producto_id_producto",
                        column: x => x.id_producto,
                        principalSchema: "cajas",
                        principalTable: "pago_externo_producto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_pago_externo_transaccion_fecha_proceso",
                schema: "cajas",
                table: "pago_externo_transaccion",
                column: "fecha_proceso");

            migrationBuilder.CreateIndex(
                name: "ix_pago_externo_transaccion_id_producto",
                schema: "cajas",
                table: "pago_externo_transaccion",
                column: "id_producto");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pago_externo_transaccion",
                schema: "cajas");

            migrationBuilder.DropTable(
                name: "pago_externo_producto",
                schema: "cajas");
        }
    }
}
