using Corela15.Api.Idempotencia;
using Corela15.Application.Proveeduria;
using Corela15.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Api.Controllers;

public record TipoArticuloListItem(string Codigo, string Nombre);

public record ArticuloListItem(string Codigo, string Nombre, string TipoArticulo, string? Marca, string Multiplo, bool Activo);

public record BodegaListItem(Guid Id, string Agencia, string Responsable, bool Activo);

public record UsuarioParaBodegaListItem(Guid Id, string NombreUsuario);

public record StockBodegaListItem(
    Guid Id, string CodigoArticulo, string NombreArticulo, int Cantidad, decimal PrecioUnitario, decimal ValorTotal);

public record SolicitudPedidoListItem(
    Guid Id, string Bodega, string Solicitante, string? Detalle, string Estado, DateTimeOffset FechaSistema, decimal ValorEstimado);

public record SolicitudPedidoDetalleLinea(string CodigoArticulo, string NombreArticulo, int Cantidad, decimal PrecioUnitario, string? Detalle);

[ApiController]
[Route("api/proveeduria")]
[Authorize(Policy = "Menu:proveeduria")]
public class ProveeduriaController(Corela15DbContext db, ISolicitudPedidoService solicitudPedidoService) : ControllerBase
{
    [HttpGet("tipos-articulo")]
    public async Task<ActionResult<IReadOnlyList<TipoArticuloListItem>>> TiposArticulo(CancellationToken ct) =>
        Ok(await db.TiposArticulo.Where(t => t.Activo).OrderBy(t => t.Nombre)
            .Select(t => new TipoArticuloListItem(t.Codigo, t.Nombre)).ToListAsync(ct));

    [HttpGet("articulos")]
    public async Task<ActionResult<IReadOnlyList<ArticuloListItem>>> Articulos(CancellationToken ct) =>
        Ok(await db.Articulos.Include(a => a.TipoArticulo).Where(a => a.Activo).OrderBy(a => a.Nombre)
            .Select(a => new ArticuloListItem(a.Codigo, a.Nombre, a.TipoArticulo.Nombre, a.Marca, a.Multiplo, a.Activo))
            .ToListAsync(ct));

    [HttpGet("usuarios")]
    public async Task<ActionResult<IReadOnlyList<UsuarioParaBodegaListItem>>> Usuarios(CancellationToken ct) =>
        Ok(await db.Usuarios.Where(u => u.Activo).OrderBy(u => u.NombreUsuario)
            .Select(u => new UsuarioParaBodegaListItem(u.Id, u.NombreUsuario)).ToListAsync(ct));

    [HttpGet("bodegas")]
    public async Task<ActionResult<IReadOnlyList<BodegaListItem>>> Bodegas(CancellationToken ct) =>
        Ok(await db.Bodegas.Include(b => b.Agencia).Include(b => b.UsuarioResponsable)
            .OrderBy(b => b.Agencia.Nombre)
            .Select(b => new BodegaListItem(b.Id, b.Agencia.Nombre, b.UsuarioResponsable.NombreUsuario, b.Activo))
            .ToListAsync(ct));

    [HttpPost("bodegas")]
    public async Task<ActionResult<BodegaListItem>> CrearBodega([FromBody] CrearBodegaBody body, CancellationToken ct)
    {
        var bodega = new Corela15.Domain.Proveeduria.Bodega
        {
            Id = Guid.NewGuid(),
            IdAgencia = body.IdAgencia,
            IdUsuarioResponsable = body.IdUsuarioResponsable,
            Activo = true,
        };
        db.Bodegas.Add(bodega);
        await db.SaveChangesAsync(ct);

        var agencia = await db.Agencias.Where(a => a.Id == body.IdAgencia).Select(a => a.Nombre).FirstAsync(ct);
        var usuario = await db.Usuarios.Where(u => u.Id == body.IdUsuarioResponsable).Select(u => u.NombreUsuario).FirstAsync(ct);
        return Created($"/api/proveeduria/bodegas/{bodega.Id}", new BodegaListItem(bodega.Id, agencia, usuario, true));
    }

    [HttpPut("bodegas/{id:guid}")]
    public async Task<IActionResult> ActualizarBodega(Guid id, [FromBody] ActualizarBodegaBody body, CancellationToken ct)
    {
        var bodega = await db.Bodegas.FirstOrDefaultAsync(b => b.Id == id, ct);
        if (bodega is null) return NotFound();
        bodega.IdUsuarioResponsable = body.IdUsuarioResponsable;
        bodega.Activo = body.Activo;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpGet("bodegas/{id:guid}/stock")]
    public async Task<ActionResult<IReadOnlyList<StockBodegaListItem>>> Stock(Guid id, CancellationToken ct)
    {
        var resultado = await db.BodegasArticulo.Include(ba => ba.Articulo)
            .Where(ba => ba.IdBodega == id)
            .OrderBy(ba => ba.Articulo.Nombre)
            .Select(ba => new StockBodegaListItem(ba.Id, ba.CodigoArticulo, ba.Articulo.Nombre, ba.Cantidad, ba.PrecioUnitario, ba.ValorTotal))
            .ToListAsync(ct);
        return Ok(resultado);
    }

