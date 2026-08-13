using Corela15.Domain.Clientes;

namespace Corela15.Domain.CallCenter;

/// <summary>Comentario/registro de atención a un socio — verificado contra CALLCENTER.COMENTARIO.</summary>
public class Comentario
{
    public Guid Id { get; set; }

    public Guid IdCliente { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public int IdTipoComentario { get; set; }
    public TipoComentario TipoComentario { get; set; } = null!;

    public string Detalle { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
}
