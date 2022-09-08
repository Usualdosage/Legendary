$(function () {
    $("#ddlRace").change(function () {
        reroll();
    });
});

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
