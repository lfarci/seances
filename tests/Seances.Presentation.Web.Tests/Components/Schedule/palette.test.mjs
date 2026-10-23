import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import { test } from "node:test";

const css = await readFile(new URL("../../../../src/Seances.Presentation.Web/wwwroot/app.css", import.meta.url), "utf8");
const palettes = [...css.matchAll(/:root\s*\{([^}]+)\}/g)].map(match =>
    Object.fromEntries([...match[1].matchAll(/--([\w-]+):\s*(#[\da-f]{6});/g)].map(([, name, value]) => [name, value])));

function luminance(hex) {
    const rgb = hex.slice(1).match(/../g).map(value => parseInt(value, 16) / 255)
        .map(value => value <= .04045 ? value / 12.92 : ((value + .055) / 1.055) ** 2.4);
    return rgb[0] * .2126 + rgb[1] * .7152 + rgb[2] * .0722;
}

function contrast(first, second) {
    const values = [luminance(first), luminance(second)].sort((a, b) => b - a);
    return (values[0] + .05) / (values[1] + .05);
}

for (const [index, palette] of palettes.entries()) {
    const theme = index === 0 ? "light" : "dark";
    test(`${theme} reading and action colors retain WCAG AA text contrast`, () => {
        for (const surface of ["canvas", "panel", "paper", "row-alternate", "time-header", "details-paper", "hover", "accent-soft"]) {
            for (const text of ["ink", "muted", "accent-dark"]) {
                const ratio = contrast(palette[text], palette[surface]);
                assert.ok(ratio >= 4.5, `${text} on ${surface}: ${ratio.toFixed(2)}:1`);
            }
        }
        for (const surface of ["accent", "accent-dark"]) {
            assert.ok(contrast(palette["on-accent"], palette[surface]) >= 4.5);
        }
        assert.ok(contrast(palette["notice-ink"], palette["notice-paper"]) >= 4.5);
    });

    test(`${theme} control, card, and focus indicators have non-text contrast`, () => {
        for (const surface of ["paper", "row-alternate", "details-paper"]) {
            for (const indicator of ["control-line", "card-line", "focus"]) {
                const ratio = contrast(palette[indicator], palette[surface]);
                assert.ok(ratio >= 3, `${indicator} on ${surface}: ${ratio.toFixed(2)}:1`);
            }
        }
    });
}

test("both palettes are defined and dark appearance follows the device preference", () => {
    assert.equal(palettes.length, 2);
    assert.match(css, /color-scheme:\s*light dark/);
    assert.match(css, /@media\s*\(prefers-color-scheme:\s*dark\)\s*\{\s*:root/);
});
