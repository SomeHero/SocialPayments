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

