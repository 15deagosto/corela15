using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Corela15.Application.Seguridad;
using Corela15.Domain.Seguridad;
using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Corela15.Infrastructure.Services;

public class AuthService(Corela15DbContext db, IConfiguration configuration) : IAuthService
{
    public async Task<LoginResult> LoginAsync(
        LoginRequest request, string? direccionIp, CancellationToken cancellationToken = default)
    {
        var usuario = await db.Usuarios
            .FirstOrDefaultAsync(u => u.NombreUsuario == request.NombreUsuario, cancellationToken);

        if (usuario is null || !BCrypt.Net.BCrypt.Verify(request.Contrasena, usuario.HashContrasena))
        {
            await RegistrarIntentoAsync(usuario?.Id, exitoso: false, direccionIp, "Credenciales inválidas", cancellationToken);
            throw new CredencialesInvalidasException();
        }

        if (!usuario.Activo || !usuario.PuedeIngresarSistema || usuario.TieneBloqueo)
        {
            await RegistrarIntentoAsync(usuario.Id, exitoso: false, direccionIp, "Usuario no habilitado", cancellationToken);
            throw new UsuarioNoHabilitadoException();
        }

        if (!await DentroDeHorarioAsync(usuario.Id, cancellationToken))
        {
            await RegistrarIntentoAsync(usuario.Id, exitoso: false, direccionIp, "Fuera del horario de acceso autorizado", cancellationToken);
            throw new FueraDeHorarioException();
        }

        await RegistrarIntentoAsync(usuario.Id, exitoso: true, direccionIp, null, cancellationToken);

        return await EmitirTokenAsync(usuario, direccionIp, cancellationToken);
    }

    public async Task<LoginResult> RefrescarAsync(
        Guid idUsuario, Guid idSesionActual, string? direccionIp, CancellationToken cancellationToken = default)
    {
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == idUsuario, cancellationToken)
            ?? throw new UsuarioNoExisteException(idUsuario);

        // Mismas validaciones que un login real — si a alguien lo
        // bloquearon o deshabilitaron mientras tenía la sesión abierta,
        // refrescar no debe emitirle un token nuevo, tiene que cortarle el
        // acceso acá mismo, no solo dejarlo con el viejo hasta que expire.
        if (!usuario.Activo || !usuario.PuedeIngresarSistema || usuario.TieneBloqueo)
        {
            throw new UsuarioNoHabilitadoException();
        }

        var resultado = await EmitirTokenAsync(usuario, direccionIp, cancellationToken);

        // La sesión vieja se revoca DESPUÉS de emitir la nueva — revocarla
        // antes arriesgaría dejar al usuario sin ninguna sesión válida si
        // algo falla en el medio. Nunca revoca la propia sesión recién
        // creada (jti distinto por diseño de EmitirTokenAsync).
        var sesionVieja = await db.SesionesUsuario.FirstOrDefaultAsync(s => s.Id == idSesionActual, cancellationToken);
        if (sesionVieja is not null && !sesionVieja.Revocada)
        {
            sesionVieja.Revocada = true;
            sesionVieja.RevocadaEn = DateTimeOffset.UtcNow;
            sesionVieja.RevocadaPor = "sistema:refresco_permisos";
            await db.SaveChangesAsync(cancellationToken);
        }

