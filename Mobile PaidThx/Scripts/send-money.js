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
        $('#amount-send').text(amountSelected);
        closeSendAmountDialog('amountbody-send');
    }

     $("#btnSelectAmountSend").die('click').live('click', function () {
        openSendAmountDialog();
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

function openSendAmountDialog() {
    $('#chooseAmountSendOverlay').fadeIn('fast', function () {
        $('#amountbody-send').css('display', 'block');
        $('#amountbody-send').animate({ 'left': '5%' }, 500);
    });
}

function closeSendAmountDialog(prospectElementID) {
    $(function ($) {
        $(document).ready(function () {
            $('#' + prospectElementID).css('position', 'absolute');
            $('#' + prospectElementID).animate({ 'left': '100%' }, 500, function () {
                $('#' + prospectElementID).css('position', 'fixed');
                $('#' + prospectElementID).css('left', '100%');
                $('#chooseAmountSendOverlay').fadeOut('fast');
            });
        });
    });
}