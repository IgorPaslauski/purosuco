; PuroSuco Tree-sitter Highlight Queries

(modifier) @keyword.modifier
"TROPA" @keyword

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

(type) @type

(boolean) @constant.builtin
(null_literal) @constant.builtin

[
  "MANDA_AI"
  "FALA_TU"
  "BROTOU"
] @function.builtin

(function_declaration
  name: (identifier) @function)

(call_expression
  (identifier) @function)

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

(comment) @comment
(string) @string
(number) @number
(identifier) @variable

