using Corela15.Application.Riesgo;
using Corela15.Domain.Contabilidad;
using Corela15.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Api.Controllers;

public record LiquidezEstructuralResult(
    DateOnly FechaCorte,
    decimal Npl01, decimal Npl02, decimal Npl03, decimal Npl04, decimal Npl05, decimal Nplt,
    decimal Dpl01, decimal Dpl02, decimal Dpl03, decimal Dpl04, decimal Dplt,
    decimal Lpl,
    decimal Nsl01, decimal Nsl02, decimal Nsl03, decimal Nslt,
    decimal Dsl01, decimal Dsl02, decimal Dsl03, decimal Dslt,
    decimal Lsl);

public record ProcesoListItem(int Id, string Nombre, string MacroProceso, bool Critico);

public record NivelEscalaListItem(int Id, string Nombre, int Nivel);

public record EventoRiesgoListItem(
    Guid Id, string Proceso, string Descripcion, string NivelImpacto, string NivelProbabilidad,
    string NivelRiesgo, string ColorNivelRiesgo, DateOnly FechaIdentificacion);

public record EstadoAvanceRiesgoListItem(string Codigo, string Nombre);

public record AvanceRiesgoEtapaItem(string CodigoEstado, string Estado, string Comentario, string RegistradoPor, DateTimeOffset Fecha);

public record AvanceRiesgoDetalleItem(
    Guid IdAvanceRiesgoDetalle, string EventoDetectado, string PosibleCausa, string Tratamiento,
    string Inconvenientes, string AccionesSugeridas, IReadOnlyList<AvanceRiesgoEtapaItem> Bitacora);

public record AvanceRiesgoListItem(
    Guid Id, Guid IdEventoRiesgo, string EventoDescripcion, string Responsable, string CodigoEstado, string Estado,
    DateOnly Fecha, IReadOnlyList<AvanceRiesgoDetalleItem> Detalles);

public record CrearAvanceRiesgoBody(
    Guid IdEventoRiesgo, Guid IdUsuarioResponsable, DateTimeOffset? HoraInicio, DateTimeOffset? HoraFin,
    string EventoDetectado, string PosibleCausa, string Tratamiento, string Inconvenientes, string AccionesSugeridas);

public record CambiarEstadoAvanceRiesgoBody(string CodigoEstadoNuevo, string Comentario);

