using Corela15.Application.Colocacion;
using Corela15.Infrastructure.Persistence;
using Corela15.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Tests;

public class TipoPrestamoAdminServiceTests : IAsyncLifetime
{
    private Corela15DbContext _db = null!;
    private TipoPrestamoAdminService _service = null!;
    private readonly List<string> _codigosCreados = [];

    public Task InitializeAsync()
    {
        _db = DbFixture.CrearContexto();
        _service = new TipoPrestamoAdminService(_db);
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        if (_codigosCreados.Count > 0)
        {
            await _db.TiposPrestamo.Where(t => _codigosCreados.Contains(t.Codigo)).ExecuteDeleteAsync();
        }
        await _db.DisposeAsync();
    }

    [Fact]
    public async Task CrearAsync_ConTasaDentroDelTechoBce_CreaElProducto()
    {
        var codigo = $"TST-{Guid.NewGuid():N}"[..15];
        _codigosCreados.Add(codigo);

        var resultado = await _service.CrearAsync(new CrearTipoPrestamoRequest(
            codigo, "Producto de test", 100, 5000, 30, 360, 0.10m, "Consumo Prioritario"));

        Assert.Equal(codigo, resultado.Codigo);
    }

    [Fact]
    public async Task CrearAsync_ConTasaQueExcedeElTechoBce_LanzaExcepcion()
    {
        var codigo = $"TST-{Guid.NewGuid():N}"[..15];

        await Assert.ThrowsAsync<TasaExcedeTechoBceException>(() =>
            _service.CrearAsync(new CrearTipoPrestamoRequest(
                codigo, "Producto excedido", 100, 5000, 30, 360, 0.99m, "Consumo Prioritario")));
    }

    [Fact]
    public async Task CrearAsync_ConSegmentoBceInexistente_LanzaExcepcion()
    {
        var codigo = $"TST-{Guid.NewGuid():N}"[..15];

        await Assert.ThrowsAsync<SegmentoBceInvalidoException>(() =>
            _service.CrearAsync(new CrearTipoPrestamoRequest(
                codigo, "Segmento inválido", 100, 5000, 30, 360, 0.10m, "Segmento Que No Existe")));
    }

    [Fact]
    public async Task CrearAsync_ConCodigoDuplicado_LanzaExcepcion()
    {
        var codigoExistente = await _db.TiposPrestamo.Select(t => t.Codigo).FirstAsync();

        await Assert.ThrowsAsync<Corela15.Application.Common.CodigoDuplicadoException>(() =>
            _service.CrearAsync(new CrearTipoPrestamoRequest(
                codigoExistente, "Duplicado", 100, 5000, 30, 360, 0.10m, "Consumo Prioritario")));
    }
}
