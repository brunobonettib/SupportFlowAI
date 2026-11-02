# Política de triagem 1.2 — protótipo

São critérios experimentais próprios do protótipo. São escolhas explícitas para experimento com mensagens fictícias. Não há evidência operacional: relato do cliente não comprova aprovação, estorno, entrega ou ação executada pelo sistema.

## Prioridade, independente da categoria

- **Alta:** risco relatado à saúde/segurança; suspeita concreta de fraude/cobrança não reconhecida; cobrança duplicada; prejuízo financeiro com prazo de reembolso já vencido; atraso explicitamente grave (horas, prazo muito excedido); cancelamento imediato antes de uma etapa iminente. Não exige confirmar fraude ou lesão: é prioridade de revisão humana.
- **Media:** incidente ativo sem sinal crítico acima. Cartão recusado, pedido não recebido sem detalhes, cancelamento comum, reembolso solicitado/ainda não confirmado dentro do prazo, erro no aplicativo. Nem toda questão financeira é Alta.
- **Baixa:** informação geral, cenário hipotético, elogio/sugestão, sem incidente ativo. Mensagem vaga sem incidente identificável: Outro/Baixa com esclarecimento; se há incidente ativo, mas faltam detalhes, Media.

Avaliar risco antes da categoria; vários problemas usam a maior urgência realmente descrita. Negação, hipótese e contexto importam: perguntar como reconhecer fraude não relata fraude. Tom irritado não torna Alta. Não inferir gravidade só por palavra isolada. Não usar score probabilístico inventado. Todos os resultados requerem revisão humana, e o texto deve explicitar a natureza preliminar.

## Categoria

Entrega: chegada, atraso, rastreamento. Pagamento: transação, recusa, duplicidade, cobrança/fraude. Cancelamento: interromper pedido e dúvidas sobre cancelar. Reembolso: devolução de dinheiro, prazo ou acompanhamento, inclusive dúvidas gerais. Outro: cadastro/app, elogios, segurança alimentar/ferimento e sem assunto identificável.

Usar a ação principal explicitamente solicitada; em ações igualmente explícitas, Reembolso > Cancelamento > Pagamento > Entrega > Outro. Isso não reduz urgência de outro problema na mesma mensagem. Dúvida informativa mantém a categoria do tema com Baixa.

## Segurança do texto

Não há integrações autorizadas com estado de pedidos. O backend não entrega afirmações de ação/resultado oriundas de texto livre. Para o cliente local, categoria/prioridade continuam inferidas; justificativa e sugestão públicas são formulações controladas, condicionais, sem repetir dados da entrada. Isso evita a classe de promessa/ação inventada no texto entregue, mas não torna o rótulo correto.

O relatório separa rascunho do modelo e texto entregue. Ausência de invenções no texto controlado não significa que o LLM aprendeu a não alucinar. Erros de classificação e situações críticas continuam exigindo revisão prioritária de um atendente. Nenhuma resposta é enviada automaticamente.
