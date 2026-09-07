using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Planificacion_EtiquetaPorArea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "codigo_etiqueta",
                schema: "planificacion",
                table: "plan_semanal_bloque",
                type: "character varying(40)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)");

            migrationBuilder.AlterColumn<string>(
                name: "codigo",
                schema: "planificacion",
                table: "etiqueta",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "codigo_area",
                schema: "planificacion",
                table: "etiqueta",
                type: "character varying(20)",
                nullable: true);

            // Backfill real: las 7 etiquetas ya sembradas son exactamente
            // las categorías reales del área TI (ver PDF de referencia
            // original) -- nunca un valor por defecto vacío/inventado.
            migrationBuilder.Sql("UPDATE planificacion.etiqueta SET codigo_area = 'TI' WHERE codigo_area IS NULL;");

            migrationBuilder.AlterColumn<string>(
                name: "codigo_area",
                schema: "planificacion",
                table: "etiqueta",
                type: "character varying(20)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_etiqueta_codigo_area_nombre",
                schema: "planificacion",
                table: "etiqueta",
                columns: new[] { "codigo_area", "nombre" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_etiqueta_area_codigo_area",
                schema: "planificacion",
                table: "etiqueta",
                column: "codigo_area",
                principalSchema: "planificacion",
                principalTable: "area",
                principalColumn: "codigo",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_etiqueta_area_codigo_area",
                schema: "planificacion",
                table: "etiqueta");

            migrationBuilder.DropIndex(
                name: "ix_etiqueta_codigo_area_nombre",
                schema: "planificacion",
                table: "etiqueta");

            migrationBuilder.DropColumn(
                name: "codigo_area",
                schema: "planificacion",
                table: "etiqueta");

            migrationBuilder.AlterColumn<string>(
                name: "codigo_etiqueta",
                schema: "planificacion",
                table: "plan_semanal_bloque",
                type: "character varying(20)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(40)");

            migrationBuilder.AlterColumn<string>(
                name: "codigo",
                schema: "planificacion",
                table: "etiqueta",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(40)",
                oldMaxLength: 40);
        }
    }
}
