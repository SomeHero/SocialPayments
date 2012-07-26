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

    $("#gobtn").die('click').live('click', function () {
        amountSelected = $('#customAmount').val();
        $.get(getBaseURL() + "Paystream/SendMoney", function (data) {
            $('#main-body').html(data).trigger("pagecreate").trigger("refresh");
            $('#amount').text(amountSelected);
        });
    });
});

function chooseAmount() {
    $.get(getBaseURL() + "Paystream/ChooseAmount", function (data) {
        $('#main-body').html(data).trigger("pagecreate").trigger("refresh");

    });
}

function oneDollar() {
    amountSelected = 1.00;
    $.get(getBaseURL() + "Paystream/SendMoney", function (data) {
        $('#main-body').html(data).trigger("pagecreate").trigger("refresh");
        $('#amount').text(amountSelected);
    });
}

function fiveDollars() {
    amountSelected = 5.00;
    $.get(getBaseURL() + "Paystream/SendMoney", function (data) {
        $('#main-body').html(data).trigger("pagecreate").trigger("refresh");
        $('#amount').text(amountSelected);
    });
}

function tenDollars() {
    amountSelected = 10.00;
    $.get(getBaseURL() + "Paystream/SendMoney", function (data) {
        $('#main-body').html(data).trigger("pagecreate").trigger("refresh");
        $('#amount').text(amountSelected);
    });
}

function twentyDollars() {
    amountSelected = 20.00;
    $.get(getBaseURL() + "Paystream/SendMoney", function (data) {
        $('#main-body').html(data).trigger("pagecreate").trigger("refresh");
        $('#amount').text(amountSelected);
    });
}

