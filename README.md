# SupportFlow AI

API de classificação e priorização de solicitações de atendimento com C#, ASP.NET Core e LLMs locais.

## Visão geral

SupportFlow AI é um MVP independente de engenharia de software e inteligência artificial. Recebe uma mensagem, sugere categoria e prioridade e devolve textos para revisão humana. A configuração padrão executa inferência real com **Ollama e Gemma 3 4B**, sem cobrança de API. Existe uma integração opcional com OpenAI, selecionada explicitamente.

O projeto reúne validação de contratos, tratamento de falhas, testes automatizados e avaliação experimental reproduzível. **Não é um sistema pronto para produção.**

## Problema que a solução resolve

A triagem exige ler solicitações, identificar assunto e urgência e preparar uma resposta inicial. Este projeto explora o uso de IA para apoiar essas etapas. Não foram medidos ganhos de produtividade, satisfação, economia ou impacto em clientes reais.

## Principais funcionalidades

- `POST /api/tickets/analyze` com validação de entrada e respostas JSON estruturadas.
- Categorias: `Entrega`, `Pagamento`, `Cancelamento`, `Reembolso` e `Outro`.
- Prioridades: `Baixa`, `Media` e `Alta`.
- Justificativa e sugestão públicas controladas pelo backend **no caminho Ollama**, preservando os rótulos inferidos.
- `requerRevisaoHumana: true` definido pelo backend; nenhuma resposta é enviada automaticamente.
- Timeout, cancelamento, limites de entrada/saída e erros em Problem Details, sem registrar mensagens ou credenciais nos logs da aplicação.
- Avaliação com acurácia, precisão/recall por classe, matrizes de confusão e latência.
- OpenAPI disponível em Development em `/openapi/v1.json`.

## Tecnologias utilizadas

| Tecnologia | Uso |
|---|---|
| C# / .NET 10 / ASP.NET Core | Minimal API, injeção de dependências e configuração |
| Ollama / `gemma3:4b` | Inferência local padrão |
| OpenAI Responses API | Provedor opcional via HTTP |
| `IHttpClientFactory` / `System.Text.Json` | Transporte e contratos, sem SDK adicional de IA |
| xUnit / `WebApplicationFactory` | Testes unitários e de integração HTTP em memória |
| PowerShell / scripts `.cmd` | Inicialização, verificação e comparação de resultados |

## Arquitetura simplificada

```text
Cliente HTTP → Minimal API → ITicketAnalyzer
                              ├─ OllamaTicketAnalyzer → Ollama local
                              │   → TicketOutputParser → SafeTicketText
                              └─ OpenAiTicketAnalyzer → Responses API
                                  → TicketOutputParser
             ← JSON com classificação e revisão humana obrigatória

Avaliador → mesmo ITicketAnalyzer → relatórios JSON e Markdown
```

| Diretório / arquivo | Responsabilidade |
|---|---|
| `Program.cs` | Composição, endpoint, configuração e erros HTTP |
| `Tickets/` | Contratos, validação, parser e textos públicos controlados |
| `Ollama/` | Cliente e opções da inferência local |
| `OpenAI/` | Cliente opcional e prompt/schema compartilhados |
| `evaluation/` | Dados sintéticos, métricas, avaliador e snapshots dos prompts |
| `tests/` | Testes automatizados sem chamadas externas de IA |
| `docs/` | Metodologia, resultados e evidências técnicas |
| `scripts/` | Comandos de execução e verificação |

## Fluxo de processamento

1. O endpoint valida a mensagem: obrigatória, até 4.000 unidades UTF-16; corpo HTTP limitado a 32 KiB no Kestrel.
2. O cliente selecionado envia instruções e mensagem separadas, com JSON Schema e limite de saída.
3. O backend valida o envelope e o JSON retornado, incluindo rótulos, campos duplicados/adicionais e tamanhos dos textos.
4. No caminho local, `SafeTicketText` substitui justificativa e sugestão por formulações controladas. Categoria e prioridade continuam sendo decisões do modelo.
5. A API devolve os cinco campos públicos para revisão humana. O rascunho livre fica fora do endpoint e só é preservado nos relatórios de avaliação sintéticos.

Não há retry nem fallback entre provedores. JSON válido comprova estrutura, **não classificação correta ou veracidade**. A justificativa pública descreve um critério de triagem; não expõe o raciocínio interno do modelo.

## Integração com Ollama

