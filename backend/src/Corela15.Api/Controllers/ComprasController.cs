using Corela15.Api.Idempotencia;
using Corela15.Application.Common;
using Corela15.Application.Contabilidad;
using Corela15.Domain.Contabilidad;
using Corela15.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Api.Controllers;

public record ProveedorListItem(
    Guid Id, string Identificacion, int IdTipoIdentificacion, string TipoIdentificacion, string Nombre,
    string? Direccion, string? Telefono, string? Email, bool EsContribuyenteEspecial, bool ObligadoLlevarContabilidad, bool Activo);

public record TipoComprobanteCompraListItem(string Codigo, string Nombre);

public record CuentaContableParaCompraListItem(Guid Id, string Codigo, string Nombre);

public record TipoIdentificacionParaCompraListItem(int Id, string Nombre);

public record CompraDetalleItem(Guid IdCuentaContable, string CuentaContable, string Detalle, decimal Cantidad, decimal ValorUnitario, decimal PorcentajeIva, decimal Subtotal, decimal MontoIva, decimal Total);

public record CompraListItem(
    Guid Id, string Numero, string Proveedor, string CodigoSustento, string TipoComprobante,
    string Establecimiento, string PuntoEmision, string Secuencial, string Autorizacion,
    DateOnly FechaEmision, string Concepto, decimal Subtotal, decimal MontoIva, decimal MontoRetencion,
    decimal Total, decimal Saldo, string Estado, IReadOnlyList<CompraDetalleItem> Detalle);

