# Avaliação SupportFlow AI

Início (UTC): 2026-11-02T15:51:54.4927820+00:00 | Provedor: Ollama | Modelo configurado: `gemma3:4b` | Prompt: 1.3
Status: **executado**.
Dataset: 15 mensagens fictícias; SHA-256: `3e7ea281f89ac8a8c69956d8e521fe70fd49261dba08ed77a2c086fa2d329414`.
Conjunto: original. Aquecimento separado solicitado: True. Origem do texto público: formulações controladas pelo backend; rascunhos separados.
Esforço: não aplicável; temperatura 0, seed 42; limite: 1000 tokens; timeout: 120s; sem retry.

| Métrica | Resultado |
|---|---|
| Chamadas executadas | 15 |
| Falhas | 0 |
| Acurácia de categoria | 100.00% (15/15) |
| Acurácia de prioridade | 73.33% (11/15) |
| Respostas válidas | 100.00% (15/15) |
| Recall de Alta | 100.00% |
| Latência média, todas as tentativas | 1724.3 ms |

| Caso | Categoria esperada → obtida | Prioridade esperada → obtida | Resultado | ms |
|---|---|---|---|---|
| E01 | Entrega → Entrega | Media → Media | correto | 1200.1 |
| E02 | Entrega → Entrega | Alta → Alta | correto | 1400.4 |
| E03 | Entrega → Entrega | Baixa → Media | divergência | 1069.4 |
| P01 | Pagamento → Pagamento | Media → Media | correto | 1493.7 |
| P02 | Pagamento → Pagamento | Alta → Alta | correto | 1407.5 |
| P03 | Pagamento → Pagamento | Baixa → Baixa | correto | 1029.4 |
| C01 | Cancelamento → Cancelamento | Media → Media | correto | 1147.6 |
| C02 | Cancelamento → Cancelamento | Alta → Alta | correto | 1251.2 |
| C03 | Cancelamento → Cancelamento | Baixa → Baixa | correto | 1329.8 |
| R01 | Reembolso → Reembolso | Media → Alta | divergência | 1264.1 |
| R02 | Reembolso → Reembolso | Alta → Alta | correto | 1386.5 |
| R03 | Reembolso → Reembolso | Baixa → Media | divergência | 1064.3 |
| O01 | Outro → Outro | Media → Baixa | divergência | 1176.5 |
| O02 | Outro → Outro | Alta → Alta | correto | 1370.8 |
| O03 | Outro → Outro | Baixa → Baixa | correto | 8272.8 |

### Categoria: precisão/recall por classe
| Classe | Suporte | Predições | Precisão | Recall |
|---|---|---|---|---|
| Entrega | 3 | 3 | 100.00% | 100.00% |
| Pagamento | 3 | 3 | 100.00% | 100.00% |
| Cancelamento | 3 | 3 | 100.00% | 100.00% |
| Reembolso | 3 | 3 | 100.00% | 100.00% |
| Outro | 3 | 3 | 100.00% | 100.00% |

Matriz: linhas esperadas, colunas obtidas.
| Esperada | Entrega | Pagamento | Cancelamento | Reembolso | Outro | SemResposta |
|---|---|---|---|---|---|---|
| Entrega | 3 | 0 | 0 | 0 | 0 | 0 |
| Pagamento | 0 | 3 | 0 | 0 | 0 | 0 |
| Cancelamento | 0 | 0 | 3 | 0 | 0 | 0 |
| Reembolso | 0 | 0 | 0 | 3 | 0 | 0 |
| Outro | 0 | 0 | 0 | 0 | 3 | 0 |

### Prioridade: precisão/recall por classe
| Classe | Suporte | Predições | Precisão | Recall |
|---|---|---|---|---|
| Baixa | 5 | 4 | 75.00% | 60.00% |
| Media | 5 | 5 | 60.00% | 60.00% |
| Alta | 5 | 6 | 83.33% | 100.00% |

Matriz: linhas esperadas, colunas obtidas.
| Esperada | Baixa | Media | Alta | SemResposta |
|---|---|---|---|---|
| Baixa | 3 | 2 | 0 | 0 |
| Media | 1 | 3 | 1 | 0 |
| Alta | 0 | 0 | 5 | 0 |

- 15 exemplos sintéticos preparados com apoio do Codex; não representam tráfego real nem rotulagem independente.
- Latência mede o serviço de análise (rede, provedor e validação); não mede o endpoint HTTP nem trabalho humano.
- Falhas contam no denominador das acurácias. Validade estrutural não garante correção semântica.
- Sugestões precisam de revisão humana. Economia de tempo, custo real e impacto operacional não foram medidos.
- Modelo configurado pode ser um alias mutável; resultados podem variar entre execuções.

O JSON irmão separa texto entregue e rascunho do modelo para revisão dos dados fictícios; não contém chave nem corpos brutos de erro. Invenções exigem revisão contextual, não são detectadas por estas métricas de classificação.
Reproduzir: `scripts\Avaliar-Local.cmd original` (Ollama local; sem cobrança de API).
