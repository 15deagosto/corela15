using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Contabilidad_MotorCompras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "proveedor",
                schema: "contabilidad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_persona = table.Column<Guid>(type: "uuid", nullable: true),
                    id_tipo_identificacion = table.Column<int>(type: "integer", nullable: false),
                    identificacion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    direccion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    telefono = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    es_contribuyente_especial = table.Column<bool>(type: "boolean", nullable: false),
                    obligado_llevar_contabilidad = table.Column<bool>(type: "boolean", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_proveedor", x => x.id);
                    table.ForeignKey(
                        name: "fk_proveedor_persona_id_persona",
                        column: x => x.id_persona,
                        principalSchema: "sujeto",
                        principalTable: "persona",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_proveedor_tipos_identificacion_id_tipo_identificacion",
                        column: x => x.id_tipo_identificacion,
                        principalSchema: "general",
                        principalTable: "tipo_identificacion",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tipo_comprobante_compra",
                schema: "contabilidad",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipo_comprobante_compra", x => x.codigo);
                });

            // Catálogo real — verificado contra CONTABILIDAD.ATS_TIPOCOMPROBANTE,
            // sembrado solo con los 7 códigos que tienen uso real en
            // CONTABILIDAD.COMPRAS de esta cooperativa (3.029 filas reales).
            migrationBuilder.Sql(@"
INSERT INTO contabilidad.tipo_comprobante_compra (codigo, nombre, activo) VALUES
('01', 'Factura', true),
('02', 'Nota o boleta de venta', true),
('03', 'Liquidación de compra de bienes o prestación de servicios', true),
('04', 'Nota de crédito', true),
('20', 'Documentos por servicios administrativos emitidos por Inst. del Estado', true),
('41', 'Comprobante de venta emitido por reembolso', true),
('47', 'Nota de crédito por reembolso emitida por intermediario', true);
");

            migrationBuilder.CreateTable(
                name: "compra",
                schema: "contabilidad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    id_proveedor = table.Column<Guid>(type: "uuid", nullable: false),
                    id_agencia = table.Column<int>(type: "integer", nullable: false),
                    codigo_sustento = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    codigo_tipo_comprobante = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    establecimiento = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    punto_emision = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    secuencial = table.Column<string>(type: "character varying(18)", maxLength: 18, nullable: false),
                    autorizacion = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    fecha_emision = table.Column<DateOnly>(type: "date", nullable: false),
                    concepto = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    subtotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    monto_iva = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    monto_retencion = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    monto_inicial = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    saldo = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fecha_registro = table.Column<DateOnly>(type: "date", nullable: false),
                    registrado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    id_comprobante = table.Column<Guid>(type: "uuid", nullable: true),
                    id_comprobante_reverso = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_compra", x => x.id);
                    table.ForeignKey(
                        name: "fk_compra_agencia_id_agencia",
                        column: x => x.id_agencia,
                        principalSchema: "general",
                        principalTable: "agencia",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_compra_proveedores_id_proveedor",
                        column: x => x.id_proveedor,
                        principalSchema: "contabilidad",
                        principalTable: "proveedor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_compra_tipo_comprobante_compra_codigo_tipo_comprobante",
                        column: x => x.codigo_tipo_comprobante,
                        principalSchema: "contabilidad",
                        principalTable: "tipo_comprobante_compra",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "compra_detalle",
                schema: "contabilidad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_compra = table.Column<Guid>(type: "uuid", nullable: false),
                    id_cuenta_contable = table.Column<Guid>(type: "uuid", nullable: false),
                    detalle = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    cantidad = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    valor_unitario = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    porcentaje_iva = table.Column<decimal>(type: "numeric(6,4)", nullable: false),
                    subtotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    monto_iva = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    total = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_compra_detalle", x => x.id);
                    table.ForeignKey(
                        name: "fk_compra_detalle_compra_id_compra",
                        column: x => x.id_compra,
                        principalSchema: "contabilidad",
                        principalTable: "compra",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_compra_detalle_cuentas_contables_id_cuenta_contable",
                        column: x => x.id_cuenta_contable,
                        principalSchema: "contabilidad",
                        principalTable: "cuenta_contable",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_compra_codigo_tipo_comprobante",
                schema: "contabilidad",
                table: "compra",
                column: "codigo_tipo_comprobante");

            migrationBuilder.CreateIndex(
                name: "ix_compra_id_agencia",
                schema: "contabilidad",
                table: "compra",
                column: "id_agencia");

            migrationBuilder.CreateIndex(
                name: "ix_compra_id_proveedor",
                schema: "contabilidad",
                table: "compra",
                column: "id_proveedor");

            migrationBuilder.CreateIndex(
                name: "ix_compra_numero",
                schema: "contabilidad",
                table: "compra",
                column: "numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_compra_detalle_id_compra",
                schema: "contabilidad",
                table: "compra_detalle",
                column: "id_compra");

            migrationBuilder.CreateIndex(
                name: "ix_compra_detalle_id_cuenta_contable",
                schema: "contabilidad",
                table: "compra_detalle",
                column: "id_cuenta_contable");

            migrationBuilder.CreateIndex(
                name: "ix_proveedor_id_persona",
                schema: "contabilidad",
                table: "proveedor",
                column: "id_persona");

            migrationBuilder.CreateIndex(
                name: "ix_proveedor_id_tipo_identificacion",
                schema: "contabilidad",
                table: "proveedor",
                column: "id_tipo_identificacion");

            migrationBuilder.CreateIndex(
                name: "ix_proveedor_identificacion",
                schema: "contabilidad",
                table: "proveedor",
                column: "identificacion",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "compra_detalle",
                schema: "contabilidad");

            migrationBuilder.DropTable(
                name: "compra",
                schema: "contabilidad");

            migrationBuilder.DropTable(
                name: "proveedor",
                schema: "contabilidad");

            migrationBuilder.DropTable(
                name: "tipo_comprobante_compra",
                schema: "contabilidad");
        }
    }
}
