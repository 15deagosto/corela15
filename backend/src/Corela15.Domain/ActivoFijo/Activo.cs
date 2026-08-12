namespace Corela15.Domain.ActivoFijo;

public enum EstadoActivo
{
    Activo = 1,
    Baja = 2
}

/// <summary>
/// Activos fijos de la cooperativa — grupo CUC 18. Espejo simplificado de
/// ACTIVOFIJO.ACTIVO (20 columnas en Softbank; depreciación
/// —DEPRECIACION_AGENCIA— y traslados —TRASLADO_ACTIVO— quedan fuera de
/// alcance inicial, se agregan cuando se construya el cálculo real).
/// </summary>
public class Activo
{
    public Guid Id { get; set; }
    public string Detalle { get; set; } = string.Empty;
    public DateOnly FechaCompra { get; set; }
    public decimal Valor { get; set; }
    public string? Marca { get; set; }
    public string? Modelo { get; set; }
    public string? Serie { get; set; }
    public bool EsVehiculo { get; set; }
    public EstadoActivo Estado { get; set; } = EstadoActivo.Activo;
}
