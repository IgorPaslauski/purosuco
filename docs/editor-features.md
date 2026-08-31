# Recursos do editor PuroSuco

O Language Server agora fornece:

- autocomplete
- hover
- diagnósticos
- go-to-definition
- rename
- find references
- outline/document symbols
- semantic highlighting
- formatter
- quick fixes

## Quick Fix

```suco
NUMERO idade RECEBA "23";
```

O editor pode oferecer:

`Arruma essa resenha: transformar PAPO em NUMERO`

Resultado:

```suco
NUMERO idade RECEBA 23;
```

## Formatter

Pelo CLI:

```bash
dotnet run --project src/PuroSuco.Cli -- formata arquivo.suco
```

No VS Code/Cursor, o LSP fornece `Document Formatting`.
