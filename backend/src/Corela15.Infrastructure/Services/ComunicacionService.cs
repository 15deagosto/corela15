using Corela15.Application.Comunicacion;
using Corela15.Domain.Comunicacion;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

public class ComunicacionService(Corela15DbContext db, IComunicacionNotificador notificador) : IComunicacionService
{
    public async Task<CanalDto> CrearCanalAsync(CrearCanalRequest request, CancellationToken cancellationToken = default)
    {
        var nombre = request.Nombre.Trim();
        if (nombre.Length == 0)
        {
            throw new NombreCanalInvalidoException();
        }

        var idsMiembros = request.IdsMiembrosIniciales.Append(request.CreadoPor).Distinct().ToList();
        var idsValidos = await db.Usuarios
            .Where(u => idsMiembros.Contains(u.Id) && u.Activo)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        var ahora = DateTimeOffset.UtcNow;
        var canal = new Canal
        {
            Id = Guid.NewGuid(),
            Nombre = nombre,
            Descripcion = request.Descripcion?.Trim(),
            EsDirecto = false,
            CreadoPor = request.CreadoPor.ToString(),
            CreadoEn = ahora,
        };
        db.Canales.Add(canal);

        foreach (var idUsuario in idsValidos)
        {
            db.CanalesMiembros.Add(new CanalMiembro
            {
                Id = Guid.NewGuid(),
                IdCanal = canal.Id,
                IdUsuario = idUsuario,
                Activo = true,
                CreadoEn = ahora,
            });
        }

        await db.SaveChangesAsync(cancellationToken);

        var dto = new CanalDto(canal.Id, canal.Nombre, canal.Descripcion, canal.EsDirecto, canal.CreadoEn);

        foreach (var idUsuario in idsValidos.Where(id => id != request.CreadoPor))
        {
            await notificador.NotificarAgregadoACanalAsync(idUsuario, dto, cancellationToken);
        }

        return dto;
    }

    public async Task<CanalDto> ObtenerOCrearDirectoAsync(
        Guid idUsuarioA, Guid idUsuarioB, CancellationToken cancellationToken = default)
    {
        var clave = ClaveDirecta(idUsuarioA, idUsuarioB);

        var existente = await db.Canales.FirstOrDefaultAsync(c => c.ClaveDirecta == clave, cancellationToken);
        if (existente is not null)
        {
            return new CanalDto(existente.Id, existente.Nombre, existente.Descripcion, existente.EsDirecto, existente.CreadoEn);
        }

        var otroActivo = await db.Usuarios.AnyAsync(u => u.Id == idUsuarioB && u.Activo, cancellationToken);
        if (!otroActivo)
        {
            throw new UsuarioInvalidoParaChatException(idUsuarioB);
        }

        var ahora = DateTimeOffset.UtcNow;
        var canal = new Canal
        {
            Id = Guid.NewGuid(),
            EsDirecto = true,
            ClaveDirecta = clave,
            CreadoPor = idUsuarioA.ToString(),
            CreadoEn = ahora,
        };
        db.Canales.Add(canal);
        db.CanalesMiembros.AddRange(
            new CanalMiembro { Id = Guid.NewGuid(), IdCanal = canal.Id, IdUsuario = idUsuarioA, Activo = true, CreadoEn = ahora },
            new CanalMiembro { Id = Guid.NewGuid(), IdCanal = canal.Id, IdUsuario = idUsuarioB, Activo = true, CreadoEn = ahora });

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Condición de carrera real: los dos usuarios se escribieron
            // "al mismo tiempo" y ambos intentaron crear la conversación —
            // el índice único de ClaveDirecta rechazó al segundo. Mismo
            // criterio ya usado en el resto del core: reintentar como
            // lectura en vez de propagar un 500.
            var creadaPorElOtro = await db.Canales.FirstAsync(c => c.ClaveDirecta == clave, cancellationToken);
            return new CanalDto(creadaPorElOtro.Id, creadaPorElOtro.Nombre, creadaPorElOtro.Descripcion, creadaPorElOtro.EsDirecto, creadaPorElOtro.CreadoEn);
        }

