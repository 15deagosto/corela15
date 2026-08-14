using Corela15.Application.Contabilidad;
using Corela15.Infrastructure.Persistence;
using Corela15.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Tests;

public class ComprobanteContableServiceTests : IAsyncLifetime
{
    private Corela15DbContext _db = null!;
    private ComprobanteContableService _service = null!;
    private Guid _idCuentaCaja;
    private Guid _idCuentaDepositos;
    private readonly List<Guid> _comprobantesCreados = [];

    public async Task InitializeAsync()
    {
        _db = DbFixture.CrearContexto();
        _service = new ComprobanteContableService(_db);

        _idCuentaCaja = await _db.CuentasContables.Where(c => c.Codigo == "1101").Select(c => c.Id).FirstAsync();
        _idCuentaDepositos = await _db.CuentasContables.Where(c => c.Codigo == "2101").Select(c => c.Id).FirstAsync();
    }

    public async Task DisposeAsync()
    {
        if (_comprobantesCreados.Count > 0)
        {
            await _db.MovimientosComprobanteContable.Where(m => _comprobantesCreados.Contains(m.IdComprobante)).ExecuteDeleteAsync();
            await _db.ComprobantesContables.Where(c => _comprobantesCreados.Contains(c.Id)).ExecuteDeleteAsync();
            await _db.SaldosContables
                .Where(s => (s.IdCuentaContable == _idCuentaCaja || s.IdCuentaContable == _idCuentaDepositos)
                    && s.Periodo == new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1))
                .ExecuteDeleteAsync();
        }
        await _db.DisposeAsync();
    }

    [Fact]
    public async Task RegistrarAsync_ConDebitosYCreditosBalanceados_RegistraElComprobante()
    {
        var request = new RegistrarComprobanteContableRequest(
            DateOnly.FromDateTime(DateTime.UtcNow), 3, 1, "Test unitario — balanceado", "tests",
            [
                new LineaMovimientoRequest(_idCuentaCaja, 25, 0, "débito"),
                new LineaMovimientoRequest(_idCuentaDepositos, 0, 25, "crédito"),
            ]);

        var resultado = await _service.RegistrarAsync(request);
        _comprobantesCreados.Add(resultado.Id);

        Assert.NotEqual(Guid.Empty, resultado.Id);
        Assert.True(resultado.Numero > 0);
    }

    [Fact]
    public async Task RegistrarAsync_ConDebitosYCreditosDesbalanceados_LanzaExcepcion()
    {
        var request = new RegistrarComprobanteContableRequest(
            DateOnly.FromDateTime(DateTime.UtcNow), 3, 1, "Test unitario — desbalanceado", "tests",
            [
                new LineaMovimientoRequest(_idCuentaCaja, 25, 0, "débito"),
                new LineaMovimientoRequest(_idCuentaDepositos, 0, 20, "crédito"),
            ]);

        await Assert.ThrowsAsync<ComprobanteDesbalanceadoException>(() => _service.RegistrarAsync(request));
    }

    [Fact]
    public async Task RegistrarAsync_ConMenosDeDosLineas_LanzaExcepcion()
    {
        var request = new RegistrarComprobanteContableRequest(
            DateOnly.FromDateTime(DateTime.UtcNow), 3, 1, "Test unitario — una línea", "tests",
            [new LineaMovimientoRequest(_idCuentaCaja, 25, 0, "débito")]);

        await Assert.ThrowsAsync<ComprobanteLineasInsuficientesException>(() => _service.RegistrarAsync(request));
    }

    [Fact]
    public async Task RegistrarAsync_ConCuentaInexistente_LanzaExcepcion()
    {
        var request = new RegistrarComprobanteContableRequest(
            DateOnly.FromDateTime(DateTime.UtcNow), 3, 1, "Test unitario — cuenta inválida", "tests",
            [
                new LineaMovimientoRequest(Guid.NewGuid(), 25, 0, "débito"),
                new LineaMovimientoRequest(_idCuentaDepositos, 0, 25, "crédito"),
            ]);

        await Assert.ThrowsAsync<CuentaContableInvalidaException>(() => _service.RegistrarAsync(request));
    }

    [Fact]
    public async Task RegistrarAsync_ActualizaElSaldoContableDeLasCuentasAfectadas()
    {
        var periodo = new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var saldoAntes = await _db.SaldosContables
            .Where(s => s.IdCuentaContable == _idCuentaCaja && s.Periodo == periodo)
            .Select(s => (decimal?)s.SaldoFinal)
            .FirstOrDefaultAsync() ?? 0m;

        var request = new RegistrarComprobanteContableRequest(
            DateOnly.FromDateTime(DateTime.UtcNow), 3, 1, "Test unitario — saldo", "tests",
            [
                new LineaMovimientoRequest(_idCuentaCaja, 100, 0, "débito"),
                new LineaMovimientoRequest(_idCuentaDepositos, 0, 100, "crédito"),
            ]);
        var resultado = await _service.RegistrarAsync(request);
        _comprobantesCreados.Add(resultado.Id);

        var saldoDespues = await _db.SaldosContables
            .Where(s => s.IdCuentaContable == _idCuentaCaja && s.Periodo == periodo)
            .Select(s => s.SaldoFinal)
            .FirstAsync();

        Assert.Equal(saldoAntes + 100, saldoDespues);
    }
}
