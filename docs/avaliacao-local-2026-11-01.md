# Avaliação SupportFlow AI

Início (UTC): 2026-11-01T23:29:03.3082260+00:00 | Provedor: Ollama | Modelo configurado: `gemma3:1b` | Prompt: 1.1
Status: **executado**.
Dataset: 15 mensagens fictícias; SHA-256: `3e7ea281f89ac8a8c69956d8e521fe70fd49261dba08ed77a2c086fa2d329414`.
Esforço: não aplicável; temperatura 0, seed 42; limite: 1000 tokens; timeout: 120s; sem retry.

| Métrica | Resultado |
|---|---|
| Chamadas executadas | 15 |
| Falhas | 0 |
| Acurácia de categoria | 80.00% (12/15) |
| Acurácia de prioridade | 40.00% (6/15) |
| Respostas válidas | 100.00% (15/15) |
| Latência média, todas as tentativas | 938.3 ms |

| Caso | Categoria esperada → obtida | Prioridade esperada → obtida | Resultado | ms |
|---|---|---|---|---|
| E01 | Entrega → Entrega | Media → Media | correto | 776.7 |
| E02 | Entrega → Entrega | Alta → Media | divergência | 852.7 |
| E03 | Entrega → Entrega | Baixa → Media | divergência | 1119.2 |
| P01 | Pagamento → Pagamento | Media → Media | correto | 927.0 |
| P02 | Pagamento → Pagamento | Alta → Media | divergência | 973.5 |
| P03 | Pagamento → Pagamento | Baixa → Media | divergência | 994.4 |
| C01 | Cancelamento → Cancelamento | Media → Baixa | divergência | 991.1 |
| C02 | Cancelamento → Cancelamento | Alta → Alta | correto | 901.9 |
| C03 | Cancelamento → Cancelamento | Baixa → Media | divergência | 907.7 |
| R01 | Reembolso → Reembolso | Media → Media | correto | 896.7 |
| R02 | Reembolso → Outro | Alta → Media | divergência | 1073.2 |
| R03 | Reembolso → Outro | Baixa → Baixa | divergência | 806.6 |
| O01 | Outro → Entrega | Media → Media | divergência | 1029.8 |
| O02 | Outro → Outro | Alta → Media | divergência | 916.0 |
| O03 | Outro → Outro | Baixa → Media | divergência | 908.5 |

- 15 exemplos sintéticos, rotulados pelo autor; não representam tráfego real nem validação independente.
- Latência mede o serviço de análise (rede, provedor e validação); não mede o endpoint HTTP nem trabalho humano.
- Falhas contam no denominador das acurácias. Validade estrutural não garante correção semântica.
- Sugestões precisam de revisão humana. Economia de tempo, custo real e impacto operacional não foram medidos.
- Modelo configurado pode ser um alias mutável; resultados podem variar entre execuções.

O JSON irmão preserva as saídas válidas para revisão humana; não contém chave nem respostas brutas de erro.
Reproduzir: `scripts\Avaliar-Local.cmd` (Ollama local; sem cobrança de API).
