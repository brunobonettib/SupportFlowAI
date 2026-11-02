> **Atualização de 02/11/2026:** a melhoria de qualidade foi verificada com **133 testes aprovados**, build sem avisos/erros e auditoria transitiva sem vulnerabilidades conhecidas listadas. Padrão local: gemma3:4b, prompt 1.3, textos controlados. [Relatório atual](RESULTADOS_EXPERIMENTAIS.md), [auditoria](auditoria-qualidade-2026-11-02.json) e [HTTP final](quality-http-2026-11-02.json). TRX: `TestResults/quality-final-2026-11-02.trx`. O conteúdo abaixo é a verificação histórica de 01/11, preservada.

# Verificação final — SupportFlow AI

**01/11/2026**, America/Sao_Paulo. Histórico inicial preservado em [VERIFICACAO-2026-10-31.md](VERIFICACAO-2026-10-31.md). Resultados abaixo foram executados, não estimados.

## Software

| Verificação | Resultado |
|---|---|
| Baseline `dotnet test ... --no-build --no-restore` | 76 aprovados, zero falhas |
| `dotnet restore SupportFlowAI.slnx` | aplicação e testes restaurados |
| `dotnet build SupportFlowAI.slnx --no-restore` | sucesso, **0 avisos, 0 erros** |
| `dotnet test SupportFlowAI.slnx --no-build --no-restore` após alterações | **114 aprovados, 0 falhas, 0 ignorados** |
| `dotnet list SupportFlowAI.slnx package --vulnerable --include-transitive` | nenhum pacote vulnerável conhecido listado em ambos os projetos |
| Nova auditoria com `--no-restore --format json` | mesma conclusão, saída preservada |

Comando reprodutível: `scripts\Verificar.cmd`. Evidência local dos testes: `TestResults/final-2026-11-01.trx` (cópia do TRX final; ignorado pelo Git). Contadores conferidos diretamente no XML: total/executed/passed=114, failed=0. [Auditoria JSON](auditoria-dependencias-2026-11-01.json). .NET SDK 10.0.302/runtime 10.0.10. Caches de desenvolvimento ficaram no workspace.

Os 38 casos adicionais cobrem transporte local e ausência de bearer, isolamento de instruções/dados, entradas ambíguas/múltiplas/injeção/dados sensíveis fictícios, envelopes incompletos/duplicados, falhas e timeout/cancelamento, autorização por provedor, métricas com falhas e relatórios parciais. Os stubs não medem interpretação do modelo. Os 76 testes anteriores foram preservados; não houve falhas ocultadas/ignoradas.

## HTTP real e inferência

[Smoke test com Kestrel](http-checks-2026-11-01.json): mensagem vazia, JSON duplicado/malformado e texto com 4001 caracteres retornaram 400; corpo acima de 32 KiB retornou 413; OpenAPI retornou 200. Nenhuma dessas entradas inválidas exige inferência.

[Primeira chamada real local](local-request-2026-11-01.json): HTTP 200, Entrega/Media, justificativa e sugestão presentes, requerRevisaoHumana=true, 96244,99 ms incluindo inicialização. O texto menciona transportadora sem contexto; não foi aprovado para envio.

[15 mensagens locais autorizadas](avaliacao-local-2026-11-01.json): categoria 12/15 (80%), prioridade 6/15 (40%), validade estrutural 15/15 (100%), zero falhas de transporte/estrutura, latência média 938,33 ms. Soma/agregação conferidas separadamente a partir das linhas do JSON. A chamada inicial não integra essa média; o modelo já estava aquecido.

A [revisão das saídas](AVALIACAO_LOCAL.md) documenta subestimação de urgência, políticas inventadas e afirmações de ação inexistente. Validação estrutural não bloqueia esses erros semânticos. Nenhuma resposta é enviada automaticamente.

Com Ollama desligado, uma requisição válida ao Kestrel retornou **503 / local_provider_unavailable**, com instrução segura para iniciar o servidor local. [Problem Details observado](local-unavailable-2026-11-01.json). Não houve inferência nessa checagem nem fallback para OpenAI.

