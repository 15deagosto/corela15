namespace Corela15.Domain.LavadoActivos;

/// <summary>
/// Banda real de patrimonio neto para el perfil LA/FT — verificada contra
/// LAVADOACTIVOS.PATRIMONIO_NETO (Softbank tiene ahí varias generaciones
/// de rangos solapados con ACTIVO=true a la vez, resultado de reconfigurar
/// el motor sin desactivar el set anterior; acá se sembró únicamente el
/// set limpio y no solapado: 100-8940→1, 8940-20000→2, 20000-50000→3,
/// 50000+→4).
/// </summary>
public class RangoPatrimonioLavado
{
    public int Id { get; set; }
    public decimal ValorInicial { get; set; }
    public decimal ValorFinal { get; set; }
    public decimal Valor { get; set; }
    public bool Activo { get; set; } = true;
}
