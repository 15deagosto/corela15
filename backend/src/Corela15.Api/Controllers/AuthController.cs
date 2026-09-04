using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Corela15.Application.Seguridad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Corela15.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResult>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var direccionIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        var resultado = await authService.LoginAsync(request, direccionIp, cancellationToken);
        return Ok(resultado);
    }

    /// <summary>
    /// Recalcula los permisos del usuario actual y emite un token nuevo,
    /// sin pedir contraseña — el frontend lo llama al cargar la app para
    /// que un cambio de rol/permisos hecho por un admin mientras el
    /// usuario ya tenía la sesión abierta se refleje solo con recargar la
    /// página, sin obligarlo a cerrar sesión y volver a entrar.
    /// </summary>
    [HttpPost("refrescar")]
    public async Task<ActionResult<LoginResult>> Refrescar(CancellationToken cancellationToken)
    {
        var idUsuario = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
        var jti = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Jti)!);
        var direccionIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        var resultado = await authService.RefrescarAsync(idUsuario, jti, direccionIp, cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("me")]
    public ActionResult<SesionActualResult> Me()
    {
        var idUsuario = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
        var nombreUsuario = User.Identity!.Name!;
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        var menus = User.FindAll("menu").Select(c => c.Value).ToList();
        var estructuras = User.FindAll("estructura").Select(c => c.Value).ToList();
        var idAgenciaEfectiva = int.Parse(User.FindFirstValue("agencia") ?? "0");

        return Ok(new SesionActualResult(idUsuario, nombreUsuario, roles, menus, estructuras, idAgenciaEfectiva));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var jti = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Jti)!);
        await authService.LogoutAsync(jti, User.Identity!.Name!, cancellationToken);
        return NoContent();
    }

    [HttpPost("cambiar-clave")]
    public async Task<IActionResult> CambiarClave([FromBody] CambiarClaveRequest request, CancellationToken cancellationToken)
    {
        var idUsuario = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
        var jti = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Jti)!);
        await authService.CambiarClaveAsync(idUsuario, jti, request, cancellationToken);
        return NoContent();
    }
}
