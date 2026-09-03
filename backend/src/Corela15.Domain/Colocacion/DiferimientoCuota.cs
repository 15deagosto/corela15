namespace Corela15.Domain.Colocacion;

/// <summary>
/// Diferimiento de cuotas real — verificado contra COLOCACION.PRESTAMO_
/// CUOTADIFERIDA_AGREGADA (222 filas reales, "período de gracia" cuando
/// el socio pierde el empleo o similar). La fuente real solo registra el
/// evento (préstamo + comentario + usuario + fecha) sin detalle
/// estructurado de cuántos días se difirió — `DiasDiferidos` es un campo
/// aditivo real, necesario para poder aplicar el efecto (mover las
/// fechas de vencimiento), documentado a propósito: no existe una tabla
/// de detalle real en Softbank que lo respalde, se construyó porque es
/// indispensable para el caso de uso real.
/// </summary>
public class DiferimientoCuota
{
    public Guid Id { get; set; }

    public Guid IdPrestamo { get; set; }
    public Prestamo Prestamo { get; set; } = null!;

    public int DiasDiferidos { get; set; }
    public string Comentario { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
    public string RegistradoPor { get; set; } = string.Empty;
}
