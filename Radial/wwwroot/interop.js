window.invokeConfirm = async (message) => {
    return confirm(message);
}

window.invokeAlert = async (message) => {
    alert(message);
}

window.invokePrompt = async (message) => {
    return prompt(message);
}


window.startDraggingY = (elementId, clientY) => {
    var element = document.getElementById(elementId);

    if (!element) {
        return;
    }

    var startTop = Number(clientY);

    function pointerMove(ev) {
        if (Math.abs(ev.clientY - startTop) > 10) {
            if (ev.clientY < 0 || ev.clientY > window.innerHeight - element.clientHeight) {
                return;
            }

            element.style.top = `${ev.clientY}px`;
        }
    }

    function pointerUpOrLeave(ev) {
        window.removeEventListener("pointermove", pointerMove);
        window.removeEventListener("pointerup", pointerUpOrLeave);
        window.removeEventListener("pointerleave", pointerUpOrLeave);
    }

    pointerUpOrLeave();

    window.addEventListener("pointermove", pointerMove);
    window.addEventListener("pointerup", pointerUpOrLeave);
    window.addEventListener("pointerleave", pointerUpOrLeave);
}