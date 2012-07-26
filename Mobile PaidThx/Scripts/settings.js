/// <reference path="jquery-1.5.1-vsdoc.js" />
/// <reference path="jquery.validate-vsdoc.js" />

var getBaseURL = function () {
    return location.protocol + "//" + location.hostname + (location.port && ":" + location.port) + "/mobile/";
}

// USE BELOW
$("#renderP").die('click').live('click', function () {
    $.get(getBaseURL() + "Profile/Settings", function (data) {
        $('#globalbg').html(data).trigger("pagecreate").trigger("refresh");
    });
});

$("#closebtn").die('click').live('click', function () {
    $.get(getBaseURL() + "Profile/ListSettings", function (data) {
        $('#globalbg').html(data).trigger("pagecreate").trigger("refresh");
    });
});

$("#renderBanks").die('click').live('click', function () {
    $.get(getBaseURL() + "PaymentAccount/List", function (data) {
        $('#globalbg').html(data).trigger("pagecreate").trigger("refresh");
    });
});

