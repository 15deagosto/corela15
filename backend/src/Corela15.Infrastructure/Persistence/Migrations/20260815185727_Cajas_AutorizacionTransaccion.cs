using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Cajas_AutorizacionTransaccion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "autorizacion_transaccion",
                schema: "cajas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_cuenta = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_tipo_transaccion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    monto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    detalle = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    procesado = table.Column<bool>(type: "boolean", nullable: false),
                    autorizada = table.Column<bool>(type: "boolean", nullable: true),
                    autorizado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    fecha_autorizacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    comentario_rechazo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    modificado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    modificado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_autorizacion_transaccion", x => x.id);
                    table.ForeignKey(
                        name: "fk_autorizacion_transaccion_cuentas_id_cuenta",
                        column: x => x.id_cuenta,
                        principalSchema: "ahorros",
                        principalTable: "cuenta",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_autorizacion_transaccion_id_cuenta",
                schema: "cajas",
                table: "autorizacion_transaccion",
                column: "id_cuenta");

            migrationBuilder.CreateIndex(
                name: "ix_autorizacion_transaccion_procesado",
                schema: "cajas",
                table: "autorizacion_transaccion",
                column: "procesado");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "autorizacion_transaccion",
                schema: "cajas");
        }
    }
}
