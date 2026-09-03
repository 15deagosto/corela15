using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Cajas_FormaNumerada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tipo_forma_numerada",
                schema: "cajas",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipo_forma_numerada", x => x.codigo);
                });

            // Catálogo real — verificado contra CAJAS.TIPO_FORMANUMERADA
            // (4 filas reales). Los rangos asignados por agencia
            // (CAJAS.FORMA_NUMERADA, 8 filas reales) no se siembran: están
            // ligados a las agencias reales de Softbank (Pilacoto, San
            // Silvestre...), que este core no tiene sembradas — se crean
            // desde la pantalla cuando la agencia real exista acá.
            migrationBuilder.Sql(@"
INSERT INTO cajas.tipo_forma_numerada (codigo, nombre, activo) VALUES
('CDPF', 'Certificados DPF', true),
('LA', 'Libretas de ahorro', true),
('PD', 'Papeleta de depósito', true),
('PR', 'Papeleta de retiro', true);
");

            migrationBuilder.CreateTable(
                name: "forma_numerada",
                schema: "cajas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_agencia = table.Column<int>(type: "integer", nullable: false),
                    codigo_tipo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    inicio = table.Column<int>(type: "integer", nullable: false),
                    fin = table.Column<int>(type: "integer", nullable: false),
                    fecha_asignacion = table.Column<DateOnly>(type: "date", nullable: false),
                    registrado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_forma_numerada", x => x.id);
                    table.ForeignKey(
                        name: "fk_forma_numerada_agencia_id_agencia",
                        column: x => x.id_agencia,
                        principalSchema: "general",
                        principalTable: "agencia",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_forma_numerada_tipo_forma_numerada_codigo_tipo",
                        column: x => x.codigo_tipo,
                        principalSchema: "cajas",
                        principalTable: "tipo_forma_numerada",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_forma_numerada_codigo_tipo",
                schema: "cajas",
                table: "forma_numerada",
                column: "codigo_tipo");

            migrationBuilder.CreateIndex(
                name: "ix_forma_numerada_id_agencia",
                schema: "cajas",
                table: "forma_numerada",
                column: "id_agencia");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "forma_numerada",
                schema: "cajas");

            migrationBuilder.DropTable(
                name: "tipo_forma_numerada",
                schema: "cajas");
        }
    }
}
