using Corela15.Application.Common;

namespace Corela15.Application.Colocacion;

public record CrearTipoPrestamoRequest(
    string Codigo, string Nombre, decimal MontoMinimo, decimal MontoMaximo,
    int PlazoMinimoDias, int PlazoMaximoDias, decimal TasaAnual, string SegmentoBce);

public record ActualizarTipoPrestamoRequest(
    string Nombre, decimal MontoMinimo, decimal MontoMaximo,
    int PlazoMinimoDias, int PlazoMaximoDias, decimal TasaAnual, string SegmentoBce, bool Activo);

public record TipoPrestamoAdminResult(int Id, string Codigo);

public interface ITipoPrestamoAdminService
{
    /// <summary>
    /// Crea/edita un producto de crédito, validando la tasa contra el
    /// techo BCE vigente del segmento en el momento de guardar — mismo
    /// chequeo que IPrestamoService.SolicitarAsync hace en cada solicitud,
    /// pero acá se detecta el error al configurar el producto, no cuando
    /// un socio ya está esperando la respuesta de una solicitud.
    /// </summary>
    Task<TipoPrestamoAdminResult> CrearAsync(CrearTipoPrestamoRequest request, CancellationToken cancellationToken = default);

    Task ActualizarAsync(int id, ActualizarTipoPrestamoRequest request, CancellationToken cancellationToken = default);
}

public class TipoPrestamoNoExisteException(int id)
    : ReglaDeNegocioException($"El tipo de préstamo {id} no existe");

public class SegmentoBceInvalidoException(string segmento)
    : SolicitudInvalidaException($"Segmento BCE inválido: '{segmento}' — no hay un techo configurado con ese nombre exacto");
