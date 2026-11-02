# Resultados experimentais — SupportFlow AI

**02/11/2026.** Configuração adotada: **Ollama local gemma3:4b, prompt 1.3, política de texto 1.0**. Nenhuma chamada paga à OpenAI. Endpoint e cinco campos públicos preservados.

A classificação continua sendo inferência real do LLM. **Justificativa e sugestão públicas no caminho local agora são formulações controladas pelo backend**, sem afirmar ações, políticas ou prazos. Os rascunhos livres são preservados apenas nos relatórios de mensagens fictícias. Essa mudança melhora segurança de saída, com perda de personalização; não deve ser apresentada como melhora de geração livre do modelo.

## 1. Diagnóstico e causas prováveis

O [diagnóstico prévio](DIAGNOSTICO_QUALIDADE_IA.md) registra a revisão individual das 15 respostas antes de mudar comportamento. No baseline: nove prioridades erradas; quatro dos cinco casos Alta subestimados; nenhum falso Alta; quatro Baixa elevados a Media e um Media reduzido a Baixa.

O modelo gemma3:1b Q4_K_M produzia Media em 12/15 respostas. O prompt misturava instruções de categoria, urgência e redação, com poucos contrastes entre perguntas gerais e incidentes ativos. As causas prováveis são capacidade limitada do modelo e dificuldade de seguir várias restrições; não foi feito experimento causal que isole cada fator.

Os rótulos foram considerados coerentes com a política do protótipo e preservados. São escolhas experimentais, sujeitas à revisão e sem vínculo com políticas de empresas. Cadastro/app é Outro; perguntas sobre reembolso continuam Reembolso; cancelamento imediato pode ser Alta sem inventar risco de saúde.

## 2. Mudanças implementadas

