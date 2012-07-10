/// <reference path="jquery-1.5.1-vsdoc.js" />
/// <reference path="jquery.validate-vsdoc.js" />
$(document).ready(function () {
    var header = $('#header');
    var getBaseURL = function () {
        return location.protocol + "//" + location.hostname + (location.port && ":" + location.port) + "/";
    }
    $.get(getBaseURL() + "mobile/Profile/CompleteProfileDialog/", function (data) {
            $('#profile-content').html(data).trigger("pagecreate").trigger("refresh");
    });
    $("#complete-profile-close").die('click').live('click', function () {
        $.get(getBaseURL() + "mobile/Profile/Settings", function (data) {
            $('#profile-content').html(data).trigger("pagecreate").trigger("refresh");
        });
    });
});