namespace Corela15.Domain.HerramientaRural;

/// <summary>
/// Catálogo de productos agrarios para la calculadora de flujo de caja de
/// crédito rural — verificado contra HERRAMIENTARURAL.TIPO_PRODUCTOAGRARIO.
/// El resto del motor de cálculo (costos, rendimientos, precios, evaluación
/// de riesgo agrícola) queda fuera de alcance inicial — apéndice de
/// Crédito (Nivel 3), se agrega cuando se necesite.
/// </summary>
public class TipoProductoAgrario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
