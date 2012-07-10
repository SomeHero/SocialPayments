/// <reference path="jquery-1.5.1-vsdoc.js" />
/// <reference path="jquery.validate-vsdoc.js" />
$(document).ready(function () {
    var header = $('#header');
    var getBaseURL = function () {
        return location.protocol + "//" + location.hostname + (location.port && ":" + location.port) + "/";
    }
    $(".edit-payment-account").click(function () {
        var paymentAccountId = $(this).attr('data-val');
        $.get(getBaseURL() + "mobile/PaymentAccount/Edit/" + paymentAccountId, function (data) {
            $('#accounts-content').html(data).trigger("pagecreate").trigger("refresh");
        });
    });
    $("#add-account").click(function () {
        $.get(getBaseURL() + "mobile/PaymentAccount/Add", function (data) {
            $('#accounts-content').html(data).trigger("pagecreate").trigger("refresh");
        });
    });
    $("#edit-close").die('click').live('click', function () {
        $.get(getBaseURL() + "mobile/PaymentAccount/List", function (data) {
            $('#accounts-content').html(data).trigger("pagecreate").trigger("refresh");
        });
    });
    $("#add-close").die('click').live('click', function () {
        $.get(getBaseURL() + "mobile/PaymentAccount/List", function (data) {
            $('#accounts-content').html(data).trigger("pagecreate").trigger("refresh");
        });
    });
    $("#add-account-submit").die('click').live('click', function () {
        $.post(getBaseURL() + "mobile/PaymentAccount/Add",
            $("#frmAddAccount").serialize(),
            function (data) {
                $('#accounts-content').html(data).trigger("pagecreate").trigger("refresh");
            }
        );
    });
});
