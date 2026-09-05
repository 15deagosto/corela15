using Corela15.Application.Obligacion;
using Corela15.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Corela15.Infrastructure.Services;

/// <summary>
/// Ver <see cref="IObligacionSyncService"/> para el alcance real y la
/// justificación de por qué este es el único servicio del backend que
/// se conecta a Softbank. Mapeo idéntico al ya verificado y probado en
/// la herramienta externa `sync-of01` -- reusado tal cual, no reinventado.
/// </summary>
public class ObligacionSyncService(Corela15DbContext db) : IObligacionSyncService
{
    private static readonly Dictionary<string, string> MapaEstado = new()
    {
        ["I"] = "NV", ["P"] = "NV", ["A"] = "VG", ["R"] = "VG", ["V"] = "VN", ["C"] = "CN",
    };

    // Verificado contra OBLIGACION.INSTITUCION_OBLIGACION real (5 filas
    // reales) -- FINANCOOP/Fondo de Liquidez BCE = sector financiero
    // popular y solidario; CONAFIPS/BanEcuador = entidades públicas;
    // Banco Desarrollo de los Pueblos = privada. Institución nueva no
    // listada acá se omite con aviso explícito, nunca se le adivina grupo.
    private static readonly Dictionary<string, string> GrupoPorRuc = new()
    {
        ["1791708040001"] = "2604",
        ["1768168480001"] = "2606",
        ["0990247536001"] = "2602",
        ["1792707544001"] = "2610",
        ["1768183520001"] = "2606",
    };

    private static string SufijoPorDiasRestantes(int dias) => dias switch
    {
        <= 30 => "05",
        <= 90 => "10",
        <= 180 => "15",
        <= 360 => "20",
        _ => "25",
    };

    public async Task<SincronizacionObligacionesResult> SincronizarAsync(CancellationToken cancellationToken = default)
    {
        var connStringSoftbank = Environment.GetEnvironmentVariable("Softbank__ConnectionStringRo")
            ?? throw new InvalidOperationException(
                "Falta Softbank__ConnectionStringRo -- sin esta variable, el módulo de " +
                "Estructuras Financieras no puede sincronizar con Softbank (a propósito, " +
                "es la única conexión de todo el backend a esa base).");

        var detalle = new List<string>();
        var sincronizadas = 0;
        var omitidas = 0;

        await using var conexionSoftbank = new SqlConnection(connStringSoftbank);
        await conexionSoftbank.OpenAsync(cancellationToken);

        const string sql = """
            USE Softbank;
            SELECT
                o.ID, o.IDAGENCIA, o.CODIGOESTADO, o.SALDOACTUAL,
                o.DEUDAINICIAL_MONEDA_NACIONAL, o.VALORENTREGADO,
                o.TASA, o.FECHAADJUDICACION, o.FECHAVENCIMIENTO,
                o.CODIGOUSUARIO, o.NUMEROPAGARE,
                i.IDENTIFICACION AS RucAcreedor, i.IDPAIS
            FROM OBLIGACION.OBLIGACION_FINANCIERA o
            LEFT JOIN OBLIGACION.INSTITUCION_OBLIGACION i ON i.ID = o.IDINSTITUCION
            """;

        var filas = new List<Dictionary<string, object?>>();
        await using (var cmd = new SqlCommand(sql, conexionSoftbank))
        await using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                var fila = new Dictionary<string, object?>();
                for (var i = 0; i < reader.FieldCount; i++)
                    fila[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                filas.Add(fila);
            }
        }

