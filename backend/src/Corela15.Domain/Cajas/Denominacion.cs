namespace Corela15.Domain.Cajas;

public enum TipoDenominacion
{
    Billete = 1,
    Moneda = 2
}

/// <summary>Catálogo de billetes/monedas — verificado contra CAJAS.DENOMINACION.</summary>
public class Denominacion
{
    public int Id { get; set; }
    public TipoDenominacion Tipo { get; set; }
    public decimal Valor { get; set; }
    public bool ConSerie { get; set; }
    public int Orden { get; set; }
    public bool Activo { get; set; } = true;
}
