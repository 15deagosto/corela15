using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Nivel4_CobranzasYCumplimiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "cobranza");

            migrationBuilder.EnsureSchema(
                name: "lavadoactivos");

            migrationBuilder.CreateTable(
                name: "accion_gestion",
                schema: "cobranza",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    es_llamada = table.Column<bool>(type: "boolean", nullable: false),
                    es_visita = table.Column<bool>(type: "boolean", nullable: false),
                    es_envio_sms = table.Column<bool>(type: "boolean", nullable: false),
                    es_acuerdo_pago = table.Column<bool>(type: "boolean", nullable: false),
                    es_judicial = table.Column<bool>(type: "boolean", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_accion_gestion", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "calificacion_cliente",
                schema: "lavadoactivos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_cliente = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    patrimonio = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    ingreso_mensual = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    perfil_comportamiento = table.Column<decimal>(type: "numeric(9,4)", nullable: false),
                    perfil_transaccional = table.Column<decimal>(type: "numeric(9,4)", nullable: false),
                    total_perfil = table.Column<decimal>(type: "numeric(9,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_calificacion_cliente", x => x.id);
                    table.ForeignKey(
                        name: "fk_calificacion_cliente_clientes_id_cliente",
                        column: x => x.id_cliente,
                        principalSchema: "clientes",
                        principalTable: "cliente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "periodo_mora",
                schema: "cobranza",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    dias_inicio = table.Column<int>(type: "integer", nullable: false),
                    dias_fin = table.Column<int>(type: "integer", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_periodo_mora", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "gestion_prestamo_cobranza",
                schema: "cobranza",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_prestamo = table.Column<Guid>(type: "uuid", nullable: false),
                    id_cliente = table.Column<Guid>(type: "uuid", nullable: false),
                    es_deudor = table.Column<bool>(type: "boolean", nullable: false),
                    id_accion_gestion = table.Column<int>(type: "integer", nullable: false),
                    tiene_compromiso_pago = table.Column<bool>(type: "boolean", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    observacion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    modificado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    modificado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_gestion_prestamo_cobranza", x => x.id);
                    table.ForeignKey(
                        name: "fk_gestion_prestamo_cobranza_accion_gestion_id_accion_gestion",
                        column: x => x.id_accion_gestion,
                        principalSchema: "cobranza",
                        principalTable: "accion_gestion",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_gestion_prestamo_cobranza_cliente_id_cliente",
                        column: x => x.id_cliente,
                        principalSchema: "clientes",
                        principalTable: "cliente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_gestion_prestamo_cobranza_prestamos_id_prestamo",
                        column: x => x.id_prestamo,
                        principalSchema: "colocacion",
                        principalTable: "prestamo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_accion_gestion_codigo",
                schema: "cobranza",
                table: "accion_gestion",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_calificacion_cliente_id_cliente_fecha",
                schema: "lavadoactivos",
                table: "calificacion_cliente",
                columns: new[] { "id_cliente", "fecha" });

            migrationBuilder.CreateIndex(
                name: "ix_gestion_prestamo_cobranza_fecha",
                schema: "cobranza",
                table: "gestion_prestamo_cobranza",
                column: "fecha");

            migrationBuilder.CreateIndex(
                name: "ix_gestion_prestamo_cobranza_id_accion_gestion",
                schema: "cobranza",
                table: "gestion_prestamo_cobranza",
                column: "id_accion_gestion");

            migrationBuilder.CreateIndex(
                name: "ix_gestion_prestamo_cobranza_id_cliente",
                schema: "cobranza",
                table: "gestion_prestamo_cobranza",
                column: "id_cliente");

            migrationBuilder.CreateIndex(
                name: "ix_gestion_prestamo_cobranza_id_prestamo",
                schema: "cobranza",
                table: "gestion_prestamo_cobranza",
                column: "id_prestamo");

            migrationBuilder.CreateIndex(
                name: "ix_periodo_mora_codigo",
                schema: "cobranza",
                table: "periodo_mora",
                column: "codigo",
                unique: true);

            // --- Seed de catálogos ---
            // Tramos verificados contra el patrón real de mora (coincide con
            // Regla #10 de CLAUDE.md: Mora SEPS = (Vencido + NDI) / Total).
            migrationBuilder.Sql(
                """
                INSERT INTO cobranza.periodo_mora (codigo, nombre, dias_inicio, dias_fin, activo) VALUES
                    ('PREV', 'Gestión preventiva', 0,  0,   true),
                    ('GEST', 'Gestión de cobranza', 1,  30,  true),
                    ('CM1',  'Comité de mora I',     31, 60,  true),
                    ('CM2',  'Comité de mora II',    61, 90,  true),
                    ('JUD',  'Judicial',             91, 999999, true);

                INSERT INTO cobranza.accion_gestion (codigo, nombre, es_llamada, es_visita, es_envio_sms, es_acuerdo_pago, es_judicial, activo) VALUES
                    ('LLAM', 'Llamada telefónica',       true,  false, false, false, false, true),
                    ('VISI', 'Visita domiciliaria',      false, true,  false, false, false, true),
                    ('SMS',  'Envío de SMS',             false, false, true,  false, false, true),
                    ('ACPA', 'Acuerdo de pago',          false, false, false, true,  false, true),
                    ('DEMJ', 'Inicio de demanda judicial', false, false, false, false, true, true);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "calificacion_cliente",
                schema: "lavadoactivos");

            migrationBuilder.DropTable(
                name: "gestion_prestamo_cobranza",
                schema: "cobranza");

            migrationBuilder.DropTable(
                name: "periodo_mora",
                schema: "cobranza");

            migrationBuilder.DropTable(
                name: "accion_gestion",
                schema: "cobranza");
        }
    }
}
