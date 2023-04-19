(async function () {
    await readInput();
}());

async function readInput() {
    return new Promise((resolve) => {
        // Event handler for key down.
        document.addEventListener("keydown", onKeyHandler);
        async function onKeyHandler(e) {
            if (e.keyCode === 13) {
                e.preventDefault();

                const messageField = document.getElementById("messageField");
                const messages = document.getElementById("messages");

                let message = messageField.value;

                // Clear out the input.
                messageField.value = "";

                // Create and append the message to the container.
                let div = document.createElement("div");
                div.append(message);
                div.className = "out-message";
                messages.append(div);

                // Autoscroll the container
                messages.scrollTop = messages.scrollHeight;

                // Call the API, and show a "typing" bubble.
                await sendChatMessage(message);

                // Resolve the promise.
                resolve();
            }
        }
    });
}

async function sendChatMessage(message) {
    // Show the typing bubble.
    const messages = document.getElementById("messages");
    let img = document.createElement("img");
    img.className = "typing";
    img.src = "img/typing.gif";
    img.id = "typing";
    messages.append(img);

    // Autoscroll the container
    messages.scrollTop = messages.scrollHeight;

    // Pass the parameters to the API via GET.
    let userName = document.getElementById("usernameInput").value;
    let persona = document.getElementById("personaInput").value;
    const response = await fetch("/Chat?username=" + userName + "&persona=" + persona + "&message=" + message);
    const result = await response.json();

    // Remove the typing bubble.
    document.getElementById("typing").remove();

    // Append the response.
    
    let div = document.createElement("div");
    div.append(result);
    div.className = "in-message";
    messages.append(div);

    // Autoscroll the container
    messages.scrollTop = messages.scrollHeight;
}