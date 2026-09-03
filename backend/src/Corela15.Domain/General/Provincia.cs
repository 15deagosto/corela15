namespace Corela15.Domain.General;

/// <summary>
/// Catálogo oficial INEC (Clasificador Geográfico Estadístico 2019) — los
/// 24 códigos reales verificados contra la Tabla 05 del "Manual Técnico de
/// Tablas de Información" de la SEPS (versión 34.0), el mismo código de
/// 2 dígitos que exige el atributo "provincia" de la estructura D01
/// (Depósitos) y el campo geográfico de C01 (Cartera de Créditos).
/// Deliberadamente NO se sembraron Cantón/Parroquia (Tablas 06/07 del
/// mismo manual): su layout en el PDF fuente es una tabla de varias
/// columnas que el extractor de texto desordena, y transcribir a mano
/// ~221 cantones + ~1500 parroquias sin una fuente confiable (CSV/Excel
/// oficial del INEC en vez del PDF) arriesgaba corromper un catálogo
/// regulatorio real — la misma razón por la que este proyecto nunca
/// inventa estructura sin poder verificarla. Revisar si aparece una
/// fuente más limpia antes de completar el resto de la jerarquía.
/// </summary>
public class Provincia
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
}
