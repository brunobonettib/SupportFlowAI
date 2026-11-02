# Avaliação SupportFlow AI

Início (UTC): 2026-11-02T15:40:51.8641736+00:00 | Provedor: Ollama | Modelo configurado: `gemma3:1b` | Prompt: 1.3
Status: **executado**.
Dataset: 15 mensagens fictícias; SHA-256: `3e7ea281f89ac8a8c69956d8e521fe70fd49261dba08ed77a2c086fa2d329414`.
Conjunto: original. Aquecimento separado solicitado: True. Origem do texto público: formulações controladas pelo backend; rascunhos separados.
Esforço: não aplicável; temperatura 0, seed 42; limite: 1000 tokens; timeout: 120s; sem retry.

| Métrica | Resultado |
|---|---|
| Chamadas executadas | 15 |
| Falhas | 0 |
| Acurácia de categoria | 93.33% (14/15) |
| Acurácia de prioridade | 60.00% (9/15) |
| Respostas válidas | 100.00% (15/15) |
| Recall de Alta | 80.00% |
| Latência média, todas as tentativas | 663.8 ms |

| Caso | Categoria esperada → obtida | Prioridade esperada → obtida | Resultado | ms |
|---|---|---|---|---|
| E01 | Entrega → Entrega | Media → Media | correto | 593.2 |
| E02 | Entrega → Entrega | Alta → Alta | correto | 791.3 |
| E03 | Entrega → Entrega | Baixa → Media | divergência | 544.7 |
| P01 | Pagamento → Pagamento | Media → Media | correto | 745.7 |
| P02 | Pagamento → Pagamento | Alta → Media | divergência | 615.1 |
| P03 | Pagamento → Pagamento | Baixa → Baixa | correto | 639.4 |
| C01 | Cancelamento → Cancelamento | Media → Media | correto | 629.6 |
| C02 | Cancelamento → Cancelamento | Alta → Alta | correto | 689.1 |
| C03 | Cancelamento → Cancelamento | Baixa → Media | divergência | 605.7 |
| R01 | Reembolso → Reembolso | Media → Alta | divergência | 607.1 |
| R02 | Reembolso → Reembolso | Alta → Alta | correto | 636.1 |
| R03 | Reembolso → Reembolso | Baixa → Media | divergência | 713.1 |
| O01 | Outro → Pagamento | Media → Media | divergência | 573.4 |
| O02 | Outro → Outro | Alta → Alta | correto | 805.5 |
| O03 | Outro → Outro | Baixa → Alta | divergência | 768.1 |

### Categoria: precisão/recall por classe
| Classe | Suporte | Predições | Precisão | Recall |
|---|---|---|---|---|
| Entrega | 3 | 3 | 100.00% | 100.00% |
| Pagamento | 3 | 4 | 75.00% | 100.00% |
| Cancelamento | 3 | 3 | 100.00% | 100.00% |
| Reembolso | 3 | 3 | 100.00% | 100.00% |
| Outro | 3 | 2 | 100.00% | 66.67% |

Matriz: linhas esperadas, colunas obtidas.
| Esperada | Entrega | Pagamento | Cancelamento | Reembolso | Outro | SemResposta |
|---|---|---|---|---|---|---|
| Entrega | 3 | 0 | 0 | 0 | 0 | 0 |
| Pagamento | 0 | 3 | 0 | 0 | 0 | 0 |
| Cancelamento | 0 | 0 | 3 | 0 | 0 | 0 |
| Reembolso | 0 | 0 | 0 | 3 | 0 | 0 |
| Outro | 0 | 1 | 0 | 0 | 2 | 0 |

### Prioridade: precisão/recall por classe
| Classe | Suporte | Predições | Precisão | Recall |
|---|---|---|---|---|
| Baixa | 5 | 1 | 100.00% | 20.00% |
| Media | 5 | 8 | 50.00% | 80.00% |
| Alta | 5 | 6 | 66.67% | 80.00% |

Matriz: linhas esperadas, colunas obtidas.
| Esperada | Baixa | Media | Alta | SemResposta |
|---|---|---|---|---|
| Baixa | 1 | 3 | 1 | 0 |
| Media | 0 | 4 | 1 | 0 |
| Alta | 0 | 1 | 4 | 0 |

- 15 exemplos sintéticos preparados com apoio do Codex; não representam tráfego real nem rotulagem independente.
- Latência mede o serviço de análise (rede, provedor e validação); não mede o endpoint HTTP nem trabalho humano.
- Falhas contam no denominador das acurácias. Validade estrutural não garante correção semântica.
- Sugestões precisam de revisão humana. Economia de tempo, custo real e impacto operacional não foram medidos.
- Modelo configurado pode ser um alias mutável; resultados podem variar entre execuções.

O JSON irmão separa texto entregue e rascunho do modelo para revisão dos dados fictícios; não contém chave nem corpos brutos de erro. Invenções exigem revisão contextual, não são detectadas por estas métricas de classificação.
Reproduzir: `scripts\Avaliar-Local.cmd original` (Ollama local; sem cobrança de API).
