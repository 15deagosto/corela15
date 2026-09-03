using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LavadoActivos_AlertaListaControl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tipo_lista_control",
                schema: "lavadoactivos",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipo_lista_control", x => x.codigo);
                });

            // Los 7 códigos reales, verificados en vivo contra SUJETO.TIPO_LISTACONTROL.
            migrationBuilder.InsertData(
                schema: "lavadoactivos", table: "tipo_lista_control", columns: new[] { "codigo", "nombre", "activo" },
                values: new object[,]
                {
                    { "001", "Catastro de empresas fantasma y personas naturales con transacciones inexistentes", true },
                    { "002", "Homónimos", true },
                    { "003", "Sentenciados", true },
                    { "004", "Personas expuestas políticamente (PEP)", true },
                    { "005", "Lista ONU", true },
                    { "006", "Lista OFAC", true },
                    { "007", "Paraísos fiscales", true },
                });

            migrationBuilder.CreateTable(
                name: "alerta_lista_control",
                schema: "lavadoactivos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_persona = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_tipo_lista_control = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    detalle = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    fecha_deteccion = table.Column<DateOnly>(type: "date", nullable: false),
                    resuelta = table.Column<bool>(type: "boolean", nullable: false),
                    comentario_resolucion = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    resuelto_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    fecha_resolucion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    modificado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    modificado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_alerta_lista_control", x => x.id);
                    table.ForeignKey(
                        name: "fk_alerta_lista_control_personas_id_persona",
                        column: x => x.id_persona,
                        principalSchema: "sujeto",
                        principalTable: "persona",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_alerta_lista_control_tipo_lista_control_codigo_tipo_lista_c",
                        column: x => x.codigo_tipo_lista_control,
                        principalSchema: "lavadoactivos",
                        principalTable: "tipo_lista_control",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_alerta_lista_control_codigo_tipo_lista_control",
                schema: "lavadoactivos",
                table: "alerta_lista_control",
                column: "codigo_tipo_lista_control");

            migrationBuilder.CreateIndex(
                name: "ix_alerta_lista_control_id_persona",
                schema: "lavadoactivos",
                table: "alerta_lista_control",
                column: "id_persona");

            migrationBuilder.CreateIndex(
                name: "ix_alerta_lista_control_resuelta",
                schema: "lavadoactivos",
                table: "alerta_lista_control",
                column: "resuelta");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alerta_lista_control",
                schema: "lavadoactivos");

            migrationBuilder.DropTable(
                name: "tipo_lista_control",
                schema: "lavadoactivos");
        }
    }
}
