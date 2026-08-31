namespace PuroSuco.Core;

public static class Keywords
{
    public static readonly IReadOnlyDictionary<string, string> ToCSharp =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["AMOSTRADINHO"] = "public",
            ["NA_MIÚDA"] = "private",
            ["NA_MIUDA"] = "private",
            ["SO_OS_DE_VERDADE"] = "protected",
            ["TROPA"] = "class",
            ["PAPO_RETO"] = "interface",
            ["SO_NA_TEORIA"] = "abstract",
            ["SEMPRE_FOI_ASSIM"] = "static",
            ["NAO_MEXE"] = "const",
            ["SEI_LA"] = "var",
            ["BROTOU"] = "new",
            ["EU_MESMO"] = "this",
            ["TOMA"] = "return",
            ["VOLTA_NADA"] = "void",
            ["CONFIA"] = "true",
            ["CONFIA_NAO"] = "false",
            ["TEM_NADA_AI"] = "null",
            ["TA_CERTO_ISSO"] = "if",
            ["NAO_TA_NAO"] = "else",
            ["ENQUANTO_TANKAR"] = "while",
            ["BORA_BILL"] = "for",
            ["CHEGA"] = "break",
            ["SEGUE_O_JOGO"] = "continue",
            ["VAI_DAR_BOM"] = "try",
            ["METEU_ESSA"] = "catch",
            ["DE_QUALQUER_JEITO"] = "finally",
            ["AI_TU_ME_QUEBRA"] = "throw",
            ["JA_VAI"] = "async",
            ["PERAI"] = "await",
            ["PAPO"] = "string",
            ["NUMERO"] = "int",
            ["CONFERE"] = "bool",
            ["NUMERO_QUEBRADO"] = "double",
            ["RECEBA"] = "=",
        };

    public static bool IsKeyword(string text) =>
        ToCSharp.ContainsKey(text) || text.Equals("MANDA_AI", StringComparison.OrdinalIgnoreCase);
}
