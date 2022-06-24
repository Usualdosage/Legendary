
class LegacyClient {
    constructor() {
    }

    Connect() {
        var protocol = location.protocol === "https:" ? "wss:" : "ws:";
        var wsUri = protocol + "//" + window.location.host;
        var socket = new WebSocket(wsUri);
        let commands = [];

        socket.onopen = e => {
            console.log("Connected to Legendary!", e);
        };

        socket.onclose = function (e) {
            console.log("Disconnected.", e);
        };

        socket.onerror = function (e) {
            console.error("Error!", e);
        };

        socket.onmessage = function (e) {

            var $roomImage = $(".loader").remove();

            var $console = $("#console");

            $console.append("<span class='message'>" + e.data + "</span>");

            // Autoscroll

            $console.prop("scrollTop", $console.prop("scrollHeight"));

            // Autotrim if there are more than 250 messages

            var $remove = $(".message");

            if ($remove.length > 250) {
                $remove[0].remove();
            }

            // Remove all but 1 of the room images (they stack)

            var $roomImage = $(".room-image");

            for (var x = 0; x < $roomImage.length - 1; x++) {
                $roomImage[x].remove();
            }

            // Remove all but 1 of the player info panels (they stack)

            var $playerInfo = $(".player-info");

            for (var x = 0; x < $playerInfo.length - 1; x++) {
                $playerInfo[x].remove();
            }
        };

        $("#inputField").click(function (e) {
            $(this).select();
        });

        $('#inputField').keypress(function (e) {

            if (e.which != 13) {
                return;
            }

            e.preventDefault();

            var message = $('#inputField').val();
            socket.send(message);

            commands.push(message);

            $(this).select();
        });
    }
}