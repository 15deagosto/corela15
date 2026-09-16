using System.Globalization;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using Corela15.Application.Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Corela15.Api.Controllers;

/// <summary>
/// Estructura B11 (Balance de Comprobación, SEPS) real — segunda
/// estructura del módulo, calculada en vivo desde el ledger real de
/// Softbank (ver <see cref="IB11Service"/>). Formato de envío real (XML +
/// hash + zip) confirmado byte a byte contra el archivo de referencia
/// oficial (SEPS, corte 31/08/2026): root <c>balance</c>, namespace
/// <c>http://www.seps.gob.ec/balances</c>, hash = MD5 plano del XML en
/// mayúsculas, nombre del zip <c>B11_RUC_dd-MM-yyyy.zip</c> con
/// <c>&lt;mismo-nombre&gt;.xml</c> + <c>&lt;mismo-nombre&gt;-hash.txt</c>
/// — a diferencia de OF01 (sin XSD real confirmado), este SÍ es un
/// formato verificado, no inferido.
/// </summary>
[ApiController]
[Route("api/estructuras-financieras/b11")]
[Authorize(Policy = "Menu:estructuras-financieras")]
[Authorize(Policy = "Estructura:B11")]
public class B11Controller(IB11Service service) : ControllerBase
{
    [HttpGet("generar")]
    public async Task<ActionResult<B11Result>> Generar([FromQuery] DateOnly fechaCorte, CancellationToken cancellationToken)
        => Ok(await service.GenerarAsync(fechaCorte, cancellationToken));

    [HttpGet("paquete")]
    public async Task<IActionResult> GenerarPaquete([FromQuery] DateOnly fechaCorte, CancellationToken cancellationToken)
    {
        var resultado = await service.GenerarAsync(fechaCorte, cancellationToken);
        if (string.IsNullOrWhiteSpace(resultado.Ruc))
            return BadRequest(new { detail = "La empresa no tiene RUC configurado (Configuración > Empresa) — no se puede armar el paquete sin ese dato obligatorio de la cabecera." });

        var xml = ArmarXml(resultado);
        var xmlBytes = Encoding.UTF8.GetBytes(xml);
        var hashHex = Convert.ToHexString(MD5.HashData(xmlBytes)); // mayúsculas, verificado byte a byte contra el hash.txt real

        var baseNombre = $"B11_{resultado.Ruc}_{fechaCorte:dd-MM-yyyy}";

        using var memoria = new MemoryStream();
        using (var zip = new ZipArchive(memoria, ZipArchiveMode.Create, leaveOpen: true))
        {
            var entradaXml = zip.CreateEntry($"{baseNombre}.xml", CompressionLevel.Optimal);
            await using (var s = entradaXml.Open()) await s.WriteAsync(xmlBytes, cancellationToken);

            var entradaHash = zip.CreateEntry($"{baseNombre}-hash.txt", CompressionLevel.Optimal);
            await using (var s = entradaHash.Open()) await s.WriteAsync(Encoding.UTF8.GetBytes(hashHex), cancellationToken);
        }

        return File(memoria.ToArray(), "application/zip", $"{baseNombre}.zip");
    }

    /// <summary>
    /// Reconstruye la jerarquía real del archivo oficial —
    /// <c>elemento(1 dígito)</c> &gt; <c>grupo(2)</c> &gt; <c>cuenta(4)</c> &gt;
    /// <c>subcuenta(6)</c> — verificada directamente contra el XML real de
    /// referencia (nunca una lista plana de <c>&lt;cuenta&gt;</c>, como
    /// tenía la primera versión de este generador). El padre de cada
    /// código se deriva de su propia longitud (mismo criterio ya usado en
    /// toda la siembra del CUC), no hace falta IdCuentaPadre acá porque
    /// <see cref="IB11Service"/> ya entrega el detalle plano ordenado por
    /// código con el saldo ya calculado bottom-up para cada nivel.
    /// </summary>
    private static string ArmarXml(B11Result r)
    {
        static string Num(decimal v) => v.ToString("0.00", CultureInfo.InvariantCulture);
        static string TagPorNivel(int longitud) => longitud switch
        {
            1 => "elemento",
            2 => "grupo",
            4 => "cuenta",
            6 => "subcuenta",
            _ => "cuenta", // códigos anómalos reales (ej. "671") ya excluidos aguas arriba
        };

        XNamespace ns = "http://www.seps.gob.ec/balances";
        var raiz = new XElement(ns + "balance",
            new XAttribute(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance"),
            new XAttribute(XNamespace.Xmlns + "xsd", "http://www.w3.org/2001/XMLSchema"),
            new XAttribute("estructura", r.CodigoEstructura),
            new XAttribute("rucEntidad", r.Ruc),
            new XAttribute("fechaCorte", r.FechaCorte.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)),
            new XAttribute("numRegistro", r.NumeroRegistros),
            new XAttribute("valorCuadre", Num(r.ValorCuadre)));

        // Nombres reales del archivo oficial van en MAYÚSCULAS (verificado
        // byte a byte) — el catálogo propio de Corela15 los guarda con
        // mayúscula inicial normal, se transforma solo acá, al armar el
        // XML de envío, nunca se toca el catálogo.
        var elementos = new Dictionary<string, XElement>();
        foreach (var d in r.Detalle.OrderBy(x => x.Codigo))
        {
            var nodo = new XElement(ns + TagPorNivel(d.Codigo.Length),
                new XAttribute("codigo", d.Codigo),
                new XAttribute("nombre", d.Nombre.ToUpperInvariant()),
                new XAttribute("total", Num(d.Total)));
            elementos[d.Codigo] = nodo;

            var codigoPadre = d.Codigo.Length switch
            {
                2 => d.Codigo[..1],
                4 => d.Codigo[..2],
                6 => d.Codigo[..4],
                _ => null,
            };

            if (codigoPadre is not null && elementos.TryGetValue(codigoPadre, out var padre))
                padre.Add(nodo);
            else
                raiz.Add(nodo);
        }

        var doc = new XDocument(new XDeclaration("1.0", "utf-8", null), raiz);
        return doc.Declaration + "\n" + doc.Root;
    }
}
