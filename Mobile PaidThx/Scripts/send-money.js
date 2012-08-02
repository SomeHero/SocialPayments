/// <reference path="jquery-1.5.1-vsdoc.js" />
/// <reference path="jquery.validate-vsdoc.js" />
var getBaseURL = function () {
    return location.protocol + "//" + location.hostname + (location.port && ":" + location.port) + "/mobile/";
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
        amountSelected = $('#customAmount').val();
        var serviceUrl = getBaseURL() + 'Send/AmountToSend';
        $.mobile.changePage(serviceUrl, {
            type: "post",
            data: "20"
        });
    });

    $("#onedollarbtnsend").die('click').live('click', function () {
        amountSelected = "1.00";
        $('#amount-send').text(amountSelected);
        closeSendAmountDialog('amountbody-send');
    });

    $("#fivedollarbtnsend").die('click').live('click', function () {
        amountSelected = "5.00";
        $('#amount-send').text(amountSelected);
        closeSendAmountDialog('amountbody-send');
    });

    $("#tendollarbtnsend").die('click').live('click', function () {
        amountSelected = "10.00";
        $('#amount-send').text(amountSelected);
        closeSendAmountDialog('amountbody-send');
    });

    $("#twentydollarbtnsend").die('click').live('click', function () {
        amountSelected = "20.00";
        $('#amount-send').text(amountSelected);
        closeSendAmountDialog('amountbody-send');
    });
});
