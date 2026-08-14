using Corela15.Api.ExceptionHandling;
using Corela15.Api.Idempotencia;
using Corela15.Application.Ahorros;
using Corela15.Application.Cajas;
using Corela15.Application.Cobranza;
using Corela15.Application.Colocacion;
using Corela15.Application.Contabilidad;
using Corela15.Application.CuentasPorCobrar;
using Corela15.Application.Inversion;
using Corela15.Application.Nomina;
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
builder.Services.AddScoped<ICuentaAhorroService, CuentaAhorroService>();
builder.Services.AddScoped<IPrestamoService, PrestamoService>();
builder.Services.AddScoped<IDepositoService, DepositoService>();
builder.Services.AddScoped<IGestionCobranzaService, GestionCobranzaService>();
builder.Services.AddScoped<IVentanillaService, VentanillaService>();
builder.Services.AddScoped<IRolPagosService, RolPagosService>();
builder.Services.AddScoped<ICuentaPorCobrarService, CuentaPorCobrarService>();
builder.Services.AddScoped<IEventoRiesgoService, EventoRiesgoService>();
builder.Services.AddScoped<IAuthService, AuthService>();
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
    });

// Un menú = un módulo (frontend/src/modules.ts). Cada controller de negocio
// exige la policy del menú al que pertenece; sin sesión o sin ese menú
// asignado al rol (seguridad.rol_menu), 401/403. FallbackPolicy exige
// sesión válida por defecto — un endpoint nuevo queda protegido solo con
// heredar, no hace falta acordarse de agregarlo cada vez.
var codigosMenu = new[]
{
    "socios", "usuarios-roles", "contabilidad", "ahorros", "creditos",
    "cobranzas-cumplimiento", "cajas", "nomina", "tesoreria", "riesgo", "configuracion",
};
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    foreach (var codigo in codigosMenu)
    {
        options.AddPolicy($"Menu:{codigo}", policy => policy.RequireClaim("menu", codigo));
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
var corsOriginsConfigurados = builder.Configuration.GetSection("Corela15:CorsOrigins").Get<string[]>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.SetIsOriginAllowed(origin =>
                Uri.TryCreate(origin, UriKind.Absolute, out var uri) &&
                (uri.Host is "localhost" or "127.0.0.1"));
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
