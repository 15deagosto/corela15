using System.IO.Compression;
using System.Xml;

namespace Corela15.Infrastructure.Xlsx;

/// <summary>
/// Lector mínimo de .xlsx (un .xlsx es un .zip con XML adentro) — sin
/// ninguna dependencia nueva (System.IO.Compression/System.Xml ya vienen
/// con .NET), mismo método ya usado para leer el .docx real de las actas
/// en otros proyectos de esta sesión. Alcance deliberadamente angosto:
/// solo lee celdas de una hoja por nombre visible, resolviendo shared
/// strings — no escribe, no soporta fórmulas ni formatos de fecha
/// especiales (eso se resuelve en el servicio que llama, con el dato
/// crudo que entrega esta clase).
/// </summary>
public static class XlsxReader
{
    /// <summary>
    /// Cada fila como diccionario columna(índice 0-based, A=0,B=1...) -> valor
    /// crudo de la celda (ya resuelto si era un shared string). Solo
    /// incluye celdas con contenido real.
    /// </summary>
    public static List<Dictionary<int, string>> LeerHoja(Stream xlsxStream, string nombreHojaVisible)
    {
        using var archive = new ZipArchive(xlsxStream, ZipArchiveMode.Read);

        var workbookEntry = archive.GetEntry("xl/workbook.xml")
            ?? throw new InvalidOperationException("El archivo no parece un .xlsx válido (falta xl/workbook.xml).");
        var workbookDoc = new XmlDocument();
        workbookDoc.Load(workbookEntry.Open());
        var wns = new XmlNamespaceManager(workbookDoc.NameTable);
        wns.AddNamespace("a", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");
        wns.AddNamespace("r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");

        var sheetNode = workbookDoc.SelectSingleNode(
            $"//a:sheets/a:sheet[translate(@name,'ÁÉÍÓÚ','AEIOU')='{nombreHojaVisible.ToUpperInvariant()}']", wns)
            ?? throw new InvalidOperationException($"No existe la hoja '{nombreHojaVisible}' en el Excel.");
        var rId = sheetNode.Attributes!["r:id"]!.Value;

        var relsEntry = archive.GetEntry("xl/_rels/workbook.xml.rels")
            ?? throw new InvalidOperationException("Falta xl/_rels/workbook.xml.rels.");
        var relsDoc = new XmlDocument();
        relsDoc.Load(relsEntry.Open());
        var relNode = relsDoc.SelectSingleNode($"//*[local-name()='Relationship'][@Id='{rId}']")
            ?? throw new InvalidOperationException($"No se pudo resolver la relación '{rId}' de la hoja.");
        var target = relNode.Attributes!["Target"]!.Value; // ej. "worksheets/sheet5.xml"

        var sharedStrings = new List<string>();
        var ssEntry = archive.GetEntry("xl/sharedStrings.xml");
        if (ssEntry != null)
        {
            var ssDoc = new XmlDocument();
            ssDoc.Load(ssEntry.Open());
            foreach (XmlNode si in ssDoc.SelectNodes("//a:si", wns)!)
            {
                var texts = si.SelectNodes(".//a:t", wns)!;
                var s = "";
                foreach (XmlNode t in texts) s += t.InnerText;
                sharedStrings.Add(s);
            }
        }

        var sheetEntry = archive.GetEntry($"xl/{target}")
            ?? throw new InvalidOperationException($"Falta xl/{target} (hoja resuelta pero el archivo no existe dentro del .xlsx).");
        var sheetDoc = new XmlDocument();
        sheetDoc.Load(sheetEntry.Open());

        var filas = new List<Dictionary<int, string>>();
        foreach (XmlNode row in sheetDoc.SelectNodes("//a:sheetData/a:row", wns)!)
        {
            var celdas = new Dictionary<int, string>();
            foreach (XmlNode c in row.SelectNodes("a:c", wns)!)
            {
                var cellRef = c.Attributes?["r"]?.Value;
                if (cellRef is null) continue;
                var colIndex = ColumnaACero(cellRef);
                var type = c.Attributes?["t"]?.Value;
                var vNode = c.SelectSingleNode("a:v", wns);
                var value = vNode?.InnerText ?? "";
                if (type == "s" && int.TryParse(value, out var idx) && idx >= 0 && idx < sharedStrings.Count)
                    value = sharedStrings[idx];
                else if (type == "inlineStr")
                    value = c.SelectSingleNode("a:is/a:t", wns)?.InnerText ?? "";
                if (!string.IsNullOrEmpty(value)) celdas[colIndex] = value;
            }
            if (celdas.Count > 0) filas.Add(celdas);
        }
        return filas;
    }

    /// <summary>"B9" -> 1 (A=0, B=1, ..., Z=25, AA=26...), ignora la parte numérica.</summary>
    private static int ColumnaACero(string cellRef)
    {
        var letras = new string(cellRef.TakeWhile(char.IsLetter).ToArray());
        var col = 0;
        foreach (var ch in letras) col = col * 26 + (char.ToUpperInvariant(ch) - 'A' + 1);
        return col - 1;
    }
}
