using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Nivel0_MenuRolMenu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "menu",
                schema: "seguridad",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    orden = table.Column<int>(type: "integer", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_menu", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rol_menu",
                schema: "seguridad",
                columns: table => new
                {
                    id_rol = table.Column<int>(type: "integer", nullable: false),
                    id_menu = table.Column<int>(type: "integer", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rol_menu", x => new { x.id_rol, x.id_menu });
                    table.ForeignKey(
                        name: "fk_rol_menu_menu_id_menu",
                        column: x => x.id_menu,
                        principalSchema: "seguridad",
                        principalTable: "menu",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_rol_menu_rol_id_rol",
                        column: x => x.id_rol,
                        principalSchema: "seguridad",
                        principalTable: "rol",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_menu_codigo",
                schema: "seguridad",
                table: "menu",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_rol_menu_id_menu",
                schema: "seguridad",
                table: "rol_menu",
                column: "id_menu");

            // Catálogo de menús — el código coincide 1:1 con el `slug` de
            // cada módulo en frontend/src/modules.ts.
            migrationBuilder.Sql(
                """
                INSERT INTO seguridad.menu (codigo, nombre, orden, activo) VALUES
                    ('socios', 'Socios', 1, true),
                    ('usuarios-roles', 'Usuarios y roles', 2, true),
                    ('contabilidad', 'Contabilidad', 3, true),
                    ('ahorros', 'Ahorros', 4, true),
                    ('creditos', 'Créditos y Plazo Fijo', 5, true),
                    ('cobranzas-cumplimiento', 'Cobranzas y Cumplimiento', 6, true),
                    ('cajas', 'Cajas', 7, true),
                    ('nomina', 'Nómina', 8, true),
                    ('tesoreria', 'Tesorería', 9, true),
                    ('riesgo', 'Riesgo', 10, true),
                    ('configuracion', 'Configuración', 11, true);
                """);

            // Permisos por defecto: ADMINISTRADOR ve todo. Los demás roles
            // sembrados en Nivel0_SeedDatosPrueba reciben solo lo propio de
            // su función — punto de partida razonable, ajustable después
            // desde Configuración > Roles (cuando se construya esa pantalla).
            migrationBuilder.Sql(
                """
                INSERT INTO seguridad.rol_menu (id_rol, id_menu, activo)
                SELECT r.id, m.id, true
                FROM seguridad.rol r, seguridad.menu m
                WHERE r.nombre = 'ADMINISTRADOR';

                INSERT INTO seguridad.rol_menu (id_rol, id_menu, activo)
                SELECT r.id, m.id, true
                FROM seguridad.rol r, seguridad.menu m
                WHERE r.nombre = 'CAJERO' AND m.codigo IN ('cajas', 'ahorros', 'socios');

                INSERT INTO seguridad.rol_menu (id_rol, id_menu, activo)
                SELECT r.id, m.id, true
                FROM seguridad.rol r, seguridad.menu m
                WHERE r.nombre = 'ASESOR DE CREDITO' AND m.codigo IN ('creditos', 'cobranzas-cumplimiento', 'socios');

                INSERT INTO seguridad.rol_menu (id_rol, id_menu, activo)
                SELECT r.id, m.id, true
                FROM seguridad.rol r, seguridad.menu m
                WHERE r.nombre = 'OFICIAL DE CAPTACIONES' AND m.codigo IN ('ahorros', 'creditos', 'socios');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rol_menu",
                schema: "seguridad");

            migrationBuilder.DropTable(
                name: "menu",
                schema: "seguridad");
        }
    }
}
