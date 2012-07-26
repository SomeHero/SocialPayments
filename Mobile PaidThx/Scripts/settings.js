/// <reference path="jquery-1.5.1-vsdoc.js" />
/// <reference path="jquery.validate-vsdoc.js" />

var getBaseURL = function () {
    return location.protocol + "//" + location.hostname + (location.port && ":" + location.port) + "/mobile/";
}

function getProfileScreen() {
    $.get(getBaseURL() + "Profile/Settings", function (data) {
        $('#globalbg').html(data).trigger("pagecreate").trigger("refresh");
    });
}

function getBankAccountsScreen() {
    $.get(getBaseURL() + "PaymentAccount/List", function (data) {
        $('#globalbg').html(data).trigger("pagecreate").trigger("refresh");
    });
}

function closeScreen() {
    $.get(getBaseURL() + "Profile/ListSettings", function (data) {
        $('#globalbg').html(data).trigger("pagecreate").trigger("refresh");
    });
}
