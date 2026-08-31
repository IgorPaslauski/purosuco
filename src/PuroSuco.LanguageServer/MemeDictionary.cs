using PuroSuco.Core;

namespace PuroSuco.LanguageServer;

public static class MemeDictionary
{
    public static readonly IReadOnlyDictionary<string, (string Equivalent, string Description)> Keywords =
        new Dictionary<string, (string, string)>(StringComparer.OrdinalIgnoreCase)
        {
            // Modificadores de Acesso e Visibilidade
            ["AMOSTRADINHO"] = ("public", "Visível pra geral."),
            ["NA_MIUDA"] = ("private", "Fica na miúda, só dentro da tropa."),
            ["SO_OS_DE_VERDADE"] = ("protected", "Só quem é da família ou herda pode acessar."),
            ["SO_ENTRE_NOS"] = ("internal", "Acesso restrito ao mesmo assembly/quebrada."),

            // Modificadores de Estrutura e Comportamento
            ["SEMPRE_FOI_ASSIM"] = ("static", "Pertence à tropa, não ao indivíduo."),
            ["SO_NA_TEORIA"] = ("abstract", "Existe na ideia, não instancia direto."),
            ["NAO_MEXE"] = ("const", "Se mexer, estraga."),
            ["SO_OLHA_NAO_TOCA"] = ("readonly", "Não pode ser alterado após a inicialização."),
            ["LACRADO"] = ("sealed", "Classe ou método fechado para herança."),
            ["FICA_A_VONTADE"] = ("virtual", "Permite que subclasses sobrescrevam."),
            ["ASSUME_A_RESPONSA"] = ("override", "Sobrescreve a implementação da classe base."),

            // Declaração de Tipos e Organização
            ["TROPA"] = ("class", "Agrupa estado e comportamento."),
            ["PAPO_RETO"] = ("interface", "É o contrato. Sem caô."),
            ["PRINT"] = ("record", "Estrutura de dados imutável com igualdade por valor."),
            ["CARDAPIO"] = ("enum", "Lista fixa de opções nomeadas."),
            ["MINI_TROPA"] = ("struct", "Tipo por valor compacto e leve."),
            ["QUEBRADA"] = ("namespace", "Agrupamento lógico de escopo."),
            ["CHAMA"] = ("using", "Importa recursos e bibliotecas."),

            // Criação e Referência
            ["BROTOU"] = ("new", "Cria uma nova instância."),
            ["EU_MESMO"] = ("this", "Referência ao próprio objeto."),
            ["MEU_VELHO"] = ("base", "Referência à classe pai / base."),

            // Tipos Primitivos e Inferência
            ["SEI_LA"] = ("var", "Deixa o compilador descobrir o tipo."),
            ["VAI_NA_FE"] = ("dynamic", "Tipo dinâmico em tempo de execução."),
            ["PAPO"] = ("string", "Texto."),
            ["LETRA"] = ("char", "Caractere único."),
            ["NUMERO"] = ("int", "Número inteiro."),
            ["NUMERO_BRUTO"] = ("long", "Inteiro longo de 64 bits."),
            ["NUMERO_QUEBRADO"] = ("double", "Número decimal de precisão dupla."),
            ["GRANA"] = ("decimal", "Número financeiro de alta precisão."),
            ["CONFERE"] = ("bool", "Booleano."),
            ["QUALQUER_COISA"] = ("object", "Tipo base universal."),
            ["VOLTA_NADA"] = ("void", "Não devolve nada."),

            // Literais Booleanos e Nulo
            ["CONFIA"] = ("true", "Verdadeiro."),
            ["CONFIA_NAO"] = ("false", "Falso."),
            ["E_MENTIRA"] = ("false", "Falso (meme 'É mentira da barata')."),
            ["TEM_NADA_AI"] = ("null", "Ausência de valor."),

            // Controle de Fluxo Condicional
            ["TA_CERTO_ISSO"] = ("if", "Testa uma condição."),
            ["NAO_TA_NAO"] = ("else", "Caminho alternativo."),
            ["QUAL_E_A_BOA"] = ("switch", "Seleção de múltiplos caminhos."),
            ["SE_FOR"] = ("case", "Caso específico do switch."),
            ["DE_PRAXE"] = ("default", "Caso padrão quando nenhum bateu."),

            // Laços de Repetição
            ["ENQUANTO_TANKAR"] = ("while", "Repete enquanto a condição aguentar."),
            ["FAZ_PRIMEIRO"] = ("do", "Executa antes de verificar a condição."),
            ["BORA_BILL"] = ("for", "Loop contado."),
            ["PRA_CADA_UM"] = ("foreach", "Itera sobre cada elemento de uma coleção."),
            ["DENTRO_DE"] = ("in", "Indica a coleção a ser percorrida."),
            ["CHEGA"] = ("break", "Para o loop."),
            ["SEGUE_O_JOGO"] = ("continue", "Vai para a próxima iteração."),

            // Retorno e Fluxo
            ["TOMA"] = ("return", "Devolve um valor."),
            ["SOLTA_UM"] = ("yield return", "Emite um elemento do iterador."),

            // Tratamento de Exceções
            ["VAI_DAR_BOM"] = ("try", "Tenta executar."),
            ["DEU_RUIM"] = ("catch", "Captura quando deu ruim."),
            ["METEU_ESSA"] = ("catch", "Captura exceção inesperada."),
            ["DE_QUALQUER_JEITO"] = ("finally", "Executa mesmo se deu ruim."),
            ["AI_TU_ME_QUEBRA"] = ("throw", "Lança uma exceção."),

            // Concorrência e Assincronia
            ["JA_VAI"] = ("async", "Operação assíncrona."),
            ["PERAI"] = ("await", "Espera a operação assíncrona."),
            ["SEGURA_A_ONDA"] = ("lock", "Bloqueio para sincronização de threads."),

            // Checagens e Conversões
            ["E_MESMO"] = ("is", "Checagem de tipo ou padrão."),
            ["DISFARCA_COMO"] = ("as", "Conversão de tipo segura."),
            ["QUAL_O_NOME"] = ("nameof", "Obtém o nome do identificador como string."),
            ["QUAL_E_A_DELE"] = ("typeof", "Obtém o System.Type do objeto."),

            // Atribuição e Operações I/O
            ["RECEBA"] = ("=", "Atribui um valor."),
            ["MANDA_AI"] = ("Console.WriteLine", "Joga a informação no console."),
            ["SOLTA_AI"] = ("Console.Write", "Escreve no console sem pular linha."),
            ["FALA_TU"] = ("Console.ReadLine", "Lê uma linha de texto digitada pelo usuário.")
        };
}
