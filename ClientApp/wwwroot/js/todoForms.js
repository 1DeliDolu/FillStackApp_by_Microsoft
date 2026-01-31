window.todoForms = {
  validateRequiredTrimmed: function (inputId) {
    const el = document.getElementById(inputId);
    if (!el) return false;

    const value = (el.value || "").trim();
    const ok = value.length > 0;

    el.classList.toggle("is-invalid", !ok);
    el.classList.toggle("is-valid", ok);

    return ok;
  },

  clearValidation: function (inputId) {
    const el = document.getElementById(inputId);
    if (!el) return;

    el.classList.remove("is-invalid");
    el.classList.remove("is-valid");
    el.value = "";
  }
};
