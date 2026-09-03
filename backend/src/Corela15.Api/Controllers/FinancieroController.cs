using Corela15.Api.Idempotencia;
using Corela15.Application.Financiero;
using Corela15.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Api.Controllers;

public record BancoListItem(int Id, string Nombre, bool EsNacional);

public record CuentaParaChequeListItem(Guid Id, string Numero, string Socio);

public record ChequeListItem(
    Guid Id, string Banco, string CuentaCorriente, string NumeroCheque, decimal Valor,
    string CuentaDestino, DateOnly FechaIngreso, string Estado);

[ApiController]
[Route("api/financiero")]
[Authorize(Policy = "Menu:financiero")]
public class FinancieroController(Corela15DbContext db, IChequeService chequeService) : ControllerBase
{
    [HttpGet("bancos")]
    public async Task<ActionResult<IReadOnlyList<BancoListItem>>> Bancos(CancellationToken ct)
    {
        var resultado = await db.Bancos
            .Where(b => b.Activo)
            .OrderBy(b => b.Nombre)
            .Select(b => new BancoListItem(b.Id, b.Nombre, b.EsNacional))
            .ToListAsync(ct);
        return Ok(resultado);
    }

    [HttpGet("cuentas")]
    public async Task<ActionResult<IReadOnlyList<CuentaParaChequeListItem>>> CuentasParaDeposito(CancellationToken ct)
    {
        var resultado = await db.Cuentas
            .Where(c => c.Estado == Corela15.Domain.Ahorros.EstadoCuenta.Activa)
            .OrderBy(c => c.Numero)
            .Select(c => new CuentaParaChequeListItem(
                c.Id, c.Numero,
                db.CuentasClientes.Where(cc => cc.IdCuenta == c.Id && cc.Principal)
                    .Select(cc => cc.Cliente.Persona.Nombre).FirstOrDefault() ?? "—"))
            .ToListAsync(ct);
        return Ok(resultado);
    }

    [HttpGet("cheques")]
    public async Task<ActionResult<IReadOnlyList<ChequeListItem>>> Cheques(CancellationToken ct)
    {
        var resultado = await db.Cheques
            .Include(c => c.Banco).Include(c => c.Cuenta)
            .OrderByDescending(c => c.FechaIngreso)
            .Select(c => new ChequeListItem(
                c.Id, c.Banco.Nombre, c.CuentaCorriente, c.NumeroCheque, c.Valor,
                c.Cuenta.Numero, c.FechaIngreso, c.Estado.ToString()))
            .ToListAsync(ct);
        return Ok(resultado);
    }

    [HttpPost("cheques")]
    public async Task<ActionResult<ChequeRegistradoResult>> Registrar([FromBody] RegistrarChequeBody body, CancellationToken ct)
    {
        var resultado = await chequeService.RegistrarAsync(
            new RegistrarChequeRequest(
                body.IdBanco, body.CuentaCorriente, body.NumeroCheque, body.Valor, body.IdCuenta, body.IdAgencia, User.Identity!.Name!),
            ct);
        return Created($"/api/financiero/cheques/{resultado.IdCheque}", resultado);
    }

    [HttpPost("cheques/{id:guid}/depositar")]
    [RequireIdempotencyKey]
    public async Task<ActionResult<ChequeDepositadoResult>> Depositar(Guid id, CancellationToken ct)
    {
        var resultado = await chequeService.DepositarAsync(new DepositarChequeRequest(id, User.Identity!.Name!), ct);
        return Ok(resultado);
    }

    [HttpPost("cheques/{id:guid}/efectivizar")]
    [RequireIdempotencyKey]
    public async Task<ActionResult<ChequeEfectivizadoResult>> Efectivizar(Guid id, CancellationToken ct)
    {
        var resultado = await chequeService.EfectivizarAsync(new EfectivizarChequeRequest(id, User.Identity!.Name!), ct);
        return Ok(resultado);
    }

    [HttpPost("cheques/{id:guid}/protestar")]
    [RequireIdempotencyKey]
    public async Task<ActionResult<ChequeProtestadoResult>> Protestar(Guid id, [FromBody] ProtestarChequeBody body, CancellationToken ct)
    {
        var resultado = await chequeService.ProtestarAsync(new ProtestarChequeRequest(id, body.Documento, User.Identity!.Name!), ct);
        return Ok(resultado);
    }
}

public record RegistrarChequeBody(int IdBanco, string CuentaCorriente, string NumeroCheque, decimal Valor, Guid IdCuenta, int IdAgencia);

public record ProtestarChequeBody(string? Documento);
