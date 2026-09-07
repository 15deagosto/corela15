using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Planificacion_AreasReales_EnvioNotas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "codigo_area_planificacion",
                schema: "seguridad",
                table: "usuario",
                type: "character varying(20)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "nota_gerencia",
                schema: "planificacion",
                table: "plan_semanal_bloque",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "enviada",
                schema: "planificacion",
                table: "plan_semanal",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "enviada_fuera_de_tiempo",
                schema: "planificacion",
                table: "plan_semanal",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "enviada_por",
                schema: "planificacion",
                table: "plan_semanal",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "fecha_envio",
                schema: "planificacion",
                table: "plan_semanal",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "nota_gerencia",
                schema: "planificacion",
                table: "plan_semanal",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_usuario_codigo_area_planificacion",
                schema: "seguridad",
                table: "usuario",
                column: "codigo_area_planificacion");

            migrationBuilder.AddForeignKey(
                name: "fk_usuario_area_codigo_area_planificacion",
                schema: "seguridad",
                table: "usuario",
                column: "codigo_area_planificacion",
                principalSchema: "planificacion",
                principalTable: "area",
                principalColumn: "codigo",
                onDelete: ReferentialAction.Restrict);

            // Las 6 jefaturas reales de la cooperativa (ver CLAUDE.md,
            // sección "Sexto lote de Colocación + motor real de Convenios"
            // no -- acá corresponde a la nota del usuario sobre las
            // jefaturas reales: Tecnologías/Contabilidad/Tesorería/
            // Riesgos/Negocios/Auditoría Interna). "TI" ya existía desde
            // la migración original del módulo -- ON CONFLICT para no
            // duplicarla ni pisar su nombre real.
            migrationBuilder.Sql(@"
                INSERT INTO planificacion.area (codigo, nombre, activo) VALUES
                    ('TI', 'Tecnologías de la Información', true),
                    ('CONTABILIDAD', 'Contabilidad', true),
                    ('TESORERIA', 'Tesorería', true),
                    ('RIESGOS', 'Riesgos', true),
                    ('NEGOCIOS', 'Negocios', true),
                    ('AUDITORIA', 'Auditoría Interna', true)
                ON CONFLICT (codigo) DO NOTHING;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_usuario_area_codigo_area_planificacion",
                schema: "seguridad",
                table: "usuario");

            migrationBuilder.DropIndex(
                name: "ix_usuario_codigo_area_planificacion",
                schema: "seguridad",
                table: "usuario");

            migrationBuilder.DropColumn(
                name: "codigo_area_planificacion",
                schema: "seguridad",
                table: "usuario");

            migrationBuilder.DropColumn(
                name: "nota_gerencia",
                schema: "planificacion",
                table: "plan_semanal_bloque");

            migrationBuilder.DropColumn(
                name: "enviada",
                schema: "planificacion",
                table: "plan_semanal");

            migrationBuilder.DropColumn(
                name: "enviada_fuera_de_tiempo",
                schema: "planificacion",
                table: "plan_semanal");

            migrationBuilder.DropColumn(
                name: "enviada_por",
                schema: "planificacion",
                table: "plan_semanal");

            migrationBuilder.DropColumn(
                name: "fecha_envio",
                schema: "planificacion",
                table: "plan_semanal");

            migrationBuilder.DropColumn(
                name: "nota_gerencia",
                schema: "planificacion",
                table: "plan_semanal");
        }
    }
}
