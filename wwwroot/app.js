"use strict";

const form = document.querySelector("#analysis-form");
const message = document.querySelector("#message");
const counter = document.querySelector("#character-count");
const inputError = document.querySelector("#input-error");
const analyzeButton = document.querySelector("#analyze-button");
const analyzeLabel = document.querySelector("#analyze-label");
const buttonIcon = document.querySelector("#button-icon");
const clearButton = document.querySelector("#clear-button");
const exampleButtons = [...document.querySelectorAll("[data-example]")];
const panel = document.querySelector("#analysis-panel");
const badge = document.querySelector("#state-badge");
const announcement = document.querySelector("#analysis-announcement");
const states = {
  empty: document.querySelector("#empty-state"),
  loading: document.querySelector("#loading-state"),
  error: document.querySelector("#error-state"),
  success: document.querySelector("#result-state")
};
const examples = {
  delivery: "Meu pedido não chegou.",
  payment: "Fui cobrado duas vezes pelo mesmo pedido.",
  refund: "Solicitei reembolso ontem e quero acompanhar o andamento."
};
const categories = new Set(["Entrega", "Pagamento", "Cancelamento", "Reembolso", "Outro"]);
const priorities = {
  Alta: { label: "Alta", className: "priority-high" },
  Media: { label: "Média", className: "priority-medium" },
  Baixa: { label: "Baixa", className: "priority-low" }
};
const maxLength = message.maxLength; // Mesmo limite UTF-16 de TicketRules.MaxMessageLength.
let busy = false;
let controller = null;

function setState(state) {
  for (const [name, element] of Object.entries(states)) element.hidden = name !== state;
  badge.textContent = { empty: "Aguardando", loading: "Em análise", error: "Não concluída", success: "Concluída" }[state];
  badge.dataset.state = state;
}

function setBusy(value) {
  busy = value;
  analyzeButton.disabled = value;
  clearButton.disabled = value;
  exampleButtons.forEach(button => { button.disabled = value; });
  message.readOnly = value;
  panel.setAttribute("aria-busy", String(value));
  analyzeLabel.textContent = value ? "Analisando…" : "Analisar solicitação";
  buttonIcon.textContent = value ? "·" : "→";
}

function resetValidation() {
  inputError.hidden = true;
  inputError.textContent = "";
  message.removeAttribute("aria-invalid");
}

function updateCount() {
  counter.textContent = `${message.value.length.toLocaleString("pt-BR")} / ${maxLength.toLocaleString("pt-BR")}`;
  counter.classList.toggle("near-limit", message.value.length >= maxLength * .9);
}

function clearResult() {
  for (const id of ["result-category", "result-priority", "result-reason", "result-response", "error-message"]) {
    document.getElementById(id).textContent = "";
  }
  document.querySelector("#result-priority").className = "priority-badge";
  setState("empty");
  announcement.textContent = "";
}

message.addEventListener("input", () => {
  updateCount();
  resetValidation();
  clearResult(); // A análise anterior não deve parecer vinculada a um texto editado.
});

exampleButtons.forEach(button => button.addEventListener("click", () => {
  if (busy) return;
  message.value = examples[button.dataset.example];
  updateCount();
  resetValidation();
  clearResult();
  message.focus();
}));

clearButton.addEventListener("click", () => {
  if (busy) return;
  message.value = "";
  updateCount();
  resetValidation();
  clearResult();
  message.focus();
});

