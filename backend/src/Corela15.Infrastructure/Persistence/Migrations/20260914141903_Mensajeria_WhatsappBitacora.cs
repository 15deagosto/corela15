using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Mensajeria_WhatsappBitacora : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "mensajeria");

            migrationBuilder.CreateTable(
                name: "mensaje_whatsapp",
                schema: "mensajeria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero_destino = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    id_persona_destino = table.Column<Guid>(type: "uuid", nullable: true),
                    texto = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    id_mensaje_externo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    detalle_error = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    id_agencia = table.Column<int>(type: "integer", nullable: false),
                    enviado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mensaje_whatsapp", x => x.id);
                    table.ForeignKey(
                        name: "fk_mensaje_whatsapp_agencia_id_agencia",
                        column: x => x.id_agencia,
                        principalSchema: "general",
                        principalTable: "agencia",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_mensaje_whatsapp_personas_id_persona_destino",
                        column: x => x.id_persona_destino,
                        principalSchema: "sujeto",
                        principalTable: "persona",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_mensaje_whatsapp_creado_en",
                schema: "mensajeria",
                table: "mensaje_whatsapp",
                column: "creado_en");

            migrationBuilder.CreateIndex(
                name: "ix_mensaje_whatsapp_id_agencia",
                schema: "mensajeria",
                table: "mensaje_whatsapp",
                column: "id_agencia");

            migrationBuilder.CreateIndex(
                name: "ix_mensaje_whatsapp_id_persona_destino",
                schema: "mensajeria",
                table: "mensaje_whatsapp",
                column: "id_persona_destino");

            migrationBuilder.CreateIndex(
                name: "ix_mensaje_whatsapp_numero_destino",
                schema: "mensajeria",
                table: "mensaje_whatsapp",
                column: "numero_destino");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mensaje_whatsapp",
                schema: "mensajeria");
        }
    }
}
