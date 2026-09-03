namespace Corela15.Domain.FlujoTrabajo;

/// <summary>
/// Grupo de aprobadores real, con rango de monto que puede autorizar —
/// verificado contra FLUJOTRABAJO.GRUPO_CONTABLE (19 filas reales, "a pesar
/// del nombre no tiene relación con contabilidad — es el nombre real que
/// usa Softbank para "grupo de aprobadores"). Un mismo tipo de decisión
/// (ej. Comité de Crédito) puede tener varios grupos activos por rango de
/// monto — acá se simplifica a un grupo por etapa+agencia (ver
/// EtapaGrupoContable), sin escalonar por monto dentro de la misma etapa,
/// documentado como simplificación consciente.
/// </summary>
public class GrupoContable
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public decimal MontoMinimo { get; set; }
    public decimal MontoMaximo { get; set; }
    public bool Activo { get; set; } = true;
}
