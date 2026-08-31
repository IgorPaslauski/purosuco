namespace PuroSuco.LanguageServer;

public static class MemeDictionary
{
    public static readonly IReadOnlyDictionary<string, (string Equivalent, string Description)> Keywords =
        new Dictionary<string, (string, string)>(StringComparer.OrdinalIgnoreCase)
        {
            ["AMOSTRADINHO"] = ("public", "Visível pra geral."),
            ["NA_MIÚDA"] = ("private", "Fica na miúda, só dentro da tropa."),
            ["NA_MIUDA"] = ("private", "Fica na miúda, só dentro da tropa."),
            ["SO_OS_DE_VERDADE"] = ("protected", "Só quem é da família ou herda pode acessar."),
            ["TROPA"] = ("class", "Agrupa estado e comportamento."),
            ["PAPO_RETO"] = ("interface", "É o contrato. Sem caô."),
            ["SO_NA_TEORIA"] = ("abstract", "Existe na ideia, não instancia direto."),
            ["SEMPRE_FOI_ASSIM"] = ("static", "Pertence à tropa, não ao indivíduo."),
            ["NAO_MEXE"] = ("const", "Se mexer, estraga."),
            ["SEI_LA"] = ("var", "Deixa o compilador descobrir o tipo."),
            ["BROTOU"] = ("new", "Cria uma nova instância."),
            ["EU_MESMO"] = ("this", "Referência ao próprio objeto."),
            ["TOMA"] = ("return", "Devolve um valor."),
            ["VOLTA_NADA"] = ("void", "Não devolve nada."),
            ["CONFIA"] = ("true", "Verdadeiro."),
            ["CONFIA_NAO"] = ("false", "Falso."),
            ["TEM_NADA_AI"] = ("null", "Ausência de valor."),
            ["TA_CERTO_ISSO"] = ("if", "Testa uma condição."),
            ["NAO_TA_NAO"] = ("else", "Caminho alternativo."),
            ["ENQUANTO_TANKAR"] = ("while", "Repete enquanto a condição aguentar."),
            ["BORA_BILL"] = ("for", "Loop contado."),
            ["CHEGA"] = ("break", "Para o loop."),
            ["SEGUE_O_JOGO"] = ("continue", "Vai para a próxima iteração."),
            ["VAI_DAR_BOM"] = ("try", "Tenta executar."),
            ["METEU_ESSA"] = ("catch", "Captura quando deu ruim."),
            ["DE_QUALQUER_JEITO"] = ("finally", "Executa mesmo se deu ruim."),
            ["AI_TU_ME_QUEBRA"] = ("throw", "Lança uma exceção."),
            ["JA_VAI"] = ("async", "Operação assíncrona."),
            ["PERAI"] = ("await", "Espera a operação assíncrona."),
            ["PAPO"] = ("string", "Texto."),
            ["NUMERO"] = ("int", "Número inteiro."),
            ["NUMERO_QUEBRADO"] = ("double", "Número decimal."),
            ["CONFERE"] = ("bool", "Booleano."),
            ["RECEBA"] = ("=", "Atribui um valor."),
            ["MANDA_AI"] = ("Console.WriteLine", "Joga a informação no console."),
            ["FALA_TU"] = ("Console.ReadLine", "Lê uma linha de texto digitada pelo usuário.")
        };
}
