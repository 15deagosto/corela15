namespace Corela15.Domain.LavadoActivos;

/// <summary>
/// Catálogo real de tipos de lista de control — verificado contra
/// SUJETO.TIPO_LISTACONTROL (7 códigos exactos): empresas fantasma,
/// homónimos, sentenciados, PEP, ONU, OFAC, paraísos fiscales. En
/// Softbank cada tipo se alimenta de un archivo externo cargado
/// periódicamente (SUJETO.LISTACONTROL_CABECERA/_DETALLE_*, hasta
/// ~988 mil filas para sentenciados) — ese insumo externo (listas
/// oficiales del Estado/organismos internacionales) está fuera del
/// alcance de este core, ver AlertaListaControl.cs para el porqué.
/// </summary>
public class TipoListaControl
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
