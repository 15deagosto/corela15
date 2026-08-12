using Corela15.Domain.Common;
using Corela15.Domain.Credito;
using Corela15.Domain.General;

namespace Corela15.Domain.Colocacion;

public enum EstadoPrestamo
{
    Vigente = 1,
    Cancelado = 2,
    Castigado = 3
}

/// <summary>
/// El préstamo ya desembolsado — verificado columna por columna contra
/// COLOCACION.PRESTAMO (solo lectura). Confirmado temporal nativa en
/// Softbank (columnas de sistema de versionado); acá se replica con el
/// patrón trigger+historico del resto del proyecto.
///
/// <see cref="DebitoSpi"/> es el campo real del incidente documentado en
/// 01-contexto-origen.md: un préstamo con este flag en false igual fue
/// debitado automáticamente porque el proceso no lo cruzó con
/// <c>ahorros.tipo_cuenta.permite_debito_prestamo</c> ni con
/// <c>ahorros.cuenta_item_saldo.acredita_prestamo</c>. El caso de uso de
/// auto-débito (cuando se construya) DEBE validar los tres campos como una
/// sola fuente de verdad, no leer solo este.
/// </summary>
public class Prestamo : AuditableEntity
{
    public Guid Id { get; set; }
    public string Numero { get; set; } = string.Empty;

    public int IdTipoPrestamo { get; set; }
    public TipoPrestamo TipoPrestamo { get; set; } = null!;

    public int IdAgencia { get; set; }
    public Agencia Agencia { get; set; } = null!;

    public int Cuotas { get; set; }
    public decimal DeudaInicial { get; set; }
    public decimal Saldo { get; set; }
    public decimal Tasa { get; set; }
    public decimal Tea { get; set; }
    public DateOnly FechaAdjudicacion { get; set; }
    public DateOnly FechaVencimiento { get; set; }
    public bool DebitoSpi { get; set; }
    public EstadoPrestamo Estado { get; set; } = EstadoPrestamo.Vigente;
}
