var muted = false;

$(function () {
    console.log("jQuery ready.");
    document.getElementById("channel0").volume = .3;
});

function changeBgAudio(e) {
    var newVolume = (e.value / 100);
    document.getElementById("channel0").volume = newVolume;
}

function changeSfxAudio(e) {
    var newVolume = (e.value / 100);
    document.getElementById("channel1").volume = newVolume;
    document.getElementById("channel2").volume = newVolume;
    document.getElementById("channel3").volume = newVolume;
    document.getElementById("channel4").volume = newVolume;
    document.getElementById("channel5").volume = newVolume;
    document.getElementById("channel6").volume = newVolume;
    document.getElementById("channel7").volume = newVolume;
}

function toggleMute(e) {
    muted = !muted;

    document.getElementById("channel0").muted = muted;
    document.getElementById("channel1").muted = muted;
    document.getElementById("channel2").muted = muted;
    document.getElementById("channel3").muted = muted;
    document.getElementById("channel4").muted = muted;
    document.getElementById("channel5").muted = muted;
    document.getElementById("channel6").muted = muted;
    document.getElementById("channel7").muted = muted;

    var muteButton = $("#muteButton");

    if (muted === true) {
        muteButton.removeClass("fa-volume-high").addClass("fa-volume-xmark");
    }
    else {
        muteButton.removeClass("fa-volume-xmark").addClass("fa-volume-high");
    }
}
