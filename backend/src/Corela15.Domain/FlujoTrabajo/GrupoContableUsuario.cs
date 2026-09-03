using Corela15.Domain.Seguridad;

namespace Corela15.Domain.FlujoTrabajo;

/// <summary>
/// Usuarios que integran un grupo de aprobadores — verificado contra
/// FLUJOTRABAJO.GRUPO_CONTABLE_USUARIO (357 filas reales). A diferencia de
/// Softbank (que referencia por CODIGOUSUARIO string), acá referencia
/// directo Usuario.Id (Guid), mismo criterio de integridad real ya usado
/// en AlertaListaControl.
/// </summary>
public class GrupoContableUsuario
{
    public int Id { get; set; }
    public string CodigoGrupoContable { get; set; } = string.Empty;
    public GrupoContable GrupoContable { get; set; } = null!;
    public Guid IdUsuario { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public bool Activo { get; set; } = true;
}
