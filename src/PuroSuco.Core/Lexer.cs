using System.Text;

namespace PuroSuco.Core;

public sealed class Lexer
{
    private readonly string _source;
    private int _position;

    public Lexer(string source) => _source = source;

    public IReadOnlyList<Token> Lex()
    {
        var tokens = new List<Token>();

        while (_position < _source.Length)
        {
            var c = _source[_position];

            if (char.IsWhiteSpace(c))
            {
                _position++;
                continue;
            }

            if (char.IsLetter(c) || c == '_')
            {
                tokens.Add(ReadWord());
                continue;
            }

            if (char.IsDigit(c))
            {
                tokens.Add(ReadNumber());
                continue;
            }

            if (c == '"')
            {
                tokens.Add(ReadString());
                continue;
            }

            if ("+-*/><!".Contains(c))
            {
                tokens.Add(ReadOperator());
                continue;
            }

            var start = _position++;
            tokens.Add(c switch
            {
                '{' => new Token(TokenKind.OpenBrace, "{", start),
                '}' => new Token(TokenKind.CloseBrace, "}", start),
                '(' => new Token(TokenKind.OpenParen, "(", start),
                ')' => new Token(TokenKind.CloseParen, ")", start),
                ';' => new Token(TokenKind.Semicolon, ";", start),
                ',' => new Token(TokenKind.Operator, ",", start),
                _ => new Token(TokenKind.Operator, c.ToString(), start)
            });
        }

        tokens.Add(new Token(TokenKind.EndOfFile, string.Empty, _position));
        return tokens;
    }

    private Token ReadWord()
    {
        var start = _position;
        while (_position < _source.Length &&
               (char.IsLetterOrDigit(_source[_position]) || _source[_position] == '_'))
            _position++;

        var text = _source[start.._position];
        return new Token(Keywords.IsKeyword(text) ? TokenKind.Keyword : TokenKind.Identifier, text, start);
    }

    private Token ReadNumber()
    {
        var start = _position;
        while (_position < _source.Length && char.IsDigit(_source[_position]))
            _position++;

        return new Token(TokenKind.Number, _source[start.._position], start);
    }

    private Token ReadString()
    {
        var start = _position++;
        var sb = new StringBuilder();

        while (_position < _source.Length && _source[_position] != '"')
        {
            if (_source[_position] == '\\' && _position + 1 < _source.Length)
            {
                sb.Append(_source[_position]);
                sb.Append(_source[_position + 1]);
                _position += 2;
                continue;
            }

            sb.Append(_source[_position++]);
        }

        if (_position >= _source.Length)
            throw new PuroSucoException("PS001", "QUE PAPINHO É ESSE?", "String não foi fechada. Tá certo isso? Não.", start);

        _position++;
        return new Token(TokenKind.String, $"\"{sb}\"", start);
    }

    private Token ReadOperator()
    {
        var start = _position;
        var first = _source[_position++].ToString();

        if (_position < _source.Length && _source[_position] == '=')
            return new Token(TokenKind.Operator, first + _source[_position++], start);

        return new Token(TokenKind.Operator, first, start);
    }
}
