using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Comunicacion_AdjuntosEnMensajes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "texto",
                schema: "comunicacion",
                table: "mensaje",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AddColumn<string>(
                name: "content_type_adjunto",
                schema: "comunicacion",
                table: "mensaje",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "nombre_archivo_adjunto",
                schema: "comunicacion",
                table: "mensaje",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ruta_adjunto",
                schema: "comunicacion",
                table: "mensaje",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "tamano_bytes_adjunto",
                schema: "comunicacion",
                table: "mensaje",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "content_type_adjunto",
                schema: "comunicacion",
                table: "mensaje");

            migrationBuilder.DropColumn(
                name: "nombre_archivo_adjunto",
                schema: "comunicacion",
                table: "mensaje");

            migrationBuilder.DropColumn(
                name: "ruta_adjunto",
                schema: "comunicacion",
                table: "mensaje");

            migrationBuilder.DropColumn(
                name: "tamano_bytes_adjunto",
                schema: "comunicacion",
                table: "mensaje");

            migrationBuilder.AlterColumn<string>(
                name: "texto",
                schema: "comunicacion",
                table: "mensaje",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);
        }
    }
}
