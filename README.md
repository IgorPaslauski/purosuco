# 🥤 PuroSuco

**PuroSuco — a linguagem que compila o Brasil.**

Uma linguagem de programação experimental baseada em memes brasileiros.

## Exemplos

```suco
AMOSTRADINHO TROPA Programa {

    AMOSTRADINHO VOLTA_NADA Main() {

        NUMERO idade RECEBA 23;

        TA_CERTO_ISSO idade >= 18 {
            MANDA_AI("RECEBA!");
        }
        NAO_TA_NAO {
            MANDA_AI("Aí não.");
        }
    }
}
```

Equivalente aproximado em C#:

```csharp
public class Programa
{
    public void Main()
    {
        int idade = 23;

        if (idade >= 18)
        {
            Console.WriteLine("RECEBA!");
        }
        else
        {
            Console.WriteLine("Aí não.");
        }
    }
}
```

## Vocabulário inicial

| C# | PuroSuco |
|---|---|
| `public` | `AMOSTRADINHO` |
| `private` | `NA_MIÚDA` |
| `class` | `TROPA` |
| `if` | `TA_CERTO_ISSO` |
| `else` | `NAO_TA_NAO` |
| `=` | `RECEBA` |
| `true` | `CONFIA` |
| `false` | `CONFIA_NAO` |
| `null` | `TEM_NADA_AI` |
| `while` | `ENQUANTO_TANKAR` |
| `for` | `BORA_BILL` |
| `break` | `CHEGA` |
| `continue` | `SEGUE_O_JOGO` |
| `try` | `VAI_DAR_BOM` |
| `catch` | `METEU_ESSA` |
| `throw` | `AI_TU_ME_QUEBRA` |
| `return` | `TOMA` |
| `void` | `VOLTA_NADA` |
| `string` | `PAPO` |
| `int` | `NUMERO` |
| `bool` | `CONFERE` |
| `print` | `MANDA_AI` |
| `read` | `FALA_TU` |

## Rodando

Requer .NET 8.

```bash
dotnet build
dotnet run --project src/PuroSuco.Cli -- traduz examples/hello.suco
```

Isso gera `examples/hello.g.cs`.

Para inspecionar os tokens:

```bash
dotnet run --project src/PuroSuco.Cli -- tokens examples/hello.suco
```

## Roadmap

- [x] Lexer inicial
- [x] Keywords meme
- [x] Transpilação para C#
- [x] CLI
- [x] Parser + AST próprios (com loops BORA_BILL e ENQUANTO_TANKAR)
- [x] Diagnósticos semânticos
- [x] Runner
- [x] Formatter
- [x] Language Server Protocol (LSP com Semantic Tokens para Zed/VS Code)
- [x] Extensão VS Code / Cursor
- [x] Integração com Zed (.zed/settings.json e tasks.json)

## Filosofia

O meme precisa fazer sentido com a operação.

`public` virou `AMOSTRADINHO` porque está exposto para geral.

`=` virou `RECEBA` porque a variável literalmente recebe um valor.

`if` virou `TA_CERTO_ISSO` porque estamos verificando uma condição.

Esse padrão deve ser mantido ao adicionar novas palavras-chave.


## Editores

### VS Code e Cursor

A extensão está em:

```text
editor/purosuco-vscode
```

Ela oferece syntax highlighting, autocomplete básico, hover, snippets e comandos
para traduzir/rodar arquivos `.suco`.

### Zed

A base da extensão está em:

```text
editor/purosuco-zed
```

Para suporte completo, falta a grammar `tree-sitter-purosuco`. O futuro LSP será
compartilhado pelos três editores.



## Language Server

O projeto agora inclui `src/PuroSuco.LanguageServer`.

Ele fornece:

- autocomplete
- hover
- diagnósticos em tempo real
- integração via LSP

Isso permite compartilhar inteligência da linguagem entre VS Code, Cursor e, futuramente, Zed.


## Parser + AST

O PuroSuco agora possui parser e AST próprios.

Exemplo:

```suco
NUMERO idade RECEBA 23;

TA_CERTO_ISSO idade >= 18 {
    MANDA_AI("RECEBA!");
}
```

Inspecione a árvore:

```bash
dotnet run --project src/PuroSuco.Cli -- ast examples/hello-ast.suco
```

Valide semanticamente:

```bash
dotnet run --project src/PuroSuco.Cli -- check examples/semantic-errors.suco
```

O Language Server usa o mesmo parser e analisador semântico, evitando duplicar regras no editor.


## Símbolos e navegação

O PuroSuco agora suporta classes `TROPA`, funções, parâmetros e escopos.

O Language Server usa a tabela de símbolos para:

- autocomplete de variáveis/funções/classes
- hover semântico
- go-to-definition
- rename
- diagnósticos de escopo e retorno


## Experiência de editor

A camada LSP agora inclui:

- Find References
- Outline / Document Symbols
- Semantic Highlighting
- Formatter
- Code Actions / Quick Fixes

Exemplo de Quick Fix:

```suco
NUMERO idade RECEBA "23";
```

O editor pode sugerir **Arruma essa resenha: transformar PAPO em NUMERO**.
