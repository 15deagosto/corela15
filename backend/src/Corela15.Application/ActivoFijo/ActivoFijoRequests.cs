namespace Corela15.Application.ActivoFijo;

public record RegistrarActivoRequest(
    int IdEstructura, int IdAgencia, string? Codigo, string Detalle, DateOnly FechaCompra, decimal Valor,
    string? Marca, string? Modelo, string? Serie, bool EsVehiculo, bool EsBienDeControl, string? Color,
    string? Motor, string? Chasis, string? Placa, string? Cilindraje, int? AnioMatriculacion, int? AnioVehiculo,
    bool Asegurado, Guid? IdResponsableInicial, string RegistradoPor);

public record ActivoRegistradoResult(Guid IdActivo, string? Codigo);

public record AsignarResponsableRequest(Guid IdActivo, Guid IdResponsable, string RegistradoPor);

public record DepreciacionEjecutadaResult(
    DateOnly Fecha, int AgenciasProcesadas, int ActivosDepreciados, decimal TotalDepreciado, IReadOnlyList<Guid> IdsComprobante);

public record CrearTrasladoRequest(
    Guid IdActivo, string Concepto, int IdMotivoTraslado, string? Razon,
    Guid? IdResponsableDestino, int IdAgenciaDestino, string RegistradoPor);

public record TrasladoCreadoResult(Guid IdTraslado);

public record SolicitarBajaRequest(Guid IdActivo, int IdMotivoBaja, string? Detalle, string RegistradoPor);

public record SolicitudBajaCreadaResult(Guid IdSolicitud);

public record BajaProcesadaResult(Guid IdComprobante, decimal ValorLibros, decimal DepreciacionReversada);
