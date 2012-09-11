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

function signOut() {
    $.get(getBaseURL() + "Preferences/SignOut", function (data) {
        var homeUrl = getBaseURL() + 'Join';
        $.mobile.changePage(homeUrl);
    });
}

function closeScreen() {
    $.get(getBaseURL() + "Profile/ListSettings", function (data) {
        $('#globalbg').html(data).trigger("pagecreate").trigger("refresh");
    });
}

function returnHome() {
    var homeUrl = getBaseURL() + 'Home';
    $.mobile.changePage(homeUrl);
}

function returnSend() {
    var sendUrl = getBaseURL() + 'Send';
    $.mobile.changePage(sendUrl);
}

function returnRequest() {
    var requestUrl = getBaseURL() + 'Request';
    $.mobile.changePage(requestUrl);
}


function returnDoGood() {
    var doGoodUrl = getBaseURL() + 'DoGood';
    $.mobile.changePage(goGoodUrl);
}

function returnPaystream() {
    var paystreamUrl = getBaseURL() + 'Paystream';
    $.mobile.changePage(paystreamUrl);
}

function returnPref() {
    var prefUrl = getBaseURL() + 'Preferences';
    $.mobile.changePage(prefUrl);
}


$(document).ready(function () {

    var dataUrl = getBaseURL() + 'Preferences/ReturnData';
    var serviceUrl = getBaseURL() + 'Preferences/PaypointInfo';

    $("#addPaypointBtn").die('click').live('click', function () {
        var paypointUri = $("#paypointName").val();
        var paypointType = $("#paypointType").val();

        var removePaypointUrl = getBaseURL() + 'Preferences/AddPaypoint';
        var settingsUrl = getBaseURL() + 'Preferences';

        var paypointModel = {
            Uri: paypointUri,
            Type: paypointType
        };

        var jsonData = $.toJSON(paypointModel);

        $.ajax({
            type: 'POST',
            url: removePaypointUrl,
            data: jsonData,
            contentType: "application/json",
            dataType: "json",
            processData: false,
            success: function (data) {
                $.mobile.changePage(settingsUrl);
            },
            error: function (objRequest, next, errorThrown) {
                alert(next);
                $("#error-block").appendTo(next);
            }
        });
    });

    $("#removePaypointBtn").die('click').live('click', function () {

        var paypointId = $("#paypointIdValue").val();
        var removePaypointUrl = getBaseURL() + 'Preferences/RemovePaypoint';
        var settingsUrl = getBaseURL() + 'Preferences';

        var paypointModel = {
            Id: paypointId
        };

        var jsonData = $.toJSON(paypointModel);

        $.ajax({
            type: 'POST',
            url: removePaypointUrl,
            data: jsonData,
            contentType: "application/json",
            dataType: "json",
            processData: false,
            success: function (data) {
                $.mobile.changePage(settingsUrl);
            },
            error: function (objRequest, next, errorThrown) {
                alert(next);
                $("#error-block").appendTo(next);
            }
        });
    });

    $(".mecode-btn").die('click').live('click', function () {
        var orgId = $(this).attr('data-value');
        var orgName = $(this).attr('organization-name');

        var paypointModel = {
            Id: orgId,
            Uri: orgName
        };

        var jsonData = $.toJSON(paypointModel);

        $.ajax({
            type: 'POST',
            url: dataUrl,
            data: jsonData,
            contentType: "application/json",
            dataType: "json",
            processData: false,
            success: function (data) {
                $.mobile.changePage(serviceUrl, {
                    type: "put",
                    data: data
                });
            },
            error: function (objRequest, next, errorThrown) {
                alert(next);
                $("#error-block").appendTo(next);
            }
        });
    });

    $(".phoneobj-btn").die('click').live('click', function () {
        var orgId = $(this).attr('data-value');
        var orgName = $(this).attr('organization-name');

        var paypointModel = {
            Id: orgId,
            Uri: orgName
        };

        var jsonData = $.toJSON(paypointModel);

        $.ajax({
            type: 'POST',
            url: dataUrl,
            data: jsonData,
            contentType: "application/json",
            dataType: "json",
            processData: false,
            success: function (data) {
                $.mobile.changePage(serviceUrl, {
                    type: "put",
                    data: data
                });
            },
            error: function (objRequest, next, errorThrown) {
                alert(next);
                $("#error-block").appendTo(next);
            }
        });
    });

    $(".emailobj-btn").die('click').live('click', function () {
        var orgId = $(this).attr('data-value');
        var orgName = $(this).attr('organization-name');

        var paypointModel = {
            Id: orgId,
            Uri: orgName
        };

        var jsonData = $.toJSON(paypointModel);

        $.ajax({
            type: 'POST',
            url: dataUrl,
            data: jsonData,
            contentType: "application/json",
            dataType: "json",
            processData: false,
            success: function (data) {
                $.mobile.changePage(serviceUrl, {
                    type: "put",
                    data: data
                });
            },
            error: function (objRequest, next, errorThrown) {
                alert(next);
                $("#error-block").appendTo(next);
            }
        });
    });

    $(".addnew-btn").die('click').live('click', function () {

        var addNewUrl = getBaseURL() + 'Preferences/AddPaypoint';

        var orgName = $(this).attr('organization-name');

        var paypointModel = {
            Uri: orgName
        };

        var jsonData = $.toJSON(paypointModel);

        $.ajax({
            type: 'POST',
            url: dataUrl,
            data: jsonData,
            contentType: "application/json",
            dataType: "json",
            processData: false,
            success: function (data) {
                $.mobile.changePage(addNewUrl, {
                    type: "put",
                    data: data
                });
            },
            error: function (objRequest, next, errorThrown) {
                alert(next);
                $("#error-block").appendTo(next);
            }
        });
    });
});