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
        if (MatchKeyword("CHAMA"))
        {
            var pos = Previous.Position;
            var ns = ConsumeNamespaceName();
            ConsumeOptional(TokenKind.Semicolon);
            return new UsingDirectiveSyntax(ns, pos);
        }

        if (MatchKeyword("QUEBRADA"))
        {
            var pos = Previous.Position;
            var ns = ConsumeNamespaceName();
            var open = Consume(TokenKind.OpenBrace, "PS104", "ABRE ESSA CHAVE", "Esperava '{' na declaração de QUEBRADA.");

            var members = new List<MemberSyntax>();
            while (Current.Kind != TokenKind.CloseBrace && Current.Kind != TokenKind.EndOfFile)
                members.Add(ParseMember());

            Consume(TokenKind.CloseBrace, "PS105", "FECHA ESSA CHAVE", "Esperava '}' na QUEBRADA.");
            return new NamespaceDeclarationSyntax(ns, members, pos);
        }

        var modifiers = ParseModifiers();

        if (IsTypeDeclarationKeyword(Current.Text))
        {
            var kindToken = Advance();
            var kindNormalized = Keywords.Normalize(kindToken.Text);
            var name = Consume(TokenKind.Identifier, "PS110", "CADÊ O NOME?", $"Esperava o nome após {kindToken.Text}.");
            var open = Consume(TokenKind.OpenBrace, "PS104", "ABRE ESSA CHAVE", "Esperava '{'.");

            var members = new List<MemberSyntax>();
            while (Current.Kind != TokenKind.CloseBrace && Current.Kind != TokenKind.EndOfFile)
                members.Add(ParseMember());

            Consume(TokenKind.CloseBrace, "PS105", "FECHA ESSA CHAVE", "Esperava '}'.");

            if (kindNormalized == "TROPA")
                return new ClassDeclarationSyntax(name.Text, members, modifiers, open.Position);

            return new TypeDeclarationSyntax(kindNormalized, name.Text, members, modifiers, open.Position);
        }

        if (LooksLikeFunction())
            return ParseFunction(modifiers);

        if (modifiers.Count > 0)
            throw new PuroSucoException("PS111", "AMOSTRADINHO PERDIDO", "Modificador sem classe ou função.", Current.Position);

        var stmt = ParseStatement();
        return new GlobalStatementSyntax(stmt, stmt.Position);
    }

    private string ConsumeNamespaceName()
    {
        var sb = new System.Text.StringBuilder();
        var id = Consume(TokenKind.Identifier, "PS115", "CADÊ O NOME?", "Esperava nome de namespace/biblioteca.");
        sb.Append(id.Text);

        while (Current.Text == ".")
        {
            Advance();
            var next = Consume(TokenKind.Identifier, "PS115", "CADÊ O NOME?", "Esperava identificador após '.'.");
            sb.Append('.').Append(next.Text);
        }

        return sb.ToString();
    }

    private static bool IsTypeDeclarationKeyword(string text)
    {
        var norm = Keywords.Normalize(text);
        return norm is "TROPA" or "PRINT" or "CARDAPIO" or "MINI_TROPA" or "PAPO_RETO";
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
        if (MatchKeyword("FAZ_PRIMEIRO")) return ParseDoWhile();
        if (MatchKeyword("BORA_BILL")) return ParseFor();
        if (MatchKeyword("PRA_CADA_UM")) return ParseForeach();
        if (MatchKeyword("VAI_DAR_BOM")) return ParseTry();
        if (MatchKeyword("AI_TU_ME_QUEBRA")) return ParseThrow();
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
            Keywords.Normalize(Peek(1).Text) == "RECEBA")
            return ParseAssignment();

        var expr = ParseExpression();
        ConsumeOptional(TokenKind.Semicolon);
        return new ExpressionStatementSyntax(expr, expr.Position);
    }

    private TryStatementSyntax ParseTry()
    {
        var pos = Previous.Position;
        var tryBlock = ParseBlock();
        var catches = new List<CatchClauseSyntax>();

        while (MatchKeyword("DEU_RUIM") || MatchKeyword("METEU_ESSA"))
        {
            var catchPos = Previous.Position;
            string? exType = null;
            string? exName = null;

            if (MatchOptional(TokenKind.OpenParen))
            {
                if (Current.Kind is TokenKind.Identifier or TokenKind.Keyword && Current.Kind != TokenKind.CloseParen)
                {
                    exType = Advance().Text;
                    if (Current.Kind == TokenKind.Identifier)
                        exName = Advance().Text;
                }
                Consume(TokenKind.CloseParen, "PS109", "FECHA O PAPO", "Esperava ')' no bloco de captura de erro.");
            }

            var catchBody = ParseBlock();
            catches.Add(new CatchClauseSyntax(exType, exName, catchBody, catchPos));
        }

        BlockStatementSyntax? finallyBlock = null;
        if (MatchKeyword("DE_QUALQUER_JEITO"))
        {
            finallyBlock = ParseBlock();
        }

        return new TryStatementSyntax(tryBlock, catches, finallyBlock, pos);
    }

    private ThrowStatementSyntax ParseThrow()
    {
        var pos = Previous.Position;
        ExpressionSyntax? expr = null;

        if (Current.Kind != TokenKind.Semicolon && Current.Kind != TokenKind.CloseBrace && Current.Kind != TokenKind.EndOfFile)
            expr = ParseExpression();

        ConsumeOptional(TokenKind.Semicolon);
        return new ThrowStatementSyntax(expr, pos);
    }

    private ForeachStatementSyntax ParseForeach()
    {
        var pos = Previous.Position;
        var hasParen = MatchOptional(TokenKind.OpenParen);

        var type = ConsumeType();
        var identifier = Consume(TokenKind.Identifier, "PS101", "CADÊ O NOME?", "Esperava variável de iteração no PRA_CADA_UM.");
        ConsumeKeyword("DENTRO_DE", "PS116", "DENTRO DE QUEM?", "Esperava DENTRO_DE no PRA_CADA_UM.");
        var collection = ParseExpression();

        if (hasParen)
            Consume(TokenKind.CloseParen, "PS109", "FECHA O PAPO", "Esperava ')' no PRA_CADA_UM.");

        var body = ParseBlock();
        return new ForeachStatementSyntax(type.Text, identifier.Text, collection, body, pos);
    }

    private DoWhileStatementSyntax ParseDoWhile()
    {
        var pos = Previous.Position;
        var body = ParseBlock();
        ConsumeKeyword("ENQUANTO_TANKAR", "PS117", "TANKA ATÉ QUANDO?", "Esperava ENQUANTO_TANKAR após FAZ_PRIMEIRO.");
        var condition = ParseExpression();
        ConsumeOptional(TokenKind.Semicolon);
        return new DoWhileStatementSyntax(body, condition, pos);
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

    private ForStatementSyntax ParseFor()
    {
        var pos = Previous.Position;
        var hasParen = MatchOptional(TokenKind.OpenParen);

        StatementSyntax? initializer = null;
        if (Current.Kind != TokenKind.Semicolon)
        {
            if (IsTypeKeyword(Current.Text))
                initializer = ParseVariableDeclaration();
            else if (Current.Kind == TokenKind.Identifier && Peek(1).Kind == TokenKind.Keyword && Keywords.Normalize(Peek(1).Text) == "RECEBA")
                initializer = ParseAssignment();
            else
            {
                var expr = ParseExpression();
                ConsumeOptional(TokenKind.Semicolon);
                initializer = new ExpressionStatementSyntax(expr, expr.Position);
            }
        }
        else
        {
            Advance();
        }

        ExpressionSyntax? condition = null;
        if (Current.Kind != TokenKind.Semicolon)
            condition = ParseExpression();
        ConsumeOptional(TokenKind.Semicolon);

        StatementSyntax? increment = null;
        if (Current.Kind != TokenKind.CloseParen && Current.Kind != TokenKind.OpenBrace)
        {
            if (Current.Kind == TokenKind.Identifier && Peek(1).Kind == TokenKind.Keyword && Keywords.Normalize(Peek(1).Text) == "RECEBA")
                increment = ParseAssignment();
            else
            {
                var expr = ParseExpression();
                ConsumeOptional(TokenKind.Semicolon);
                increment = new ExpressionStatementSyntax(expr, expr.Position);
            }
        }

        if (hasParen)
            Consume(TokenKind.CloseParen, "PS109", "FECHA O PAPO", "Esperava ')' no BORA_BILL.");

        var body = ParseBlock();
        return new ForStatementSyntax(initializer, condition, increment, body, pos);
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

        if (MatchKeyword("PERAI"))
        {
            var pos = Previous.Position;
            var operand = ParseExpression(6);
            left = new AwaitExpressionSyntax(operand, pos);
        }
        else if (MatchKeyword("BROTOU"))
        {
            var pos = Previous.Position;
            var typeToken = ConsumeTypeOrIdentifier();
            var args = new List<ExpressionSyntax>();
            if (MatchOptional(TokenKind.OpenParen))
            {
                while (Current.Kind != TokenKind.CloseParen && Current.Kind != TokenKind.EndOfFile)
                {
                    args.Add(ParseExpression());
                    if (Current.Text == ",") Advance();
                    else break;
                }
                Consume(TokenKind.CloseParen, "PS109", "FECHA O PAPO", "Esperava ')' no BROTOU.");
            }
            left = new NewExpressionSyntax(typeToken.Text, args, pos);
        }
        else if (Current.Kind == TokenKind.OpenParen)
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
            if (Current.Text == ".")
            {
                Advance();
                var member = Consume(TokenKind.Identifier, "PS118", "CADÊ O MEMBRO?", "Esperava nome de método ou propriedade após '.'.");
                if (Current.Kind == TokenKind.OpenParen)
                {
                    var call = ParseCall(member.Text, member.Position);
                    left = new MemberAccessExpressionSyntax(left, call.Name + "(" + string.Join(", ", call.Arguments) + ")", member.Position);
                }
                else
                {
                    left = new MemberAccessExpressionSyntax(left, member.Text, member.Position);
                }
                continue;
            }

            var precedence = GetBinaryPrecedence(Current.Text);
            if (precedence == 0 || precedence <= parentPrecedence) break;

            var op = Advance();
            var right = ParseExpression(precedence);
            left = new BinaryExpressionSyntax(left, op.Text, right, op.Position);
        }

        return left;
    }

    private Token ConsumeTypeOrIdentifier()
    {
        if (IsTypeKeyword(Current.Text) || Current.Kind == TokenKind.Identifier)
            return Advance();

        throw new PuroSucoException("PS114", "QUE TIPO É ESSE?", "Esperava um tipo ou classe.", Current.Position);
    }

    private ExpressionSyntax ParsePrimary()
    {
        var token = Current;

        if (token.Kind == TokenKind.Number)
        {
            Advance();
            if (token.Text.Contains('.'))
                return new LiteralExpressionSyntax(double.Parse(token.Text, System.Globalization.CultureInfo.InvariantCulture), "NUMERO_QUEBRADO", token.Position);

            return new LiteralExpressionSyntax(int.Parse(token.Text), "NUMERO", token.Position);
        }

        if (token.Kind == TokenKind.String)
        {
            Advance();
            return new LiteralExpressionSyntax(token.Text[1..^1], "PAPO", token.Position);
        }

        var normalized = Keywords.Normalize(token.Text);

        if (token.Kind == TokenKind.Keyword && normalized == "CONFIA")
        {
            Advance();
            return new LiteralExpressionSyntax(true, "CONFERE", token.Position);
        }

        if (token.Kind == TokenKind.Keyword && (normalized == "CONFIA_NAO" || normalized == "E_MENTIRA"))
        {
            Advance();
            return new LiteralExpressionSyntax(false, "CONFERE", token.Position);
        }

        if (token.Kind == TokenKind.Keyword && normalized == "TEM_NADA_AI")
        {
            Advance();
            return new LiteralExpressionSyntax(null, "TEM_NADA_AI", token.Position);
        }

        if (token.Kind == TokenKind.Keyword && normalized == "MANDA_AI")
        {
            Advance();
            return ParseCall(token.Text, token.Position);
        }

        if (token.Kind == TokenKind.Keyword && normalized == "SOLTA_AI")
        {
            Advance();
            return ParseCall(token.Text, token.Position);
        }

        if (token.Kind == TokenKind.Keyword && normalized == "FALA_TU")
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

    private static bool IsModifier(string text)
    {
        var normalized = Keywords.Normalize(text);
        return normalized is "AMOSTRADINHO"
            or "NA_MIUDA"
            or "SO_OS_DE_VERDADE"
            or "SO_ENTRE_NOS"
            or "SEMPRE_FOI_ASSIM"
            or "SO_NA_TEORIA"
            or "SO_OLHA_NAO_TOCA"
            or "LACRADO"
            or "FICA_A_VONTADE"
            or "ASSUME_A_RESPONSA"
            or "NAO_MEXE";
    }

    private static int GetBinaryPrecedence(string op) => op switch
    {
        "*" or "/" => 5,
        "+" or "-" => 4,
        ">" or "<" or ">=" or "<=" => 3,
        "==" or "!=" => 2,
        _ => 0
    };

    private static bool IsTypeKeyword(string text)
    {
        var normalized = Keywords.Normalize(text);
        return normalized is "NUMERO"
            or "NUMERO_QUEBRADO"
            or "NUMERO_BRUTO"
            or "PAPO"
            or "LETRA"
            or "GRANA"
            or "CONFERE"
            or "QUALQUER_COISA"
            or "SEI_LA"
            or "VAI_NA_FE"
            or "VOLTA_NADA";
    }

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
            Keywords.Normalize(Current.Text) == Keywords.Normalize(keyword))
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

    private bool MatchOptional(TokenKind kind)
    {
        if (Current.Kind == kind)
        {
            Advance();
            return true;
        }
        return false;
    }

    private void ConsumeOptional(TokenKind kind)
    {
        if (Current.Kind == kind) Advance();
    }
}
