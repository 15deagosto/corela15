namespace Corela15.Domain.Portafolio;

/// <summary>Verificado contra PORTAFOLIO.TIPOINSTITUCION (6 filas reales).</summary>
public class TipoInstitucion
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}

/// <summary>
/// Institución financiera donde la cooperativa coloca sus inversiones
/// propias — verificado contra PORTAFOLIO.INSTITUCION (44 filas reales:
/// otras cooperativas, FINANCOOP, bancos). Se modelaron los campos de
/// identificación reales; los de calificación de riesgo del emisor
/// (PATRIMONIO/CALIDADACTIVOS/RENTABILIDAD/LIQUIDEZ/calificadora) quedan
/// fuera de esta ronda — sin caso de uso real de reporte de riesgo de
/// contraparte todavía, ver CLAUDE.md.
/// </summary>
public class Institucion
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;

    public string CodigoTipoInstitucion { get; set; } = string.Empty;
    public TipoInstitucion TipoInstitucion { get; set; } = null!;

    public bool Activa { get; set; } = true;
}
