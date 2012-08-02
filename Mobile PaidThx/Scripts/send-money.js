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

    $("#frmSendMoney").validate({
        submitHandler: function (form) {
            form.submit();
            alert("test");
        }
    });

    $("#gobtn-send").die('click').live('click', function () {
        amountSelected = $('#customAmountSend').val();
        var serviceUrl = getBaseURL() + 'Send';
        $.mobile.changePage(serviceUrl, {
            type: "post",
            data: {index: amountSelected}
        });
    });

    $("#onedollarbtnsend").die('click').live('click', function () {
        amountSelected = "1.00";
        var serviceUrl = getBaseURL() + 'Send';
        $.mobile.changePage(serviceUrl, {
            type: "post",
            data: { index: amountSelected }
        });
    });

    $("#fivedollarbtnsend").die('click').live('click', function () {
        amountSelected = "5.00";
        var serviceUrl = getBaseURL() + 'Send';
        $.mobile.changePage(serviceUrl, {
            type: "post",
            data: { index: amountSelected }
        });
    });

    $("#tendollarbtnsend").die('click').live('click', function () {
        amountSelected = "10.00";
        var serviceUrl = getBaseURL() + 'Send';
        $.mobile.changePage(serviceUrl, {
            type: "post",
            data: { index: amountSelected }
        });
    });

    $("#twentydollarbtnsend").die('click').live('click', function () {
        amountSelected = "20.00";
        var serviceUrl = getBaseURL() + 'Send';
        $.mobile.changePage(serviceUrl, {
            type: "post",
            data: { index: amountSelected }
        });
    });
});
