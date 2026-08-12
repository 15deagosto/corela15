using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Corela15.Infrastructure.Persistence;

/// <summary>
/// Solo para `dotnet ef migrations add/update` en tiempo de diseño.
/// Lee la cadena de conexión de la variable de entorno CORELA15_CONNECTION
/// (ver backend/README.md) — nunca hardcodear credenciales acá.
/// </summary>
public class Corela15DbContextFactory : IDesignTimeDbContextFactory<Corela15DbContext>
{
    public Corela15DbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("CORELA15_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=corela15_core;Username=corela15_admin;Password=changeme";

        var optionsBuilder = new DbContextOptionsBuilder<Corela15DbContext>();
        optionsBuilder.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();

        return new Corela15DbContext(optionsBuilder.Options);
    }
}
