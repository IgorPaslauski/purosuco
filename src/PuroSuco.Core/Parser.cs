namespace PuroSuco.Core;

public sealed class Parser
{
    private readonly IReadOnlyList<Token> _tokens;
    private int _position;

    public Parser(string source) : this(new Lexer(source).Lex()) { }
    public Parser(IReadOnlyList<Token> tokens) => _tokens = tokens;

    public CompilationUnit ParseCompilationUnit()
    {
        var members = new List<MemberSyntax>();

        while (Current.Kind != TokenKind.EndOfFile)
            members.Add(ParseMember());

        return new CompilationUnit(members);
    }

    private MemberSyntax ParseMember()
    {
        var modifiers = ParseModifiers();

        if (MatchKeyword("TROPA"))
        {
            var name = Consume(TokenKind.Identifier, "PS110", "CADÊ O NOME DA TROPA?", "Esperava o nome da classe.");
            var open = Consume(TokenKind.OpenBrace, "PS104", "ABRE ESSA CHAVE", "Esperava '{'.");

            var members = new List<MemberSyntax>();
            while (Current.Kind != TokenKind.CloseBrace && Current.Kind != TokenKind.EndOfFile)
                members.Add(ParseMember());

            Consume(TokenKind.CloseBrace, "PS105", "FECHA ESSA CHAVE", "Esperava '}'.");
            return new ClassDeclarationSyntax(name.Text, members, modifiers, open.Position);
        }

        if (LooksLikeFunction())
            return ParseFunction(modifiers);

        if (modifiers.Count > 0)
            throw new PuroSucoException("PS111", "AMOSTRADINHO PERDIDO", "Modificador sem classe ou função.", Current.Position);

        var stmt = ParseStatement();
        return new GlobalStatementSyntax(stmt, stmt.Position);
    }

    private FunctionDeclarationSyntax ParseFunction(IReadOnlyList<string> modifiers)
    {
        var returnType = Advance();
        var name = Consume(TokenKind.Identifier, "PS112", "CADÊ O NOME DA FUNÇÃO?", "Esperava o nome da função.");

        Consume(TokenKind.OpenParen, "PS108", "ABRE O PAPO", "Esperava '('.");

        var parameters = new List<ParameterSyntax>();
        while (Current.Kind != TokenKind.CloseParen && Current.Kind != TokenKind.EndOfFile)
        {
            var type = ConsumeType();
            var parameterName = Consume(TokenKind.Identifier, "PS113", "CADÊ O PARÂMETRO?", "Esperava o nome do parâmetro.");
            parameters.Add(new ParameterSyntax(type.Text, parameterName.Text, type.Position));

            if (Current.Text == ",")
                Advance();
            else
                break;
        }

        Consume(TokenKind.CloseParen, "PS109", "FECHA O PAPO", "Esperava ')'.");
        var body = ParseBlock();

        return new FunctionDeclarationSyntax(name.Text, returnType.Text, parameters, body, modifiers, returnType.Position);
    }

    private IReadOnlyList<string> ParseModifiers()
    {
        var modifiers = new List<string>();

        while (IsModifier(Current.Text))
            modifiers.Add(Advance().Text);

        return modifiers;
    }

    private bool LooksLikeFunction() =>
        IsTypeKeyword(Current.Text) &&
        Peek(1).Kind == TokenKind.Identifier &&
        Peek(2).Kind == TokenKind.OpenParen;

    private StatementSyntax ParseStatement()
    {
        if (MatchKeyword("TA_CERTO_ISSO")) return ParseIf();
        if (MatchKeyword("ENQUANTO_TANKAR")) return ParseWhile();
        if (MatchKeyword("TOMA")) return ParseReturn();

        if (MatchKeyword("CHEGA"))
        {
            var pos = Previous.Position;
            ConsumeOptional(TokenKind.Semicolon);
            return new BreakStatementSyntax(pos);
        }

        if (MatchKeyword("SEGUE_O_JOGO"))
        {
            var pos = Previous.Position;
            ConsumeOptional(TokenKind.Semicolon);
            return new ContinueStatementSyntax(pos);
        }

        if (IsTypeKeyword(Current.Text))
            return ParseVariableDeclaration();

        if (Current.Kind == TokenKind.Identifier &&
            Peek(1).Kind == TokenKind.Keyword &&
            Peek(1).Text.Equals("RECEBA", StringComparison.OrdinalIgnoreCase))
            return ParseAssignment();

        var expr = ParseExpression();
        ConsumeOptional(TokenKind.Semicolon);
        return new ExpressionStatementSyntax(expr, expr.Position);
    }

