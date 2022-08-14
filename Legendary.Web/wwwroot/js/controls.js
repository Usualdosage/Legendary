$(function () {    
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
