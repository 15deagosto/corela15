namespace Corela15.Domain.LavadoActivos;

/// <summary>
/// Banda real de ingreso mensual para el perfil LA/FT — verificada tal
/// cual contra LAVADOACTIVOS.INGRESOS_MENSUALES (5 filas, todas activas,
/// sin solape: 0-460→0, 460-920→1, 920-1380→2, 1380-1840→3, 1840+→4).
/// </summary>
public class RangoIngresoLavado
{
    public int Id { get; set; }
    public decimal ValorInicial { get; set; }
    public decimal ValorFinal { get; set; }
    public decimal Valor { get; set; }
    public bool Activo { get; set; } = true;
}
