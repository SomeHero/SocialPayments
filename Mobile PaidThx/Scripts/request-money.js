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

    $("#gobtn").die('click').live('click', function () {
        amountSelected = $('#customAmount').val();
        $.get(getBaseURL() + "Paystream/RequestMoney", function (data) {
            $('#main-body-request').html(data).trigger("pagecreate").trigger("refresh");
            $('#amount-request').text(amountSelected);
        });
    });
});

function chooseAmountRequest() {
    $.get(getBaseURL() + "Paystream/ChooseAmountRequest", function (data) {
        $('#main-body-request').html(data).trigger("pagecreate").trigger("refresh");

    });
}

function oneDollarRequest() {
    amountSelected = 1.00;
    $.get(getBaseURL() + "Paystream/RequestMoney", function (data) {
        $('#main-body-request').html(data).trigger("pagecreate").trigger("refresh");
        $('#amount-request').text(amountSelected);
    });
}

function fiveDollarsRequest() {
    amountSelected = 5.00;
    $.get(getBaseURL() + "Paystream/RequestMoney", function (data) {
        $('#main-body-request').html(data).trigger("pagecreate").trigger("refresh");
        $('#amount-request').text(amountSelected);
    });
}

function tenDollarsRequest() {
    amountSelected = 10.00;
    $.get(getBaseURL() + "Paystream/RequestMoney", function (data) {
        $('#main-body-request').html(data).trigger("pagecreate").trigger("refresh");
        $('#amount-request').text(amountSelected);
    });
}

function twentyDollarsRequest() {
    amountSelected = 20.00;
    $.get(getBaseURL() + "Paystream/RequestMoney", function (data) {
        $('#main-body-request').html(data).trigger("pagecreate").trigger("refresh");
        $('#amount-request').text(amountSelected);
    });
}