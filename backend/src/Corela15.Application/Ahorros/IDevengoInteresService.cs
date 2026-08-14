namespace Corela15.Application.Ahorros;

public record DevengoInteresEjecutadoResult(
    DateOnly Fecha, int CuentasProcesadas, decimal TotalDevengado, Guid? IdComprobanteContable);

public interface IDevengoInteresService
{
    /// <summary>
    /// Devengo diario de interés sobre depósitos de ahorro: para cada
    /// cuenta activa cuyo producto tiene TasaInteresAnual > 0, calcula
    /// interés_día = saldo disponible × (tasa_anual / 365), lo acredita al
    /// balde de saldo "Interés por pagar" (item_saldo INT, ya existía en
    /// el catálogo desde Nivel 2, sin usar hasta ahora), y registra UN
    /// asiento consolidado (débito 4101 Gasto / crédito 2503 Pasivo) por
    /// toda la corrida — no un comprobante por cuenta. Seguro de correr
    /// más de una vez el mismo día: el índice único de
    /// devengo_interes_log (Fecha, IdCuenta) hace que la segunda corrida
    /// simplemente no encuentre cuentas pendientes. Diseñado para
    /// correrse una vez por día (batch), no por cuenta individual.
    /// </summary>
    Task<DevengoInteresEjecutadoResult> EjecutarDevengoDiarioAsync(
        string registradoPor, CancellationToken cancellationToken = default);
}
