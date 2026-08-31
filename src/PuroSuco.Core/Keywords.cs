using System.Globalization;
using System.Text;

namespace PuroSuco.Core;

public static class Keywords
{
    public static string Normalize(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        return sb.ToString().Normalize(NormalizationForm.FormC).ToUpperInvariant();
    }

    public static readonly IReadOnlyDictionary<string, string> ToCSharp =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Modificadores de Acesso e Visibilidade
            ["AMOSTRADINHO"] = "public",
            ["NA_MIUDA"] = "private",
            ["SO_OS_DE_VERDADE"] = "protected",
            ["SO_ENTRE_NOS"] = "internal",

            // Modificadores de Estrutura e Comportamento
            ["SEMPRE_FOI_ASSIM"] = "static",
            ["SO_NA_TEORIA"] = "abstract",
            ["NAO_MEXE"] = "const",
            ["SO_OLHA_NAO_TOCA"] = "readonly",
            ["LACRADO"] = "sealed",
            ["FICA_A_VONTADE"] = "virtual",
            ["ASSUME_A_RESPONSA"] = "override",

            // Declaração de Tipos e Organização
            ["TROPA"] = "class",
            ["PAPO_RETO"] = "interface",
            ["PRINT"] = "record",
            ["CARDAPIO"] = "enum",
            ["MINI_TROPA"] = "struct",
            ["QUEBRADA"] = "namespace",
            ["CHAMA"] = "using",

            // Criação e Referência
            ["BROTOU"] = "new",
            ["EU_MESMO"] = "this",
            ["MEU_VELHO"] = "base",

            // Tipos Primitivos e Inferência
            ["SEI_LA"] = "var",
            ["VAI_NA_FE"] = "dynamic",
            ["PAPO"] = "string",
            ["LETRA"] = "char",
            ["NUMERO"] = "int",
            ["NUMERO_BRUTO"] = "long",
            ["NUMERO_QUEBRADO"] = "double",
            ["GRANA"] = "decimal",
            ["CONFERE"] = "bool",
            ["QUALQUER_COISA"] = "object",
            ["VOLTA_NADA"] = "void",

            // Literais Booleanos e Nulo
            ["CONFIA"] = "true",
            ["CONFIA_NAO"] = "false",
            ["E_MENTIRA"] = "false",
            ["TEM_NADA_AI"] = "null",

            // Controle de Fluxo Condicional
            ["TA_CERTO_ISSO"] = "if",
            ["NAO_TA_NAO"] = "else",
            ["QUAL_E_A_BOA"] = "switch",
            ["SE_FOR"] = "case",
            ["DE_PRAXE"] = "default",

            // Laços de Repetição
            ["ENQUANTO_TANKAR"] = "while",
            ["FAZ_PRIMEIRO"] = "do",
            ["BORA_BILL"] = "for",
            ["PRA_CADA_UM"] = "foreach",
            ["DENTRO_DE"] = "in",
            ["CHEGA"] = "break",
            ["SEGUE_O_JOGO"] = "continue",

            // Retorno e Fluxo
            ["TOMA"] = "return",
            ["SOLTA_UM"] = "yield return",

            // Tratamento de Exceções
            ["VAI_DAR_BOM"] = "try",
            ["DEU_RUIM"] = "catch",
            ["METEU_ESSA"] = "catch",
            ["DE_QUALQUER_JEITO"] = "finally",
            ["AI_TU_ME_QUEBRA"] = "throw",

            // Concorrência e Assincronia
            ["JA_VAI"] = "async",
            ["PERAI"] = "await",
            ["SEGURA_A_ONDA"] = "lock",

            // Checagens e Conversões
            ["E_MESMO"] = "is",
            ["DISFARCA_COMO"] = "as",
            ["QUAL_O_NOME"] = "nameof",
            ["QUAL_E_A_DELE"] = "typeof",

            // Atribuição e Operações I/O
            ["RECEBA"] = "=",
            ["MANDA_AI"] = "Console.WriteLine",
            ["SOLTA_AI"] = "Console.Write",
            ["FALA_TU"] = "Console.ReadLine",
        };

    public static bool IsKeyword(string text)
    {
        var normalized = Normalize(text);
        return ToCSharp.ContainsKey(normalized);
    }

    public static string? GetCSharpEquivalent(string text)
    {
        var normalized = Normalize(text);
        return ToCSharp.TryGetValue(normalized, out var equivalent) ? equivalent : null;
    }
}
