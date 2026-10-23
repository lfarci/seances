import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import { test } from "node:test";
import { runInNewContext } from "node:vm";

const source = await readFile(new URL("../../../../src/Seances.Presentation.Web/wwwroot/js/theme.js", import.meta.url), "utf8");

function setup({ stored = null, dark = false, blocked = false } = {}) {
    const listeners = new Map();
    const mediaListeners = new Map();
    const navigationListeners = new Map();
    const root = { dataset: {}, style: {} };
    const stylesheet = { media: "(prefers-color-scheme: dark)" };
    const select = { value: "system", matches: selector => selector === "[data-theme-selector]" };
    const metadata = ["light", "dark"].map(value => ({ dataset: { themeColor: value }, media: "" }));
    const media = { matches: dark, media: "(prefers-color-scheme: dark)",
        addEventListener: (type, callback) => mediaListeners.set(type, callback) };
    let saved = stored;
    const document = {
        documentElement: root,
        getElementById: id => id === "dark-theme" ? stylesheet : null,
        querySelectorAll: selector => selector === "[data-theme-selector]" ? [select] : metadata,
        addEventListener: (type, callback) => listeners.set(type, callback)
    };
    const window = {
        Blazor: { addEventListener: (type, callback) => navigationListeners.set(type, callback) },
        matchMedia: () => media,
        localStorage: {
            getItem: () => { if (blocked) throw new Error("Storage denied"); return saved; },
            setItem: (key, value) => {
                if (blocked) throw new Error("Storage denied");
                assert.equal(key, "seances.theme");
                saved = value;
            }
        }
    };
    runInNewContext(source, { window, document });
    return { root, stylesheet, select, metadata, window, saved: () => saved,
        choose(value) { select.value = value; listeners.get("change")({ target: select }); },
        systemDark(value) { media.matches = value; mediaListeners.get("change")(); },
        ready() { listeners.get("DOMContentLoaded")(); },
        enhancedLoad() { navigationListeners.get("enhancedload")(); },
        unrelatedChange() { listeners.get("change")({ target: { matches: () => false, value: "dark" } }); }
    };
}

test("system is the initial preference and follows the operating system", () => {
    const state = setup();
    assert.equal(state.root.dataset.theme, "system");
    assert.equal(state.root.style.colorScheme, "light");
    assert.equal(state.stylesheet.media, "(prefers-color-scheme: dark)");
    state.systemDark(true);
    assert.equal(state.root.style.colorScheme, "dark");
    assert.equal(state.saved(), null);
});

test("stored dark preference overrides a light device before the picker is ready", () => {
    const state = setup({ stored: "dark" });
    assert.equal(state.stylesheet.media, "all");
    assert.equal(state.root.style.colorScheme, "dark");
    assert.equal(state.select.value, "dark");
    assert.equal(state.metadata[0].media, "not all");
    assert.equal(state.metadata[1].media, "all");
});

test("stored light preference overrides a dark device", () => {
    const state = setup({ stored: "light", dark: true });
    assert.equal(state.stylesheet.media, "not all");
    assert.equal(state.root.style.colorScheme, "light");
    state.systemDark(false);
    state.systemDark(true);
    assert.equal(state.root.style.colorScheme, "light");
});

test("manual selection persists and system selection restores live alignment", () => {
    const state = setup();
    state.choose("dark");
    assert.equal(state.saved(), "dark");
    assert.equal(state.stylesheet.media, "all");
    state.systemDark(false);
    assert.equal(state.root.style.colorScheme, "dark");
    state.choose("system");
    assert.equal(state.saved(), "system");
    assert.equal(state.root.style.colorScheme, "light");
    state.systemDark(true);
    assert.equal(state.root.style.colorScheme, "dark");
});

test("picker synchronization survives DOM ready and interactive hydration", () => {
    const state = setup({ stored: "dark" });
    state.select.value = "system";
    state.ready();
    assert.equal(state.select.value, "dark");
    state.select.value = "system";
    state.window.seancesTheme.sync();
    assert.equal(state.select.value, "dark");
});

test("blocked browser storage does not prevent in-page theme changes", () => {
    const state = setup({ blocked: true });
    state.choose("dark");
    assert.equal(state.root.style.colorScheme, "dark");
    state.choose("light");
    assert.equal(state.root.style.colorScheme, "light");
});

test("enhanced date navigation reapplies a saved theme after head attributes are restored", () => {
    const state = setup({ stored: "dark" });
    state.ready();
    state.stylesheet.media = "(prefers-color-scheme: dark)";
    state.select.value = "system";
    state.enhancedLoad();
    assert.equal(state.stylesheet.media, "all");
    assert.equal(state.select.value, "dark");
    assert.equal(state.root.style.colorScheme, "dark");
});

test("invalid stored or selected values fall back to system", () => {
    const state = setup({ stored: "neon", dark: true });
    assert.equal(state.root.dataset.theme, "system");
    state.choose("unrecognized");
    assert.equal(state.stylesheet.media, "(prefers-color-scheme: dark)");
    assert.equal(state.saved(), "system");
});

test("schedule filter changes do not change theme preference", () => {
    const state = setup({ stored: "light" });
    state.unrelatedChange();
    assert.equal(state.saved(), "light");
    assert.equal(state.root.dataset.theme, "light");
});
