using Corela15.Api.ExceptionHandling;
using Corela15.Api.Idempotencia;
using Corela15.Application.ActivoFijo;
using Corela15.Application.Ahorros;
using Corela15.Application.Cajas;
using Corela15.Application.Cobranza;
using Corela15.Application.Financiero;
using Corela15.Application.Cumplimiento;
using Corela15.Application.LavadoActivos;
using Corela15.Application.Colocacion;
using Corela15.Application.Sujeto;
using Corela15.Application.Contabilidad;
using Corela15.Application.CuentasPorCobrar;
using Corela15.Application.Inversion;
using Corela15.Application.MesaServicio;
using Corela15.Application.Nomina;
using Corela15.Application.Obligacion;
using Corela15.Application.Portafolio;
using Corela15.Application.Proveeduria;
using Corela15.Application.Riesgo;
using Corela15.Application.Seguridad;
using Corela15.Infrastructure.Persistence;
using Corela15.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

// Carga .env.core (Postgres del core nuevo) — separado de .env/.env.nominal
// que son de solo lectura de Softbank/SIGA y nunca deben mezclarse acá.
var envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", ".env.core");
if (File.Exists(envPath))
{
    DotNetEnv.Env.Load(envPath);
}

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
{
    config
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console();
});

var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Core")
    ?? builder.Configuration.GetConnectionString("Core")
    ?? throw new InvalidOperationException("Falta ConnectionStrings__Core (revisar .env.core)");

builder.Services.AddDbContext<Corela15DbContext>(options =>
    options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());

builder.Services.AddScoped<IComprobanteContableService, ComprobanteContableService>();
builder.Services.AddScoped<ICuentaContableAdminService, CuentaContableAdminService>();
builder.Services.AddScoped<ICierrePeriodoService, CierrePeriodoService>();
builder.Services.AddScoped<ICierreEjercicioService, CierreEjercicioService>();
builder.Services.AddScoped<ICuentaAhorroService, CuentaAhorroService>();
builder.Services.AddScoped<IAutorizacionTransaccionService>(sp => (CuentaAhorroService)sp.GetRequiredService<ICuentaAhorroService>());
builder.Services.AddScoped<IDevengoInteresService, DevengoInteresService>();
builder.Services.AddScoped<IPrestamoService, PrestamoService>();
builder.Services.AddScoped<Corela15.Application.FlujoTrabajo.IFlujoTrabajoService, FlujoTrabajoService>();
builder.Services.AddScoped<IGarantiaService, GarantiaService>();
builder.Services.AddScoped<ITipoPrestamoAdminService, TipoPrestamoAdminService>();
builder.Services.AddScoped<IScoreCrediticioService, ScoreCrediticioService>();
builder.Services.AddScoped<IAutoDebitoSpiService, AutoDebitoSpiService>();
builder.Services.AddScoped<IMoraCarteraService, MoraCarteraService>();
builder.Services.AddScoped<Corela15.Application.Cobranza.IGastoCobranzaService, GastoCobranzaService>();
builder.Services.AddScoped<IProvisionCarteraService, ProvisionCarteraService>();
builder.Services.AddScoped<IDepositoService, DepositoService>();
builder.Services.AddScoped<IGestionCobranzaService, GestionCobranzaService>();
builder.Services.AddScoped<IVentanillaService, VentanillaService>();
builder.Services.AddScoped<IPagoExternoService, PagoExternoService>();
builder.Services.AddScoped<IRolPagosService, RolPagosService>();
builder.Services.AddScoped<IBeneficioSocialService, BeneficioSocialService>();
builder.Services.AddScoped<IEmpleadoService, EmpleadoService>();
builder.Services.AddScoped<ISolicitudAccionPersonalService, SolicitudAccionPersonalService>();
builder.Services.AddScoped<ICalculoImpuestoRentaService, CalculoImpuestoRentaService>();
builder.Services.AddScoped<ICuentaPorCobrarService, CuentaPorCobrarService>();
builder.Services.AddScoped<ICuentaPorPagarService, CuentaPorPagarService>();
builder.Services.AddScoped<IComprasService, ComprasService>();
builder.Services.AddScoped<IActivoService, ActivoService>();
builder.Services.AddScoped<IInversionPortafolioService, InversionPortafolioService>();
builder.Services.AddScoped<IChequeService, ChequeService>();
builder.Services.AddScoped<ISolicitudPedidoService, SolicitudPedidoService>();
builder.Services.AddScoped<IObligacionFinancieraService, ObligacionFinancieraService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<IEventoRiesgoService, EventoRiesgoService>();
builder.Services.AddScoped<IAvanceRiesgoService, AvanceRiesgoService>();
builder.Services.AddScoped<IHallazgoService, HallazgoService>();
builder.Services.AddScoped<IIndicadorLiquidezService, IndicadorLiquidezService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IListaControlService, ListaControlService>();
builder.Services.AddScoped<IPerfilLavadoActivosService, PerfilLavadoActivosService>();
builder.Services.AddScoped<IDatosPersonaService, DatosPersonaService>();
builder.Services.AddScoped<IReclamoService, ReclamoService>();
builder.Services.AddScoped<ISocioService, SocioService>();
builder.Services.AddHostedService<AutoDebitoSpiBackgroundService>();
builder.Services.AddScoped<IdempotenciaFilter>();

builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddProblemDetails();

// Autenticación JWT — el secreto y demás config viven en .env.core
// (Jwt__Secret/Issuer/Audience/ExpiryMinutes), nunca hardcodeados.
var jwtSecret = Environment.GetEnvironmentVariable("Jwt__Secret")
    ?? builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Falta Jwt__Secret (revisar .env.core)");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "Corela15";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "Corela15Api";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.FromMinutes(1),
        };

        // Un JWT válido por firma/expiración igual puede haber sido
        // revocado (logout real o "cerrar todas las sesiones" desde
        // administración) — sin este chequeo extra contra
        // seguridad.sesion_usuario, la revocación no tendría ningún efecto
        // real hasta que el token expirara solo a las 8h.
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var jtiClaim = context.Principal?.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti)?.Value;
                if (jtiClaim is null || !Guid.TryParse(jtiClaim, out var jti))
                {
                    context.Fail("Token sin jti.");
                    return;
                }

                var db = context.HttpContext.RequestServices.GetRequiredService<Corela15DbContext>();
                var revocada = await db.SesionesUsuario
                    .Where(s => s.Id == jti)
                    .Select(s => (bool?)s.Revocada)
                    .FirstOrDefaultAsync();

                if (revocada != false)
                {
                    context.Fail("Sesión revocada o inexistente.");
                }
            },
        };
    });

// Un menú = un módulo (frontend/src/modules.ts). Cada controller de negocio
// exige la policy del menú al que pertenece; sin sesión o sin ese menú
// asignado al rol (seguridad.rol_menu), 401/403. FallbackPolicy exige
// sesión válida por defecto — un endpoint nuevo queda protegido solo con
// heredar, no hace falta acordarse de agregarlo cada vez.
var codigosMenu = new[]
{
    "socios", "usuarios-roles", "contabilidad", "ahorros", "creditos",
    "cobranzas-cumplimiento", "cajas", "nomina", "tesoreria", "riesgo", "configuracion", "activofijo", "portafolio",
    "financiero", "proveeduria", "estructuras-financieras", "mesa-servicio", "mesa-servicio-agente",
};

// Segundo nivel de permiso, más fino que el menú (ver TipoEstructura.cs) —
// dentro del módulo "Estructuras y Procesos Financieros", qué estructuras
// puntuales puede generar cada usuario. "OF01" primero, con espacio real
// para sumar más sin tocar este arreglo si se agregan por catálogo — acá
// solo se declaran las policies base ya conocidas al arrancar.
var codigosTipoEstructura = new[] { "OF01" };

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    foreach (var codigo in codigosMenu)
    {
        options.AddPolicy($"Menu:{codigo}", policy => policy.RequireClaim("menu", codigo));
    }

    foreach (var codigo in codigosTipoEstructura)
    {
        options.AddPolicy($"Estructura:{codigo}", policy => policy.RequireClaim("estructura", codigo));
    }
});

builder.Services.AddControllers(options => options.Filters.Add<IdempotenciaFilter>());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// En desarrollo aceptamos cualquier puerto de localhost/127.0.0.1: Vite salta
// de puerto según qué otras apps (propias, de la cooperativa) estén corriendo
// en la misma máquina — fijar un solo puerto acá era frágil y rompía cada vez
// que cambiaba. En producción sí se restringe a los orígenes configurados
// explícitamente (Corela15:CorsOrigins).
//
// También se acepta cualquier IP de red privada (LAN) — 192.168.x.x,
// 10.x.x.x, 172.16.x.x-172.31.x.x — para que otros equipos de la misma red
// puedan usar la app apuntando a la IP de esta máquina (ej. compartir con
// un compañero de oficina sin desplegar nada). Sigue siendo solo desarrollo:
// nunca acepta un origen público/externo, solo direcciones de red privada
// reales (RFC 1918).
static bool EsRedPrivada(string host)
{
    if (host is "localhost" or "127.0.0.1") return true;
    if (!System.Net.IPAddress.TryParse(host, out var ip)) return false;
    var b = ip.GetAddressBytes();
    if (b.Length != 4) return false;
    return b[0] == 192 && b[1] == 168
        || b[0] == 10
        || b[0] == 172 && b[1] is >= 16 and <= 31;
}

var corsOriginsConfigurados = builder.Configuration.GetSection("Corela15:CorsOrigins").Get<string[]>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.SetIsOriginAllowed(origin =>
                Uri.TryCreate(origin, UriKind.Absolute, out var uri) && EsRedPrivada(uri.Host));
        }
        else
        {
            policy.WithOrigins(corsOriginsConfigurados ?? []);
        }

        policy.AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "ok", timestampUtc = DateTimeOffset.UtcNow })).AllowAnonymous();

app.Run();
