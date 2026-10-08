using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Planificacion_KpisAprobacionAreasReales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "aprobado_por",
                schema: "planificacion",
                table: "plan_semanal",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            // defaultValue real explícito -- EF Core no propaga el valor
            // C# del enum (Pendiente) al generar la migración, mismo gap
            // ya documentado para Rol.DiasCambioClave/Usuario.CambiaClave.
            // Sin esto, los planes ya existentes quedarían con
            // estado_aprobacion='' en vez de 'Pendiente'.
            migrationBuilder.AddColumn<string>(
                name: "estado_aprobacion",
                schema: "planificacion",
                table: "plan_semanal",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pendiente");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "fecha_aprobacion",
                schema: "planificacion",
                table: "plan_semanal",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "codigo_area",
                schema: "planificacion",
                table: "indicador",
                type: "character varying(20)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "unidad",
                schema: "planificacion",
                table: "indicador",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "plan_semanal_indicador",
                schema: "planificacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_plan_semanal = table.Column<Guid>(type: "uuid", nullable: false),
                    id_indicador = table.Column<int>(type: "integer", nullable: false),
                    meta = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    real = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    comentario = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_plan_semanal_indicador", x => x.id);
                    table.ForeignKey(
                        name: "fk_plan_semanal_indicador_indicador_id_indicador",
                        column: x => x.id_indicador,
                        principalSchema: "planificacion",
                        principalTable: "indicador",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_plan_semanal_indicador_plan_semanal_id_plan_semanal",
                        column: x => x.id_plan_semanal,
                        principalSchema: "planificacion",
                        principalTable: "plan_semanal",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_indicador_codigo_area_nombre",
                schema: "planificacion",
                table: "indicador",
                columns: new[] { "codigo_area", "nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_plan_semanal_indicador_id_indicador",
                schema: "planificacion",
                table: "plan_semanal_indicador",
                column: "id_indicador");

            migrationBuilder.CreateIndex(
                name: "ix_plan_semanal_indicador_id_plan_semanal_id_indicador",
                schema: "planificacion",
                table: "plan_semanal_indicador",
                columns: new[] { "id_plan_semanal", "id_indicador" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_indicador_area_codigo_area",
                schema: "planificacion",
                table: "indicador",
                column: "codigo_area",
                principalSchema: "planificacion",
                principalTable: "area",
                principalColumn: "codigo",
                onDelete: ReferentialAction.Restrict);

            // Catálogo real completo de departamentos/jefaturas de la
            // cooperativa, verificado en vivo contra GENERAL.DEPARTAMENTO
            // de Softbank (27 filas, 1 inactiva -- código 13, "GERENCIA
            // GENERAL" duplicada de la fila 6, excluida acá, mismo
            // criterio ya aplicado a otras anomalías reales del proyecto
            // como el código 671 del CUC). Pedido explícito del usuario:
            // "que todas las áreas suban" a Planificación, no solo las 6
            // ya sembradas ad-hoc en la ronda original (TI/CONTABILIDAD/
            // TESORERIA/RIESGOS/NEGOCIOS/AUDITORIA, que no tienen 1:1 con
            // un SIGLAS real -- se mantienen intactas, los usuarios ya
            // están vinculados a esos códigos). 4 departamentos reales
            // (TECNOLOGIA/NEGOCIOS/AUDITORIA INTERNA/UNIDAD DE RIESGO) ya
            // están cubiertos conceptualmente por esas 6, no se duplican
            // acá con su sigla real. "TAH" aparece dos veces en la fuente
            // real para dos departamentos reales distintos (anomalía real
            // de Softbank, no un error de esta migración) -- desambiguado
            // con el sufijo TAH2 en el segundo.
            migrationBuilder.Sql("""
                INSERT INTO planificacion.area (codigo, nombre, activo) VALUES
                ('FIN',  'Financiero', true),
                ('OYP',  'Operaciones', true),
                ('TAH',  'Subgerencia Administrativa Financiera', true),
                ('GEG',  'Gerencia General', true),
                ('LEG',  'Legal', true),
                ('CUM',  'Cumplimiento', true),
                ('TAH2', 'Administración de TTHH y Materiales', true),
                ('CGR',  'Crédito Grupal', true),
                ('MYC',  'Mensajería y Conserjería', true),
                ('OPE',  'Procesos', true),
                ('AFI',  'Administración Financiera', true),
                ('CDV',  'Consejo de Vigilancia', true),
                ('UDC',  'Unidad de Cumplimiento', true),
                ('AGE',  'Agencia', true),
                ('CIN',  'Control Interno', true),
                ('COB',  'Cobranzas', true),
                ('MKT',  'Marketing', true),
                ('SFI',  'Seguridad Física', true),
                ('SIN',  'Seguridad de la Información', true),
                ('SOC',  'Salud Ocupacional', true),
                ('CDA',  'Consejo de Administración', true),
                ('REC',  'Recaudación', true)
                ON CONFLICT (codigo) DO NOTHING;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM planificacion.area WHERE codigo IN
                ('FIN','OYP','TAH','GEG','LEG','CUM','TAH2','CGR','MYC','OPE',
                 'AFI','CDV','UDC','AGE','CIN','COB','MKT','SFI','SIN','SOC','CDA','REC');
                """);

            migrationBuilder.DropForeignKey(
                name: "fk_indicador_area_codigo_area",
                schema: "planificacion",
                table: "indicador");

            migrationBuilder.DropTable(
                name: "plan_semanal_indicador",
                schema: "planificacion");

            migrationBuilder.DropIndex(
                name: "ix_indicador_codigo_area_nombre",
                schema: "planificacion",
                table: "indicador");

            migrationBuilder.DropColumn(
                name: "aprobado_por",
                schema: "planificacion",
                table: "plan_semanal");

            migrationBuilder.DropColumn(
                name: "estado_aprobacion",
                schema: "planificacion",
                table: "plan_semanal");

            migrationBuilder.DropColumn(
                name: "fecha_aprobacion",
                schema: "planificacion",
                table: "plan_semanal");

            migrationBuilder.DropColumn(
                name: "codigo_area",
                schema: "planificacion",
                table: "indicador");

            migrationBuilder.DropColumn(
                name: "unidad",
                schema: "planificacion",
                table: "indicador");
        }
    }
}
