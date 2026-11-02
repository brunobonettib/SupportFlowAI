# Evidências de verificação

Data da sessão: **31/10/2026**, fuso do usuário America/Sao_Paulo. Os relatórios do avaliador usam UTC.

## Ambiente e escopo

- Projeto existente: ASP.NET Core `net10.0`, template WeatherForecast.
- SDK observado: `10.0.302`; runtime `10.0.10`.
- Dependência original direta: `Microsoft.AspNetCore.OpenApi 10.0.10`.
- Dependência transitiva original vulnerável: `Microsoft.OpenApi 2.0.0`, aviso `NU1903`.
- A pasta não era um repositório Git. Nenhum commit, push, publicação ou deploy foi realizado.
- Arquivos e caches criados nesta tarefa ficam dentro de SupportFlowAI. Não foram configurados certificados nem gravadas credenciais.

## Comandos e resultados

| Verificação | Resultado observado |
|---|---|
| `dotnet restore SupportFlowAI.slnx --configfile NuGet.Config` | ambos os projetos restaurados |
| `dotnet build SupportFlowAI.slnx --no-restore` | sucesso; 0 avisos, 0 erros |
| `dotnet test SupportFlowAI.slnx --no-restore --logger 'trx;LogFileName=tests.trx' --results-directory TestResults` | 76 aprovados, 0 falhas, 0 ignorados |
| `dotnet list SupportFlowAI.slnx package --vulnerable --include-transitive --no-restore --format json` | nenhum pacote vulnerável conhecido listado para aplicação ou testes |
| `dotnet run --project SupportFlowAI.csproj --no-build --launch-profile http` | servidor iniciado em `http://localhost:5143`, Development, sem aviso de redirecionamento HTTPS |
| `dotnet run --project SupportFlowAI.csproj --no-build -- --evaluate` | relatório `nao_executado`, motivo: chave ausente; nenhuma chamada ao provedor |

O restore e a auditoria precisaram de permissão para ler a configuração global do NuGet fora da sandbox; o acesso foi autorizado. Downloads/caches foram direcionados ao projeto. A [saída JSON final da auditoria](auditoria-dependencias.json) foi preservada. O TRX está em `TestResults/tests.trx` (artefato local ignorado pelo Git).

## Smoke test com Kestrel real

| Requisição | Resultado |
|---|---|
| Mensagem vazia | 400 |
| Campo message ausente | 400 |
| JSON malformado | 400 |
| Message numérica | 400 |
| Campo extra | 400 |
| Mensagem com 4001 caracteres | 400 |
| Corpo com mais de 32 KiB | 413 |
| Mensagem válida sem chave configurada | 503, `provider_not_configured` |
| GET `/openapi/v1.json` | 200, endpoint de análise presente |

O servidor iniciado para a verificação foi encerrado ao final. Não houve HTTP 200 de análise produzido por um LLM real. O caminho de sucesso foi validado com resposta fictícia nos testes automatizados.

## Problemas encontrados e corrigidos

1. **Dependência vulnerável:** o advisory oficial aponta correção na linha 2.x a partir de `2.7.5`; adicionada referência direta. O grafo final da aplicação contém apenas `Microsoft.AspNetCore.OpenApi/10.0.10` e `Microsoft.OpenApi/2.7.5`. O teste de geração do OpenAPI passou. [Advisory Microsoft](https://github.com/microsoft/OpenAPI.NET/security/advisories/GHSA-v5pm-xwqc-g5wc).
2. **HTTPS sem porta no perfil HTTP:** removido redirecionamento; Development admite loopback HTTP, demais ambientes rejeitam HTTP. [Documentação Microsoft](https://learn.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-10.0).
3. **Falha de logging no Windows:** o Event Log padrão falhava por permissão e abortava respostas de erro. Configurado somente console, com erros sanitizados e sem escrita no log do sistema. A repetição do smoke test passou.
4. **Erro na montagem de um teste:** substituir texto acentuado após serialização não produzia a entrada pretendida; corrigido para serializar diretamente a justificativa em branco. A validação passou.
5. **Script PowerShell sem assinatura bloqueado:** fornecidos comandos de ambiente no README; a política do sistema não foi alterada.

## Avaliação de IA e pendências

Foi gerado localmente `evaluation/reports/evaluation-20261031-203124-289-1621f09fd2654ae2bd5846161abbd05a.md`, com JSON correspondente. Seu status é **não executado**, com 0 tentativas e as quatro métricas nulas. Dataset SHA-256: `3e7ea281f89ac8a8c69956d8e521fe70fd49261dba08ed77a2c086fa2d329414`.

Na verificação inicial faltava configurar a chave. Posteriormente o usuário a configurou localmente e autorizou uma única tentativa: o backend retornou HTTP 503 / `provider_rate_limited`, correspondente a HTTP 429 da OpenAI, em 2100,37 ms. Não houve classificação. O [registro do teste real](TESTE-REAL-2026-10-31.md) descreve a evidência e suas limitações. Faltam conferir faturamento/limites, obter autorização para nova tentativa e, separadamente, estimar e autorizar a avaliação das 15 mensagens. Não houve medição de qualidade do LLM nem de tempo economizado.

Os testes automatizados usam mensagens e respostas fictícias. Eles comprovam comportamento do código sob condições controladas; não comprovam disponibilidade da conta, compatibilidade observada ao vivo com a API ou qualidade da classificação.
