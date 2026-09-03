using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ActivoFijo_MotorReal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "serie",
                schema: "activofijo",
                table: "activo",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "modelo",
                schema: "activofijo",
                table: "activo",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "marca",
                schema: "activofijo",
                table: "activo",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "detalle",
                schema: "activofijo",
                table: "activo",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(300)",
                oldMaxLength: 300);

            migrationBuilder.AddColumn<int>(
                name: "anio_matriculacion",
                schema: "activofijo",
                table: "activo",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "anio_vehiculo",
                schema: "activofijo",
                table: "activo",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "asegurado",
                schema: "activofijo",
                table: "activo",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "chasis",
                schema: "activofijo",
                table: "activo",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cilindraje",
                schema: "activofijo",
                table: "activo",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "codigo",
                schema: "activofijo",
                table: "activo",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "color",
                schema: "activofijo",
                table: "activo",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "condicion",
                schema: "activofijo",
                table: "activo",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "creado_en",
                schema: "activofijo",
                table: "activo",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "creado_por",
                schema: "activofijo",
                table: "activo",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "depreciacion_acumulada",
                schema: "activofijo",
                table: "activo",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "es_bien_de_control",
                schema: "activofijo",
                table: "activo",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "es_bien_intangible",
                schema: "activofijo",
                table: "activo",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateOnly>(
                name: "fecha_inicio_calculo",
                schema: "activofijo",
                table: "activo",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<int>(
                name: "id_agencia",
                schema: "activofijo",
                table: "activo",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "id_estructura",
                schema: "activofijo",
                table: "activo",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "modificado_en",
                schema: "activofijo",
                table: "activo",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "modificado_por",
                schema: "activofijo",
                table: "activo",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "motor",
                schema: "activofijo",
                table: "activo",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "placa",
                schema: "activofijo",
                table: "activo",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "activofijo",
                table: "activo",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.CreateTable(
                name: "depreciacion_agencia",
                schema: "activofijo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_agencia = table.Column<int>(type: "integer", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_depreciacion_agencia", x => x.id);
                    table.ForeignKey(
                        name: "fk_depreciacion_agencia_agencia_id_agencia",
                        column: x => x.id_agencia,
                        principalSchema: "general",
                        principalTable: "agencia",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "estructura",
                schema: "activofijo",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    se_deprecia = table.Column<bool>(type: "boolean", nullable: false),
                    porcentaje_depreciacion_anual = table.Column<decimal>(type: "numeric(9,4)", nullable: false),
                    es_bien_intangible = table.Column<bool>(type: "boolean", nullable: false),
                    id_cuenta_contable_activo = table.Column<Guid>(type: "uuid", nullable: false),
                    id_cuenta_contable_deprecia = table.Column<Guid>(type: "uuid", nullable: true),
                    id_cuenta_contable_gasto = table.Column<Guid>(type: "uuid", nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_estructura", x => x.id);
                    table.ForeignKey(
                        name: "fk_estructura_cuenta_contable_id_cuenta_contable_activo",
                        column: x => x.id_cuenta_contable_activo,
                        principalSchema: "contabilidad",
                        principalTable: "cuenta_contable",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_estructura_cuenta_contable_id_cuenta_contable_deprecia",
                        column: x => x.id_cuenta_contable_deprecia,
                        principalSchema: "contabilidad",
                        principalTable: "cuenta_contable",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_estructura_cuenta_contable_id_cuenta_contable_gasto",
                        column: x => x.id_cuenta_contable_gasto,
                        principalSchema: "contabilidad",
                        principalTable: "cuenta_contable",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "motivo_baja",
                schema: "activofijo",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    detalle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    es_donacion = table.Column<bool>(type: "boolean", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_motivo_baja", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "motivo_traslado_activo",
                schema: "activofijo",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    es_definitivo = table.Column<bool>(type: "boolean", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_motivo_traslado_activo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "responsable",
                schema: "activofijo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_persona = table.Column<Guid>(type: "uuid", nullable: false),
                    id_agencia = table.Column<int>(type: "integer", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_responsable", x => x.id);
                    table.ForeignKey(
                        name: "fk_responsable_agencia_id_agencia",
                        column: x => x.id_agencia,
                        principalSchema: "general",
                        principalTable: "agencia",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_responsable_persona_id_persona",
                        column: x => x.id_persona,
                        principalSchema: "sujeto",
                        principalTable: "persona",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "depreciacion_agencia_detalle",
                schema: "activofijo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_depreciacion_agencia = table.Column<Guid>(type: "uuid", nullable: false),
                    id_activo = table.Column<Guid>(type: "uuid", nullable: false),
                    depreciacion_periodo = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    depreciacion_acumulada = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    saldo_libros = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    depreciado_total = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_depreciacion_agencia_detalle", x => x.id);
                    table.ForeignKey(
                        name: "fk_depreciacion_agencia_detalle_activo_id_activo",
                        column: x => x.id_activo,
                        principalSchema: "activofijo",
                        principalTable: "activo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_depreciacion_agencia_detalle_depreciacion_agencia_id_deprec",
                        column: x => x.id_depreciacion_agencia,
                        principalSchema: "activofijo",
                        principalTable: "depreciacion_agencia",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "solicitud_activo_baja",
                schema: "activofijo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_activo = table.Column<Guid>(type: "uuid", nullable: false),
                    id_motivo_baja = table.Column<int>(type: "integer", nullable: false),
                    detalle = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    fecha_solicitud = table.Column<DateOnly>(type: "date", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    id_comprobante = table.Column<Guid>(type: "uuid", nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    modificado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    modificado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_solicitud_activo_baja", x => x.id);
                    table.ForeignKey(
                        name: "fk_solicitud_activo_baja_activo_id_activo",
                        column: x => x.id_activo,
                        principalSchema: "activofijo",
                        principalTable: "activo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_solicitud_activo_baja_motivo_baja_id_motivo_baja",
                        column: x => x.id_motivo_baja,
                        principalSchema: "activofijo",
                        principalTable: "motivo_baja",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "activo_responsable",
                schema: "activofijo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_activo = table.Column<Guid>(type: "uuid", nullable: false),
                    id_responsable = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha_asignacion = table.Column<DateOnly>(type: "date", nullable: false),
                    activa = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_activo_responsable", x => x.id);
                    table.ForeignKey(
                        name: "fk_activo_responsable_activo_id_activo",
                        column: x => x.id_activo,
                        principalSchema: "activofijo",
                        principalTable: "activo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_activo_responsable_responsables_activo_fijo_id_responsable",
                        column: x => x.id_responsable,
                        principalSchema: "activofijo",
                        principalTable: "responsable",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "traslado_activo",
                schema: "activofijo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_activo = table.Column<Guid>(type: "uuid", nullable: false),
                    concepto = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    id_motivo_traslado = table.Column<int>(type: "integer", nullable: false),
                    razon = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    id_responsable_origen = table.Column<Guid>(type: "uuid", nullable: true),
                    id_agencia_origen = table.Column<int>(type: "integer", nullable: false),
                    id_responsable_destino = table.Column<Guid>(type: "uuid", nullable: true),
                    id_agencia_destino = table.Column<int>(type: "integer", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    modificado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    modificado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_traslado_activo", x => x.id);
                    table.ForeignKey(
                        name: "fk_traslado_activo_activo_id_activo",
                        column: x => x.id_activo,
                        principalSchema: "activofijo",
                        principalTable: "activo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_traslado_activo_agencia_id_agencia_destino",
                        column: x => x.id_agencia_destino,
                        principalSchema: "general",
                        principalTable: "agencia",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_traslado_activo_agencia_id_agencia_origen",
                        column: x => x.id_agencia_origen,
                        principalSchema: "general",
                        principalTable: "agencia",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_traslado_activo_motivo_traslado_activo_id_motivo_traslado",
                        column: x => x.id_motivo_traslado,
                        principalSchema: "activofijo",
                        principalTable: "motivo_traslado_activo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_traslado_activo_responsable_id_responsable_destino",
                        column: x => x.id_responsable_destino,
                        principalSchema: "activofijo",
                        principalTable: "responsable",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_traslado_activo_responsable_id_responsable_origen",
                        column: x => x.id_responsable_origen,
                        principalSchema: "activofijo",
                        principalTable: "responsable",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_activo_id_agencia",
                schema: "activofijo",
                table: "activo",
                column: "id_agencia");

            migrationBuilder.CreateIndex(
                name: "ix_activo_id_estructura",
                schema: "activofijo",
                table: "activo",
                column: "id_estructura");

            migrationBuilder.CreateIndex(
                name: "ix_activo_responsable_id_activo",
                schema: "activofijo",
                table: "activo_responsable",
                column: "id_activo");

            migrationBuilder.CreateIndex(
                name: "ix_activo_responsable_id_responsable",
                schema: "activofijo",
                table: "activo_responsable",
                column: "id_responsable");

            migrationBuilder.CreateIndex(
                name: "ix_depreciacion_agencia_id_agencia_fecha",
                schema: "activofijo",
                table: "depreciacion_agencia",
                columns: new[] { "id_agencia", "fecha" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_depreciacion_agencia_detalle_id_activo",
                schema: "activofijo",
                table: "depreciacion_agencia_detalle",
                column: "id_activo");

            migrationBuilder.CreateIndex(
                name: "ix_depreciacion_agencia_detalle_id_depreciacion_agencia",
                schema: "activofijo",
                table: "depreciacion_agencia_detalle",
                column: "id_depreciacion_agencia");

            migrationBuilder.CreateIndex(
                name: "ix_estructura_id_cuenta_contable_activo",
                schema: "activofijo",
                table: "estructura",
                column: "id_cuenta_contable_activo");

            migrationBuilder.CreateIndex(
                name: "ix_estructura_id_cuenta_contable_deprecia",
                schema: "activofijo",
                table: "estructura",
                column: "id_cuenta_contable_deprecia");

            migrationBuilder.CreateIndex(
                name: "ix_estructura_id_cuenta_contable_gasto",
                schema: "activofijo",
                table: "estructura",
                column: "id_cuenta_contable_gasto");

            migrationBuilder.CreateIndex(
                name: "ix_responsable_id_agencia",
                schema: "activofijo",
                table: "responsable",
                column: "id_agencia");

            migrationBuilder.CreateIndex(
                name: "ix_responsable_id_persona",
                schema: "activofijo",
                table: "responsable",
                column: "id_persona");

            migrationBuilder.CreateIndex(
                name: "ix_solicitud_activo_baja_id_activo",
                schema: "activofijo",
                table: "solicitud_activo_baja",
                column: "id_activo");

            migrationBuilder.CreateIndex(
                name: "ix_solicitud_activo_baja_id_motivo_baja",
                schema: "activofijo",
                table: "solicitud_activo_baja",
                column: "id_motivo_baja");

            migrationBuilder.CreateIndex(
                name: "ix_traslado_activo_id_activo",
                schema: "activofijo",
                table: "traslado_activo",
                column: "id_activo");

            migrationBuilder.CreateIndex(
                name: "ix_traslado_activo_id_agencia_destino",
                schema: "activofijo",
                table: "traslado_activo",
                column: "id_agencia_destino");

            migrationBuilder.CreateIndex(
                name: "ix_traslado_activo_id_agencia_origen",
                schema: "activofijo",
                table: "traslado_activo",
                column: "id_agencia_origen");

            migrationBuilder.CreateIndex(
                name: "ix_traslado_activo_id_motivo_traslado",
                schema: "activofijo",
                table: "traslado_activo",
                column: "id_motivo_traslado");

            migrationBuilder.CreateIndex(
                name: "ix_traslado_activo_id_responsable_destino",
                schema: "activofijo",
                table: "traslado_activo",
                column: "id_responsable_destino");

            migrationBuilder.CreateIndex(
                name: "ix_traslado_activo_id_responsable_origen",
                schema: "activofijo",
                table: "traslado_activo",
                column: "id_responsable_origen");

            migrationBuilder.AddForeignKey(
                name: "fk_activo_agencias_id_agencia",
                schema: "activofijo",
                table: "activo",
                column: "id_agencia",
                principalSchema: "general",
                principalTable: "agencia",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_activo_estructuras_activo_fijo_id_estructura",
                schema: "activofijo",
                table: "activo",
                column: "id_estructura",
                principalSchema: "activofijo",
                principalTable: "estructura",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            // Catálogo real de 72 categorías de activo fijo (verificado
            // contra ACTIVOFIJO.ESTRUCTURA nivel 3, 73 filas activas reales
            // — deduplicado a 72 al encontrar "AIRE ACONDICIONADO" repetido
            // dos veces con datos idénticos, mismo criterio de anomalías
            // reales ya documentado con el código "671"/"111" en rondas
            // anteriores). Los códigos de cuenta contable reales de
            // Softbank (8-10 dígitos, sub-mayor interno) no existen en el
            // Catálogo Único de Cuentas oficial ya sembrado — cada
            // categoría se mapeó por semántica a la cuenta oficial de 4-6
            // dígitos más cercana (nunca inventada), ver Estructura.cs.
            migrationBuilder.Sql("""
                WITH cuentas AS (
                    SELECT codigo, id FROM contabilidad.cuenta_contable
                )
                INSERT INTO activofijo.estructura
                    (nombre, se_deprecia, porcentaje_depreciacion_anual, es_bien_intangible, id_cuenta_contable_activo, id_cuenta_contable_deprecia, id_cuenta_contable_gasto, activo)
                SELECT v.nombre, v.se_deprecia, v.pct, v.intangible,
                    (SELECT id FROM cuentas WHERE codigo = v.cta_activo),
                    (SELECT id FROM cuentas WHERE codigo = v.cta_deprecia),
                    (SELECT id FROM cuentas WHERE codigo = v.cta_gasto),
                    true
                FROM (VALUES
                    ('Terreno',                       false, 0.00,  false, '1801',   NULL,     NULL),
                    ('Edificio',                       true,  5.00,  false, '1802',   '189905', '450515'),
                    ('Cerraduras',                      true, 10.00, false, '1805',   '189915', '450525'),
                    ('Aire Acondicionado',               true, 10.00, false, '1805',   '189915', '450525'),
                    ('Alcoholímetro',                    true, 10.00, false, '1805',   '189915', '450525'),
                    ('Amplificador de Sonido',           true, 10.00, false, '1805',   '189915', '450525'),
                    ('Amplificador',                     true, 10.00, false, '1805',   '189915', '450525'),
                    ('Anaquel',                          true, 10.00, false, '1805',   '189915', '450525'),
                    ('Archivador',                       true, 10.00, false, '1805',   '189915', '450525'),
                    ('Arrendamientos',                   true, 10.00, true,  '190490', '190499', '450330'),
                    ('Reloj Biométrico',                 true, 10.00, false, '1805',   '189915', '450525'),
                    ('Central de Alarmas',               true, 10.00, false, '1805',   '189915', '450525'),
                    ('Calefactores',                     true, 10.00, false, '1805',   '189915', '450525'),
                    ('Calificadora Servicios',           true, 33.33, false, '1806',   '189920', '450530'),
                    ('Cámaras de Fotos',                 true, 10.00, false, '1805',   '189915', '450525'),
                    ('Celular',                          true, 10.00, false, '1805',   '189915', '450525'),
                    ('Central de Computación',           true, 33.33, false, '1806',   '189920', '450530'),
                    ('Cajas Fuertes',                    true, 10.00, false, '1805',   '189915', '450525'),
                    ('Chaleco Antibala',                 true, 10.00, false, '1805',   '189915', '450525'),
                    ('Contadora de Monedas',             true, 10.00, false, '1805',   '189915', '450525'),
                    ('Computador',                       true, 33.33, false, '1806',   '189920', '450530'),
                    ('Copiadoras',                       true, 33.33, false, '1806',   '189920', '450530'),
                    ('CPU',                              true, 33.33, false, '1806',   '189920', '450530'),
                    ('Cubículos',                        true, 10.00, false, '1805',   '189915', '450525'),
                    ('Contador de Billetes',             true, 10.00, false, '1805',   '189915', '450525'),
                    ('Radios Portátil',                  true, 33.33, false, '1806',   '189920', '450530'),
                    ('Disco Duro',                       true, 33.33, false, '1806',   '189920', '450530'),
                    ('Equipos Electrónicos (seguro)',    true, 10.00, true,  '190490', '190499', '450325'),
                    ('Escritorios',                      true, 10.00, false, '1805',   '189915', '450525'),
                    ('Estanterías Perchas',              true, 10.00, false, '1805',   '189915', '450525'),
                    ('Gastos de Adecuación',             true, 10.00, true,  '190525', '190599', '450630'),
                    ('Gavinete Pack',                    true, 10.00, false, '1805',   '189915', '450525'),
                    ('Gastos de Constitución y Organización', true, 10.00, true, '190505', '190599', '450610'),
                    ('Generador Eléctrico',              true, 10.00, false, '1805',   '189915', '450525'),
                    ('Grabador',                         true, 10.00, false, '1805',   '189915', '450525'),
                    ('Impresora',                        true, 33.33, false, '1806',   '189920', '450530'),
                    ('Incendio (seguro)',                true, 10.00, true,  '190490', '190499', '450325'),
                    ('Intercomunicador de Ventanilla',   true, 33.33, false, '1806',   '189920', '450530'),
                    ('Laptop',                           true, 33.33, false, '1806',   '189920', '450530'),
                    ('UPS',                              true, 33.33, false, '1806',   '189920', '450530'),
                    ('Mamparas',                         true, 10.00, false, '1805',   '189915', '450525'),
                    ('Monitor',                          true, 33.33, false, '1806',   '189920', '450530'),
                    ('DVR',                              true, 33.33, false, '1806',   '189920', '450530'),
                    ('Motos',                            true, 20.00, false, '1807',   '189925', '450535'),
                    ('Mesa',                             true, 10.00, false, '1805',   '189915', '450525'),
                    ('NAS Cloud',                        true, 33.33, false, '1806',   '189920', '450530'),
                    ('Fraude Informático (seguro)',      true, 10.00, true,  '190490', '190499', '450325'),
                    ('Puertas',                          true, 10.00, false, '1805',   '189915', '450525'),
                    ('Pedestales',                       true, 10.00, false, '1805',   '189915', '450525'),
                    ('Pizarras',                         true, 10.00, false, '1805',   '189915', '450525'),
                    ('Equipo Panel Monitor',             true, 33.33, false, '1806',   '189920', '450530'),
                    ('Proyector',                        true, 33.33, false, '1806',   '189920', '450530'),
                    ('Purificador de Agua',              true, 10.00, false, '1805',   '189915', '450525'),
                    ('Publicidad',                       true, 10.00, true,  '190490', '190499', '450315'),
                    ('Biblioteca de Metal',              true, 10.00, false, '1805',   '189915', '450525'),
                    ('Gavinete para Servidor',           true, 33.33, false, '1806',   '189920', '450530'),
                    ('Refrigeradora',                    true, 10.00, false, '1805',   '189915', '450525'),
                    ('Robo y Asalto (seguro)',           true, 10.00, true,  '190490', '190499', '450325'),
                    ('Scanner',                          true, 33.33, false, '1806',   '189920', '450530'),
                    ('Housing Metálico',                 true, 33.33, false, '1806',   '189920', '450530'),
                    ('Servidor',                         true, 33.33, false, '1806',   '189920', '450530'),
                    ('Sillas',                           true, 10.00, false, '1805',   '189915', '450525'),
                    ('Trituradora de Papel',             true, 10.00, false, '1805',   '189915', '450525'),
                    ('Swich 3COM',                       true, 33.33, false, '1806',   '189920', '450530'),
                    ('Tablero Transferencia',            true, 10.00, false, '1805',   '189915', '450525'),
                    ('Teléfono',                         true, 10.00, false, '1805',   '189915', '450525'),
                    ('Transformador',                    true, 10.00, false, '1805',   '189915', '450525'),
                    ('Turnero',                          true, 33.33, false, '1806',   '189920', '450530'),
                    ('Televisor',                        true, 10.00, false, '1805',   '189915', '450525'),
                    ('Vehículos (seguro)',               true, 10.00, true,  '190490', '190499', '450325'),
                    ('Vehículos',                        true, 20.00, false, '1807',   '189925', '450535'),
                    ('Vitrina',                          true, 10.00, false, '1805',   '189915', '450525')
                ) AS v(nombre, se_deprecia, pct, intangible, cta_activo, cta_deprecia, cta_gasto);
                """);

            // ACTIVOFIJO.MOTIVO_TRASLADO_ACTIVO real (4 filas).
            migrationBuilder.Sql("""
                INSERT INTO activofijo.motivo_traslado_activo (id, nombre, es_definitivo, activo) VALUES
                (1, 'Traslado definitivo', true, true),
                (2, 'Traslado temporal', false, true),
                (3, 'Traspaso por mantenimiento', false, true),
                (4, 'Traspaso por devolución de mantenimiento', false, true);
                """);

            // ACTIVOFIJO.MOTIVO_BAJA real (12 filas).
            migrationBuilder.Sql("""
                INSERT INTO activofijo.motivo_baja (id, detalle, es_donacion, activo) VALUES
                (1,  'Deterioro',              false, true),
                (2,  'Falla eléctrica',        false, true),
                (3,  'Falla mecánica',         false, true),
                (4,  'En mal estado',          false, true),
                (5,  'Averiado',               false, true),
                (6,  'Robo',                   false, false),
                (7,  'Pérdida',                false, false),
                (8,  'Desastre natural',       false, true),
                (9,  'Donación',               true,  true),
                (10, 'Pérdida, robo o hurto',  false, true),
                (11, 'Remate',                 false, true),
                (12, 'Venta directa',          false, true);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_activo_agencias_id_agencia",
                schema: "activofijo",
                table: "activo");

            migrationBuilder.DropForeignKey(
                name: "fk_activo_estructuras_activo_fijo_id_estructura",
                schema: "activofijo",
                table: "activo");

            migrationBuilder.DropTable(
                name: "activo_responsable",
                schema: "activofijo");

            migrationBuilder.DropTable(
                name: "depreciacion_agencia_detalle",
                schema: "activofijo");

            migrationBuilder.DropTable(
                name: "estructura",
                schema: "activofijo");

            migrationBuilder.DropTable(
                name: "solicitud_activo_baja",
                schema: "activofijo");

            migrationBuilder.DropTable(
                name: "traslado_activo",
                schema: "activofijo");

            migrationBuilder.DropTable(
                name: "depreciacion_agencia",
                schema: "activofijo");

            migrationBuilder.DropTable(
                name: "motivo_baja",
                schema: "activofijo");

            migrationBuilder.DropTable(
                name: "motivo_traslado_activo",
                schema: "activofijo");

            migrationBuilder.DropTable(
                name: "responsable",
                schema: "activofijo");

            migrationBuilder.DropIndex(
                name: "ix_activo_id_agencia",
                schema: "activofijo",
                table: "activo");

            migrationBuilder.DropIndex(
                name: "ix_activo_id_estructura",
                schema: "activofijo",
                table: "activo");

            migrationBuilder.DropColumn(
                name: "anio_matriculacion",
                schema: "activofijo",
                table: "activo");

            migrationBuilder.DropColumn(
                name: "anio_vehiculo",
                schema: "activofijo",
                table: "activo");

            migrationBuilder.DropColumn(
                name: "asegurado",
                schema: "activofijo",
                table: "activo");

            migrationBuilder.DropColumn(
                name: "chasis",
                schema: "activofijo",
                table: "activo");

            migrationBuilder.DropColumn(
                name: "cilindraje",
                schema: "activofijo",
                table: "activo");

            migrationBuilder.DropColumn(
                name: "codigo",
                schema: "activofijo",
                table: "activo");

            migrationBuilder.DropColumn(
                name: "color",
                schema: "activofijo",
                table: "activo");

            migrationBuilder.DropColumn(
                name: "condicion",
                schema: "activofijo",
                table: "activo");

            migrationBuilder.DropColumn(
                name: "creado_en",
                schema: "activofijo",
                table: "activo");

            migrationBuilder.DropColumn(
                name: "creado_por",
                schema: "activofijo",
                table: "activo");

            migrationBuilder.DropColumn(
                name: "depreciacion_acumulada",
                schema: "activofijo",
                table: "activo");

            migrationBuilder.DropColumn(
                name: "es_bien_de_control",
                schema: "activofijo",
                table: "activo");

            migrationBuilder.DropColumn(
                name: "es_bien_intangible",
                schema: "activofijo",
                table: "activo");

            migrationBuilder.DropColumn(
                name: "fecha_inicio_calculo",
                schema: "activofijo",
                table: "activo");

            migrationBuilder.DropColumn(
                name: "id_agencia",
                schema: "activofijo",
                table: "activo");

            migrationBuilder.DropColumn(
                name: "id_estructura",
                schema: "activofijo",
                table: "activo");

            migrationBuilder.DropColumn(
                name: "modificado_en",
                schema: "activofijo",
                table: "activo");

            migrationBuilder.DropColumn(
                name: "modificado_por",
                schema: "activofijo",
                table: "activo");

            migrationBuilder.DropColumn(
                name: "motor",
                schema: "activofijo",
                table: "activo");

            migrationBuilder.DropColumn(
                name: "placa",
                schema: "activofijo",
                table: "activo");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "activofijo",
                table: "activo");

            migrationBuilder.AlterColumn<string>(
                name: "serie",
                schema: "activofijo",
                table: "activo",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "modelo",
                schema: "activofijo",
                table: "activo",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "marca",
                schema: "activofijo",
                table: "activo",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "detalle",
                schema: "activofijo",
                table: "activo",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);
        }
    }
}
