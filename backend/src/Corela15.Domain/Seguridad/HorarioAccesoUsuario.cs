namespace Corela15.Domain.Seguridad;

/// <summary>
/// Ventana real de acceso por día de la semana — verificado contra
/// SEGURIDAD.HORARIOINGRESO_USUARIO y HORARIORECESO_USUARIO (tablas
/// gemelas en Softbank, mismas columnas: usuario+día+horainicio+horafin).
/// Consolidadas acá en una sola tabla con el discriminador `EsReceso`
/// (false = ventana de ingreso permitido; true = bloqueo de receso/
/// almuerzo dentro de esa ventana) — simplificación de esquema, no de
/// dato: ambos conceptos reales se conservan tal cual.
/// </summary>
public class HorarioAccesoUsuario
{
    public Guid Id { get; set; }

    public Guid IdUsuario { get; set; }
    public Usuario Usuario { get; set; } = null!;

    /// <summary>1=Lunes ... 7=Domingo, verificado contra el campo DIA real.</summary>
    public int DiaSemana { get; set; }

    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }

    public bool EsReceso { get; set; }
    public bool Activo { get; set; } = true;
}
