namespace PuroSuco.Core;

public sealed record CodeFix(
    string Title,
    int Start,
    int Length,
    string Replacement);

public static class CodeFixProvider
{
    public static IReadOnlyList<CodeFix> GetFixes(string source, SemanticDiagnostic diagnostic)
    {
        var fixes = new List<CodeFix>();

        if (diagnostic.Code == "PS003")
        {
            var lineStart = source.LastIndexOf('\n', Math.Max(0, diagnostic.Position - 1)) + 1;
            var lineEnd = source.IndexOf('\n', diagnostic.Position);
            if (lineEnd < 0) lineEnd = source.Length;
            var line = source[lineStart..lineEnd];

            // NUMERO x RECEBA "23";
            var marker = "RECEBA";
            var receba = line.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (receba >= 0)
            {
                var after = line[(receba + marker.Length)..].Trim().TrimEnd(';').Trim();
                if (after.Length >= 2 && after.StartsWith('"') && after.EndsWith('"'))
                {
                    var inner = after[1..^1];
                    if (int.TryParse(inner, out _))
                    {
                        var absolute = source.IndexOf(after, lineStart, StringComparison.Ordinal);
                        if (absolute >= 0)
                        {
                            fixes.Add(new CodeFix(
                                "Arruma essa resenha: transformar PAPO em NUMERO",
                                absolute,
                                after.Length,
                                inner));
                        }
                    }
                }
            }
        }

        if (diagnostic.Code == "PS021")
        {
            fixes.Add(new CodeFix(
                "Arruma essa resenha: trocar zero por 1",
                diagnostic.Position,
                Math.Max(1, diagnostic.Length),
                "1"));
        }

        return fixes;
    }
}
