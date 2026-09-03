namespace Corela15.Domain.FlujoTrabajo;

/// <summary>Etapa ordenada dentro de un TipoEtapa — verificado contra FLUJOTRABAJO.ETAPA (15 filas reales).</summary>
public class Etapa
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int IdTipoEtapa { get; set; }
    public TipoEtapa TipoEtapa { get; set; } = null!;
    public int Orden { get; set; }

    /// <summary>Si true, basta un aprobador del grupo contable; si false, se requieren todos.</summary>
    public bool ApruebaAlMenosUno { get; set; } = true;

    public int TiempoMaximoDia { get; set; }
    public bool Activa { get; set; } = true;
}
