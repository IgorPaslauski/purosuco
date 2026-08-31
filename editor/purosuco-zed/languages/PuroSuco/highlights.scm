; PuroSuco Tree-sitter & Semantic Highlight Queries

; Keywords
[
  "AMOSTRADINHO"
  "NA_MIÚDA"
  "NA_MIUDA"
  "SO_OS_DE_VERDADE"
  "SO_NA_TEORIA"
  "SEMPRE_FOI_ASSIM"
  "NAO_MEXE"
  "BROTOU"
  "EU_MESMO"
] @keyword.modifier

[
  "TA_CERTO_ISSO"
  "NAO_TA_NAO"
  "ENQUANTO_TANKAR"
  "BORA_BILL"
  "CHEGA"
  "SEGUE_O_JOGO"
  "VAI_DAR_BOM"
  "METEU_ESSA"
  "DE_QUALQUER_JEITO"
  "AI_TU_ME_QUEBRA"
  "TOMA"
  "JA_VAI"
  "PERAI"
] @keyword.control

; Tipos
[
  "PAPO"
  "NUMERO"
  "NUMERO_QUEBRADO"
  "CONFERE"
  "VOLTA_NADA"
  "SEI_LA"
  "TROPA"
  "PAPO_RETO"
] @type

; Literais booleanos e nulos
[
  "CONFIA"
  "CONFIA_NAO"
  "TEM_NADA_AI"
] @constant.builtin

; Funções built-in
[
  "MANDA_AI"
  "FALA_TU"
] @function.builtin

; Atribuição e Operadores
"RECEBA" @keyword.operator
[
  "+"
  "-"
  "*"
  "/"
  "=="
  "!="
  ">="
  "<="
  ">"
  "<"
] @operator

; Comentários e strings
(comment) @comment
(string) @string
(number) @number
