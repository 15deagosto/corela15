using Corela15.Domain.General;

namespace Corela15.Domain.FlujoTrabajo;

/// <summary>
/// Ruteo real: qué grupo de aprobadores resuelve una etapa, en una agencia
/// dada — verificado contra FLUJOTRABAJO.ETAPA_GRUPO_CONTABLE (26 filas
/// reales). Esta es la tabla que realmente responde "¿quién puede aprobar
/// esto?": Etapa + Agencia → GrupoContable → GrupoContableUsuario.
/// </summary>
public class EtapaGrupoContable
{
    public int Id { get; set; }
    public int IdEtapa { get; set; }
    public Etapa Etapa { get; set; } = null!;
    public int IdAgencia { get; set; }
    public Agencia Agencia { get; set; } = null!;
    public string CodigoGrupoContable { get; set; } = string.Empty;
    public GrupoContable GrupoContable { get; set; } = null!;
    public int Orden { get; set; }
    public bool Activa { get; set; } = true;
}
