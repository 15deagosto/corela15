namespace Corela15.Domain.Colocacion;

/// <summary>
/// Capital/interés/mora/seguro por cuota — verificado contra
/// COLOCACION.PRESTAMO_RUBRO (la tabla que reconstruyó el incidente SPI
/// minuto a minuto en la investigación original). Confirmado temporal
/// nativa en Softbank — versionado real acá también, mismo patrón trigger.
/// </summary>
public class PrestamoRubro
{
    public Guid Id { get; set; }

    public Guid IdPrestamo { get; set; }
    public Prestamo Prestamo { get; set; } = null!;

    public int IdRubro { get; set; }
    public Rubro Rubro { get; set; } = null!;

    public int NumeroCuota { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }

    /// <summary>Lo que se proyectó cobrar en esta cuota/rubro.</summary>
    public decimal Proyectado { get; set; }

    /// <summary>Lo que el cálculo diario acumuló (interés/mora corridos).</summary>
    public decimal Calculado { get; set; }

    /// <summary>Lo efectivamente cobrado.</summary>
    public decimal Cobrado { get; set; }

    public string Estado { get; set; } = string.Empty;
}
