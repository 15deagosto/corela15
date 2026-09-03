using Corela15.Domain.Common;
using Corela15.Domain.Sujeto;

namespace Corela15.Domain.LavadoActivos;

/// <summary>
/// Coincidencia real de una persona contra una lista de control —
/// verificado contra SUJETO.LISTACONTROL_ALERTAS (75 alertas reales en
/// Softbank, el resultado final de cruzar el catastro completo del tipo
/// de lista contra la base de clientes). Este core no puede replicar el
/// cruce automático porque no tiene acceso a las listas oficiales
/// externas (sentenciados/PEP/ONU/OFAC se cargan de un archivo del
/// Estado o de un organismo internacional, no de datos que este
/// proyecto genere) — así que la alerta se registra manualmente por
/// Cumplimiento, el mismo resultado final que Softbank guarda en
/// LISTACONTROL_ALERTAS, aunque el proceso de detección automatizado
/// (LISTACONTROL_CABECERA/_DETALLE_*) queda fuera de alcance.
///
/// A diferencia de Softbank (que guarda identificación/nombre como texto
/// libre, sin relación real), acá se referencia directo a Persona — más
/// estricto, evita alertas huérfanas de una persona que no existe en el
/// core.
/// </summary>
public class AlertaListaControl : AuditableEntity
{
    public Guid Id { get; set; }

    public Guid IdPersona { get; set; }
    public Persona Persona { get; set; } = null!;

    public string CodigoTipoListaControl { get; set; } = string.Empty;
    public TipoListaControl TipoListaControl { get; set; } = null!;

    public string Detalle { get; set; } = string.Empty;
    public DateOnly FechaDeteccion { get; set; }

    public bool Resuelta { get; set; }
    public string? ComentarioResolucion { get; set; }
    public string? ResueltoPor { get; set; }
    public DateTimeOffset? FechaResolucion { get; set; }
}
