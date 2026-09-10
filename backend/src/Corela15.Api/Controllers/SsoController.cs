using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Corela15.Api.Controllers;

/// <summary>
/// Puente de sesión real hacia apps externas propias (hoy solo CredVault --
/// gestor de credenciales de TI, Django/React, base y despliegue separados)
/// sin compartir contraseñas entre sistemas: un ticket corto, de un solo
/// uso, firmado con un secreto que solo conocen los dos backends. Nunca
/// asume que la persona ya tiene cuenta del otro lado -- esa decisión
/// (crear o no la cuenta de CredVault de alguien) sigue siendo manual, del
/// Jefe de TI, la validación real vive en CredVault, no acá.
/// </summary>
[ApiController]
[Route("api/sso")]
public class SsoController(IConfiguration configuration) : ControllerBase
{
    /// <summary>
    /// Mismo criterio de menú que ya gatea el ícono en el sidebar
    /// (Menu:credvault) -- nadie sin ese permiso puede pedir un ticket,
    /// aunque conozca la URL del endpoint.
    /// </summary>
    [HttpPost("credvault/ticket")]
    [Authorize(Policy = "Menu:credvault")]
    public ActionResult<object> TicketCredVault()
    {
        var secret = configuration["CredVault:SsoSecret"]
            ?? throw new InvalidOperationException("Falta CredVault__SsoSecret (revisar .env.core/.env.prod).");
        var nombreUsuario = User.Identity?.Name
            ?? throw new InvalidOperationException("Token sin nombre de usuario.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Vida muy corta a propósito (60s) -- alcanza de sobra para el
        // salto de pestaña real, y minimiza la ventana de un ticket
        // interceptado. CredVault además lo rechaza si ya se usó una vez
        // (ver SsoTicketUsado del lado de CredVault), no solo por tiempo.
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, nombreUsuario),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        var token = new JwtSecurityToken(
            issuer: "corela15",
            audience: "credvault",
            claims: claims,
            expires: DateTime.UtcNow.AddSeconds(60),
            signingCredentials: credenciales);

        return Ok(new { ticket = new JwtSecurityTokenHandler().WriteToken(token) });
    }
}
