using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Seguridad_OpcionesComunicacionInterna : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Tercer nivel de permiso (mismo patrón ya usado en Créditos/
            // Biblioteca de Documentos) aplicado a Comunicación Interna --
            // pedido explícito del usuario: un usuario puede tener el menú
            // (ve y participa en los canales a los que ya lo agregaron,
            // envía mensajes ahí) sin poder iniciar conversaciones nuevas
            // por su cuenta (crear canal, mensaje directo a cualquiera,
            // buscar personas, descubrir canales públicos) -- el caso real
            // es el usuario de Cajas, que solo debe interactuar en el canal
            // que le agreguen, nunca escribirle a cualquiera.
            migrationBuilder.Sql("""
                INSERT INTO seguridad.opcion (codigo, nombre, codigo_menu, activo) VALUES
                ('comunicacion-interna.iniciar-conversaciones', 'Iniciar conversaciones nuevas (crear canal, mensaje directo, buscar personas)', 'comunicacion-interna', true);

                -- Otorgada por defecto a los roles reales que ya tenían el
                -- menú universal de Comunicación Interna (ver
                -- Seguridad_ComunicacionInternaPorPermiso), EXCEPTO Cajero
                -- -- ese es el caso real que motivó este permiso: la
                -- cajera participa donde la agreguen, nunca inicia.
                INSERT INTO seguridad.rol_opcion (id_rol, codigo_opcion, activo)
                SELECT r.id, 'comunicacion-interna.iniciar-conversaciones', true
                FROM seguridad.rol r
                WHERE r.nombre IN ('ADMINISTRADOR', 'ASESOR DE CREDITO', 'OFICIAL DE CAPTACIONES', 'JEFATURA DE TI');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM seguridad.rol_opcion WHERE codigo_opcion = 'comunicacion-interna.iniciar-conversaciones';
                DELETE FROM seguridad.usuario_opcion WHERE codigo_opcion = 'comunicacion-interna.iniciar-conversaciones';
                DELETE FROM seguridad.opcion WHERE codigo_menu = 'comunicacion-interna';
                """);
        }
    }
}
