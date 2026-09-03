using Corela15.Domain.General;

namespace Corela15.Domain.Cajas;

/// <summary>
/// Control real de formularios pre-numerados (papeletas de depósito/
/// retiro, libretas de ahorro, certificados DPF impresos) — verificado
/// contra CAJAS.TIPO_FORMANUMERADA (4 filas reales) / CAJAS.
/// FORMA_NUMERADA (8 filas reales, rangos asignados por agencia). Es
/// solo el control administrativo del rango físico asignado — no lleva
/// un contador de consumo (Softbank tampoco lo tiene en esta tabla; la
/// numeración real de lo que este core ya emite, como los códigos de
/// DPF, sigue su propio mecanismo de conteo+1 ya construido).
/// </summary>
public class TipoFormaNumerada
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}

public class FormaNumerada
{
    public Guid Id { get; set; }

    public int IdAgencia { get; set; }
    public Agencia Agencia { get; set; } = null!;

    public string CodigoTipo { get; set; } = string.Empty;
    public TipoFormaNumerada Tipo { get; set; } = null!;

    public int Inicio { get; set; }
    public int Fin { get; set; }

    public DateOnly FechaAsignacion { get; set; }
    public string RegistradoPor { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
