namespace Corela15.Application.Seguridad;

public record LoginRequest(string NombreUsuario, string Contrasena);

public record LoginResult(
    string Token, DateTimeOffset ExpiraEn, Guid IdUsuario, string NombreUsuario,
    IReadOnlyList<string> Roles, IReadOnlyList<string> Menus);

public record SesionActualResult(
    Guid IdUsuario, string NombreUsuario, IReadOnlyList<string> Roles, IReadOnlyList<string> Menus);
