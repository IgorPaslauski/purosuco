# PuroSuco Language Server

O `PuroSuco.LanguageServer` usa LSP via stdio.

## Recursos atuais

- sincronização de documentos `.suco`
- autocomplete de palavras-chave
- hover
- diagnósticos em tempo real

## Diagnósticos

### PS003 — QUE PAPINHO É ESSE?

Detecta atribuição literal de `PAPO` em `NUMERO`.

```suco
NUMERO idade RECEBA "vinte";
```

### PS017 — METEU ESSA?

Detecta atribuição em variável ainda não declarada.

```suco
idade RECEBA 20;
```

### PS021 — AÍ TU ME QUEBRA

Detecta divisão literal por zero.

```suco
NUMERO resultado RECEBA 10 / 0;
```

## Próximos passos

- parser/AST completo para diagnósticos sem regex
- go-to-definition
- rename
- document symbols
- semantic tokens
- formatter
- code actions
- inlay hints
