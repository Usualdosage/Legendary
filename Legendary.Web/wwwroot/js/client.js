﻿
class LegendaryClient {

    constructor() {

    }

    Connect() {
        var protocol = location.protocol === "https:" ? "wss:" : "ws:";
        var wsUri = protocol + "//" + window.location.host;
        var socket = new WebSocket(wsUri);
        let commandIndex = 0;
        let commands = [];
        const $console = $("#console");
        var updateUI = true;
 
        socket.onopen = e => {
            console.log("Created a secure connection to Legendary.", e);
        };

        socket.onclose = function (e) {
            console.log("Disconnected.", e);
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
                                if (channel === 0) {
                                    audioElem.volume = 0.2;
                                }
                                else {
                                    audioElem.volume = 1.0;
                                }
                            }).catch(error => {
                                // Autoplay was prevented, no sound for you.
                            });
                        }
                    }
                    else {
                        return;
                    }
                }
                return;
            }
            else if (message.startsWith("{"))
            {
                // Parse the incoming message.
                var context = JSON.parse(message);

                // Get the template.
                var template = $('#component-template').html();

                // Compile it.
                var templateScript = Handlebars.compile(template);

                // Generate the dynamic content.
                var html = templateScript(context);

                // Apply content to control panel.
                $("#control-panel").empty().append(html);

                $(".loader").hide();

                // Bind the nav buttons.
                bindNav();

                // Bind the tooltips.
                $('[data-toggle="tooltip"]').tooltip({ boundary: 'window', placement: 'left' });
            }
            else {

                $console.append("<span class='message'>" + message + "</span>");

                // Autoscroll
                $console.prop("scrollTop", $console.prop("scrollHeight"));

                // Autotrim if there are more than 500 messages
                var $remove = $(".message");

                if ($remove.length > 500) {
                    $remove[0].remove();
                }
            }
        };

        $("#inputField").on("click", function (e) {
            $(this).select();
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

        var bindNav = function () {
            $(".nav-arrow").on("click", function (e) {
                socket.send($(this).attr("move"));
            })
        }
    }
}