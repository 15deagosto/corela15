namespace Corela15.Domain.Clientes;

/// <summary>
/// Catálogo real regulatorio — verificado contra CLIENTES.CAUSAVINCULACION
/// (15 filas: la lista real de causales de vinculación/partes relacionadas
/// del Código Orgánico Monetario y Financiero, más "NV No vinculado").
/// </summary>
public class CausaVinculacion
{
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool Activa { get; set; } = true;
}
