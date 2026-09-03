using Corela15.Application.Obligacion;
using Corela15.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Api.Controllers;

public record RegistrarObligacionFinancieraBody(
    int IdAgencia,
    string TipoIdentificacionAcreedor,
    string IdentificacionAcreedor,
    string CodigoPaisAcreedor,
    string NumeroObligacion,
    string DestinoLineaCredito,
    decimal MontoLineaCredito,
    decimal MontoPorUtilizar,
    string CodigoEstado,
    decimal Saldo,
    Guid IdCuentaContable,
    decimal TasaInteres,
    decimal InteresesPorPagar,
    bool PagaComision,
    decimal? TasaInteresComision,
    decimal? ValorComision,
    DateOnly FechaConcesion,
    DateOnly FechaVencimiento,
    string CodigoPeriodicidadPago,
    bool TienePeriodoGracia,
    int? NumeroPeriodosGracia,
    string CodigoClase,
    decimal? ValorVencido,
    string? CodigoFormaCancelacion,
    string? NumeroObligacionAnterior);

public record ActualizarObligacionFinancieraBody(
    decimal MontoPorUtilizar,
    string CodigoEstado,
    decimal Saldo,
    decimal TasaInteres,
    decimal InteresesPorPagar,
    bool PagaComision,
    decimal? TasaInteresComision,
    decimal? ValorComision,
    bool TienePeriodoGracia,
    int? NumeroPeriodosGracia,
    decimal? ValorVencido,
    string? CodigoFormaCancelacion);

public record CatalogoDto(string Codigo, string Nombre);

public record Of01CabeceraDto(string CodigoEstructura, string Ruc, DateOnly FechaCorte, int NumeroTotalRegistros);

public record Of01DetalleDto(
    string TipoIdentificacionAcreedor, string IdentificacionAcreedor, string CodigoPaisAcreedor,
    string NumeroObligacion, string DestinoLineaCredito, decimal MontoLineaCredito, decimal MontoPorUtilizar,
    string CodigoEstado, decimal Saldo, string CodigoCuentaContable, decimal TasaInteres, decimal InteresesPorPagar,
    bool PagaComision, decimal? TasaInteresComision, decimal? ValorComision,
    DateOnly FechaConcesion, DateOnly FechaVencimiento, string CodigoPeriodicidadPago,
    bool TienePeriodoGracia, int? NumeroPeriodosGracia, string CodigoClase, decimal? ValorVencido,
    string? CodigoFormaCancelacion, string? NumeroObligacionAnterior);

public record Of01Result(Of01CabeceraDto Cabecera, IReadOnlyList<Of01DetalleDto> Detalle, IReadOnlyList<string> Advertencias);

