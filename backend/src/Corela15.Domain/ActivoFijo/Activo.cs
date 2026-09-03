using Corela15.Domain.General;

namespace Corela15.Domain.ActivoFijo;

/// <summary>Verificado contra ACTIVOFIJO.ESTADO_ACTIVO (4 códigos reales).</summary>
public enum EstadoActivo
{
    Pendiente = 1,
    Activo = 2,
    Baja = 3,
    Anulado = 4,
}

/// <summary>Verificado contra ACTIVOFIJO.CONDICION_ACTIVO (3 códigos reales).</summary>
public enum CondicionActivo
{
    Bueno = 1,
    Regular = 2,
    Malo = 3,
}

/// <summary>
/// Activo fijo real de la cooperativa — grupo CUC 18. Reconstruido contra
/// ACTIVOFIJO.ACTIVO completa (32 columnas reales verificadas; se
/// modelaron las relevantes para el ciclo de vida real — depreciación,
/// traslado, baja — dejando fuera QR/reavalúo/homologación, sin caso de
/// uso real todavía, ver CLAUDE.md).
/// </summary>
public class Activo
{
    public Guid Id { get; set; }

    /// <summary>Código físico/etiqueta del bien — ACTIVOFIJO.ACTIVO.CODIGOSUPER.</summary>
    public string? Codigo { get; set; }

    public int IdEstructura { get; set; }
    public Estructura Estructura { get; set; } = null!;

    public int IdAgencia { get; set; }
    public Agencia Agencia { get; set; } = null!;

    public string Detalle { get; set; } = string.Empty;
    public DateOnly FechaCompra { get; set; }
    public decimal Valor { get; set; }
    public string? Marca { get; set; }
    public string? Modelo { get; set; }
    public string? Serie { get; set; }

    public bool EsVehiculo { get; set; }
    public bool EsBienDeControl { get; set; }
    public bool EsBienIntangible { get; set; }
    public bool Asegurado { get; set; }

    // Datos de vehículo — solo aplican si EsVehiculo=true.
    public string? Color { get; set; }
    public string? Motor { get; set; }
    public string? Chasis { get; set; }
    public string? Placa { get; set; }
    public string? Cilindraje { get; set; }
    public int? AnioMatriculacion { get; set; }
    public int? AnioVehiculo { get; set; }

    public CondicionActivo Condicion { get; set; } = CondicionActivo.Bueno;
    public EstadoActivo Estado { get; set; } = EstadoActivo.Activo;

    public DateOnly FechaInicioCalculo { get; set; }

    /// <summary>
    /// Depreciación acumulada a la fecha — se mantiene acá como total
    /// corriente (además del detalle histórico en DepreciacionAgenciaDetalle)
    /// para no recalcular sumando todo el historial en cada consulta,
    /// mismo criterio que Prestamo.Saldo.
    /// </summary>
    public decimal DepreciacionAcumulada { get; set; }

    public DateTimeOffset CreadoEn { get; set; }
    public string CreadoPor { get; set; } = string.Empty;
    public DateTimeOffset? ModificadoEn { get; set; }
    public string? ModificadoPor { get; set; }
}
