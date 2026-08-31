namespace PuroSuco.Core;

public sealed class SemanticAnalyzer
{
    private readonly List<SemanticDiagnostic> _diagnostics = [];
    private readonly List<Symbol> _symbols = [];
    private readonly Stack<Dictionary<string, Symbol>> _scopes = new();
    private readonly Dictionary<string, FunctionDeclarationSyntax> _functions = new(StringComparer.Ordinal);
    private string? _currentReturnType;

    public SemanticModel Analyze(CompilationUnit unit)
    {
        _diagnostics.Clear();
        _symbols.Clear();
        _scopes.Clear();
        _functions.Clear();
        _currentReturnType = null;

        PushScope();

        foreach (var member in unit.Members)
        {
            if (member is FunctionDeclarationSyntax f)
                DeclareFunction(f);
            else if (member is ClassDeclarationSyntax c)
                DeclareClass(c);
        }

        foreach (var member in unit.Members)
            AnalyzeMember(member);

        PopScope();

        return new SemanticModel
        {
            SyntaxTree = unit,
            Diagnostics = _diagnostics.ToArray(),
            Symbols = _symbols.ToArray()
        };
    }

    private void DeclareClass(ClassDeclarationSyntax cls)
    {
        var symbol = new Symbol(cls.Name, SymbolKind.Class, cls.Name, cls.Position, cls);
        _symbols.Add(symbol);
    }

    private void DeclareFunction(FunctionDeclarationSyntax fn)
    {
        if (_functions.ContainsKey(fn.Name))
        {
            _diagnostics.Add(new("PS040", "FUNÇÃO REPETIDA", $"Já existe uma função '{fn.Name}'.", fn.Position, fn.Name.Length, true));
            return;
        }

        _functions[fn.Name] = fn;
        _symbols.Add(new Symbol(fn.Name, SymbolKind.Function, NormalizeType(fn.ReturnType), fn.Position, fn));
    }

    private void AnalyzeMember(MemberSyntax member)
    {
        switch (member)
        {
            case GlobalStatementSyntax g:
                AnalyzeStatement(g.Statement);
                break;

            case FunctionDeclarationSyntax f:
                AnalyzeFunction(f);
                break;

            case ClassDeclarationSyntax c:
                PushScope();
                foreach (var child in c.Members)
                    AnalyzeMember(child);
                PopScope();
                break;
        }
    }

    private void AnalyzeFunction(FunctionDeclarationSyntax fn)
    {
        PushScope();
        _currentReturnType = NormalizeType(fn.ReturnType);

        foreach (var parameter in fn.Parameters)
        {
            var symbol = new Symbol(parameter.Name, SymbolKind.Parameter, NormalizeType(parameter.TypeName), parameter.Position, parameter);
            DeclareLocal(symbol, parameter.Position);
        }

        AnalyzeBlock(fn.Body);

        _currentReturnType = null;
        PopScope();
    }

    private void AnalyzeStatement(StatementSyntax statement)
    {
        switch (statement)
        {
            case VariableDeclarationSyntax variable:
                AnalyzeVariable(variable);
                break;

            case AssignmentStatementSyntax assignment:
                AnalyzeAssignment(assignment);
                break;

            case ExpressionStatementSyntax expression:
                GetExpressionType(expression.Expression);
                break;

            case IfStatementSyntax @if:
                AnalyzeCondition(@if.Condition, @if.Position);
                AnalyzeBlock(@if.Then);
                if (@if.Else is not null) AnalyzeBlock(@if.Else);
                break;

            case WhileStatementSyntax @while:
                AnalyzeCondition(@while.Condition, @while.Position);
                AnalyzeBlock(@while.Body);
                break;

            case ReturnStatementSyntax ret:
                AnalyzeReturn(ret);
                break;

            case BlockStatementSyntax block:
                AnalyzeBlock(block);
                break;
        }
    }

    private void AnalyzeBlock(BlockStatementSyntax block)
    {
        PushScope();
        foreach (var statement in block.Statements)
            AnalyzeStatement(statement);
        PopScope();
    }

    private void AnalyzeVariable(VariableDeclarationSyntax variable)
    {
        var declaredType = NormalizeType(variable.TypeName);

        if (variable.Initializer is not null)
        {
            var valueType = GetExpressionType(variable.Initializer);

            if (declaredType != "var" && valueType is not null && !CanAssign(declaredType, valueType))
                _diagnostics.Add(new("PS003", "QUE PAPINHO É ESSE?", $"Esperava {Pretty(declaredType)}, recebeu {Pretty(valueType)}.", variable.Initializer.Position, 1, true));

            if (declaredType == "var" && valueType is not null)
                declaredType = valueType;
        }

        DeclareLocal(new Symbol(variable.Identifier, SymbolKind.Variable, declaredType, variable.Position, variable), variable.Position);
    }

    private void AnalyzeAssignment(AssignmentStatementSyntax assignment)
    {
        var symbol = Lookup(assignment.Identifier);

        if (symbol is null)
        {
            _diagnostics.Add(new("PS017", "METEU ESSA?", $"A variável '{assignment.Identifier}' não foi declarada nessa resenha.", assignment.Position, assignment.Identifier.Length, false));
            GetExpressionType(assignment.Expression);
            return;
        }

        var valueType = GetExpressionType(assignment.Expression);
        if (valueType is not null && !CanAssign(symbol.TypeName, valueType))
            _diagnostics.Add(new("PS003", "QUE PAPINHO É ESSE?", $"Não dá para meter {Pretty(valueType)} em {Pretty(symbol.TypeName)}.", assignment.Expression.Position, 1, true));
    }