[ApiController]
[Route("api/tesoreria/compras")]
[Authorize(Policy = "Menu:tesoreria")]
public class ComprasController(Corela15DbContext db, IComprasService comprasService) : ControllerBase
{
    [HttpGet("tipos-identificacion")]
    public async Task<ActionResult<IReadOnlyList<TipoIdentificacionParaCompraListItem>>> TiposIdentificacion(CancellationToken cancellationToken)
    {
        var resultado = await db.TiposIdentificacion
            .OrderBy(t => t.Nombre)
            .Select(t => new TipoIdentificacionParaCompraListItem(t.Id, t.Nombre))
            .ToListAsync(cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("proveedores")]
    public async Task<ActionResult<IReadOnlyList<ProveedorListItem>>> Proveedores(CancellationToken cancellationToken)
    {
        var resultado = await db.Proveedores
            .Include(p => p.TipoIdentificacion)
            .Where(p => p.Activo)
            .OrderBy(p => p.Nombre)
            .Select(p => new ProveedorListItem(
                p.Id, p.Identificacion, p.IdTipoIdentificacion, p.TipoIdentificacion.Nombre, p.Nombre,
                p.Direccion, p.Telefono, p.Email, p.EsContribuyenteEspecial, p.ObligadoLlevarContabilidad, p.Activo))
            .ToListAsync(cancellationToken);
        return Ok(resultado);
    }

    [HttpPost("proveedores")]
    public async Task<ActionResult> CrearProveedor([FromBody] ProveedorBody body, CancellationToken cancellationToken)
    {
        if (await db.Proveedores.AnyAsync(p => p.Identificacion == body.Identificacion, cancellationToken))
        {
            throw new CodigoDuplicadoException("un proveedor", body.Identificacion);
        }

        var proveedor = new Proveedor
        {
            Id = Guid.NewGuid(),
            IdTipoIdentificacion = body.IdTipoIdentificacion,
            Identificacion = body.Identificacion,
            Nombre = body.Nombre,
            Direccion = body.Direccion,
            Telefono = body.Telefono,
            Email = body.Email,
            EsContribuyenteEspecial = body.EsContribuyenteEspecial,
            ObligadoLlevarContabilidad = body.ObligadoLlevarContabilidad,
            Activo = true,
        };
        db.Proveedores.Add(proveedor);
        await db.SaveChangesAsync(cancellationToken);
        return Created($"/api/tesoreria/compras/proveedores/{proveedor.Id}", new { proveedor.Id });
    }

    [HttpPut("proveedores/{id:guid}")]
    public async Task<IActionResult> ActualizarProveedor(Guid id, [FromBody] ProveedorActualizarBody body, CancellationToken cancellationToken)
    {
        var proveedor = await db.Proveedores.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (proveedor is null)
        {
            return NotFound();
        }

        proveedor.Nombre = body.Nombre;
        proveedor.Direccion = body.Direccion;
        proveedor.Telefono = body.Telefono;
        proveedor.Email = body.Email;
        proveedor.EsContribuyenteEspecial = body.EsContribuyenteEspecial;
        proveedor.ObligadoLlevarContabilidad = body.ObligadoLlevarContabilidad;
        proveedor.Activo = body.Activo;

        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("cuentas-contables")]
    public async Task<ActionResult<IReadOnlyList<CuentaContableParaCompraListItem>>> CuentasContables(
        [FromQuery] string? q, CancellationToken cancellationToken)
    {
        var query = db.CuentasContables.Where(c => c.Activa && c.EsMayor).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(c => EF.Functions.ILike(c.Nombre, $"%{q}%") || EF.Functions.ILike(c.Codigo, $"%{q}%"));
        }

        var resultado = await query
            .OrderBy(c => c.Codigo)
            .Select(c => new CuentaContableParaCompraListItem(c.Id, c.Codigo, c.Nombre))
            .Take(50)
            .ToListAsync(cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("tipos-comprobante")]
    public async Task<ActionResult<IReadOnlyList<TipoComprobanteCompraListItem>>> TiposComprobante(CancellationToken cancellationToken)
    {
        var resultado = await db.TiposComprobanteCompra
            .Where(t => t.Activo)
            .OrderBy(t => t.Nombre)
            .Select(t => new TipoComprobanteCompraListItem(t.Codigo, t.Nombre))
            .ToListAsync(cancellationToken);
        return Ok(resultado);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CompraListItem>>> Listar(CancellationToken cancellationToken)
    {
        var resultado = await db.Compras
            .Include(c => c.Proveedor)
            .Include(c => c.TipoComprobante)
            .Include(c => c.Detalle).ThenInclude(d => d.CuentaContable)
            .OrderByDescending(c => c.FechaRegistro)
            .Select(c => new CompraListItem(
                c.Id, c.Numero, c.Proveedor.Nombre, c.CodigoSustento, c.TipoComprobante.Nombre,
                c.Establecimiento, c.PuntoEmision, c.Secuencial, c.Autorizacion,
                c.FechaEmision, c.Concepto, c.Subtotal, c.MontoIva, c.MontoRetencion, c.Total, c.Saldo,
                c.Estado.ToString(),
                c.Detalle.Select(d => new CompraDetalleItem(
                    d.IdCuentaContable, d.CuentaContable.Codigo + " " + d.CuentaContable.Nombre, d.Detalle,
                    d.Cantidad, d.ValorUnitario, d.PorcentajeIva, d.Subtotal, d.MontoIva, d.Total)).ToList()))
            .ToListAsync(cancellationToken);

        return Ok(resultado);
    }

    [HttpPost]
    [RequireIdempotencyKey]
    public async Task<ActionResult<CompraRegistradaResult>> Registrar([FromBody] RegistrarCompraBody body, CancellationToken cancellationToken)
    {
        var resultado = await comprasService.RegistrarAsync(
            new RegistrarCompraRequest(
                body.IdProveedor, body.IdAgencia, body.CodigoSustento, body.CodigoTipoComprobante,
                body.Establecimiento, body.PuntoEmision, body.Secuencial, body.Autorizacion,
                body.FechaEmision, body.Concepto, body.MontoRetencion,
                body.Detalle.Select(d => new DetalleCompraRequest(d.IdCuentaContable, d.Detalle, d.Cantidad, d.ValorUnitario, d.PorcentajeIva)).ToList(),
                User.Identity!.Name!),
            cancellationToken);
        return Created($"/api/tesoreria/compras/{resultado.Id}", resultado);
    }

    [HttpPost("{id:guid}/pagos")]
    [RequireIdempotencyKey]
    public async Task<ActionResult<PagoCompraRegistradoResult>> Pagar(Guid id, [FromBody] PagarCompraBody body, CancellationToken cancellationToken)
    {
        var resultado = await comprasService.PagarAsync(
            new PagarCompraRequest(id, body.Monto, body.CodigoFormaCancelacion, User.Identity!.Name!), cancellationToken);
        return Ok(resultado);
    }

    [HttpPost("{id:guid}/anular")]
    [RequireIdempotencyKey]
    public async Task<ActionResult<CompraAnuladaResult>> Anular(Guid id, [FromBody] AnularCompraBody body, CancellationToken cancellationToken)
    {
        var resultado = await comprasService.AnularAsync(
            new AnularCompraRequest(id, body.Motivo, User.Identity!.Name!), cancellationToken);
        return Ok(resultado);
    }
}

public record ProveedorBody(
    int IdTipoIdentificacion, string Identificacion, string Nombre, string? Direccion, string? Telefono,
    string? Email, bool EsContribuyenteEspecial, bool ObligadoLlevarContabilidad);

public record ProveedorActualizarBody(
    string Nombre, string? Direccion, string? Telefono, string? Email, bool EsContribuyenteEspecial,
    bool ObligadoLlevarContabilidad, bool Activo);

public record DetalleCompraBody(Guid IdCuentaContable, string Detalle, decimal Cantidad, decimal ValorUnitario, decimal PorcentajeIva);

public record RegistrarCompraBody(
    Guid IdProveedor, int IdAgencia, string CodigoSustento, string CodigoTipoComprobante,
    string Establecimiento, string PuntoEmision, string Secuencial, string Autorizacion,
    DateOnly FechaEmision, string Concepto, decimal MontoRetencion, IReadOnlyList<DetalleCompraBody> Detalle);

public record PagarCompraBody(decimal Monto, string CodigoFormaCancelacion);

public record AnularCompraBody(string Motivo);