[ApiController]
[Route("api/riesgo")]
[Authorize(Policy = "Menu:riesgo")]
public class RiesgoController(
    Corela15DbContext db, IEventoRiesgoService service, IIndicadorLiquidezService indicadorLiquidezService,
    IAvanceRiesgoService avanceRiesgoService) : ControllerBase
{
    [HttpGet("procesos")]
    public async Task<ActionResult<IReadOnlyList<ProcesoListItem>>> Procesos(CancellationToken cancellationToken)
    {
        var resultado = await db.Procesos
            .Include(p => p.MacroProceso)
            .Where(p => p.Activo)
            .OrderBy(p => p.Nombre)
            .Select(p => new ProcesoListItem(p.Id, p.Nombre, p.MacroProceso.Nombre, p.Critico))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpGet("niveles-impacto")]
    public async Task<ActionResult<IReadOnlyList<NivelEscalaListItem>>> NivelesImpacto(CancellationToken cancellationToken)
    {
        var resultado = await db.NivelesImpacto
            .Where(n => n.Activo)
            .OrderBy(n => n.Nivel)
            .Select(n => new NivelEscalaListItem(n.Id, n.Nombre, n.Nivel))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpGet("niveles-probabilidad")]
    public async Task<ActionResult<IReadOnlyList<NivelEscalaListItem>>> NivelesProbabilidad(CancellationToken cancellationToken)
    {
        var resultado = await db.NivelesProbabilidad
            .Where(n => n.Activo)
            .OrderBy(n => n.Nivel)
            .Select(n => new NivelEscalaListItem(n.Id, n.Nombre, n.Nivel))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpGet("eventos")]
    public async Task<ActionResult<IReadOnlyList<EventoRiesgoListItem>>> Eventos(CancellationToken cancellationToken)
    {
        var resultado = await db.EventosRiesgo
            .Include(e => e.Proceso)
            .Include(e => e.NivelImpacto)
            .Include(e => e.NivelProbabilidad)
            .Include(e => e.NivelRiesgo)
            .OrderByDescending(e => e.FechaIdentificacion)
            .Select(e => new EventoRiesgoListItem(
                e.Id, e.Proceso.Nombre, e.Descripcion, e.NivelImpacto.Nombre, e.NivelProbabilidad.Nombre,
                e.NivelRiesgo.Nombre, e.NivelRiesgo.Color ?? "#999999", e.FechaIdentificacion))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpPost("eventos")]
    public async Task<ActionResult<EventoRiesgoRegistradoResult>> Registrar(
        [FromBody] RegistrarEventoRiesgoRequest request, CancellationToken cancellationToken)
    {
        var resultado = await service.RegistrarAsync(request, cancellationToken);
        return Created($"/api/riesgo/eventos/{resultado.IdEventoRiesgo}", resultado);
    }

    [HttpGet("usuarios")]
    public async Task<ActionResult<IReadOnlyList<UsuarioParaCajaListItem>>> Usuarios(CancellationToken cancellationToken)
    {
        var resultado = await db.Usuarios
            .Where(u => u.Activo)
            .OrderBy(u => u.NombreUsuario)
            .Select(u => new UsuarioParaCajaListItem(u.Id, u.NombreUsuario))
            .ToListAsync(cancellationToken);
        return Ok(resultado);
    }

    // Plan de acción real sobre un evento de riesgo — cierra el gap
    // documentado desde Nivel 8, ver AvanceRiesgo.cs.
    [HttpGet("avances")]
    public async Task<ActionResult<IReadOnlyList<AvanceRiesgoListItem>>> Avances(CancellationToken cancellationToken)
    {
        var avances = await db.AvancesRiesgo
            .Include(a => a.EventoRiesgo)
            .Include(a => a.UsuarioResponsable)
            .Include(a => a.Estado)
            .OrderByDescending(a => a.Fecha)
            .ToListAsync(cancellationToken);

        var idsAvance = avances.Select(a => a.Id).ToList();
        var detalles = await db.AvancesRiesgoDetalle
            .Where(d => idsAvance.Contains(d.IdAvanceRiesgo))
            .ToListAsync(cancellationToken);
        var idsDetalle = detalles.Select(d => d.Id).ToList();
        var etapas = await db.AvancesRiesgoEtapa
            .Include(e => e.Estado)
            .Where(e => idsDetalle.Contains(e.IdAvanceRiesgoDetalle))
            .OrderBy(e => e.Fecha)
            .ToListAsync(cancellationToken);

        var resultado = avances.Select(a => new AvanceRiesgoListItem(
            a.Id, a.IdEventoRiesgo, a.EventoRiesgo.Descripcion, a.UsuarioResponsable.NombreUsuario,
            a.CodigoEstado, a.Estado.Nombre, a.Fecha,
            detalles.Where(d => d.IdAvanceRiesgo == a.Id).Select(d => new AvanceRiesgoDetalleItem(
                d.Id, d.EventoDetectado, d.PosibleCausa, d.Tratamiento, d.Inconvenientes, d.AccionesSugeridas,
                etapas.Where(e => e.IdAvanceRiesgoDetalle == d.Id)
                    .Select(e => new AvanceRiesgoEtapaItem(e.CodigoEstado, e.Estado.Nombre, e.Comentario, e.RegistradoPor, e.Fecha))
                    .ToList()))
                .ToList()))
            .ToList();

        return Ok(resultado);
    }

    [HttpGet("estados-avance-riesgo")]
    public async Task<ActionResult<IReadOnlyList<EstadoAvanceRiesgoListItem>>> EstadosAvanceRiesgo(CancellationToken cancellationToken)
    {
        var resultado = await db.EstadosAvanceRiesgo
            .Where(e => e.Activo)
            .Select(e => new EstadoAvanceRiesgoListItem(e.Codigo, e.Nombre))
            .ToListAsync(cancellationToken);
        return Ok(resultado);
    }

    [HttpPost("avances")]
    public async Task<ActionResult<AvanceRiesgoCreadoResult>> CrearAvance(
        [FromBody] CrearAvanceRiesgoBody body, CancellationToken cancellationToken)
    {
        var resultado = await avanceRiesgoService.CrearAsync(
            new CrearAvanceRiesgoRequest(
                body.IdEventoRiesgo, body.IdUsuarioResponsable, body.HoraInicio, body.HoraFin,
                body.EventoDetectado, body.PosibleCausa, body.Tratamiento, body.Inconvenientes, body.AccionesSugeridas,
                User.Identity!.Name!),
            cancellationToken);
        return Created($"/api/riesgo/avances/{resultado.IdAvanceRiesgo}", resultado);
    }

    [HttpPost("avances/detalles/{idDetalle:guid}/estado")]
    public async Task<IActionResult> CambiarEstadoAvance(
        Guid idDetalle, [FromBody] CambiarEstadoAvanceRiesgoBody body, CancellationToken cancellationToken)
    {
        await avanceRiesgoService.CambiarEstadoAsync(
            new CambiarEstadoAvanceRiesgoRequest(idDetalle, body.CodigoEstadoNuevo, body.Comentario, User.Identity!.Name!),
            cancellationToken);
        return NoContent();
    }

    [HttpGet("liquidez")]
    public async Task<ActionResult<IReadOnlyList<IndicadorLiquidezResult>>> HistoricoLiquidez(CancellationToken cancellationToken)
    {
        var resultado = await indicadorLiquidezService.ListarHistoricoAsync(cancellationToken);
        return Ok(resultado);
    }

    [HttpPost("liquidez/calcular")]
    public async Task<ActionResult<IndicadorLiquidezResult>> CalcularLiquidez(CancellationToken cancellationToken)
    {
        var resultado = await indicadorLiquidezService.CalcularAsync(cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("liquidez/parametro")]
    public async Task<ActionResult<ParametroLiquidezResult>> ParametroLiquidez(CancellationToken cancellationToken)
    {
        var resultado = await indicadorLiquidezService.ObtenerParametroAsync(cancellationToken);
        return Ok(resultado);
    }

    [HttpPut("liquidez/parametro")]
    public async Task<ActionResult<ParametroLiquidezResult>> ActualizarParametroLiquidez(
        [FromBody] ActualizarParametroLiquidezBody body, CancellationToken cancellationToken)
    {
        var resultado = await indicadorLiquidezService.ActualizarParametroAsync(body.MinimoRegulatorio, User.Identity!.Name!, cancellationToken);
        return Ok(resultado);
    }

    // Fórmula real de Liquidez Estructural (L01), verificada contra el
    // "Manual Técnico de Estructuras de Datos — Riesgo de Liquidez" (v5.0,
    // SEPS) y contra el catálogo CUC real ya sembrado (todos los códigos
    // usados acá existen y sus nombres coinciden exactamente con las
    // categorías del manual — confirmación cruzada real, no una lista
    // inventada). DPL03 excluye los códigos 261005/261010/261015/261090
    // (solo aplican a Segmento 1/Caja Central/Mutualistas — esta
    // cooperativa es Segmento 2). VOLGEN/VOLPL/VOLSL/VOLABS/CONC/MAYREQ/IML
    // (el "Indicador Mínimo de Liquidez" final) NO se calculan: VOLGEN
    // depende de una "Nota Técnica" de metodología propia de la entidad,
    // un documento que no está entre los manuales descargados — inventar
    // esa metodología sería exactamente el error que este proyecto evita
    // siempre. LPL/LSL (Liquidez de primera/segunda línea) sí son 100%
    // reales y verificables, y reemplazan como indicador de referencia al
    // coeficiente simplificado anterior (grupo 11 / grupo 21, documentado
    // desde el inicio como "pendiente de verificación oficial" — ya no
    // hace falta esa reserva para LPL/LSL).
    private static readonly (string Codigo, decimal Signo)[] CodigosNpl03 =
        [("130105", 1), ("130110", 1), ("130150", 1), ("130155", 1), ("130205", 1), ("130210", 1)];
    private static readonly (string Codigo, decimal Signo)[] CodigosNpl04 =
        [("130305", 1), ("130310", 1), ("130350", 1), ("130355", 1), ("130405", 1), ("130410", 1)];
    private static readonly (string Codigo, decimal Signo)[] CodigosDpl03 =
        [("2601", 1), ("260205", 1), ("260210", 1), ("260250", 1), ("260255", 1), ("260305", 1), ("260310", 1),
         ("260450", 1), ("260455", 1), ("260605", 1), ("260610", 1), ("260705", 1), ("260710", 1)];
    private static readonly (string Codigo, decimal Signo)[] CodigosNsl01 = [("130115", 1), ("130160", 1), ("130215", 1)];
    private static readonly (string Codigo, decimal Signo)[] CodigosNsl02 = [("130315", 1), ("130360", 1), ("130415", 1)];
    private static readonly (string Codigo, decimal Signo)[] CodigosNsl03 =
        [("130505", 1), ("130510", 1), ("130515", 1), ("130550", 1), ("130555", 1), ("130560", 1),
         ("130605", 1), ("130610", 1), ("130615", 1)];
    private static readonly (string Codigo, decimal Signo)[] CodigosDsl03 =
        [("260215", 1), ("260220", 1), ("260260", 1), ("260265", 1), ("260315", 1), ("260320", 1),
         ("260460", 1), ("260465", 1), ("260615", 1), ("260620", 1), ("260715", 1), ("260720", 1)];

    [HttpGet("liquidez-estructural")]
    public async Task<ActionResult<LiquidezEstructuralResult>> LiquidezEstructural(CancellationToken cancellationToken)
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var periodoActual = new DateOnly(hoy.Year, hoy.Month, 1);

        var saldosLeaf = await db.SaldosContables
            .Where(s => s.Periodo <= periodoActual)
            .GroupBy(s => s.IdCuentaContable)
            .Select(g => new { IdCuentaContable = g.Key, SaldoAcumulado = g.Sum(s => s.SaldoFinal) })
            .ToDictionaryAsync(x => x.IdCuentaContable, x => x.SaldoAcumulado, cancellationToken);

        var todasLasCuentas = await db.CuentasContables.Where(c => c.Activa).ToListAsync(cancellationToken);
        var porCodigo = todasLasCuentas.ToDictionary(c => c.Codigo);
        var hijosPorPadre = todasLasCuentas
            .Where(c => c.IdCuentaPadre.HasValue)
            .GroupBy(c => c.IdCuentaPadre!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());
        var saldoCalculado = new Dictionary<Guid, decimal>();

        decimal CalcularSaldo(CuentaContable cuenta)
        {
            if (saldoCalculado.TryGetValue(cuenta.Id, out var yaCalculado)) return yaCalculado;
            var resultado = cuenta.EsMayor
                ? saldosLeaf.GetValueOrDefault(cuenta.Id, 0m)
                : hijosPorPadre.GetValueOrDefault(cuenta.Id, []).Sum(CalcularSaldo);
            saldoCalculado[cuenta.Id] = resultado;
            return resultado;
        }

        decimal Saldo(string codigo) => porCodigo.TryGetValue(codigo, out var c) ? CalcularSaldo(c) : 0m;
        decimal SumaCodigos((string Codigo, decimal Signo)[] codigos) => codigos.Sum(x => Saldo(x.Codigo) * x.Signo);

        var npl01 = Saldo("11") - Saldo("1105");
        var npl02 = Saldo("1201") - Saldo("2201") + Saldo("1202") - Saldo("2102") - Saldo("2202");
        var npl03 = SumaCodigos(CodigosNpl03);
        var npl04 = SumaCodigos(CodigosNpl04);
        var npl05 = Saldo("190286");
        var nplt = npl01 + npl02 + npl03 + npl04 + npl05;

        var dpl01 = Saldo("2101");
        var dpl02 = Saldo("210305") + Saldo("210310");
        var dpl03 = SumaCodigos(CodigosDpl03);
        var dpl04 = Saldo("23") + Saldo("27") + Saldo("2903");
        var dplt = dpl01 + dpl02 + dpl03 + dpl04;

        var lpl = dplt == 0 ? 0 : nplt / dplt;

        var nsl01 = SumaCodigos(CodigosNsl01);
        var nsl02 = SumaCodigos(CodigosNsl02);
        var nsl03 = SumaCodigos(CodigosNsl03);
        var nslt = nplt + nsl01 + nsl02 + nsl03;

        var dsl01 = Saldo("2103") - Saldo("210305") - Saldo("210310");
        var dsl02 = Saldo("2105");
        var dsl03 = SumaCodigos(CodigosDsl03);
        var dslt = dplt + dsl01 + dsl02 + dsl03;

        var lsl = dslt == 0 ? 0 : nslt / dslt;

        return Ok(new LiquidezEstructuralResult(
            hoy, npl01, npl02, npl03, npl04, npl05, nplt, dpl01, dpl02, dpl03, dpl04, dplt, lpl,
            nsl01, nsl02, nsl03, nslt, dsl01, dsl02, dsl03, dslt, lsl));
    }
}

public record ActualizarParametroLiquidezBody(decimal MinimoRegulatorio);
