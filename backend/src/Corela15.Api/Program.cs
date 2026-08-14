using Corela15.Api.ExceptionHandling;
using Corela15.Application.Ahorros;
using Corela15.Application.Cajas;
using Corela15.Application.Cobranza;
using Corela15.Application.Colocacion;
using Corela15.Application.Contabilidad;
using Corela15.Application.CuentasPorCobrar;
using Corela15.Application.Inversion;
using Corela15.Application.Nomina;
using Corela15.Infrastructure.Persistence;
using Corela15.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

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

builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers();
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
app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "ok", timestampUtc = DateTimeOffset.UtcNow }));

app.Run();
