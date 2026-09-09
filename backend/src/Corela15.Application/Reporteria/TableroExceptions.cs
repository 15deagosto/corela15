using Corela15.Application.Common;

namespace Corela15.Application.Reporteria;

/// <summary>El usuario no es dueño del tablero e intenta editarlo/borrarlo/cambiar sus permisos.</summary>
public class TableroSinPermisoException() : ReglaDeNegocioException("No tenés permiso sobre este tablero.");