    [HttpPost("bodegas/{id:guid}/ingresar-stock")]
    [RequireIdempotencyKey]
    public async Task<ActionResult<StockIngresadoResult>> IngresarStock(
        Guid id, [FromBody] IngresarStockBody body, CancellationToken ct)
    {
        var resultado = await solicitudPedidoService.IngresarStockAsync(
            new IngresarStockRequest(id, body.CodigoArticulo, body.Cantidad, body.PrecioUnitario, User.Identity!.Name!), ct);
        return Ok(resultado);
    }

    [HttpGet("solicitudes")]
    public async Task<ActionResult<IReadOnlyList<SolicitudPedidoListItem>>> Solicitudes(CancellationToken ct)
    {
        var resultado = await db.SolicitudesPedido
            .Include(s => s.Bodega).ThenInclude(b => b.Agencia)
            .Include(s => s.UsuarioSolicitante)
            .OrderByDescending(s => s.FechaSistema)
            .Select(s => new SolicitudPedidoListItem(
                s.Id, s.Bodega.Agencia.Nombre, s.UsuarioSolicitante.NombreUsuario, s.Detalle, s.Estado.ToString(), s.FechaSistema,
                db.SolicitudesPedidoArticulo.Where(l => l.IdSolicitud == s.Id).Sum(l => l.Cantidad * l.PrecioUnitario)))
            .ToListAsync(ct);
        return Ok(resultado);
    }

    [HttpGet("solicitudes/{id:guid}/lineas")]
    public async Task<ActionResult<IReadOnlyList<SolicitudPedidoDetalleLinea>>> LineasSolicitud(Guid id, CancellationToken ct)
    {
        var resultado = await db.SolicitudesPedidoArticulo.Include(l => l.Articulo)
            .Where(l => l.IdSolicitud == id)
            .Select(l => new SolicitudPedidoDetalleLinea(l.CodigoArticulo, l.Articulo.Nombre, l.Cantidad, l.PrecioUnitario, l.Detalle))
            .ToListAsync(ct);
        return Ok(resultado);
    }

    [HttpPost("solicitudes")]
    public async Task<ActionResult<SolicitudPedidoCreadaResult>> CrearSolicitud(
        [FromBody] CrearSolicitudPedidoBody body, CancellationToken ct)
    {
        var resultado = await solicitudPedidoService.CrearAsync(
            new CrearSolicitudPedidoRequest(
                body.IdBodega, body.IdUsuarioSolicitante, body.Detalle,
                body.Lineas.Select(l => new LineaSolicitudPedidoRequest(l.CodigoArticulo, l.Cantidad, l.Detalle)).ToList(),
                User.Identity!.Name!),
            ct);
        return Created($"/api/proveeduria/solicitudes/{resultado.IdSolicitud}", resultado);
    }

    [HttpPost("solicitudes/{id:guid}/procesar")]
    [RequireIdempotencyKey]
    public async Task<ActionResult<ProcesarSolicitudPedidoResult>> Procesar(Guid id, CancellationToken ct)
    {
        var resultado = await solicitudPedidoService.ProcesarAsync(id, User.Identity!.Name!, ct);
        return Ok(resultado);
    }

    [HttpPost("solicitudes/{id:guid}/anular")]
    public async Task<IActionResult> Anular(Guid id, [FromBody] AnularSolicitudBody body, CancellationToken ct)
    {
        await solicitudPedidoService.AnularAsync(id, User.Identity!.Name!, body.Comentario, ct);
        return NoContent();
    }
}

public record CrearBodegaBody(int IdAgencia, Guid IdUsuarioResponsable);
public record ActualizarBodegaBody(Guid IdUsuarioResponsable, bool Activo);
public record IngresarStockBody(string CodigoArticulo, int Cantidad, decimal PrecioUnitario);
public record LineaSolicitudPedidoBody(string CodigoArticulo, int Cantidad, string? Detalle);
public record CrearSolicitudPedidoBody(Guid IdBodega, Guid IdUsuarioSolicitante, string? Detalle, List<LineaSolicitudPedidoBody> Lineas);
public record AnularSolicitudBody(string? Comentario);
