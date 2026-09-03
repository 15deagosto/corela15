using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Colocacion_GarantiaPersonal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "estado_garantia",
                schema: "credito",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    detalle = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_estado_garantia", x => x.codigo);
                });

            // Los 5 códigos reales, verificados en vivo contra CREDITO.ESTADO_GARANTIA.
            migrationBuilder.InsertData(
                schema: "credito", table: "estado_garantia", columns: new[] { "codigo", "detalle", "activo" },
                values: new object[,]
                {
                    { "A", "Abierta", true },
                    { "C", "Cerrada", true },
                    { "L", "Levantada", true },
                    { "N", "Anulada", true },
                    { "P", "Por constituir", true },
                });

            migrationBuilder.CreateTable(
                name: "solicitud_prestamo_garantia",
                schema: "credito",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_solicitud_prestamo = table.Column<Guid>(type: "uuid", nullable: false),
                    id_cliente_garante = table.Column<Guid>(type: "uuid", nullable: false),
                    detalle = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    modificado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    modificado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_solicitud_prestamo_garantia", x => x.id);
                    table.ForeignKey(
                        name: "fk_solicitud_prestamo_garantia_cliente_id_cliente_garante",
                        column: x => x.id_cliente_garante,
                        principalSchema: "clientes",
                        principalTable: "cliente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_solicitud_prestamo_garantia_solicitud_prestamo_id_solicitud",
                        column: x => x.id_solicitud_prestamo,
                        principalSchema: "credito",
                        principalTable: "solicitud_prestamo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prestamo_garantia",
                schema: "colocacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_prestamo = table.Column<Guid>(type: "uuid", nullable: false),
                    id_cliente_garante = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_estado_garantia = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    modificado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    modificado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_prestamo_garantia", x => x.id);
                    table.ForeignKey(
                        name: "fk_prestamo_garantia_cliente_id_cliente_garante",
                        column: x => x.id_cliente_garante,
                        principalSchema: "clientes",
                        principalTable: "cliente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_prestamo_garantia_estado_garantia_codigo_estado_garantia",
                        column: x => x.codigo_estado_garantia,
                        principalSchema: "credito",
                        principalTable: "estado_garantia",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_prestamo_garantia_prestamo_id_prestamo",
                        column: x => x.id_prestamo,
                        principalSchema: "colocacion",
                        principalTable: "prestamo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_prestamo_garantia_codigo_estado_garantia",
                schema: "colocacion",
                table: "prestamo_garantia",
                column: "codigo_estado_garantia");

            migrationBuilder.CreateIndex(
                name: "ix_prestamo_garantia_id_cliente_garante",
                schema: "colocacion",
                table: "prestamo_garantia",
                column: "id_cliente_garante");

            migrationBuilder.CreateIndex(
                name: "ix_prestamo_garantia_id_prestamo",
                schema: "colocacion",
                table: "prestamo_garantia",
                column: "id_prestamo");

            migrationBuilder.CreateIndex(
                name: "ix_solicitud_prestamo_garantia_id_cliente_garante",
                schema: "credito",
                table: "solicitud_prestamo_garantia",
                column: "id_cliente_garante");

            migrationBuilder.CreateIndex(
                name: "ix_solicitud_prestamo_garantia_id_solicitud_prestamo",
                schema: "credito",
                table: "solicitud_prestamo_garantia",
                column: "id_solicitud_prestamo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "prestamo_garantia",
                schema: "colocacion");

            migrationBuilder.DropTable(
                name: "solicitud_prestamo_garantia",
                schema: "credito");

            migrationBuilder.DropTable(
                name: "estado_garantia",
                schema: "credito");
        }
    }
}
