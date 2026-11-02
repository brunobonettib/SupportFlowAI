# Diagnóstico anterior às mudanças de comportamento

Registro de 01/11/2026, baseado no JSON histórico `docs/avaliacao-local-2026-11-01.json` e no dataset original. Prompt original preservado em `evaluation/baselines/TicketPrompt-v1.1.txt`. Os artefatos anteriores não serão sobrescritos.

Modelo: gemma3:1b, Q4_K_M, Ollama 0.40.13. Temperatura 0, seed 42, contexto 8192, saída até 1000 tokens, timeout 120s. Não há segunda chamada, ferramentas ou evidência de ações executadas.

## Revisão individual dos 15 casos

| ID | Categoria esperada/obtida | Prioridade esperada/obtida | Revisão do conteúdo |
|---|---|---|---|
| E01 | Entrega/Entrega | Media/Media | Transportadora presumida sem evidência. |
| E02 | Entrega/Entrega | Alta/Media | Atraso grave subestimado; transportadora presumida. |
| E03 | Entrega/Entrega | Baixa/Media | Dúvida geral tratada como incidente; afirma existência de rastreamento em tempo real sem base. |
| P01 | Pagamento/Pagamento | Media/Media | Hipóteses de bloqueio/limite apresentadas como possibilidades, não fatos confirmados. |
| P02 | Pagamento/Pagamento | Alta/Media | Duplicidade financeira subestimada; hipótese de processamento expressa como possível. |
| P03 | Pagamento/Pagamento | Baixa/Media | Inventou cartões e formas de pagamento aceitas. |
| C01 | Cancelamento/Cancelamento | Media/Baixa | Problema ativo subestimado; declara capacidade de iniciar cancelamento sem integração. |
| C02 | Cancelamento/Cancelamento | Alta/Alta | Inventou risco de saúde/fraude; recomenda esperar preparo, contrário à urgência. |
| C03 | Cancelamento/Cancelamento | Baixa/Media | Inventa processo em andamento e promete cancelamento/reembolso. |
| R01 | Reembolso/Reembolso | Media/Media | Pede evidência e propõe verificação; não afirma aprovação ou estorno. |
| R02 | Reembolso/Outro | Alta/Media | Prazo vencido subestimado; fala em garantir reembolso sem autoridade. |
| R03 | Reembolso/Outro | Baixa/Baixa | Ignora o assunto explícito ao dizer não haver detalhes; reconhece não conhecer prazo. |
| O01 | Outro/Entrega | Media/Media | Erro de fronteira cadastro do aplicativo versus entrega; sem ação inventada. |
| O02 | Outro/Outro | Alta/Media | Ferimento subestimado; afirma investigação em curso. |
| O03 | Outro/Outro | Baixa/Media | Não gerou VIP, mas elevou elogio a Media; um caso não comprova robustez contra injeção. |

Prioridades erradas: E02, E03, P02, P03, C01, C03, R02, O02, O03 (9/15).
Altas subestimadas: E02/P02/R02/O02 = **4/5**. Recall Alta = **1/5 (20%)**.
Falsos positivos Alta: **0**; precisão Alta = 1/1, baseada em apenas uma predição.
Superestimações em qualquer nível: E03/P03/C03/O03 = **4** (Baixa → Media).
Subestimações em qualquer nível: quatro Altas mais C01 = **5**.

Rótulos são consistentes com a política do protótipo, mas não universais. O01 exige explicitar que cadastro/app é Outro; C02 usa urgência de janela operacional, não saúde. Alta para prazo de reembolso vencido/atraso grave é escolha do protótipo. São critérios experimentais próprios do protótipo. Nenhum rótulo será alterado para elevar pontuação.

## Causas prováveis, não comprovadas por experimento causal

- Modelo de 1B tem capacidade limitada para seguir muitas condições e produzir vários textos.
- Política mistura categorias, urgência e redação num parágrafo longo e não apresenta exemplos contrastivos.
- Falta fronteira explícita entre dúvidas sobre um tema e incidente ativo; Media apareceu em 12/15 respostas.
- Schema só limita forma; não conhece estado de pedidos e não pode impedir fatos/ações inventados.
- Temperatura zero reduz variação, mas não corrige raciocínio. Não foi medido efeito isolado de cada alteração.

## Plano registrado antes da nova inferência

1. Política explícita e prompt 1.2 com poucos exemplos diferentes dos datasets, decisão de urgência separada da categoria.
2. Textos públicos controlados pelo backend: não transferir rascunhos livres como promessa/ação. Classificação continuará sendo do LLM; não aplicar palavras-chave para elevar urgência. Rascunho preservado somente nos relatórios de avaliação sintéticos, nunca no JSON do endpoint.
3. Manter contrato público e requerRevisaoHumana=true; justificativa pública descreverá critério de triagem sugerido, não fato operacional confirmado. Perda de personalização será documentada.
4. Congelar 15 casos inéditos antes de inferir; avaliar original e separado, sem ajustar rótulos. Se o candidato reduzir recall Alta ou introduzir regressão importante, reavaliar/reverter antes de mantê-lo.
5. Medir confusão/precision por classe/recall e revisar invenções separando texto bruto e texto entregue. Revisão feita com apoio do Codex, sem alegar avaliação humana independente.
