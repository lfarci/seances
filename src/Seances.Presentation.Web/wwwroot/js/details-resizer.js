export function create(handle) {
    if (!handle?.isConnected) return { dispose() {} };
    const workspace = handle.closest(".calendar-workspace");
    const pane = workspace.querySelector(".screening-details");
    const view = handle.ownerDocument.defaultView;
    const desktop = view.matchMedia("(min-width: 52.001rem)");
    const listeners = new AbortController();
    const options = { signal: listeners.signal };
    let drag = null;

    function limits() {
        const width = workspace.getBoundingClientRect().width;
        const rem = parseFloat(view.getComputedStyle(handle.ownerDocument.documentElement).fontSize);
        return { width, min: Math.min(16 * rem, width * .2), max: width * .25, rem };
    }

    function resize(requested) {
        if (!desktop.matches) return;
        const { width, min, max } = limits();
        if (width <= 0) return;
        const next = Math.min(max, Math.max(min, requested));
        workspace.style.setProperty("--details-width", `${next}px`);
        handle.setAttribute("aria-valuemin", String(Math.round(min / width * 100)));
        handle.setAttribute("aria-valuenow", String(Math.round(next / width * 100)));
        handle.setAttribute("aria-valuetext", `${Math.round(next)} pixels, ${Math.round(next / width * 100)}% of schedule width`);
    }

    function finishDrag() {
        if (!drag) return;
        const pointerId = drag.id;
        drag = null;
        workspace.classList.remove("is-resizing");
        if (handle.hasPointerCapture(pointerId)) handle.releasePointerCapture(pointerId);
    }

    handle.addEventListener("pointerdown", event => {
        if (!desktop.matches || event.button !== 0 || drag) return;
        event.preventDefault();
        handle.focus({ preventScroll: true });
        drag = { id: event.pointerId, x: event.clientX, width: pane.getBoundingClientRect().width };
        handle.setPointerCapture(event.pointerId);
        workspace.classList.add("is-resizing");
    }, options);

    handle.addEventListener("pointermove", event => {
        if (drag?.id === event.pointerId) resize(drag.width + drag.x - event.clientX);
    }, options);

    for (const event of ["pointerup", "pointercancel", "lostpointercapture"])
        handle.addEventListener(event, finishDrag, options);

    handle.addEventListener("keydown", event => {
        if (!desktop.matches) return;
        const step = event.shiftKey ? 48 : 16;
        const width = pane.getBoundingClientRect().width;
        const { min, max } = limits();
        const targets = { ArrowLeft: width + step, ArrowRight: width - step, Home: min, End: max };
        if (!(event.key in targets)) return;
        event.preventDefault();
        event.stopPropagation();
        resize(targets[event.key]);
    }, options);

    handle.addEventListener("dblclick", () => resize(20.5 * limits().rem), options);

    const observer = new ResizeObserver(() => {
        if (!desktop.matches) finishDrag();
        resize(pane.getBoundingClientRect().width);
    });
    observer.observe(workspace);
    resize(pane.getBoundingClientRect().width);

    return {
        dispose() {
            finishDrag();
            listeners.abort();
            observer.disconnect();
        }
    };
}
