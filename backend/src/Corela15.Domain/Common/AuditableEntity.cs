namespace Corela15.Domain.Common;

/// <summary>
/// Toda entidad que cambia de estado en el tiempo hereda de acá.
/// Regla de diseño (ver 02-arquitectura-datos-40-modulos.md, Nivel 0 y 1):
/// versionado real desde el día uno, nunca "copiar la tabla a mano".
/// El historial real vive en la tabla *_historico gemela (ver HistoryTrigger
/// en Infrastructure), no en estas columnas.
/// </summary>
public abstract class AuditableEntity
{
    public DateTimeOffset CreadoEn { get; set; }
    public string CreadoPor { get; set; } = string.Empty;
    public DateTimeOffset? ModificadoEn { get; set; }
    public string? ModificadoPor { get; set; }
}
