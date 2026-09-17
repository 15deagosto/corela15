using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Comunicacion_AdjuntosMultiples : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mensaje_adjunto",
                schema: "comunicacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_mensaje = table.Column<Guid>(type: "uuid", nullable: false),
                    ruta_adjunto = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    nombre_archivo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    content_type = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    tamano_bytes = table.Column<long>(type: "bigint", nullable: false),
                    orden = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mensaje_adjunto", x => x.id);
                    table.ForeignKey(
                        name: "fk_mensaje_adjunto_mensajes_id_mensaje",
                        column: x => x.id_mensaje,
                        principalSchema: "comunicacion",
                        principalTable: "mensaje",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_mensaje_adjunto_id_mensaje",
                schema: "comunicacion",
                table: "mensaje_adjunto",
                column: "id_mensaje");

            // Migra el adjunto real (si ya existía alguno) de la vieja columna
            // única al esquema real de múltiples adjuntos -- antes de
            // eliminar las columnas viejas, nunca se pierde un archivo real
            // ya guardado (ver el mensaje real con adjunto de la ronda
            // anterior en producción).
            migrationBuilder.Sql(@"
                INSERT INTO comunicacion.mensaje_adjunto (id, id_mensaje, ruta_adjunto, nombre_archivo, content_type, tamano_bytes, orden)
                SELECT gen_random_uuid(), id, ruta_adjunto, nombre_archivo_adjunto, content_type_adjunto, tamano_bytes_adjunto, 0
                FROM comunicacion.mensaje
                WHERE ruta_adjunto IS NOT NULL;
            ");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mensaje_adjunto",
                schema: "comunicacion");

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
    }
}
