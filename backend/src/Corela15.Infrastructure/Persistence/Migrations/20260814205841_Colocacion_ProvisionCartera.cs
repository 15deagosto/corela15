using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Colocacion_ProvisionCartera : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categoria_riesgo_cartera",
                schema: "colocacion",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    dias_mora_inicio = table.Column<int>(type: "integer", nullable: false),
                    dias_mora_fin = table.Column<int>(type: "integer", nullable: false),
                    porcentaje_provision = table.Column<decimal>(type: "numeric(9,4)", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categoria_riesgo_cartera", x => x.id);
                    table.CheckConstraint("ck_categoria_riesgo_cartera_rango", "dias_mora_fin >= dias_mora_inicio");
                });

            migrationBuilder.CreateIndex(
                name: "ix_categoria_riesgo_cartera_codigo",
                schema: "colocacion",
                table: "categoria_riesgo_cartera",
                column: "codigo",
                unique: true);

            // Matriz de calificación de riesgo real (Norma para la Gestión
            // del Riesgo de Crédito en las COAC, Art. 44) — 9 categorías,
            // % de provisión verificado (piso de cada rango oficial:
            // A1 1-1.99%, A2 2-2.99%, A3 3-5.99%, B1 6-9.99%, B2 10-19.99%,
            // C1 20-39.99%, C2 40-59.99%, D 60-99.99%, E 100%). Rangos de
            // días de mora: convención estándar SEPS/Superbancos para
            // Consumo/Microcrédito — pendiente verificación final contra
            // el Manual Técnico de Operaciones de Cartera oficial (el PDF
            // no se pudo extraer programáticamente al momento de esta
            // migración), documentado explícitamente en CLAUDE.md.
            migrationBuilder.Sql(
                """
                INSERT INTO colocacion.categoria_riesgo_cartera (codigo, nombre, dias_mora_inicio, dias_mora_fin, porcentaje_provision, activo) VALUES
                    ('A1', 'Riesgo normal A1', 0, 0, 0.0100, true),
                    ('A2', 'Riesgo normal A2', 1, 8, 0.0200, true),
                    ('A3', 'Riesgo normal A3', 9, 15, 0.0300, true),
                    ('B1', 'Riesgo potencial B1', 16, 30, 0.0600, true),
                    ('B2', 'Riesgo potencial B2', 31, 45, 0.1000, true),
                    ('C1', 'Deficiente C1', 46, 70, 0.2000, true),
                    ('C2', 'Deficiente C2', 71, 90, 0.4000, true),
                    ('D', 'Dudoso recaudo', 91, 120, 0.6000, true),
                    ('E', 'Pérdida', 121, 999999, 1.0000, true);
                """);

            // Cuentas reales para el asiento de provisión: 1499 (contra-activo,
            // reduce la cartera en el balance, naturaleza Acreedora aunque
            // vive bajo el grupo Activo — igual que en el CUC real) y 4402
            // (gasto, bajo el grupo 44 Provisiones ya sembrado en Nivel1).
            migrationBuilder.Sql(
                """
                INSERT INTO contabilidad.cuenta_contable (id, codigo, nombre, grupo, naturaleza, id_cuenta_padre, es_mayor, activa, creado_en, creado_por)
                SELECT gen_random_uuid(), '1499', 'Provisión para créditos incobrables', 'Activo', 'Acreedora', p.id, true, true, now(), 'seed:migracion'
                FROM contabilidad.cuenta_contable p WHERE p.codigo = '14';

                INSERT INTO contabilidad.cuenta_contable (id, codigo, nombre, grupo, naturaleza, id_cuenta_padre, es_mayor, activa, creado_en, creado_por)
                SELECT gen_random_uuid(), '4402', 'Provisión para cartera de crédito', 'Gastos', 'Deudora', p.id, true, true, now(), 'seed:migracion'
                FROM contabilidad.cuenta_contable p WHERE p.codigo = '44';

                INSERT INTO contabilidad.tipo_transaccion
                    (codigo, nombre, id_cuenta_contable_debito, id_cuenta_contable_credito, id_tipo_comprobante, signo_saldo_cuenta, activo)
                SELECT 'PROV-CART', 'Constitución de provisión de cartera', gasto.id, prov.id, tc.id, 1, true
                FROM contabilidad.cuenta_contable gasto, contabilidad.cuenta_contable prov, contabilidad.tipo_comprobante_contable tc
                WHERE gasto.codigo = '4402' AND prov.codigo = '1499' AND tc.codigo = 'DIA';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM contabilidad.tipo_transaccion WHERE codigo = 'PROV-CART';
                DELETE FROM contabilidad.cuenta_contable WHERE codigo IN ('1499', '4402');
                """);

            migrationBuilder.DropTable(
                name: "categoria_riesgo_cartera",
                schema: "colocacion");
        }
    }
}
