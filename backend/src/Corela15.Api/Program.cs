using Corela15.Infrastructure.Persistence;
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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var corsOrigins = builder.Configuration.GetSection("Corela15:CorsOrigins").Get<string[]>()
    ?? ["http://localhost:5173"];
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(corsOrigins).AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "ok", timestampUtc = DateTimeOffset.UtcNow }));

app.Run();
