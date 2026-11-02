> **Baseline histórico preservado.** A comparação com o modelo 4B e a proteção atual do texto público estão em [RESULTADOS_EXPERIMENTAIS.md](RESULTADOS_EXPERIMENTAIS.md). Esta página mantém os resultados originais do modelo 1B.

# Avaliação crítica da inferência local — 01/11/2026

A integração com Ollama executou um LLM real, sem uso da API OpenAI. O usuário autorizou explicitamente o lote de 15 mensagens. Houve uma chamada inicial pelo endpoint e depois 15 inferências pelo serviço do avaliador, sem retries. Não foram executadas novas inferências para melhorar artificialmente os números.

## Evidências e ambiente

- [Resposta inicial pelo endpoint](local-request-2026-11-01.json): HTTP 200, Entrega/Media, 96244,99 ms. Esta duração inclui a inicialização; não faz parte da média do lote.
- [Relatório integral](avaliacao-local-2026-11-01.md) e [saídas por caso em JSON](avaliacao-local-2026-11-01.json).
- Ollama **0.40.13**, modelo **gemma3:1b**, quantização **Q4_K_M**, cerca de 806 MB em disco; [digest e detalhes](ollama-model-2026-11-01.json).
- Modelo SHA-256: `97558784bdc9175075a72655bf360b15da17df6b3b9e5182af9f8c0c8e58ba7c`.
- Windows, RAM 15,9 GB, GPU NVIDIA RTX 2060 com 6 GB; execução local com CUDA, contexto 8192, temperatura 0, seed 42, limite de 1000 tokens de saída e timeout de 120 segundos.
- Prompt 1.1, hashes de prompt/schema/dataset registrados no relatório. O conjunto original de 15 mensagens não foi alterado.
- Lote executado de **23:29:03 a 23:29:17 UTC** (20:29 no horário local), após a chamada inicial aquecer o modelo.
- Recursos de nuvem desativados por `OLLAMA_NO_CLOUD=1`; bind 127.0.0.1:11434. Nenhum prompt deste lote foi enviado à OpenAI.

## Resultados observados

| Métrica | Resultado |
|---|---|
| Tentativas | 15 |
| Categoria | 12/15 = **80%** |
| Prioridade | 6/15 = **40%** |
| Validade estrutural | 15/15 = **100%** |
| Falhas de transporte/validação | 0 |
| Latência média do serviço, modelo aquecido | **938,33 ms** |

Os cálculos também foram conferidos a partir das 15 linhas do JSON por uma agregação separada no PowerShell. Validade estrutural não mede segurança textual, empatia ou precisão factual. Zero falhas de transporte não significa zero erros do modelo. A latência não inclui entrada HTTP no backend nem revisão humana. Consumo efetivo de tokens e energia não foi instrumentado; não foi inventado custo monetário.

## Erros e riscos observados na revisão das saídas

Esta revisão foi feita com apoio do Codex, não por avaliadores humanos independentes.

| Casos | Observação |
|---|---|
| R02, R03, O01 | Categorias divergentes: reembolsos viraram Outro; problema de cadastro de endereço virou Entrega. |
| E02, P02, R02, O02 | Urgências Alta foram reduzidas a Media, incluindo atraso grave, cobrança duplicada e ferimento. |
| E03, P03, C01, C03, O03 | Outras divergências de prioridade. No total, nove prioridades incorretas. |
| P03 | Inventou meios de pagamento aceitos. Não existe base de políticas para sustentar isso. |
| C02 | Categoria/prioridade corretas, mas inventou risco de saúde/fraude e sugeriu aguardar o preparo, contrário à necessidade do cliente. |
| C03 | Prometeu cancelamento/reembolso e descreveu processo em andamento sem evidência. |
| O02 | “Estamos investigando” afirma ação que o sistema não executou. A sugestão é inadequada. |
| E01, E02 e resposta inicial | Mencionou transportadora sem contexto fornecido. |
| O03 | Não devolveu categoria VIP, mas errou a prioridade. Um único caso não comprova resistência a prompt injection. |

## Conclusão técnica e limite de uso

O fluxo de integração, validação estrutural e medição funciona. **O modelo local de 1B não está adequado para priorização confiável ou envio automático de respostas.** O projeto não envia respostas; a revisão humana permanece obrigatória. O prompt contém proibições de ações e promessas, mas os resultados demonstram que essas instruções não bastam para garantir obediência semântica.

Não foi adicionado um filtro por palavras para alegar uma garantia que não existiria. O parser faz validação estrutural; não certifica verdade ou segurança do conteúdo. A demonstração deve mostrar tanto o funcionamento quanto esses limites.

Próximo experimento proposto, ainda não executado: comparar um modelo local mais capaz e/ou um prompt revisado, mantendo este resultado como baseline, com casos novos e avaliação independente das sugestões. Não ajustar e medir exclusivamente nos mesmos 15 exemplos para afirmar generalização. O lote pago OpenAI permanece pendente de estimativa e autorização específica.

Não foram medidos ganho de produtividade, redução de custo, satisfação ou benefício em operação real. A assinatura ChatGPT Plus não foi usada como API.
