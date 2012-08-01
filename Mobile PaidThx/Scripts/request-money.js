/// <reference path="jquery-1.5.1-vsdoc.js" />
/// <reference path="jquery.validate-vsdoc.js" />
var getBaseURL = function () {
    return location.protocol + "//" + location.hostname + (location.port && ":" + location.port) + "/mobile/";
}

$(document).ready(function () {
    var amountSelected = 0.0;
    $("#frmRequestMoney").validate({
        submitHandler: function (form) {
            form.submit();
        }
    });

    $("#gobtn-request").die('click').live('click', function () {
        amountSelected = $('#customAmount').val();
        $('#amount-request').text(amountSelected);
        closeAmountDialog('amountbody-request');
    });

    $("#btnSelectAmount").die('click').live('click', function () {
        openAmountDialog();
    });

    $("#onedollarbtn").die('click').live('click', function () {
        amountSelected = "1.00";
        $('#amount-request').text(amountSelected);
        closeAmountDialog('amountbody-request');
    });

    $("#fivedollarbtn").die('click').live('click', function () {
        amountSelected = "5.00";
        $('#amount-request').text(amountSelected);
        closeAmountDialog('amountbody-request');
    });

    $("#tendollarbtn").die('click').live('click', function () {
        amountSelected = "10.00";
        $('#amount-request').text(amountSelected);
        closeAmountDialog('amountbody-request');
    });

    $("#twentydollarbtn").die('click').live('click', function () {
        amountSelected = "20.00";
        $('#amount-request').text(amountSelected);
        closeAmountDialog('amountbody-request');
    });
});

function openAmountDialog() {
    $('#chooseRequestOverlay').fadeIn('fast', function () {
        $('#amountbody-request').css('display', 'block');
        $('#amountbody-request').animate({ 'left': '5%' }, 500);
    });
}

function closeAmountDialog(prospectElementID) {
    $(function ($) {
        $(document).ready(function () {
            $('#' + prospectElementID).css('position', 'absolute');
            $('#' + prospectElementID).animate({ 'left': '100%' }, 500, function () {
                $('#' + prospectElementID).css('position', 'fixed');
                $('#' + prospectElementID).css('left', '100%');
                $('#chooseRequestOverlay').fadeOut('fast');
            });
        });
    });
}
