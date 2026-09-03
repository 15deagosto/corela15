namespace Corela15.Domain.FlujoTrabajo;

/// <summary>
/// Transiciones de retorno válidas desde una etapa (a dónde puede volver si
/// se rechaza) — verificado contra FLUJOTRABAJO.ETAPA_RETORNO (16 filas reales).
/// </summary>
public class EtapaRetorno
{
    public int Id { get; set; }
    public int IdEtapa { get; set; }
    public Etapa Etapa { get; set; } = null!;
    public int IdEtapaRetorno { get; set; }
    public Etapa EtapaDestino { get; set; } = null!;
    public bool Activa { get; set; } = true;
}
