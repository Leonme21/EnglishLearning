const connections = new Map();

export function attach(id, chat) {
    if (!chat || !window.visualViewport || connections.has(id)) return;
    const shell = chat.closest(".tutor-shell");
    if (!shell) return;

    shell.dataset.tutorViewportOwner = id;
    const viewport = window.visualViewport;
    const resize = () => {
        if (shell.dataset.tutorViewportOwner !== id) return;
        if (viewport.scale === 1) {
            shell.style.setProperty("--tutor-viewport-height", `${Math.round(viewport.height)}px`);
        } else {
            shell.style.removeProperty("--tutor-viewport-height");
        }
    };
    viewport.addEventListener("resize", resize);
    connections.set(id, () => {
        viewport.removeEventListener("resize", resize);
        if (shell.dataset.tutorViewportOwner === id) {
            delete shell.dataset.tutorViewportOwner;
            shell.style.removeProperty("--tutor-viewport-height");
        }
    });
    resize();
}

export function detach(id) {
    connections.get(id)?.();
    connections.delete(id);
}
