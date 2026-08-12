namespace Corela15.Domain.Cobranza;

/// <summary>
/// Catálogo de acciones de gestión de cobranza — verificado contra
/// COBRANZA.ACCION_GESTION (19 columnas en Softbank, casi todas flags
/// booleanos por tipo de acción; acá se simplifican a las que importan
/// para el flujo de negocio, no las 19).
/// </summary>
public class AccionGestion
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool EsLlamada { get; set; }
    public bool EsVisita { get; set; }
    public bool EsEnvioSms { get; set; }
    public bool EsAcuerdoPago { get; set; }
    public bool EsJudicial { get; set; }
    public bool Activo { get; set; } = true;
}
