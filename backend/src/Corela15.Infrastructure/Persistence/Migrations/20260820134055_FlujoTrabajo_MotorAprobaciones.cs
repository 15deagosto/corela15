using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FlujoTrabajo_MotorAprobaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "flujotrabajo");

            migrationBuilder.AddColumn<int>(
                name: "id_etapa_actual",
                schema: "credito",
                table: "solicitud_prestamo",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "grupo_contable",
                schema: "flujotrabajo",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    monto_minimo = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    monto_maximo = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_grupo_contable", x => x.codigo);
                });

            migrationBuilder.CreateTable(
                name: "tipo_etapa",
                schema: "flujotrabajo",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    es_solicitud = table.Column<bool>(type: "boolean", nullable: false),
                    es_comprobante = table.Column<bool>(type: "boolean", nullable: false),
                    es_solicitud_administrativa = table.Column<bool>(type: "boolean", nullable: false),
                    activa = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipo_etapa", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grupo_contable_usuario",
                schema: "flujotrabajo",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo_grupo_contable = table.Column<string>(type: "character varying(10)", nullable: false),
                    id_usuario = table.Column<Guid>(type: "uuid", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_grupo_contable_usuario", x => x.id);
                    table.ForeignKey(
                        name: "fk_grupo_contable_usuario_grupo_contable_codigo_grupo_contable",
                        column: x => x.codigo_grupo_contable,
                        principalSchema: "flujotrabajo",
                        principalTable: "grupo_contable",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_grupo_contable_usuario_usuarios_id_usuario",
                        column: x => x.id_usuario,
                        principalSchema: "seguridad",
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "etapa",
                schema: "flujotrabajo",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    id_tipo_etapa = table.Column<int>(type: "integer", nullable: false),
                    orden = table.Column<int>(type: "integer", nullable: false),
                    aprueba_al_menos_uno = table.Column<bool>(type: "boolean", nullable: false),
                    tiempo_maximo_dia = table.Column<int>(type: "integer", nullable: false),
                    activa = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_etapa", x => x.id);
                    table.ForeignKey(
                        name: "fk_etapa_tipos_etapa_id_tipo_etapa",
                        column: x => x.id_tipo_etapa,
                        principalSchema: "flujotrabajo",
                        principalTable: "tipo_etapa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "etapa_grupo_contable",
                schema: "flujotrabajo",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_etapa = table.Column<int>(type: "integer", nullable: false),
                    id_agencia = table.Column<int>(type: "integer", nullable: false),
                    codigo_grupo_contable = table.Column<string>(type: "character varying(10)", nullable: false),
                    orden = table.Column<int>(type: "integer", nullable: false),
                    activa = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_etapa_grupo_contable", x => x.id);
                    table.ForeignKey(
                        name: "fk_etapa_grupo_contable_agencia_id_agencia",
                        column: x => x.id_agencia,
                        principalSchema: "general",
                        principalTable: "agencia",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_etapa_grupo_contable_etapa_id_etapa",
                        column: x => x.id_etapa,
                        principalSchema: "flujotrabajo",
                        principalTable: "etapa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_etapa_grupo_contable_grupo_contable_codigo_grupo_contable",
                        column: x => x.codigo_grupo_contable,
                        principalSchema: "flujotrabajo",
                        principalTable: "grupo_contable",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "etapa_retorno",
                schema: "flujotrabajo",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_etapa = table.Column<int>(type: "integer", nullable: false),
                    id_etapa_retorno = table.Column<int>(type: "integer", nullable: false),
                    activa = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_etapa_retorno", x => x.id);
                    table.ForeignKey(
                        name: "fk_etapa_retorno_etapa_id_etapa",
                        column: x => x.id_etapa,
                        principalSchema: "flujotrabajo",
                        principalTable: "etapa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_etapa_retorno_etapa_id_etapa_retorno",
                        column: x => x.id_etapa_retorno,
                        principalSchema: "flujotrabajo",
                        principalTable: "etapa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "solicitud_prestamo_etapa_hist",
                schema: "credito",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_solicitud_prestamo = table.Column<Guid>(type: "uuid", nullable: false),
                    id_etapa_anterior = table.Column<int>(type: "integer", nullable: true),
                    id_etapa = table.Column<int>(type: "integer", nullable: false),
                    comentario = table.Column<string>(type: "text", nullable: true),
                    registrado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    fecha = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    es_retorno = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_solicitud_prestamo_etapa_hist", x => x.id);
                    table.ForeignKey(
                        name: "fk_solicitud_prestamo_etapa_hist_etapa_id_etapa",
                        column: x => x.id_etapa,
                        principalSchema: "flujotrabajo",
                        principalTable: "etapa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_solicitud_prestamo_etapa_hist_solicitud_prestamo_id_solicit",
                        column: x => x.id_solicitud_prestamo,
                        principalSchema: "credito",
                        principalTable: "solicitud_prestamo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Datos reales verificados contra FLUJOTRABAJO.TIPO_ETAPA/ETAPA/
            // ETAPA_RETORNO de Softbank (decompilado de SBK_Models.dll y
            // cruzado columna por columna contra la base real, mismo método
            // que el resto del proyecto). IDs explícitos = los mismos IDs
            // reales de Softbank para el TipoEtapa "SOLICITUD CRÉDITO" (id=1)
            // — se excluye TipoEtapa=2 "COMPROBANTE CONTABLE" y sus 3 etapas
            // (Softbank ids 5,6,7,29), documentado a propósito: el motor de
            // comprobantes de este core (ComprobanteContableService) ya tiene
            // su propio flujo directo sin aprobación por etapas, integrarlo
            // sería un cambio de diseño mayor fuera de esta ronda.
            migrationBuilder.Sql(@"
INSERT INTO flujotrabajo.tipo_etapa (id, nombre, es_solicitud, es_comprobante, es_solicitud_administrativa, activa) VALUES
(1, 'SOLICITUD CRÉDITO', true, false, false, true);
SELECT setval(pg_get_serial_sequence('flujotrabajo.tipo_etapa', 'id'), 1);

INSERT INTO flujotrabajo.etapa (id, nombre, id_tipo_etapa, orden, aprueba_al_menos_uno, tiempo_maximo_dia, activa) VALUES
(1, 'INGRESO', 1, 1, true, 1, true),
(2, 'VERIFICACION', 1, 4, true, 1, true),
(3, 'APROBACION', 1, 7, true, 1, true),
(4, 'DESEMBOLSO', 1, 8, true, 1, true),
(8, 'COMITE DE CREDITO', 1, 5, true, 1, true),
(26, 'NEGADA', 1, 1, true, 1, true),
(27, 'LIQUIDADA', 1, 9, true, 1, true),
(28, 'VERIFICACION COMUNAL', 1, 2, false, 1, false),
(30, 'DIGITADA', 1, 3, true, 1, true),
(31, 'RIESGOS', 1, 2, true, 2, true),
(32, 'JURIDICO', 1, 6, true, 2, true);
SELECT setval(pg_get_serial_sequence('flujotrabajo.etapa', 'id'), 32);

INSERT INTO flujotrabajo.etapa_retorno (id_etapa, id_etapa_retorno, activa) VALUES
(28, 1, true),
(31, 1, true),
(4, 1, true),
(4, 2, true),
(4, 26, true),
(4, 8, true),
(30, 1, true),
(30, 26, true),
(3, 8, true),
(3, 2, true),
(2, 1, true),
(8, 1, true);

-- Grupos de aprobadores reales por etapa — Softbank los tiene por agencia
-- física (Pilacoto/San Silvestre/El Salto), Corela15 hoy solo tiene una
-- agencia (""Matriz""), así que se colapsan a un grupo por rol de decisión
-- en vez de triplicar nombres de sucursales que no existen acá — mismo
-- criterio de adaptación real usado con TasaTechoBce/GrupoContable en
-- otras secciones (nunca inventar una sucursal falsa).
INSERT INTO flujotrabajo.grupo_contable (codigo, nombre, monto_minimo, monto_maximo, activo) VALUES
('ING-CRED', 'Ingreso y verificación de crédito', 0.01, 999999999.00, true),
('COM-CRED', 'Comité de crédito', 0.01, 999999999.00, true),
('DES-CRED', 'Desembolso de crédito', 0.01, 999999999.00, true);

INSERT INTO flujotrabajo.etapa_grupo_contable (id_etapa, id_agencia, codigo_grupo_contable, orden, activa) VALUES
(1, 1, 'ING-CRED', 0, true),
(2, 1, 'ING-CRED', 0, true),
(28, 1, 'ING-CRED', 0, true),
(30, 1, 'ING-CRED', 0, true),
(31, 1, 'ING-CRED', 0, true),
(32, 1, 'ING-CRED', 0, true),
(3, 1, 'COM-CRED', 0, true),
(8, 1, 'COM-CRED', 0, true),
(4, 1, 'DES-CRED', 0, true);

-- El usuario 'admin' de ejemplo (Nivel0_SeedDatosPrueba) queda como
-- integrante de los 3 grupos para que el flujo de aprobación siga
-- funcionando end-to-end en desarrollo sin configuración manual adicional
-- — en un ambiente real, el administrador ajusta esto desde Configuración.
INSERT INTO flujotrabajo.grupo_contable_usuario (codigo_grupo_contable, id_usuario, activo)
SELECT g.codigo, u.id, true
FROM flujotrabajo.grupo_contable g
CROSS JOIN seguridad.usuario u
WHERE u.nombre_usuario = 'admin';
");

            migrationBuilder.CreateIndex(
                name: "ix_solicitud_prestamo_id_etapa_actual",
                schema: "credito",
                table: "solicitud_prestamo",
                column: "id_etapa_actual");

            migrationBuilder.CreateIndex(
                name: "ix_etapa_id_tipo_etapa",
                schema: "flujotrabajo",
                table: "etapa",
                column: "id_tipo_etapa");

            migrationBuilder.CreateIndex(
                name: "ix_etapa_grupo_contable_codigo_grupo_contable",
                schema: "flujotrabajo",
                table: "etapa_grupo_contable",
                column: "codigo_grupo_contable");

            migrationBuilder.CreateIndex(
                name: "ix_etapa_grupo_contable_id_agencia",
                schema: "flujotrabajo",
                table: "etapa_grupo_contable",
                column: "id_agencia");

            migrationBuilder.CreateIndex(
                name: "ix_etapa_grupo_contable_id_etapa_id_agencia",
                schema: "flujotrabajo",
                table: "etapa_grupo_contable",
                columns: new[] { "id_etapa", "id_agencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_etapa_retorno_id_etapa",
                schema: "flujotrabajo",
                table: "etapa_retorno",
                column: "id_etapa");

            migrationBuilder.CreateIndex(
                name: "ix_etapa_retorno_id_etapa_retorno",
                schema: "flujotrabajo",
                table: "etapa_retorno",
                column: "id_etapa_retorno");

            migrationBuilder.CreateIndex(
                name: "ix_grupo_contable_usuario_codigo_grupo_contable",
                schema: "flujotrabajo",
                table: "grupo_contable_usuario",
                column: "codigo_grupo_contable");

            migrationBuilder.CreateIndex(
                name: "ix_grupo_contable_usuario_id_usuario",
                schema: "flujotrabajo",
                table: "grupo_contable_usuario",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "ix_solicitud_prestamo_etapa_hist_id_etapa",
                schema: "credito",
                table: "solicitud_prestamo_etapa_hist",
                column: "id_etapa");

            migrationBuilder.CreateIndex(
                name: "ix_solicitud_prestamo_etapa_hist_id_solicitud_prestamo",
                schema: "credito",
                table: "solicitud_prestamo_etapa_hist",
                column: "id_solicitud_prestamo");

            migrationBuilder.AddForeignKey(
                name: "fk_solicitud_prestamo_etapa_id_etapa_actual",
                schema: "credito",
                table: "solicitud_prestamo",
                column: "id_etapa_actual",
                principalSchema: "flujotrabajo",
                principalTable: "etapa",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_solicitud_prestamo_etapa_id_etapa_actual",
                schema: "credito",
                table: "solicitud_prestamo");

            migrationBuilder.DropTable(
                name: "etapa_grupo_contable",
                schema: "flujotrabajo");

            migrationBuilder.DropTable(
                name: "etapa_retorno",
                schema: "flujotrabajo");

            migrationBuilder.DropTable(
                name: "grupo_contable_usuario",
                schema: "flujotrabajo");

            migrationBuilder.DropTable(
                name: "solicitud_prestamo_etapa_hist",
                schema: "credito");

            migrationBuilder.DropTable(
                name: "grupo_contable",
                schema: "flujotrabajo");

            migrationBuilder.DropTable(
                name: "etapa",
                schema: "flujotrabajo");

            migrationBuilder.DropTable(
                name: "tipo_etapa",
                schema: "flujotrabajo");

            migrationBuilder.DropIndex(
                name: "ix_solicitud_prestamo_id_etapa_actual",
                schema: "credito",
                table: "solicitud_prestamo");

            migrationBuilder.DropColumn(
                name: "id_etapa_actual",
                schema: "credito",
                table: "solicitud_prestamo");
        }
    }
}
