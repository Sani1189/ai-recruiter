using System.Globalization;
using System.IO.Compression;
using System.Xml;
using System.Xml.Linq;

namespace Recruiter.Application.QuestionnaireTemplate.Import.OpenXml;

/// <summary>
/// Minimal .xlsx (OpenXML) reader for the questionnaire import use-case.
/// We only need to read a single worksheet by name and return rows keyed by the header row.
/// No external packages required.
/// </summary>
internal static class XlsxReader
{
    private static readonly XNamespace MainNs = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private static readonly XNamespace RelNs = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    private static readonly XNamespace PkgRelNs = "http://schemas.openxmlformats.org/package/2006/relationships";

    public static IReadOnlyList<Dictionary<string, string?>> ReadWorksheetRowsByHeader(
        Stream xlsxStream,
        string worksheetName)
    {
        if (xlsxStream == null) throw new ArgumentNullException(nameof(xlsxStream));
        if (!xlsxStream.CanRead) throw new ArgumentException("Stream must be readable.", nameof(xlsxStream));
        if (string.IsNullOrWhiteSpace(worksheetName)) throw new ArgumentNullException(nameof(worksheetName));

        // ZipArchive requires seek for some stream types; our WebAPI copies uploads into MemoryStream so this is fine.
        if (!xlsxStream.CanSeek)
            throw new InvalidOperationException("Unsupported stream for .xlsx import. Stream must be seekable.");

        xlsxStream.Position = 0;

        using var zip = new ZipArchive(xlsxStream, ZipArchiveMode.Read, leaveOpen: true);

        var sharedStrings = LoadSharedStrings(zip);
        var worksheetPath = ResolveWorksheetPath(zip, worksheetName);
        var worksheetDoc = LoadXml(zip, worksheetPath);

        var sheetData = worksheetDoc.Root?.Element(MainNs + "sheetData");
        if (sheetData == null)
            return Array.Empty<Dictionary<string, string?>>();

        var rows = sheetData.Elements(MainNs + "row").ToList();
        if (rows.Count == 0)
            return Array.Empty<Dictionary<string, string?>>();

        // Row 1: header, Row 2: hint (optional), then data rows
        var headerRow = rows[0];
        var headers = ReadRowCells(headerRow, sharedStrings)
            .OrderBy(x => x.Index)
            .Select(x => NormalizeHeader(x.Value))
            .ToList();

        if (headers.Count == 0 || headers.All(string.IsNullOrWhiteSpace))
            throw new InvalidOperationException($"Worksheet '{worksheetName}' is missing a header row.");

        var startAt = rows.Count >= 2 ? 2 : 1;
        var result = new List<Dictionary<string, string?>>();
        for (var i = startAt; i < rows.Count; i++)
        {
            var dict = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            foreach (var cell in ReadRowCells(rows[i], sharedStrings))
            {
                var headerIndex = cell.Index - 1;
                if (headerIndex < 0 || headerIndex >= headers.Count)
                    continue;

                var header = headers[headerIndex];
                if (string.IsNullOrWhiteSpace(header))
                    continue;

                dict[header] = cell.Value;
            }

            if (dict.Count == 0)
                continue;

            result.Add(dict);
        }

        return result;
    }

    private static string NormalizeHeader(string? raw)
    {
        // Be forgiving: users may edit in Google Sheets which can introduce spaces in headers.
        // Example: "Section Order" => "SectionOrder"
        var s = (raw ?? string.Empty).Trim();
        if (s.Length == 0) return string.Empty;

        Span<char> buf = stackalloc char[s.Length];
        var j = 0;
        foreach (var ch in s)
        {
            if (char.IsWhiteSpace(ch) || ch == '_' || ch == '-' || ch == '/')
                continue;
            buf[j++] = ch;
        }

        return new string(buf[..j]);
    }

