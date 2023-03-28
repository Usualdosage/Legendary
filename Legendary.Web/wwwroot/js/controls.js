$(function () {

    var volume = document.getElementById("channel0");

    if (volume) {
        volume.volume = .3;
    }

    Notification.requestPermission().then((result) => {
        console.log(result);
    });
});

function displayNotification(img, text) {
    const notification = new Notification('Legendary', { body: text, icon: img });
}

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

function autocomplete(inp, arr) {
    var currentFocus;
    inp.addEventListener("input", function (e) {
        var a, b, i, val = this.value;
        closeAllLists();
        if (!val) { return false; }
        currentFocus = -1;
        a = document.createElement("DIV");
        a.setAttribute("id", this.id + "autocomplete-list");
        a.setAttribute("class", "autocomplete-items");
        this.parentNode.appendChild(a);
        for (i = 0; i < arr.length; i++) {
            if (arr[i].substr(0, val.length).toUpperCase() == val.toUpperCase()) {
                b = document.createElement("DIV");
                b.innerHTML = "<strong>" + arr[i].substr(0, val.length) + "</strong>";
                b.innerHTML += arr[i].substr(val.length);
                b.innerHTML += "<input type='hidden' value='" + arr[i] + "'>";
                b.addEventListener("click", function (e) {

                    let messageTo = document.getElementById("message-to");
                    let value = this.getElementsByTagName("input")[0].value;
                    let span = document.createElement("span");
                    span.className = "email-address";
                    span.innerText = value;

                    span.addEventListener("click", function (e) {
                        e.currentTarget.remove();
                    });

                    messageTo.appendChild(span);

                    inp.value = '';
                    inp.focus();

                    closeAllLists();
                });
                a.appendChild(b);
            }
        }
    });
    inp.addEventListener("keydown", function (e) {
        var x = document.getElementById(this.id + "autocomplete-list");
        if (x) x = x.getElementsByTagName("div");
        if (e.keyCode == 40) {
            currentFocus++;
            addActive(x);
        } else if (e.keyCode == 38) { //up
            currentFocus--;
            addActive(x);
        } else if (e.keyCode == 13) {
            e.preventDefault();
            if (currentFocus > -1) {
                if (x) x[currentFocus].click();
            }
        }
    });
    function addActive(x) {
        if (!x) return false;
        removeActive(x);
        if (currentFocus >= x.length) currentFocus = 0;
        if (currentFocus < 0) currentFocus = (x.length - 1);
        x[currentFocus].classList.add("autocomplete-active");
    }
    function removeActive(x) {
        for (var i = 0; i < x.length; i++) {
            x[i].classList.remove("autocomplete-active");
        }
    }
    function closeAllLists(elmnt) {
        var x = document.getElementsByClassName("autocomplete-items");
        for (var i = 0; i < x.length; i++) {
            if (elmnt != x[i] && elmnt != inp) {
                x[i].parentNode.removeChild(x[i]);
            }
        }
    }

    document.addEventListener("click", function (e) {
        closeAllLists(e.target);
    });
}
