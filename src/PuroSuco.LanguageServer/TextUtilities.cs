using System.Text.RegularExpressions;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace PuroSuco.LanguageServer;

public static class TextUtilities
{
    public static int ToOffset(string text, Position position)
    {
        var line = 0;
        var offset = 0;

        while (offset < text.Length && line < position.Line)
        {
            if (text[offset++] == '\n')
                line++;
        }

        return Math.Min(text.Length, offset + position.Character);
    }

    public static Position ToPosition(string text, int index)
    {
        var line = 0;
        var character = 0;

        for (var i = 0; i < index && i < text.Length; i++)
        {
            if (text[i] == '\n')
            {
                line++;
                character = 0;
            }
            else
            {
                character++;
            }
        }

        return new Position(line, character);
    }

    public static (string Word, int Start, int Length)? WordAt(string text, Position position)
    {
        var offset = ToOffset(text, position);

        foreach (Match match in Regex.Matches(text, @"[A-Za-zÀ-ÿ_][A-Za-zÀ-ÿ0-9_]*"))
        {
            if (offset >= match.Index && offset <= match.Index + match.Length)
                return (match.Value, match.Index, match.Length);
        }

        return null;
    }
}
