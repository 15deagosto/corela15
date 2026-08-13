namespace Corela15.Domain.ReporteControl;

public enum EntidadReguladora
{
    Seps = 1,
    Bce = 2,
    Sri = 3,
    Uaf = 4
}

/// <summary>
/// Índice de los reportes regulatorios obligatorios — verificado contra
/// REPORTECONTROL (existen 48 tablas CABECERA_* activas con filas reales en
/// Softbank hoy: B13, D01, BCE01/BCE02, S01, C01-C07, L01/L02, IG01, TIN,
/// UAF, RFD, ROTEF, R01-R22, CRS, entre otros).
///
/// IMPORTANTE — esto es solo el ÍNDICE de qué reportes existen (nombre y
/// entidad), NO la estructura de datos de cada reporte. La estructura real
/// de cada uno (qué campos exige, con qué fórmula, en qué periodicidad) NO
/// se infiere de las tablas de Softbank — hay que verificarla contra la
/// norma oficial de la SEPS/BCE para cada código, uno por uno, antes de
/// modelar su CABECERA_*/DETALLE_* real (ver
/// 02-arquitectura-datos-40-modulos.md, cierre de Nivel 8). No implementar
/// el detalle de ningún reporte sin esa verificación regulatoria explícita.
/// </summary>
public class ReporteRegulatorio
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public EntidadReguladora Entidad { get; set; }
    public bool Activo { get; set; } = true;
}
