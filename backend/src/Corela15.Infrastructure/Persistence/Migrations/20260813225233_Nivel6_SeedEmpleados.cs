using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Nivel6_SeedEmpleados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Datos de EJEMPLO para desarrollo local (nombres/cédulas ficticios) —
            // empleados no son socios, así que no reutilizan las personas de
            // Nivel0_SeedDatosPrueba. Verificado contra Softbank que NOMINA.CARGO
            // no guarda un sueldo base ni existe otra tabla de sueldo por cargo —
            // el ingreso se digita por período en ROLPAGOS_EMPLEADO.INGRESOS
            // directamente, no se deriva de un campo fijo. Por eso Empleado no
            // tiene SueldoBase acá tampoco: sería un campo inventado que no existe
            // en el sistema real.
            migrationBuilder.Sql(
                """
                WITH tipo_ced AS (SELECT id FROM general.tipo_identificacion WHERE codigo = 'CED'),
                     personas AS (
                        INSERT INTO sujeto.persona
                            (id, identificacion, id_tipo_identificacion, nombre, email, id_pais, creado_en, creado_por)
                        SELECT gen_random_uuid(), d.identificacion, tipo_ced.id, d.nombre,
                               d.email, (SELECT id FROM general.pais WHERE codigo = 'EC'), now(), 'seed:migracion'
                        FROM (VALUES
                            ('1806789012', 'PATRICIA ELENA SANTANA MOLINA', 'psantana@ejemplo.test'),
                            ('1807890123', 'DIEGO FERNANDO ROBALINO CASTRO', 'drobalino@ejemplo.test')
                        ) AS d(identificacion, nombre, email), tipo_ced
                        RETURNING id, identificacion, nombre
                     ),
                     pn AS (
                        INSERT INTO sujeto.persona_natural
                            (id_persona, primer_nombre, apellido_paterno, fecha_nacimiento, es_masculino, es_pep)
                        SELECT p.id, split_part(p.nombre, ' ', 1), split_part(p.nombre, ' ', 3),
                               DATE '1988-01-01', p.identificacion = '1807890123', false
                        FROM personas p
                        RETURNING id_persona
                     )
                INSERT INTO nomina.empleado
                    (id, id_persona, id_agencia, cargo, fecha_ingreso, recibe_fondos_reserva, estado)
                SELECT gen_random_uuid(), p.id, (SELECT id FROM general.agencia WHERE codigo = '001'),
                       CASE p.identificacion WHEN '1806789012' THEN 'ASESORA DE CAPTACIONES' ELSE 'CAJERO' END,
                       DATE '2023-03-01', true, 'Activo'
                FROM personas p;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM nomina.empleado WHERE id_persona IN (
                    SELECT id FROM sujeto.persona WHERE identificacion IN ('1806789012', '1807890123'));
                DELETE FROM sujeto.persona WHERE identificacion IN ('1806789012', '1807890123');
                """);
        }
    }
}
