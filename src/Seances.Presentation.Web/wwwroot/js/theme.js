(() => {
    const storageKey = "seances.theme";
    const system = window.matchMedia("(prefers-color-scheme: dark)");
    const normalize = value => ["light", "dark", "system"].includes(value) ? value : "system";
    let preference = "system";

    try {
        preference = normalize(window.localStorage.getItem(storageKey));
    } catch {
        // Storage may be unavailable; the in-page choice still works.
    }

    function sync() {
        const resolved = preference === "system" ? (system.matches ? "dark" : "light") : preference;
        const stylesheet = document.getElementById("dark-theme");
        stylesheet.media = preference === "system" ? system.media : preference === "dark" ? "all" : "not all";
        document.documentElement.dataset.theme = preference;
        document.documentElement.style.colorScheme = resolved;
        document.querySelectorAll("[data-theme-selector]").forEach(select => { select.value = preference; });
        document.querySelectorAll("meta[data-theme-color]").forEach(meta => {
            meta.media = meta.dataset.themeColor === resolved ? "all" : "not all";
        });
    }

    document.addEventListener("change", event => {
        if (!event.target.matches("[data-theme-selector]")) return;
        preference = normalize(event.target.value);
        try {
            window.localStorage.setItem(storageKey, preference);
        } catch {
            // Keep the current choice even when it cannot be persisted.
        }
        sync();
    });
    document.addEventListener("DOMContentLoaded", () => {
        sync();
        window.Blazor?.addEventListener("enhancedload", sync);
    });
    system.addEventListener("change", sync);
    window.seancesTheme = { sync };
    sync();
})();
