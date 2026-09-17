namespace Corela15.Domain.Comunicacion;

/// <summary>
/// Comunicación interna real (chat propio del core, sin depender de
/// ningún servicio externo) — dos formas del mismo concepto:
/// <list type="bullet">
/// <item>Canal de grupo (<see cref="EsDirecto"/> = false): nombre real,
/// con un dueño real (<see cref="IdPropietario"/>) — solo esa persona
/// puede agregar o quitar miembros (bug real cerrado: antes cualquier
/// usuario con acceso al chat podía agregarse o agregar a otros a
/// cualquier canal, sin ningún control real de quién administra qué).
/// <see cref="EsPublico"/> decide si aparece en "Descubrir canales" para
/// cualquiera (autoservicio real) o si solo lo ven quienes ya son
/// miembros — pedido explícito del usuario ("solo ellos deberían ver
/// esos canales"). Dos canales reales se siembran desde el día uno
/// ("Cajas", "Balcón de Servicio") — nacen sin dueño real
/// (<see cref="IdPropietario"/> null, creados por la migración, no por
/// una persona) hasta que un administrador les asigna uno real.</item>
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

    /// <summary>
    /// Dueño real del canal — solo quien crea un canal de grupo puede
    /// agregar/quitar miembros. Nullable a propósito: los canales
    /// sembrados por migración ("Cajas"/"Balcón de Servicio") nacen sin
    /// un dueño real hasta que un administrador les asigna uno.
    /// </summary>
    public Guid? IdPropietario { get; set; }

    /// <summary>Si es visible en "Descubrir canales" para cualquiera (autoservicio) o solo para sus miembros ya agregados por el dueño.</summary>
    public bool EsPublico { get; set; }

    public string CreadoPor { get; set; } = string.Empty;
    public DateTimeOffset CreadoEn { get; set; }
}
