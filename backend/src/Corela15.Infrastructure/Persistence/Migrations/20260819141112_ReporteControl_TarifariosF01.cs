using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReporteControl_TarifariosF01 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tarifa_gasto_cobranza",
                schema: "reportecontrol",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    cuota_inicio = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    cuota_final = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    dia_mora_inicio = table.Column<int>(type: "integer", nullable: false),
                    dia_mora_fin = table.Column<int>(type: "integer", nullable: false),
                    detalle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    valor = table.Column<decimal>(type: "numeric(9,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tarifa_gasto_cobranza", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tarifa_servicio_financiero",
                schema: "reportecontrol",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    tarifa = table.Column<decimal>(type: "numeric(9,2)", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tarifa_servicio_financiero", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_tarifa_gasto_cobranza_codigo",
                schema: "reportecontrol",
                table: "tarifa_gasto_cobranza",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tarifa_servicio_financiero_codigo",
                schema: "reportecontrol",
                table: "tarifa_servicio_financiero",
                column: "codigo",
                unique: true);

            // Datos reales verificados contra REPORTECONTROL.F01_TARIFARIO (Softbank,
            // solo lectura) — 21 filas fuente, 20 códigos únicos: SFM020 aparece dos
            // veces en la fuente (ID 43 e ID 65, mismo nombre y misma tarifa 1.92,
            // solo difiere IDTRANSACCION interno de Softbank) — deduplicado acá porque
            // el catálogo real de servicios SEPS es por código, no por vínculo interno.
            migrationBuilder.Sql(@"
INSERT INTO reportecontrol.tarifa_servicio_financiero (codigo, nombre, tarifa, activo) VALUES
('SFM010', 'Emisión de referencias financieras', 2.00, true),
('SFM020', 'Transferencias SPI enviadas, oficina', 1.92, true),
('SFM021', 'Transferencias interbancarias SPI recibidas', 0.27, true),
('SFM003', 'Cheque devuelto nacional', 2.49, true),
('SFM001', 'Cheque de emergencia', 2.23, true),
('SFM007', 'Impresión consulta por cajero automático', 0.31, true),
('SFM022', 'Transferencia nacionales otras entidades oficina', 2.00, true),
('SFM030', 'Servicio de Emisión de Tarjetas', 4.37, true),
('SFM031', 'Servicio de Renovación de Tarjetas', 1.57, true),
('SFM057', 'Gestión de Cobranza Extrajudicial', 7.35, true),
('SFB002', 'Apertura Cuentas de Ahorro', 0.00, true),
('SFB004', 'Apertura de Plazos Fijos', 0.00, true),
('SFB005', 'Depósitos de Cuentas de Ahorro', 0.00, true),
('SFB007', 'Depósitos de Plazo Fijo', 0.00, true),
('SFB017', 'Retiro de Dinero Cajero Automático', 0.31, true),
('SFB018', 'Retiro de Dinero Ventanilla', 0.00, true),
('SFB020', 'Transferencia entre cuentas internas', 0.00, true),
('SFB021', 'Cierre de cuentas de ahorro', 0.00, true),
('SFB038', 'Reposición de Tarjetas', 0.00, true),
('SFM028', 'Reposición libreta/cartola', 1.00, true);

-- Datos reales verificados contra REPORTECONTROL.F01_TARIFARIO_GASTOCOBRANZA
-- (Softbank, solo lectura) — 24 filas, escalonado por rango de cuota × rango
-- de días de mora, códigos SFM057-SFM080.
INSERT INTO reportecontrol.tarifa_gasto_cobranza (codigo, cuota_inicio, cuota_final, dia_mora_inicio, dia_mora_fin, detalle, valor) VALUES
('SFM077', 0.00, 99.99, 1, 30, 'Cobranza extrajudicial rango de cuota menor a $ 100 (de 1 a 30 días)', 6.38),
('SFM078', 0.00, 99.99, 31, 60, 'Cobranza extrajudicial rango de cuota menor a $ 100 (de 31 a 60 días)', 16.23),
('SFM079', 0.00, 99.99, 61, 90, 'Cobranza extrajudicial rango de cuota menor a $ 100 (de 61 a 90 días)', 23.17),
('SFM080', 0.00, 99.99, 91, 99999, 'Cobranza extrajudicial rango de cuota menor a $ 100 (más de 90 días)', 25.56),
('SFM057', 100.00, 199.99, 1, 30, 'Cobranza extrajudicial rango de cuota de $ 100 a 199 (de 1 a 30 días)', 7.35),
('SFM058', 100.00, 199.99, 31, 60, 'Cobranza extrajudicial rango de cuota de $ 100 a 199 (de 31 a 60 días)', 16.46),
('SFM059', 100.00, 199.99, 61, 90, 'Cobranza extrajudicial rango de cuota de $ 100 a 199 (de 61 a 90 días)', 23.85),
('SFM060', 100.00, 199.99, 91, 99999, 'Cobranza extrajudicial rango de cuota de $ 100 a 199 (más de 90 días)', 26.64),
('SFM061', 200.00, 299.99, 1, 30, 'Cobranza extrajudicial rango de cuota de $ 200 a 299 (de 1 a 30 días)', 7.92),
('SFM062', 200.00, 299.99, 31, 60, 'Cobranza extrajudicial rango de cuota de $ 200 a 299 (de 31 a 60 días)', 17.83),
('SFM063', 200.00, 299.99, 61, 90, 'Cobranza extrajudicial rango de cuota de $ 200 a 299 (de 61 a 90 días)', 25.27),
('SFM064', 200.00, 299.99, 91, 99999, 'Cobranza extrajudicial rango de cuota de $ 200 a 299 (más de 90 días)', 29.03),
('SFM065', 300.00, 499.99, 1, 30, 'Cobranza extrajudicial rango de cuota de $ 300 a 499 (de 1 a 30 días)', 8.32),
('SFM066', 300.00, 499.99, 31, 60, 'Cobranza extrajudicial rango de cuota de $ 300 a 499 (de 31 a 60 días)', 20.34),
('SFM067', 300.00, 499.99, 61, 90, 'Cobranza extrajudicial rango de cuota de $ 300 a 499 (de 61 a 90 días)', 27.43),
('SFM068', 300.00, 499.99, 91, 99999, 'Cobranza extrajudicial rango de cuota de $ 300 a 499 (más de 90 días)', 32.72),
('SFM069', 500.00, 999.99, 1, 30, 'Cobranza extrajudicial rango de cuota de $ 500 a 999 (de 1 a 30 días)', 8.63),
('SFM070', 500.00, 999.99, 31, 60, 'Cobranza extrajudicial rango de cuota de $ 500 a 999 (de 31 a 60 días)', 23.99),
('SFM071', 500.00, 999.99, 61, 90, 'Cobranza extrajudicial rango de cuota de $ 500 a 999 (de 61 a 90 días)', 30.34),
('SFM072', 500.00, 999.99, 91, 99999, 'Cobranza extrajudicial rango de cuota de $ 500 a 999 (más de 90 días)', 37.70),
('SFM073', 1000.00, 9999999.00, 1, 30, 'Cobranza extrajudicial rango de cuota mayor a $ 1.000 (de 1 a 30 días)', 8.88),
('SFM074', 1000.00, 9999999.00, 31, 60, 'Cobranza extrajudicial rango de cuota mayor a $ 1.000 (de 31 a 60 días)', 28.78),
('SFM075', 1000.00, 9999999.00, 61, 90, 'Cobranza extrajudicial rango de cuota mayor a $ 1.000 (de 61 a 90 días)', 34.01),
('SFM076', 1000.00, 9999999.00, 91, 99999, 'Cobranza extrajudicial rango de cuota mayor a $ 1.000 (más de 90 días)', 43.99);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tarifa_gasto_cobranza",
                schema: "reportecontrol");

            migrationBuilder.DropTable(
                name: "tarifa_servicio_financiero",
                schema: "reportecontrol");
        }
    }
}
