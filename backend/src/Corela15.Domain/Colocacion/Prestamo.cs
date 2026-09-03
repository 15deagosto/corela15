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

    /// <summary>
    /// El asesor real que originó la solicitud (nombre de usuario, mismo
    /// campo real verificado en vivo contra <c>COLOCACION.PRESTAMO.
    /// CODIGOUSUARIO</c>) — nullable y aditivo: los préstamos desembolsados
    /// antes de este campo no tienen forma real de reconstruirlo (no existe
    /// FK de <see cref="Prestamo"/> a <c>SolicitudPrestamo</c>), se sembró
    /// solo hacia adelante desde <c>DesembolsarAsync</c>. Distinto de
    /// <see cref="AuditableEntity.CreadoPor"/> del préstamo (que es quien
    /// ejecutó el desembolso, no necesariamente el asesor que gestionó al
    /// socio) — mismo criterio que Softbank, donde `CODIGOUSUARIO` del
    /// préstamo no siempre coincide con quien procesa cada transacción
    /// puntual.
    /// </summary>
    public string? CodigoUsuarioAsesor { get; set; }

    // Convenio real bajo el que se desembolsó (copiado de
    // SolicitudPrestamo.CodigoTipoConvenio al momento del desembolso,
    // verificado contra COLOCACION.PRESTAMO_TIPOCONVENIO — solo 2 códigos
    // reales tienen uso real hoy, ver migración Credito_TipoConvenio).
    public string? CodigoTipoConvenio { get; set; }
    public Credito.TipoConvenio? TipoConvenio { get; set; }
}
