using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Planificacion_DisposicionesGerencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "id_disposicion",
                schema: "planificacion",
                table: "plan_semanal_bloque",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "disposicion_gerencia",
                schema: "planificacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    titulo = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    fecha_limite = table.Column<DateOnly>(type: "date", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_disposicion_gerencia", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "disposicion_gerencia_area",
                schema: "planificacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_disposicion = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_area = table.Column<string>(type: "character varying(20)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_disposicion_gerencia_area", x => x.id);
                    table.ForeignKey(
                        name: "fk_disposicion_gerencia_area_area_codigo_area",
                        column: x => x.codigo_area,
                        principalSchema: "planificacion",
                        principalTable: "area",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_disposicion_gerencia_area_disposiciones_gerencia_id_disposi",
                        column: x => x.id_disposicion,
                        principalSchema: "planificacion",
                        principalTable: "disposicion_gerencia",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_plan_semanal_bloque_id_disposicion",
                schema: "planificacion",
                table: "plan_semanal_bloque",
                column: "id_disposicion");

            migrationBuilder.CreateIndex(
                name: "ix_disposicion_gerencia_area_codigo_area",
                schema: "planificacion",
                table: "disposicion_gerencia_area",
                column: "codigo_area");

            migrationBuilder.CreateIndex(
                name: "ix_disposicion_gerencia_area_id_disposicion_codigo_area",
                schema: "planificacion",
                table: "disposicion_gerencia_area",
                columns: new[] { "id_disposicion", "codigo_area" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_plan_semanal_bloque_disposicion_gerencia_id_disposicion",
                schema: "planificacion",
                table: "plan_semanal_bloque",
                column: "id_disposicion",
                principalSchema: "planificacion",
                principalTable: "disposicion_gerencia",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_plan_semanal_bloque_disposicion_gerencia_id_disposicion",
                schema: "planificacion",
                table: "plan_semanal_bloque");

            migrationBuilder.DropTable(
                name: "disposicion_gerencia_area",
                schema: "planificacion");

            migrationBuilder.DropTable(
                name: "disposicion_gerencia",
                schema: "planificacion");

            migrationBuilder.DropIndex(
                name: "ix_plan_semanal_bloque_id_disposicion",
                schema: "planificacion",
                table: "plan_semanal_bloque");

            migrationBuilder.DropColumn(
                name: "id_disposicion",
                schema: "planificacion",
                table: "plan_semanal_bloque");
        }
    }
}
