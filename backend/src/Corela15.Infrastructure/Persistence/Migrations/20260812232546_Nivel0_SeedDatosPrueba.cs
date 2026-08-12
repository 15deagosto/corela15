using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Nivel0_SeedDatosPrueba : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Datos de EJEMPLO para desarrollo local (nombres/cédulas ficticios,
            // no corresponden a socios reales de la cooperativa) — para que las
            // pantallas de Socios/Usuarios tengan contenido real que mostrar en
            // vez de tablas vacías. hash_contrasena es un placeholder: todavía
            // no existe flujo de autenticación real (login), ver CLAUDE.md.
            migrationBuilder.Sql(
                """
                INSERT INTO seguridad.rol (nombre, nivel) VALUES
                    ('ADMINISTRADOR', 100),
                    ('CAJERO', 20),
                    ('ASESOR DE CREDITO', 30),
                    ('OFICIAL DE CAPTACIONES', 30);

                WITH tipo_ced AS (SELECT id FROM general.tipo_identificacion WHERE codigo = 'CED'),
                     personas AS (
                        INSERT INTO sujeto.persona
                            (id, identificacion, id_tipo_identificacion, nombre, email, id_pais, creado_en, creado_por)
                        SELECT gen_random_uuid(), d.identificacion, tipo_ced.id, d.nombre,
                               d.email, (SELECT id FROM general.pais WHERE codigo = 'EC'), now(), 'seed:migracion'
                        FROM (VALUES
                            ('1801234567', 'MARIA FERNANDA GUAMAN TOAPANTA', 'mguaman@ejemplo.test'),
                            ('1802345678', 'CARLOS ANDRES CHICAIZA LEMA', 'cchicaiza@ejemplo.test'),
                            ('1803456789', 'LUCIA ELIZABETH TOAQUIZA RAMOS', 'ltoaquiza@ejemplo.test'),
                            ('1804567890', 'JOSE LUIS PILALUMBO GUANOLUISA', 'jpilalumbo@ejemplo.test'),
                            ('1805678901', 'ANA GABRIELA QUISHPE SANTANA', 'aquishpe@ejemplo.test')
                        ) AS d(identificacion, nombre, email), tipo_ced
                        RETURNING id, identificacion, nombre
                     )
                INSERT INTO sujeto.persona_natural
                    (id_persona, primer_nombre, apellido_paterno, fecha_nacimiento, es_masculino, es_pep)
                SELECT p.id,
                       split_part(p.nombre, ' ', 1),
                       split_part(p.nombre, ' ', 3),
                       DATE '1990-01-01',
                       p.identificacion IN ('1802345678', '1804567890'),
                       false
                FROM personas p;

                INSERT INTO clientes.cliente (id, numero, id_persona, id_agencia, estado, creado_en, creado_por)
                SELECT gen_random_uuid(), lpad((row_number() over (ORDER BY p.nombre))::text, 6, '0'),
                       p.id, (SELECT id FROM general.agencia WHERE codigo = '001'), 'Activo', now(), 'seed:migracion'
                FROM sujeto.persona p
                WHERE p.identificacion IN
                    ('1801234567','1802345678','1803456789','1804567890','1805678901');

                INSERT INTO seguridad.usuario
                    (id, nombre_usuario, hash_contrasena, id_persona, id_agencia, puede_ingresar_sistema,
                     tiene_bloqueo, activo, creado_en, creado_por)
                SELECT gen_random_uuid(), 'admin', 'placeholder:sin-auth-todavia', NULL,
                       (SELECT id FROM general.agencia WHERE codigo = '001'), true, false, true, now(), 'seed:migracion'
                UNION ALL
                SELECT gen_random_uuid(), 'mguaman', 'placeholder:sin-auth-todavia', p.id,
                       (SELECT id FROM general.agencia WHERE codigo = '001'), true, false, true, now(), 'seed:migracion'
                FROM sujeto.persona p WHERE p.identificacion = '1801234567';

                INSERT INTO seguridad.usuario_rol (id_usuario, id_rol, activo, asignado_en)
                SELECT u.id, r.id, true, now()
                FROM seguridad.usuario u, seguridad.rol r
                WHERE u.nombre_usuario = 'admin' AND r.nombre = 'ADMINISTRADOR';

                INSERT INTO seguridad.usuario_rol (id_usuario, id_rol, activo, asignado_en)
                SELECT u.id, r.id, true, now()
                FROM seguridad.usuario u, seguridad.rol r
                WHERE u.nombre_usuario = 'mguaman' AND r.nombre IN ('CAJERO', 'OFICIAL DE CAPTACIONES');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM seguridad.usuario_rol WHERE id_usuario IN (
                    SELECT id FROM seguridad.usuario WHERE nombre_usuario IN ('admin', 'mguaman'));
                DELETE FROM seguridad.usuario WHERE nombre_usuario IN ('admin', 'mguaman');
                DELETE FROM clientes.cliente WHERE id_persona IN (
                    SELECT id FROM sujeto.persona WHERE identificacion IN
                        ('1801234567','1802345678','1803456789','1804567890','1805678901'));
                DELETE FROM sujeto.persona_natural WHERE id_persona IN (
                    SELECT id FROM sujeto.persona WHERE identificacion IN
                        ('1801234567','1802345678','1803456789','1804567890','1805678901'));
                DELETE FROM sujeto.persona WHERE identificacion IN
                    ('1801234567','1802345678','1803456789','1804567890','1805678901');
                DELETE FROM seguridad.rol WHERE nombre IN
                    ('ADMINISTRADOR', 'CAJERO', 'ASESOR DE CREDITO', 'OFICIAL DE CAPTACIONES');
                """);
        }
    }
}
