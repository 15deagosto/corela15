namespace Corela15.Domain.Seguridad;

/// <summary>
/// Bitácora real de acciones administrativas sobre usuarios — verificado
/// contra SEGURIDAD.ACCION_USUARIO/ACCION (6 tipos reales: Cambio Sujeto,
/// Creación Sujeto, Eliminación Sujeto, Cambio Clave Personal, Bloqueo de
/// Acceso, Cambio Clave en Lote). Este core usa hoy solo dos
/// (`CambioClavePersonal`/`BloqueoAcceso`, ver `AuthService`) — los
/// códigos son constantes en código, no un catálogo en base, mismo
/// criterio que otros catálogos pequeños de un solo consumidor (ver
/// Tabla 13/57/58/59/60 SEPS en D01). Nunca se borra, misma auditoría
/// que `accion_ingreso_usuario`/`sesion_usuario`.
/// </summary>
public class AccionUsuario
{
    public Guid Id { get; set; }
    public string CodigoAccion { get; set; } = string.Empty;
    public string UsuarioRegistro { get; set; } = string.Empty;
    public DateTimeOffset Fecha { get; set; }
    public string Descripcion { get; set; } = string.Empty;
}
