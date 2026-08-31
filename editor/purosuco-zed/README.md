# PuroSuco para Zed

Suporte à linguagem PuroSuco no editor [Zed](https://zed.dev).

## Como usar no Zed

### 1. Configuração Automática do Workspace
Este repositório já inclui o arquivo `.zed/settings.json` na raiz do projeto. Ao abrir o workspace no Zed, o Language Server do PuroSuco (`src/PuroSuco.LanguageServer`) será conectado automaticamente aos arquivos `.suco`.

### 2. Instalar a Extensão em Modo Dev no Zed
1. No Zed, abra a paleta de comandos (`Ctrl+Shift+P` ou `Cmd+Shift+P`).
2. Digite e selecione: `zed: install dev extension`.
3. Selecione a pasta deste diretório: `editor/purosuco-zed`.

### 3. Configuração Global (Opcional)
Se desejar que o suporte a `.suco` e o LSP funcionem em qualquer projeto no Zed, adicione o seguinte ao seu `~/.config/zed/settings.json` (ou `%APPDATA%\Zed\settings.json` no Windows):

```json
{
  "lsp": {
    "purosuco-lsp": {
      "binary": {
        "path": "dotnet",
        "arguments": ["run", "--project", "<CAMINHO_COMPLETO>/src/PuroSuco.LanguageServer/PuroSuco.LanguageServer.csproj"]
      }
    }
  },
  "languages": {
    "PuroSuco": {
      "language_servers": ["purosuco-lsp", "..."],
      "tab_size": 4
    }
  },
  "file_types": {
    "PuroSuco": ["suco"]
  }
}
```

O Language Server fornece autocomplete, hover, diagnósticos em tempo real, go-to-definition, referências e formatação.
