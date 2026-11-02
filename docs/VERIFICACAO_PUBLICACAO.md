# Verificação da publicação

Data: **02/11/2026**, America/Sao_Paulo. Verificação sobre o projeto existente, sem alterar código de inferência, prompts, datasets ou testes.

## Software e execução

Executado na raiz pelo script `scripts/Verificar.cmd`:

| Verificação | Resultado |
|---|---|
| `dotnet restore SupportFlowAI.slnx` | Sucesso |
| `dotnet build SupportFlowAI.slnx --no-restore` | Sucesso; 0 avisos, 0 erros |
| `dotnet test SupportFlowAI.slnx --no-build --no-restore` | 133 aprovados, 0 falhas, 0 ignorados |
| Auditoria NuGet com `--vulnerable --include-transitive` | Nenhuma vulnerabilidade conhecida listada para os dois projetos |
| Inicialização com `Iniciar-Ollama.cmd` e `Iniciar-Api-Local.cmd` | Sucesso em loopback |
| `GET /openapi/v1.json` | HTTP 200; endpoint de análise presente |
| `scripts/Demonstrar.cmd`, mensagem sintética | HTTP 200; Entrega / Media; cinco campos públicos; revisão humana obrigatória |

Ambiente observado: SDK .NET 10.0.302, runtime 10.0.10. Os contadores do TRX local `TestResults/verification.trx` foram conferidos diretamente. A demonstração realizou uma inferência local com `gemma3:4b`; nenhuma chamada paga foi feita nesta preparação. A duração da requisição foi aproximadamente 6,80 segundos, incluindo o estado de carregamento dessa execução; não constitui benchmark nem substitui as latências históricas.

Restore, build, os 133 testes e auditoria também passaram em uma exportação limpa da árvore preparada no Git, sem depender de arquivos privados, caches ou binários do workspace original. Os processos de API e Ollama iniciados para a demonstração foram encerrados ao final.

## Conferência das evidências experimentais

As métricas abaixo foram recalculadas a partir dos casos dos JSON históricos, sem executar novos lotes ou escolher resultados diferentes:

| Conjunto | Categoria | Prioridade | Recall Alta | Falsos Alta |
|---|---|---|---|---|
| Original, 15 casos | 15/15 | 11/15 | 5/5 | 1 |
| Adicional, 15 casos | 14/15 | 12/15 | 5/5 | 2 |

Foram verificados os hashes dos dois datasets e do baseline original contra `evaluation/dataset-freeze.json`. Os arquivos C#, snapshots dos prompts, dados de avaliação e documentos privados originais foram comparados aos hashes anteriores à preparação e permaneceram inalterados.

Resultados, regressões e limites estão em [RESULTADOS_EXPERIMENTAIS.md](RESULTADOS_EXPERIMENTAIS.md). Cada conjunto contém somente 15 casos sintéticos; validade estrutural não implica classificação correta e estes números não representam produção.

## Preparação dos arquivos públicos

- README reestruturado em português brasileiro, com arquitetura, instalação, configuração, HTTP, testes, avaliação, resultados e limitações.
- Relatório experimental público independente, mantendo os resultados e referências técnicas.
- Documentos privados mantidos no computador e excluídos por regras específicas; a pasta `docs/` permanece versionável.
- Caminhos absolutos pessoais de três auditorias históricas substituídos por caminhos relativos, sem alterar os resultados. Originais preservados localmente em pasta ignorada.
- Detalhe pessoal de faturamento removido do registro histórico de integração; HTTP 429, duração e limitações diagnósticas preservados.
- Caches, pesos, runtime, logs, arquivos de ambiente, credenciais locais e resultados temporários fora do conteúdo público.
- Nenhuma licença adicionada: a escolha permanece com o proprietário.

## Segurança e integridade da publicação

- Inventário público: 91 arquivos, incluindo código, solução, testes, configurações sem segredos, scripts, documentação e dados sintéticos.
- Gitleaks 8.30.1 executado com valores redigidos sobre uma exportação do conteúdo preparado: nenhum segredo detectado. Ferramenta portátil mantida somente no ambiente local ignorado.
- Revisão adicional de nomes, referências privadas, caminhos pessoais, credenciais literais e links Markdown: nenhuma pendência. As duas atribuições de credenciais identificadas são placeholders fictícios dos testes.
- `.gitattributes` preserva os bytes dos datasets, relatórios JSON e snapshots/prompt, evitando que a normalização de quebras de linha altere os hashes congelados. Os hashes foram conferidos também na exportação do Git.
- A branch de publicação não possuía commits anteriores. Referências locais de ferramentas não integram o histórico de `main` e não são parte da publicação.

Essas verificações reduzem o risco de exposição; não são uma certificação de ausência de vulnerabilidades. Os artefatos locais completos de verificação permanecem em diretórios ignorados.

A auditoria NuGet é limitada aos avisos conhecidos das dependências .NET na data da execução. Não avalia o runtime Ollama, pesos do modelo ou segurança completa da aplicação.
