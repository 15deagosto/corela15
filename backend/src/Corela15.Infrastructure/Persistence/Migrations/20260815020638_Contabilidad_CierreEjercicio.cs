using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Contabilidad_CierreEjercicio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cierre_ejercicio",
                schema: "contabilidad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    anio = table.Column<int>(type: "integer", nullable: false),
                    fecha_cierre = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    total_ingresos = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    total_gastos = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    utilidad = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    id_comprobante_contable = table.Column<Guid>(type: "uuid", nullable: false),
                    cerrado_por = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cierre_ejercicio", x => x.id);
                    table.ForeignKey(
                        name: "fk_cierre_ejercicio_comprobantes_contables_id_comprobante_cont",
                        column: x => x.id_comprobante_contable,
                        principalSchema: "contabilidad",
                        principalTable: "comprobante_contable",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_cierre_ejercicio_anio",
                schema: "contabilidad",
                table: "cierre_ejercicio",
                column: "anio",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cierre_ejercicio_id_comprobante_contable",
                schema: "contabilidad",
                table: "cierre_ejercicio",
                column: "id_comprobante_contable");

            // Subcuentas de detalle reales del grupo CUC 36 (Resultados),
            // necesarias para liquidar utilidad/pérdida del ejercicio contra
            // patrimonio — mismo criterio que Contabilidad_SeedCuentaInteresesGanados.
            migrationBuilder.Sql(
                """
                INSERT INTO contabilidad.cuenta_contable (id, codigo, nombre, grupo, naturaleza, id_cuenta_padre, es_mayor, activa, creado_en, creado_por)
                SELECT gen_random_uuid(), '3603', 'Utilidad del ejercicio', 'Patrimonio', 'Acreedora', p.id, true, true, now(), 'seed:migracion'
                FROM contabilidad.cuenta_contable p WHERE p.codigo = '36';

                INSERT INTO contabilidad.cuenta_contable (id, codigo, nombre, grupo, naturaleza, id_cuenta_padre, es_mayor, activa, creado_en, creado_por)
                SELECT gen_random_uuid(), '3604', '(Pérdida del ejercicio)', 'Patrimonio', 'Acreedora', p.id, true, true, now(), 'seed:migracion'
                FROM contabilidad.cuenta_contable p WHERE p.codigo = '36';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM contabilidad.cuenta_contable WHERE codigo IN ('3603', '3604');");

            migrationBuilder.DropTable(
                name: "cierre_ejercicio",
                schema: "contabilidad");
        }
    }
}
