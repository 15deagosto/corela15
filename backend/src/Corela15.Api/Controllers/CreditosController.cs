using Corela15.Api.Idempotencia;
using Corela15.Application.Colocacion;
using Corela15.Domain.Colocacion;
using Corela15.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Api.Controllers;

public record DiferirCuotasBody(int DiasDiferidos, string Comentario);

public record CastigarPrestamoBody(string Comentario);

public record PagareCustodiaMovimientoItem(string CodigoEstado, string Ubicacion, bool EsRecepcion, DateTimeOffset Fecha, string RegistradoPor);

public record PagareCustodiaItem(
    Guid Id, string CodigoEstado, string Estado, string Ubicacion, DateOnly FechaActualizacion,
    IReadOnlyList<PagareCustodiaMovimientoItem> Movimientos);

public record RegistrarCustodiaPagareBody(string CodigoEstado, string Ubicacion);

public record TipoPrestamoListItem(
    int Id, string Codigo, string Nombre, decimal MontoMinimo, decimal MontoMaximo, decimal TasaAnual, bool Activo);

public record SolicitudPrestamoListItem(
    Guid Id, string Numero, Guid IdCliente, string Socio, string Producto, decimal MontoSolicitado, int Cuotas, string Estado, DateOnly FechaSolicitud,
    decimal? MontoAprobado, string? AprobadoPor, string? ComentarioAprobacion, string? EtapaActual);

public record SolicitudEtapaHistItem(
    string? EtapaAnterior, string Etapa, string? Comentario, string RegistradoPor, DateTimeOffset Fecha, bool EsRetorno);

public record PrestamoListItem(
    Guid Id, string Numero, string Producto, decimal DeudaInicial, decimal Saldo, decimal Tasa, string Estado,
    DateOnly FechaAdjudicacion, bool DebitoSpi, string? CodigoUsuarioAsesor, string? NombreConvenio);

