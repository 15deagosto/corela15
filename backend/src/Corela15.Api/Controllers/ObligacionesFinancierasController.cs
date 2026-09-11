using System.Globalization;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
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
        var (cabecera, detalle, advertencias) = await ArmarAsync(fechaCorte, cancellationToken);
        return Ok(new Of01Result(cabecera, detalle, advertencias));
    }

    private async Task<(Of01CabeceraDto Cabecera, List<Of01DetalleDto> Detalle, List<string> Advertencias)> ArmarAsync(
        DateOnly fechaCorte, CancellationToken cancellationToken)
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

        return (cabecera, detalle, advertencias);
    }

    /// <summary>
    /// Empaquetado real de envío: XML + hash (.txt) comprimidos en un
    /// único .zip, con el nombre exacto que exige el manual
    /// (OF01_RUC_dd-mm-aaaa.zip, sección "Control de nombre del archivo").
    ///
    /// Advertencia real, no oculta: el Manual Técnico v1.0 (el que
    /// compartió el usuario) describe cada campo de la cabecera y el
    /// detalle en una tabla (Nro/Campo/Tipo/Obligatoriedad/Tabla) pero
    /// -- a diferencia del XSD real de otras estructuras ya construidas
    /// esta sesión (ROTEF, F01/Servicios Financieros) -- NO trae una
    /// columna de "tag XML" con el nombre exacto de cada elemento. Los
    /// nombres de atributo usados acá (camelCase) siguen la misma
    /// convención real que sí está confirmada en el XSD oficial de F01
    /// (mismo organismo, mismo tipo de estructura: <financiero
    /// estructura="" rucEntidad="" fechaCorte="" numRegistro=""><elemento
    /// .../></financiero>) -- es la mejor inferencia disponible, pero NO
    /// es un XSD verificado como los de ROTEF/S01/D01. Recomendado
    /// confirmar el primer envío real con Soporte SEPS (ver sección 6 del
    /// manual, ej. maria.delgado@seps.gob.ec) antes de asumir que se
    /// acepta sin ajustes de nomenclatura.
    ///
    /// El algoritmo de hash tampoco lo especifica el manual (solo dice
    /// "archivo .txt – hash de seguridad") -- se usa SHA-256 en
    /// hexadecimal minúscula, el estándar real más común para este tipo
    /// de acuse de integridad; ajustar acá mismo si SEPS confirma otro.
    /// </summary>
    [HttpGet("of01/paquete")]
    public async Task<IActionResult> GenerarPaquete([FromQuery] DateOnly fechaCorte, CancellationToken cancellationToken)
    {
        var (cabecera, detalle, _) = await ArmarAsync(fechaCorte, cancellationToken);
        if (string.IsNullOrWhiteSpace(cabecera.Ruc))
            return BadRequest(new { detail = "La empresa no tiene RUC configurado (Configuración > Empresa) — no se puede armar el paquete sin ese dato obligatorio de la cabecera." });

        var xml = ArmarXml(cabecera, detalle);
        var xmlBytes = Encoding.UTF8.GetBytes(xml);
        var hashHex = Convert.ToHexString(SHA256.HashData(xmlBytes)).ToLowerInvariant();

        var baseNombre = $"OF01_{cabecera.Ruc}_{fechaCorte:dd-MM-yyyy}";

        using var memoria = new MemoryStream();
        using (var zip = new ZipArchive(memoria, ZipArchiveMode.Create, leaveOpen: true))
        {
            var entradaXml = zip.CreateEntry($"{baseNombre}.xml", CompressionLevel.Optimal);
            await using (var s = entradaXml.Open()) await s.WriteAsync(xmlBytes, cancellationToken);

            var entradaHash = zip.CreateEntry($"{baseNombre}.txt", CompressionLevel.Optimal);
            await using (var s = entradaHash.Open()) await s.WriteAsync(Encoding.UTF8.GetBytes(hashHex), cancellationToken);
        }

        return File(memoria.ToArray(), "application/zip", $"{baseNombre}.zip");
    }

    private static string ArmarXml(Of01CabeceraDto cabecera, IReadOnlyList<Of01DetalleDto> detalle)
    {
        static string Num(decimal v) => v.ToString("0.00", CultureInfo.InvariantCulture);
        static string? Fecha(DateOnly? f) => f?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

        var raiz = new XElement("of01",
            new XAttribute("estructura", cabecera.CodigoEstructura),
            new XAttribute("rucEntidad", cabecera.Ruc),
            new XAttribute("fechaCorte", cabecera.FechaCorte.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)),
            new XAttribute("numRegistro", cabecera.NumeroTotalRegistros));

        foreach (var d in detalle)
        {
            var el = new XElement("obligacion",
                new XAttribute("tipoIdentificacionInstitucionAcreedora", d.TipoIdentificacionAcreedor),
                new XAttribute("identificacionInstitucionAcreedora", d.IdentificacionAcreedor),
                new XAttribute("paisInstitucionAcreedora", d.CodigoPaisAcreedor),
                new XAttribute("numeroObligacionFinanciera", d.NumeroObligacion),
                new XAttribute("destinoLineaCreditoDesembolsada", d.DestinoLineaCredito.ToUpperInvariant()),
                new XAttribute("montoLineaCreditoDesembolsada", Num(d.MontoLineaCredito)),
                new XAttribute("montoPorUtilizar", Num(d.MontoPorUtilizar)),
                new XAttribute("estadoObligacionFinanciera", d.CodigoEstado),
                new XAttribute("saldo", Num(d.Saldo)),
                new XAttribute("cuentaContable", d.CodigoCuentaContable[..Math.Min(4, d.CodigoCuentaContable.Length)]),
                new XAttribute("tasaInteres", d.TasaInteres.ToString("0.00", CultureInfo.InvariantCulture)),
                new XAttribute("interesesPorPagar", Num(d.InteresesPorPagar)),
                new XAttribute("pagoComision", d.PagaComision ? "S" : "N"),
                new XAttribute("fechaConcesion", Fecha(d.FechaConcesion)!),
                new XAttribute("fechaVencimiento", Fecha(d.FechaVencimiento)!),
                new XAttribute("periodicidadPago", d.CodigoPeriodicidadPago),
                new XAttribute("periodoGracia", d.TienePeriodoGracia ? "S" : "N"),
                new XAttribute("claseObligacionFinanciera", d.CodigoClase));

            // Regla real del manual ("Cuenta y Subcuenta contable"): si la
            // cuenta es 2601 (sobregiros), la subcuenta NO se reporta.
            var cuenta4 = d.CodigoCuentaContable[..Math.Min(4, d.CodigoCuentaContable.Length)];
            if (cuenta4 != "2601")
                el.Add(new XAttribute("subcuentaContable", d.CodigoCuentaContable));

            // Campos condicionales del manual (marcados "X*") -- se omiten
            // por completo cuando no aplican, nunca se manda vacío ni
            // cero, tal como exige cada regla puntual del manual.
            if (d.PagaComision)
            {
                if (d.TasaInteresComision is { } tic) el.Add(new XAttribute("tasaInteresComision", tic.ToString("0.00", CultureInfo.InvariantCulture)));
                if (d.ValorComision is { } vc) el.Add(new XAttribute("valorComision", Num(vc)));
            }
            if (d.TienePeriodoGracia && d.NumeroPeriodosGracia is { } npg)
                el.Add(new XAttribute("numeroPeriodoGracia", npg));
            if (d.CodigoEstado == "VN" && d.ValorVencido is { } vv)
                el.Add(new XAttribute("valorVencido", Num(vv)));
            if (d.CodigoEstado == "CN" && !string.IsNullOrWhiteSpace(d.CodigoFormaCancelacion))
                el.Add(new XAttribute("formaCancelacion", d.CodigoFormaCancelacion));
            if (!string.IsNullOrWhiteSpace(d.NumeroObligacionAnterior))
                el.Add(new XAttribute("numeroObligacionAnterior", d.NumeroObligacionAnterior));

            raiz.Add(el);
        }

        var doc = new XDocument(new XDeclaration("1.0", "UTF-8", null), raiz);
        return doc.Declaration + "\n" + doc.Root;
    }

    /// <summary>
    /// Única llamada real de todo el backend que toca Softbank -- ver
    /// <see cref="IObligacionSyncService"/>. Trae/actualiza obligaciones
    /// financieras reales desde Softbank hacia esta misma base, en el
    /// momento, sin depender de correr una herramienta externa por
    /// consola. Protegido por las mismas 2 políticas del controller
    /// (módulo + estructura OF01) -- nadie más del sistema puede
    /// dispararlo.
    /// </summary>
    [HttpPost("sincronizar")]
    public async Task<ActionResult<SincronizacionObligacionesResult>> Sincronizar(
        [FromServices] IObligacionSyncService syncService, CancellationToken cancellationToken)
        => Ok(await syncService.SincronizarAsync(cancellationToken));
}
