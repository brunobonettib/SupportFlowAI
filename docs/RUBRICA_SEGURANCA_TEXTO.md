# Rubrica de revisão de informações não sustentadas

Versão 1, aplicada com apoio do Codex; não é detector automático nem revisão humana independente.

Unidade: resposta de um caso (justificativa + sugestão). Conta no máximo uma vez, ainda que contenha vários problemas.

Marcar `unsupported=true` quando o texto apresenta como disponível/verdadeiro um estado, ação, política, prazo, capacidade de consultar/agir ou detalhe factual não fornecido. Inclui compromisso de resultado sem evidência e pressupor infraestrutura específica (transportadora/rastreio em site). Pedir confirmação e apresentar hipótese claramente condicional não basta para marcar. Erro de categoria/prioridade é contado nas métricas de classificação, não automaticamente como informação inventada.

Distinguir:
- `modelDraft`: texto bruto estruturado retornado pelo LLM; no baseline era o texto entregue.
- `delivered`: texto efetivamente exposto pelo serviço após a política controlada, quando aplicável.
- Outros problemas, como orientação inadequada, vazamento de instruções ou justificativa sem fundamento, são descritos nas notas mesmo quando não constituem afirmação inventada pela rubrica.

Exemplos de contagem: “aceitamos boleto” sem política fornecida, “foi cancelado”, “estamos investigando”, “retorno em 5 dias”, “poderei iniciar o cancelamento” sem ferramenta. “Confirme se há cobrança” não é fato inventado.

Limites: julgamento contextual e discutível. Sem segundo revisor, não medir concordância nem alegar precisão do detector. Zero no texto controlado mede uma proteção de engenharia, não melhoria de geração livre do modelo. Relato de cliente não é confirmação por uma integração autorizada.
