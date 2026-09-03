using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Sujeto_ReclamosOrganoGobierno : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "canal_reclamo",
                schema: "sujeto",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_canal_reclamo", x => x.codigo);
                });

            // Catálogo real — verificado contra SUJETO.PERSONA_RECLAMO_CANAL (3 filas).
            migrationBuilder.Sql(@"
INSERT INTO sujeto.canal_reclamo (codigo, nombre, activo) VALUES
('P', 'Presencial', true),
('T', 'Telefónico', true),
('W', 'Web', true);
");

            migrationBuilder.CreateTable(
                name: "concepto_reclamo",
                schema: "sujeto",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_concepto_reclamo", x => x.codigo);
                });

            // Catálogo real — verificado contra SUJETO.PERSONA_RECLAMO_CONCEPTO (3 filas).
            migrationBuilder.Sql(@"
INSERT INTO sujeto.concepto_reclamo (codigo, nombre, activo) VALUES
('CC', 'Cartera de crédito', true),
('OP', 'Obligaciones con el público: Cuentas de ahorros/cuenta básica/Depósitos a plazo fijo', true),
('TC', 'Tarjetas de crédito', true);
");

            migrationBuilder.CreateTable(
                name: "estado_reclamo",
                schema: "sujeto",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_estado_reclamo", x => x.codigo);
                });

            // Catálogo real — verificado contra SUJETO.PERSONA_RECLAMO_ESTADO (2 filas).
            migrationBuilder.Sql(@"
INSERT INTO sujeto.estado_reclamo (codigo, nombre, activo) VALUES
('1', 'En trámite', true),
('2', 'Resuelto', true);
");

            migrationBuilder.CreateTable(
                name: "miembro_organo_gobierno",
                schema: "sujeto",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_persona = table.Column<Guid>(type: "uuid", nullable: false),
                    es_asamblea_general = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_inicia_asamblea_general = table.Column<DateOnly>(type: "date", nullable: true),
                    fecha_termina_asamblea_general = table.Column<DateOnly>(type: "date", nullable: true),
                    es_consejo_administracion = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_inicia_consejo_administracion = table.Column<DateOnly>(type: "date", nullable: true),
                    fecha_termina_consejo_administracion = table.Column<DateOnly>(type: "date", nullable: true),
                    es_consejo_vigilancia = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_inicia_consejo_vigilancia = table.Column<DateOnly>(type: "date", nullable: true),
                    fecha_termina_consejo_vigilancia = table.Column<DateOnly>(type: "date", nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    registrado_por = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_miembro_organo_gobierno", x => x.id);
                    table.ForeignKey(
                        name: "fk_miembro_organo_gobierno_personas_id_persona",
                        column: x => x.id_persona,
                        principalSchema: "sujeto",
                        principalTable: "persona",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tipo_producto_reclamo",
                schema: "sujeto",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    codigo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipo_producto_reclamo", x => x.id);
                });

            // Catálogo real — verificado contra SUJETO.TIPO_PRODUCTO_RECLAMO (3 filas).
            migrationBuilder.Sql(@"
INSERT INTO sujeto.tipo_producto_reclamo (id, codigo, nombre, activo) VALUES
(1, '001', 'Ahorros', true),
(2, '002', 'Préstamo', true),
(3, '003', 'Inversiones', true);
");

            migrationBuilder.CreateTable(
                name: "tipo_resolucion_reclamo",
                schema: "sujeto",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipo_resolucion_reclamo", x => x.codigo);
                });

            // Catálogo real — verificado contra SUJETO.PERSONA_RECLAMO_TIPORESOLUCION (4 filas).
            migrationBuilder.Sql(@"
INSERT INTO sujeto.tipo_resolucion_reclamo (codigo, nombre, activo) VALUES
('DS', 'Desistimiento del socio o cliente', true),
('DU', 'Desfavorable para el socio o cliente', true),
('FU', 'Favorable para el socio o cliente', true),
('PU', 'Parcialmente a favor del socio o cliente', true);
");

            migrationBuilder.CreateTable(
                name: "concepto_reclamo_detalle",
                schema: "sujeto",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    codigo_concepto = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_concepto_reclamo_detalle", x => x.codigo);
                    table.ForeignKey(
                        name: "fk_concepto_reclamo_detalle_concepto_reclamo_codigo_concepto",
                        column: x => x.codigo_concepto,
                        principalSchema: "sujeto",
                        principalTable: "concepto_reclamo",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Restrict);
                });

            // Catálogo real completo — verificado contra SUJETO.PERSONA_RECLAMO_
            // CONCEPTO_DETALLE (26 filas, catálogo oficial SEPS de tipificación
            // de reclamos por cobros indebidos).
            migrationBuilder.Sql(@"
INSERT INTO sujeto.concepto_reclamo_detalle (codigo, codigo_concepto, descripcion, activo) VALUES
('CC01', 'CC', 'Cargos por servicios financieros básicos', true),
('CC02', 'CC', 'Servicios financieros con cargos máximos cuyo cobro excede el límite establecido en la norma', true),
('CC03', 'CC', 'Servicios financieros con cargo diferenciado cuyo cobro excede el valor aprobado', true),
('CC04', 'CC', 'Cargos en servicios no financieros que exceden los valores facturados por el prestador del servicio', true),
('CC05', 'CC', 'Cobros por servicios no financieros que no estén autorizados', true),
('CC06', 'CC', 'Comisiones o cargos no autorizados en operaciones de crédito', true),
('CC07', 'CC', 'Crédito no autorizado', true),
('CC08', 'CC', 'Falta de registro en pagos de la operación por parte de la entidad', true),
('CC09', 'CC', 'Imposición de castigos por pagos anticipados o negativa a recibir y registrar pagos anticipados', true),
('CC10', 'CC', 'Anatocismo', true),
('CC11', 'CC', 'Cobros por intereses no devengados o superiores a los máximos vigentes', true),
('OP01', 'OP', 'Cargos por servicios financieros básicos', true),
('OP02', 'OP', 'Servicios financieros con cargos máximos cuyo cobro excede el límite establecido en la norma', true),
('OP03', 'OP', 'Servicios financieros con cargo diferenciado cuyo cobro excede el valor aprobado', true),
('OP04', 'OP', 'Cobros por consumos no autorizados con tarjeta de débito', true),
('OP05', 'OP', 'Cargos en servicios no financieros que exceden los valores facturados por el prestador del servicio', true),
('OP06', 'OP', 'Cobros por servicios no financieros que no estén autorizados', true),
('OP07', 'OP', 'Descuentos no autorizados en capital o intereses de certificados de depósitos a plazo fijo', true),
('TC01', 'TC', 'Cargos por servicios financieros básicos', true),
('TC02', 'TC', 'Servicios financieros con cargos máximos cuyo cobro excede el límite establecido en la norma', true),
('TC03', 'TC', 'Servicios financieros con cargo diferenciado cuyo cobro excede el valor aprobado', true),
('TC04', 'TC', 'Cobros por consumos no autorizados con tarjetas de crédito', true),
('TC05', 'TC', 'Cargos en servicios no financieros que exceden los valores facturados por el prestador del servicio', true),
('TC06', 'TC', 'Cobros por servicios no financieros que no estén autorizados', true),
('TC07', 'TC', 'Emisión de tarjeta de crédito no autorizada', true),
('TC08', 'TC', 'Solicitud de bloqueo no realizada por la entidad', true);
");

            migrationBuilder.CreateTable(
                name: "reclamo",
                schema: "sujeto",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_persona = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_canal_recepcion = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    fecha_recepcion = table.Column<DateOnly>(type: "date", nullable: false),
                    id_tipo_producto = table.Column<int>(type: "integer", nullable: false),
                    codigo_concepto_detalle = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    codigo_estado = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    registrado_por = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reclamo", x => x.id);
                    table.ForeignKey(
                        name: "fk_reclamo_canal_reclamo_codigo_canal_recepcion",
                        column: x => x.codigo_canal_recepcion,
                        principalSchema: "sujeto",
                        principalTable: "canal_reclamo",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_reclamo_concepto_reclamo_detalle_codigo_concepto_detalle",
                        column: x => x.codigo_concepto_detalle,
                        principalSchema: "sujeto",
                        principalTable: "concepto_reclamo_detalle",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_reclamo_estado_reclamo_codigo_estado",
                        column: x => x.codigo_estado,
                        principalSchema: "sujeto",
                        principalTable: "estado_reclamo",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_reclamo_persona_id_persona",
                        column: x => x.id_persona,
                        principalSchema: "sujeto",
                        principalTable: "persona",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_reclamo_tipos_producto_reclamo_id_tipo_producto",
                        column: x => x.id_tipo_producto,
                        principalSchema: "sujeto",
                        principalTable: "tipo_producto_reclamo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "reclamo_respuesta",
                schema: "sujeto",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_reclamo = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_tipo_resolucion = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    monto_restituido = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    interes_sobre_monto = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    descripcion = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    registrado_por = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reclamo_respuesta", x => x.id);
                    table.ForeignKey(
                        name: "fk_reclamo_respuesta_reclamo_id_reclamo",
                        column: x => x.id_reclamo,
                        principalSchema: "sujeto",
                        principalTable: "reclamo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_reclamo_respuesta_tipo_resolucion_reclamo_codigo_tipo_resol",
                        column: x => x.codigo_tipo_resolucion,
                        principalSchema: "sujeto",
                        principalTable: "tipo_resolucion_reclamo",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_concepto_reclamo_detalle_codigo_concepto",
                schema: "sujeto",
                table: "concepto_reclamo_detalle",
                column: "codigo_concepto");

            migrationBuilder.CreateIndex(
                name: "ix_miembro_organo_gobierno_id_persona",
                schema: "sujeto",
                table: "miembro_organo_gobierno",
                column: "id_persona");

            migrationBuilder.CreateIndex(
                name: "ix_reclamo_codigo_canal_recepcion",
                schema: "sujeto",
                table: "reclamo",
                column: "codigo_canal_recepcion");

            migrationBuilder.CreateIndex(
                name: "ix_reclamo_codigo_concepto_detalle",
                schema: "sujeto",
                table: "reclamo",
                column: "codigo_concepto_detalle");

            migrationBuilder.CreateIndex(
                name: "ix_reclamo_codigo_estado",
                schema: "sujeto",
                table: "reclamo",
                column: "codigo_estado");

            migrationBuilder.CreateIndex(
                name: "ix_reclamo_id_persona",
                schema: "sujeto",
                table: "reclamo",
                column: "id_persona");

            migrationBuilder.CreateIndex(
                name: "ix_reclamo_id_tipo_producto",
                schema: "sujeto",
                table: "reclamo",
                column: "id_tipo_producto");

            migrationBuilder.CreateIndex(
                name: "ix_reclamo_respuesta_codigo_tipo_resolucion",
                schema: "sujeto",
                table: "reclamo_respuesta",
                column: "codigo_tipo_resolucion");

            migrationBuilder.CreateIndex(
                name: "ix_reclamo_respuesta_id_reclamo",
                schema: "sujeto",
                table: "reclamo_respuesta",
                column: "id_reclamo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "miembro_organo_gobierno",
                schema: "sujeto");

            migrationBuilder.DropTable(
                name: "reclamo_respuesta",
                schema: "sujeto");

            migrationBuilder.DropTable(
                name: "reclamo",
                schema: "sujeto");

            migrationBuilder.DropTable(
                name: "tipo_resolucion_reclamo",
                schema: "sujeto");

            migrationBuilder.DropTable(
                name: "canal_reclamo",
                schema: "sujeto");

            migrationBuilder.DropTable(
                name: "concepto_reclamo_detalle",
                schema: "sujeto");

            migrationBuilder.DropTable(
                name: "estado_reclamo",
                schema: "sujeto");

            migrationBuilder.DropTable(
                name: "tipo_producto_reclamo",
                schema: "sujeto");

            migrationBuilder.DropTable(
                name: "concepto_reclamo",
                schema: "sujeto");
        }
    }
}
