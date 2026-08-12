namespace Corela15.Domain.Colocacion;

/// <summary>
/// Los baldes de clasificación de cartera (Regla #10: Mora SEPS =
/// (Vencido + No Devenga Interés) / Total) — verificado contra
/// COLOCACION.TIPO_VENCIMIENTO: en la base real no son 3 filas fijas por
/// enum, sino un catálogo con flags booleanos independientes
/// (es posible tener más de un "tipo" con la misma bandera activa).
/// </summary>
public class TipoVencimiento
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool EsVigente { get; set; }
    public bool EsNoDevengaInteres { get; set; }
    public bool EsVencido { get; set; }
    public bool Activo { get; set; } = true;
}
