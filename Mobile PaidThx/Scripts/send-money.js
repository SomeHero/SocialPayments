/// <reference path="jquery-1.5.1-vsdoc.js" />
/// <reference path="jquery.validate-vsdoc.js" />


$(document).ready(function () {
    var amountSelected = 0.0;

    $("#frmSendMoney").validate({
        submitHandler: function (form) {
            form.submit();
            alert("test");
        }
    });

    $("#clickamountbtn").die('click').live('click', function () {
        $.get(getBaseURL() + "Paystream/ChooseAmount", function (data) {
            $('#main-body').html(data).trigger("pagecreate").trigger("refresh");

        });
    });
    $("#onedollarbtn").die('click').live('click', function () {
        amountSelected = 1.00;
        $.get(getBaseURL() + "Paystream/SendMoney", function (data) {
            $('#main-body').html(data).trigger("pagecreate").trigger("refresh");
            $('#amount').text(amountSelected);
        });
    });
    $("#fivedollarbtn").die('click').live('click', function () {
        amountSelected = 5.00;
        $.get(getBaseURL() + "Paystream/SendMoney", function (data) {
            $('#main-body').html(data).trigger("pagecreate").trigger("refresh");
            $('#amount').text(amountSelected);
        });
    });
    $("#tendollarbtn").die('click').live('click', function () {
        amountSelected = 10.00;
        $.get(getBaseURL() + "Paystream/SendMoney", function (data) {
            $('#main-body').html(data).trigger("pagecreate").trigger("refresh");
            $('#amount').text(amountSelected);
        });
    });
    $("#twentydollarbtn").die('click').live('click', function () {
        amountSelected = 20.00;
        $.get(getBaseURL() + "Paystream/SendMoney", function (data) {
            $('#main-body').html(data).trigger("pagecreate").trigger("refresh");
            $('#amount').text(amountSelected);
        });
    });
    $("#gobtn").die('click').live('click', function () {
        amountSelected = $('#customAmount').val();
        $.get(getBaseURL() + "Paystream/SendMoney", function (data) {
            $('#main-body').html(data).trigger("pagecreate").trigger("refresh");
            $('#amount').text(amountSelected);
        });
    });
});


