# PuroSuco Language — VS Code / Cursor

Extensão da linguagem PuroSuco com cliente LSP.

## Recursos

- `.suco`
- syntax highlighting
- snippets
- autocomplete via Language Server
- hover mostrando equivalente em C#
- diagnósticos em tempo real
- `PuroSuco: Rodar a Resenha`
- `PuroSuco: Traduzir pra C#`
- `PuroSuco: Ver Dicionário de Memes`

## Desenvolvimento local

Requisitos:

- .NET 8 SDK
- Node.js 20+
- VS Code ou Cursor

Instale as dependências:

```bash
cd editor/purosuco-vscode
npm install
```

Abra a pasta `editor/purosuco-vscode` no VS Code e rode a extensão em modo de desenvolvimento.
Para o Language Server funcionar, abra também o repositório PuroSuco como workspace ou ajuste
o caminho do servidor conforme sua instalação.

## Diagnósticos atuais

```suco
NUMERO idade RECEBA "vinte";
```

Gera:

```text
PS003 — QUE PAPINHO É ESSE?
Esperava NUMERO, recebeu PAPO. Tá certo isso? Não.
```

```suco
NUMERO resultado RECEBA 10 / 0;
```

Gera:

```text
PS021 — AÍ TU ME QUEBRA
Divisão literal por zero detectada.
```
