using Corela15.Application.Contabilidad;
using Corela15.Infrastructure.Persistence;
using Corela15.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Tests;

public class CuentaContableAdminServiceTests : IAsyncLifetime
{
    private Corela15DbContext _db = null!;
    private CuentaContableAdminService _service = null!;
    private readonly List<Guid> _cuentasCreadas = [];

    public Task InitializeAsync()
    {
        _db = DbFixture.CrearContexto();
        _service = new CuentaContableAdminService(_db);
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        if (_cuentasCreadas.Count > 0)
        {
            await _db.Database.ExecuteSqlRawAsync(
                "DELETE FROM contabilidad.cuenta_contable_historico WHERE id = ANY({0})", _cuentasCreadas.ToArray());
            await _db.CuentasContables.Where(c => _cuentasCreadas.Contains(c.Id)).ExecuteDeleteAsync();
        }
        await _db.DisposeAsync();
    }

    [Fact]
    public async Task ActualizarAsync_DesactivarCajaGeneral_SiempreSeRechaza()
    {
        // 1101 Caja siempre debe rechazar la desactivación en un ambiente
        // con actividad real: o tiene saldo distinto de cero (verificado
        // primero) o está en uso por un tipo de transacción activo — cuál
        // de las dos dispare depende del estado real de los datos, no
        // hace falta acoplar el test a un balance exacto.
        var idCaja = await _db.CuentasContables.Where(c => c.Codigo == "1101").Select(c => c.Id).FirstAsync();

        var excepcion = await Assert.ThrowsAnyAsync<Corela15.Application.Common.ReglaDeNegocioException>(() =>
            _service.ActualizarAsync(new ActualizarCuentaContableRequest(idCaja, "Caja General", false, "tests")));

        Assert.True(
            excepcion is CuentaContableConSaldoException or CuentaContableEnUsoException,
            $"Se esperaba CuentaContableConSaldoException o CuentaContableEnUsoException, se obtuvo {excepcion.GetType().Name}");
    }

    [Fact]
    public async Task CrearAsync_ConCuentaPadreDeDetalle_LanzaExcepcion()
    {
        var idCuentaDetalle = await _db.CuentasContables.Where(c => c.Codigo == "1101").Select(c => c.Id).FirstAsync();

        await Assert.ThrowsAsync<CuentaContablePadreInvalidaException>(() =>
            _service.CrearAsync(new CrearCuentaContableRequest(
                $"T{Guid.NewGuid():N}"[..10], "Cuenta inválida", "Activo", "Deudora", idCuentaDetalle, true, "tests")));
    }

    [Fact]
    public async Task CrearAsync_ConCodigoValido_CreaLaCuentaYPermiteDesactivarlaSinSaldo()
    {
        var idGrupo56 = await _db.CuentasContables.Where(c => c.Codigo == "56").Select(c => c.Id).FirstAsync();
        var codigo = $"T{Guid.NewGuid():N}"[..10];

        var resultado = await _service.CrearAsync(new CrearCuentaContableRequest(
            codigo, "Cuenta de test", "Ingresos", "Acreedora", idGrupo56, true, "tests"));
        _cuentasCreadas.Add(resultado.Id);

        // Sin saldo ni uso — debe poder desactivarse sin lanzar excepción.
        await _service.ActualizarAsync(new ActualizarCuentaContableRequest(resultado.Id, "Cuenta de test", false, "tests"));

        var cuenta = await _db.CuentasContables.FirstAsync(c => c.Id == resultado.Id);
        Assert.False(cuenta.Activa);
    }
}