        return resultado;
    }

    // Cálculo real compartido entre LoginAsync y RefrescarAsync — nunca
    // duplicado. Roles efectivos = permanentes (usuario_rol) ∪ temporales
    // vigentes (usuario_rol_temporal, no vencidos); menús = rol_menu real
    // + Mesa de Servicio siempre (transversal por diseño); estructuras =
    // segundo nivel de permiso (OF01 y lo que se sume); agencia efectiva =
    // reasignación temporal vigente si existe, si no la real del usuario.
    private async Task<LoginResult> EmitirTokenAsync(
        Usuario usuario, string? direccionIp, CancellationToken cancellationToken)
    {
        var ahora = DateTimeOffset.UtcNow;

        var idsRolPermanentes = await db.UsuarioRoles
            .Where(ur => ur.IdUsuario == usuario.Id && ur.Activo)
            .Select(ur => ur.IdRol)
            .ToListAsync(cancellationToken);
        var idsRolTemporales = await db.UsuariosRolTemporal
            .Where(rt => rt.IdUsuario == usuario.Id && rt.Activo && rt.FechaCaducidad > ahora)
            .Select(rt => rt.IdRol)
            .ToListAsync(cancellationToken);
        var idsRolEfectivos = idsRolPermanentes.Union(idsRolTemporales).ToList();

        var roles = await db.Roles
            .Where(r => idsRolEfectivos.Contains(r.Id))
            .Select(r => r.Nombre)
            .ToListAsync(cancellationToken);

        var menusPorRol = await db.RolesMenu
            .Where(rm => rm.Activo && rm.Menu.Activo && idsRolEfectivos.Contains(rm.IdRol))
            .Select(rm => rm.Menu.Codigo)
            .ToListAsync(cancellationToken);

        // Otorgamiento directo a la persona, además de lo que ya le da el
        // rol -- nunca resta, solo suma (ver UsuarioMenu.cs). Cierra el
        // pedido real de dar acceso a un módulo puntual a una sola persona
        // sin tener que crear o tocar un rol para eso.
        var menusPorUsuario = await db.UsuariosMenu
            .Where(um => um.Activo && !um.Excluido && um.Menu.Activo && um.IdUsuario == usuario.Id)
            .Select(um => um.Menu.Codigo)
            .ToListAsync(cancellationToken);

        // Exclusión real: la única forma de "restar" lo que un rol ya
        // otorga -- tiene prioridad absoluta, se aplica DESPUÉS de la
        // unión rol∪directo (ver UsuarioMenu.Excluido). "mesa-servicio"
        // queda deliberadamente fuera: se agrega siempre después, sin
        // excepción para nadie (requisito SEPS universal).
        var menusExcluidos = await db.UsuariosMenu
            .Where(um => um.Excluido && um.IdUsuario == usuario.Id)
            .Select(um => um.Menu.Codigo)
            .ToListAsync(cancellationToken);

        var menus = menusPorRol.Union(menusPorUsuario).Except(menusExcluidos).ToList();
        if (!menus.Contains("mesa-servicio")) menus.Add("mesa-servicio");

        var estructuras = await db.RolesTipoEstructura
            .Where(re => re.Activo && re.TipoEstructura.Activo && idsRolEfectivos.Contains(re.IdRol))
            .Select(re => re.CodigoTipoEstructura)
            .Distinct()
            .ToListAsync(cancellationToken);

        // Datasets del módulo "Reportería Gerencial" (motor semántico
        // portado de SIGA) — mismo patrón exacto que `estructuras`, ver
        // DatasetReporteria.cs. "nominal" (PII de socios) solo aparece acá
        // si el rol o el usuario lo tienen otorgado explícitamente, nunca
        // por defecto. Mismo criterio "suma, nunca resta" que los menús.
        var datasetsPorRol = await db.RolesDatasetReporteria
            .Where(rd => rd.Activo && rd.Dataset.Activo && idsRolEfectivos.Contains(rd.IdRol))
            .Select(rd => rd.CodigoDataset)
            .ToListAsync(cancellationToken);

        var datasetsPorUsuario = await db.UsuariosDatasetReporteria
            .Where(ud => ud.Activo && !ud.Excluido && ud.Dataset.Activo && ud.IdUsuario == usuario.Id)
            .Select(ud => ud.CodigoDataset)
            .ToListAsync(cancellationToken);

        var datasetsExcluidos = await db.UsuariosDatasetReporteria
            .Where(ud => ud.Excluido && ud.IdUsuario == usuario.Id)
            .Select(ud => ud.CodigoDataset)
            .ToListAsync(cancellationToken);

        var datasets = datasetsPorRol.Union(datasetsPorUsuario).Except(datasetsExcluidos).ToList();

        // Tercer nivel de permiso, genérico para cualquier módulo (ver
        // Opcion.cs) -- un reporte puntual, una acción puntual. Mismo
        // criterio de unión que menús/datasets, nunca resta.
        var opcionesPorRol = await db.RolesOpcion
            .Where(ro => ro.Activo && ro.Opcion.Activo && idsRolEfectivos.Contains(ro.IdRol))
            .Select(ro => ro.CodigoOpcion)
            .ToListAsync(cancellationToken);

        var opcionesPorUsuario = await db.UsuariosOpcion
            .Where(uo => uo.Activo && !uo.Excluido && uo.Opcion.Activo && uo.IdUsuario == usuario.Id)
            .Select(uo => uo.CodigoOpcion)
            .ToListAsync(cancellationToken);

        var opcionesExcluidas = await db.UsuariosOpcion
            .Where(uo => uo.Excluido && uo.IdUsuario == usuario.Id)
            .Select(uo => uo.CodigoOpcion)
            .ToListAsync(cancellationToken);

        var opciones = opcionesPorRol.Union(opcionesPorUsuario).Except(opcionesExcluidas).ToList();

        var idAgenciaEfectiva = await db.UsuariosAgenciaTemporal
            .Where(at => at.IdUsuario == usuario.Id && at.Activo && at.FechaCaducidad > ahora)
            .Select(at => (int?)at.IdAgenciaActual)
            .FirstOrDefaultAsync(cancellationToken) ?? usuario.IdAgencia;

        var (token, expiraEn, jti) = GenerarToken(usuario, roles, menus, estructuras, datasets, opciones, idAgenciaEfectiva);

        db.SesionesUsuario.Add(new SesionUsuario
        {
            Id = jti,
            IdUsuario = usuario.Id,
            EmitidaEn = DateTimeOffset.UtcNow,
            ExpiraEn = expiraEn,
            DireccionIp = direccionIp,
            Revocada = false,
        });
        await db.SaveChangesAsync(cancellationToken);

        return new LoginResult(
            token, expiraEn, usuario.Id, usuario.NombreUsuario, roles, menus, estructuras, datasets, opciones, idAgenciaEfectiva,
            usuario.CambiaClave);
    }

    // Ventana real de acceso por día de la semana (ver HorarioAccesoUsuario.cs)
    // — opt-in: si el usuario no tiene ninguna fila configurada, no aplica
    // ninguna restricción (mismo comportamiento real observado en Softbank,
    // donde solo una parte de los 255 usuarios tenía horario configurado).
    // Si tiene filas de ingreso, la hora actual debe caer dentro de alguna
    // Y no debe caer dentro de ninguna ventana de receso activa ese día.
    private async Task<bool> DentroDeHorarioAsync(Guid idUsuario, CancellationToken cancellationToken)
    {
        var horarios = await db.HorariosAccesoUsuario
            .Where(h => h.IdUsuario == idUsuario && h.Activo)
            .ToListAsync(cancellationToken);
        if (horarios.Count == 0)
        {
            return true;
        }

        // Ecuador: UTC-5 fijo, sin horario de verano — se convierte acá
        // porque las columnas HoraInicio/HoraFin se capturan en hora local
        // real del cajero/oficinista, no en UTC.
        var ahoraEcuador = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(-5));
        var diaSemana = ((int)ahoraEcuador.DayOfWeek + 6) % 7 + 1; // Lunes=1 ... Domingo=7
        var horaActual = TimeOnly.FromDateTime(ahoraEcuador.DateTime);

        var ventanasIngreso = horarios.Where(h => !h.EsReceso && h.DiaSemana == diaSemana).ToList();
        if (ventanasIngreso.Count == 0)
        {
            return false;
        }
        if (!ventanasIngreso.Any(h => horaActual >= h.HoraInicio && horaActual <= h.HoraFin))
        {
            return false;
        }

        var enReceso = horarios
            .Where(h => h.EsReceso && h.DiaSemana == diaSemana)
            .Any(h => horaActual >= h.HoraInicio && horaActual <= h.HoraFin);

        return !enReceso;
    }

    public async Task LogoutAsync(Guid idSesion, string registradoPor, CancellationToken cancellationToken = default)
    {
        var sesion = await db.SesionesUsuario.FirstOrDefaultAsync(s => s.Id == idSesion, cancellationToken);
        if (sesion is null || sesion.Revocada)
        {
            return;
        }

        sesion.Revocada = true;
        sesion.RevocadaEn = DateTimeOffset.UtcNow;
        sesion.RevocadaPor = registradoPor;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SesionUsuarioResult>> ListarSesionesAsync(
        Guid idUsuario, CancellationToken cancellationToken = default)
    {
        var ahora = DateTimeOffset.UtcNow;
        return await db.SesionesUsuario
            .Where(s => s.IdUsuario == idUsuario)
            .OrderByDescending(s => s.EmitidaEn)
            .Select(s => new SesionUsuarioResult(
                s.Id, s.EmitidaEn, s.ExpiraEn, s.DireccionIp, s.Revocada, s.RevocadaEn, s.RevocadaPor,
                !s.Revocada && s.ExpiraEn > ahora))
            .ToListAsync(cancellationToken);
    }

    public async Task RevocarTodasLasSesionesAsync(
        Guid idUsuario, string registradoPor, CancellationToken cancellationToken = default)
    {
        var ahora = DateTimeOffset.UtcNow;
        var sesionesActivas = await db.SesionesUsuario
            .Where(s => s.IdUsuario == idUsuario && !s.Revocada && s.ExpiraEn > ahora)
            .ToListAsync(cancellationToken);

        foreach (var sesion in sesionesActivas)
        {
            sesion.Revocada = true;
            sesion.RevocadaEn = ahora;
            sesion.RevocadaPor = registradoPor;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task CambiarClaveAsync(
        Guid idUsuario, Guid idSesionActual, CambiarClaveRequest request, CancellationToken cancellationToken = default)
    {
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == idUsuario, cancellationToken);
        if (usuario is null)
        {
            throw new UsuarioNoExisteException(idUsuario);
        }

        if (!BCrypt.Net.BCrypt.Verify(request.ContrasenaActual, usuario.HashContrasena))
        {
            throw new ContrasenaActualIncorrectaException();
        }

        if (request.ContrasenaNueva.Length < 8)
        {
            throw new ContrasenaNuevaInvalidaException("La contraseña nueva debe tener al menos 8 caracteres");
        }
        if (BCrypt.Net.BCrypt.Verify(request.ContrasenaNueva, usuario.HashContrasena))
        {
            throw new ContrasenaNuevaInvalidaException("La contraseña nueva debe ser distinta de la actual");
        }

        usuario.HashContrasena = BCrypt.Net.BCrypt.HashPassword(request.ContrasenaNueva);
        // Si el cambio estaba forzado (clave temporal de arranque, ej. la
        // importación real de usuarios de Softbank donde la clave inicial
        // es el propio nombre de usuario), ya se cumplió — se limpia acá,
        // nunca queda pidiendo el cambio de nuevo hasta que un admin lo
        // fuerce explícitamente otra vez.
        usuario.CambiaClave = false;
        usuario.ModificadoEn = DateTimeOffset.UtcNow;
        usuario.ModificadoPor = usuario.NombreUsuario;

        db.AccionesUsuario.Add(new AccionUsuario
        {
            Id = Guid.NewGuid(),
            CodigoAccion = "CambioClavePersonal",
            UsuarioRegistro = usuario.NombreUsuario,
            Fecha = DateTimeOffset.UtcNow,
            Descripcion = $"El usuario '{usuario.NombreUsuario}' cambió su propia contraseña.",
        });

        // Otras sesiones (no esta) quedan revocadas — si alguien más tiene
        // un token vigente con la clave anterior, deja de servir.
        var ahora = DateTimeOffset.UtcNow;
        var otrasSesionesActivas = await db.SesionesUsuario
            .Where(s => s.IdUsuario == idUsuario && s.Id != idSesionActual && !s.Revocada && s.ExpiraEn > ahora)
            .ToListAsync(cancellationToken);
        foreach (var sesion in otrasSesionesActivas)
        {
            sesion.Revocada = true;
            sesion.RevocadaEn = ahora;
            sesion.RevocadaPor = usuario.NombreUsuario;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task ConfigurarBloqueoAsync(
        Guid idUsuario, bool bloquear, string registradoPor, CancellationToken cancellationToken = default)
    {
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == idUsuario, cancellationToken);
        if (usuario is null)
        {
            throw new UsuarioNoExisteException(idUsuario);
        }

        usuario.TieneBloqueo = bloquear;
        usuario.ModificadoEn = DateTimeOffset.UtcNow;
        usuario.ModificadoPor = registradoPor;

        db.AccionesUsuario.Add(new AccionUsuario
        {
            Id = Guid.NewGuid(),
            CodigoAccion = "BloqueoAcceso",
            UsuarioRegistro = registradoPor,
            Fecha = DateTimeOffset.UtcNow,
            Descripcion = bloquear
                ? $"'{registradoPor}' bloqueó el acceso del usuario '{usuario.NombreUsuario}'."
                : $"'{registradoPor}' desbloqueó el acceso del usuario '{usuario.NombreUsuario}'.",
        });

        if (bloquear)
        {
            var ahora = DateTimeOffset.UtcNow;
            var sesionesActivas = await db.SesionesUsuario
                .Where(s => s.IdUsuario == idUsuario && !s.Revocada && s.ExpiraEn > ahora)
                .ToListAsync(cancellationToken);
            foreach (var sesion in sesionesActivas)
            {
                sesion.Revocada = true;
                sesion.RevocadaEn = ahora;
                sesion.RevocadaPor = registradoPor;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<Guid> CrearUsuarioAsync(
        CrearUsuarioRequest request, string registradoPor, CancellationToken cancellationToken = default)
    {
        if (await db.Usuarios.AnyAsync(u => u.NombreUsuario == request.NombreUsuario, cancellationToken))
        {
            throw new NombreUsuarioDuplicadoException(request.NombreUsuario);
        }

        if (!await db.Agencias.AnyAsync(a => a.Id == request.IdAgencia && a.Activa, cancellationToken))
        {
            throw new AgenciaInvalidaException(request.IdAgencia);
        }

        if (request.ContrasenaInicial.Length < 8)
        {
            throw new ContrasenaNuevaInvalidaException("La contraseña inicial debe tener al menos 8 caracteres");
        }

        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            NombreUsuario = request.NombreUsuario,
            HashContrasena = BCrypt.Net.BCrypt.HashPassword(request.ContrasenaInicial),
            IdAgencia = request.IdAgencia,
            IdPersona = request.IdPersona,
            PuedeIngresarSistema = true,
            Activo = true,
            UsaDispositivoMovil = request.UsaDispositivoMovil,
            PermiteRiesgoOperativo = request.PermiteRiesgoOperativo,
            PermiteConsultaEmpleados = request.PermiteConsultaEmpleados,
            ValidaIp = request.ValidaIp,
            CambiaClave = request.CambiaClave,
            DiasCambioClave = request.DiasCambioClave,
            CreadoEn = DateTimeOffset.UtcNow,
            CreadoPor = registradoPor,
        };
        db.Usuarios.Add(usuario);

        db.AccionesUsuario.Add(new AccionUsuario
        {
            Id = Guid.NewGuid(),
            CodigoAccion = "CreacionSujeto",
            UsuarioRegistro = registradoPor,
            Fecha = DateTimeOffset.UtcNow,
            Descripcion = $"'{registradoPor}' creó el usuario '{usuario.NombreUsuario}'.",
        });

        await db.SaveChangesAsync(cancellationToken);
        return usuario.Id;
    }

    public async Task ActualizarUsuarioAsync(
        Guid idUsuario, ActualizarUsuarioRequest request, string registradoPor, CancellationToken cancellationToken = default)
    {
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == idUsuario, cancellationToken);
        if (usuario is null)
        {
            throw new UsuarioNoExisteException(idUsuario);
        }

        if (!await db.Agencias.AnyAsync(a => a.Id == request.IdAgencia && a.Activa, cancellationToken))
        {
            throw new AgenciaInvalidaException(request.IdAgencia);
        }

        usuario.IdAgencia = request.IdAgencia;
        usuario.IdPersona = request.IdPersona;
        usuario.PuedeIngresarSistema = request.PuedeIngresarSistema;
        usuario.UsaDispositivoMovil = request.UsaDispositivoMovil;
        usuario.PermiteRiesgoOperativo = request.PermiteRiesgoOperativo;
        usuario.PermiteConsultaEmpleados = request.PermiteConsultaEmpleados;
        usuario.ValidaIp = request.ValidaIp;
        usuario.CambiaClave = request.CambiaClave;
        usuario.DiasCambioClave = request.DiasCambioClave;
        usuario.ModificadoEn = DateTimeOffset.UtcNow;
        usuario.ModificadoPor = registradoPor;

        db.AccionesUsuario.Add(new AccionUsuario
        {
            Id = Guid.NewGuid(),
            CodigoAccion = "CambioSujeto",
            UsuarioRegistro = registradoPor,
            Fecha = DateTimeOffset.UtcNow,
            Descripcion = $"'{registradoPor}' editó el usuario '{usuario.NombreUsuario}'.",
        });

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task ResetearClaveAsync(
        Guid idUsuario, string claveNueva, string registradoPor, CancellationToken cancellationToken = default)
    {
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == idUsuario, cancellationToken);
        if (usuario is null)
        {
            throw new UsuarioNoExisteException(idUsuario);
        }

        if (claveNueva.Length < 8)
        {
            throw new ContrasenaNuevaInvalidaException("La contraseña nueva debe tener al menos 8 caracteres");
        }

        usuario.HashContrasena = BCrypt.Net.BCrypt.HashPassword(claveNueva);
        usuario.ModificadoEn = DateTimeOffset.UtcNow;
        usuario.ModificadoPor = registradoPor;

        db.AccionesUsuario.Add(new AccionUsuario
        {
            Id = Guid.NewGuid(),
            CodigoAccion = "CambioClaveEnLote",
            UsuarioRegistro = registradoPor,
            Fecha = DateTimeOffset.UtcNow,
            Descripcion = $"'{registradoPor}' reseteó la contraseña del usuario '{usuario.NombreUsuario}'.",
        });

        var ahora = DateTimeOffset.UtcNow;
        var sesionesActivas = await db.SesionesUsuario
            .Where(s => s.IdUsuario == idUsuario && !s.Revocada && s.ExpiraEn > ahora)
            .ToListAsync(cancellationToken);
        foreach (var sesion in sesionesActivas)
        {
            sesion.Revocada = true;
            sesion.RevocadaEn = ahora;
            sesion.RevocadaPor = registradoPor;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task RegistrarIntentoAsync(
        Guid? idUsuario, bool exitoso, string? direccionIp, string? detalle, CancellationToken cancellationToken)
    {
        if (idUsuario is null) return;

        db.AccionesIngresoUsuario.Add(new AccionIngresoUsuario
        {
            IdUsuario = idUsuario.Value,
            FechaHora = DateTimeOffset.UtcNow,
            Exitoso = exitoso,
            DireccionIp = direccionIp,
            Detalle = detalle,
        });
        await db.SaveChangesAsync(cancellationToken);
    }

    private (string Token, DateTimeOffset ExpiraEn, Guid Jti) GenerarToken(
        Usuario usuario, List<string> roles, List<string> menus, List<string> estructuras, List<string> datasets, List<string> opciones, int idAgenciaEfectiva)
    {
        var secret = configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("Falta Jwt:Secret (revisar .env.core)");
        var issuer = configuration["Jwt:Issuer"] ?? "Corela15";
        var audience = configuration["Jwt:Audience"] ?? "Corela15Api";
        var expiryMinutes = int.TryParse(configuration["Jwt:ExpiryMinutes"], out var m) ? m : 480;

        var expiraEn = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes);
        var jti = Guid.NewGuid();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.NombreUsuario),
            new(JwtRegisteredClaimNames.Jti, jti.ToString()),
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        claims.AddRange(menus.Select(m2 => new Claim("menu", m2)));
        claims.AddRange(estructuras.Select(e => new Claim("estructura", e)));
        claims.AddRange(datasets.Select(d => new Claim("dataset", d)));
        claims.AddRange(opciones.Select(o => new Claim("opcion", o)));
        claims.Add(new Claim("agencia", idAgenciaEfectiva.ToString()));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiraEn.UtcDateTime,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiraEn, jti);
    }
}
