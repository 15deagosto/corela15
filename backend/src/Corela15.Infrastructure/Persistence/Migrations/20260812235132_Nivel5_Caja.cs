using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Nivel5_Caja : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "cajas");

            migrationBuilder.CreateTable(
                name: "denominacion",
                schema: "cajas",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tipo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    valor = table.Column<decimal>(type: "numeric(9,2)", nullable: false),
                    con_serie = table.Column<bool>(type: "boolean", nullable: false),
                    orden = table.Column<int>(type: "integer", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_denominacion", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ventanilla",
                schema: "cajas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    id_agencia = table.Column<int>(type: "integer", nullable: false),
                    id_usuario = table.Column<Guid>(type: "uuid", nullable: false),
                    cuadrada = table.Column<bool>(type: "boolean", nullable: false),
                    cerrada = table.Column<bool>(type: "boolean", nullable: false),
                    puede_transaccionar = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ventanilla", x => x.id);
                    table.ForeignKey(
                        name: "fk_ventanilla_agencia_id_agencia",
                        column: x => x.id_agencia,
                        principalSchema: "general",
                        principalTable: "agencia",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_ventanilla_usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalSchema: "seguridad",
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ventanilla_cuadre",
                schema: "cajas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_ventanilla = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha_proceso = table.Column<DateOnly>(type: "date", nullable: false),
                    total_efectivo = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    total_cheque = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    diferencia_efectivo = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    diferencia_cheque = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    esta_cuadrado = table.Column<bool>(type: "boolean", nullable: false),
                    aprobada = table.Column<bool>(type: "boolean", nullable: false),
                    activa = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ventanilla_cuadre", x => x.id);
                    table.ForeignKey(
                        name: "fk_ventanilla_cuadre_ventanilla_id_ventanilla",
                        column: x => x.id_ventanilla,
                        principalSchema: "cajas",
                        principalTable: "ventanilla",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_denominacion_tipo_valor",
                schema: "cajas",
                table: "denominacion",
                columns: new[] { "tipo", "valor" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ventanilla_id_agencia",
                schema: "cajas",
                table: "ventanilla",
                column: "id_agencia");

            migrationBuilder.CreateIndex(
                name: "ix_ventanilla_id_usuario_fecha",
                schema: "cajas",
                table: "ventanilla",
                columns: new[] { "id_usuario", "fecha" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ventanilla_cuadre_id_ventanilla",
                schema: "cajas",
                table: "ventanilla_cuadre",
                column: "id_ventanilla");

            // Denominaciones reales de USD (moneda de curso legal en Ecuador).
            migrationBuilder.Sql(
                """
                INSERT INTO cajas.denominacion (tipo, valor, con_serie, orden, activo) VALUES
                    ('Billete', 100.00, true,  1, true),
                    ('Billete', 50.00,  true,  2, true),
                    ('Billete', 20.00,  true,  3, true),
                    ('Billete', 10.00,  true,  4, true),
                    ('Billete', 5.00,   true,  5, true),
                    ('Billete', 1.00,   true,  6, true),
                    ('Moneda',  1.00,   false, 7, true),
                    ('Moneda',  0.50,   false, 8, true),
                    ('Moneda',  0.25,   false, 9, true),
                    ('Moneda',  0.10,   false, 10, true),
                    ('Moneda',  0.05,   false, 11, true),
                    ('Moneda',  0.01,   false, 12, true);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "denominacion",
                schema: "cajas");

            migrationBuilder.DropTable(
                name: "ventanilla_cuadre",
                schema: "cajas");

            migrationBuilder.DropTable(
                name: "ventanilla",
                schema: "cajas");
        }
    }
}
