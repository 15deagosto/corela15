using Corela15.Domain.General;

namespace Corela15.Domain.Portafolio;

/// <summary>
/// Verificado contra PORTAFOLIO.ESTADOINVERSION (8 códigos reales) — solo
/// 4 tienen uso real observado (A/C/N/V, 204 filas reales); Ingresado/
/// Autorizado/Exigible/Procesado sin caso de uso real todavía.
/// </summary>
public enum EstadoInversionPortafolio
{
    Activa = 1,
    Cancelada = 2,
    Anulada = 3,
    Vencida = 4,
}

/// <summary>
/// Portafolio de inversiones propias de la cooperativa (dónde coloca su
/// liquidez excedente en otras instituciones — no confundir con los DPF
/// de los socios, Nivel 3, que es la cooperativa recibiendo depósitos) —
/// grupo CUC 13. Reconstruido contra PORTAFOLIO.INVERSIONPORTAFOLIO real
/// (52 columnas) — verificado que el 100% de las 204 inversiones reales
/// de esta cooperativa son Certificados de Inversión simples (tipo único
/// real "001 CERTIFICADO DE INVERSION", sin cupón/vector de precio/
/// materialización — esos campos reales de la fuente no se modelaron acá
/// por no tener ningún caso de uso real detrás, mismo criterio que
/// garantía hipotecaria/prendaria en Colocación). Económicamente
/// equivalente a un DPF, pero del lado activo (la cooperativa invierte,
/// no capta) — mismo tratamiento contable/de renovación que
/// Inversion.Deposito, con las cuentas reales del grupo 13 en vez del 21.
/// </summary>
public class InversionPortafolio
{
    public Guid Id { get; set; }
    public string Documento { get; set; } = string.Empty;

    public int IdAgencia { get; set; }
    public Agencia Agencia { get; set; } = null!;

    public string CodigoInstitucion { get; set; } = string.Empty;
    public Institucion Institucion { get; set; } = null!;

    public decimal ValorNominal { get; set; }
    public decimal Tasa { get; set; }
    public DateOnly FechaCompra { get; set; }
    public DateOnly FechaVencimiento { get; set; }

    public EstadoInversionPortafolio Estado { get; set; } = EstadoInversionPortafolio.Activa;

    public DateTimeOffset CreadoEn { get; set; }
    public string CreadoPor { get; set; } = string.Empty;
    public DateTimeOffset? ModificadoEn { get; set; }
    public string? ModificadoPor { get; set; }
}
