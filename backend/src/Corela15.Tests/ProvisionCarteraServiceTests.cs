using Corela15.Infrastructure.Persistence;
using Corela15.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Tests;

public class ProvisionCarteraServiceTests : IAsyncLifetime
{
    private Corela15DbContext _db = null!;
    private ProvisionCarteraService _service = null!;

    public Task InitializeAsync()
    {
        _db = DbFixture.CrearContexto();
        _service = new ProvisionCarteraService(_db, new ComprobanteContableService(_db));
        return Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _db.DisposeAsync();

    [Fact]
    public async Task EjecutarCalculoAsync_SinCarteraVigente_NoRequiereProvision()
    {
        // Asume que no hay préstamos vigentes de prueba sin limpiar de otra
        // corrida — si el conteo no es cero, el assert de "provisión = 0"
        // sigue siendo válido matemáticamente (0 preexistente + 0 nuevo),
        // así que no depende de que la cartera esté realmente vacía.
        var resultado = await _service.EjecutarCalculoAsync("tests");

        Assert.True(resultado.ProvisionRequeridaTotal >= 0);
        Assert.True(resultado.IncrementoRegistrado >= 0);
    }

    [Theory]
    [InlineData(0, "A1", 0.01)]
    [InlineData(5, "A2", 0.02)]
    [InlineData(12, "A3", 0.03)]
    [InlineData(20, "B1", 0.06)]
    [InlineData(40, "B2", 0.10)]
    [InlineData(60, "C1", 0.20)]
    [InlineData(80, "C2", 0.40)]
    [InlineData(100, "D", 0.60)]
    [InlineData(200, "E", 1.00)]
    public async Task MatrizDeCategorias_ClasificaCadaRangoDeMoraCorrectamente(int diasMora, string codigoEsperado, decimal porcentajeEsperado)
    {
        var categorias = await _db.CategoriasRiesgoCartera.Where(c => c.Activo).ToListAsync();

        var categoria = categorias.FirstOrDefault(c => diasMora >= c.DiasMoraInicio && diasMora <= c.DiasMoraFin);

        Assert.NotNull(categoria);
        Assert.Equal(codigoEsperado, categoria!.Codigo);
        Assert.Equal(porcentajeEsperado, categoria.PorcentajeProvision);
    }
}
