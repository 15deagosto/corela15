using Corela15.Application.Documentos;
using Corela15.Application.Planificacion;
using Corela15.Domain.Planificacion;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

public class PlanificacionService(Corela15DbContext db, IDocumentoStorageService storage) : IPlanificacionService
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

    private const string CodigoMenuPlanificacion = "planificacion";

    public async Task<EspacioPlanificacionDto> ObtenerMiEspacioAsync(string usuarioActual, CancellationToken cancellationToken = default)
    {
        var usuario = await db.Usuarios.Include(u => u.Persona)
            .FirstOrDefaultAsync(u => u.NombreUsuario == usuarioActual, cancellationToken)
            ?? throw new UsuarioSinAreaPlanificacionException();
        var espacio = await AsegurarEspacioPersonalAsync(usuario, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return new EspacioPlanificacionDto(espacio.Codigo, espacio.Nombre, usuario.NombreUsuario, true);
    }

    public async Task<IReadOnlyList<EspacioPlanificacionDto>> ListarEspaciosAsync(bool esGerencia, string usuarioActual, CancellationToken cancellationToken = default)
    {
        if (!esGerencia)
            return [await ObtenerMiEspacioAsync(usuarioActual, cancellationToken)];

        // Gerencia ve a TODAS las personas registradas y habilitadas (pedido
        // explícito del usuario: poder asignar disposiciones a cualquiera por
        // nombre, no solo a quien ya tiene el módulo). La cuenta genérica
        // "admin" no es una persona real (mismo criterio que la lista de
        // agentes de Mesa de Servicio). Se marca quién tiene acceso real al
        // menú (rol permanente, rol temporal vigente u otorgamiento directo,
        // sin exclusión -- mismo criterio de unión que AuthService), porque
        // sin acceso esa persona todavía no puede ver lo que se le asigna.
        var ahora = DateTimeOffset.UtcNow;
        var idsRol = db.RolesMenu
            .Where(rm => rm.Activo && rm.Menu.Activo && rm.Menu.Codigo == CodigoMenuPlanificacion)
            .Select(rm => rm.IdRol);
        var personas = await db.Usuarios.Include(u => u.Persona)
            .Where(u => u.Activo && u.PuedeIngresarSistema && u.NombreUsuario != "admin")
            .ToListAsync(cancellationToken);
        var idsConAcceso = (await db.Usuarios
            .Where(u => u.Activo)
            .Where(u => !db.UsuariosMenu.Any(um => um.IdUsuario == u.Id && um.Excluido && um.Menu.Codigo == CodigoMenuPlanificacion))
            .Where(u => u.UsuarioRoles.Any(ur => ur.Activo && idsRol.Contains(ur.IdRol))
                || db.UsuariosRolTemporal.Any(t => t.IdUsuario == u.Id && t.Activo && t.FechaCaducidad > ahora && idsRol.Contains(t.IdRol))
                || db.UsuariosMenu.Any(um => um.IdUsuario == u.Id && um.Activo && !um.Excluido && um.Menu.Codigo == CodigoMenuPlanificacion))
            .Select(u => u.Id)
            .ToListAsync(cancellationToken)).ToHashSet();

        foreach (var u in personas)
            await AsegurarEspacioPersonalAsync(u, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        var espacios = await db.AreasPlanificacion
            .Where(a => a.Activo && a.IdUsuario != null && a.Usuario!.Activo)
            .OrderBy(a => a.Nombre)
            .Select(a => new { a.Codigo, a.Nombre, a.Usuario!.NombreUsuario, IdUsuario = a.IdUsuario!.Value })
            .ToListAsync(cancellationToken);
        // Dos personas reales con el mismo nombre (pasa en los datos
        // importados de Softbank) se distinguen con su usuario.
        var repetidos = espacios.GroupBy(e => e.Nombre.Trim().ToUpperInvariant()).Where(g => g.Count() > 1).Select(g => g.Key).ToHashSet();
        return espacios
            .Select(e => new EspacioPlanificacionDto(
                e.Codigo,
                repetidos.Contains(e.Nombre.Trim().ToUpperInvariant()) ? $"{e.Nombre} ({e.NombreUsuario})" : e.Nombre,
                e.NombreUsuario, idsConAcceso.Contains(e.IdUsuario)))
            .ToList();
    }

    public async Task ValidarDuenoEspacioAsync(string codigoArea, string usuarioActual, CancellationToken cancellationToken = default)
        => await ValidarPerteneceAlAreaAsync(codigoArea, usuarioActual, cancellationToken);

    /// <summary>
    /// Crea (si no existe) el espacio personal del usuario y lo deja
    /// vinculado en Usuario.CodigoAreaPlanificacion. No guarda -- el caller
    /// hace SaveChanges. El nombre se sincroniza con el nombre real vigente.
    /// </summary>
    private async Task<Domain.Planificacion.AreaPlanificacion> AsegurarEspacioPersonalAsync(Domain.Seguridad.Usuario usuario, CancellationToken cancellationToken)
    {
        var nombre = usuario.Persona?.Nombre ?? usuario.NombreCompleto ?? usuario.NombreUsuario;
        if (nombre.Length > 100) nombre = nombre[..100];

        var espacio = db.AreasPlanificacion.Local.FirstOrDefault(a => a.IdUsuario == usuario.Id)
            ?? await db.AreasPlanificacion.FirstOrDefaultAsync(a => a.IdUsuario == usuario.Id, cancellationToken);

        if (espacio is null)
        {
            var baseCodigo = "P" + new string(usuario.NombreUsuario.ToUpperInvariant().Where(char.IsLetterOrDigit).ToArray());
            if (baseCodigo.Length > 12) baseCodigo = baseCodigo[..12];
            var codigo = baseCodigo;
            var sufijo = 1;
            while (db.AreasPlanificacion.Local.Any(a => a.Codigo == codigo)
                   || await db.AreasPlanificacion.AnyAsync(a => a.Codigo == codigo, cancellationToken))
                codigo = $"{baseCodigo}{++sufijo}";

            espacio = new Domain.Planificacion.AreaPlanificacion { Codigo = codigo, Nombre = nombre, Activo = true, IdUsuario = usuario.Id };
            db.AreasPlanificacion.Add(espacio);
        }
        else
        {
            espacio.Nombre = nombre;
            espacio.Activo = true;
        }

        if (usuario.CodigoAreaPlanificacion != espacio.Codigo)
            usuario.CodigoAreaPlanificacion = espacio.Codigo;

        return espacio;
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

    public async Task<EtiquetaDto> ActualizarEtiquetaAsync(string codigo, string nombre, string colorHex, string usuarioActual, CancellationToken cancellationToken = default)
    {
        var etiqueta = await db.EtiquetasPlanificacion.FirstOrDefaultAsync(e => e.Codigo == codigo, cancellationToken)
            ?? throw new EtiquetaPlanificacionInvalidaException(codigo);
        await ValidarPerteneceAlAreaAsync(etiqueta.CodigoArea, usuarioActual, cancellationToken);

        var nombreLimpio = nombre.Trim();
        if (string.IsNullOrWhiteSpace(nombreLimpio))
            throw new BloqueHorarioInvalidoException("El nombre de la etiqueta no puede estar vacío.");

        var duplicada = await db.EtiquetasPlanificacion.AnyAsync(
            e => e.CodigoArea == etiqueta.CodigoArea && e.Codigo != codigo && EF.Functions.ILike(e.Nombre, nombreLimpio),
            cancellationToken);
        if (duplicada)
            throw new NombreEtiquetaDuplicadoException(nombreLimpio);

        etiqueta.Nombre = nombreLimpio;
        etiqueta.ColorHex = colorHex;
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

        foreach (var i in request.Indicadores)
        {
            var indicadorValido = await db.Indicadores
                .AnyAsync(x => x.Id == i.IdIndicador && x.CodigoArea == request.CodigoArea && x.Activo, cancellationToken);
            if (!indicadorValido)
                throw new IndicadorPlanificacionInvalidoException(i.IdIndicador);
        }

        foreach (var idDisposicion in request.Bloques.Where(b => b.IdDisposicion is not null).Select(b => b.IdDisposicion!.Value).Distinct())
        {
            // Solo se puede etiquetar un bloque con una disposición
            // real asignada a ESTA área y todavía abierta -- nunca una
            // cerrada ni una de otra área, aunque el usuario conozca su
            // Id.
            var disposicionValida = await db.DisposicionesGerenciaArea
                .AnyAsync(a => a.IdDisposicion == idDisposicion && a.CodigoArea == request.CodigoArea
                    && a.Disposicion.Estado == EstadoDisposicionGerencia.Abierta, cancellationToken);
            if (!disposicionValida)
                throw new DisposicionGerenciaInvalidaException(idDisposicion);
        }

        var existente = await db.PlanesSemanales
            .Include(p => p.Bloques)
            .Include(p => p.Indicadores)
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

            // La protección contra perder una nota de gerencia solo aplica
            // pasado el corte real (viernes 17:00) -- antes de esa hora, un
            // comentario de gerencia suele ser justamente un pedido de
            // cambio, y el área tiene que poder reguardar libremente para
            // aplicarlo. Después del corte, la semana ya quedó bloqueada
            // por el chequeo de arriba salvo que nunca se haya enviado
            // (envío tardío pendiente) -- ahí sí protegemos la nota real
            // como registro histórico, porque ya no hay margen operativo
            // para "coordinar y volver a guardar".
            if (DateTimeOffset.UtcNow >= FechaLimiteEnvio(existente.FechaInicioSemana) &&
                existente.Bloques.Any(b => !string.IsNullOrWhiteSpace(b.NotaGerencia)))
            {
                throw new PlanSemanalConNotaGerenciaException();
            }

            plan = existente;
            plan.NombreResponsable = request.NombreResponsable;
            plan.CargoResponsable = request.CargoResponsable;
            plan.ModificadoEn = DateTimeOffset.UtcNow;
            plan.ModificadoPor = request.RegistradoPor;
            db.PlanesSemanalesBloques.RemoveRange(existente.Bloques);
            db.PlanesSemanalesIndicadores.RemoveRange(existente.Indicadores);

            // El contenido cambió -- cualquier revisión de gerencia ya
            // hecha (Aprobado/ConObservaciones) queda superada, vuelve a
            // Pendiente sola. La NotaGerencia general se conserva tal
            // cual (es el registro histórico de la observación, no se
            // borra), solo cambia el estado de la revisión.
            if (plan.EstadoAprobacion != EstadoAprobacionPlan.Pendiente)
            {
                plan.EstadoAprobacion = EstadoAprobacionPlan.Pendiente;
                plan.AprobadoPor = null;
                plan.FechaAprobacion = null;
            }
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
                IdDisposicion = b.IdDisposicion,
            });
        }

        foreach (var i in request.Indicadores)
        {
            db.PlanesSemanalesIndicadores.Add(new PlanSemanalIndicador
            {
                Id = Guid.NewGuid(),
                IdPlanSemanal = plan.Id,
                IdIndicador = i.IdIndicador,
                Meta = i.Meta,
                Real = i.Real,
                Comentario = string.IsNullOrWhiteSpace(i.Comentario) ? null : i.Comentario.Trim(),
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
            var areaUsuario = await db.AreasPlanificacion
                .Where(a => a.IdUsuario != null && a.Usuario!.NombreUsuario == filtro.UsuarioActual)
                .Select(a => a.Codigo)
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
                p.Enviada, p.EnviadaFueraDeTiempo, p.EstadoAprobacion,
                p.CreadoEn, p.CreadoPor,
            })
            .ToListAsync(cancellationToken);

        return planes.Select(p => new PlanSemanalListItemDto(
            p.Id, p.CodigoArea, p.Area, p.FechaInicioSemana, p.NombreResponsable, p.CantidadBloques,
            p.Enviada, p.EnviadaFueraDeTiempo, EstaBloqueada(p.Enviada, p.FechaInicioSemana),
            p.EstadoAprobacion.ToString(),
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

    public async Task<PlanSemanalDto> AprobarAsync(Guid idPlan, string usuarioActual, CancellationToken cancellationToken = default)
    {
        var plan = await db.PlanesSemanales.FirstOrDefaultAsync(p => p.Id == idPlan, cancellationToken)
            ?? throw new PlanSemanalNoExisteException(idPlan);
        if (!plan.Enviada)
            throw new PlanSemanalNoEnviadoException();

        plan.EstadoAprobacion = EstadoAprobacionPlan.Aprobado;
        plan.AprobadoPor = usuarioActual;
        plan.FechaAprobacion = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return await MapearDtoAsync(idPlan, cancellationToken);
    }

    public async Task<PlanSemanalDto> SolicitarCambiosAsync(Guid idPlan, string usuarioActual, string comentario, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(comentario))
            throw new ComentarioObligatorioException();

        var plan = await db.PlanesSemanales.FirstOrDefaultAsync(p => p.Id == idPlan, cancellationToken)
            ?? throw new PlanSemanalNoExisteException(idPlan);
        if (!plan.Enviada)
            throw new PlanSemanalNoEnviadoException();

        plan.EstadoAprobacion = EstadoAprobacionPlan.ConObservaciones;
        plan.AprobadoPor = usuarioActual;
        plan.FechaAprobacion = DateTimeOffset.UtcNow;
        plan.NotaGerencia = comentario.Trim();
        await db.SaveChangesAsync(cancellationToken);
        return await MapearDtoAsync(idPlan, cancellationToken);
    }

    public async Task<IndicadorDto> ObtenerOCrearIndicadorAsync(string codigoArea, string nombre, string? unidad, CancellationToken cancellationToken = default)
    {
        var nombreLimpio = nombre.Trim();
        if (string.IsNullOrWhiteSpace(nombreLimpio))
            throw new BloqueHorarioInvalidoException("El nombre del indicador no puede estar vacío.");

        var existente = await db.Indicadores
            .FirstOrDefaultAsync(i => i.CodigoArea == codigoArea && EF.Functions.ILike(i.Nombre, nombreLimpio), cancellationToken);
        if (existente is not null)
            return new IndicadorDto(existente.Id, existente.CodigoArea, existente.Nombre, existente.Unidad);

        if (!await db.AreasPlanificacion.AnyAsync(a => a.Codigo == codigoArea && a.Activo, cancellationToken))
            throw new AreaPlanificacionInvalidaException(codigoArea);

        var indicador = new Indicador
        {
            CodigoArea = codigoArea,
            Nombre = nombreLimpio,
            Unidad = string.IsNullOrWhiteSpace(unidad) ? null : unidad.Trim(),
            Activo = true,
        };
        db.Indicadores.Add(indicador);
        await db.SaveChangesAsync(cancellationToken);

        return new IndicadorDto(indicador.Id, indicador.CodigoArea, indicador.Nombre, indicador.Unidad);
    }

    public async Task<DisposicionGerenciaDto> CrearDisposicionAsync(CrearDisposicionRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Titulo))
            throw new BloqueHorarioInvalidoException("El título de la disposición no puede estar vacío.");
        if (request.CodigosArea.Count == 0)
            throw new DisposicionSinAreasException();

        var areasValidas = await db.AreasPlanificacion
            .Where(a => request.CodigosArea.Contains(a.Codigo) && a.Activo && a.IdUsuario != null)
            .Select(a => a.Codigo)
            .ToListAsync(cancellationToken);
        var invalidas = request.CodigosArea.Except(areasValidas).ToList();
        if (invalidas.Count > 0)
            throw new AreaPlanificacionInvalidaException(string.Join(", ", invalidas));

        var disposicion = new DisposicionGerencia
        {
            Id = Guid.NewGuid(),
            Titulo = request.Titulo.Trim(),
            Descripcion = string.IsNullOrWhiteSpace(request.Descripcion) ? null : request.Descripcion.Trim(),
            FechaLimite = request.FechaLimite,
            HoraLimite = request.HoraLimite,
            Estado = EstadoDisposicionGerencia.Abierta,
            CreadoEn = DateTimeOffset.UtcNow,
            CreadoPor = request.CreadoPor,
        };
        foreach (var codigoArea in areasValidas.Distinct())
            disposicion.Areas.Add(new DisposicionGerenciaArea { Id = Guid.NewGuid(), CodigoArea = codigoArea });

        db.DisposicionesGerencia.Add(disposicion);
        await db.SaveChangesAsync(cancellationToken);

        return await MapearDisposicionDtoAsync(disposicion.Id, cancellationToken);
    }

    public async Task CerrarDisposicionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var disposicion = await db.DisposicionesGerencia.FirstOrDefaultAsync(d => d.Id == id, cancellationToken)
            ?? throw new DisposicionGerenciaInvalidaException(id);
        disposicion.Estado = EstadoDisposicionGerencia.Cerrada;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<DisposicionGerenciaDto> CalificarCumplimientoAsync(Guid idDisposicion, string codigoArea, string cumplimiento, string? comentario, string calificadoPor, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<CumplimientoDisposicion>(cumplimiento, ignoreCase: true, out var valor))
            throw new CumplimientoInvalidoException(cumplimiento);

        var asignacion = await db.DisposicionesGerenciaArea
            .FirstOrDefaultAsync(a => a.IdDisposicion == idDisposicion && a.CodigoArea == codigoArea, cancellationToken)
            ?? throw new DisposicionAreaInvalidaException();

        asignacion.Cumplimiento = valor;
        asignacion.ComentarioCumplimiento = string.IsNullOrWhiteSpace(comentario) ? null : comentario.Trim();
        asignacion.CalificadoPor = calificadoPor;
        asignacion.FechaCalificacion = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return await MapearDisposicionDtoAsync(idDisposicion, cancellationToken);
    }

    private const int MaxMbEntregable = 20;
    private static readonly HashSet<string> ExtensionesEntregable =
        new(StringComparer.OrdinalIgnoreCase) { "pdf", "doc", "docx", "xls", "xlsx", "ppt", "pptx", "txt", "csv", "jpg", "jpeg", "png" };

    public async Task<DisposicionGerenciaDto> SubirEntregableAsync(Guid idDisposicion, string codigoArea, EntregableArchivoEntrada archivo, string? comentario, string usuarioActual, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(archivo.NombreOriginal).TrimStart('.');
        if (!ExtensionesEntregable.Contains(extension))
            throw new ArchivoEntregableInvalidoException($"Tipo de archivo .{extension} no permitido. Se aceptan: {string.Join(", ", ExtensionesEntregable)}");
        if (archivo.TamanoBytes <= 0)
            throw new ArchivoEntregableInvalidoException("El archivo está vacío");
        if (archivo.TamanoBytes > MaxMbEntregable * 1024L * 1024L)
            throw new ArchivoEntregableInvalidoException($"El archivo supera {MaxMbEntregable} MB");

        // Solo la persona dueña de ese espacio entrega lo suyo.
        await ValidarPerteneceAlAreaAsync(codigoArea, usuarioActual, cancellationToken);

        var asignacion = await db.DisposicionesGerenciaArea
            .Include(a => a.Disposicion)
            .FirstOrDefaultAsync(a => a.IdDisposicion == idDisposicion && a.CodigoArea == codigoArea, cancellationToken)
            ?? throw new DisposicionAreaInvalidaException();
        if (asignacion.Disposicion.Estado != EstadoDisposicionGerencia.Abierta)
            throw new EntregableNoPermitidoException("La disposición ya está cerrada -- no se pueden subir más entregables");
        if (asignacion.Cumplimiento != CumplimientoDisposicion.SinEvaluar)
            throw new EntregableNoPermitidoException("Gerencia ya calificó tu cumplimiento de esta disposición -- no se pueden subir más entregables");

        var ruta = await storage.GuardarAsync(archivo.Contenido, archivo.NombreOriginal, cancellationToken);
        db.EntregablesDisposicion.Add(new EntregableDisposicion
        {
            Id = Guid.NewGuid(),
            IdDisposicionArea = asignacion.Id,
            Comentario = string.IsNullOrWhiteSpace(comentario) ? null : comentario.Trim(),
            RutaArchivo = ruta,
            NombreArchivo = Path.GetFileName(archivo.NombreOriginal),
            ContentType = archivo.ContentType,
            TamanoBytes = archivo.TamanoBytes,
            Activo = true,
            CreadoEn = DateTimeOffset.UtcNow,
            CreadoPor = usuarioActual,
        });
        await db.SaveChangesAsync(cancellationToken);
        return await MapearDisposicionDtoAsync(idDisposicion, cancellationToken);
    }

    public async Task<EntregableDescarga> DescargarEntregableAsync(Guid idEntregable, string usuarioActual, bool esGerencia, CancellationToken cancellationToken = default)
    {
        var entregable = await db.EntregablesDisposicion
            .Include(e => e.DisposicionArea).ThenInclude(a => a.Area).ThenInclude(a => a.Usuario)
            .FirstOrDefaultAsync(e => e.Id == idEntregable && e.Activo, cancellationToken)
            ?? throw new EntregableNoPermitidoException("El entregable no existe");

        // Sin permiso de gerencia: solo quien participa en ESA disposición
        // (la persona dueña, o un compañero asignado a la misma disposición
        // compartida) ve el entregable. Cualquier otro lo ve igual que uno
        // inexistente -- nunca se filtran disposiciones de otras personas.
        if (!esGerencia)
        {
            var participa = await db.DisposicionesGerenciaArea.AnyAsync(a =>
                a.IdDisposicion == entregable.DisposicionArea.IdDisposicion
                && a.Area.Usuario != null && a.Area.Usuario.NombreUsuario == usuarioActual, cancellationToken);
            if (!participa)
                throw new EntregableNoPermitidoException("El entregable no existe");
        }

        var contenido = await storage.AbrirAsync(entregable.RutaArchivo, cancellationToken);
        return new EntregableDescarga(contenido, entregable.NombreArchivo, entregable.ContentType);
    }

    public async Task<DisposicionGerenciaDto> QuitarEntregableAsync(Guid idEntregable, string usuarioActual, CancellationToken cancellationToken = default)
    {
        var entregable = await db.EntregablesDisposicion
            .Include(e => e.DisposicionArea).ThenInclude(a => a.Disposicion)
            .FirstOrDefaultAsync(e => e.Id == idEntregable && e.Activo, cancellationToken)
            ?? throw new EntregableNoPermitidoException("El entregable no existe");

        await ValidarPerteneceAlAreaAsync(entregable.DisposicionArea.CodigoArea, usuarioActual, cancellationToken);
        if (entregable.DisposicionArea.Disposicion.Estado != EstadoDisposicionGerencia.Abierta
            || entregable.DisposicionArea.Cumplimiento != CumplimientoDisposicion.SinEvaluar)
            throw new EntregableNoPermitidoException("Ya no se puede quitar: la disposición está cerrada o gerencia ya calificó tu cumplimiento");

        entregable.Activo = false;
        await db.SaveChangesAsync(cancellationToken);
        return await MapearDisposicionDtoAsync(entregable.DisposicionArea.IdDisposicion, cancellationToken);
    }

    public async Task<IReadOnlyList<DisposicionGerenciaDto>> ListarDisposicionesAsync(bool esGerencia, string? codigoArea, CancellationToken cancellationToken = default)
    {
        var query = db.DisposicionesGerencia.Include(d => d.Areas).AsQueryable();

        if (!esGerencia)
        {
            // Un área normal solo ve las disposiciones reales que le
            // asignaron, y solo mientras estén abiertas -- una cerrada
            // ya no tiene sentido ofrecerla para etiquetar bloques
            // nuevos (sigue visible en los bloques ya guardados vía
            // BloqueDto.TituloDisposicion, pero no en este listado).
            if (string.IsNullOrWhiteSpace(codigoArea)) return [];
            query = query.Where(d => d.Estado == EstadoDisposicionGerencia.Abierta && d.Areas.Any(a => a.CodigoArea == codigoArea));
        }
        else if (!string.IsNullOrWhiteSpace(codigoArea))
        {
            // Gerencia revisando un área puntual (ej. desde el detalle de
            // un plan semanal) -- ve todas las disposiciones de esa área
            // sin importar el estado, para poder calificar cumplimiento
            // incluso de una ya cerrada.
            query = query.Where(d => d.Areas.Any(a => a.CodigoArea == codigoArea));
        }

        var ids = await query.OrderByDescending(d => d.FechaLimite).ThenByDescending(d => d.HoraLimite).Select(d => d.Id).ToListAsync(cancellationToken);
        var resultado = new List<DisposicionGerenciaDto>();
        foreach (var id in ids)
            resultado.Add(await MapearDisposicionDtoAsync(id, cancellationToken));
        return resultado;
    }

    private async Task<DisposicionGerenciaDto> MapearDisposicionDtoAsync(Guid idDisposicion, CancellationToken cancellationToken)
    {
        var d = await db.DisposicionesGerencia
            .Include(x => x.Areas).ThenInclude(a => a.Area)
            .FirstAsync(x => x.Id == idDisposicion, cancellationToken);

        // Horas dedicadas reales por área = suma de la duración de todos
        // los bloques (cualquier semana) de esa área etiquetados con esta
        // disposición -- el avance real que gerencia pidió ver, sin que
        // nadie lo reporte aparte.
        var bloques = await db.PlanesSemanalesBloques
            .Where(b => b.IdDisposicion == idDisposicion)
            .Select(b => new { b.PlanSemanal.CodigoArea, b.HoraInicio, b.HoraFin })
            .ToListAsync(cancellationToken);
        var horasPorArea = bloques
            .GroupBy(b => b.CodigoArea)
            .ToDictionary(g => g.Key, g => g.Sum(b => (decimal)(b.HoraFin.ToTimeSpan() - b.HoraInicio.ToTimeSpan()).TotalHours));

        var idsAsignacion = d.Areas.Select(a => a.Id).ToList();
        var entregables = (await db.EntregablesDisposicion
            .Where(e => idsAsignacion.Contains(e.IdDisposicionArea) && e.Activo)
            .OrderBy(e => e.CreadoEn)
            .ToListAsync(cancellationToken))
            .GroupBy(e => e.IdDisposicionArea)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<EntregableDto>)g
                .Select(e => new EntregableDto(e.Id, e.NombreArchivo, e.ContentType, e.TamanoBytes, e.Comentario, e.CreadoEn, e.CreadoPor))
                .ToList());

        var areas = d.Areas.Select(a => new DisposicionAreaDto(
            a.CodigoArea, a.Area.Nombre, a.Cumplimiento.ToString(), a.ComentarioCumplimiento,
            a.CalificadoPor, a.FechaCalificacion, horasPorArea.GetValueOrDefault(a.CodigoArea, 0m),
            entregables.GetValueOrDefault(a.Id, []))).ToList();

        return new DisposicionGerenciaDto(d.Id, d.Titulo, d.Descripcion, d.FechaLimite, d.HoraLimite, d.Estado.ToString(),
            areas, areas.Sum(a => a.HorasDedicadas), d.CreadoEn, d.CreadoPor);
    }

    private async Task ValidarPerteneceAlAreaAsync(string codigoArea, string usuarioActual, CancellationToken cancellationToken)
    {
        // El dueño real es el usuario vinculado al espacio personal --
        // nunca un área de departamento heredada (esas quedaron inactivas
        // y solo de historia, nadie puede escribir en ellas).
        var dueno = await db.AreasPlanificacion
            .Where(a => a.Codigo == codigoArea)
            .Select(a => a.IdUsuario == null ? null : a.Usuario!.NombreUsuario)
            .FirstOrDefaultAsync(cancellationToken);

        if (dueno is null || dueno != usuarioActual)
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
            .Include(p => p.Bloques).ThenInclude(b => b.Disposicion)
            .Include(p => p.Indicadores).ThenInclude(i => i.Indicador)
            .FirstAsync(p => p.Id == idPlan, cancellationToken);

        var bloques = plan.Bloques
            .OrderBy(b => b.DiaSemana).ThenBy(b => b.HoraInicio)
            .Select(b => new BloqueDto(b.Id, b.DiaSemana, b.HoraInicio, b.HoraFin, b.CodigoEtiqueta, b.Etiqueta.Nombre, b.Etiqueta.ColorHex, b.Descripcion, b.NotaGerencia, b.IdDisposicion, b.Disposicion?.Titulo))
            .ToList();

        var indicadores = plan.Indicadores
            .OrderBy(i => i.Indicador.Nombre)
            .Select(i => new IndicadorSemanalDto(i.Id, i.IdIndicador, i.Indicador.Nombre, i.Indicador.Unidad, i.Meta, i.Real, i.Comentario))
            .ToList();

        return new PlanSemanalDto(
            plan.Id, plan.CodigoArea, plan.Area.Nombre, plan.FechaInicioSemana,
            plan.NombreResponsable, plan.CargoResponsable,
            bloques, indicadores,
            plan.Enviada, plan.FechaEnvio, plan.EnviadaFueraDeTiempo, plan.EnviadaPor,
            plan.NotaGerencia, plan.EstadoAprobacion.ToString(), plan.AprobadoPor, plan.FechaAprobacion,
            EstaBloqueada(plan), FechaLimiteEnvio(plan.FechaInicioSemana),
            plan.CreadoEn, plan.CreadoPor, plan.ModificadoEn);
    }
}
