namespace Corela15.Domain.Seguridad;

/// <summary>
/// Registro de idempotencia para operaciones que mueven dinero (depósitos,
/// pagos de cuota, aperturas/cancelaciones de DPF, abonos...). El cliente
/// manda un header `Idempotency-Key` por operación real; un reintento con
/// la misma clave (mismo usuario, misma ruta) devuelve la respuesta
/// original guardada acá en vez de repetir el efecto — evita que un
/// doble-clic o un reintento de red por timeout duplique un movimiento.
/// </summary>
public class SolicitudIdempotente
{
    public Guid Id { get; set; }
    public string Clave { get; set; } = string.Empty;
    public Guid IdUsuario { get; set; }
    public string Ruta { get; set; } = string.Empty;
    public int CodigoEstado { get; set; }
    public string CuerpoRespuesta { get; set; } = string.Empty;
    public DateTimeOffset CreadoEn { get; set; }
}
