using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Seguridad_DatasetReporteria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dataset_reporteria",
                schema: "seguridad",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dataset_reporteria", x => x.codigo);
                });

            migrationBuilder.CreateTable(
                name: "rol_dataset_reporteria",
                schema: "seguridad",
                columns: table => new
                {
                    id_rol = table.Column<int>(type: "integer", nullable: false),
                    codigo_dataset = table.Column<string>(type: "character varying(30)", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rol_dataset_reporteria", x => new { x.id_rol, x.codigo_dataset });
                    table.ForeignKey(
                        name: "fk_rol_dataset_reporteria_dataset_reporteria_codigo_dataset",
                        column: x => x.codigo_dataset,
                        principalSchema: "seguridad",
                        principalTable: "dataset_reporteria",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_rol_dataset_reporteria_rol_id_rol",
                        column: x => x.id_rol,
                        principalSchema: "seguridad",
                        principalTable: "rol",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_rol_dataset_reporteria_codigo_dataset",
                schema: "seguridad",
                table: "rol_dataset_reporteria",
                column: "codigo_dataset");

            // Módulo nuevo real: "Reportería Gerencial" -- motor semántico
            // portado del proyecto SIGA (tableros de Cartera/Captaciones/
            // Contabilidad/Socios/Solicitudes/Vinculados sobre Softbank, solo
            // lectura), integrado acá para que se administre con los mismos
            // permisos que el resto de Corela15 en vez de un login/rol
            // separado. `nominal` es el dataset con identificación de socios
            // (PII) -- se siembra en el catálogo pero deliberadamente NO se
            // otorga a nadie por defecto, ni siquiera a ADMINISTRADOR: exige
            // aprobación explícita documentada, mismo criterio que
            // `siga/CLAUDE.md` ya exigía para el módulo Nominal original.
            migrationBuilder.Sql("""
                INSERT INTO seguridad.menu (codigo, nombre, orden, activo) VALUES
                ('reporteria-gerencial', 'Reportería Gerencial (SIGA)', 23, true);

                INSERT INTO seguridad.rol_menu (id_rol, id_menu, activo)
                SELECT r.id, m.id, true
                FROM seguridad.rol r, seguridad.menu m
                WHERE r.nombre = 'ADMINISTRADOR' AND m.codigo = 'reporteria-gerencial';

                INSERT INTO seguridad.dataset_reporteria (codigo, nombre, activo) VALUES
                ('cartera', 'Cartera de Crédito', true),
                ('ahorros', 'Captaciones — Ahorros', true),
                ('inversion', 'Captaciones — Depósitos a Plazo Fijo', true),
                ('contabilidad', 'Contabilidad — Balance de Comprobación', true),
                ('socios', 'Socios', true),
                ('solicitudes', 'Solicitudes de Crédito', true),
                ('vinculados', 'Créditos Vinculados', true),
                ('nominal', 'Cartera Nominal (datos personales de socios — requiere aprobación)', true);

                INSERT INTO seguridad.rol_dataset_reporteria (id_rol, codigo_dataset, activo)
                SELECT r.id, d.codigo, true
                FROM seguridad.rol r, seguridad.dataset_reporteria d
                WHERE r.nombre = 'ADMINISTRADOR' AND d.codigo <> 'nominal';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM seguridad.rol_dataset_reporteria;
                DELETE FROM seguridad.dataset_reporteria;
                DELETE FROM seguridad.rol_menu WHERE id_menu = (SELECT id FROM seguridad.menu WHERE codigo = 'reporteria-gerencial');
                DELETE FROM seguridad.menu WHERE codigo = 'reporteria-gerencial';
                """);

            migrationBuilder.DropTable(
                name: "rol_dataset_reporteria",
                schema: "seguridad");

            migrationBuilder.DropTable(
                name: "dataset_reporteria",
                schema: "seguridad");
        }
    }
}
