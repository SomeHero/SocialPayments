/// <reference path="jquery-1.5.1-vsdoc.js" />
/// <reference path="jquery.validate-vsdoc.js" />
var getBaseURL = function () {
    return location.protocol + "//" + location.hostname + (location.port && ":" + location.port) + "/mobile/";
}

function formatAmount(amount) {
    return "$" + amount.toFixed(2);
}

$(document).ready(function () {
    var amountSelected = 0.0;
    $("#frmRequestMoney").validate({
        submitHandler: function (form) {
            form.submit();
        }
    });

    $("#gobtn-request").die('click').live('click', function () {
        amountSelected = $('#customAmountRequest').val();
        var serviceUrl = getBaseURL() + 'Request';
        $.mobile.changePage(serviceUrl, {
            type: "post",
            data: { index: amountSelected }
        });
    });

    $("#onedollarbtnrequest").die('click').live('click', function () {
        amountSelected = "1.00";
        var serviceUrl = getBaseURL() + 'Request';
        $.mobile.changePage(serviceUrl, {
            type: "post",
            data: { index: amountSelected }
        });
    });

    $("#fivedollarbtnrequest").die('click').live('click', function () {
        amountSelected = "5.00";
        var serviceUrl = getBaseURL() + 'Request';
        $.mobile.changePage(serviceUrl, {
            type: "post",
            data: { index: amountSelected }
        });
    });

    $("#tendollarbtnrequest").die('click').live('click', function () {
        amountSelected = "10.00";
        var serviceUrl = getBaseURL() + 'Request';
        $.mobile.changePage(serviceUrl, {
            type: "post",
            data: { index: amountSelected }
        });
    });

    $("#twentydollarbtnrequest").die('click').live('click', function () {
        amountSelected = "20.00";
        var serviceUrl = getBaseURL() + 'Request';
        $.mobile.changePage(serviceUrl, {
            type: "post",
            data: { index: amountSelected }
        });
    });
});

