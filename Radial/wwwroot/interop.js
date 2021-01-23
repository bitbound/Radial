window.invokeConfirm = async (message) => {
    return confirm(message);
}

window.invokeAlert = async (message) => {
    alert(message);
}

window.invokePrompt = async (message) => {
    return prompt(message);
}