function friendlyError(status, code) {
  if (code === "local_model_missing") return "O modelo local não foi encontrado. Confira se o modelo configurado foi baixado no Ollama e tente novamente.";
  if (code === "local_provider_unavailable") return "O Ollama está indisponível ou ocupado. Confira se ele está em execução nesta máquina e tente novamente.";
  if (code === "provider_not_configured" || code === "provider_authentication_failed") return "O provedor de IA precisa ser configurado no servidor. Para uma demonstração local, inicie a aplicação com Ollama.";
  if (code === "provider_rate_limited" || status === 429) return "O provedor atingiu um limite de uso. Aguarde e confira a configuração do servidor antes de tentar novamente.";
  if (status === 504) return "A análise excedeu o tempo limite. O modelo pode estar carregando ou ocupado. Aguarde um momento e tente novamente.";
  if (status === 503) return "O serviço de IA está temporariamente indisponível. Confira se o provedor configurado está ativo e tente novamente.";
  if (status === 502) return "O modelo não retornou uma análise válida. Tente novamente ou reformule a solicitação.";
  if (status === 400 || status === 413) return "A solicitação não foi aceita. Use uma mensagem de até 4.000 caracteres e confira se a aplicação foi iniciada com a configuração de desenvolvimento.";
  return "Ocorreu uma falha ao processar a solicitação. Tente novamente em alguns instantes.";
}

function isPublicAnalysis(data) {
  return data && categories.has(data.categoria) && Object.hasOwn(priorities, data.prioridade)
    && typeof data.justificativa === "string" && data.justificativa.trim().length > 0 && data.justificativa.length <= 400
    && typeof data.sugestaoResposta === "string" && data.sugestaoResposta.trim().length > 0 && data.sugestaoResposta.length <= 1200
    && data.requerRevisaoHumana === true;
}

function showError(text) {
  document.querySelector("#error-message").textContent = text;
  setState("error");
  announcement.textContent = text;
}

form.addEventListener("submit", async event => {
  event.preventDefault();
  if (busy) return;
  resetValidation();
  if (!message.value.trim() || message.value.length > maxLength) {
    inputError.textContent = !message.value.trim() ? "Escreva uma solicitação ou escolha um exemplo para começar." : "Use no máximo 4.000 caracteres.";
    inputError.hidden = false;
    message.setAttribute("aria-invalid", "true");
    message.focus();
    return;
  }

  clearResult();
  setBusy(true);
  setState("loading");
  announcement.textContent = "Análise iniciada. Aguarde o resultado; o primeiro carregamento pode levar mais tempo.";
  controller = new AbortController();
  // Acima do máximo configurável do backend (300 s), sem retry automático.
  const timeout = window.setTimeout(() => controller?.abort(), 315000);
  try {
    const response = await fetch("/api/tickets/analyze", {
      method: "POST",
      headers: { "Content-Type": "application/json", Accept: "application/json" },
      body: JSON.stringify({ message: message.value }),
      signal: controller.signal
    });
    const data = await response.json().catch(() => null);
    if (!response.ok) {
      showError(friendlyError(response.status, data?.code));
      return;
    }
    if (!isPublicAnalysis(data)) {
      showError("A API retornou uma resposta inesperada. Nenhum resultado foi exibido. Tente novamente em alguns instantes.");
      return;
    }
    const priority = priorities[data.prioridade];
    document.querySelector("#result-category").textContent = data.categoria;
    document.querySelector("#result-priority").textContent = priority.label;
    document.querySelector("#result-priority").className = `priority-badge ${priority.className}`;
    document.querySelector("#result-reason").textContent = data.justificativa;
    document.querySelector("#result-response").textContent = data.sugestaoResposta;
    setState("success");
    announcement.textContent = `Análise concluída. Categoria: ${data.categoria}. Prioridade: ${priority.label}. Revisão humana obrigatória. O resultado está na seção Análise do ticket.`;
  } catch (error) {
    showError(error.name === "AbortError"
      ? "O tempo de espera terminou. Confira se o serviço de IA está disponível antes de tentar novamente."
      : "Não foi possível conectar à API. Confira se a aplicação está em execução e tente novamente.");
  } finally {
    window.clearTimeout(timeout);
    controller = null;
    setBusy(false);
  }
});

window.addEventListener("pagehide", () => controller?.abort());
updateCount();
