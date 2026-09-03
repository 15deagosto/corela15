using Corela15.Application.Common;

namespace Corela15.Application.Colocacion;

public record AgregarGaranteRequest(Guid IdSolicitud, Guid IdClienteGarante, string? Detalle, string RegistradoPor);

public record GaranteAgregadoResult(Guid Id);

public record QuitarGaranteRequest(Guid IdGarantia, string RegistradoPor);

public record GaranteDetalle(
    Guid Id, Guid IdClienteGarante, string Garante, string? Detalle,
    string? CodigoEstadoGarantia, string? EstadoGarantia);

public interface IGarantiaService
{
    /// <summary>
    /// Registra un garante/codeudor sobre una solicitud EnAnalisis — la
    /// misma información real que ve el Comité de Crédito antes de decidir
    /// (CREDITO.SOLICITUD_PRESTAMO_GARANTIAPERSONAL). El garante debe ser
    /// un cliente activo distinto del titular de la solicitud.
    /// </summary>
    Task<GaranteAgregadoResult> AgregarGaranteAsync(AgregarGaranteRequest request, CancellationToken cancellationToken = default);

    /// <summary>Quita (desactiva) un garante de una solicitud todavía EnAnalisis, antes de enviarla a Comité.</summary>
    Task QuitarGaranteAsync(QuitarGaranteRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GaranteDetalle>> ListarPorSolicitudAsync(Guid idSolicitud, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GaranteDetalle>> ListarPorPrestamoAsync(Guid idPrestamo, CancellationToken cancellationToken = default);
}

public class GaranteEsElTitularException(Guid idCliente)
    : ReglaDeNegocioException($"El garante no puede ser el mismo socio titular de la solicitud ({idCliente})");

public class GaranteClienteInvalidoException(Guid idCliente)
    : ReglaDeNegocioException($"El cliente {idCliente} no existe o no está activo, no puede ser garante");

public class GarantiaInvalidaException(Guid idGarantia)
    : ReglaDeNegocioException($"La garantía {idGarantia} no existe o ya fue quitada");
