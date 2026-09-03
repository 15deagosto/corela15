using Corela15.Domain.Clientes;

namespace Corela15.Domain.LavadoActivos;

/// <summary>
/// Los 4 niveles reales usados para clasificar el perfil (nombres
/// verificados contra LAVADOACTIVOS.TIPO_CALIFICACION — esa tabla tiene 5
/// niveles reales, MUY BAJO incluido; acá se usan los 4 que corresponden
/// al rango 1-4 de las bandas de patrimonio/ingreso que sí se sembraron,
/// ver RangoPatrimonioLavado/RangoIngresoLavado).
/// </summary>
public enum CategoriaRiesgoLavado
{
    Bajo = 1,
    Medio = 2,
    Alto = 3,
    MuyAlto = 4
}

/// <summary>
/// Calificación de riesgo de lavado de activos por cliente — verificado
/// contra LAVADOACTIVOS.CALIFICACIONCLIENTE (15 columnas estadísticas en
/// Softbank; acá se modela el resumen del perfil, no el detalle completo
/// del cálculo).
///
/// **Alcance real, no inventado**: Softbank calcula el perfil como un
/// promedio ponderado de 5 grupos reales (LAVADOACTIVOS.GRUPO/
/// GRUPOPONDERACION: Clientes 40%, Canales 20%, Tipo de productos 25%,
/// Zona geográfica 10%, Transaccional 5%). De esos 5, solo "Clientes"
/// (patrimonio + ingreso, bandas verificadas en PATRIMONIO_NETO/
/// INGRESOS_MENSUALES) es calculable con datos 100% reales y sin
/// ambigüedad — los otros 4 requieren datos que este core no captura
/// (canal de la transacción, división política/zona más allá de
/// provincia, catálogo producto→peso) o, en el caso de "Transaccional",
/// el motor de 6.3M filas fuera de alcance — y la fórmula exacta de cómo
/// Softbank combina cada grupo internamente (más allá del peso final)
/// tampoco está documentada en ningún manual disponible, así que
/// replicarla sería inventar un paso que no se puede verificar. Por eso
/// `TotalPerfil` acá es el promedio simple de patrimonio+ingreso
/// bandeados (1-4), no el score ponderado de 5 grupos — una
/// simplificación real y declarada, no un motor completo.
/// </summary>
public class CalificacionCliente
{
    public Guid Id { get; set; }

    public Guid IdCliente { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public DateOnly Fecha { get; set; }
    public decimal? Patrimonio { get; set; }
    public decimal? IngresoMensual { get; set; }
    public decimal? BandaPatrimonio { get; set; }
    public decimal? BandaIngreso { get; set; }
    public decimal? TotalPerfil { get; set; }
    public CategoriaRiesgoLavado? Categoria { get; set; }
}
