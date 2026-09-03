using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Seguridad_TipoEstructuraYModuloEstructuras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tipo_estructura",
                schema: "seguridad",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipo_estructura", x => x.codigo);
                });

            migrationBuilder.CreateTable(
                name: "rol_tipo_estructura",
                schema: "seguridad",
                columns: table => new
                {
                    id_rol = table.Column<int>(type: "integer", nullable: false),
                    codigo_tipo_estructura = table.Column<string>(type: "character varying(20)", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rol_tipo_estructura", x => new { x.id_rol, x.codigo_tipo_estructura });
                    table.ForeignKey(
                        name: "fk_rol_tipo_estructura_rol_id_rol",
                        column: x => x.id_rol,
                        principalSchema: "seguridad",
                        principalTable: "rol",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_rol_tipo_estructura_tipo_estructura_codigo_tipo_estructura",
                        column: x => x.codigo_tipo_estructura,
                        principalSchema: "seguridad",
                        principalTable: "tipo_estructura",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_rol_tipo_estructura_codigo_tipo_estructura",
                schema: "seguridad",
                table: "rol_tipo_estructura",
                column: "codigo_tipo_estructura");

            // Módulo nuevo real: "Estructuras y Procesos Financieros" —
            // primer módulo pensado desde el diseño para publicarse por
            // separado del resto del core (empieza con OF01, la estructura
            // real que la SEPS exige desde el corte 30-sep-2026, ver
            // CLAUDE.md). El ADMINISTRADOR recibe el módulo y la
            // estructura por defecto, mismo criterio que todo menú nuevo
            // ya sembrado; roles/usuarios genéricos con acceso acotado se
            // crean después desde la pantalla real de permisos.
            migrationBuilder.Sql("""
                INSERT INTO seguridad.menu (codigo, nombre, orden, activo) VALUES
                ('estructuras-financieras', 'Estructuras y Procesos Financieros', 16, true);

                INSERT INTO seguridad.rol_menu (id_rol, id_menu, activo)
                SELECT r.id, m.id, true
                FROM seguridad.rol r, seguridad.menu m
                WHERE r.nombre = 'ADMINISTRADOR' AND m.codigo = 'estructuras-financieras';

                INSERT INTO seguridad.tipo_estructura (codigo, nombre, activo) VALUES
                ('OF01', 'Obligaciones Financieras (OF01)', true);

                INSERT INTO seguridad.rol_tipo_estructura (id_rol, codigo_tipo_estructura, activo)
                SELECT r.id, 'OF01', true
                FROM seguridad.rol r
                WHERE r.nombre = 'ADMINISTRADOR';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM seguridad.rol_tipo_estructura WHERE codigo_tipo_estructura = 'OF01';
                DELETE FROM seguridad.tipo_estructura WHERE codigo = 'OF01';
                DELETE FROM seguridad.rol_menu WHERE id_menu = (SELECT id FROM seguridad.menu WHERE codigo = 'estructuras-financieras');
                DELETE FROM seguridad.menu WHERE codigo = 'estructuras-financieras';
                """);

            migrationBuilder.DropTable(
                name: "rol_tipo_estructura",
                schema: "seguridad");

            migrationBuilder.DropTable(
                name: "tipo_estructura",
                schema: "seguridad");
        }
    }
}
