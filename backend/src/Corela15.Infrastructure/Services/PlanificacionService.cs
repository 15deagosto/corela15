using Corela15.Application.Planificacion;
using Corela15.Domain.Planificacion;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

public class PlanificacionService(Corela15DbContext db) : IPlanificacionService
{
    // Paleta rotativa real para etiquetas creadas "sobre la marcha" --
    // colores distintos y con buen contraste, ciclando según cuántas
    // etiquetas ya tiene el área (nunca dos seguidas iguales mientras
    // haya colores libres en la paleta).
    private static readonly string[] PaletaColores =
    [
        "#c9a227", "#16213e", "#64748b", "#1d4e5f", "#7a1f2b",
        "#6b4226", "#a67c1f", "#2f6f4f", "#5b3a8e", "#8a4b2e",
    ];

    // Ecuador es UTC-5 fijo, sin horario de verano -- mismo criterio ya
    // usado en el resto del proyecto (ver HorarioAccesoUsuario) para no
    // depender de TimeZoneInfo del sistema operativo del servidor.
    private static readonly TimeSpan OffsetEcuador = TimeSpan.FromHours(-5);

    /// <summary>Viernes 17:00 hora Ecuador de la semana real que empieza el lunes dado.</summary>
    private static DateTimeOffset FechaLimiteEnvio(DateOnly fechaInicioSemana)
    {
        var viernes = fechaInicioSemana.AddDays(4);
        return new DateTimeOffset(viernes.Year, viernes.Month, viernes.Day, 17, 0, 0, OffsetEcuador);
    }

    public async Task<EtiquetaDto> ObtenerOCrearEtiquetaAsync(string codigoArea, string nombre, CancellationToken cancellationToken = default)
    {
        var nombreLimpio = nombre.Trim();
        if (string.IsNullOrWhiteSpace(nombreLimpio))
            throw new BloqueHorarioInvalidoException("El nombre de la etiqueta no puede estar vacío.");

        var existente = await db.EtiquetasPlanificacion
            .FirstOrDefaultAsync(e => e.CodigoArea == codigoArea && EF.Functions.ILike(e.Nombre, nombreLimpio), cancellationToken);
        if (existente is not null)
            return new EtiquetaDto(existente.Codigo, existente.Nombre, existente.ColorHex);

        if (!await db.AreasPlanificacion.AnyAsync(a => a.Codigo == codigoArea && a.Activo, cancellationToken))
            throw new AreaPlanificacionInvalidaException(codigoArea);

        var slug = new string(nombreLimpio.ToUpperInvariant().Where(c => char.IsLetterOrDigit(c) || c == ' ').ToArray())
            .Replace(' ', '_');
        if (slug.Length > 20) slug = slug[..20];

        var codigo = $"{codigoArea}_{slug}";
        var sufijo = 1;
        while (await db.EtiquetasPlanificacion.AnyAsync(e => e.Codigo == codigo, cancellationToken))
            codigo = $"{codigoArea}_{slug}_{++sufijo}";

        var cantidadActual = await db.EtiquetasPlanificacion.CountAsync(e => e.CodigoArea == codigoArea, cancellationToken);
        var color = PaletaColores[cantidadActual % PaletaColores.Length];

        var etiqueta = new EtiquetaPlanificacion { Codigo = codigo, CodigoArea = codigoArea, Nombre = nombreLimpio, ColorHex = color, Activo = true };
        db.EtiquetasPlanificacion.Add(etiqueta);
        await db.SaveChangesAsync(cancellationToken);

        return new EtiquetaDto(etiqueta.Codigo, etiqueta.Nombre, etiqueta.ColorHex);
    }

