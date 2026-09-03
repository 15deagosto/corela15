using Corela15.Application.Cobranza;
using Corela15.Application.Colocacion;
using Corela15.Application.Cumplimiento;
using Corela15.Application.LavadoActivos;
using Corela15.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Api.Controllers;

public record AccionGestionListItem(int Id, string Codigo, string Nombre);

public record GestionCobranzaListItem(
    Guid Id, string PrestamoNumero, string Socio, string Accion, bool TieneCompromisoPago, string? Observacion, DateOnly Fecha);

public record PrestamoParaCobranzaListItem(
    Guid Id, Guid IdCliente, string Numero, string Socio, decimal Saldo, string Estado,
    int DiasMora, string CodigoPeriodoMora, string NombrePeriodoMora);

public record ResumenMoraTramoItem(string Codigo, string Nombre, int CantidadPrestamos, decimal SaldoTotal);

public record TipoListaControlListItem(string Codigo, string Nombre);

public record PersonaParaListaControlItem(Guid Id, string Nombre, string Identificacion);

public record UsuarioParaCumplimientoItem(Guid Id, string NombreUsuario);

public record EstadoHallazgoListItem(string Codigo, string Nombre);

public record HallazgoEtapaItem(string CodigoEstado, string Estado, string Comentario, string RegistradoPor, DateTimeOffset Fecha);

public record HallazgoRespuestaItem(string Respuesta, DateTimeOffset Fecha);

public record HallazgoUsuarioItem(Guid Id, string Usuario, bool EstadoRespondido, IReadOnlyList<HallazgoRespuestaItem> Respuestas);

public record HallazgoListItem(
    Guid Id, string Nombre, string Detalle, string ReportadoPor, string CodigoEstado, string Estado, DateOnly Fecha,
    IReadOnlyList<HallazgoUsuarioItem> Asignados, IReadOnlyList<HallazgoEtapaItem> Bitacora);

public record CrearHallazgoBody(string Nombre, string Detalle, Guid IdUsuarioReporta, IReadOnlyList<Guid> IdsUsuarioAsignado);

public record ResponderHallazgoBody(string Respuesta);

public record CambiarEstadoHallazgoBody(string CodigoEstadoNuevo, string Comentario);

