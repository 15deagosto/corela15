using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Documentos_Motor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "documentos");

            migrationBuilder.CreateTable(
                name: "documento",
                schema: "documentos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    titulo = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    area = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre_archivo_original = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ruta_almacenamiento = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    tamano_bytes = table.Column<long>(type: "bigint", nullable: false),
                    content_type = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    instancia_aprobacion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    instancia_revision = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    fecha_aprobacion = table.Column<DateOnly>(type: "date", nullable: true),
                    proxima_revision = table.Column<DateOnly>(type: "date", nullable: true),
                    notas = table.Column<string>(type: "text", nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    modificado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    modificado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_documento", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_documento_activo",
                schema: "documentos",
                table: "documento",
                column: "activo");

            migrationBuilder.CreateIndex(
                name: "ix_documento_area",
                schema: "documentos",
                table: "documento",
                column: "area");

            migrationBuilder.CreateIndex(
                name: "ix_documento_estado",
                schema: "documentos",
                table: "documento",
                column: "estado");

            migrationBuilder.CreateIndex(
                name: "ix_documento_tipo",
                schema: "documentos",
                table: "documento",
                column: "tipo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "documento",
                schema: "documentos");
        }
    }
}
