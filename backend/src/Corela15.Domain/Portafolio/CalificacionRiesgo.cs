namespace Corela15.Domain.Portafolio;

/// <summary>
/// Verificado contra PORTAFOLIO.CALIFICACIONRIESGO (26 filas reales) —
/// escala real de calificación de riesgo de un emisor/depositario de
/// portafolio, verificada contra el archivo I02 real de referencia
/// (agosto 2026), que la asigna a cada inversión junto con la
/// calificadora que la emitió.
/// </summary>
public class CalificacionRiesgo
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}

/// <summary>
/// Verificado contra PORTAFOLIO.CALIFICADORARIESGO (12 filas reales) —
/// calificadoras de riesgo reales (S&amp;P, Moody's, Fitch, Ecuability,
/// Humphreys, Pcr Pacific, etc.) que emiten la calificación anterior.
/// </summary>
public class CalificadoraRiesgo
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
