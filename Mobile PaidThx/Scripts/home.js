/// <reference path="jquery-1.5.1-vsdoc.js" />
/// <reference path="jquery.validate-vsdoc.js" />
$(document).ready(function () {
    var header = $('#header');
    var getBaseURL = function () {
        return location.protocol + "//" + location.hostname + (location.port && ":" + location.port) + "/";
    }
    $.get(getBaseURL() + "mobile/Home/GetMobileNumberModal", function (data) {
        $('#modal').html(data).trigger("pagecreate").trigger("refresh");
    });
});