        foreach (var f in filas)
        {
            var codigoEstadoSoftbank = (string?)f["CODIGOESTADO"];
            var ruc = ((string?)f["RucAcreedor"])?.Trim();
            var idAgencia = (int?)f["IDAGENCIA"];
            var idSoftbank = f["ID"];

            if (codigoEstadoSoftbank is null || codigoEstadoSoftbank == "X")
            {
                detalle.Add($"Obligación #{idSoftbank}: omitida (anulada o sin estado real)");
                omitidas++;
                continue;
            }
            if (!MapaEstado.TryGetValue(codigoEstadoSoftbank, out var codigoEstadoOf01))
            {
                detalle.Add($"Obligación #{idSoftbank}: estado '{codigoEstadoSoftbank}' sin mapeo real conocido, omitida");
                omitidas++;
                continue;
            }
            if (ruc is null || !GrupoPorRuc.TryGetValue(ruc, out var grupoCuc))
            {
                detalle.Add($"Obligación #{idSoftbank}: institución RUC '{ruc}' sin mapear a cuenta contable real, omitida");
                omitidas++;
                continue;
            }
            if (idAgencia is null)
            {
                detalle.Add($"Obligación #{idSoftbank}: sin agencia, omitida");
                omitidas++;
                continue;
            }

            var fechaVencimiento = (DateTime?)f["FECHAVENCIMIENTO"] ?? DateTime.Today;
            var diasRestantes = Math.Max(0, (fechaVencimiento.Date - DateTime.Today).Days);
            var codigoSubcuenta = grupoCuc + SufijoPorDiasRestantes(diasRestantes);

            var cuenta = await db.CuentasContables
                .FirstOrDefaultAsync(c => c.Codigo == codigoSubcuenta && c.EsMayor && c.Activa, cancellationToken);
            if (cuenta is null)
            {
                detalle.Add($"Obligación #{idSoftbank}: subcuenta calculada '{codigoSubcuenta}' no existe/inactiva en el CUC, omitida");
                omitidas++;
                continue;
            }

            var numeroObligacion = f["NUMEROPAGARE"] as string ?? $"SBK-{idSoftbank}";
            var deudaInicial = Convert.ToDecimal(f["DEUDAINICIAL_MONEDA_NACIONAL"] ?? 0m);
            var entregado = Convert.ToDecimal(f["VALORENTREGADO"] ?? 0m);
            var saldo = Convert.ToDecimal(f["SALDOACTUAL"] ?? 0m);
            var tasa = Convert.ToDecimal(f["TASA"] ?? 0m);
            var fechaConcesion = (DateTime?)f["FECHAADJUDICACION"] ?? DateTime.Today;
            var codigoUsuario = f["CODIGOUSUARIO"] as string ?? "sync:softbank";
            var idPais = (int?)f["IDPAIS"];
            var codigoPaisAcreedor = idPais == 52 ? "ECU" : "EXT";

            var existente = await db.ObligacionesFinancieras.FirstOrDefaultAsync(o =>
                o.TipoIdentificacionAcreedor == "R" &&
                o.IdentificacionAcreedor == ruc &&
                o.NumeroObligacion == numeroObligacion &&
                o.IdCuentaContable == cuenta.Id, cancellationToken);

            if (existente is not null)
            {
                existente.Saldo = saldo;
                existente.CodigoEstado = codigoEstadoOf01;
                existente.MontoPorUtilizar = Math.Max(0, deudaInicial - entregado);
                existente.TasaInteres = tasa;
                existente.ModificadoEn = DateTimeOffset.UtcNow;
                existente.ModificadoPor = $"sync:softbank:{codigoUsuario}";
                detalle.Add($"Obligación #{idSoftbank} ({numeroObligacion}): actualizada -> cuenta {codigoSubcuenta}, saldo {saldo:C}");
            }
            else
            {
                db.ObligacionesFinancieras.Add(new Domain.Obligacion.ObligacionFinanciera
                {
                    Id = Guid.NewGuid(),
                    IdAgencia = idAgencia.Value,
                    TipoIdentificacionAcreedor = "R",
                    IdentificacionAcreedor = ruc,
                    CodigoPaisAcreedor = codigoPaisAcreedor,
                    NumeroObligacion = numeroObligacion,
                    DestinoLineaCredito = "Sincronizado desde Softbank",
                    MontoLineaCredito = deudaInicial,
                    MontoPorUtilizar = Math.Max(0, deudaInicial - entregado),
                    CodigoEstado = codigoEstadoOf01,
                    Saldo = saldo,
                    IdCuentaContable = cuenta.Id,
                    TasaInteres = tasa,
                    InteresesPorPagar = 0,
                    PagaComision = false,
                    FechaConcesion = DateOnly.FromDateTime(fechaConcesion),
                    FechaVencimiento = DateOnly.FromDateTime(fechaVencimiento),
                    CodigoPeriodicidadPago = "NA",
                    TienePeriodoGracia = false,
                    CodigoClase = "N",
                    CreadoEn = DateTimeOffset.UtcNow,
                    CreadoPor = $"sync:softbank:{codigoUsuario}",
                });
                detalle.Add($"Obligación #{idSoftbank} ({numeroObligacion}): sincronizada -> cuenta {codigoSubcuenta}, saldo {saldo:C}");
            }
            sincronizadas++;
        }

        await db.SaveChangesAsync(cancellationToken);

        return new SincronizacionObligacionesResult(filas.Count, sincronizadas, omitidas, detalle);
    }
}
