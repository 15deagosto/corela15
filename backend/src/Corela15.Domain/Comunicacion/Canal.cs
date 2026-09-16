namespace Corela15.Domain.Comunicacion;

/// <summary>
/// Comunicación interna real (chat propio del core, sin depender de
/// ningún servicio externo) — dos formas del mismo concepto:
/// <list type="bullet">
/// <item>Canal de grupo (<see cref="EsDirecto"/> = false): nombre real,
/// cualquier usuario con el menú lo puede crear y agregar miembros. Dos
/// canales reales se siembran desde el día uno ("Cajas", "Balcón de
/// Servicio") porque ese es el caso concreto que originó el módulo.</item>
/// <item>Mensaje directo 1:1 (<see cref="EsDirecto"/> = true): sin
/// nombre real, se identifica por <see cref="ClaveDirecta"/> (los dos IDs
/// de usuario ordenados y concatenados) — único, para que "buscar o crear
/// la conversación entre A y B" sea una sola consulta atómica sin
/// condición de carrera real, mismo criterio de índices únicos ya usado
/// en todo el proyecto para evitar duplicados (ej. `DevengoInteresLog`).</item>
/// </list>
/// Ningún mensaje se edita ni se borra — bitácora real de comunicación,
/// mismo criterio de auditoría de todo el core.
/// </summary>
public class Canal
{
    public Guid Id { get; set; }

    public string? Nombre { get; set; }
    public string? Descripcion { get; set; }

    public bool EsDirecto { get; set; }

    /// <summary>Solo para EsDirecto=true: "{idUsuarioMenor}_{idUsuarioMayor}" — único.</summary>
    public string? ClaveDirecta { get; set; }

    public bool Activo { get; set; } = true;

    public string CreadoPor { get; set; } = string.Empty;
    public DateTimeOffset CreadoEn { get; set; }
}
