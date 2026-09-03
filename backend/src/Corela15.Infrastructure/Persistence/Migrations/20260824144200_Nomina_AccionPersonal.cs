using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Nomina_AccionPersonal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "estado_accion_personal",
                schema: "nomina",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_estado_accion_personal", x => x.codigo);
                });

            // Catálogo real — verificado contra NOMINA.ESTADO_ACCIONPERSONAL (3 filas).
            migrationBuilder.Sql(@"
INSERT INTO nomina.estado_accion_personal (codigo, nombre, activo) VALUES
('IN', 'Ingresada', true),
('AP', 'Aprobada', true),
('AN', 'Anulada', true);
");

            migrationBuilder.CreateTable(
                name: "tipo_accion_personal",
                schema: "nomina",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    detalle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    activa_contrato = table.Column<bool>(type: "boolean", nullable: false),
                    desactiva_contrato = table.Column<bool>(type: "boolean", nullable: false),
                    es_cambio_cargo_sueldo = table.Column<bool>(type: "boolean", nullable: false),
                    es_cambio_agencia_departamento = table.Column<bool>(type: "boolean", nullable: false),
                    es_forma_pago_decimos = table.Column<bool>(type: "boolean", nullable: false),
                    es_forma_pago_fondos_reserva = table.Column<bool>(type: "boolean", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipo_accion_personal", x => x.id);
                });

            // Catálogo real completo — verificado contra NOMINA.ACCION_PERSONAL
            // (17 filas reales, IDs 2-18 preservados idénticos a la fuente).
            // Solo las banderas con efecto real implementado en
            // SolicitudAccionPersonalService quedan en true según la fuente;
            // el resto de columnas reales (EsLiquidacion, EsPagoVacacion,
            // EsSancion...) no se modelaron — sin caso de uso todavía.
            migrationBuilder.Sql(@"
INSERT INTO nomina.tipo_accion_personal
    (id, detalle, activa_contrato, desactiva_contrato, es_cambio_cargo_sueldo, es_cambio_agencia_departamento, es_forma_pago_decimos, es_forma_pago_fondos_reserva, activo) VALUES
(2, 'Activación de contrato', true, false, false, false, false, false, true),
(3, 'Cambio de cargo / ingresos del empleado', false, false, true, false, false, false, true),
(4, 'Cambio de agencia / departamento', false, false, false, true, false, false, true),
(5, 'Forma pago décimos', false, false, false, false, true, false, true),
(6, 'Forma pago fondos de reserva', false, false, false, false, false, true, true),
(7, 'Anticipo de sueldo', false, false, false, false, false, false, true),
(8, 'Desactiva contrato', false, true, false, false, false, false, true),
(9, 'Liquidación del empleado', false, false, false, false, false, false, true),
(10, 'Pago de vacación', false, false, false, false, false, false, true),
(11, 'Cambio de pasante a empleado', false, false, false, false, false, false, true),
(12, 'Reingreso del personal', false, false, false, false, false, false, true),
(13, 'Añadir empleado a vinculados', false, false, false, false, false, false, true),
(14, 'Eliminar empleado de vinculados', false, false, false, false, false, false, true),
(15, 'Sanción económica (hasta el 10% de la remuneración mensual)', false, false, false, false, false, false, true),
(16, 'Sanción verbal', false, false, false, false, false, false, true),
(17, 'Sanción escrita', false, false, false, false, false, false, true),
(18, 'Acción de prueba', false, false, false, false, false, false, true);
");

            migrationBuilder.CreateTable(
                name: "solicitud_accion_personal",
                schema: "nomina",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero_accion = table.Column<int>(type: "integer", nullable: false),
                    id_empleado = table.Column<Guid>(type: "uuid", nullable: false),
                    id_tipo_accion_personal = table.Column<int>(type: "integer", nullable: false),
                    detalle = table.Column<string>(type: "character varying(800)", maxLength: 800, nullable: false),
                    codigo_estado = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    id_usuario = table.Column<Guid>(type: "uuid", nullable: false),
                    id_cargo_nuevo = table.Column<int>(type: "integer", nullable: true),
                    id_agencia_nueva = table.Column<int>(type: "integer", nullable: true),
                    nuevo_valor_recibe_fondos_reserva = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_solicitud_accion_personal", x => x.id);
                    table.ForeignKey(
                        name: "fk_solicitud_accion_personal_empleado_id_empleado",
                        column: x => x.id_empleado,
                        principalSchema: "nomina",
                        principalTable: "empleado",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_solicitud_accion_personal_estado_accion_personal_codigo_est",
                        column: x => x.codigo_estado,
                        principalSchema: "nomina",
                        principalTable: "estado_accion_personal",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_solicitud_accion_personal_tipos_accion_personal_id_tipo_acc",
                        column: x => x.id_tipo_accion_personal,
                        principalSchema: "nomina",
                        principalTable: "tipo_accion_personal",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "solicitud_accion_personal_etapa",
                schema: "nomina",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_solicitud_accion_personal = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_estado = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    comentario = table.Column<string>(type: "text", nullable: true),
                    fecha = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    registrado_por = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_solicitud_accion_personal_etapa", x => x.id);
                    table.ForeignKey(
                        name: "fk_solicitud_accion_personal_etapa_solicitud_accion_personal_i",
                        column: x => x.id_solicitud_accion_personal,
                        principalSchema: "nomina",
                        principalTable: "solicitud_accion_personal",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_solicitud_accion_personal_codigo_estado",
                schema: "nomina",
                table: "solicitud_accion_personal",
                column: "codigo_estado");

            migrationBuilder.CreateIndex(
                name: "ix_solicitud_accion_personal_id_empleado",
                schema: "nomina",
                table: "solicitud_accion_personal",
                column: "id_empleado");

            migrationBuilder.CreateIndex(
                name: "ix_solicitud_accion_personal_id_tipo_accion_personal",
                schema: "nomina",
                table: "solicitud_accion_personal",
                column: "id_tipo_accion_personal");

            migrationBuilder.CreateIndex(
                name: "ix_solicitud_accion_personal_etapa_id_solicitud_accion_personal",
                schema: "nomina",
                table: "solicitud_accion_personal_etapa",
                column: "id_solicitud_accion_personal");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "solicitud_accion_personal_etapa",
                schema: "nomina");

            migrationBuilder.DropTable(
                name: "solicitud_accion_personal",
                schema: "nomina");

            migrationBuilder.DropTable(
                name: "estado_accion_personal",
                schema: "nomina");

            migrationBuilder.DropTable(
                name: "tipo_accion_personal",
                schema: "nomina");
        }
    }
}
