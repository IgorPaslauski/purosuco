# PuroSuco para Zed

Suporte à linguagem PuroSuco no editor [Zed](https://zed.dev).

## Como Gerar os Executáveis

Para compilar os executáveis nativos (CLI e Language Server) de alta performance:

```powershell
.\build-executables.ps1
```
Ou no CMD:
```cmd
build-executables.bat
```

Isso gera os arquivos prontos na pasta `dist/`:
- `dist/purosuco.exe` (CLI do compilador / executor)
- `dist/purosuco-lsp.exe` (Language Server Protocol)

---

## Como usar no Zed

### 1. Configuração Automática do Workspace
Este repositório já inclui os arquivos `.zed/settings.json` e `.zed/tasks.json` na raiz do projeto. 
Ao abrir este workspace no Zed:
- O Language Server (`dist/purosuco-lsp.exe`) inicializa de forma instantânea em arquivos `.suco`.
- As Tasks do Zed (`Ctrl+Shift+P` -> `task: spawn`) executam comandos como `roda`, `traduz`, `check` e `formata` usando diretamente o executável `dist/purosuco.exe`.

### 2. Instalar a Extensão em Modo Dev no Zed
1. No Zed, abra a paleta de comandos (`Ctrl+Shift+P` ou `Cmd+Shift+P`).
2. Digite e selecione: `zed: install dev extension`.
3. Selecione a pasta deste diretório: `editor/purosuco-zed`.

### 3. Configuração Global (Opcional)
Se desejar que o suporte a `.suco` e o LSP funcionem em qualquer projeto no Zed fora deste workspace, adicione o seguinte ao seu `~/.config/zed/settings.json` (ou `%APPDATA%\Zed\settings.json` no Windows):

```json
{
  "lsp": {
    "purosuco-lsp": {
      "binary": {
        "path": "<CAMINHO_COMPLETO>/dist/purosuco-lsp.exe"
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

O Language Server fornece autocomplete, hover com memes, diagnósticos em tempo real, go-to-definition, referências e formatação.
