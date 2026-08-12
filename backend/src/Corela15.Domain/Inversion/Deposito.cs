using Corela15.Domain.Common;
using Corela15.Domain.General;

namespace Corela15.Domain.Inversion;

public enum EstadoDeposito
{
    Vigente = 1,
    Renovado = 2,
    Cancelado = 3,
    Vencido = 4
}

/// <summary>
/// Certificado de depósito a plazo fijo (DPF) — grupo CUC 2103. Espejo de
/// INVERSION.DEPOSITO, verificado columna por columna contra la base real
/// (solo lectura) — igual que <c>Ahorros.Cuenta</c>, el titular NO es una FK
/// directa acá: vive en <see cref="DepositoCliente"/> (bridge con
/// <c>EsPrincipal</c>), y Softbank ya versiona esta tabla de forma nativa
/// (confirmado: tiene columnas de sistema de temporal table) — acá se
/// replica con el mismo patrón trigger+historico del resto del proyecto.
/// Aprendizaje de la investigación original (ver
/// 02-arquitectura-datos-40-modulos.md 3.1): la renovación automática debe
/// tomar la tasa vigente en <see cref="ItemPlazoTasa"/> al momento de
/// renovar, no una copia vieja — eso se resuelve en el caso de uso de
/// renovación (Application), no en esta entidad.
/// </summary>
public class Deposito : AuditableEntity
{
    public Guid Id { get; set; }
    public string Codigo { get; set; } = string.Empty;

    public int IdAgencia { get; set; }
    public Agencia Agencia { get; set; } = null!;

    public decimal Monto { get; set; }
    public decimal Tasa { get; set; }
    public decimal VariacionTasa { get; set; }
    public int PlazoDias { get; set; }
    public bool PagoPeriodicoInteres { get; set; }
    public DateOnly FechaCreacion { get; set; }
    public DateOnly FechaVencimiento { get; set; }
    public EstadoDeposito Estado { get; set; } = EstadoDeposito.Vigente;
}