    private StatementSyntax ParseVariableDeclaration()
    {
        var type = ConsumeType();
        var identifier = Consume(TokenKind.Identifier, "PS101", "CADÊ O NOME?", "Esperava um nome de variável depois do tipo.");

        ExpressionSyntax? initializer = null;
        if (MatchKeyword("RECEBA"))
            initializer = ParseExpression();

        ConsumeOptional(TokenKind.Semicolon);
        return new VariableDeclarationSyntax(type.Text, identifier.Text, initializer, type.Position);
    }

    private StatementSyntax ParseAssignment()
    {
        var identifier = Consume(TokenKind.Identifier, "PS102", "QUE FOI ISSO?", "Esperava uma variável.");
        ConsumeKeyword("RECEBA", "PS103", "RECEBA CADÊ?", "Esperava RECEBA na atribuição.");
        var expression = ParseExpression();
        ConsumeOptional(TokenKind.Semicolon);
        return new AssignmentStatementSyntax(identifier.Text, expression, identifier.Position);
    }

    private IfStatementSyntax ParseIf()
    {
        var pos = Previous.Position;
        var condition = ParseExpression();
        var thenBlock = ParseBlock();

        BlockStatementSyntax? elseBlock = null;
        if (MatchKeyword("NAO_TA_NAO"))
            elseBlock = ParseBlock();

        return new IfStatementSyntax(condition, thenBlock, elseBlock, pos);
    }

    private WhileStatementSyntax ParseWhile()
    {
        var pos = Previous.Position;
        var condition = ParseExpression();
        var body = ParseBlock();
        return new WhileStatementSyntax(condition, body, pos);
    }

    private ReturnStatementSyntax ParseReturn()
    {
        var pos = Previous.Position;
        if (Current.Kind is TokenKind.Semicolon or TokenKind.CloseBrace)
        {
            ConsumeOptional(TokenKind.Semicolon);
            return new ReturnStatementSyntax(null, pos);
        }

        var expression = ParseExpression();
        ConsumeOptional(TokenKind.Semicolon);
        return new ReturnStatementSyntax(expression, pos);
    }

    private BlockStatementSyntax ParseBlock()
    {
        var open = Consume(TokenKind.OpenBrace, "PS104", "ABRE ESSA CHAVE", "Esperava '{'.");
        var statements = new List<StatementSyntax>();

        while (Current.Kind != TokenKind.CloseBrace && Current.Kind != TokenKind.EndOfFile)
            statements.Add(ParseStatement());

        Consume(TokenKind.CloseBrace, "PS105", "FECHA ESSA CHAVE", "Esperava '}'.");
        return new BlockStatementSyntax(statements, open.Position);
    }

    private ExpressionSyntax ParseExpression(int parentPrecedence = 0)
    {
        ExpressionSyntax left;

        if (Current.Kind == TokenKind.OpenParen)
        {
            Advance();
            left = ParseExpression();
            Consume(TokenKind.CloseParen, "PS106", "FECHA O PARÊNTESE", "Esperava ')'.");
        }
        else
        {
            left = ParsePrimary();
        }

        while (true)
        {
            var precedence = GetBinaryPrecedence(Current.Text);
            if (precedence == 0 || precedence <= parentPrecedence) break;

            var op = Advance();
            var right = ParseExpression(precedence);
            left = new BinaryExpressionSyntax(left, op.Text, right, op.Position);
        }

        return left;
    }

    private ExpressionSyntax ParsePrimary()
    {
        var token = Current;

        if (token.Kind == TokenKind.Number)
        {
            Advance();
            return new LiteralExpressionSyntax(int.Parse(token.Text), "NUMERO", token.Position);
        }

        if (token.Kind == TokenKind.String)
        {
            Advance();
            return new LiteralExpressionSyntax(token.Text[1..^1], "PAPO", token.Position);
        }

        if (token.Kind == TokenKind.Keyword && token.Text.Equals("CONFIA", StringComparison.OrdinalIgnoreCase))
        {
            Advance();
            return new LiteralExpressionSyntax(true, "CONFERE", token.Position);
        }

        if (token.Kind == TokenKind.Keyword && token.Text.Equals("CONFIA_NAO", StringComparison.OrdinalIgnoreCase))
        {
            Advance();
            return new LiteralExpressionSyntax(false, "CONFERE", token.Position);
        }

        if (token.Kind == TokenKind.Keyword && token.Text.Equals("TEM_NADA_AI", StringComparison.OrdinalIgnoreCase))
        {
            Advance();
            return new LiteralExpressionSyntax(null, "TEM_NADA_AI", token.Position);
        }

        if (token.Kind == TokenKind.Keyword && token.Text.Equals("MANDA_AI", StringComparison.OrdinalIgnoreCase))
        {
            Advance();
            return ParseCall(token.Text, token.Position);
        }

        if (token.Kind == TokenKind.Identifier)
        {
            Advance();

            if (Current.Kind == TokenKind.OpenParen)
                return ParseCall(token.Text, token.Position);

            return new NameExpressionSyntax(token.Text, token.Position);
        }

        throw new PuroSucoException("PS107", "QUE PAPINHO É ESSE?", $"Não entendi '{token.Text}' como expressão.", token.Position);
    }

