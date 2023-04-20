class LegendaryClient {

    constructor() {
        
    }

    Connect() {
        var protocol = location.protocol === "https:" ? "wss:" : "ws:";
        var wsUri = protocol + "//" + window.location.host;
        const socket = new WebSocket(wsUri);
        let commandIndex = 0;
        let commands = [];
        const $console = $("#console");
        var updateUI = true;
        var isScrolling = false;
        var lastScrollTop = 0;

        window.onfocus = function () {
            if (document.title.indexOf("*") > -1) {
                document.title = document.title.replace("*", "");
            }
        }

        socket.onopen = e => {
            console.log("Created a secure connection to Legendary.", e);
        };

        socket.onclose = function (e) {
            console.log("Disconnected. Reconnecting in 2 seconds...", e);
            setTimeout(
                function () { 
                    const client = new LegendaryClient();
                    client.Connect();
                }, 2000);
        };

        socket.onerror = function (e) {
            console.error("Error!", e);
        };

        socket.onmessage = function (e) {

            var message = e.data.toString();

            if (message.startsWith("[AUDIO]")) {
            
                var audioParts = message.split('|');
                var channel = audioParts[1];
                var sound = audioParts[2];

                var audioChannel = "channel" + channel;
                var audioElem = document.getElementById(audioChannel);
                audioElem.addEventListener("ended", () => {
                    audioElem.currentTime = 0;       
                });

                if (audioElem != null) {
                    // If it's not playing, go ahead and start.
                    if (audioElem.currentTime === 0) {
                        audioElem.setAttribute("src", sound);

                        var promise = audioElem.play();

                        if (promise !== undefined) {
                            promise.then(_ => {
                                console.log("Playing background audio.")
                            }).catch(error => {
                                // Autoplay was prevented, no sound for you.
                                console.log("Autoplay of audio has been disabled.")
                            });
                        }
                    }
                    else {
                        return;
                    }
                }
                return;
            }
            else if (message.startsWith("[NOTIFICATION]"))
            {
                if (!document.hasFocus()) {
                    document.title = "*" + document.title;
                }

                var messageParts = message.split('|');
                var img = messageParts[1];
                var text = messageParts[2];

                displayNotification(img, text);
            }
            else if (message.startsWith("CLEARCHAT"))
            {
                var mobileId = message.split(":")[1];
                $(".chat-bubble-" + mobileId).remove();
                isScrolling = false;
                $console.prop("scrollTop", $console.prop("scrollHeight"));
            }
            else if (message.startsWith("{"))
            {
                // Parse the incoming message.
                var context = JSON.parse(message);

                if (context.type === "update") {

                    // Get the template.
                    var template = $('#component-template').html();

                    // Compile it.
                    var templateScript = Handlebars.compile(template);

                    // Generate the dynamic content.
                    var html = templateScript(context);

                    // Apply content to control panel.
                    $("#control-panel").empty().append(html);

                    $(".loader").hide();

                    // Generate the mini-map.
                    renderMiniMap(context.m.m);
                }
                else if (context.type === "score") {
                    // Get the template.
                    var template = $('#score-template').html();

                    // Compile it.
                    var templateScript = Handlebars.compile(template);

                    // Generate the dynamic content.
                    var html = templateScript(context);

                    // Apply content to control panel.
                    $("#console").append(html);
                }
                else {
                    return;
                }
            }
            else if (message.startsWith("<video"))
            {
                $console.append("<span class='message'>" + message + "</span>");

                // Stop playing the other audio if the user is watching a video.
                const videoElement = document.querySelector("video");
                videoElement.addEventListener("play", function () {
                    const audioElements = document.getElementsByTagName('audio');
                    for (const audio of audioElements) {
                        audio.pause();
                    }
                    console.log("All background audio paused.");
                });

                videoElement.addEventListener("ended", function () {
                    const audioElements = document.getElementsByTagName('audio');
                    for (const audio of audioElements) {
                        audio.play();
                    }
                    console.log("All background audio un-paused.");
                });
            }
            else {
                $console.append("<span class='message'>" + message + "</span>");
            }

            // Autoscroll
            if (!isScrolling) {
                $console.prop("scrollTop", $console.prop("scrollHeight"));
            }

            // Autotrim if there are more than 500 messages
            var $remove = $(".message");

            if ($remove.length > 500) {
                $remove[0].remove();
            }
        };

        $console.on("scroll", function (e) {
            var st = $(this).scrollTop();
            if (st > lastScrollTop) {
                isScrolling = false;
            } else {
                // Allow scrolling back through the console.
                isScrolling = true;
            }
            lastScrollTop = st;
        });
        
        $("#inputField").on("click", function (e) {
            isScrolling = false;
            $(this).select();
            $console.prop("scrollTop", $console.prop("scrollHeight"));
        });

        $("#inputField").on("keydown", function (e) {

            if (e.which === 38) {
                if (commands.length === 0) {
                    return;
                }
                else {

                    e.preventDefault();
                    e.stopPropagation();

                    let command = commands[commandIndex];

                    if (command) {
                        commandIndex--;
                    }
                    else {
                        commandIndex = commands.length - 1;
                        command = commands[commandIndex]
                    }

                    $('#inputField').val(command);
                    $('#inputField').select();
                }
            }
        });

        $("#inputField").on("keypress", function (e) {

            if (e.which != 13) {
                return;
            }

            e.preventDefault();

            var message = $('#inputField').val();
            socket.send(message);

            commands.push(message);

            commandIndex = commands.length - 2;

            $(this).select();
        });

        this.socket = socket;

        // Mini-map operations
        const canvas = document.getElementById("mini-map");
        const ctx = canvas.getContext("2d");
        const roomWidth = 15;
        const roomHeight = 7;

        var renderMiniMap = function (mapData) {
            let currentRoom = mapData.c;
            let rooms = mapData.r;
            let playersInArea = mapData.p;
            let mobsInArea = mapData.m;

            var explored = document.getElementById("span_explored");
            explored.innerText = Math.round(mapData.x);

            ctx.clearRect(0, 0, canvas.width, canvas.height);

            for (let x = 0, room; room = rooms[x]; x++) {

                var positionX = ((room.RoomId - room.AreaId) % 20) * roomWidth;
                var positionY = Math.floor((room.RoomId - room.AreaId) / 20) * roomHeight;

                var mobInRoom = mobsInArea.find(m => m.Value == room.RoomId);
                var playerInRoom = playersInArea.find(p => p.Value == room.RoomId);

                if (room.RoomId == currentRoom) {
                    renderRoom(positionX, positionY, "blue", true, false, false);
                }
                else {
                    renderRoom(positionX, positionY, "#999999", false, mobInRoom != null, playerInRoom != null);
                }
            }
        }

        var renderRoom = function (x, y, color, fill, hasMob, hasPlayer) {
            ctx.beginPath();
            ctx.lineWidth = "1";
            if (fill) {
                ctx.fillStyle = color;
                ctx.fillRect(x, y, roomWidth, roomHeight);
                ctx.strokeStyle = "#999999";
                ctx.rect(x, y, roomWidth, roomHeight);
                ctx.stroke();
            }
            else {
                ctx.fillStyle = "#444444";
                ctx.fillRect(x, y, roomWidth, roomHeight);
                ctx.rect(x, y, roomWidth, roomHeight);
                ctx.strokeStyle = color;
                ctx.stroke();
            }

            if (hasMob) {
                ctx.fillStyle = "#FFFFFF";
                ctx.fillRect(x + (roomWidth / 2 - 2), y + roomHeight / 2, 2, 1);
            }

            if (hasPlayer) {
                ctx.fillStyle = "red";
                ctx.fillRect(x + (roomWidth / 2 + 2), y + roomHeight / 2, 2, 1);
            }
        };
    }

    SendMessage() {
        let toList = $(".email-address");

        let validator = document.getElementById("validator");

        validator.style.display = "none";

        if (toList.length === 0) {
            validator.innerText = 'You must provide at least one recipient.';
            validator.style.display = "block";
            return;
        }

        let messageSubject = document.getElementById('message-subject');

        if (messageSubject.value === '') {
            validator.innerText = 'You must provide a subject.';
            validator.style.display = "block";
            return;
        }

        let messageContent = tinymce.get("message-body").getContent();

        if (messageContent === '') {
            validator.innerText = 'You must provide message content.';
            validator.style.display = "block";
            return;
        }

        let toAddresses = [];

        toList.toArray().map(m => { toAddresses.push(m.innerText); });

        var data = {
            FromAddress: $("#playerName").val(),
            ToAddresses: toAddresses,
            Subject: messageSubject.value,
            Content: messageContent
        };

        fetch("/Message", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify(data),
        })
        .then((response) => response.json())
        .then((data) => {
            let messageModal = document.getElementById('messageModal');
            let modal = bootstrap.Modal.getInstance(messageModal)
            modal.hide();
            $("#console").append("<span class='message'>Your hand your message to a messenger and send them on their way.</span>");
            this.socket.send(JSON.stringify(data));
        })
        .catch((error) => {
            validator.innerText = 'There was a problem sending your message. Please try again later.';
            validator.style.display = "block";
            console.error("Error:", error);
        });
    }
}