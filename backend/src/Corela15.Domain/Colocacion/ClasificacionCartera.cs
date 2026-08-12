using Corela15.Domain.Contabilidad;

namespace Corela15.Domain.Colocacion;

/// <summary>
/// Regla de clasificación de cartera: qué rango de días de mora corresponde
/// a qué cuenta contable y a qué balde de vencimiento — NO es un registro
/// por préstamo (corrección real tras verificar contra la base: en Softbank
/// COLOCACION.CLASIFICACION_CARTERA es una tabla de reglas/catálogo, con
/// columnas DIASINICIO/DIASFIN/CODIGOCUENTACONTABLE/CODIGOTIPOVENCIMIENTO,
/// no el estado de un préstamo puntual — ese vive en PRESTAMO_CONSOLIDADO,
/// fuera del alcance inicial). El día a día de un préstamo se clasifica
/// buscando en qué rango cae su mora actual.
/// </summary>
public class ClasificacionCartera
{
    public Guid Id { get; set; }

    public int DiasInicio { get; set; }
    public int DiasFin { get; set; }

    public Guid IdCuentaContable { get; set; }
    public CuentaContable CuentaContable { get; set; } = null!;

    public int IdTipoVencimiento { get; set; }
    public TipoVencimiento TipoVencimiento { get; set; } = null!;

    public bool Activo { get; set; } = true;
}