A aplicação usa `http://127.0.0.1:11434/api/chat`, sem proxy, redirects ou credencial. Padrões: `gemma3:4b`, prompt 1.3, temperatura 0, seed 42, contexto 8.192 tokens, saída até 1.000 tokens e timeout de 120 segundos.

Instale o Ollama conforme a [documentação oficial para Windows](https://docs.ollama.com/windows). Com o servidor ativo, baixe o modelo:

```powershell
ollama pull gemma3:4b
```

Para executar um servidor pelo terminal, defina as variáveis **antes** de iniciá-lo. Se o aplicativo Ollama já estiver ativo, encerre-o primeiro para aplicar esta configuração e evitar conflito de porta:

```powershell
$env:OLLAMA_HOST = '127.0.0.1:11434'
$env:OLLAMA_NO_CLOUD = '1'
ollama serve
```

O [modo local do Ollama](https://docs.ollama.com/faq#how-do-i-disable-ollamas-cloud-features) permite desativar os recursos de nuvem. Os scripts deste projeto usam uma alternativa portátil: extraia a distribuição oficial para `.runtime/ollama`, contendo `ollama.exe`, e execute `scripts/Iniciar-Ollama.cmd`. Esse script guarda modelos em `.runtime/models`, separadamente da instalação padrão. Nesse caso, baixe o modelo no servidor portátil usando:

```powershell
Invoke-RestMethod -Method Post -Uri 'http://127.0.0.1:11434/api/pull' `
  -ContentType 'application/json' -Body '{"model":"gemma3:4b","stream":false}' -TimeoutSec 900
```

Runtime, pesos e caches não fazem parte do repositório. Os experimentos registrados usaram Ollama 0.40.13, Gemma 3 4B Q4_K_M e GPU RTX 2060 de 6 GB. Consulte os [metadados do modelo](docs/quality-models-2026-11-02.json). Hardware, cache e versão podem alterar os resultados e a latência.

## Integração opcional com OpenAI

O código implementa a Responses API via HTTP, com JSON Schema estrito e `store=false`. A configuração versionada usa `gpt-6-luna` e esforço `none`; isso registra o valor configurado, sem garantir disponibilidade ou compatibilidade ao vivo. O histórico disponível contém uma tentativa com HTTP 429 do provedor, traduzida para `503 / provider_rate_limited`, sem classificação bem-sucedida. Veja o [registro técnico](docs/TESTE-REAL-2026-10-31.md).

A aplicação lê `OpenAI:ApiKey` da configuração ou `OPENAI_API_KEY` do ambiente. Para desenvolvimento, salve a chave em .NET user-secrets com entrada oculta, sem colocá-la no repositório ou no histórico do terminal:

```powershell
$sfKey = Read-Host 'Chave da API OpenAI' -AsSecureString
try {
  if ($sfKey.Length -eq 0) { throw 'Chave vazia.' }
  $sfJson = @{ 'OpenAI:ApiKey' = [Net.NetworkCredential]::new('', $sfKey).Password } | ConvertTo-Json -Compress
  $sfJson | dotnet user-secrets set --id SupportFlowAI-local-development 2>$null | Out-Null
  if ($LASTEXITCODE -ne 0) { throw 'Não foi possível salvar a chave.' }
} finally {
  $sfKey.Dispose()
  Remove-Variable sfKey, sfJson -ErrorAction SilentlyContinue
}
```

User-secrets é armazenamento local fora do Git, sem criptografia. Para usar esse provedor, selecione-o em outro terminal, depois de verificar acesso ao modelo e orçamento:

```powershell
$env:Analysis__Provider = 'OpenAI'
dotnet run --project SupportFlowAI.csproj --no-build --launch-profile http
```

Mensagens válidas passam a chamar uma API paga. Iniciar o servidor, compilar e executar os testes não faz inferência paga. O controle de texto de `SafeTicketText` é exclusivo do cliente Ollama; o cliente OpenAI devolve os textos do modelo após validação estrutural, exigindo revisão humana. Não há fallback automático para OpenAI.

## Como instalar e executar

Pré-requisitos: Git, SDK .NET 10, Ollama e o modelo local. Os comandos abaixo usam PowerShell no Windows; os scripts `.cmd` são específicos do Windows. O projeto foi verificado com SDK 10.0.302 e runtime 10.0.10.

```powershell
git clone https://github.com/brunobonettib/SupportFlowAI.git
cd SupportFlowAI
dotnet restore SupportFlowAI.slnx
dotnet build SupportFlowAI.slnx --no-restore

# Mantenha Ollama ativo em outro terminal.
$env:Analysis__Provider = 'Ollama'
dotnet run --project SupportFlowAI.csproj --no-build --launch-profile http
```

API: `http://localhost:5143`. OpenAPI: `http://localhost:5143/openapi/v1.json`. Ctrl+C encerra o servidor do terminal. O primeiro carregamento do modelo pode levar mais tempo.

Alternativa com os scripts existentes, a partir da raiz:

```powershell
.\scripts\Verificar.cmd
# Terminal 1, se estiver usando a distribuição portátil:
.\scripts\Iniciar-Ollama.cmd
# Terminal 2:
.\scripts\Iniciar-Api-Local.cmd
```

Os scripts isolam os caches no projeto e não alteram o perfil global do terminal. `appsettings.json` e `appsettings.Development.json` são configurações públicas sem chave. Variáveis de ambiente sobrescrevem os padrões:

| Variável | Padrão | Limites / observação |
|---|---|---|
| `Analysis__Provider` | `Ollama` | `Ollama` ou `OpenAI`, diferencia maiúsculas |
| `Ollama__Model` | `gemma3:4b` | Modelo local; scripts respeitam a variável |
| `Ollama__TimeoutSeconds` | `120` | 1 a 300 |
| `Ollama__MaxOutputTokens` | `1000` | 128 a 4000 |
| `Ollama__ContextTokens` | `8192` | 8192 a 16384 |
| `OpenAI__Model` | `gpt-6-luna` | Configurável; sucesso ao vivo não validado |
| `OpenAI__ReasoningEffort` | `none` | Deve ser compatível com o modelo escolhido |
| `OpenAI__TimeoutSeconds` | `30` | 1 a 120 |
| `OpenAI__MaxOutputTokens` | `1000` | 128 a 4000 |
| `OpenAI__ApiKey` | Ausente | Precede `OPENAI_API_KEY` |

## Como fazer uma requisição HTTP

Com a API iniciada em modo Ollama:

```powershell
curl.exe --silent --show-error --fail-with-body --max-time 130 `
  --header 'Content-Type: application/json' `
  --data-binary '@evaluation/demo-request.json' `
  http://localhost:5143/api/tickets/analyze
```

Também é possível executar `.\scripts\Demonstrar.cmd` ou usar os exemplos de [SupportFlowAI.http](SupportFlowAI.http).

| Situação | HTTP |
|---|---|
| Mensagem ausente/vazia/longa ou JSON inválido | 400 |
| Corpo acima de 32 KiB no Kestrel | 413 |
| Content-Type incompatível | 415 |
| Resposta inválida/incompleta, recusa ou rejeição pelo provedor | 502 |
| Provedor indisponível, modelo local ausente, credencial/limite OpenAI | 503 |
| Timeout do provedor | 504 |
| Falha inesperada | 500, sem detalhes internos |

## Exemplo de entrada e saída

Entrada:

```json
{"message":"Meu pedido não chegou."}
```

Resposta registrada com Ollama 4B e a política de texto controlado ([evidência HTTP](docs/quality-http-2026-11-02.json)); inferências futuras podem variar:

```json
{
  "categoria": "Entrega",
  "prioridade": "Media",
  "justificativa": "Triagem preliminar em Entrega. Media indica possível problema ativo; confirme a ausência de sinais de urgência na revisão.",
  "sugestaoResposta": "Para orientar sobre a entrega, um atendente precisa esclarecer sua dúvida ou conferir os detalhes disponíveis. Não há confirmação de status nesta sugestão.",
  "requerRevisaoHumana": true
}
```

## Como executar testes automatizados

Depois de restore e build:

```powershell
dotnet test SupportFlowAI.slnx --no-build --no-restore
dotnet list SupportFlowAI.slnx package --vulnerable --include-transitive
```

`.\scripts\Verificar.cmd` reúne restore, build, testes e auditoria NuGet. A verificação de publicação confirmou **133 testes aprovados**, nenhum ignorado, build sem avisos/erros e nenhuma vulnerabilidade conhecida listada na auditoria transitiva. [Registro de verificação](docs/VERIFICACAO_PUBLICACAO.md).

Os testes usam stubs e HTTP em memória: verificam contratos, falhas, segurança de saída e métricas; não medem qualidade semântica do LLM nem validam a disponibilidade dos provedores.

## Como executar a avaliação do modelo

Com o build concluído e o servidor Ollama ativo:

```powershell
.\scripts\Avaliar-Local.cmd original
.\scripts\Avaliar-Local.cmd holdout
```

Cada lote faz **um aquecimento separado e 15 inferências locais**. O avaliador usa o mesmo `ITicketAnalyzer` do endpoint, sequencialmente, sem fornecer os rótulos esperados ao modelo e sem precisar iniciar o servidor ASP.NET. Salva novos JSON/Markdown em `evaluation/reports/`, ignorado pelo Git, preservando os relatórios históricos de `docs/`.

Equivalente sem o script:

```powershell
$env:Analysis__Provider = 'Ollama'
dotnet run --project SupportFlowAI.csproj --no-build --no-launch-profile -- `
  --evaluate --allow-local-evaluation --warm-up --evaluation-set=original
```

Sem `--allow-local-evaluation`, o lote local não é executado. A avaliação paga exige uma flag distinta (`--allow-paid-evaluation`) e chave; a autorização local não habilita chamadas pagas. Ctrl+C preserva relatório parcial. Código de saída 0 significa conclusão sem falha de provedor/estrutura, não classificação perfeita.

Para recalcular os resultados arquivados sem inferência, execute `.\scripts\Comparar-Qualidade.ps1`. Consulte o [protocolo](docs/PROTOCOLO_QUALIDADE.md), a [política de triagem](docs/POLITICA_TRIAGEM.md) e o [relatório técnico](docs/RESULTADOS_EXPERIMENTAIS.md).

## Resultados experimentais

Resultados registrados em **02/11/2026**, com Gemma 3 4B, prompt 1.3 e política de texto 1.0. Contagens conferidas diretamente nas linhas dos relatórios JSON:

| Métrica | Original — 15 casos | Adicional — 15 casos |
|---|---|---|
| Acurácia de categoria | **100% (15/15)** | **93,33% (14/15)** |
| Acurácia de prioridade | **73,33% (11/15)** | **80% (12/15)** |
| Recall de prioridade Alta | **100% (5/5)** | **100% (5/5)** |
| Precisão de prioridade Alta | 83,33% (5/6) | 71,43% (5/7) |
| Falsos alertas Alta | 1 | 2 |
| Respostas estruturalmente válidas | 15/15 | 15/15 |
| Latência média sem aquecimento | 1724,27 ms | 1320,26 ms |

Fontes: [original](docs/quality-original-4b-v1.3.json), [adicional](docs/quality-holdout-4b-v1.3.json) e [comparação com baselines, erros e regressões](docs/RESULTADOS_EXPERIMENTAIS.md).

São **apenas 15 exemplos sintéticos por conjunto**, preparados com apoio do Codex, sem rotulagem humana independente. O conjunto adicional foi usado para selecionar o modelo e representa validação, não um teste cego final. Os resultados não demonstram desempenho em produção. Recall Alta 100% representa cinco acertos em cada amostra, com falsos alertas remanescentes.

## Limitações conhecidas

- O modelo ainda erra categoria e prioridade, inclusive sob prompt injection. Não há confiança calibrada nem garantia de generalização.
- Rascunhos livres continham informações não sustentadas. Textos públicos controlados no caminho local reduzem esse risco, com perda de personalização; isso não significa ausência de alucinações do modelo.
- Revisão humana permanece necessária. Não há consulta a pedidos, confirmação de ações, cancelamento, reembolso ou envio automático.
- Não há autenticação, rate limiting, persistência, frontend, RAG, controle agregado de orçamento ou deploy. Use dados fictícios e mantenha o MVP em loopback.
- Fora de Development, HTTP é rejeitado. Hospedagem futura exigiria configuração adequada de TLS e infraestrutura.
- OpenAI não teve inferência bem-sucedida validada ao vivo; limites por chamada não limitam gasto acumulado.
- Auditoria NuGet cobre avisos conhecidos de dependências .NET na data da consulta; não certifica o runtime Ollama, o modelo ou todo o sistema.

## Melhorias futuras

1. Avaliar em uma amostra maior e independente, com rotulagem humana e casos ambíguos/múltiplos.
2. Comparar com um baseline por regras e medir tempo manual versus assistido, incluindo correções.
3. Investigar erros de prioridade, falsos alertas Alta e resistência a prompt injection sem ocultar regressões.
4. Validar OpenAI com orçamento definido e registrar qualidade, consumo e limitações.
5. Para eventual uso real, implementar controles de acesso, privacidade, orçamento, observabilidade e hospedagem segura.
