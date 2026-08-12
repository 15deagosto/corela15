using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Nivel0_SeedCatalogosGenerales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Catálogos y datos base sin los que ningún módulo puede operar
            // (quedaron sin sembrar en Nivel0_Cimientos). El RUC de empresa es
            // un placeholder de desarrollo — actualizar con el RUC real antes
            // de cualquier ambiente que no sea local.
            migrationBuilder.Sql(
                """
                INSERT INTO general.moneda (codigo, nombre, simbolo) VALUES
                    ('USD', 'Dólar de los Estados Unidos', '$');

                INSERT INTO general.pais (codigo, nombre) VALUES
                    ('EC', 'Ecuador');

                INSERT INTO general.tipo_identificacion (codigo, nombre) VALUES
                    ('CED', 'Cédula'),
                    ('RUC', 'RUC'),
                    ('PAS', 'Pasaporte');

                INSERT INTO general.empresa (codigo, nombre, ruc, id_moneda, id_pais)
                SELECT '001', 'Cooperativa de Ahorro y Crédito 15 de Agosto de Pilacoto',
                       '0000000000001', m.id, p.id
                FROM general.moneda m, general.pais p
                WHERE m.codigo = 'USD' AND p.codigo = 'EC';

                INSERT INTO general.agencia (id_empresa, codigo, nombre, es_operativa, activa)
                SELECT e.id, '001', 'Matriz', true, true
                FROM general.empresa e
                WHERE e.codigo = '001';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM general.agencia WHERE codigo = '001';
                DELETE FROM general.empresa WHERE codigo = '001';
                DELETE FROM general.tipo_identificacion WHERE codigo IN ('CED', 'RUC', 'PAS');
                DELETE FROM general.pais WHERE codigo = 'EC';
                DELETE FROM general.moneda WHERE codigo = 'USD';
                """);
        }
    }
}
