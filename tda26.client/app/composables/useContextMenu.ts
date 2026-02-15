export function useContextMenu() {
    const isOpen = ref(false);
    const position = ref({ x: 0, y: 0 });

    function open(event: MouseEvent) {
        event.preventDefault();
        position.value = { x: event.clientX, y: event.clientY };
        isOpen.value = true;
    }

    function close() {
        isOpen.value = false;
    }

    return {
        isOpen,
        position,
        open,
        close,
    };
}