[ApiController]
[Route("api/creditos")]
[Authorize(Policy = "Menu:creditos")]
public class CreditosController(
    Corela15DbContext db, IPrestamoService prestamoService, IProvisionCarteraService provisionCarteraService,
    IScoreCrediticioService scoreCrediticioService, IAutoDebitoSpiService autoDebitoSpiService,
    IGarantiaService garantiaService, IMoraCarteraService moraCarteraService) : ControllerBase
{
    [HttpGet("productos")]
    public async Task<ActionResult<IReadOnlyList<TipoPrestamoListItem>>> Productos(CancellationToken cancellationToken)
    {
        var resultado = await db.TiposPrestamo
            .OrderBy(t => t.Nombre)
            .Select(t => new TipoPrestamoListItem(t.Id, t.Codigo, t.Nombre, t.MontoMinimo, t.MontoMaximo, t.TasaAnual, t.Activo))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpGet("solicitudes")]
    public async Task<ActionResult<IReadOnlyList<SolicitudPrestamoListItem>>> Solicitudes(CancellationToken cancellationToken)
    {
        var resultado = await db.SolicitudesPrestamo
            .Include(s => s.Cliente).ThenInclude(c => c.Persona)
            .Include(s => s.TipoPrestamo)
            .Include(s => s.EtapaActual)
            .OrderByDescending(s => s.FechaSolicitud)
            .Select(s => new SolicitudPrestamoListItem(
                s.Id, s.Numero, s.IdCliente, s.Cliente.Persona.Nombre, s.TipoPrestamo.Nombre,
                s.MontoSolicitado, s.Cuotas, s.Estado.ToString(), s.FechaSolicitud,
                s.MontoAprobado, s.AprobadoPor, s.ComentarioAprobacion,
                s.EtapaActual != null ? s.EtapaActual.Nombre : null))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    // Bitácora real de decisiones (ver Domain/FlujoTrabajo/CLAUDE.md "Motor
    // de aprobaciones genérico") — quién decidió qué etapa, cuándo, y si fue
    // un retorno (rechazo).
    [HttpGet("solicitudes/{idSolicitud:guid}/etapas-historial")]
    public async Task<ActionResult<IReadOnlyList<SolicitudEtapaHistItem>>> EtapasHistorial(
        Guid idSolicitud, CancellationToken cancellationToken)
    {
        var resultado = await db.SolicitudesPrestamoEtapaHist
            .Include(h => h.Etapa)
            .Where(h => h.IdSolicitudPrestamo == idSolicitud)
            .OrderBy(h => h.Fecha)
            .Select(h => new SolicitudEtapaHistItem(
                h.IdEtapaAnterior != null ? db.Etapas.Where(e => e.Id == h.IdEtapaAnterior).Select(e => e.Nombre).FirstOrDefault() : null,
                h.Etapa.Nombre, h.Comentario, h.RegistradoPor, h.Fecha, h.EsRetorno))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpGet("prestamos")]
    public async Task<ActionResult<IReadOnlyList<PrestamoListItem>>> Prestamos(CancellationToken cancellationToken)
    {
        var resultado = await db.Prestamos
            .Include(p => p.TipoPrestamo)
            .Include(p => p.TipoConvenio)
            .OrderByDescending(p => p.FechaAdjudicacion)
            .Select(p => new PrestamoListItem(
                p.Id, p.Numero, p.TipoPrestamo.Nombre, p.DeudaInicial, p.Saldo, p.Tasa, p.Estado.ToString(),
                p.FechaAdjudicacion, p.DebitoSpi, p.CodigoUsuarioAsesor, p.TipoConvenio!.Nombre))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    // Reportes reales verificados contra el catálogo de Softbank
    // (SEGURIDAD.MENU_REPORTE, ver 06-catalogo-reportes-softbank.md) —
    // Colocacion.ConcesionCreditoReport, Colocacion.CreditosPrecancelados,
    // Colocacion.ProximosVencimientos.
    [HttpGet("reportes/concesion-credito")]
    public async Task<ActionResult<IReadOnlyList<ConcesionCreditoItem>>> ReporteConcesionCredito(
        [FromQuery] DateOnly desde, [FromQuery] DateOnly hasta, CancellationToken cancellationToken)
    {
        var resultado = await db.Prestamos
            .Include(p => p.TipoPrestamo)
            .Where(p => p.FechaAdjudicacion >= desde && p.FechaAdjudicacion <= hasta)
            .OrderBy(p => p.FechaAdjudicacion)
            .Select(p => new ConcesionCreditoItem(
                p.Numero, p.TipoPrestamo.Nombre,
                db.PrestamosClientes.Where(pc => pc.IdPrestamo == p.Id && pc.Principal)
                    .Select(pc => pc.Cliente.Persona.Nombre).FirstOrDefault() ?? "—",
                p.DeudaInicial, p.Tasa, p.FechaAdjudicacion, p.FechaVencimiento, p.Estado.ToString()))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    // Precancelado = Cancelado antes de su fecha de vencimiento nominal
    // (ModificadoEn del préstamo, que se actualiza en el pago de la cuota
    // que lo cancela, comparado contra FechaVencimiento) — no requiere un
    // campo nuevo, se deriva de datos ya reales del dominio.
    [HttpGet("reportes/creditos-precancelados")]
    public async Task<ActionResult<IReadOnlyList<CreditoPrecanceladoItem>>> ReporteCreditosPrecancelados(
        [FromQuery] DateOnly desde, [FromQuery] DateOnly hasta, CancellationToken cancellationToken)
    {
        var resultado = await db.Prestamos
            .Include(p => p.TipoPrestamo)
            .Where(p => p.Estado == EstadoPrestamo.Cancelado && p.ModificadoEn != null
                && DateOnly.FromDateTime(p.ModificadoEn!.Value.Date) >= desde
                && DateOnly.FromDateTime(p.ModificadoEn!.Value.Date) <= hasta
                && DateOnly.FromDateTime(p.ModificadoEn!.Value.Date) < p.FechaVencimiento)
            .OrderByDescending(p => p.ModificadoEn)
            .Select(p => new CreditoPrecanceladoItem(
                p.Numero, p.TipoPrestamo.Nombre,
                db.PrestamosClientes.Where(pc => pc.IdPrestamo == p.Id && pc.Principal)
                    .Select(pc => pc.Cliente.Persona.Nombre).FirstOrDefault() ?? "—",
                p.DeudaInicial, p.FechaAdjudicacion, p.FechaVencimiento,
                DateOnly.FromDateTime(p.ModificadoEn!.Value.Date)))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpGet("reportes/proximos-vencimientos")]
    public async Task<ActionResult<IReadOnlyList<ProximoVencimientoItem>>> ReporteProximosVencimientos(
        [FromQuery] int dias, CancellationToken cancellationToken)
    {
        var limite = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(dias);
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

        var resultado = await db.PrestamosRubros
            .Include(r => r.Prestamo).ThenInclude(p => p.TipoPrestamo)
            .Include(r => r.Rubro).ThenInclude(rb => rb.TipoRubro)
            .Where(r => r.Estado == "P" && r.Rubro.TipoRubro.EsCapital
                && r.Prestamo.Estado == EstadoPrestamo.Vigente
                && r.FechaFin >= hoy && r.FechaFin <= limite)
            .OrderBy(r => r.FechaFin)
            .Select(r => new ProximoVencimientoItem(
                r.Prestamo.Numero, r.Prestamo.TipoPrestamo.Nombre,
                db.PrestamosClientes.Where(pc => pc.IdPrestamo == r.IdPrestamo && pc.Principal)
                    .Select(pc => pc.Cliente.Persona.Nombre).FirstOrDefault() ?? "—",
                r.NumeroCuota, r.Proyectado, r.FechaFin))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    // Colocacion.ConsolidadoCredCancelados / Colocacion.AnexoGarantias —
    // ver 06-catalogo-reportes-softbank.md.
    [HttpGet("reportes/creditos-cancelados")]
    public async Task<ActionResult<IReadOnlyList<CreditoCanceladoItem>>> ReporteCreditosCancelados(
        [FromQuery] DateOnly desde, [FromQuery] DateOnly hasta, CancellationToken cancellationToken)
    {
        var resultado = await db.Prestamos
            .Include(p => p.TipoPrestamo)
            .Where(p => p.Estado == EstadoPrestamo.Cancelado && p.ModificadoEn != null
                && DateOnly.FromDateTime(p.ModificadoEn!.Value.Date) >= desde
                && DateOnly.FromDateTime(p.ModificadoEn!.Value.Date) <= hasta)
            .OrderByDescending(p => p.ModificadoEn)
            .Select(p => new CreditoCanceladoItem(
                p.Numero, p.TipoPrestamo.Nombre,
                db.PrestamosClientes.Where(pc => pc.IdPrestamo == p.Id && pc.Principal)
                    .Select(pc => pc.Cliente.Persona.Nombre).FirstOrDefault() ?? "—",
                p.DeudaInicial, p.FechaAdjudicacion, DateOnly.FromDateTime(p.ModificadoEn!.Value.Date)))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    // Colocacion.CreditosMoraAsesor — ver 06-catalogo-reportes-softbank.md.
    // A diferencia del mismo reporte en Softbank (ver Base-Conocimiento/
    // Softbank/Caso-Reporte-Mora-Asesor-No-Respeta-Consulta-Cruzada.md — el
    // bug real encontrado ahí: filtra por agencia propia del usuario en vez
    // de por consulta cruzada, ocultando préstamos legítimos), acá no hay
    // ningún filtro de agencia oculto — cualquier usuario con
    // Menu:creditos ve la cartera completa por asesor, sin importar su
    // propia agencia.
    [HttpGet("reportes/creditos-mora-asesor")]
    public async Task<ActionResult<IReadOnlyList<CreditoMoraAsesorItem>>> ReporteCreditosMoraAsesor(
        [FromQuery] string? codigoUsuarioAsesor, CancellationToken cancellationToken)
    {
        var moras = await moraCarteraService.CalcularAsync(cancellationToken);
        if (moras.Count == 0) return Ok(Array.Empty<CreditoMoraAsesorItem>());

        var idsPrestamo = moras.Where(m => m.DiasMora > 0).Select(m => m.IdPrestamo).ToList();
        var prestamos = await db.Prestamos
            .Include(p => p.TipoPrestamo)
            .Where(p => idsPrestamo.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        var titulares = await db.PrestamosClientes
            .Where(pc => pc.Principal && idsPrestamo.Contains(pc.IdPrestamo))
            .Select(pc => new { pc.IdPrestamo, Nombre = pc.Cliente.Persona.Nombre })
            .ToDictionaryAsync(x => x.IdPrestamo, x => x.Nombre, cancellationToken);

        var resultado = moras
            .Where(m => m.DiasMora > 0 && prestamos.ContainsKey(m.IdPrestamo))
            .Select(m => new { Mora = m, Prestamo = prestamos[m.IdPrestamo] })
            .Where(x => codigoUsuarioAsesor == null || x.Prestamo.CodigoUsuarioAsesor == codigoUsuarioAsesor)
            .OrderByDescending(x => x.Mora.DiasMora)
            .Select(x => new CreditoMoraAsesorItem(
                x.Prestamo.Numero, x.Prestamo.TipoPrestamo.Nombre,
                titulares.GetValueOrDefault(x.Mora.IdPrestamo, "—"),
                x.Prestamo.CodigoUsuarioAsesor, x.Mora.Saldo, x.Mora.DiasMora))
            .ToList();

        return Ok(resultado);
    }

    // Colocacion.IndiceMorosidad / IndiceMorosidad2 — cartera vencida real
    // (saldo de préstamos con DiasMora > 0) sobre cartera total vigente,
    // única fuente de verdad de mora ya construida (IMoraCarteraService).
    [HttpGet("reportes/indice-morosidad")]
    public async Task<ActionResult<IndiceMorosidadItem>> ReporteIndiceMorosidad(CancellationToken cancellationToken)
    {
        var moras = await moraCarteraService.CalcularAsync(cancellationToken);
        var carteraTotal = moras.Sum(m => m.Saldo);
        var carteraVencida = moras.Where(m => m.DiasMora > 0).Sum(m => m.Saldo);
        var indice = carteraTotal > 0 ? carteraVencida / carteraTotal : 0m;

        return Ok(new IndiceMorosidadItem(
            carteraTotal, carteraVencida, indice,
            moras.Count, moras.Count(m => m.DiasMora > 0)));
    }

    // Colocacion.DebitosSpiNoProcesados — bitácora real ya construida
    // (AutoDebitoSpiLog, ver CLAUDE.md "Auto-débito de cuota por SPI"),
    // filtrada a los omitidos (Debitado=false) con su motivo explícito —
    // exactamente el gap que originó este proyecto (01-contexto-origen.md).
    [HttpGet("reportes/debitos-spi-no-procesados")]
    public async Task<ActionResult<IReadOnlyList<DebitoSpiNoProcesadoItem>>> ReporteDebitosSpiNoProcesados(
        [FromQuery] DateOnly desde, [FromQuery] DateOnly hasta, CancellationToken cancellationToken)
    {
        var resultado = await db.AutoDebitosSpiLog
            .Include(l => l.Prestamo)
            .Where(l => !l.Debitado && l.Fecha >= desde && l.Fecha <= hasta)
            .OrderByDescending(l => l.Fecha)
            .Select(l => new DebitoSpiNoProcesadoItem(
                l.Prestamo.Numero, l.NumeroCuota, l.Monto, l.Motivo, l.Fecha))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    // Colocacion.AnexoCarteraCastigada — el evento formal real ya
    // construido (PrestamoCastigado, ver CLAUDE.md "Colocación:
    // diferimiento de cuotas, cartera castigada, custodia de pagaré"),
    // nunca duplicado, solo listado por rango de fecha.
    [HttpGet("reportes/anexo-cartera-castigada")]
    public async Task<ActionResult<IReadOnlyList<AnexoCarteraCastigadaItem>>> ReporteAnexoCarteraCastigada(
        [FromQuery] DateOnly desde, [FromQuery] DateOnly hasta, CancellationToken cancellationToken)
    {
        var resultado = await db.PrestamosCastigados
            .Include(c => c.Prestamo).ThenInclude(p => p.TipoPrestamo)
            .Where(c => c.Fecha >= desde && c.Fecha <= hasta)
            .OrderByDescending(c => c.Fecha)
            .Select(c => new AnexoCarteraCastigadaItem(
                c.Prestamo.Numero, c.Prestamo.TipoPrestamo.Nombre,
                db.PrestamosClientes.Where(pc => pc.IdPrestamo == c.IdPrestamo && pc.Principal)
                    .Select(pc => pc.Cliente.Persona.Nombre).FirstOrDefault() ?? "—",
                c.SaldoTransferido, c.Fecha, c.Comentario, c.RegistradoPor))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    // Colocacion.AnexoCarteraCastigadaAgencia / AnexoCarteraCastigadaCliente
    // — mismas filas de PrestamosCastigados ya usadas en el reporte base,
    // solo agrupadas. Agencia real desde Prestamo.IdAgencia (ya existía,
    // sin campo nuevo); no requirió índice adicional (volumen bajo, sin
    // filtro de fecha en el agrupado).
    [HttpGet("reportes/anexo-cartera-castigada-agencia")]
    public async Task<ActionResult<IReadOnlyList<AnexoCarteraCastigadaAgenciaItem>>> ReporteAnexoCarteraCastigadaAgencia(
        CancellationToken cancellationToken)
    {
        var resultado = await db.PrestamosCastigados
            .Include(c => c.Prestamo).ThenInclude(p => p.Agencia)
            .GroupBy(c => new { c.Prestamo.IdAgencia, c.Prestamo.Agencia.Nombre })
            .OrderByDescending(g => g.Sum(c => c.SaldoTransferido))
            .Select(g => new AnexoCarteraCastigadaAgenciaItem(g.Key.Nombre, g.Count(), g.Sum(c => c.SaldoTransferido)))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpGet("reportes/anexo-cartera-castigada-cliente")]
    public async Task<ActionResult<IReadOnlyList<AnexoCarteraCastigadaClienteItem>>> ReporteAnexoCarteraCastigadaCliente(
        CancellationToken cancellationToken)
    {
        var castigos = await db.PrestamosCastigados
            .Include(c => c.Prestamo)
            .ToListAsync(cancellationToken);

        var idsPrestamo = castigos.Select(c => c.IdPrestamo).ToList();
        var titulares = await db.PrestamosClientes
            .Where(pc => pc.Principal && idsPrestamo.Contains(pc.IdPrestamo))
            .Select(pc => new { pc.IdPrestamo, Nombre = pc.Cliente.Persona.Nombre, Identificacion = pc.Cliente.Persona.Identificacion })
            .ToListAsync(cancellationToken);
        var titularPorPrestamo = titulares.ToDictionary(x => x.IdPrestamo, x => x);

        var resultado = castigos
            .GroupBy(c => c.Prestamo.Id)
            .Select(g =>
            {
                var t = titularPorPrestamo.GetValueOrDefault(g.Key);
                return new AnexoCarteraCastigadaClienteItem(
                    t?.Nombre ?? "—", t?.Identificacion ?? "—", g.Count(), g.Sum(c => c.SaldoTransferido));
            })
            .OrderByDescending(x => x.TotalCastigado)
            .ToList();

        return Ok(resultado);
    }

    // Colocacion.AnexoItemCreditoReport — detalle real de todos los rubros
    // (capital, interés, seguro, mora, manuales) de la cartera vigente,
    // el mismo dato que ya alimenta pago de cuota/mora/provisión, expuesto
    // como listado — sin duplicar ningún cálculo.
    [HttpGet("reportes/anexo-item-credito")]
    public async Task<ActionResult<IReadOnlyList<AnexoItemCreditoItem>>> ReporteAnexoItemCredito(
        [FromQuery] string? numeroPrestamo, CancellationToken cancellationToken)
    {
        var query = db.PrestamosRubros
            .Include(r => r.Prestamo)
            .Include(r => r.Rubro).ThenInclude(ru => ru.TipoRubro)
            .Where(r => r.Prestamo.Estado == EstadoPrestamo.Vigente);

        if (!string.IsNullOrWhiteSpace(numeroPrestamo))
            query = query.Where(r => r.Prestamo.Numero == numeroPrestamo);

        var resultado = await query
            .OrderBy(r => r.Prestamo.Numero).ThenBy(r => r.NumeroCuota)
            .Select(r => new AnexoItemCreditoItem(
                r.Prestamo.Numero, r.NumeroCuota, r.Rubro.Nombre, r.Rubro.CodigoTipoRubro,
                r.Proyectado, r.Cobrado, r.Estado, r.FechaFin, r.FechaCobro))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    // Colocacion.CreditosVinculados — cartera vigente de clientes con causa
    // de vinculación real distinta de "NV" (No vinculado), catálogo oficial
    // ya sembrado (ver CLAUDE.md "Catálogos socioeconómicos reales de
    // Socios"). Nunca inventa el concepto: reusa Cliente.CodigoCausaVinculacion
    // tal cual ya existía.
    [HttpGet("reportes/creditos-vinculados")]
    public async Task<ActionResult<IReadOnlyList<CreditoVinculadoItem>>> ReporteCreditosVinculados(
        CancellationToken cancellationToken)
    {
        var resultado = await db.Prestamos
            .Include(p => p.TipoPrestamo)
            .Where(p => p.Estado == EstadoPrestamo.Vigente)
            .Where(p => db.PrestamosClientes.Any(pc => pc.IdPrestamo == p.Id && pc.Principal
                && pc.Cliente.CodigoCausaVinculacion != null && pc.Cliente.CodigoCausaVinculacion != "NV"))
            .Select(p => new
            {
                p.Numero, Producto = p.TipoPrestamo.Nombre, p.Saldo,
                Vinculo = db.PrestamosClientes.Where(pc => pc.IdPrestamo == p.Id && pc.Principal)
                    .Select(pc => new { pc.Cliente.Persona.Nombre, pc.Cliente.CodigoCausaVinculacion, pc.Cliente.CausaVinculacion!.Descripcion })
                    .FirstOrDefault()
            })
            .Where(x => x.Vinculo != null)
            .OrderByDescending(x => x.Saldo)
            .ToListAsync(cancellationToken);

        var items = resultado
            .Select(x => new CreditoVinculadoItem(
                x.Numero, x.Producto, x.Vinculo!.Nombre, x.Saldo, x.Vinculo.CodigoCausaVinculacion!, x.Vinculo.Descripcion))
            .ToList();

        return Ok(items);
    }

    // Colocacion.PrestamoPorTipoConvenio — cartera vigente con convenio real
    // asignado (Prestamo.CodigoTipoConvenio, ver credito.tipo_convenio).
    [HttpGet("reportes/prestamo-por-convenio")]
    public async Task<ActionResult<IReadOnlyList<PrestamoPorConvenioItem>>> ReportePrestamoPorConvenio(
        CancellationToken cancellationToken)
    {
        var resultado = await db.Prestamos
            .Include(p => p.TipoPrestamo)
            .Include(p => p.TipoConvenio)
            .Where(p => p.Estado == EstadoPrestamo.Vigente && p.CodigoTipoConvenio != null)
            .OrderBy(p => p.TipoConvenio!.Nombre).ThenByDescending(p => p.Saldo)
            .Select(p => new
            {
                p.Numero, Producto = p.TipoPrestamo.Nombre, p.Saldo,
                p.CodigoTipoConvenio, Convenio = p.TipoConvenio!.Nombre,
            })
            .ToListAsync(cancellationToken);

        var idsPrestamo = await db.Prestamos
            .Where(p => p.Estado == EstadoPrestamo.Vigente && p.CodigoTipoConvenio != null)
            .Select(p => p.Id).ToListAsync(cancellationToken);
        var titulares = await db.PrestamosClientes
            .Where(pc => pc.Principal && idsPrestamo.Contains(pc.IdPrestamo))
            .Select(pc => new { pc.IdPrestamo, Nombre = pc.Cliente.Persona.Nombre })
            .ToDictionaryAsync(x => x.IdPrestamo, x => x.Nombre, cancellationToken);
        var numeroAId = await db.Prestamos.Where(p => idsPrestamo.Contains(p.Id))
            .Select(p => new { p.Id, p.Numero }).ToDictionaryAsync(x => x.Numero, x => x.Id, cancellationToken);

        var items = resultado
            .Select(r => new PrestamoPorConvenioItem(
                r.Numero, r.Producto,
                numeroAId.TryGetValue(r.Numero, out var idP) ? titulares.GetValueOrDefault(idP, "—") : "—",
                r.Saldo, r.CodigoTipoConvenio!, r.Convenio))
            .ToList();

        return Ok(items);
    }

    // Colocacion.ReportePorTipoConvenio ("REPORTE ABONO PRESTAMO CONVENIO")
    // — pagos de capital reales (rubros CAP cobrados) en el rango, sobre
    // préstamos con convenio asignado. Reusa PrestamoRubro, sin duplicar
    // ningún cálculo de pago.
    [HttpGet("reportes/abonos-por-convenio")]
    public async Task<ActionResult<IReadOnlyList<AbonoConvenioItem>>> ReporteAbonosPorConvenio(
        [FromQuery] DateOnly desde, [FromQuery] DateOnly hasta, CancellationToken cancellationToken)
    {
        var resultado = await db.PrestamosRubros
            .Include(r => r.Prestamo).ThenInclude(p => p.TipoConvenio)
            .Include(r => r.Rubro).ThenInclude(ru => ru.TipoRubro)
            .Where(r => r.Rubro.TipoRubro.EsCapital && r.Estado == "C"
                && r.Prestamo.CodigoTipoConvenio != null
                && r.FechaCobro != null && r.FechaCobro >= desde && r.FechaCobro <= hasta)
            .OrderByDescending(r => r.FechaCobro)
            .Select(r => new AbonoConvenioItem(
                r.Prestamo.Numero, r.Prestamo.TipoConvenio!.Nombre, r.NumeroCuota, r.Cobrado, r.FechaCobro!.Value))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    // Colocacion.CalificacionPrestamoDetallado — misma clasificación real
    // usada por IProvisionCarteraService.EjecutarCalculoAsync (matriz
    // A1-E, Art. 44), pero de solo lectura: nunca llama al cálculo que
    // registra el asiento contable, solo replica la misma regla de
    // negocio para mostrarla — evita que "ver el reporte" tenga un efecto
    // contable secundario.
    [HttpGet("reportes/calificacion-prestamos")]
    public async Task<ActionResult<IReadOnlyList<CalificacionPrestamoItem>>> ReporteCalificacionPrestamos(
        CancellationToken cancellationToken)
    {
        var categorias = await db.CategoriasRiesgoCartera
            .Where(c => c.Activo)
            .OrderBy(c => c.DiasMoraInicio)
            .ToListAsync(cancellationToken);

        var moras = await moraCarteraService.CalcularAsync(cancellationToken);
        if (moras.Count == 0) return Ok(Array.Empty<CalificacionPrestamoItem>());

        var idsPrestamo = moras.Select(m => m.IdPrestamo).ToList();
        var prestamos = await db.Prestamos
            .Include(p => p.TipoPrestamo)
            .Where(p => idsPrestamo.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);
        var titulares = await db.PrestamosClientes
            .Where(pc => pc.Principal && idsPrestamo.Contains(pc.IdPrestamo))
            .Select(pc => new { pc.IdPrestamo, Nombre = pc.Cliente.Persona.Nombre })
            .ToDictionaryAsync(x => x.IdPrestamo, x => x.Nombre, cancellationToken);

        var resultado = moras
            .Where(m => prestamos.ContainsKey(m.IdPrestamo))
            .OrderByDescending(m => m.DiasMora)
            .Select(m =>
            {
                var categoria = categorias.FirstOrDefault(c => m.DiasMora >= c.DiasMoraInicio && m.DiasMora <= c.DiasMoraFin)
                    ?? categorias[^1];
                var prestamo = prestamos[m.IdPrestamo];
                return new CalificacionPrestamoItem(
                    prestamo.Numero, prestamo.TipoPrestamo.Nombre,
                    titulares.GetValueOrDefault(m.IdPrestamo, "—"),
                    m.Saldo, m.DiasMora, categoria.Codigo, categoria.Nombre,
                    categoria.PorcentajeProvision, m.Saldo * categoria.PorcentajeProvision);
            })
            .ToList();

        return Ok(resultado);
    }

    // Colocacion.GastosJudicialesPorPrestamo — rubros reales clasificados
    // como GAJ (Certificado de Gravamen, Notificaciones, Demanda Judicial,
    // Cobranzas, Honorario de Abogados, Gastos — catálogo real ya
    // sembrado, ver CLAUDE.md "Motor de rubros real"), nunca solo el
    // rubro puntual "Gastos Judiciales" — la clasificación GAJ es la
    // agrupación real que usa el propio catálogo de Softbank.
    [HttpGet("reportes/gastos-judiciales")]
    public async Task<ActionResult<IReadOnlyList<GastoJudicialItem>>> ReporteGastosJudiciales(
        [FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta, CancellationToken cancellationToken)
    {
        var query = db.PrestamosRubros
            .Include(r => r.Prestamo).ThenInclude(p => p.TipoPrestamo)
            .Include(r => r.Rubro)
            .Where(r => r.Rubro.CodigoTipoRubro == "GAJ" && r.Estado != "C");

        if (desde is not null) query = query.Where(r => r.FechaInicio >= desde);
        if (hasta is not null) query = query.Where(r => r.FechaInicio <= hasta);

        var resultado = await query
            .OrderByDescending(r => r.FechaInicio)
            .Select(r => new GastoJudicialItem(
                r.Prestamo.Numero, r.Prestamo.TipoPrestamo.Nombre,
                db.PrestamosClientes.Where(pc => pc.IdPrestamo == r.IdPrestamo && pc.Principal)
                    .Select(pc => pc.Cliente.Persona.Nombre).FirstOrDefault() ?? "—",
                r.Rubro.Nombre, r.Proyectado, r.Cobrado, r.FechaInicio, r.Estado))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    // Colocacion.ConsolidadoTipoCartera — cartera vigente agrupada por
    // producto. `IdTipoPrestamo` ya tiene índice real
    // (`ix_prestamo_id_tipo_prestamo`) y `Estado` recibió índice propio
    // en la ronda anterior — el query no necesitó ningún campo/índice
    // nuevo, ya estaba todo listo.
    [HttpGet("reportes/consolidado-tipo-cartera")]
    public async Task<ActionResult<IReadOnlyList<ConsolidadoTipoCarteraItem>>> ReporteConsolidadoTipoCartera(
        CancellationToken cancellationToken)
    {
        var resultado = await db.Prestamos
            .Where(p => p.Estado == EstadoPrestamo.Vigente)
            .GroupBy(p => p.TipoPrestamo.Nombre)
            .OrderByDescending(g => g.Sum(p => p.Saldo))
            .Select(g => new ConsolidadoTipoCarteraItem(
                g.Key, g.Count(), g.Sum(p => p.Saldo), g.Average(p => p.Tasa)))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    // Colocacion.ConsolidadoSeguroDesgravamen — préstamos vigentes con
    // saldo pendiente real del rubro Seguro Desgravamen (CodigoTipoRubro
    // = 'SEG', Estado='P'). Mismo índice compuesto real
    // (IdPrestamo,Estado,FechaFin) de la ronda anterior ya cubre el
    // filtro por Estado; se suma aquí ordenado por préstamo, no por
    // fecha, así que no requiere índice adicional.
    [HttpGet("reportes/consolidado-seguro-desgravamen")]
    public async Task<ActionResult<IReadOnlyList<ConsolidadoSeguroDesgravamenItem>>> ReporteConsolidadoSeguroDesgravamen(
        CancellationToken cancellationToken)
    {
        var resultado = await db.PrestamosRubros
            .Include(r => r.Prestamo).ThenInclude(p => p.TipoPrestamo)
            .Include(r => r.Rubro)
            .Where(r => r.Rubro.CodigoTipoRubro == "SEG" && r.Estado == "P"
                && r.Prestamo.Estado == EstadoPrestamo.Vigente)
            .GroupBy(r => new { r.IdPrestamo, r.Prestamo.Numero, Producto = r.Prestamo.TipoPrestamo.Nombre })
            .Select(g => new { g.Key, SaldoPendiente = g.Sum(r => r.Proyectado - r.Cobrado) })
            .OrderByDescending(x => x.SaldoPendiente)
            .ToListAsync(cancellationToken);

        var idsPrestamo = resultado.Select(r => r.Key.IdPrestamo).ToList();
        var titulares = await db.PrestamosClientes
            .Where(pc => pc.Principal && idsPrestamo.Contains(pc.IdPrestamo))
            .Select(pc => new { pc.IdPrestamo, Nombre = pc.Cliente.Persona.Nombre })
            .ToDictionaryAsync(x => x.IdPrestamo, x => x.Nombre, cancellationToken);

        var items = resultado
            .Select(r => new ConsolidadoSeguroDesgravamenItem(
                r.Key.Numero, r.Key.Producto, titulares.GetValueOrDefault(r.Key.IdPrestamo, "—"), r.SaldoPendiente))
            .ToList();

        return Ok(items);
    }

    [HttpGet("reportes/anexo-garantias")]
    public async Task<ActionResult<IReadOnlyList<AnexoGarantiaItem>>> ReporteAnexoGarantias(CancellationToken cancellationToken)
    {
        var resultado = await db.PrestamosGarantias
            .Include(g => g.Prestamo).ThenInclude(p => p.TipoPrestamo)
            .Include(g => g.ClienteGarante).ThenInclude(c => c.Persona)
            .Include(g => g.EstadoGarantia)
            .Where(g => g.CodigoEstadoGarantia == "A")
            .OrderBy(g => g.Prestamo.Numero)
            .Select(g => new AnexoGarantiaItem(
                g.Prestamo.Numero, g.Prestamo.TipoPrestamo.Nombre, g.Prestamo.Saldo,
                db.PrestamosClientes.Where(pc => pc.IdPrestamo == g.IdPrestamo && pc.Principal)
                    .Select(pc => pc.Cliente.Persona.Nombre).FirstOrDefault() ?? "—",
                g.ClienteGarante.Persona.Nombre, g.EstadoGarantia.Detalle))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpPost("solicitudes")]
    public async Task<ActionResult<SolicitudPrestamoCreadaResult>> Solicitar(
        [FromBody] SolicitarPrestamoBody body, CancellationToken cancellationToken)
    {
        var resultado = await prestamoService.SolicitarAsync(
            new SolicitarPrestamoRequest(
                body.IdCliente, body.IdTipoPrestamo, body.IdAgencia, body.MontoSolicitado, body.Cuotas,
                User.Identity!.Name!, body.CodigoTipoConvenio),
            cancellationToken);
        return Created($"/api/creditos/solicitudes/{resultado.IdSolicitud}", resultado);
    }

    [HttpPost("solicitudes/{idSolicitud:guid}/aprobar")]
    public async Task<ActionResult<SolicitudAprobadaResult>> AprobarSolicitud(
        Guid idSolicitud, [FromBody] AprobarSolicitudBody body, CancellationToken cancellationToken)
    {
        var resultado = await prestamoService.AprobarSolicitudAsync(
            new AprobarSolicitudRequest(idSolicitud, body.MontoAprobado, body.Comentario, User.Identity!.Name!), cancellationToken);
        return Ok(resultado);
    }

    [HttpPost("solicitudes/{idSolicitud:guid}/rechazar")]
    public async Task<IActionResult> RechazarSolicitud(
        Guid idSolicitud, [FromBody] RechazarSolicitudBody body, CancellationToken cancellationToken)
    {
        await prestamoService.RechazarSolicitudAsync(
            new RechazarSolicitudRequest(idSolicitud, body.Comentario, User.Identity!.Name!), cancellationToken);
        return NoContent();
    }

    [HttpGet("solicitudes/{idSolicitud:guid}/garantes")]
    public async Task<ActionResult<IReadOnlyList<GaranteDetalle>>> GarantesDeSolicitud(Guid idSolicitud, CancellationToken cancellationToken)
    {
        var resultado = await garantiaService.ListarPorSolicitudAsync(idSolicitud, cancellationToken);
        return Ok(resultado);
    }

    [HttpPost("solicitudes/{idSolicitud:guid}/garantes")]
    public async Task<ActionResult<GaranteAgregadoResult>> AgregarGarante(
        Guid idSolicitud, [FromBody] AgregarGaranteBody body, CancellationToken cancellationToken)
    {
        var resultado = await garantiaService.AgregarGaranteAsync(
            new AgregarGaranteRequest(idSolicitud, body.IdClienteGarante, body.Detalle, User.Identity!.Name!), cancellationToken);
        return Created($"/api/creditos/solicitudes/{idSolicitud}/garantes", resultado);
    }

    [HttpDelete("garantes/{idGarantia:guid}")]
    public async Task<IActionResult> QuitarGarante(Guid idGarantia, CancellationToken cancellationToken)
    {
        await garantiaService.QuitarGaranteAsync(new QuitarGaranteRequest(idGarantia, User.Identity!.Name!), cancellationToken);
        return NoContent();
    }

    [HttpGet("prestamos/{idPrestamo:guid}/garantes")]
    public async Task<ActionResult<IReadOnlyList<GaranteDetalle>>> GarantesDePrestamo(Guid idPrestamo, CancellationToken cancellationToken)
    {
        var resultado = await garantiaService.ListarPorPrestamoAsync(idPrestamo, cancellationToken);
        return Ok(resultado);
    }

    [HttpPost("solicitudes/{idSolicitud:guid}/desembolsar")]
    [RequireIdempotencyKey]
    public async Task<ActionResult<PrestamoDesembolsadoResult>> Desembolsar(
        Guid idSolicitud, CancellationToken cancellationToken)
    {
        var resultado = await prestamoService.DesembolsarAsync(
            new DesembolsarPrestamoRequest(idSolicitud, User.Identity!.Name!), cancellationToken);
        return Ok(resultado);
    }

    [HttpPost("prestamos/{idPrestamo:guid}/pagos")]
    [RequireIdempotencyKey]
    public async Task<ActionResult<PagoCuotaRegistradoResult>> PagarCuota(
        Guid idPrestamo, CancellationToken cancellationToken)
    {
        var resultado = await prestamoService.PagarCuotaAsync(
            new PagarCuotaRequest(idPrestamo, User.Identity!.Name!), cancellationToken);
        return Ok(resultado);
    }

    [HttpPost("provision-cartera/calcular")]
    public async Task<ActionResult<ProvisionCarteraCalculadaResult>> CalcularProvisionCartera(CancellationToken cancellationToken)
    {
        var resultado = await provisionCarteraService.EjecutarCalculoAsync(User.Identity!.Name!, cancellationToken);
        return Ok(resultado);
    }

    [HttpPost("clientes/{idCliente:guid}/score")]
    public async Task<ActionResult<ScoreCrediticioResult>> CalcularScore(Guid idCliente, CancellationToken cancellationToken)
    {
        var resultado = await scoreCrediticioService.CalcularAsync(idCliente, cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("clientes/{idCliente:guid}/score/historial")]
    public async Task<ActionResult<IReadOnlyList<ScoreCrediticioResult>>> HistorialScore(Guid idCliente, CancellationToken cancellationToken)
    {
        var resultado = await scoreCrediticioService.HistorialAsync(idCliente, cancellationToken);
        return Ok(resultado);
    }

    [HttpPatch("prestamos/{idPrestamo:guid}/debito-spi")]
    public async Task<IActionResult> ConfigurarDebitoSpi(
        Guid idPrestamo, [FromBody] ConfigurarDebitoSpiBody body, CancellationToken cancellationToken)
    {
        await autoDebitoSpiService.ConfigurarAsync(idPrestamo, body.Activar, User.Identity!.Name!, cancellationToken);
        return NoContent();
    }

    [HttpPost("auto-debito-spi/ejecutar")]
    public async Task<ActionResult<AutoDebitoSpiEjecutadoResult>> EjecutarAutoDebitoSpi(CancellationToken cancellationToken)
    {
        var resultado = await autoDebitoSpiService.EjecutarAsync(User.Identity!.Name!, cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("auto-debito-spi/historial")]
    public async Task<ActionResult<IReadOnlyList<AutoDebitoSpiDetalle>>> HistorialAutoDebitoSpi(CancellationToken cancellationToken)
    {
        var resultado = await autoDebitoSpiService.HistorialAsync(cancellationToken);
        return Ok(resultado);
    }

    // Motor de rubros real (ver CLAUDE.md, "Motor de rubros real") — solo
    // los rubros reales activos que generan cuenta por cobrar (o que son
    // cargables manualmente en general) se ofrecen para el selector, nunca
    // Capital/Interés (esos son exclusivos de la tabla de amortización).
    [HttpGet("rubros-manuales-disponibles")]
    public async Task<ActionResult<IReadOnlyList<RubroManualDisponibleItem>>> RubrosManualesDisponibles(CancellationToken cancellationToken)
    {
        var resultado = await db.Rubros
            .Include(r => r.TipoRubro)
            .Where(r => r.Activo && !r.TipoRubro.EsCapital && !r.TipoRubro.EsTasa)
            .OrderBy(r => r.OrdenDeCobro).ThenBy(r => r.Nombre)
            .Select(r => new RubroManualDisponibleItem(r.Id, r.Nombre, r.EsCuentaPorCobrar))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpGet("prestamos/{idPrestamo:guid}/rubros-manuales")]
    public async Task<ActionResult<IReadOnlyList<RubroManualCargadoResult>>> RubrosManuales(
        Guid idPrestamo, CancellationToken cancellationToken)
    {
        var resultado = await prestamoService.ListarRubrosManualesAsync(idPrestamo, cancellationToken);
        return Ok(resultado);
    }

    [HttpPost("prestamos/{idPrestamo:guid}/rubros-manuales")]
    [RequireIdempotencyKey]
    public async Task<ActionResult<RubroManualCargadoResult>> CargarRubroManual(
        Guid idPrestamo, [FromBody] CargarRubroManualBody body, CancellationToken cancellationToken)
    {
        var resultado = await prestamoService.CargarRubroManualAsync(
            new CargarRubroManualRequest(idPrestamo, body.IdRubro, body.Monto, body.Detalle, User.Identity!.Name!),
            cancellationToken);
        return Created($"/api/creditos/prestamos/{idPrestamo}/rubros-manuales", resultado);
    }

    // Diferimiento de cuotas (período de gracia real)

    [HttpGet("prestamos/{idPrestamo:guid}/diferimientos")]
    public async Task<ActionResult<IReadOnlyList<DiferimientoCuotaResult>>> Diferimientos(
        Guid idPrestamo, CancellationToken cancellationToken)
    {
        return Ok(await prestamoService.ListarDiferimientosAsync(idPrestamo, cancellationToken));
    }

    [HttpPost("prestamos/{idPrestamo:guid}/diferimientos")]
    public async Task<ActionResult<DiferimientoCuotaResult>> DiferirCuotas(
        Guid idPrestamo, [FromBody] DiferirCuotasBody body, CancellationToken cancellationToken)
    {
        var resultado = await prestamoService.DiferirCuotasAsync(
            new DiferirCuotasRequest(idPrestamo, body.DiasDiferidos, body.Comentario, User.Identity!.Name!),
            cancellationToken);
        return Created($"/api/creditos/prestamos/{idPrestamo}/diferimientos", resultado);
    }

    // Castigo formal de cartera

    [HttpPost("prestamos/{idPrestamo:guid}/castigar")]
    [RequireIdempotencyKey]
    public async Task<ActionResult<PrestamoCastigadoResult>> Castigar(
        Guid idPrestamo, [FromBody] CastigarPrestamoBody body, CancellationToken cancellationToken)
    {
        var resultado = await prestamoService.CastigarAsync(
            new CastigarPrestamoRequest(idPrestamo, body.Comentario, User.Identity!.Name!),
            cancellationToken);
        return Ok(resultado);
    }

    // Custodia física de pagaré (título valor)

    [HttpGet("prestamos/{idPrestamo:guid}/custodia-pagare")]
    public async Task<ActionResult<PagareCustodiaItem>> CustodiaPagare(Guid idPrestamo, CancellationToken cancellationToken)
    {
        var custodia = await db.PagaresCustodia
            .Include(c => c.Estado)
            .Include(c => c.Movimientos)
            .FirstOrDefaultAsync(c => c.IdPrestamo == idPrestamo, cancellationToken);
        if (custodia is null) return NotFound();

        return Ok(new PagareCustodiaItem(
            custodia.Id, custodia.CodigoEstado, custodia.Estado.Nombre, custodia.Ubicacion, custodia.FechaActualizacion,
            custodia.Movimientos.OrderByDescending(m => m.Fecha)
                .Select(m => new PagareCustodiaMovimientoItem(m.CodigoEstado, m.Ubicacion, m.EsRecepcion, m.Fecha, m.RegistradoPor))
                .ToList()));
    }

    [HttpPost("prestamos/{idPrestamo:guid}/custodia-pagare")]
    public async Task<ActionResult<PagareCustodiaItem>> RegistrarCustodiaPagare(
        Guid idPrestamo, [FromBody] RegistrarCustodiaPagareBody body, CancellationToken cancellationToken)
    {
        var prestamoExiste = await db.Prestamos.AnyAsync(p => p.Id == idPrestamo, cancellationToken);
        if (!prestamoExiste) return NotFound();

        var estadoValido = await db.EstadosCustodioPagare.AnyAsync(e => e.Codigo == body.CodigoEstado && e.Activo, cancellationToken);
        if (!estadoValido) return BadRequest(new { detail = $"El estado de custodio '{body.CodigoEstado}' no existe o no está activo." });

        var registradoPor = User.Identity?.Name ?? "sistema";
        var custodia = await db.PagaresCustodia.FirstOrDefaultAsync(c => c.IdPrestamo == idPrestamo, cancellationToken);
        if (custodia is null)
        {
            custodia = new PagareCustodia { Id = Guid.NewGuid(), IdPrestamo = idPrestamo };
            db.PagaresCustodia.Add(custodia);
        }

        custodia.CodigoEstado = body.CodigoEstado;
        custodia.Ubicacion = body.Ubicacion;
        custodia.FechaActualizacion = DateOnly.FromDateTime(DateTime.UtcNow);

        db.PagaresCustodiaMovimiento.Add(new PagareCustodiaMovimiento
        {
            Id = Guid.NewGuid(),
            IdPagareCustodia = custodia.Id,
            CodigoEstado = body.CodigoEstado,
            Ubicacion = body.Ubicacion,
            EsRecepcion = body.CodigoEstado == "R",
            Fecha = DateTimeOffset.UtcNow,
            RegistradoPor = registradoPor,
        });

        await db.SaveChangesAsync(cancellationToken);

        var estado = await db.EstadosCustodioPagare.FirstAsync(e => e.Codigo == body.CodigoEstado, cancellationToken);
        return Ok(new PagareCustodiaItem(custodia.Id, custodia.CodigoEstado, estado.Nombre, custodia.Ubicacion, custodia.FechaActualizacion, []));
    }
}

public record RubroManualDisponibleItem(int Id, string Nombre, bool EsCuentaPorCobrar);

public record CargarRubroManualBody(int IdRubro, decimal Monto, string Detalle);

public record ConfigurarDebitoSpiBody(bool Activar);

public record SolicitarPrestamoBody(
    Guid IdCliente, int IdTipoPrestamo, int IdAgencia, decimal MontoSolicitado, int Cuotas, string? CodigoTipoConvenio = null);

public record AprobarSolicitudBody(decimal MontoAprobado, string? Comentario);

public record RechazarSolicitudBody(string Comentario);

public record AgregarGaranteBody(Guid IdClienteGarante, string? Detalle);

public record ConcesionCreditoItem(
    string Numero, string Producto, string Cliente, decimal Monto, decimal Tasa,
    DateOnly FechaAdjudicacion, DateOnly FechaVencimiento, string Estado);

public record CreditoPrecanceladoItem(
    string Numero, string Producto, string Cliente, decimal Monto,
    DateOnly FechaAdjudicacion, DateOnly FechaVencimientoNominal, DateOnly FechaCancelacionReal);

public record ProximoVencimientoItem(
    string NumeroPrestamo, string Producto, string Cliente, int NumeroCuota, decimal MontoCuota, DateOnly FechaVencimiento);

public record CreditoCanceladoItem(
    string Numero, string Producto, string Cliente, decimal Monto, DateOnly FechaAdjudicacion, DateOnly FechaCancelacion);

public record AnexoGarantiaItem(string NumeroPrestamo, string Producto, decimal Saldo, string Titular, string Garante, string EstadoGarantia);

public record CreditoMoraAsesorItem(
    string NumeroPrestamo, string Producto, string Cliente, string? Asesor, decimal Saldo, int DiasMora);

public record IndiceMorosidadItem(
    decimal CarteraTotal, decimal CarteraVencida, decimal Indice, int TotalPrestamos, int PrestamosVencidos);

public record DebitoSpiNoProcesadoItem(
    string NumeroPrestamo, int? NumeroCuota, decimal Monto, string Motivo, DateOnly Fecha);

public record AnexoCarteraCastigadaItem(
    string NumeroPrestamo, string Producto, string Cliente, decimal SaldoTransferido,
    DateOnly Fecha, string Comentario, string RegistradoPor);

public record AnexoCarteraCastigadaAgenciaItem(string Agencia, int CantidadCastigos, decimal TotalCastigado);

public record AnexoCarteraCastigadaClienteItem(string Cliente, string Identificacion, int CantidadCastigos, decimal TotalCastigado);

public record AnexoItemCreditoItem(
    string NumeroPrestamo, int NumeroCuota, string Rubro, string CodigoTipoRubro,
    decimal Proyectado, decimal Cobrado, string Estado, DateOnly FechaVencimiento, DateOnly? FechaCobro);

public record CreditoVinculadoItem(
    string NumeroPrestamo, string Producto, string Cliente, decimal Saldo,
    string CodigoCausaVinculacion, string CausaVinculacion);

public record PrestamoPorConvenioItem(
    string NumeroPrestamo, string Producto, string Cliente, decimal Saldo,
    string CodigoTipoConvenio, string Convenio);

public record AbonoConvenioItem(
    string NumeroPrestamo, string Convenio, int NumeroCuota, decimal Capital, DateOnly Fecha);

public record CalificacionPrestamoItem(
    string NumeroPrestamo, string Producto, string Cliente, decimal Saldo, int DiasMora,
    string CodigoCategoria, string Categoria, decimal PorcentajeProvision, decimal ProvisionRequerida);

public record GastoJudicialItem(
    string NumeroPrestamo, string Producto, string Cliente, string Rubro,
    decimal Proyectado, decimal Cobrado, DateOnly Fecha, string Estado);

public record ConsolidadoTipoCarteraItem(
    string Producto, int CantidadPrestamos, decimal SaldoTotal, decimal TasaPromedio);

public record ConsolidadoSeguroDesgravamenItem(
    string NumeroPrestamo, string Producto, string Cliente, decimal SaldoPendiente);