    private CallExpressionSyntax ParseCall(string name, int position)
    {
        Consume(TokenKind.OpenParen, "PS108", "ABRE O PAPO", "Esperava '(' na chamada.");
        var args = new List<ExpressionSyntax>();

        while (Current.Kind != TokenKind.CloseParen && Current.Kind != TokenKind.EndOfFile)
        {
            args.Add(ParseExpression());
            if (Current.Text == ",") Advance();
            else break;
        }

        Consume(TokenKind.CloseParen, "PS109", "FECHA O PAPO", "Esperava ')' na chamada.");
        return new CallExpressionSyntax(name, args, position);
    }

    private Token ConsumeType()
    {
        if (IsTypeKeyword(Current.Text))
            return Advance();

        throw new PuroSucoException("PS114", "QUE TIPO É ESSE?", "Esperava um tipo PuroSuco.", Current.Position);
    }

    private static bool IsModifier(string text) =>
        text.Equals("AMOSTRADINHO", StringComparison.OrdinalIgnoreCase) ||
        text.Equals("NA_MIÚDA", StringComparison.OrdinalIgnoreCase) ||
        text.Equals("NA_MIUDA", StringComparison.OrdinalIgnoreCase) ||
        text.Equals("SO_OS_DE_VERDADE", StringComparison.OrdinalIgnoreCase) ||
        text.Equals("SEMPRE_FOI_ASSIM", StringComparison.OrdinalIgnoreCase) ||
        text.Equals("SO_NA_TEORIA", StringComparison.OrdinalIgnoreCase);

    private static int GetBinaryPrecedence(string op) => op switch
    {
        "*" or "/" => 5,
        "+" or "-" => 4,
        ">" or "<" or ">=" or "<=" => 3,
        "==" or "!=" => 2,
        _ => 0
    };

    private static bool IsTypeKeyword(string text) =>
        text.Equals("NUMERO", StringComparison.OrdinalIgnoreCase) ||
        text.Equals("NUMERO_QUEBRADO", StringComparison.OrdinalIgnoreCase) ||
        text.Equals("PAPO", StringComparison.OrdinalIgnoreCase) ||
        text.Equals("CONFERE", StringComparison.OrdinalIgnoreCase) ||
        text.Equals("SEI_LA", StringComparison.OrdinalIgnoreCase) ||
        text.Equals("VOLTA_NADA", StringComparison.OrdinalIgnoreCase);

    private Token Current => Peek(0);
    private Token Previous => _tokens[Math.Max(0, _position - 1)];

    private Token Peek(int offset)
    {
        var index = _position + offset;
        return index >= _tokens.Count ? _tokens[^1] : _tokens[index];
    }

    private Token Advance()
    {
        var current = Current;
        if (_position < _tokens.Count) _position++;
        return current;
    }

    private bool MatchKeyword(string keyword)
    {
        if (Current.Kind == TokenKind.Keyword &&
            Current.Text.Equals(keyword, StringComparison.OrdinalIgnoreCase))
        {
            Advance();
            return true;
        }

        return false;
    }

    private Token ConsumeKeyword(string keyword, string code, string title, string message)
    {
        if (MatchKeyword(keyword)) return Previous;
        throw new PuroSucoException(code, title, message, Current.Position);
    }

    private Token Consume(TokenKind kind, string code, string title, string message)
    {
        if (Current.Kind == kind) return Advance();
        throw new PuroSucoException(code, title, message, Current.Position);
    }

    private void ConsumeOptional(TokenKind kind)
    {
        if (Current.Kind == kind) Advance();
    }
}
