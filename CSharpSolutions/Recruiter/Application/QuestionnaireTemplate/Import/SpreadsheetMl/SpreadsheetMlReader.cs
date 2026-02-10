using System.Globalization;
using Recruiter.Application.QuestionnaireTemplate.Import.OpenXml;
using System.Xml;
using System.Xml.Linq;

namespace Recruiter.Application.QuestionnaireTemplate.Import.SpreadsheetMl;

internal static class SpreadsheetMlReader
{
    private static readonly XNamespace SsNs = "urn:schemas-microsoft-com:office:spreadsheet";

    public static IReadOnlyList<Dictionary<string, string?>> ReadWorksheetRowsByHeader(
        Stream spreadsheetStream,
        string worksheetName)
    {
        if (spreadsheetStream == null) throw new ArgumentNullException(nameof(spreadsheetStream));
        if (!spreadsheetStream.CanRead) throw new ArgumentException("Stream must be readable.", nameof(spreadsheetStream));

        // Support:
        // - SpreadsheetML XML (our provided ".xls" template is actually XML)
        // - .xlsx OpenXML (Google Drive / modern Excel saves)
        //
        // Reject:
        // - legacy binary .xls (OLE compound file)
        if (TryDetectXlsx(spreadsheetStream))
        {
            return XlsxReader.ReadWorksheetRowsByHeader(spreadsheetStream, worksheetName);
        }

        if (TryDetectLegacyBinaryXls(spreadsheetStream))
            throw new InvalidOperationException("Unsupported file type (legacy binary .xls). Please upload .xlsx or the provided SpreadsheetML (.xls) template.");

        XDocument doc;
        try
        {
            // Security: block DTD processing / external entity resolution (XXE) for uploaded files.
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null
            };

            using var reader = XmlReader.Create(spreadsheetStream, settings);
            doc = XDocument.Load(reader, LoadOptions.None);
        }
        catch (Exception ex) when (ex is XmlException or InvalidOperationException)
        {
            throw new InvalidOperationException(
                "Invalid file format. Please upload .xlsx or the provided SpreadsheetML (.xls) import template.",
                ex);
        }

        var worksheets = doc.Descendants(SsNs + "Worksheet").ToList();
        var worksheet = worksheets.FirstOrDefault(w => string.Equals((string?)w.Attribute(SsNs + "Name"), worksheetName, StringComparison.Ordinal));

        if (worksheet == null)
        {
            var availableNames = worksheets
                .Select(w => (string?)w.Attribute(SsNs + "Name"))
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

        var table = worksheet.Descendants(SsNs + "Table").FirstOrDefault();
        if (table == null)
            throw new InvalidOperationException($"Worksheet '{worksheetName}' is missing a Table.");

        var rows = table.Elements(SsNs + "Row").ToList();
        if (rows.Count == 0)
            return Array.Empty<Dictionary<string, string?>>();

        var headerRow = rows[0];
        var headers = ReadRowCells(headerRow)
            .OrderBy(c => c.Index)
            .Select(c => NormalizeHeader(c.Value))
            .ToList();

        if (headers.Count == 0 || headers.All(string.IsNullOrWhiteSpace))
            throw new InvalidOperationException($"Worksheet '{worksheetName}' is missing a header row.");

        // Data rows begin after:
        // - Row 1: header
        // - Row 2: hint row (optional, ignored)
        var startAt = rows.Count >= 2 ? 2 : 1;
        var result = new List<Dictionary<string, string?>>();
        for (var i = startAt; i < rows.Count; i++)
        {
            var rowDict = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            foreach (var cell in ReadRowCells(rows[i]))
            {
                var headerIndex = cell.Index - 1;
                if (headerIndex < 0 || headerIndex >= headers.Count)
                    continue;

                var header = headers[headerIndex];
                if (string.IsNullOrWhiteSpace(header))
                    continue;

                rowDict[header] = cell.Value;
            }

            // Keep blank rows out (very common at bottom of Excel sheet).
            if (rowDict.Count == 0)
                continue;

            result.Add(rowDict);
        }

        return result;
    }

    private static string NormalizeHeader(string? raw)
    {
        // Match the .xlsx normalization: tolerate spaces/underscores/dashes.
        // Example: "Question Title" => "QuestionTitle"
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

    private static IEnumerable<(int Index, string? Value)> ReadRowCells(XElement row)
    {
        // SpreadsheetML may omit empty cells. When present, "ss:Index" defines the 1-based column position.
        // If ss:Index is missing, the cell increments from the previous cell.
        var index = 0;
        foreach (var cell in row.Elements(SsNs + "Cell"))
        {
            var explicitIndex = (string?)cell.Attribute(SsNs + "Index");
            if (!string.IsNullOrWhiteSpace(explicitIndex) && int.TryParse(explicitIndex, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
            {
                index = parsed;
            }
            else
            {
                index++;
            }

            var data = cell.Element(SsNs + "Data");
            var value = data?.Value?.Trim();
            if (string.IsNullOrWhiteSpace(value))
                value = null;

            yield return (index, value);
        }
    }

    private static bool TryDetectXlsx(Stream stream)
    {
        if (!stream.CanSeek) return false;
        var originalPos = stream.Position;
        try
        {
            Span<byte> header = stackalloc byte[2];
            var read = stream.Read(header);
            return read >= 2 && header[0] == (byte)'P' && header[1] == (byte)'K';
        }
        finally { stream.Position = originalPos; }
    }

    private static bool TryDetectLegacyBinaryXls(Stream stream)
    {
        if (!stream.CanSeek) return false;
        var originalPos = stream.Position;
        try
        {
            Span<byte> header = stackalloc byte[8];
            var read = stream.Read(header);
            if (read < 8) return false;
            // OLE Compound File header (legacy binary .xls)
            return header[0] == 0xD0 && header[1] == 0xCF && header[2] == 0x11 && header[3] == 0xE0 &&
                   header[4] == 0xA1 && header[5] == 0xB1 && header[6] == 0x1A && header[7] == 0xE1;
        }
        finally { stream.Position = originalPos; }
    }
}

