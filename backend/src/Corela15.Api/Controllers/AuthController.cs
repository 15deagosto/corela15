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

    [HttpGet("me")]
    public ActionResult<SesionActualResult> Me()
    {
        var idUsuario = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
        var nombreUsuario = User.Identity!.Name!;
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        var menus = User.FindAll("menu").Select(c => c.Value).ToList();

        return Ok(new SesionActualResult(idUsuario, nombreUsuario, roles, menus));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var jti = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Jti)!);
        await authService.LogoutAsync(jti, User.Identity!.Name!, cancellationToken);
        return NoContent();
    }
}
