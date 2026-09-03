using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    /// <summary>
    /// Siembra dos catálogos reales verificados en vivo contra Softbank
    /// (Ronda 5 del plan): GENERAL.ACTIVIDAD_ECONOMICA (jerarquía CIIU real,
    /// 3.046 de 3.063 filas activas sembradas — 6 anomalías de longitud de
    /// código y 1 código duplicado real excluidos, ver
    /// Persistence/Seeds/ActividadEconomica.sql y CLAUDE.md) y
    /// SUJETO.PROFESION (861 filas). Cierra el campo huérfano
    /// Persona.IdActividadEconomica (sembrado desde Nivel 0 sin catálogo
    /// real detrás) y agrega PersonaNatural.CodigoProfesion.
    /// </summary>
    public partial class General_ActividadEconomicaYProfesion : Migration
    {
        private static string LeerRecurso(string nombre)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream($"Corela15.Infrastructure.Persistence.Seeds.{nombre}");
            using var reader = new StreamReader(stream!);
            return reader.ReadToEnd();
        }

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "codigo_profesion",
                schema: "sujeto",
                table: "persona_natural",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "profesion",
                schema: "sujeto",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_profesion", x => x.codigo);
                });

            migrationBuilder.Sql(LeerRecurso("Profesion.sql"));

            migrationBuilder.CreateTable(
                name: "tipo_actividad",
                schema: "general",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipo_actividad", x => x.id);
                });

            migrationBuilder.Sql(LeerRecurso("TipoActividad.sql"));

            migrationBuilder.CreateTable(
                name: "actividad_economica",
                schema: "general",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    id_tipo_actividad = table.Column<int>(type: "integer", nullable: false),
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    id_actividad_padre = table.Column<int>(type: "integer", nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_actividad_economica", x => x.id);
                    table.ForeignKey(
                        name: "fk_actividad_economica_actividad_economica_id_actividad_padre",
                        column: x => x.id_actividad_padre,
                        principalSchema: "general",
                        principalTable: "actividad_economica",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_actividad_economica_tipos_actividad_id_tipo_actividad",
                        column: x => x.id_tipo_actividad,
                        principalSchema: "general",
                        principalTable: "tipo_actividad",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql(LeerRecurso("ActividadEconomica.sql"));

            migrationBuilder.CreateIndex(
                name: "ix_persona_natural_codigo_profesion",
                schema: "sujeto",
                table: "persona_natural",
                column: "codigo_profesion");

            migrationBuilder.CreateIndex(
                name: "ix_persona_id_actividad_economica",
                schema: "sujeto",
                table: "persona",
                column: "id_actividad_economica");

            migrationBuilder.CreateIndex(
                name: "ix_actividad_economica_codigo",
                schema: "general",
                table: "actividad_economica",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_actividad_economica_id_actividad_padre",
                schema: "general",
                table: "actividad_economica",
                column: "id_actividad_padre");

            migrationBuilder.CreateIndex(
                name: "ix_actividad_economica_id_tipo_actividad",
                schema: "general",
                table: "actividad_economica",
                column: "id_tipo_actividad");

            migrationBuilder.AddForeignKey(
                name: "fk_persona_actividad_economica_id_actividad_economica",
                schema: "sujeto",
                table: "persona",
                column: "id_actividad_economica",
                principalSchema: "general",
                principalTable: "actividad_economica",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_persona_natural_profesion_codigo_profesion",
                schema: "sujeto",
                table: "persona_natural",
                column: "codigo_profesion",
                principalSchema: "sujeto",
                principalTable: "profesion",
                principalColumn: "codigo",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_persona_actividad_economica_id_actividad_economica",
                schema: "sujeto",
                table: "persona");

            migrationBuilder.DropForeignKey(
                name: "fk_persona_natural_profesion_codigo_profesion",
                schema: "sujeto",
                table: "persona_natural");

            migrationBuilder.DropTable(
                name: "actividad_economica",
                schema: "general");

            migrationBuilder.DropTable(
                name: "profesion",
                schema: "sujeto");

            migrationBuilder.DropTable(
                name: "tipo_actividad",
                schema: "general");

            migrationBuilder.DropIndex(
                name: "ix_persona_natural_codigo_profesion",
                schema: "sujeto",
                table: "persona_natural");

            migrationBuilder.DropIndex(
                name: "ix_persona_id_actividad_economica",
                schema: "sujeto",
                table: "persona");

            migrationBuilder.DropColumn(
                name: "codigo_profesion",
                schema: "sujeto",
                table: "persona_natural");
        }
    }
}
