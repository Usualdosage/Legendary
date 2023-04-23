$(function () {
    $("#ddlRace").change(function () {
        reroll();
    });
});

function check(input) {
    if (input.value != document.getElementById('password').value) {
        input.setCustomValidity('Passwords must match.');
        return;
    }

    if (input.value.length < 8) {
        input.setCustomValidity('Plassword must be at least 8 characters.');
        return;
    } 

    input.setCustomValidity('');
}

function checkName(input) {

    if (input.value === "") {
        input.setCustomValidity('');
        return;
    }

    var regex = /[a-zA-Z]+/g;
    if (input.value.match(regex) === null) {
        input.setCustomValidity('Your name cannot contain numbers or special characters.');
        return;
    }

    input.setCustomValidity('');
}

function checkDesc(input) {
    if (input.value.length < 100) {
        input.setCustomValidity('Please provide some more detail in your description.');
    } else {
        input.setCustomValidity('');
    }
}

function reroll() {

    var race = $("#ddlRace").val();

    $.ajax({
        type: "Get",
        url: "/Home/UpdateUser?race=" + race,
        success: function (data) {
            console.log(data);

            $("#str").text(data.str);
            $("#int").text(data.int);
            $("#dex").text(data.dex);
            $("#wis").text(data.wis);
            $("#con").text(data.con);

            var aligns = data.alignments;

            var ddlAlign = $("#ddlAlign");

            ddlAlign.find('option')
                .remove()
                .end();

            for (var x = 0; x < aligns.length; x++) {
                ddlAlign.append('<option value="' + aligns[x] + '">' + GetAlign(aligns[x]) + '</option>')
            }
        },
        error: function (response) {
            console.log(response.responseText);
        }
    });
}

function GetAlign(value) {
    switch (value) {
        case 0:
            return "Good";
        case 1:
            return "Neutral";
        case 2:
            return "Evil";
    }
}
