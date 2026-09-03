using Corela15.Application.Common;
using Corela15.Domain.LavadoActivos;

namespace Corela15.Application.LavadoActivos;

public record PerfilLavadoActivosResult(
    Guid Id, Guid IdCliente, DateOnly Fecha, decimal? Patrimonio, decimal? IngresoMensual,
    decimal? BandaPatrimonio, decimal? BandaIngreso, decimal? TotalPerfil, CategoriaRiesgoLavado? Categoria);

public interface IPerfilLavadoActivosService
{
    /// <summary>
    /// Calcula y guarda un nuevo snapshot del perfil LA/FT de un cliente —
    /// ver CalificacionCliente.cs para el alcance real (solo el grupo
    /// "Clientes" de los 5 reales de Softbank, los otros 4 requieren datos
    /// que este core no captura todavía).
    /// </summary>
    Task<PerfilLavadoActivosResult> CalcularAsync(Guid idCliente, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PerfilLavadoActivosResult>> HistorialAsync(Guid idCliente, CancellationToken cancellationToken = default);
}

public class ClienteInvalidoParaPerfilLavadoException(Guid idCliente)
    : ReglaDeNegocioException($"El cliente {idCliente} no existe");
