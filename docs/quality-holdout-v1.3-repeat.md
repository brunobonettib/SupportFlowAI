# Avaliação SupportFlow AI

Início (UTC): 2026-11-02T15:44:04.5106030+00:00 | Provedor: Ollama | Modelo configurado: `gemma3:1b` | Prompt: 1.3
Status: **executado**.
Dataset: 15 mensagens fictícias; SHA-256: `e763d38861ccad2b9bfbb9c42fa76627a1d3730229d5080e5abbf781edafe850`.
Conjunto: holdout. Aquecimento separado solicitado: True. Origem do texto público: formulações controladas pelo backend; rascunhos separados.
Esforço: não aplicável; temperatura 0, seed 42; limite: 1000 tokens; timeout: 120s; sem retry.

| Métrica | Resultado |
|---|---|
| Chamadas executadas | 15 |
| Falhas | 1 |
| Acurácia de categoria | 46.67% (7/15) |
| Acurácia de prioridade | 40.00% (6/15) |
| Respostas válidas | 93.33% (14/15) |
| Recall de Alta | 60.00% |
| Latência média, todas as tentativas | 1012.6 ms |

| Caso | Categoria esperada → obtida | Prioridade esperada → obtida | Resultado | ms |
|---|---|---|---|---|
| H01 | Pagamento → Pagamento | Alta → Alta | correto | 723.9 |
| H02 | Pagamento → Outro | Alta → Media | divergência | 689.1 |
| H03 | Entrega → Entrega | Alta → Media | divergência | 649.1 |
| H04 | Cancelamento → Outro | Alta → Alta | divergência | 605.3 |
| H05 | Outro → Outro | Alta → Alta | correto | 649.2 |
| H06 | Pagamento → Pagamento | Media → Media | correto | 682.7 |
| H07 | Reembolso → Reembolso | Media → Alta | divergência | 744.0 |
| H08 | Reembolso → Reembolso | Media → Alta | divergência | 526.6 |
| H09 | Outro → Pagamento | Media → Baixa | divergência | 710.2 |
| H10 | Cancelamento → Outro | Media → Baixa | divergência | 508.7 |
| H11 | Reembolso → Outro | Baixa → Media | divergência | 734.7 |
| H12 | Pagamento → Outro | Baixa → Baixa | divergência | 708.7 |
| H13 | Outro → Outro | Baixa → Media | divergência | 626.2 |
| H14 | Outro → — | Baixa → — | invalid_provider_response | 6008.5 |
| H15 | Entrega → Outro | Baixa → Baixa | divergência | 622.7 |

### Categoria: precisão/recall por classe
| Classe | Suporte | Predições | Precisão | Recall |
|---|---|---|---|---|
| Entrega | 2 | 1 | 100.00% | 50.00% |
| Pagamento | 4 | 3 | 66.67% | 50.00% |
| Cancelamento | 2 | 0 | não medida | 0.00% |
| Reembolso | 3 | 2 | 100.00% | 66.67% |
| Outro | 4 | 8 | 25.00% | 50.00% |

Matriz: linhas esperadas, colunas obtidas.
| Esperada | Entrega | Pagamento | Cancelamento | Reembolso | Outro | SemResposta |
|---|---|---|---|---|---|---|
| Entrega | 1 | 0 | 0 | 0 | 1 | 0 |
| Pagamento | 0 | 2 | 0 | 0 | 2 | 0 |
| Cancelamento | 0 | 0 | 0 | 0 | 2 | 0 |
| Reembolso | 0 | 0 | 0 | 2 | 1 | 0 |
| Outro | 0 | 1 | 0 | 0 | 2 | 1 |

### Prioridade: precisão/recall por classe
| Classe | Suporte | Predições | Precisão | Recall |
|---|---|---|---|---|
| Baixa | 5 | 4 | 50.00% | 40.00% |
| Media | 5 | 5 | 20.00% | 20.00% |
| Alta | 5 | 5 | 60.00% | 60.00% |

Matriz: linhas esperadas, colunas obtidas.
| Esperada | Baixa | Media | Alta | SemResposta |
|---|---|---|---|---|
| Baixa | 2 | 2 | 0 | 1 |
| Media | 2 | 1 | 2 | 0 |
| Alta | 0 | 2 | 3 | 0 |

- 15 exemplos sintéticos preparados com apoio do Codex; não representam tráfego real nem rotulagem independente.
- Latência mede o serviço de análise (rede, provedor e validação); não mede o endpoint HTTP nem trabalho humano.
- Falhas contam no denominador das acurácias. Validade estrutural não garante correção semântica.
- Sugestões precisam de revisão humana. Economia de tempo, custo real e impacto operacional não foram medidos.
- Modelo configurado pode ser um alias mutável; resultados podem variar entre execuções.

O JSON irmão separa texto entregue e rascunho do modelo para revisão dos dados fictícios; não contém chave nem corpos brutos de erro. Invenções exigem revisão contextual, não são detectadas por estas métricas de classificação.
Reproduzir: `scripts\Avaliar-Local.cmd holdout` (Ollama local; sem cobrança de API).
