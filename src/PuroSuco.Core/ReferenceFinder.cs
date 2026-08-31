using System.Text.RegularExpressions;

namespace PuroSuco.Core;

public sealed record ReferenceOccurrence(string Name, int Position, int Length);

public static class ReferenceFinder
{
    public static IReadOnlyList<ReferenceOccurrence> Find(string source, string name)
    {
        var pattern = $@"\b{Regex.Escape(name)}\b";
        return Regex.Matches(source, pattern)
            .Select(m => new ReferenceOccurrence(name, m.Index, m.Length))
            .ToArray();
    }
}