- [Política de triagem](POLITICA_TRIAGEM.md): critérios separados, risco antes de urgência comum, dúvidas gerais, negação/hipótese, múltiplos problemas e revisão humana.
- Prompt 1.3 mais curto, com seis exemplos representativos diferentes das mensagens de avaliação. Mantidos papéis separados, JSON Schema, uma chamada por ticket e temperatura 0/seed 42.
- `SafeTicketText`: preserva rótulos inferidos e substitui ambos os textos livres por formulações condicionais/controladas. Não detecta alucinações por palavra-chave, não eleva prioridade por mencionar “pagamento” e não simula classificação. A justificativa descreve o critério sugerido, não uma confirmação factual do relato nem uma explicação fiel do raciocínio interno do modelo.
- `ModelDraft` tem JsonIgnore: não aparece no contrato do endpoint. O avaliador grava rascunho separadamente para auditar mensagens fictícias; logs continuam sem esse conteúdo.
- Avaliação ampliada: precisão e recall por classe, recall de Alta, matrizes com coluna SemResposta, contagem de falhas, seleção de dataset e aquecimento separado.
- Modelo 4B adotado somente após comparação real autorizada. Tags, digest e tamanho em [metadados](quality-models-2026-11-02.json). Artefato de 3.350.272.403 bytes, Q4_K_M, runtime Ollama 0.40.13, RTX 2060 de 6 GB; [estado de carregamento](quality-runtime-2026-11-02.json). [Distribuição oficial](https://ollama.com/library/gemma3:4b).

`RequerRevisaoHumana` já era true e continua assim; não foi inventado um score de confiança. Não há ferramentas ou evidência autorizada de que qualquer ação operacional ocorreu.

## 3. Experimentos e regressões preservadas

O [protocolo](PROTOCOLO_QUALIDADE.md) descreve a sequência inteira. Prompt 1.2 foi rejeitado: categoria caiu para 11/15 e nenhum Reembolso foi reconhecido. Prompt 1.3 no modelo 1B melhorou o conjunto conhecido, mas falhou no conjunto novo. Não foi adotado como configuração final nesse modelo.

Houve uma repetição involuntária da avaliação 1B/prompt 1.3 durante tentativa de restaurar 1.1: timestamp antigo fez o build incremental reutilizar o binário. O relatório detectou a versão real; o artefato foi preservado como [repetição](quality-holdout-v1.3-repeat.json). O baseline correto foi executado após build com `--no-incremental`. Não foi escolhido o melhor de vários resultados ocultos.

O prompt 1.3 ficou congelado durante a comparação com 4B. Todos os casos e rótulos originais foram preservados; hashes conferidos antes e depois. O conjunto separado foi rotulado antes de inferir, em [dataset-freeze.json](../evaluation/dataset-freeze.json).

## 4. Comparação antes/depois no conjunto original

| Métrica (15 casos) | Baseline 1B / prompt 1.1 | Final 4B / prompt 1.3 |
|---|---|---|
| Categoria | 12/15 = 80% | **15/15 = 100%** |
| Prioridade | 6/15 = 40% | **11/15 = 73,33%** |
| Recall Alta | 1/5 = 20% | **5/5 = 100%** |
| Precisão Alta | 1/1 = 100% | **5/6 = 83,33%** |
| Altas subestimadas | 4 | **0** |
| Falsos Alta | 0 | **1** |
| Validade estrutural | 15/15 | 15/15 |
| Latência média aquecida | 938,33 ms | **1724,27 ms** |
| Respostas com informação não sustentada no texto entregue | 9/15, texto livre | **0/15, texto controlado** |
| Rascunhos livres com informação não sustentada | 9/15 | **8/15** |

Recall melhorou com mais falsos positivos e latência maior: **não houve melhora em todas as dimensões**. O03 demorou 8272,79 ms no lote final e permaneceu na média. Aquecimento separado levou 18195 ms nesta execução; não comparar como benchmark de inicialização contra os 96s históricos sem controlar caches/condições.

Fontes: [baseline](avaliacao-local-2026-11-01.json), [final JSON](quality-original-4b-v1.3.json), [final legível](quality-original-4b-v1.3.md). Recálculo independente dos agregados a partir das linhas em [quality-comparison.json](quality-comparison.json).

### Precisão por classe

| Classe de prioridade | Baseline original | Final original | Final conjunto novo |
|---|---|---|---|
| Baixa | 50% (1/2) | 75% (3/4) | 80% (4/5) |
| Media | 33,33% (4/12) | 60% (3/5) | 100% (3/3) |
| Alta | 100% (1/1) | 83,33% (5/6) | 71,43% (5/7) |

Precisão de categoria (Entrega/Pagamento/Cancelamento/Reembolso/Outro): baseline **75%, 100%, 100%, 100%, 50%**; final original **100% em cada classe**; final novo **100%, 100%, 100%, 100%, 80%**. Isso não implica recall perfeito: no conjunto novo, Entrega teve recall de 1/2. Todos os suportes, recalls e matrizes de categoria estão nos relatórios integrais e no JSON comparativo. Precisão sem predições é null, não 100%.

### Matrizes de prioridade

Linhas = esperado, colunas = obtido. Nenhuma falha nos dois lotes finais; por isso SemResposta é zero aqui. Candidatos com falha mantêm essa coluna nos relatórios.

| Original baseline | Baixa | Media | Alta |
|---|---|---|---|
| Baixa | 1 | 4 | 0 |
| Media | 1 | 4 | 0 |
| Alta | 0 | 4 | 1 |

| Original final | Baixa | Media | Alta |
|---|---|---|---|
| Baixa | 3 | 2 | 0 |
| Media | 1 | 3 | 1 |
| Alta | 0 | 0 | 5 |

| Novo final | Baixa | Media | Alta |
|---|---|---|---|
| Baixa | 4 | 0 | 1 |
| Media | 1 | 3 | 1 |
| Alta | 0 | 0 | 5 |

## 5. Casos novos e generalização

[15 mensagens novas](../evaluation/holdout.json): cinco por prioridade, cobrindo duplicidade, suspeita de fraude, atraso, janela de cancelamento, ferimento, recusa, reembolso dentro do prazo, múltiplos assuntos, cadastro, dúvidas, ambiguidade e injeção. Rótulos definidos antes da primeira inferência; nenhum foi alterado.

| Configuração no conjunto novo | Categoria | Prioridade | Recall Alta | Validade | Média |
|---|---|---|---|---|---|
| 1B / prompt antigo 1.1 | 10/15 (66,67%) | 6/15 (40%) | 0/5 | 15/15 | 935,14 ms |
| 1B / candidato 1.3 | 7/15 (46,67%) | 6/15 (40%) | 3/5 | 14/15 | 1024,66 ms |
| **4B / final 1.3** | **14/15 (93,33%)** | **12/15 (80%)** | **5/5** | **15/15** | **1320,26 ms** |

No final houve dois falsos Alta (H08 e H14), um Media subestimado (H09) e erro de categoria em H15 (rastreio futuro → Outro). Falhas permanecem nos denominadores e na latência. [Relatório final novo](quality-holdout-4b-v1.3.md), [JSON completo](quality-holdout-4b-v1.3.json).

A melhora no conjunto novo dá evidência adicional, **não prova generalização em produção**. Esse conjunto acabou sendo usado para comparar/selecionar candidatos, portanto é validação, não teste cego final. Não houve novo ajuste do prompt após lê-lo, mas a escolha do modelo também introduz viés de seleção. Próximo passo: outro conjunto independente, revisores humanos e amostra maior.

## 6. Segurança textual: o que mudou de fato

Rubrica em [RUBRICA_SEGURANCA_TEXTO.md](RUBRICA_SEGURANCA_TEXTO.md), decisões por relatório/IDs em [quality-text-review.json](quality-text-review.json). Unidade de contagem: resposta (justificativa + sugestão), no máximo uma ocorrência por caso. Inclui fatos, infraestrutura, capacidades, ações e compromissos não sustentados. Hipóteses condicionais e pedidos de verificação não são automaticamente marcados.

Revisão contextual com apoio do Codex, não revisão humana independente; não existe detector automático de alucinações. Nas 15 novas respostas 4B, **5/15 rascunhos** tiveram afirmações não sustentadas; **0/15 textos entregues** tiveram esse problema segundo a rubrica. Nos originais: 8/15 rascunhos e 0/15 entregues. Zero nos textos controlados é proteção do backend, não uma taxa de alucinação zero do modelo.

Exemplos:

- **P02:** cobrança duplicada era Media e passou a Alta. **E02/R02/O02:** atraso grave, prazo vencido e ferimento também passaram de Media para Alta.
- **P03:** baseline inventou meios de pagamento aceitos. O texto público atual pede revisão sem enumerar meios aceitos.
- **C03:** baseline afirmou cancelamento/reembolso. O texto público atual explica que um atendente precisa avaliar e nega confirmação de cancelamento.
- **H08:** rascunho 4B prometeu “Processaremos ... imediatamente” e estorno; **não foi publicado**. A prioridade ainda foi elevada indevidamente a Alta.
- **O02:** rascunho 4B inferiu ingestão de vidro, não explicitada na entrada. A justificativa pública descreve critério de triagem preliminar sem repetir esse fato inventado.
- **H14:** a injeção não levou a confirmar estorno no texto público, mas recebeu Alta apesar da referência Baixa, resultado compatível com seguir a instrução maliciosa. **Prompt injection não está resolvida.**

Regressões de prioridade no original: O01 (problema de cadastro) ficou Baixa; R01 ficou Alta e R03 ficou Media. A sensibilidade a urgência melhorou, com falsos alarmes e erros não críticos remanescentes. Não recomendar ordenação autônoma de fila sem revisão.

Não foi usada correção determinística de rótulos por palavras-chave: a mesma palavra pode aparecer em negação, pergunta ou hipótese. A proteção determinística foi aplicada ao texto que o serviço controla, preservando rótulos brutos para não inflar acurácia.

## 7. Validação técnica e reprodução

- `dotnet restore`: sucesso.
- `dotnet build`: sucesso, **0 avisos, 0 erros**.
- `dotnet test`: **133 aprovados, 0 falhas, 0 ignorados**; os 114 existentes foram preservados, com 19 casos adicionais.
- Auditoria NuGet transitiva: nenhum pacote vulnerável conhecido listado. [JSON](auditoria-qualidade-2026-11-02.json). Isso não audita vulnerabilidades do runtime/modelo Ollama.
- TRX local: `TestResults/quality-final-2026-11-02.trx`.
- [Smoke test HTTP final](quality-http-2026-11-02.json): HTTP 200, Entrega/Media, cinco campos públicos, requerRevisaoHumana=true, sem modelDraft e com textos controlados; 1534,87 ms.
- Datasets e baseline original conferidos por hash e preservados. Nenhum segredo foi lido/exibido; nenhum deploy, push ou instalação global.

Testes novos verificam fronteira de texto com promessas de estorno/aprovação/entrega/ação/prazo e dados fictícios, não publicação do rascunho, preservação dos rótulos, precisão/recall com falhas e classe sem predição, dataset distinto, aquecimento fora do denominador e bloqueio de aquecimento pago. Stubs **não** medem classificação semântica: essa evidência está nas inferências reais.

```powershell
# Verificar software
.\scripts\Verificar.cmd
# Terminais separados: manter serviços ativos
.\scripts\Iniciar-Ollama.cmd
.\scripts\Iniciar-Api-Local.cmd
# Uma mensagem fictícia pelo endpoint
.\scripts\Demonstrar.cmd
# Cada lote: 1 aquecimento + 15 inferências locais
.\scripts\Avaliar-Local.cmd original
.\scripts\Avaliar-Local.cmd holdout
# Recálculo de artefatos e anotações; SEM inferência
.\scripts\Comparar-Qualidade.ps1
```

Scripts usam 4B por padrão e respeitam `Ollama__Model`. Para reproduzir 1B/prompt 1.3, defina essa variável como gemma3:1b; remova-a depois para voltar ao padrão. Nenhum fallback seleciona OpenAI.

Os servidores iniciados para esta verificação foram encerrados ao final; os modelos permanecem instalados somente em `.runtime/` para a próxima demonstração. Os links locais da documentação e os contadores do TRX final também foram conferidos.

Nesta melhoria ocorreram sete lotes de 16 chamadas locais (incluindo aquecimentos e a repetição involuntária) mais uma chamada HTTP final: **113 tentativas de inferência local**. Duas tentativas H14 do modelo 1B foram rejeitadas pelo parser; todos os lotes/erros permanecem preservados. Não houve chamadas pagas. Geração de rascunhos ainda consome tokens mesmo quando o texto é substituído; uma evolução seria usar um contrato interno menor, mas isso não foi necessário para este MVP.

## 8. Arquivos principais trabalhados

- `OpenAI/TicketPrompt.cs`; snapshots em `evaluation/baselines/`; `docs/POLITICA_TRIAGEM.md`.
- `Tickets/SafeTicketText.cs`, `Tickets/TicketContracts.cs`, `Ollama/OllamaTicketAnalyzer.cs`.
- `Ollama/OllamaOptions.cs`, `appsettings.json`, scripts de API/avaliação: padrão 4B e configuração por ambiente.
- `evaluation/ClassificationMetrics.cs`, `EvaluationMetrics.cs`, `EvaluationRunner.cs`, `holdout.json`, `dataset-freeze.json`; `Program.cs` para flags de conjunto/aquecimento.
- `tests/SupportFlowAI.Tests/QualitySafetyTests.cs`, `ClassificationMetricsTests.cs`, `EvaluationRunnerTests.cs` e projeto de testes.
- `scripts/Comparar-Qualidade.ps1`, relatórios `docs/quality-*`, diagnóstico, protocolo, rubrica, este relatório, README e avisos de atualização nos documentos históricos.

## 9. Limites e revisão humana

A API não sabe se um pedido foi entregue, um reembolso aprovado ou um pagamento estornado. A classificação é preliminar; o texto público conservador perde personalização e não constitui resposta operacional definitiva. O caminho OpenAI não foi validado ao vivo e a proteção de texto desta etapa foi aplicada ao cliente local.

Os 30 exemplos são sintéticos, poucos e preparados com apoio da mesma ferramenta que implementou o código. Não foram medidos produtividade, satisfação, custo energético ou impacto financeiro. Recall Alta 100% significa 5/5 **em cada pequeno conjunto**, não garantia universal. Latência média não representa SLA; houve outlier, cache e aquecimento. Não há métricas de confiança calibrada.

O ganho verificável é: melhor identificação de urgências nos exemplos medidos, separação entre classificação e geração insegura, avaliação mais informativa e capacidade de demonstrar erros com evidências. Atendimento autônomo, envio automático e integração operacional continuam fora do escopo.
