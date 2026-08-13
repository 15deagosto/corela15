using Corela15.Application.Common;

namespace Corela15.Application.Contabilidad;

public interface IComprobanteContableService
{
    /// <summary>
    /// Registra un comprobante contable completo (cabecera + líneas), validando
    /// que la suma de débitos sea igual a la suma de créditos — ese invariante
    /// no vive en la base de datos (a diferencia del CHECK por línea, que sí
    /// está en el schema), es responsabilidad de este caso de uso.
    /// Actualiza saldo_contable del período de cada cuenta afectada.
    /// </summary>
    /// <exception cref="ComprobanteDesbalanceadoException">
    /// Suma de débitos != suma de créditos.
    /// </exception>
    Task<ComprobanteContableRegistradoResult> RegistrarAsync(
        RegistrarComprobanteContableRequest request, CancellationToken cancellationToken = default);
}

public class ComprobanteDesbalanceadoException(decimal totalDebitos, decimal totalCreditos)
    : ReglaDeNegocioException($"El comprobante no cuadra: débitos {totalDebitos:0.00} != créditos {totalCreditos:0.00}")
{
    public decimal TotalDebitos { get; } = totalDebitos;
    public decimal TotalCreditos { get; } = totalCreditos;
}

public class CuentaContableInvalidaException(Guid idCuentaContable)
    : ReglaDeNegocioException($"La cuenta contable {idCuentaContable} no existe, está inactiva, o no es de detalle (es_mayor)")
{
    public Guid IdCuentaContable { get; } = idCuentaContable;
}

public class ComprobanteLineasInsuficientesException()
    : SolicitudInvalidaException("Un comprobante necesita al menos dos líneas (débito y crédito).");
