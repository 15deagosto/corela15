using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Corela15.Application.Seguridad;
using Corela15.Domain.Seguridad;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Corela15.Infrastructure.Services;

public class AuthService(Corela15DbContext db, IConfiguration configuration) : IAuthService
{
    public async Task<LoginResult> LoginAsync(
        LoginRequest request, string? direccionIp, CancellationToken cancellationToken = default)
    {
        var usuario = await db.Usuarios
            .FirstOrDefaultAsync(u => u.NombreUsuario == request.NombreUsuario, cancellationToken);

        if (usuario is null || !BCrypt.Net.BCrypt.Verify(request.Contrasena, usuario.HashContrasena))
        {
            await RegistrarIntentoAsync(usuario?.Id, exitoso: false, direccionIp, "Credenciales inválidas", cancellationToken);
            throw new CredencialesInvalidasException();
        }

        if (!usuario.Activo || !usuario.PuedeIngresarSistema || usuario.TieneBloqueo)
        {
            await RegistrarIntentoAsync(usuario.Id, exitoso: false, direccionIp, "Usuario no habilitado", cancellationToken);
            throw new UsuarioNoHabilitadoException();
        }

        var roles = await db.UsuarioRoles
            .Where(ur => ur.IdUsuario == usuario.Id && ur.Activo)
            .Select(ur => ur.Rol.Nombre)
            .ToListAsync(cancellationToken);

        var menus = await db.RolesMenu
            .Where(rm => rm.Activo && rm.Menu.Activo
                && db.UsuarioRoles.Any(ur => ur.IdUsuario == usuario.Id && ur.Activo && ur.IdRol == rm.IdRol))
            .Select(rm => rm.Menu.Codigo)
            .Distinct()
            .ToListAsync(cancellationToken);

        await RegistrarIntentoAsync(usuario.Id, exitoso: true, direccionIp, null, cancellationToken);

        var (token, expiraEn) = GenerarToken(usuario, roles, menus);
        return new LoginResult(token, expiraEn, usuario.Id, usuario.NombreUsuario, roles, menus);
    }

    private async Task RegistrarIntentoAsync(
        Guid? idUsuario, bool exitoso, string? direccionIp, string? detalle, CancellationToken cancellationToken)
    {
        if (idUsuario is null) return;

        db.AccionesIngresoUsuario.Add(new AccionIngresoUsuario
        {
            IdUsuario = idUsuario.Value,
            FechaHora = DateTimeOffset.UtcNow,
            Exitoso = exitoso,
            DireccionIp = direccionIp,
            Detalle = detalle,
        });
        await db.SaveChangesAsync(cancellationToken);
    }

    private (string Token, DateTimeOffset ExpiraEn) GenerarToken(Usuario usuario, List<string> roles, List<string> menus)
    {
        var secret = configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("Falta Jwt:Secret (revisar .env.core)");
        var issuer = configuration["Jwt:Issuer"] ?? "Corela15";
        var audience = configuration["Jwt:Audience"] ?? "Corela15Api";
        var expiryMinutes = int.TryParse(configuration["Jwt:ExpiryMinutes"], out var m) ? m : 480;

        var expiraEn = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.NombreUsuario),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        claims.AddRange(menus.Select(m2 => new Claim("menu", m2)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiraEn.UtcDateTime,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiraEn);
    }
}
