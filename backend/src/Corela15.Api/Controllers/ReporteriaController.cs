using System.Diagnostics;
using System.Text.Json;
using Corela15.Infrastructure.Reporteria;
using Corela15.Infrastructure.Reporteria.Query;
using Corela15.Infrastructure.Reporteria.Semantic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Corela15.Api.Controllers;

public record QueryRequestBody(
    string Dataset, string[] Dimensions, string[] Measures, FilterValue[] Filters,
    DateTime? Snapshot, string? OrderBy, bool OrderDesc, int Limit);

public record TableroBody(string Nombre, string? Descripcion, string Definicion, bool EsPublico, string[] RolesPermitidos);
public record PermisosTableroBody(bool EsPublico, string[] RolesPermitidos);

/// <summary>
/// Reportería Gerencial — motor semántico portado del proyecto SIGA (tipo
/// Qlik Sense: el usuario elige dimensiones/métricas/filtros, nunca escribe
/// SQL), integrado acá para administrarse con los mismos permisos que el
/// resto de Corela15 en vez de un login/rol separado (ver
/// <c>seguridad.dataset_reporteria</c>/<c>rol_dataset_reporteria</c>,
/// claim <c>dataset</c> del JWT).
///
/// A diferencia del resto de controllers, la autorización fina no es una
/// sola policy de clase: cada dataset tiene su propia policy
/// (<c>Dataset:cartera</c>, <c>Dataset:nominal</c>, etc.) pero el dataset
/// pedido viaja en el body, no en la ruta — se valida en tiempo real contra
/// los claims del usuario en cada acción, mismo criterio real que
/// <c>QueryCompiler.Compile</c> ya aplica como segunda barrera.
///
/// Fuente de datos, deliberada y documentada: este controller consulta
/// Softbank en vivo (solo lectura, ver QueryExecutor.cs) — mismo patrón ya
/// establecido para "Estructuras y Procesos Financieros"/OF01. Es un
/// estado de transición: cuando la lógica de negocio de cada área
/// (Cartera, Captaciones, Contabilidad...) esté completa en el core propio
/// (Postgres), estos datasets deben migrar a leer de ahí en vez de
/// Softbank, mismo patrón "sync externo → tabla propia → generador sin
/// cambios" ya usado para OF01 (ver "Sincronización real OF01" en
/// CLAUDE.md) — el modelo semántico (Semantic/models/*.json) es justamente
/// el punto único donde ese cambio de fuente se haría, sin tocar el
/// compilador ni el frontend.
/// </summary>
[ApiController]
[Route("api/reporteria")]
[Authorize(Policy = "Menu:reporteria-gerencial")]
public class ReporteriaController(
    SemanticModelStore store, QueryCompiler compiler, QueryExecutor executor, TableroReporteriaService tableros) : ControllerBase
{
    private string Usuario => User.Identity!.Name!;
    private string[] Roles => User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToArray();
    private IReadOnlyCollection<string> DatasetsOtorgados =>
        User.FindAll("dataset").Select(c => c.Value).ToArray();

    // ------------------------------------------------------------------
    // Consulta (motor semántico)
    // ------------------------------------------------------------------

    [HttpGet("datasets")]
    public ActionResult<IReadOnlyList<DatasetInfo>> Datasets()
        => Ok(store.Describe(DatasetsOtorgados).ToArray());

    [HttpPost("query")]
    public async Task<ActionResult<QueryResult>> Query(QueryRequestBody body, CancellationToken cancellationToken)
    {
        var otorgados = DatasetsOtorgados;
        if (!otorgados.Contains(body.Dataset, StringComparer.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        var req = new QueryRequest
        {
            Dataset = body.Dataset,
            Dimensions = body.Dimensions,
            Measures = body.Measures,
            Filters = body.Filters,
            Snapshot = body.Snapshot,
            OrderBy = body.OrderBy,
            OrderDesc = body.OrderDesc,
            Limit = body.Limit == 0 ? 5000 : body.Limit,
        };

        var sw = Stopwatch.StartNew();
        CompiledQuery compilado;
        try
        {
            compilado = compiler.Compile(req, otorgados);
        }
        catch (QueryCompilerException ex)
        {
            // Identificador desconocido/no permitido, filtro mal formado,
            // etc. -- error del cliente, nunca un 500 (QueryCompilerException
            // no hereda de Corela15.Application.Common.DomainException, así
            // que DomainExceptionHandler no la traduce sola).
            await tableros.RegistrarAuditoriaAsync(
                Usuario, body.Dataset, body.Dimensions, body.Measures, JsonSerializer.Serialize(body.Filters),
                body.Snapshot, sw.ElapsedMilliseconds, 0, ex.Message, cancellationToken);
            return BadRequest(new { detail = ex.Message });
        }

        QueryResult resultado;
        try
        {
            resultado = await executor.ExecuteAsync(compilado, cancellationToken);
        }
        catch (Exception ex)
        {
            await tableros.RegistrarAuditoriaAsync(
                Usuario, body.Dataset, body.Dimensions, body.Measures, JsonSerializer.Serialize(body.Filters),
                body.Snapshot, sw.ElapsedMilliseconds, 0, ex.Message, cancellationToken);
            throw;
        }

        await tableros.RegistrarAuditoriaAsync(
            Usuario, body.Dataset, body.Dimensions, body.Measures, JsonSerializer.Serialize(body.Filters),
            body.Snapshot, sw.ElapsedMilliseconds, resultado.Rows.Count, null, cancellationToken);

        return Ok(resultado);
    }

    /// <summary>Llena el desplegable de un filtro con catálogo (ej. lista de agencias). El SQL vive en el modelo semántico, nunca lo manda el cliente.</summary>
    [HttpGet("{dataset}/filtros/{filtroId}/catalogo")]
    public async Task<ActionResult<IReadOnlyList<CatalogItem>>> Catalogo(string dataset, string filtroId, CancellationToken cancellationToken)
    {
        if (!DatasetsOtorgados.Contains(dataset, StringComparer.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        var ds = store.Get(dataset);
        var filtro = ds?.Filters.FirstOrDefault(f => f.Id == filtroId);
        if (filtro?.CatalogSql is null)
        {
            return NotFound();
        }

        return Ok(await executor.CatalogAsync(filtro.CatalogSql, cancellationToken));
    }

    // ------------------------------------------------------------------
    // Tableros guardados (propio Postgres de Corela15, no Softbank)
    // ------------------------------------------------------------------

    [HttpGet("tableros")]
    public async Task<ActionResult<IReadOnlyList<TableroResumenDto>>> ListarTableros(CancellationToken ct)
        => Ok(await tableros.ListarAsync(Usuario, Roles, ct));

    /// <summary>Panel de administración: todos los tableros, sin filtrar por visibilidad -- solo ADMINISTRADOR.</summary>
    [HttpGet("tableros/admin")]
    public async Task<ActionResult<IReadOnlyList<TableroResumenDto>>> ListarTodosLosTableros(CancellationToken ct)
    {
        if (!User.IsInRole("ADMINISTRADOR")) return Forbid();
        return Ok(await tableros.ListarTodosAsync(Usuario, ct));
    }

    [HttpGet("tableros/{id:int}")]
    public async Task<ActionResult<TableroDto>> ObtenerTablero(int id, CancellationToken ct)
    {
        var t = await tableros.ObtenerAsync(id, Usuario, Roles, ct);
        return t is null ? NotFound() : Ok(t);
    }

    [HttpPost("tableros")]
    public async Task<ActionResult<TableroDto>> CrearTablero(TableroBody body, CancellationToken ct)
    {
        var resultado = await tableros.CrearAsync(new TableroInput(body.Nombre, body.Descripcion, body.Definicion, body.EsPublico, body.RolesPermitidos), Usuario, ct);
        return CreatedAtAction(nameof(ObtenerTablero), new { id = resultado.Id }, resultado);
    }

    [HttpPut("tableros/{id:int}")]
    public async Task<ActionResult<TableroDto>> ActualizarTablero(int id, TableroBody body, CancellationToken ct)
    {
        var resultado = await tableros.ActualizarAsync(id, new TableroInput(body.Nombre, body.Descripcion, body.Definicion, body.EsPublico, body.RolesPermitidos), Usuario, ct);
        return resultado is null ? NotFound() : Ok(resultado);
    }

    /// <summary>Panel de administración: cambia visibilidad de cualquier tablero, sin importar el dueño -- solo ADMINISTRADOR.</summary>
    [HttpPut("tableros/{id:int}/permisos")]
    public async Task<IActionResult> ActualizarPermisosTablero(int id, PermisosTableroBody body, CancellationToken ct)
    {
        if (!User.IsInRole("ADMINISTRADOR")) return Forbid();
        var ok = await tableros.ActualizarPermisosAsync(id, new PermisosTableroInput(body.EsPublico, body.RolesPermitidos), ct);
        return ok ? NoContent() : NotFound();
    }

    [HttpDelete("tableros/{id:int}")]
    public async Task<IActionResult> BorrarTablero(int id, CancellationToken ct)
    {
        var ok = await tableros.BorrarAsync(id, Usuario, ct);
        return ok ? NoContent() : NotFound();
    }

    [HttpPost("tableros/{id:int}/favorito")]
    public async Task<IActionResult> MarcarFavorito(int id, CancellationToken ct)
    {
        await tableros.MarcarFavoritoAsync(id, Usuario, ct);
        return NoContent();
    }

    [HttpDelete("tableros/{id:int}/favorito")]
    public async Task<IActionResult> QuitarFavorito(int id, CancellationToken ct)
    {
        await tableros.QuitarFavoritoAsync(id, Usuario, ct);
        return NoContent();
    }
}
