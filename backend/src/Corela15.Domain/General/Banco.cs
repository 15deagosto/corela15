namespace Corela15.Domain.General;

/// <summary>
/// Catálogo real de bancos/instituciones — verificado contra
/// GENERAL.BANCO (89 filas reales, usado real por FINANCIERO.CHEQUE vía
/// IDBANCO). El campo `Codigo` real tiene duplicados en la fuente
/// (anomalía de captura de Softbank, ej. "100" se repite en ~15 filas
/// distintas) — sembrado tal cual, sin forzar unicidad que la fuente no
/// tiene, el `Id` real es la única clave confiable.
/// </summary>
public class Banco
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool EsNacional { get; set; } = true;
    public bool Activo { get; set; } = true;
}
