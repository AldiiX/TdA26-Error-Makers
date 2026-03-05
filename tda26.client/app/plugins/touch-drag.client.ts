/**
 * Touch-to-drag polyfill.
 *
 * Translates touch events that start on [data-drag-handle] elements into the
 * standard HTML5 DragEvent sequence (dragstart → dragover/dragleave → drop →
 * dragend).  This lets the existing mouse-based drag-and-drop logic work on
 * mobile/touch devices without any changes to the business logic.
 */
export default defineNuxtPlugin(() => {
    if (typeof document === 'undefined') return;

    /** Minimal DataTransfer polyfill (only the parts the app uses). */
    class FakeDT {
        private _store: Record<string, string> = {};
        types: string[] = [];
        effectAllowed = 'uninitialized';
        dropEffect = 'none';

        setData(type: string, data: string): void {
            this._store[type] = data;
            if (!this.types.includes(type)) this.types.push(type);
        }

        getData(type: string): string {
            return this._store[type] ?? '';
        }

        clearData(type?: string): void {
            if (type) {
                delete this._store[type];
                this.types = this.types.filter(t => t !== type);
            } else {
                this._store = {};
                this.types = [];
            }
        }
    }

    /** Fires a synthetic DragEvent with our fake DataTransfer injected. */
    function fire(type: string, target: Element, touch: Touch): boolean {
        const ev = new DragEvent(type, {
            bubbles: true,
            cancelable: true,
            clientX: touch.clientX,
            clientY: touch.clientY,
            screenX: touch.screenX,
            screenY: touch.screenY,
        });
        Object.defineProperty(ev, 'dataTransfer', { value: fdt, configurable: true, writable: false });
        return target.dispatchEvent(ev);
    }

    let fdt: FakeDT | null = null;
    let dragSrc: Element | null = null;
    let prevOver: Element | null = null;
    let active = false;

    // ── touchstart ──────────────────────────────────────────────────────────
    // Detect whether the touch started on a drag handle.  We deliberately do
    // NOT call preventDefault() here so that normal tap/click interactions on
    // other elements are not affected.
    document.addEventListener('touchstart', (e: TouchEvent) => {
        // Ignore multi-finger gestures (pinch-zoom etc.)
        if (e.touches.length > 1) return;

        const handle = (e.target as Element).closest('[data-drag-handle]');
        if (!handle) return;

        const draggable = handle.closest('[draggable="true"]');
        if (!draggable) return;

        dragSrc = draggable;
        fdt = new FakeDT();
        prevOver = null;
        active = false;
    }, { passive: true });

    // ── touchmove ───────────────────────────────────────────────────────────
    document.addEventListener('touchmove', (e: TouchEvent) => {
        if (!dragSrc || !fdt) return;

        // Prevent the page from scrolling while a drag is in progress.
        e.preventDefault();

        const touch = e.touches[0];
        if (!touch) return;

        // Fire dragstart on the very first move after the handle was pressed.
        if (!active) {
            active = true;
            fire('dragstart', dragSrc, touch);
        }

        const target = document.elementFromPoint(touch.clientX, touch.clientY);

        // Fire dragleave / dragover transition when the finger moves between elements.
        if (target !== prevOver) {
            if (prevOver) fire('dragleave', prevOver, touch);
            prevOver = target;
        }
        if (target) fire('dragover', target, touch);
    }, { passive: false });

    // ── touchend ────────────────────────────────────────────────────────────
    document.addEventListener('touchend', (e: TouchEvent) => {
        if (!dragSrc || !fdt) return;

        const touch = e.changedTouches[0];

        if (active && touch) {
            const dropTarget = document.elementFromPoint(touch.clientX, touch.clientY);
            if (dropTarget) {
                // dispatchEvent returns false when preventDefault() was called,
                // which signals that the drop was accepted.
                const accepted = !fire('drop', dropTarget, touch);
                if (accepted) fdt.dropEffect = 'move';
            }
            fire('dragend', dragSrc, touch);
        }

        dragSrc = null;
        fdt = null;
        prevOver = null;
        active = false;
    }, { passive: true });

    // ── touchcancel ─────────────────────────────────────────────────────────
    // Clean up state if the touch sequence is cancelled (e.g. incoming call).
    document.addEventListener('touchcancel', () => {
        dragSrc = null;
        fdt = null;
        prevOver = null;
        active = false;
    }, { passive: true });
});
