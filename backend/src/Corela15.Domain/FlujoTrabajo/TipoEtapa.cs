namespace Corela15.Domain.FlujoTrabajo;

/// <summary>
/// Motor de aprobaciones genérico — verificado contra FLUJOTRABAJO.TIPO_ETAPA
/// de Softbank (2 filas reales: "SOLICITUD CRÉDITO", "COMPROBANTE CONTABLE").
/// Decompilado del cliente real SBK_WPF (SBK_Models.dll, Models.FlujoTrabajo) y
/// cruzado columna por columna contra la base real antes de modelar — mismo
/// método ya usado para B11/B13/S01/D01/L01. Reemplaza el "Comité de Crédito"
/// de dos estados fijos (Aprobada/Rechazada) por el motor real: tipos de
/// etapa → etapas ordenadas → grupos contables (aprobadores por rango de
/// monto y agencia) → usuarios del grupo. Documentado como "generalizar el
/// patrón de aprobación" en la sección de Comité de Crédito — este es ese
/// paso.
/// </summary>
public class TipoEtapa
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool EsSolicitud { get; set; }
    public bool EsComprobante { get; set; }
    public bool EsSolicitudAdministrativa { get; set; }
    public bool Activa { get; set; } = true;
}
