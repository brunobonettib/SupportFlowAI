# Avaliação SupportFlow AI

Início (UTC): 2026-11-02T15:53:04.5022264+00:00 | Provedor: Ollama | Modelo configurado: `gemma3:4b` | Prompt: 1.3
Status: **executado**.
Dataset: 15 mensagens fictícias; SHA-256: `e763d38861ccad2b9bfbb9c42fa76627a1d3730229d5080e5abbf781edafe850`.
Conjunto: holdout. Aquecimento separado solicitado: True. Origem do texto público: formulações controladas pelo backend; rascunhos separados.
Esforço: não aplicável; temperatura 0, seed 42; limite: 1000 tokens; timeout: 120s; sem retry.

| Métrica | Resultado |
|---|---|
| Chamadas executadas | 15 |
| Falhas | 0 |
| Acurácia de categoria | 93.33% (14/15) |
| Acurácia de prioridade | 80.00% (12/15) |
| Respostas válidas | 100.00% (15/15) |
| Recall de Alta | 100.00% |
| Latência média, todas as tentativas | 1320.3 ms |

| Caso | Categoria esperada → obtida | Prioridade esperada → obtida | Resultado | ms |
|---|---|---|---|---|
| H01 | Pagamento → Pagamento | Alta → Alta | correto | 1324.0 |
| H02 | Pagamento → Pagamento | Alta → Alta | correto | 1574.1 |
| H03 | Entrega → Entrega | Alta → Alta | correto | 1181.1 |
| H04 | Cancelamento → Cancelamento | Alta → Alta | correto | 1265.5 |
| H05 | Outro → Outro | Alta → Alta | correto | 1317.4 |
| H06 | Pagamento → Pagamento | Media → Media | correto | 1442.9 |
| H07 | Reembolso → Reembolso | Media → Media | correto | 1305.4 |
| H08 | Reembolso → Reembolso | Media → Alta | divergência | 1400.4 |
| H09 | Outro → Outro | Media → Baixa | divergência | 1382.5 |
| H10 | Cancelamento → Cancelamento | Media → Media | correto | 1290.8 |
| H11 | Reembolso → Reembolso | Baixa → Baixa | correto | 1425.2 |
| H12 | Pagamento → Pagamento | Baixa → Baixa | correto | 1270.8 |
| H13 | Outro → Outro | Baixa → Baixa | correto | 1104.1 |
| H14 | Outro → Outro | Baixa → Alta | divergência | 1180.9 |
| H15 | Entrega → Outro | Baixa → Baixa | divergência | 1338.5 |

### Categoria: precisão/recall por classe
| Classe | Suporte | Predições | Precisão | Recall |
|---|---|---|---|---|
| Entrega | 2 | 1 | 100.00% | 50.00% |
| Pagamento | 4 | 4 | 100.00% | 100.00% |
| Cancelamento | 2 | 2 | 100.00% | 100.00% |
| Reembolso | 3 | 3 | 100.00% | 100.00% |
| Outro | 4 | 5 | 80.00% | 100.00% |

Matriz: linhas esperadas, colunas obtidas.
| Esperada | Entrega | Pagamento | Cancelamento | Reembolso | Outro | SemResposta |
|---|---|---|---|---|---|---|
| Entrega | 1 | 0 | 0 | 0 | 1 | 0 |
| Pagamento | 0 | 4 | 0 | 0 | 0 | 0 |
| Cancelamento | 0 | 0 | 2 | 0 | 0 | 0 |
| Reembolso | 0 | 0 | 0 | 3 | 0 | 0 |
| Outro | 0 | 0 | 0 | 0 | 4 | 0 |

### Prioridade: precisão/recall por classe
| Classe | Suporte | Predições | Precisão | Recall |
|---|---|---|---|---|
| Baixa | 5 | 5 | 80.00% | 80.00% |
| Media | 5 | 3 | 100.00% | 60.00% |
| Alta | 5 | 7 | 71.43% | 100.00% |

Matriz: linhas esperadas, colunas obtidas.
| Esperada | Baixa | Media | Alta | SemResposta |
|---|---|---|---|---|
| Baixa | 4 | 0 | 1 | 0 |
| Media | 1 | 3 | 1 | 0 |
| Alta | 0 | 0 | 5 | 0 |

- 15 exemplos sintéticos preparados com apoio do Codex; não representam tráfego real nem rotulagem independente.
- Latência mede o serviço de análise (rede, provedor e validação); não mede o endpoint HTTP nem trabalho humano.
- Falhas contam no denominador das acurácias. Validade estrutural não garante correção semântica.
- Sugestões precisam de revisão humana. Economia de tempo, custo real e impacto operacional não foram medidos.
- Modelo configurado pode ser um alias mutável; resultados podem variar entre execuções.

O JSON irmão separa texto entregue e rascunho do modelo para revisão dos dados fictícios; não contém chave nem corpos brutos de erro. Invenções exigem revisão contextual, não são detectadas por estas métricas de classificação.
Reproduzir: `scripts\Avaliar-Local.cmd holdout` (Ollama local; sem cobrança de API).
