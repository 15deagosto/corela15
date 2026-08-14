using Corela15.Application.Seguridad;
using Corela15.Infrastructure.Persistence;
using Corela15.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Corela15.Tests;

public class AuthServiceTests : IAsyncLifetime
{
    private Corela15DbContext _db = null!;
    private AuthService _service = null!;
    private readonly List<long> _accionesCreadas = [];

    public Task InitializeAsync()
    {
        _db = DbFixture.CrearContexto();
        var configuracion = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "clave-de-prueba-solo-para-tests-nunca-en-produccion-1234567890",
                ["Jwt:Issuer"] = "Corela15Tests",
                ["Jwt:Audience"] = "Corela15TestsApi",
                ["Jwt:ExpiryMinutes"] = "60",
            })
            .Build();
        _service = new AuthService(_db, configuracion);
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        if (_accionesCreadas.Count > 0)
        {
            await _db.AccionesIngresoUsuario.Where(a => _accionesCreadas.Contains(a.Id)).ExecuteDeleteAsync();
        }
        await _db.DisposeAsync();
    }

    [Fact]
    public async Task LoginAsync_ConCredencialesCorrectas_EmiteTokenConRolesYMenus()
    {
        var resultado = await _service.LoginAsync(new LoginRequest("admin", "Corela15!Dev"), "127.0.0.1");

        Assert.False(string.IsNullOrWhiteSpace(resultado.Token));
        Assert.Contains("ADMINISTRADOR", resultado.Roles);
        Assert.NotEmpty(resultado.Menus);

        var idAccion = await _db.AccionesIngresoUsuario
            .Where(a => a.IdUsuario == resultado.IdUsuario && a.Exitoso)
            .OrderByDescending(a => a.FechaHora)
            .Select(a => a.Id)
            .FirstAsync();
        _accionesCreadas.Add(idAccion);
    }

    [Fact]
    public async Task LoginAsync_ConContrasenaIncorrecta_LanzaExcepcionYAuditaElIntento()
    {
        await Assert.ThrowsAsync<CredencialesInvalidasException>(() =>
            _service.LoginAsync(new LoginRequest("admin", "contraseña-incorrecta"), "127.0.0.1"));

        var idUsuario = await _db.Usuarios.Where(u => u.NombreUsuario == "admin").Select(u => u.Id).FirstAsync();
        var idAccion = await _db.AccionesIngresoUsuario
            .Where(a => a.IdUsuario == idUsuario && !a.Exitoso)
            .OrderByDescending(a => a.FechaHora)
            .Select(a => a.Id)
            .FirstAsync();
        _accionesCreadas.Add(idAccion);
    }

    [Fact]
    public async Task LoginAsync_ConUsuarioInexistente_LanzaCredencialesInvalidas()
    {
        await Assert.ThrowsAsync<CredencialesInvalidasException>(() =>
            _service.LoginAsync(new LoginRequest("usuario-que-no-existe", "cualquiera"), "127.0.0.1"));
    }
}