    private void AnalyzeReturn(ReturnStatementSyntax ret)
    {
        var actual = ret.Expression is null ? "void" : GetExpressionType(ret.Expression);

        if (_currentReturnType is null)
        {
            _diagnostics.Add(new("PS050", "TOMA PRA QUEM?", "TOMA só faz sentido dentro de uma função.", ret.Position, 1, true));
            return;
        }

        if (actual is not null && !CanAssign(_currentReturnType, actual))
            _diagnostics.Add(new("PS051", "DEVOLVEU ERRADO", $"A função prometeu {Pretty(_currentReturnType)}, mas devolveu {Pretty(actual)}.", ret.Position, 1, true));
    }

    private void AnalyzeCondition(ExpressionSyntax expression, int position)
    {
        var type = GetExpressionType(expression);
        if (type is not null && type != "bool")
            _diagnostics.Add(new("PS030", "TÁ CERTO ISSO?", $"Condição deveria ser CONFERE, mas veio {Pretty(type)}.", position, 1, true));
    }

    private string? GetExpressionType(ExpressionSyntax expression)
    {
        switch (expression)
        {
            case LiteralExpressionSyntax literal:
                return NormalizeType(literal.TypeName);

            case NameExpressionSyntax name:
                var symbol = Lookup(name.Identifier);
                if (symbol is not null) return symbol.TypeName;

                _diagnostics.Add(new("PS017", "METEU ESSA?", $"Nunca vi '{name.Identifier}' nessa resenha.", name.Position, name.Identifier.Length, false));
                return null;

            case CallExpressionSyntax call:
                foreach (var arg in call.Arguments) GetExpressionType(arg);

                if (call.Name.Equals("MANDA_AI", StringComparison.OrdinalIgnoreCase))
                    return "void";

                if (_functions.TryGetValue(call.Name, out var fn))
                {
                    if (call.Arguments.Count != fn.Parameters.Count)
                        _diagnostics.Add(new("PS041", "FALTOU GENTE NA RESENHA", $"'{call.Name}' espera {fn.Parameters.Count} argumento(s), recebeu {call.Arguments.Count}.", call.Position, call.Name.Length, true));

                    return NormalizeType(fn.ReturnType);
                }

                _diagnostics.Add(new("PS042", "QUEM É ESSE CARA?", $"Função '{call.Name}' não foi declarada.", call.Position, call.Name.Length, false));
                return null;

            case BinaryExpressionSyntax binary:
                return AnalyzeBinary(binary);

            default:
                return null;
        }
    }

    private string? AnalyzeBinary(BinaryExpressionSyntax binary)
    {
        var left = GetExpressionType(binary.Left);
        var right = GetExpressionType(binary.Right);

        if (binary.Operator == "/" &&
            binary.Right is LiteralExpressionSyntax { Value: int value } &&
            value == 0)
            _diagnostics.Add(new("PS021", "AÍ TU ME QUEBRA", "Divisão literal por zero detectada.", binary.Right.Position, 1, true));

        if (binary.Operator is "==" or "!=" or ">" or "<" or ">=" or "<=")
            return "bool";

        if (left == "string" || right == "string")
        {
            if (binary.Operator == "+") return "string";
            _diagnostics.Add(new("PS031", "QUE CONTA É ESSA?", $"Operador '{binary.Operator}' não combina com PAPO.", binary.Position, 1, true));
            return null;
        }

        if (left == "double" || right == "double") return "double";
        if (left == "int" && right == "int") return "int";
        return null;
    }

    private void DeclareLocal(Symbol symbol, int position)
    {
        var scope = _scopes.Peek();

        if (scope.ContainsKey(symbol.Name))
        {
            _diagnostics.Add(new("PS018", "AMOSTRADINHO DEMAIS", $"'{symbol.Name}' já existe nesse escopo.", position, symbol.Name.Length, true));
            return;
        }

        scope[symbol.Name] = symbol;
        _symbols.Add(symbol);
    }

    private Symbol? Lookup(string name)
    {
        foreach (var scope in _scopes)
            if (scope.TryGetValue(name, out var symbol))
                return symbol;

        return _symbols.LastOrDefault(s => s.Kind is SymbolKind.Function or SymbolKind.Class && s.Name == name);
    }

    private void PushScope() => _scopes.Push(new Dictionary<string, Symbol>(StringComparer.Ordinal));
    private void PopScope() => _scopes.Pop();

    private static bool CanAssign(string target, string value) =>
        target == value || (target == "double" && value == "int") || target == "var" || (target != "void" && value == "null");

    private static string NormalizeType(string type) => type.ToUpperInvariant() switch
    {
        "NUMERO" => "int",
        "NUMERO_QUEBRADO" => "double",
        "PAPO" => "string",
        "CONFERE" => "bool",
        "TEM_NADA_AI" => "null",
        "SEI_LA" => "var",
        "VOLTA_NADA" => "void",
        _ => type.ToLowerInvariant()
    };

    private static string Pretty(string type) => type switch
    {
        "int" => "NUMERO",
        "double" => "NUMERO_QUEBRADO",
        "string" => "PAPO",
        "bool" => "CONFERE",
        "null" => "TEM_NADA_AI",
        "void" => "VOLTA_NADA",
        _ => type
    };
}
