# Avaliação SupportFlow AI

Início (UTC): 2026-11-02T15:45:03.9871151+00:00 | Provedor: Ollama | Modelo configurado: `gemma3:1b` | Prompt: 1.1
Status: **executado**.
Dataset: 15 mensagens fictícias; SHA-256: `e763d38861ccad2b9bfbb9c42fa76627a1d3730229d5080e5abbf781edafe850`.
Conjunto: holdout. Aquecimento separado solicitado: True. Origem do texto público: formulações controladas pelo backend; rascunhos separados.
Esforço: não aplicável; temperatura 0, seed 42; limite: 1000 tokens; timeout: 120s; sem retry.

| Métrica | Resultado |
|---|---|
| Chamadas executadas | 15 |
| Falhas | 0 |
| Acurácia de categoria | 66.67% (10/15) |
| Acurácia de prioridade | 40.00% (6/15) |
| Respostas válidas | 100.00% (15/15) |
| Recall de Alta | 0.00% |
| Latência média, todas as tentativas | 935.1 ms |

| Caso | Categoria esperada → obtida | Prioridade esperada → obtida | Resultado | ms |
|---|---|---|---|---|
| H01 | Pagamento → Pagamento | Alta → Media | divergência | 1024.7 |
| H02 | Pagamento → Outro | Alta → Media | divergência | 904.9 |
| H03 | Entrega → Entrega | Alta → Media | divergência | 966.9 |
| H04 | Cancelamento → Cancelamento | Alta → Media | divergência | 905.4 |
| H05 | Outro → Outro | Alta → Media | divergência | 926.1 |
| H06 | Pagamento → Pagamento | Media → Media | correto | 986.3 |
| H07 | Reembolso → Reembolso | Media → Media | correto | 831.3 |
| H08 | Reembolso → Reembolso | Media → Media | correto | 907.4 |
| H09 | Outro → Outro | Media → Media | correto | 988.4 |
| H10 | Cancelamento → Outro | Media → Baixa | divergência | 889.1 |
| H11 | Reembolso → Outro | Baixa → Media | divergência | 1104.8 |
| H12 | Pagamento → Outro | Baixa → Baixa | divergência | 884.6 |
| H13 | Outro → Outro | Baixa → Media | divergência | 835.5 |
| H14 | Outro → Outro | Baixa → Alta | divergência | 998.0 |
| H15 | Entrega → Outro | Baixa → Baixa | divergência | 873.7 |

### Categoria: precisão/recall por classe
| Classe | Suporte | Predições | Precisão | Recall |
|---|---|---|---|---|
| Entrega | 2 | 1 | 100.00% | 50.00% |
| Pagamento | 4 | 2 | 100.00% | 50.00% |
| Cancelamento | 2 | 1 | 100.00% | 50.00% |
| Reembolso | 3 | 2 | 100.00% | 66.67% |
| Outro | 4 | 9 | 44.44% | 100.00% |

Matriz: linhas esperadas, colunas obtidas.
| Esperada | Entrega | Pagamento | Cancelamento | Reembolso | Outro | SemResposta |
|---|---|---|---|---|---|---|
| Entrega | 1 | 0 | 0 | 0 | 1 | 0 |
| Pagamento | 0 | 2 | 0 | 0 | 2 | 0 |
| Cancelamento | 0 | 0 | 1 | 0 | 1 | 0 |
| Reembolso | 0 | 0 | 0 | 2 | 1 | 0 |
| Outro | 0 | 0 | 0 | 0 | 4 | 0 |

### Prioridade: precisão/recall por classe
| Classe | Suporte | Predições | Precisão | Recall |
|---|---|---|---|---|
| Baixa | 5 | 3 | 66.67% | 40.00% |
| Media | 5 | 11 | 36.36% | 80.00% |
| Alta | 5 | 1 | 0.00% | 0.00% |

Matriz: linhas esperadas, colunas obtidas.
| Esperada | Baixa | Media | Alta | SemResposta |
|---|---|---|---|---|
| Baixa | 2 | 2 | 1 | 0 |
| Media | 1 | 4 | 0 | 0 |
| Alta | 0 | 5 | 0 | 0 |

- 15 exemplos sintéticos preparados com apoio do Codex; não representam tráfego real nem rotulagem independente.
- Latência mede o serviço de análise (rede, provedor e validação); não mede o endpoint HTTP nem trabalho humano.
- Falhas contam no denominador das acurácias. Validade estrutural não garante correção semântica.
- Sugestões precisam de revisão humana. Economia de tempo, custo real e impacto operacional não foram medidos.
- Modelo configurado pode ser um alias mutável; resultados podem variar entre execuções.

O JSON irmão separa texto entregue e rascunho do modelo para revisão dos dados fictícios; não contém chave nem corpos brutos de erro. Invenções exigem revisão contextual, não são detectadas por estas métricas de classificação.
Reproduzir: `scripts\Avaliar-Local.cmd holdout` (Ollama local; sem cobrança de API).