O servidor foi iniciado sem aviso de redirecionamento HTTPS; testes de Production também verificam rejeição de HTTP. Não foi alterado certificado ou configuração global. Scripts locais selecionam Ollama explicitamente; não há fallback OpenAI.

## Rastreabilidade do runtime local

- Ollama portátil v0.40.13 obtido da release oficial; ZIP SHA-256 conferido: `7afa38624fa62ee5d8293422d0ab084c1bb4603134c7f4dafce11a033b57cc19`.
- Modelo gemma3:1b Q4_K_M; digest e tamanho em [metadados](ollama-model-2026-11-01.json), dados de carregamento em [memória](ollama-memory-2026-11-01.json).
- GPU detectada: RTX 2060, 6 GB. Servidor confirmou `Ollama cloud disabled: true` e bind 127.0.0.1:11434.
- Arquivos, modelo, logs e diretórios de estado dentro de `.runtime/`, ignorado pelo Git e excluído dos itens de compilação/publicação.
- Download e execução foram locais/sem instalação global. A primeira inicialização via redirecionamento PowerShell foi interrompida por tratamento de stderr; a inicialização com Start-Process e arquivos de log dedicados resolveu o problema. Não foi preciso elevar permissões.

## Diagnóstico e mudanças por prioridade

| Prioridade | Achado | Correção ou limite registrado |
|---|---|---|
| Alta | Risco de confundir execução local com API paga | seleção explícita de provedor, padrão Ollama e flags separadas de avaliação; nenhum fallback |
| Alta | Envelopes/JSON ambíguos | rejeição de propriedades duplicadas e múltiplas mensagens assistentes OpenAI |
| Alta | Privacidade e recurso ilimitado | cliente sem log de corpo/headers, leitura limitada compartilhada, timeout/cancelamento, sem retries |
| Alta | Saídas semanticamente ruins | erros reais preservados; revisão humana obrigatória e uso exclusivamente demonstrativo; não alegar garantia por prompt |
| Média | Avaliador identificava todo resultado como OpenAI | metadados específicos do provedor e contagem explícita de falhas |
| Média | Cancelamento do avaliador não preservava progresso | Ctrl+C interrompe novas chamadas e grava relatório parcial; teste automatizado |
| Média | Casos ambíguos/múltiplos não tinham política clara | prompt 1.1 com desempate e instrução de esclarecimento |
| Média | Demonstração dependia de API paga | runtime portátil, cliente local e scripts Windows sem ferramentas globais |
| Média | Documentação desatualizada e autoria pouco explícita | Documentação reescrita com resultados, limitações e participação do Codex |

A correção anterior de Microsoft.OpenApi 2.0.0 para 2.7.5 foi mantida; advisory oficial GHSA-v5pm-xwqc-g5wc e testes do contrato demonstram justificativa e compatibilidade. Nenhum aviso NuGet foi suprimido. A auditoria cobre NuGet; não é uma auditoria de vulnerabilidades do runtime/modelo Ollama nem certificação de segurança do MVP.

## OpenAI e fronteiras da evidência

Não houve chamada OpenAI nesta finalização. A única tentativa histórica autorizada de 31/10/2026 retornou 503/provider_rate_limited, mapeado de 429, em 2100,37 ms; sem classificação. [Registro](TESTE-REAL-2026-10-31.md). Logo, **integração OpenAI implementada e testada com stubs, mas sem inferência bem-sucedida validada ao vivo**.

Chave não foi acessada, exibida ou copiada pelas ferramentas. ChatGPT Plus não foi utilizado como API. Novas tentativas pagas dependem de orçamento, estimativa e autorização explícita. Nenhuma métrica de produtividade, qualidade OpenAI, custo real ou satisfação foi inventada.

A pasta não era um repositório Git; não houve commit, push, deploy ou publicação. Os servidores de verificação são encerrados ao final. Para demonstrar, inicie novamente pelos scripts documentados no README.
