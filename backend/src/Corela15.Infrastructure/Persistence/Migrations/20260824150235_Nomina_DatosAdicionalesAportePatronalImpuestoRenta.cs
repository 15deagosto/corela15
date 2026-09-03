using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Nomina_DatosAdicionalesAportePatronalImpuestoRenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "calculo_impuesto_renta",
                schema: "nomina",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_empleado = table.Column<Guid>(type: "uuid", nullable: false),
                    anio = table.Column<int>(type: "integer", nullable: false),
                    ingreso_anual_proyectado = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    base_imponible = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    impuesto_causado_anual = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    retencion_mensual = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    fecha_calculo = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_calculo_impuesto_renta", x => x.id);
                    table.ForeignKey(
                        name: "fk_calculo_impuesto_renta_empleados_id_empleado",
                        column: x => x.id_empleado,
                        principalSchema: "nomina",
                        principalTable: "empleado",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "empleado_aporte_patronal",
                schema: "nomina",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_empleado = table.Column<Guid>(type: "uuid", nullable: false),
                    proyectado = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    acumulado = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    pagado = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ultimo_devengo = table.Column<DateOnly>(type: "date", nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_empleado_aporte_patronal", x => x.id);
                    table.ForeignKey(
                        name: "fk_empleado_aporte_patronal_empleados_id_empleado",
                        column: x => x.id_empleado,
                        principalSchema: "nomina",
                        principalTable: "empleado",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "empleado_datos_adicionales",
                schema: "nomina",
                columns: table => new
                {
                    id_empleado = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_iess = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    fecha_ingreso_iess = table.Column<DateOnly>(type: "date", nullable: true),
                    fecha_salida_iess = table.Column<DateOnly>(type: "date", nullable: true),
                    fecha_ingreso_ministerio_laboral = table.Column<DateOnly>(type: "date", nullable: true),
                    fecha_salida_ministerio_laboral = table.Column<DateOnly>(type: "date", nullable: true),
                    numero_cargas_familiares = table.Column<int>(type: "integer", nullable: true),
                    pago_decimo_mensual = table.Column<bool>(type: "boolean", nullable: false),
                    pago_fondos_reserva_rol = table.Column<bool>(type: "boolean", nullable: false),
                    extension_conyugal = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_empleado_datos_adicionales", x => x.id_empleado);
                    table.ForeignKey(
                        name: "fk_empleado_datos_adicionales_empleado_id_empleado",
                        column: x => x.id_empleado,
                        principalSchema: "nomina",
                        principalTable: "empleado",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tipo_contrato",
                schema: "nomina",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    es_tiempo_completo = table.Column<bool>(type: "boolean", nullable: false),
                    es_tiempo_parcial = table.Column<bool>(type: "boolean", nullable: false),
                    tiene_fecha_salida = table.Column<bool>(type: "boolean", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipo_contrato", x => x.codigo);
                });

            // Catálogo real — verificado contra NOMINA.TIPO_CONTRATO (6 filas).
            migrationBuilder.Sql(@"
INSERT INTO nomina.tipo_contrato (codigo, nombre, es_tiempo_completo, es_tiempo_parcial, tiene_fecha_salida, activo) VALUES
('AP', 'A prueba', true, false, false, true),
('CI', 'Contrato indefinido', true, false, false, true),
('TC', 'Tiempo completo', true, false, false, true),
('TF', 'Término fijo', true, false, false, true),
('TI', 'Término indefinido', true, false, false, true),
('TP', 'Tiempo parcial', false, true, false, true);
");

            migrationBuilder.CreateTable(
                name: "tramo_impuesto_renta",
                schema: "nomina",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    fraccion_basica = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    exceso_hasta = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    impuesto_fraccion_basica = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    porcentaje_excedente = table.Column<decimal>(type: "numeric(5,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tramo_impuesto_renta", x => x.id);
                });

            // Tabla real de tramos del Impuesto a la Renta para personas
            // naturales en relación de dependencia — verificado contra
            // NOMINA.PERIODO_FISCAL_IMPUESTORENTA, período fiscal 2023
            // (IDPERIODO=4, vigente, 10 tramos reales fila por fila).
            migrationBuilder.Sql(@"
INSERT INTO nomina.tramo_impuesto_renta (id, fraccion_basica, exceso_hasta, impuesto_fraccion_basica, porcentaje_excedente) VALUES
(1, 0.00, 11722.00, 0.00, 0.00),
(2, 11722.00, 14935.00, 0.00, 5.00),
(3, 14935.00, 18666.00, 161.00, 10.00),
(4, 18666.00, 22418.00, 534.00, 12.00),
(5, 22418.00, 32783.00, 984.00, 15.00),
(6, 32783.00, 43147.00, 2539.00, 20.00),
(7, 43147.00, 53512.00, 4612.00, 25.00),
(8, 53512.00, 63876.00, 7203.00, 30.00),
(9, 63876.00, 103644.00, 10312.00, 35.00),
(10, 103644.00, 99999999.00, 24231.00, 37.00);
");

            migrationBuilder.CreateTable(
                name: "empleado_contrato",
                schema: "nomina",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_empleado = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_tipo_contrato = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    numero_contrato = table.Column<int>(type: "integer", nullable: false),
                    fecha_ingreso = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_salida = table.Column<DateOnly>(type: "date", nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_empleado_contrato", x => x.id);
                    table.ForeignKey(
                        name: "fk_empleado_contrato_empleado_id_empleado",
                        column: x => x.id_empleado,
                        principalSchema: "nomina",
                        principalTable: "empleado",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_empleado_contrato_tipo_contrato_codigo_tipo_contrato",
                        column: x => x.codigo_tipo_contrato,
                        principalSchema: "nomina",
                        principalTable: "tipo_contrato",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Restrict);
                });

            // Fila vacía de datos adicionales para cada empleado ya
            // sembrado — el resto de altas nuevas la crea EmpleadoService.
            migrationBuilder.Sql(@"
INSERT INTO nomina.empleado_datos_adicionales (id_empleado, pago_decimo_mensual, pago_fondos_reserva_rol, extension_conyugal)
SELECT id, false, false, false FROM nomina.empleado;
");

            migrationBuilder.CreateIndex(
                name: "ix_calculo_impuesto_renta_id_empleado_anio",
                schema: "nomina",
                table: "calculo_impuesto_renta",
                columns: new[] { "id_empleado", "anio" });

            migrationBuilder.CreateIndex(
                name: "ix_empleado_aporte_patronal_id_empleado",
                schema: "nomina",
                table: "empleado_aporte_patronal",
                column: "id_empleado",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_empleado_contrato_codigo_tipo_contrato",
                schema: "nomina",
                table: "empleado_contrato",
                column: "codigo_tipo_contrato");

            migrationBuilder.CreateIndex(
                name: "ix_empleado_contrato_id_empleado",
                schema: "nomina",
                table: "empleado_contrato",
                column: "id_empleado");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "calculo_impuesto_renta",
                schema: "nomina");

            migrationBuilder.DropTable(
                name: "empleado_aporte_patronal",
                schema: "nomina");

            migrationBuilder.DropTable(
                name: "empleado_contrato",
                schema: "nomina");

            migrationBuilder.DropTable(
                name: "empleado_datos_adicionales",
                schema: "nomina");

            migrationBuilder.DropTable(
                name: "tramo_impuesto_renta",
                schema: "nomina");

            migrationBuilder.DropTable(
                name: "tipo_contrato",
                schema: "nomina");
        }
    }
}
