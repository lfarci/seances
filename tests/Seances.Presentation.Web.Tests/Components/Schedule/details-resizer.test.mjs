import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import { test } from "node:test";

const source = await readFile(new URL("../../../../src/Seances.Presentation.Web/wwwroot/js/details-resizer.js", import.meta.url));
const { create } = await import(`data:text/javascript;base64,${source.toString("base64")}`);

function setup(width = 2000, desktop = true) {
    const properties = new Map();
    const classes = new Set();
    const attributes = new Map();
    const captures = new Set();
    const workspace = {
        width,
        style: { setProperty: (key, value) => properties.set(key, value) },
        classList: { add: key => classes.add(key), remove: key => classes.delete(key) },
        getBoundingClientRect: () => ({ width: workspace.width }),
        querySelector: () => pane
    };
    const pane = {
        getBoundingClientRect: () => ({
            width: Math.min(workspace.width * .25, Math.max(Math.min(256, workspace.width * .2),
                parseFloat(properties.get("--details-width") ?? "328")))
        })
    };
    let observer;
    globalThis.ResizeObserver = class {
        constructor(callback) { this.callback = callback; observer = this; }
        observe() {}
        disconnect() { this.disconnected = true; }
    };
    const handle = Object.assign(new EventTarget(), {
        isConnected: true,
        closest: () => workspace,
        ownerDocument: { documentElement: {}, defaultView: {
            matchMedia: () => ({ matches: desktop }),
            getComputedStyle: () => ({ fontSize: "16px" })
        } },
        setAttribute: (key, value) => attributes.set(key, value),
        focus() {},
        setPointerCapture: id => captures.add(id),
        hasPointerCapture: id => captures.has(id),
        releasePointerCapture: id => captures.delete(id)
    });
    const resizer = create(handle);
    const dispatch = (type, values = {}) => {
        const event = Object.assign(new Event(type, { cancelable: true }), values);
        handle.dispatchEvent(event);
        return event;
    };
    return { resizer, workspace, observer, dispatch, properties, classes, attributes, captures,
        width: () => parseFloat(properties.get("--details-width")) };
}

test("initial width and viewport changes never exceed 25 percent", () => {
    const state = setup(1000);
    assert.equal(state.width(), 250);
    assert.equal(state.attributes.get("aria-valuenow"), "25");
    state.workspace.width = 900;
    state.observer.callback();
    assert.equal(state.width(), 225);
    state.resizer.dispose();
});

test("dragging clamps both ends and releases pointer capture", () => {
    const state = setup(1500);
    state.dispatch("pointerdown", { button: 0, pointerId: 1, clientX: 1172 });
    assert.equal(state.captures.has(1), true);
    state.dispatch("pointermove", { pointerId: 1, clientX: 0 });
    assert.equal(state.width(), 375);
    state.dispatch("pointermove", { pointerId: 1, clientX: 2000 });
    assert.equal(state.width(), 256);
    state.dispatch("pointerup", { pointerId: 1 });
    assert.equal(state.captures.size, 0);
    assert.equal(state.classes.has("is-resizing"), false);
    state.resizer.dispose();
});

test("keyboard resizing, limits, and double-click reset", () => {
    const state = setup();
    assert.equal(state.dispatch("keydown", { key: "ArrowLeft" }).defaultPrevented, true);
    assert.equal(state.width(), 344);
    state.dispatch("keydown", { key: "ArrowRight" });
    assert.equal(state.width(), 328);
    state.dispatch("keydown", { key: "ArrowLeft", shiftKey: true });
    assert.equal(state.width(), 376);
    state.dispatch("keydown", { key: "Home" });
    assert.equal(state.width(), 256);
    state.dispatch("keydown", { key: "End" });
    assert.equal(state.width(), 500);
    state.dispatch("dblclick");
    assert.equal(state.width(), 328);
    state.resizer.dispose();
});

test("disposing while dragging cleans up listeners and observers", () => {
    const state = setup();
    state.dispatch("pointerdown", { button: 0, pointerId: 1, clientX: 1000 });
    state.resizer.dispose();
    assert.equal(state.classes.size, 0);
    assert.equal(state.captures.size, 0);
    assert.equal(state.observer.disconnected, true);
    state.dispatch("keydown", { key: "End" });
    assert.equal(state.width(), 328);
});

test("mobile drawers do not resize", () => {
    const state = setup(390, false);
    state.dispatch("pointerdown", { button: 0, pointerId: 1, clientX: 100 });
    state.dispatch("keydown", { key: "End" });
    assert.equal(state.properties.size, 0);
    assert.equal(state.captures.size, 0);
    state.resizer.dispose();
});
