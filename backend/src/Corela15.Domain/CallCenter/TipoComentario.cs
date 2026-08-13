namespace Corela15.Domain.CallCenter;

/// <summary>Catálogo de tipos de comentario de atención — verificado contra CALLCENTER.TIPOCOMENTARIO.</summary>
public class TipoComentario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
