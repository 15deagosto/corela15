using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Seguridad_OpcionesYCreditosReportes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "ak_menu_codigo",
                schema: "seguridad",
                table: "menu",
                column: "codigo");

            migrationBuilder.CreateTable(
                name: "opcion",
                schema: "seguridad",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    codigo_menu = table.Column<string>(type: "character varying(50)", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_opcion", x => x.codigo);
                    table.ForeignKey(
                        name: "fk_opcion_menu_codigo_menu",
                        column: x => x.codigo_menu,
                        principalSchema: "seguridad",
                        principalTable: "menu",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rol_opcion",
                schema: "seguridad",
                columns: table => new
                {
                    id_rol = table.Column<int>(type: "integer", nullable: false),
                    codigo_opcion = table.Column<string>(type: "character varying(80)", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rol_opcion", x => new { x.id_rol, x.codigo_opcion });
                    table.ForeignKey(
                        name: "fk_rol_opcion_opcion_codigo_opcion",
                        column: x => x.codigo_opcion,
                        principalSchema: "seguridad",
                        principalTable: "opcion",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_rol_opcion_rol_id_rol",
                        column: x => x.id_rol,
                        principalSchema: "seguridad",
                        principalTable: "rol",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuario_opcion",
                schema: "seguridad",
                columns: table => new
                {
                    id_usuario = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_opcion = table.Column<string>(type: "character varying(80)", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuario_opcion", x => new { x.id_usuario, x.codigo_opcion });
                    table.ForeignKey(
                        name: "fk_usuario_opcion_opcion_codigo_opcion",
                        column: x => x.codigo_opcion,
                        principalSchema: "seguridad",
                        principalTable: "opcion",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_usuario_opcion_usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalSchema: "seguridad",
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_opcion_codigo_menu",
                schema: "seguridad",
                table: "opcion",
                column: "codigo_menu");

            migrationBuilder.CreateIndex(
                name: "ix_rol_opcion_codigo_opcion",
                schema: "seguridad",
                table: "rol_opcion",
                column: "codigo_opcion");

            migrationBuilder.CreateIndex(
                name: "ix_usuario_opcion_codigo_opcion",
                schema: "seguridad",
                table: "usuario_opcion",
                column: "codigo_opcion");

            // Primer caso real del tercer nivel de permiso: los 20 reportes
            // de Créditos, cada uno gateable por separado -- pedido explícito
            // del usuario, "a veces querrán acceder a un solo reporte...
            // eso es lo que quería que sea super seguro, no dar todo un
            // módulo solo por una opción o reporte que se necesite". Códigos
            // 1:1 con el id de pestaña real en Creditos.tsx (SeccionReportesCreditos).
            migrationBuilder.Sql("""
                INSERT INTO seguridad.opcion (codigo, nombre, codigo_menu, activo) VALUES
                ('creditos.reportes.concesion', 'Reporte: Concesión de crédito', 'creditos', true),
                ('creditos.reportes.precancelados', 'Reporte: Créditos precancelados', 'creditos', true),
                ('creditos.reportes.vencimientos', 'Reporte: Próximos vencimientos', 'creditos', true),
                ('creditos.reportes.cancelados', 'Reporte: Créditos cancelados', 'creditos', true),
                ('creditos.reportes.garantias', 'Reporte: Anexo garantías', 'creditos', true),
                ('creditos.reportes.mora-asesor', 'Reporte: Créditos en mora por asesor', 'creditos', true),
                ('creditos.reportes.indice-morosidad', 'Reporte: Índice de morosidad', 'creditos', true),
                ('creditos.reportes.spi-no-procesados', 'Reporte: Débitos SPI no procesados', 'creditos', true),
                ('creditos.reportes.cartera-castigada', 'Reporte: Anexo cartera castigada', 'creditos', true),
                ('creditos.reportes.calificacion', 'Reporte: Calificación y provisión', 'creditos', true),
                ('creditos.reportes.gastos-judiciales', 'Reporte: Gastos judiciales', 'creditos', true),
                ('creditos.reportes.consolidado-tipo-cartera', 'Reporte: Consolidado por producto', 'creditos', true),
                ('creditos.reportes.seguro-desgravamen', 'Reporte: Seguro desgravamen', 'creditos', true),
                ('creditos.reportes.castigada-agencia', 'Reporte: Cartera castigada por agencia', 'creditos', true),
                ('creditos.reportes.castigada-cliente', 'Reporte: Cartera castigada por cliente', 'creditos', true),
                ('creditos.reportes.item-credito', 'Reporte: Anexo ítems de crédito', 'creditos', true),
                ('creditos.reportes.vinculados', 'Reporte: Créditos vinculados', 'creditos', true),
                ('creditos.reportes.por-convenio', 'Reporte: Préstamos por convenio', 'creditos', true),
                ('creditos.reportes.abonos-convenio', 'Reporte: Abonos por convenio', 'creditos', true),
                ('creditos.reportes.entrega-recuperacion', 'Reporte: Entrega vs recuperación', 'creditos', true);

                INSERT INTO seguridad.rol_opcion (id_rol, codigo_opcion, activo)
                SELECT r.id, o.codigo, true
                FROM seguridad.rol r, seguridad.opcion o
                WHERE r.nombre = 'ADMINISTRADOR' AND o.codigo_menu = 'creditos';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rol_opcion",
                schema: "seguridad");

            migrationBuilder.DropTable(
                name: "usuario_opcion",
                schema: "seguridad");

            migrationBuilder.DropTable(
                name: "opcion",
                schema: "seguridad");

            migrationBuilder.DropUniqueConstraint(
                name: "ak_menu_codigo",
                schema: "seguridad",
                table: "menu");
        }
    }
}