        var dto = new CanalDto(canal.Id, canal.Nombre, canal.Descripcion, canal.EsDirecto, canal.CreadoEn);
        await notificador.NotificarAgregadoACanalAsync(idUsuarioB, dto, cancellationToken);
        return dto;
    }

    public async Task AgregarMiembroAsync(
        Guid idCanal, Guid idUsuarioNuevo, string ejecutadoPor, CancellationToken cancellationToken = default)
    {
        var canal = await db.Canales.FirstOrDefaultAsync(c => c.Id == idCanal && c.Activo, cancellationToken)
            ?? throw new CanalNoExisteException(idCanal);

        if (canal.EsDirecto)
        {
            throw new NoSePuedeSalirDeConversacionDirectaException();
        }

        var usuarioActivo = await db.Usuarios.AnyAsync(u => u.Id == idUsuarioNuevo && u.Activo, cancellationToken);
        if (!usuarioActivo)
        {
            throw new UsuarioInvalidoParaChatException(idUsuarioNuevo);
        }

        var membresia = await db.CanalesMiembros
            .FirstOrDefaultAsync(m => m.IdCanal == idCanal && m.IdUsuario == idUsuarioNuevo, cancellationToken);

        if (membresia is null)
        {
            db.CanalesMiembros.Add(new CanalMiembro
            {
                Id = Guid.NewGuid(),
                IdCanal = idCanal,
                IdUsuario = idUsuarioNuevo,
                Activo = true,
                CreadoEn = DateTimeOffset.UtcNow,
            });
        }
        else if (!membresia.Activo)
        {
            membresia.Activo = true;
        }
        else
        {
            return; // ya es miembro — idempotente, no hace falta notificar de nuevo
        }

        await db.SaveChangesAsync(cancellationToken);

        var dto = new CanalDto(canal.Id, canal.Nombre, canal.Descripcion, canal.EsDirecto, canal.CreadoEn);
        await notificador.NotificarAgregadoACanalAsync(idUsuarioNuevo, dto, cancellationToken);
    }

    public async Task SalirDelCanalAsync(Guid idCanal, Guid idUsuario, CancellationToken cancellationToken = default)
    {
        var canal = await db.Canales.FirstOrDefaultAsync(c => c.Id == idCanal, cancellationToken)
            ?? throw new CanalNoExisteException(idCanal);

        if (canal.EsDirecto)
        {
            throw new NoSePuedeSalirDeConversacionDirectaException();
        }

        var membresia = await db.CanalesMiembros
            .FirstOrDefaultAsync(m => m.IdCanal == idCanal && m.IdUsuario == idUsuario && m.Activo, cancellationToken);
        if (membresia is null)
        {
            return; // idempotente
        }

        membresia.Activo = false;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<MensajeDto> EnviarMensajeAsync(
        Guid idCanal, Guid idUsuarioRemitente, string texto, CancellationToken cancellationToken = default)
    {
        var textoLimpio = texto.Trim();
        if (textoLimpio.Length == 0 || textoLimpio.Length > 2000)
        {
            throw new TextoMensajeInvalidoException();
        }

        var canalActivo = await db.Canales.AnyAsync(c => c.Id == idCanal && c.Activo, cancellationToken);
        if (!canalActivo)
        {
            throw new CanalNoExisteException(idCanal);
        }

        var esMiembro = await db.CanalesMiembros
            .AnyAsync(m => m.IdCanal == idCanal && m.IdUsuario == idUsuarioRemitente && m.Activo, cancellationToken);
        if (!esMiembro)
        {
            throw new NoEsMiembroDelCanalException();
        }

        var nombreRemitente = await db.Usuarios
            .Where(u => u.Id == idUsuarioRemitente)
            .Select(u => u.Persona != null ? u.Persona.Nombre : u.NombreCompleto)
            .FirstOrDefaultAsync(cancellationToken) ?? "Usuario";

        var mensaje = new Mensaje
        {
            Id = Guid.NewGuid(),
            IdCanal = idCanal,
            IdUsuarioRemitente = idUsuarioRemitente,
            Texto = textoLimpio,
            CreadoEn = DateTimeOffset.UtcNow,
        };
        db.Mensajes.Add(mensaje);
        await db.SaveChangesAsync(cancellationToken);

        // El propio remitente marca el canal como leído hasta su mensaje —
        // nunca debería aparecer como "no leído" para quien lo escribió.
        var membresiaPropia = await db.CanalesMiembros
            .FirstOrDefaultAsync(m => m.IdCanal == idCanal && m.IdUsuario == idUsuarioRemitente, cancellationToken);
        if (membresiaPropia is not null)
        {
            membresiaPropia.FechaUltimaLectura = mensaje.CreadoEn;
            await db.SaveChangesAsync(cancellationToken);
        }

        var dto = new MensajeDto(mensaje.Id, mensaje.IdCanal, mensaje.IdUsuarioRemitente, nombreRemitente, mensaje.Texto, mensaje.CreadoEn);
        await notificador.NotificarMensajeNuevoAsync(idCanal, dto, cancellationToken);
        return dto;
    }

    public async Task<IReadOnlyList<CanalListItemDto>> ListarCanalesAsync(Guid idUsuario, CancellationToken cancellationToken = default)
    {
        var membresias = await db.CanalesMiembros
            .Where(m => m.IdUsuario == idUsuario && m.Activo)
            .Select(m => new { m.IdCanal, m.FechaUltimaLectura, m.Canal.Nombre, m.Canal.EsDirecto })
            .ToListAsync(cancellationToken);

        var resultado = new List<CanalListItemDto>(membresias.Count);
        foreach (var m in membresias)
        {
            var nombreMostrado = m.Nombre;
            if (m.EsDirecto)
            {
                nombreMostrado = await db.CanalesMiembros
                    .Where(x => x.IdCanal == m.IdCanal && x.IdUsuario != idUsuario)
                    .Select(x => x.Usuario.Persona != null ? x.Usuario.Persona.Nombre : x.Usuario.NombreCompleto)
                    .FirstOrDefaultAsync(cancellationToken) ?? "(usuario)";
            }

            var ultimoMensaje = await db.Mensajes
                .Where(msg => msg.IdCanal == m.IdCanal)
                .OrderByDescending(msg => msg.CreadoEn)
                .Select(msg => new { msg.Texto, Autor = (msg.UsuarioRemitente.Persona != null ? msg.UsuarioRemitente.Persona.Nombre : msg.UsuarioRemitente.NombreCompleto) ?? "Usuario", msg.CreadoEn })
                .FirstOrDefaultAsync(cancellationToken);

            var desde = m.FechaUltimaLectura ?? DateTimeOffset.MinValue;
            var noLeidos = await db.Mensajes
                .CountAsync(msg => msg.IdCanal == m.IdCanal && msg.CreadoEn > desde && msg.IdUsuarioRemitente != idUsuario, cancellationToken);

            resultado.Add(new CanalListItemDto(
                m.IdCanal, nombreMostrado ?? "(sin nombre)", m.EsDirecto,
                ultimoMensaje?.Texto, ultimoMensaje?.Autor, ultimoMensaje?.CreadoEn, noLeidos));
        }

        return resultado
            .OrderByDescending(c => c.UltimoMensajeFecha ?? DateTimeOffset.MinValue)
            .ToList();
    }

    public async Task<IReadOnlyList<MensajeDto>> ListarMensajesAsync(
        Guid idCanal, Guid idUsuario, Guid? antesDe, CancellationToken cancellationToken = default)
    {
        var esMiembro = await db.CanalesMiembros.AnyAsync(m => m.IdCanal == idCanal && m.IdUsuario == idUsuario, cancellationToken);
        if (!esMiembro)
        {
            throw new NoEsMiembroDelCanalException();
        }

        var query = db.Mensajes.Where(m => m.IdCanal == idCanal);

        if (antesDe is Guid idAntesDe)
        {
            var fechaCorte = await db.Mensajes.Where(m => m.Id == idAntesDe).Select(m => m.CreadoEn).FirstOrDefaultAsync(cancellationToken);
            query = query.Where(m => m.CreadoEn < fechaCorte);
        }

        var mensajes = await query
            .OrderByDescending(m => m.CreadoEn)
            .Take(50)
            .Select(m => new MensajeDto(
                m.Id, m.IdCanal, m.IdUsuarioRemitente,
                (m.UsuarioRemitente.Persona != null ? m.UsuarioRemitente.Persona.Nombre : m.UsuarioRemitente.NombreCompleto) ?? "Usuario",
                m.Texto, m.CreadoEn))
            .ToListAsync(cancellationToken);

        mensajes.Reverse();
        return mensajes;
    }

    public async Task MarcarLeidoAsync(Guid idCanal, Guid idUsuario, CancellationToken cancellationToken = default)
    {
        var membresia = await db.CanalesMiembros
            .FirstOrDefaultAsync(m => m.IdCanal == idCanal && m.IdUsuario == idUsuario, cancellationToken);
        if (membresia is null)
        {
            throw new NoEsMiembroDelCanalException();
        }

        membresia.FechaUltimaLectura = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UsuarioParaChatDto>> BuscarUsuariosAsync(
        string? q, Guid idUsuarioExcluir, CancellationToken cancellationToken = default)
    {
        var query = db.Usuarios.Where(u => u.Activo && u.Id != idUsuarioExcluir);
        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(u =>
                EF.Functions.ILike(u.NombreUsuario, $"%{q}%") ||
                EF.Functions.ILike(u.NombreCompleto ?? "", $"%{q}%") ||
                (u.Persona != null && EF.Functions.ILike(u.Persona.Nombre, $"%{q}%")));
        }

        return await query
            .OrderBy(u => u.NombreUsuario)
            .Take(20)
            .Select(u => new UsuarioParaChatDto(u.Id, u.Persona != null ? u.Persona.Nombre : (u.NombreCompleto ?? u.NombreUsuario)))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Guid>> ListarIdsCanalesDelUsuarioAsync(Guid idUsuario, CancellationToken cancellationToken = default)
    {
        return await db.CanalesMiembros
            .Where(m => m.IdUsuario == idUsuario && m.Activo)
            .Select(m => m.IdCanal)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CanalDescubribleDto>> ListarCanalesDescubriblesAsync(Guid idUsuario, CancellationToken cancellationToken = default)
    {
        var idsMiembro = await db.CanalesMiembros
            .Where(m => m.IdUsuario == idUsuario && m.Activo)
            .Select(m => m.IdCanal)
            .ToListAsync(cancellationToken);

        return await db.Canales
            .Where(c => c.Activo && !c.EsDirecto)
            .OrderBy(c => c.Nombre)
            .Select(c => new CanalDescubribleDto(c.Id, c.Nombre!, c.Descripcion, idsMiembro.Contains(c.Id)))
            .ToListAsync(cancellationToken);
    }

    private static string ClaveDirecta(Guid idA, Guid idB)
    {
        var (menor, mayor) = idA.CompareTo(idB) <= 0 ? (idA, idB) : (idB, idA);
        return $"{menor}_{mayor}";
    }
}