[ApiController]
[Route("api/cobranzas")]
[Authorize(Policy = "Menu:cobranzas-cumplimiento")]
public class CobranzasController(
    Corela15DbContext db, IGestionCobranzaService gestionService, IMoraCarteraService moraCartera,
    IListaControlService listaControlService, IPerfilLavadoActivosService perfilLavadoActivosService,
    IGastoCobranzaService gastoCobranzaService, IHallazgoService hallazgoService) : ControllerBase
{
    [HttpGet("acciones")]
    public async Task<ActionResult<IReadOnlyList<AccionGestionListItem>>> Acciones(CancellationToken cancellationToken)
    {
        var resultado = await db.AccionesGestion
            .Where(a => a.Activo)
            .OrderBy(a => a.Nombre)
            .Select(a => new AccionGestionListItem(a.Id, a.Codigo, a.Nombre))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    // Solo préstamos con mora real (DiasMora > 0) — antes mostraba TODOS
    // los préstamos vigentes por igual, sin distinguir cuáles realmente
    // necesitan gestión de cobranza. El tramo (PeriodoMora, sembrado desde
    // Nivel 4 pero sin usar hasta ahora) sale del mismo cálculo real de
    // mora que usa el motor de provisiones — una sola fuente de verdad.
    [HttpGet("prestamos")]
    public async Task<ActionResult<IReadOnlyList<PrestamoParaCobranzaListItem>>> PrestamosVigentes(CancellationToken cancellationToken)
    {
        var moras = await moraCartera.CalcularAsync(cancellationToken);
        var tramos = await db.PeriodosMora
            .Where(t => t.Activo)
            .OrderBy(t => t.DiasInicio)
            .ToListAsync(cancellationToken);

        var titulares = await db.PrestamosClientes
            .Where(pc => pc.Principal)
            .Select(pc => new { pc.IdPrestamo, pc.IdCliente, Nombre = pc.Cliente.Persona.Nombre })
            .ToDictionaryAsync(x => x.IdPrestamo, cancellationToken);

        var resultado = moras
            .Where(m => m.DiasMora > 0)
            .OrderByDescending(m => m.DiasMora)
            .Select(m =>
            {
                var tramo = tramos.FirstOrDefault(t => m.DiasMora >= t.DiasInicio && m.DiasMora <= t.DiasFin) ?? tramos[^1];
                titulares.TryGetValue(m.IdPrestamo, out var titular);
                return new PrestamoParaCobranzaListItem(
                    m.IdPrestamo, titular?.IdCliente ?? Guid.Empty, m.Numero, titular?.Nombre ?? "—",
                    m.Saldo, "Vigente", m.DiasMora, tramo.Codigo, tramo.Nombre);
            })
            .ToList();

        return Ok(resultado);
    }

    [HttpGet("mora/resumen")]
    public async Task<ActionResult<IReadOnlyList<ResumenMoraTramoItem>>> ResumenMora(CancellationToken cancellationToken)
    {
        var moras = await moraCartera.CalcularAsync(cancellationToken);
        var tramos = await db.PeriodosMora
            .Where(t => t.Activo)
            .OrderBy(t => t.DiasInicio)
            .ToListAsync(cancellationToken);

        var resultado = tramos
            .Select(t =>
            {
                var enTramo = moras.Where(m => m.DiasMora >= t.DiasInicio && m.DiasMora <= t.DiasFin).ToList();
                return new ResumenMoraTramoItem(t.Codigo, t.Nombre, enTramo.Count, enTramo.Sum(m => m.Saldo));
            })
            .ToList();

        return Ok(resultado);
    }

    // Estimación informativa del gasto de cobranza extrajudicial por préstamo
    // vencido (tarifario real F01, ver TarifaGastoCobranza.cs) — no cobra
    // nada, solo informa cuánto le correspondería según el rango de cuota y
    // días de mora reales.
    [HttpGet("mora/gasto-cobranza")]
    public async Task<ActionResult<IReadOnlyList<GastoCobranzaEstimado>>> GastoCobranza(CancellationToken cancellationToken)
    {
        var resultado = await gastoCobranzaService.EstimarAsync(cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("gestiones")]
    public async Task<ActionResult<IReadOnlyList<GestionCobranzaListItem>>> Gestiones(CancellationToken cancellationToken)
    {
        var resultado = await db.GestionesPrestamoCobranza
            .Include(g => g.Prestamo)
            .Include(g => g.Cliente).ThenInclude(c => c.Persona)
            .Include(g => g.AccionGestion)
            .OrderByDescending(g => g.Fecha)
            .Select(g => new GestionCobranzaListItem(
                g.Id, g.Prestamo.Numero, g.Cliente.Persona.Nombre, g.AccionGestion.Nombre,
                g.TieneCompromisoPago, g.Observacion, g.Fecha))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpPost("gestiones")]
    public async Task<ActionResult<GestionCobranzaRegistradaResult>> Registrar(
        [FromBody] RegistrarGestionBody body, CancellationToken cancellationToken)
    {
        var resultado = await gestionService.RegistrarAsync(
            new RegistrarGestionCobranzaRequest(
                body.IdPrestamo, body.IdCliente, body.EsDeudor, body.CodigoAccionGestion,
                body.TieneCompromisoPago, body.Observacion, User.Identity!.Name!),
            cancellationToken);
        return Created($"/api/cobranzas/gestiones/{resultado.IdGestion}", resultado);
    }
    [HttpGet("listas-control/tipos")]
    public async Task<ActionResult<IReadOnlyList<TipoListaControlListItem>>> TiposListaControl(CancellationToken cancellationToken)
    {
        var resultado = await db.TiposListaControl
            .Where(t => t.Activo)
            .OrderBy(t => t.Codigo)
            .Select(t => new TipoListaControlListItem(t.Codigo, t.Nombre))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpGet("listas-control/personas")]
    public async Task<ActionResult<IReadOnlyList<PersonaParaListaControlItem>>> PersonasParaListaControl(
        [FromQuery] string? q, CancellationToken cancellationToken)
    {
        var query = db.Personas.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(p => EF.Functions.ILike(p.Nombre, $"%{q}%") || EF.Functions.ILike(p.Identificacion, $"%{q}%"));
        }

        var resultado = await query
            .OrderBy(p => p.Nombre)
            .Take(20)
            .Select(p => new PersonaParaListaControlItem(p.Id, p.Nombre, p.Identificacion))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpGet("listas-control/alertas")]
    public async Task<ActionResult<IReadOnlyList<AlertaListaControlDetalle>>> AlertasListaControl(
        [FromQuery] bool soloNoResueltas, CancellationToken cancellationToken)
    {
        var resultado = await listaControlService.ListarAsync(soloNoResueltas, cancellationToken);
        return Ok(resultado);
    }

    [HttpPost("listas-control/alertas")]
    public async Task<ActionResult<AlertaListaControlResult>> RegistrarAlertaListaControl(
        [FromBody] RegistrarAlertaListaControlBody body, CancellationToken cancellationToken)
    {
        var resultado = await listaControlService.RegistrarAlertaAsync(
            new RegistrarAlertaListaControlRequest(body.IdPersona, body.CodigoTipoListaControl, body.Detalle, User.Identity!.Name!),
            cancellationToken);
        return Created($"/api/cobranzas/listas-control/alertas/{resultado.Id}", resultado);
    }

    [HttpPost("listas-control/alertas/{idAlerta:guid}/resolver")]
    public async Task<IActionResult> ResolverAlertaListaControl(
        Guid idAlerta, [FromBody] ResolverAlertaListaControlBody body, CancellationToken cancellationToken)
    {
        await listaControlService.ResolverAlertaAsync(
            new ResolverAlertaListaControlRequest(idAlerta, body.Comentario, User.Identity!.Name!), cancellationToken);
        return NoContent();
    }

    [HttpPost("clientes/{idCliente:guid}/perfil-lavado")]
    public async Task<ActionResult<PerfilLavadoActivosResult>> CalcularPerfilLavado(Guid idCliente, CancellationToken cancellationToken)
    {
        var resultado = await perfilLavadoActivosService.CalcularAsync(idCliente, cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("clientes/{idCliente:guid}/perfil-lavado/historial")]
    public async Task<ActionResult<IReadOnlyList<PerfilLavadoActivosResult>>> HistorialPerfilLavado(Guid idCliente, CancellationToken cancellationToken)
    {
        var resultado = await perfilLavadoActivosService.HistorialAsync(idCliente, cancellationToken);
        return Ok(resultado);
    }

    // Hallazgos de auditoría/cumplimiento con ciclo de vida real — cierra
    // el gap documentado en 03-sbk-wpf-decompilado.md, ver Hallazgo.cs.
    [HttpGet("usuarios")]
    public async Task<ActionResult<IReadOnlyList<UsuarioParaCumplimientoItem>>> UsuariosParaCumplimiento(CancellationToken cancellationToken)
    {
        var resultado = await db.Usuarios
            .Where(u => u.Activo)
            .OrderBy(u => u.NombreUsuario)
            .Select(u => new UsuarioParaCumplimientoItem(u.Id, u.NombreUsuario))
            .ToListAsync(cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("estados-hallazgo")]
    public async Task<ActionResult<IReadOnlyList<EstadoHallazgoListItem>>> EstadosHallazgo(CancellationToken cancellationToken)
    {
        var resultado = await db.EstadosHallazgo
            .Where(e => e.Activo)
            .Select(e => new EstadoHallazgoListItem(e.Codigo, e.Nombre))
            .ToListAsync(cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("hallazgos")]
    public async Task<ActionResult<IReadOnlyList<HallazgoListItem>>> Hallazgos(CancellationToken cancellationToken)
    {
        var hallazgos = await db.Hallazgos
            .Include(h => h.UsuarioReporta)
            .Include(h => h.Estado)
            .OrderByDescending(h => h.Fecha)
            .ToListAsync(cancellationToken);

        var idsHallazgo = hallazgos.Select(h => h.Id).ToList();

        var asignados = await db.HallazgosUsuario
            .Include(hu => hu.Usuario)
            .Where(hu => idsHallazgo.Contains(hu.IdHallazgo) && hu.Activo)
            .ToListAsync(cancellationToken);
        var idsAsignado = asignados.Select(a => a.Id).ToList();

        var respuestas = await db.HallazgosUsuarioRespuesta
            .Where(r => idsAsignado.Contains(r.IdHallazgoUsuario))
            .OrderBy(r => r.Fecha)
            .ToListAsync(cancellationToken);

        var etapas = await db.HallazgosEtapa
            .Include(e => e.Estado)
            .Where(e => idsHallazgo.Contains(e.IdHallazgo))
            .OrderBy(e => e.Fecha)
            .ToListAsync(cancellationToken);

        var resultado = hallazgos.Select(h => new HallazgoListItem(
            h.Id, h.Nombre, h.Detalle, h.UsuarioReporta.NombreUsuario, h.CodigoEstado, h.Estado.Nombre, h.Fecha,
            asignados.Where(a => a.IdHallazgo == h.Id).Select(a => new HallazgoUsuarioItem(
                a.Id, a.Usuario.NombreUsuario, a.EstadoRespondido,
                respuestas.Where(r => r.IdHallazgoUsuario == a.Id)
                    .Select(r => new HallazgoRespuestaItem(r.Respuesta, r.Fecha)).ToList()))
                .ToList(),
            etapas.Where(e => e.IdHallazgo == h.Id)
                .Select(e => new HallazgoEtapaItem(e.CodigoEstado, e.Estado.Nombre, e.Comentario, e.RegistradoPor, e.Fecha))
                .ToList()))
            .ToList();

        return Ok(resultado);
    }

    [HttpPost("hallazgos")]
    public async Task<ActionResult<HallazgoCreadoResult>> CrearHallazgo(
        [FromBody] CrearHallazgoBody body, CancellationToken cancellationToken)
    {
        var resultado = await hallazgoService.CrearAsync(
            new CrearHallazgoRequest(body.Nombre, body.Detalle, body.IdUsuarioReporta, body.IdsUsuarioAsignado, User.Identity!.Name!),
            cancellationToken);
        return Created($"/api/cobranzas/hallazgos/{resultado.IdHallazgo}", resultado);
    }

    [HttpPost("hallazgos/asignaciones/{idHallazgoUsuario:guid}/responder")]
    public async Task<IActionResult> ResponderHallazgo(
        Guid idHallazgoUsuario, [FromBody] ResponderHallazgoBody body, CancellationToken cancellationToken)
    {
        await hallazgoService.ResponderAsync(new ResponderHallazgoRequest(idHallazgoUsuario, body.Respuesta), cancellationToken);
        return NoContent();
    }

    [HttpPost("hallazgos/{idHallazgo:guid}/estado")]
    public async Task<IActionResult> CambiarEstadoHallazgo(
        Guid idHallazgo, [FromBody] CambiarEstadoHallazgoBody body, CancellationToken cancellationToken)
    {
        await hallazgoService.CambiarEstadoAsync(
            new CambiarEstadoHallazgoRequest(idHallazgo, body.CodigoEstadoNuevo, body.Comentario, User.Identity!.Name!),
            cancellationToken);
        return NoContent();
    }
}

public record RegistrarAlertaListaControlBody(Guid IdPersona, string CodigoTipoListaControl, string Detalle);

public record ResolverAlertaListaControlBody(string Comentario);

public record RegistrarGestionBody(
    Guid IdPrestamo, Guid IdCliente, bool EsDeudor, string CodigoAccionGestion,
    bool TieneCompromisoPago, string? Observacion);
