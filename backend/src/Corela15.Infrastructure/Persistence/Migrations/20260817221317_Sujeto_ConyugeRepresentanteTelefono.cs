using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Sujeto_ConyugeRepresentanteTelefono : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "conyuge",
                schema: "sujeto",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_persona_natural = table.Column<Guid>(type: "uuid", nullable: false),
                    id_persona_conyuge = table.Column<Guid>(type: "uuid", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_conyuge", x => x.id);
                    table.ForeignKey(
                        name: "fk_conyuge_persona_natural_id_persona_natural",
                        column: x => x.id_persona_natural,
                        principalSchema: "sujeto",
                        principalTable: "persona_natural",
                        principalColumn: "id_persona",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_conyuge_personas_id_persona_conyuge",
                        column: x => x.id_persona_conyuge,
                        principalSchema: "sujeto",
                        principalTable: "persona",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "persona_telefono",
                schema: "sujeto",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_persona = table.Column<Guid>(type: "uuid", nullable: false),
                    telefono = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    es_telefono_movil = table.Column<bool>(type: "boolean", nullable: false),
                    es_principal = table.Column<bool>(type: "boolean", nullable: false),
                    notificacion_sms = table.Column<bool>(type: "boolean", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_persona_telefono", x => x.id);
                    table.ForeignKey(
                        name: "fk_persona_telefono_persona_id_persona",
                        column: x => x.id_persona,
                        principalSchema: "sujeto",
                        principalTable: "persona",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "representante",
                schema: "sujeto",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_persona = table.Column<Guid>(type: "uuid", nullable: false),
                    id_persona_representante = table.Column<Guid>(type: "uuid", nullable: false),
                    principal = table.Column<bool>(type: "boolean", nullable: false),
                    ejerce_control = table.Column<bool>(type: "boolean", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_representante", x => x.id);
                    table.ForeignKey(
                        name: "fk_representante_persona_id_persona",
                        column: x => x.id_persona,
                        principalSchema: "sujeto",
                        principalTable: "persona",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_representante_persona_id_persona_representante",
                        column: x => x.id_persona_representante,
                        principalSchema: "sujeto",
                        principalTable: "persona",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_conyuge_id_persona_conyuge",
                schema: "sujeto",
                table: "conyuge",
                column: "id_persona_conyuge");

            migrationBuilder.CreateIndex(
                name: "ix_conyuge_id_persona_natural",
                schema: "sujeto",
                table: "conyuge",
                column: "id_persona_natural");

            migrationBuilder.CreateIndex(
                name: "ix_persona_telefono_id_persona",
                schema: "sujeto",
                table: "persona_telefono",
                column: "id_persona");

            migrationBuilder.CreateIndex(
                name: "ix_representante_id_persona",
                schema: "sujeto",
                table: "representante",
                column: "id_persona");

            migrationBuilder.CreateIndex(
                name: "ix_representante_id_persona_representante",
                schema: "sujeto",
                table: "representante",
                column: "id_persona_representante");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "conyuge",
                schema: "sujeto");

            migrationBuilder.DropTable(
                name: "persona_telefono",
                schema: "sujeto");

            migrationBuilder.DropTable(
                name: "representante",
                schema: "sujeto");
        }
    }
}