    public async Task<PlanSemanalDto> GuardarAsync(GuardarPlanSemanalRequest request, CancellationToken cancellationToken = default)
    {
        var area = await db.AreasPlanificacion.FirstOrDefaultAsync(a => a.Codigo == request.CodigoArea && a.Activo, cancellationToken)
            ?? throw new AreaPlanificacionInvalidaException(request.CodigoArea);

        await ValidarPerteneceAlAreaAsync(request.CodigoArea, request.RegistradoPor, cancellationToken);

        if (request.Bloques.Count == 0)
            throw new BloqueHorarioInvalidoException("La planificación necesita al menos un bloque de actividad.");

        foreach (var b in request.Bloques)
        {
            if (b.HoraFin <= b.HoraInicio)
                throw new BloqueHorarioInvalidoException($"El bloque de {b.HoraInicio:HH\\:mm} a {b.HoraFin:HH\\:mm} tiene la hora fin antes o igual a la hora inicio.");
            if (b.DiaSemana < 1 || b.DiaSemana > 6)
                throw new BloqueHorarioInvalidoException("El día de la semana debe estar entre lunes (1) y sábado (6).");

            var etiquetaValida = await db.EtiquetasPlanificacion
                .AnyAsync(e => e.Codigo == b.CodigoEtiqueta && e.CodigoArea == request.CodigoArea && e.Activo, cancellationToken);
            if (!etiquetaValida)
                throw new EtiquetaPlanificacionInvalidaException(b.CodigoEtiqueta);
        }

        var existente = await db.PlanesSemanales
            .Include(p => p.Bloques)
            .FirstOrDefaultAsync(p => p.CodigoArea == request.CodigoArea && p.FechaInicioSemana == request.FechaInicioSemana, cancellationToken);

        PlanSemanal plan;
        if (existente is null)
        {
            plan = new PlanSemanal
            {
                Id = Guid.NewGuid(),
                CodigoArea = request.CodigoArea,
                FechaInicioSemana = request.FechaInicioSemana,
                NombreResponsable = request.NombreResponsable,
                CargoResponsable = request.CargoResponsable,
                CreadoEn = DateTimeOffset.UtcNow,
                CreadoPor = request.RegistradoPor,
            };
            db.PlanesSemanales.Add(plan);
        }
        else
        {
            if (EstaBloqueada(existente))
                throw new PlanSemanalBloqueadoException();
            if (existente.Bloques.Any(b => !string.IsNullOrWhiteSpace(b.NotaGerencia)))
                throw new PlanSemanalConNotaGerenciaException();

            plan = existente;
            plan.NombreResponsable = request.NombreResponsable;
            plan.CargoResponsable = request.CargoResponsable;
            plan.ModificadoEn = DateTimeOffset.UtcNow;
            plan.ModificadoPor = request.RegistradoPor;
            db.PlanesSemanalesBloques.RemoveRange(existente.Bloques);
        }

        foreach (var b in request.Bloques)
        {
            db.PlanesSemanalesBloques.Add(new PlanSemanalBloque
            {
                Id = Guid.NewGuid(),
                IdPlanSemanal = plan.Id,
                DiaSemana = b.DiaSemana,
                HoraInicio = b.HoraInicio,
                HoraFin = b.HoraFin,
                CodigoEtiqueta = b.CodigoEtiqueta,
                Descripcion = b.Descripcion,
            });
        }

        await db.SaveChangesAsync(cancellationToken);
        return await MapearDtoAsync(plan.Id, cancellationToken);
    }

    public async Task<PlanSemanalDto> EnviarAsync(Guid idPlan, string usuarioActual, bool esGerencia, CancellationToken cancellationToken = default)
    {
        var plan = await db.PlanesSemanales.FirstOrDefaultAsync(p => p.Id == idPlan, cancellationToken)
            ?? throw new PlanSemanalNoExisteException(idPlan);

        if (!esGerencia)
            await ValidarPerteneceAlAreaAsync(plan.CodigoArea, usuarioActual, cancellationToken);

        var ahora = DateTimeOffset.UtcNow;
        var limite = FechaLimiteEnvio(plan.FechaInicioSemana);

        // Ya enviada y ya pasó el corte -> bloqueada por completo, ni
        // siquiera reenviar. Ya enviada pero todavía antes del corte ->
        // se permite "reenviar" (ej. después de una edición) sin
        // restricción. Nunca enviada y ya pasó el corte -> se permite
        // el único envío tardío real, marcado como tal.
        if (plan.Enviada && ahora >= limite)
            throw new PlanSemanalBloqueadoException();

        plan.Enviada = true;
        plan.FechaEnvio = ahora;
        plan.EnviadaFueraDeTiempo = ahora >= limite;
        plan.EnviadaPor = usuarioActual;
        await db.SaveChangesAsync(cancellationToken);

        return await MapearDtoAsync(idPlan, cancellationToken);
    }

    public async Task<IReadOnlyList<PlanSemanalListItemDto>> ListarAsync(ListarPlanesFiltro filtro, CancellationToken cancellationToken = default)
    {
        var query = db.PlanesSemanales.Include(p => p.Area).Include(p => p.Bloques).AsQueryable();

        // Sin permiso de gerencia, solo ve el área a la que está
        // vinculado su propio usuario -- nunca opcional del lado del
        // cliente, mismo criterio real ya aplicado en Mesa de Servicio
        // tras el hallazgo de privacidad de esa ronda.
        if (!filtro.EsGerencia)
        {
            var areaUsuario = await db.Usuarios
                .Where(u => u.NombreUsuario == filtro.UsuarioActual)
                .Select(u => u.CodigoAreaPlanificacion)
                .FirstOrDefaultAsync(cancellationToken);
            query = query.Where(p => p.CodigoArea == areaUsuario);
        }

        if (!string.IsNullOrWhiteSpace(filtro.CodigoArea))
            query = query.Where(p => p.CodigoArea == filtro.CodigoArea);
        if (filtro.Desde is not null)
            query = query.Where(p => p.FechaInicioSemana >= filtro.Desde);
        if (filtro.Hasta is not null)
            query = query.Where(p => p.FechaInicioSemana <= filtro.Hasta);

        var planes = await query
            .OrderByDescending(p => p.FechaInicioSemana)
            .Select(p => new
            {
                p.Id, p.CodigoArea, Area = p.Area.Nombre, p.FechaInicioSemana,
                p.NombreResponsable, CantidadBloques = p.Bloques.Count,
                p.Enviada, p.EnviadaFueraDeTiempo,
                p.CreadoEn, p.CreadoPor,
            })
            .ToListAsync(cancellationToken);

        return planes.Select(p => new PlanSemanalListItemDto(
            p.Id, p.CodigoArea, p.Area, p.FechaInicioSemana, p.NombreResponsable, p.CantidadBloques,
            p.Enviada, p.EnviadaFueraDeTiempo, EstaBloqueada(p.Enviada, p.FechaInicioSemana),
            p.CreadoEn, p.CreadoPor)).ToList();
    }

