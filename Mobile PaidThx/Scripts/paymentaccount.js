/// <reference path="jquery-1.5.1-vsdoc.js" />
/// <reference path="jquery.validate-vsdoc.js" />
$(document).ready(function () {

    var header = $('#header');
    var getBaseURL = function () {
        return location.protocol + "//" + location.hostname + (location.port && ":" + location.port) + "/mobile/";
    }
    $(".edit-payment-account").click(function () {
        var paymentAccountId = $(this).attr('data-val');
        $.get(getBaseURL() + "PaymentAccount/Edit/" + paymentAccountId, function (data) {
            $('#accounts-content').html(data).trigger("pagecreate").trigger("refresh");
        });
    });
    $("#remove-account").click(function () {
        var paymentAccountId = $(this).attr('data-val');
        $.post(getBaseURL() + "PaymentAccount/Remove/" + paymentAccountId,
            function (data) {
                $('#accounts-content').html(data).trigger("pagecreate").trigger("refresh");
            });
    });
    $("#add-account").click(function () {
        $.get(getBaseURL() + "PaymentAccount/Add", function (data) {
            $('#accounts-content').html(data).trigger("pagecreate").trigger("refresh");
        });
    });
    $("#edit-close").die('click').live('click', function () {
        $.get(getBaseURL() + "PaymentAccount/List/", function (data) {
            $('#accounts-content').html(data).trigger("pagecreate").trigger("refresh");
        });
    });
    $("#add-close").die('click').live('click', function () {
        $.get(getBaseURL() + "PaymentAccount/List/", function (data) {
            $('#accounts-content').html(data).trigger("pagecreate").trigger("refresh");
        });
    });
    $("#add-account-submit").die('click').live('click', function () {
        $.post(getBaseURL() + "PaymentAccount/Add/",
            $("#frmAddAccount").serialize(),
            function (data) {
                $('#accounts-content').html(data).trigger("pagecreate").trigger("refresh");
            });
    });
});
