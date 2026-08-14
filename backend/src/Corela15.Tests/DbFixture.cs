using Corela15.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Tests;

/// <summary>
/// Corre contra la misma base de desarrollo real (Postgres local, ver
/// CORELA15_CONNECTION) que se usó para probar cada caso de uso a mano
/// durante toda la construcción del core — no un proveedor en memoria, que
/// no ejecutaría los triggers de versionado ni el token de concurrencia
/// real (xmin). Cada test crea su propio DbContext y es responsable de
/// limpiar los datos que crea (mismo patrón manual que se usó en curl
/// durante toda la sesión, ahora automatizado).
/// </summary>
public static class DbFixture
{
    public static Corela15DbContext CrearContexto()
    {
        var connectionString = Environment.GetEnvironmentVariable("CORELA15_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=corela15_core;Username=corela15_admin;Password=burT7pgre3Ri5SG6jqZ3DBFLK9IDaro";

        var options = new DbContextOptionsBuilder<Corela15DbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new Corela15DbContext(options);
    }
}
