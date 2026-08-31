# Recursos atuais da linguagem

## Classes

```suco
AMOSTRADINHO TROPA Programa {
}
```

## Funções

```suco
AMOSTRADINHO SEMPRE_FOI_ASSIM NUMERO Soma(NUMERO a, NUMERO b) {
    TOMA a + b;
}
```

## Escopo

Variáveis e parâmetros são registrados em escopos léxicos.

## Símbolos

O analisador mantém símbolos para:

- classes
- funções
- parâmetros
- variáveis locais

O LSP usa essa tabela para:

- autocomplete contextual
- hover
- go-to-definition
- rename
