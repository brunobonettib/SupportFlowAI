# Teste real de integração — 31/10/2026

Uma única tentativa foi explicitamente autorizada pelo usuário e executada. Não houve repetição automática nem execução do conjunto de 15 mensagens.

## Resultado observado

- Início: 18:00:51, America/Sao_Paulo (21:00:51 UTC).
- Endpoint: `POST http://localhost:5143/api/tickets/analyze`.
- Mensagem fictícia: `Meu pedido não chegou.`
- Rótulos esperados pela política do MVP: Entrega / Media.
- HTTP recebido do backend: **503**.
- Código: **provider_rate_limited**.
- Duração da tentativa completa, incluindo HTTP local: **2100,37 ms**.
- Categoria, prioridade, justificativa e sugestão: **não retornadas**.
- Uso de tokens e custo efetivo: **não disponíveis**, registrados como `null`.

O [registro JSON sanitizado](single-request-20261031-210053.json) contém os dados observados. Não contém chave, headers de autenticação, arquivo de secrets nem corpo bruto do provedor. O valor da chave não foi lido ou mostrado pelo procedimento de teste; apenas o backend a utiliza.

## O que foi validado

A requisição alcançou o endpoint e acionou o cliente real, sem substituto de testes. Pelo código de `OpenAiTicketAnalyzer`, `provider_rate_limited` é produzido quando a OpenAI retorna HTTP 429. O endpoint transformou essa falha em Problem Details HTTP 503, com mensagem segura e traceId. Não houve análise fabricada nem retorno parcial como sucesso.

Isso valida o tratamento desse caminho de erro. Não comprova geração estruturada pelo modelo, acesso ao modelo configurado ou qualidade de classificação. A duração é de uma tentativa com falha, não latência de inferência bem-sucedida nem média de avaliação.

## Diagnóstico e limites

HTTP 429 pode indicar ausência de créditos ou limites de requisições, gastos ou uso, segundo a [documentação oficial de erros](https://developers.openai.com/api/docs/guides/error-codes). O cliente não distingue a causa específica a partir do código HTTP; não há diagnóstico confirmado de faturamento.

O cliente atual descarta o corpo de erro externo e agrupa todos os 429 no mesmo código. Essa escolha evita exposição de conteúdo, mas impede distinguir a causa específica nesta tentativa. Não foi feita nova chamada para obter detalhes. Uma futura melhoria pode extrair somente códigos conhecidos do provedor por lista permitida, sem registrar mensagens brutas.

## Próximos passos

1. O usuário confere saldo/faturamento e limites do projeto na plataforma OpenAI.
2. Após corrigir a condição, uma nova tentativa exige autorização explícita separada.
3. Antes de executar 15 mensagens, apresentar estimativa de tokens/custo com preços e premissas, e obter autorização específica. O conjunto segue não executado.

Não foram calculadas acurácias a partir deste teste isolado. Os 76 testes locais anteriores continuam sendo evidência de software; não foram executados novamente nesta etapa, que alterou apenas registros/documentação.
