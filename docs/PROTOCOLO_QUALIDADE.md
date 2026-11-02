# Experimentos de qualidade — protocolo e registro

Atualização: 02/11/2026. Dados exclusivamente fictícios, Ollama local, nenhuma chamada OpenAI.

## Preservação

O baseline original continua em `avaliacao-local-2026-11-01.json`. O dataset original mantém SHA-256 `3e7ea281f89ac8a8c69956d8e521fe70fd49261dba08ed77a2c086fa2d329414`.

O conjunto de 15 casos novos foi registrado em `evaluation/holdout.json` antes da primeira inferência, com hash `e763d38861ccad2b9bfbb9c42fa76627a1d3730229d5080e5abbf781edafe850`. Data e hash do baseline em `evaluation/dataset-freeze.json`. Prompts preservados em `evaluation/baselines/`. Nenhum rótulo foi ajustado para aumentar acertos.

## Sequência e decisões

1. Diagnóstico escrito antes da mudança de comportamento: `DIAGNOSTICO_QUALIDADE_IA.md`.
2. Prompt 1.2 + controle de texto: original 11/15 categorias, 9/15 prioridades, recall Alta 4/5. Rejeitado pelo colapso da categoria Reembolso (0/3) e regressão de categoria.
3. Prompt 1.3 mais curto + controle de texto: original 14/15 categorias, 9/15 prioridades, recall Alta 4/5. A seguir, congelado para testar conjunto separado.
4. Prompt 1.3 no conjunto novo: 7/15 categorias, 6/15 prioridades, recall Alta 3/5, uma resposta inválida (H14). O backend bloqueou essa saída; não existe texto revisável para H14. O código de erro genérico não permite atribuir retrospectivamente a causa exata a truncamento ou campos inválidos.
5. Comparação com prompt antigo no mesmo conjunto novo: houve inicialmente uma repetição involuntária do binário 1.3. Copiar um arquivo com timestamp antigo fez o build incremental reutilizar a saída. A versão no relatório revelou o problema. O artefato foi preservado corretamente como `quality-holdout-v1.3-repeat.*`, sem ser apresentado como baseline. A repetição teve as mesmas contagens de classificação e outra latência/textos; não foi escolhida para melhorar números.
6. Depois de `dotnet build --no-incremental`, o relatório confirmou prompt 1.1: 10/15 categorias, 6/15 prioridades, recall Alta 0/5 no conjunto novo. A proteção de texto foi mantida; ela não altera categoria/prioridade. Isso isola a comparação de classificação entre prompts, sem entregar textos perigosos pela API.
7. O usuário autorizou download e comparação com gemma3:4b. O prompt 1.3 é mantido sem novos ajustes baseados no conjunto separado. Modelo 4B: original 15/15 categorias, 11/15 prioridades, recall Alta 5/5; novo 14/15, 12/15, recall Alta 5/5. Escolhido como padrão local, com regressões/limitações em RESULTADOS_EXPERIMENTAIS.md.

## Fronteiras da avaliação

- O conjunto original virou conjunto de desenvolvimento: foi usado para analisar erros e escolher prompt.
- O conjunto separado foi rotulado antes de inferir e não foi copiado para o prompt. Porém, ao comparar e selecionar candidatos com ele, passa a ser conjunto de validação; não é um teste cego independente final. Será necessário outro conjunto para estimar generalização sem viés de seleção.
- Mesmo temperatura 0 e seed 42 não produziram textos idênticos entre execuções observadas. Alias de modelo, hardware, contexto de execução/cache e implementação podem afetar reprodutibilidade. Não atribuir variação a uma causa específica sem experimento.
- Warm-up é uma chamada separada e sua latência fica fora da média dos 15 casos; o custo computacional não desaparece. Falhas ficam nas acurácias/latência e na coluna SemResposta das matrizes.
- Precisão sem predições de uma classe é nula; recall sem exemplos reais da classe também é nulo. Não substituir por 100% ou por zero inventado.
- Contagem de informações não sustentadas exige leitura contextual conforme RUBRICA_SEGURANCA_TEXTO.md. Anotações em quality-text-review.json e recálculo por scripts/Comparar-Qualidade.ps1.
- O JSON público local não contém rascunho: categoria/prioridade são do LLM; justificativa e sugestão são formulações controladas pelo backend. Zero invenções nesses textos não equivale a zero alucinações do LLM.
