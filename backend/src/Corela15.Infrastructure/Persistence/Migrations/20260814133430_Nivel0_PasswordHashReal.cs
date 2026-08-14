using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corela15.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Nivel0_PasswordHashReal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Reemplaza el placeholder literal 'placeholder:sin-auth-todavia'
            // (Nivel0_SeedDatosPrueba) por un hash BCrypt real — contraseña
            // de desarrollo documentada en CLAUDE.md, NUNCA usar en un
            // ambiente real. A partir de acá el login (POST /api/auth/login)
            // valida contra este hash de verdad, no contra el placeholder.
            var hash = BCrypt.Net.BCrypt.HashPassword("Corela15!Dev");
            migrationBuilder.Sql(
                $"""
                UPDATE seguridad.usuario SET hash_contrasena = '{hash}'
                WHERE nombre_usuario IN ('admin', 'mguaman');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE seguridad.usuario SET hash_contrasena = 'placeholder:sin-auth-todavia'
                WHERE nombre_usuario IN ('admin', 'mguaman');
                """);
        }
    }
}
