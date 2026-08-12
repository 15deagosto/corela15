namespace Corela15.Domain.Seguridad;

public class Rol
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Nivel { get; set; }

    public List<UsuarioRol> UsuarioRoles { get; set; } = new();
}
