namespace Corela15.Domain.Credito;

/// <summary>
/// Catálogo real de seguro de desgravamen flat — verificado contra
/// CREDITO.TIPO_SEGURO (3 filas reales: Individual $1.00, Deudor y
/// Adicional $2.00, Deudor y Familiares $3.00 — valor mensual fijo, no un
/// porcentaje sobre saldo). Modela solo el caso "flat"
/// (COLOCACION.PRESTAMO_SEGURODESGRAVAMEN.ESSEGUROFLAT=true en Softbank):
/// el caso variable (declinante sobre saldo de capital, observado en
/// datos reales de Softbank con valores que bajan cuota a cuota) no tiene
/// una tasa % documentada en ningún manual disponible — se deja fuera a
/// propósito, no inventado.
/// </summary>
public class TipoSeguro
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public decimal ValorMensual { get; set; }
    public int BeneficiarioAdicional { get; set; }
    public bool Activo { get; set; } = true;
}