/// <summary>
/// Captura y generación real de la estructura OF01 (Obligaciones
/// Financieras, SEPS) — primer generador del módulo "Estructuras y
/// Procesos Financieros", protegido por las dos políticas (módulo +
/// estructura específica) construidas para ese módulo.
/// </summary>
[ApiController]
[Route("api/estructuras-financieras/obligaciones")]
[Authorize(Policy = "Menu:estructuras-financieras")]
[Authorize(Policy = "Estructura:OF01")]
public class ObligacionesFinancierasController(IObligacionFinancieraService service, Corela15DbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ObligacionFinancieraDto>>> Listar(CancellationToken cancellationToken)
        => Ok(await service.ListarAsync(cancellationToken));

    [HttpPost]
    public async Task<ActionResult<ObligacionFinancieraDto>> Registrar(RegistrarObligacionFinancieraBody body, CancellationToken cancellationToken)
    {
        var resultado = await service.RegistrarAsync(new RegistrarObligacionFinancieraRequest(
            body.IdAgencia, body.TipoIdentificacionAcreedor, body.IdentificacionAcreedor, body.CodigoPaisAcreedor,
            body.NumeroObligacion, body.DestinoLineaCredito, body.MontoLineaCredito, body.MontoPorUtilizar,
            body.CodigoEstado, body.Saldo, body.IdCuentaContable, body.TasaInteres, body.InteresesPorPagar,
            body.PagaComision, body.TasaInteresComision, body.ValorComision,
            body.FechaConcesion, body.FechaVencimiento, body.CodigoPeriodicidadPago,
            body.TienePeriodoGracia, body.NumeroPeriodosGracia, body.CodigoClase, body.ValorVencido,
            body.CodigoFormaCancelacion, body.NumeroObligacionAnterior,
            User.Identity!.Name!), cancellationToken);

        return Created($"/api/estructuras-financieras/obligaciones/{resultado.Id}", resultado);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ObligacionFinancieraDto>> Actualizar(Guid id, ActualizarObligacionFinancieraBody body, CancellationToken cancellationToken)
    {
        var resultado = await service.ActualizarAsync(id, new ActualizarObligacionFinancieraRequest(
            body.MontoPorUtilizar, body.CodigoEstado, body.Saldo, body.TasaInteres, body.InteresesPorPagar,
            body.PagaComision, body.TasaInteresComision, body.ValorComision,
            body.TienePeriodoGracia, body.NumeroPeriodosGracia, body.ValorVencido, body.CodigoFormaCancelacion,
            User.Identity!.Name!), cancellationToken);

        return Ok(resultado);
    }

    [HttpGet("catalogos/paises")]
    public async Task<ActionResult<IReadOnlyList<CatalogoDto>>> Paises(CancellationToken cancellationToken)
        => Ok(await db.Nacionalidades.Where(n => n.Activo).OrderBy(n => n.Nombre)
            .Select(n => new CatalogoDto(n.Codigo, n.Nombre)).ToListAsync(cancellationToken));

    [HttpGet("catalogos/estados")]
    public async Task<ActionResult<IReadOnlyList<CatalogoDto>>> Estados(CancellationToken cancellationToken)
        => Ok(await db.Set<Domain.Obligacion.EstadoObligacionFinanciera>().Where(e => e.Activo).OrderBy(e => e.Codigo)
            .Select(e => new CatalogoDto(e.Codigo, e.Nombre)).ToListAsync(cancellationToken));

    [HttpGet("catalogos/periodicidades-pago")]
    public async Task<ActionResult<IReadOnlyList<CatalogoDto>>> Periodicidades(CancellationToken cancellationToken)
        => Ok(await db.Set<Domain.Obligacion.PeriodicidadPago>().Where(p => p.Activo).OrderBy(p => p.Nombre)
            .Select(p => new CatalogoDto(p.Codigo, p.Nombre)).ToListAsync(cancellationToken));

    [HttpGet("catalogos/clases")]
    public async Task<ActionResult<IReadOnlyList<CatalogoDto>>> Clases(CancellationToken cancellationToken)
        => Ok(await db.Set<Domain.Obligacion.ClaseObligacionFinanciera>().Where(c => c.Activo).OrderBy(c => c.Codigo)
            .Select(c => new CatalogoDto(c.Codigo, c.Nombre)).ToListAsync(cancellationToken));

    [HttpGet("catalogos/formas-cancelacion")]
    public async Task<ActionResult<IReadOnlyList<CatalogoDto>>> FormasCancelacion(CancellationToken cancellationToken)
        => Ok(await db.Set<Domain.Obligacion.FormaCancelacionObligacion>().Where(f => f.Activo).OrderBy(f => f.Codigo)
            .Select(f => new CatalogoDto(f.Codigo, f.Nombre)).ToListAsync(cancellationToken));

    [HttpGet("catalogos/cuentas-contables")]
    public async Task<ActionResult<IReadOnlyList<CatalogoDto>>> CuentasContables(CancellationToken cancellationToken)
        => Ok(await db.CuentasContables
            .Where(c => c.Activa && c.EsMayor && c.Codigo.StartsWith("26"))
            .OrderBy(c => c.Codigo)
            .Select(c => new CatalogoDto(c.Codigo, c.Nombre))
            .ToListAsync(cancellationToken));

    /// <summary>
    /// Genera la estructura OF01 real (cabecera + detalle, campos exactos
    /// del Manual Técnico v1.0) a la fecha de corte solicitada — sobre las
    /// obligaciones capturadas hasta ese momento. Mismo criterio ya
    /// aplicado en B11/B13/S01: expone los datos y campos reales de la
    /// estructura; el empaquetado final XML+hash+zip
    /// (OF01_RUC_dd-mm-aaaa.zip, exigido por el manual) queda fuera de
    /// alcance por no contar con el XSD/layout XML exacto todavía.
    /// </summary>
    [HttpGet("generar")]
    public async Task<ActionResult<Of01Result>> Generar([FromQuery] DateOnly fechaCorte, CancellationToken cancellationToken)
    {
        var empresa = await db.Empresas.FirstOrDefaultAsync(cancellationToken);
        var advertencias = new List<string>();
        if (empresa is null || string.IsNullOrWhiteSpace(empresa.Ruc))
            advertencias.Add("La empresa no tiene RUC configurado (Configuración > Empresa) — campo obligatorio de la cabecera.");

        var obligaciones = await db.ObligacionesFinancieras
            .Include(o => o.CuentaContable)
            .Where(o => o.FechaConcesion <= fechaCorte)
            .OrderBy(o => o.NumeroObligacion)
            .ToListAsync(cancellationToken);

        if (obligaciones.Count == 0)
            advertencias.Add("No hay obligaciones financieras capturadas hasta la fecha de corte — el reporte queda vacío por diseño, no es un error.");

        var detalle = obligaciones.Select(o => new Of01DetalleDto(
            o.TipoIdentificacionAcreedor, o.IdentificacionAcreedor, o.CodigoPaisAcreedor,
            o.NumeroObligacion, o.DestinoLineaCredito, o.MontoLineaCredito, o.MontoPorUtilizar,
            o.CodigoEstado, o.Saldo, o.CuentaContable.Codigo, o.TasaInteres, o.InteresesPorPagar,
            o.PagaComision, o.TasaInteresComision, o.ValorComision,
            o.FechaConcesion, o.FechaVencimiento, o.CodigoPeriodicidadPago,
            o.TienePeriodoGracia, o.NumeroPeriodosGracia, o.CodigoClase, o.ValorVencido,
            o.CodigoFormaCancelacion, o.NumeroObligacionAnterior)).ToList();

        var cabecera = new Of01CabeceraDto("OF01", empresa?.Ruc ?? string.Empty, fechaCorte, detalle.Count + 1);

        return Ok(new Of01Result(cabecera, detalle, advertencias));
    }
}
