namespace Corela15.Domain.Reporteria;

/// <summary>
/// Un tablero guardado del módulo Reportería Gerencial (portado del
/// concepto real ya construido y probado en SIGA — constructor visual,
/// favoritos, permisos por rol). La definición (widgets, layout, cada
/// consulta que arma) es un JSON opaco para el backend, igual que en SIGA:
/// lo valida como JSON válido y lo guarda tal cual -- el frontend es dueño
/// de esa forma, y cada widget se vuelve a validar contra el modelo
/// semántico real cuando el tablero se renderiza (nunca se confía en el
/// JSON guardado para ejecutar SQL directo).
///
/// A diferencia de SIGA (SQLite local, `RolesPermitidos` serializado como
/// texto JSON), acá vive en el propio Postgres de Corela15, con
/// `RolesPermitidos` como arreglo nativo -- sin necesidad de serializar/
/// deserializar a mano.
/// </summary>
public class Tablero
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }

    /// <summary>JSON con los widgets del tablero. Opaco para el backend.</summary>
    public string Definicion { get; set; } = string.Empty;

    /// <summary>Nombre de usuario (seguridad.usuario.nombre_usuario), no FK -- mismo criterio que "RegistradoPor" en el resto del core.</summary>
    public string Propietario { get; set; } = string.Empty;

    public bool EsPublico { get; set; }

    /// <summary>Roles (nombre real de seguridad.rol) que pueden ver este tablero cuando EsPublico=true. Vacío = todos los roles.</summary>
    public string[] RolesPermitidos { get; set; } = [];

    public bool EsPredefinido { get; set; }

    public DateTimeOffset CreadoEn { get; set; }
    public DateTimeOffset ModificadoEn { get; set; }
}