    private static string ResolveWorksheetPath(ZipArchive zip, string worksheetName)
    {
        var workbook = LoadXml(zip, "xl/workbook.xml");
        var sheets = workbook.Root?.Element(MainNs + "sheets");
        if (sheets == null)
            throw new InvalidOperationException("Invalid .xlsx: missing xl/workbook.xml sheets.");

        var sheetElements = sheets.Elements(MainNs + "sheet").ToList();
        var sheet = sheetElements.FirstOrDefault(s => string.Equals((string?)s.Attribute("name"), worksheetName, StringComparison.Ordinal));

        // Fallback: if "Import" not found but file has exactly one sheet, use it (e.g. ChatGPT-generated .xlsx with "Sheet").
        if (sheet == null && sheetElements.Count == 1)
            sheet = sheetElements[0];

        if (sheet == null)
        {
            var availableNames = sheetElements
                .Select(s => (string?)s.Attribute("name"))
                .Where(n => !string.IsNullOrEmpty(n))
                .Select(n => $"'{n}'")
                .ToList();
            var availableStr = availableNames.Count > 0
                ? string.Join(", ", availableNames)
                : "(none)";
            throw new InvalidOperationException(
                $"Worksheet '{worksheetName}' was not found. " +
                $"Available worksheets: {availableStr}. " +
                "Rename your worksheet to 'Import' or use the provided template (e.g. personality-test-questions.xls).");
        }

        var relId = (string?)sheet.Attribute(RelNs + "id");
        if (string.IsNullOrWhiteSpace(relId))
            throw new InvalidOperationException($"Invalid .xlsx: worksheet '{worksheetName}' is missing relationship id.");

        var rels = LoadXml(zip, "xl/_rels/workbook.xml.rels");
        var relationship = rels.Root?
            .Elements(PkgRelNs + "Relationship")
            .FirstOrDefault(r => string.Equals((string?)r.Attribute("Id"), relId, StringComparison.Ordinal));

        var target = (string?)relationship?.Attribute("Target");
        if (string.IsNullOrWhiteSpace(target))
            throw new InvalidOperationException($"Invalid .xlsx: could not resolve worksheet target for '{worksheetName}'.");

        // Targets are relative to xl/
        target = target.Replace('\\', '/').TrimStart('/');
        return "xl/" + target;
    }

    private static List<string> LoadSharedStrings(ZipArchive zip)
    {
        var entry = zip.GetEntry("xl/sharedStrings.xml");
        if (entry == null)
            return new List<string>();

        var doc = LoadXml(zip, "xl/sharedStrings.xml");
        var list = new List<string>();

        foreach (var si in doc.Descendants(MainNs + "si"))
        {
            // <si><t>text</t></si> OR rich text: <si><r><t>...</t></r>...</si>
            var text = string.Concat(si.Descendants(MainNs + "t").Select(t => (string?)t ?? string.Empty));
            list.Add(text);
        }

        return list;
    }

    private static XDocument LoadXml(ZipArchive zip, string path)
    {
        var entry = zip.GetEntry(path);
        if (entry == null)
            throw new InvalidOperationException($"Invalid .xlsx: missing '{path}'.");

        using var stream = entry.Open();

        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null
        };

        using var reader = XmlReader.Create(stream, settings);
        return XDocument.Load(reader, LoadOptions.None);
    }

    private static IEnumerable<(int Index, string? Value)> ReadRowCells(XElement row, List<string> sharedStrings)
    {
        foreach (var c in row.Elements(MainNs + "c"))
        {
            var r = (string?)c.Attribute("r");
            if (string.IsNullOrWhiteSpace(r))
                continue;

            var colIndex = TryGetColumnIndex(r);
            if (colIndex <= 0)
                continue;

            var t = (string?)c.Attribute("t");
            string? value = null;

            if (string.Equals(t, "inlineStr", StringComparison.OrdinalIgnoreCase))
            {
                value = c.Element(MainNs + "is")?.Descendants(MainNs + "t").Select(x => (string?)x ?? string.Empty).Aggregate(string.Empty, string.Concat);
            }
            else
            {
                var v = c.Element(MainNs + "v")?.Value;
                if (!string.IsNullOrWhiteSpace(v))
                {
                    if (string.Equals(t, "s", StringComparison.OrdinalIgnoreCase) &&
                        int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out var sIdx) &&
                        sIdx >= 0 && sIdx < sharedStrings.Count)
                    {
                        value = sharedStrings[sIdx];
                    }
                    else
                    {
                        value = v;
                    }
                }
            }

            value = value?.Trim();
            if (string.IsNullOrWhiteSpace(value))
                value = null;

            yield return (colIndex, value);
        }
    }

    private static int TryGetColumnIndex(string cellRef)
    {
        // cellRef like "A1", "BC23"
        var letters = new string(cellRef.TakeWhile(char.IsLetter).ToArray());
        if (letters.Length == 0)
            return 0;

        var col = 0;
        foreach (var ch in letters.ToUpperInvariant())
        {
            if (ch < 'A' || ch > 'Z')
                return 0;
            col = (col * 26) + (ch - 'A' + 1);
        }

        return col;
    }
}

