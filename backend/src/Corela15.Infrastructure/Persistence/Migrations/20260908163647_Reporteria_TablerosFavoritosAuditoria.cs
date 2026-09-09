using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Reporteria_TablerosFavoritosAuditoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "reporteria");

            migrationBuilder.CreateTable(
                name: "auditoria_consulta",
                schema: "reporteria",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    usuario = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    dataset = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    dimensiones = table.Column<string>(type: "text", nullable: false),
                    metricas = table.Column<string>(type: "text", nullable: false),
                    filtros = table.Column<string>(type: "text", nullable: false),
                    snapshot = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    duracion_ms = table.Column<long>(type: "bigint", nullable: false),
                    filas = table.Column<int>(type: "integer", nullable: false),
                    error = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    fecha_hora = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_auditoria_consulta", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tablero",
                schema: "reporteria",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    definicion = table.Column<string>(type: "jsonb", nullable: false),
                    propietario = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    es_publico = table.Column<bool>(type: "boolean", nullable: false),
                    roles_permitidos = table.Column<string[]>(type: "text[]", nullable: false),
                    es_predefinido = table.Column<bool>(type: "boolean", nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modificado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tablero", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "favorito_tablero",
                schema: "reporteria",
                columns: table => new
                {
                    usuario = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    id_tablero = table.Column<int>(type: "integer", nullable: false),
                    agregado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_favorito_tablero", x => new { x.usuario, x.id_tablero });
                    table.ForeignKey(
                        name: "fk_favorito_tablero_tableros_reporteria_id_tablero",
                        column: x => x.id_tablero,
                        principalSchema: "reporteria",
                        principalTable: "tablero",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_auditoria_consulta_fecha_hora",
                schema: "reporteria",
                table: "auditoria_consulta",
                column: "fecha_hora");

            migrationBuilder.CreateIndex(
                name: "ix_auditoria_consulta_usuario",
                schema: "reporteria",
                table: "auditoria_consulta",
                column: "usuario");

            migrationBuilder.CreateIndex(
                name: "ix_favorito_tablero_id_tablero",
                schema: "reporteria",
                table: "favorito_tablero",
                column: "id_tablero");

            migrationBuilder.CreateIndex(
                name: "ix_tablero_propietario",
                schema: "reporteria",
                table: "tablero",
                column: "propietario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "auditoria_consulta",
                schema: "reporteria");

            migrationBuilder.DropTable(
                name: "favorito_tablero",
                schema: "reporteria");

            migrationBuilder.DropTable(
                name: "tablero",
                schema: "reporteria");
        }
    }
}
