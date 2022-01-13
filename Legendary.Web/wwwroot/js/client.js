
class LegacyClient {
    constructor() {
    }

    Connect() {
        var protocol = location.protocol === "https:" ? "wss:" : "ws:";
        var wsUri = protocol + "//" + window.location.host;
        var socket = new WebSocket(wsUri);

        socket.onopen = e => {
            console.log("Connected!", e);
        };

        socket.onclose = function (e) {
            console.log("Disconnected.", e);
        };

        socket.onerror = function (e) {
            console.error(e.data);
        };

        socket.onmessage = function (e) {

            var $console = $("#console");

            $console.append("<span class='message'>" + e.data + "</span>");

            // Autoscroll
            $console.prop("scrollTop", $console.prop("scrollHeight"));

            // Autotrim if there are more than 250 messages
            var $remove = $(".message");

            if ($remove.length > 250) {
                $remove[0].remove();
            }
        };

        $('#inputField').keypress(function (e) {
            if (e.which != 13) {
                return;
            }

            e.preventDefault();

            var message = $('#inputField').val();
            socket.send(message);

            $('#inputField').val('');
        });
    }
}