    public async Task<PlanSemanalDto> ObtenerAsync(Guid idPlan, string usuarioActual, bool esGerencia, CancellationToken cancellationToken = default)
    {
        var plan = await db.PlanesSemanales.FirstOrDefaultAsync(p => p.Id == idPlan, cancellationToken)
            ?? throw new PlanSemanalNoExisteException(idPlan);

        if (!esGerencia)
            await ValidarPerteneceAlAreaAsync(plan.CodigoArea, usuarioActual, cancellationToken);

        return await MapearDtoAsync(idPlan, cancellationToken);
    }

    public async Task<PlanSemanalDto> GuardarNotaGerenciaAsync(Guid idPlan, string? nota, CancellationToken cancellationToken = default)
    {
        var plan = await db.PlanesSemanales.FirstOrDefaultAsync(p => p.Id == idPlan, cancellationToken)
            ?? throw new PlanSemanalNoExisteException(idPlan);
        plan.NotaGerencia = string.IsNullOrWhiteSpace(nota) ? null : nota.Trim();
        await db.SaveChangesAsync(cancellationToken);
        return await MapearDtoAsync(idPlan, cancellationToken);
    }

    public async Task<PlanSemanalDto> GuardarNotaBloqueAsync(Guid idBloque, string? nota, CancellationToken cancellationToken = default)
    {
        var bloque = await db.PlanesSemanalesBloques.FirstOrDefaultAsync(b => b.Id == idBloque, cancellationToken)
            ?? throw new BloqueHorarioInvalidoException($"El bloque {idBloque} no existe.");
        bloque.NotaGerencia = string.IsNullOrWhiteSpace(nota) ? null : nota.Trim();
        await db.SaveChangesAsync(cancellationToken);
        return await MapearDtoAsync(bloque.IdPlanSemanal, cancellationToken);
    }

    private async Task ValidarPerteneceAlAreaAsync(string codigoArea, string usuarioActual, CancellationToken cancellationToken)
    {
        var areaUsuario = await db.Usuarios
            .Where(u => u.NombreUsuario == usuarioActual)
            .Select(u => u.CodigoAreaPlanificacion)
            .FirstOrDefaultAsync(cancellationToken);

        if (areaUsuario is null)
            throw new UsuarioSinAreaPlanificacionException();
        if (areaUsuario != codigoArea)
            throw new PlanSemanalAjenoException();
    }

    private static bool EstaBloqueada(PlanSemanal plan) => EstaBloqueada(plan.Enviada, plan.FechaInicioSemana);

    private static bool EstaBloqueada(bool enviada, DateOnly fechaInicioSemana) =>
        enviada && DateTimeOffset.UtcNow >= FechaLimiteEnvio(fechaInicioSemana);

    private async Task<PlanSemanalDto> MapearDtoAsync(Guid idPlan, CancellationToken cancellationToken)
    {
        var plan = await db.PlanesSemanales
            .Include(p => p.Area)
            .Include(p => p.Bloques).ThenInclude(b => b.Etiqueta)
            .FirstAsync(p => p.Id == idPlan, cancellationToken);

        var bloques = plan.Bloques
            .OrderBy(b => b.DiaSemana).ThenBy(b => b.HoraInicio)
            .Select(b => new BloqueDto(b.Id, b.DiaSemana, b.HoraInicio, b.HoraFin, b.CodigoEtiqueta, b.Etiqueta.Nombre, b.Etiqueta.ColorHex, b.Descripcion, b.NotaGerencia))
            .ToList();

        return new PlanSemanalDto(
            plan.Id, plan.CodigoArea, plan.Area.Nombre, plan.FechaInicioSemana,
            plan.NombreResponsable, plan.CargoResponsable,
            bloques,
            plan.Enviada, plan.FechaEnvio, plan.EnviadaFueraDeTiempo, plan.EnviadaPor,
            plan.NotaGerencia, EstaBloqueada(plan), FechaLimiteEnvio(plan.FechaInicioSemana),
            plan.CreadoEn, plan.CreadoPor, plan.ModificadoEn);
    }
}
