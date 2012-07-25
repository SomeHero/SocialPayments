/// <reference path="jquery-1.5.1-vsdoc.js" />
/// <reference path="jquery.validate-vsdoc.js" />

$(document).ready(function () {
    $("#frmSendMoney").validate({
        submitHandler: function (form) {
            form.submit();
            alert("test");
        }
    });
});

$("#clickamountbtn").die('click').live('click', function () {
    $.get(getBaseURL() + "Paystream/ChooseAmount", function (data) {
        $('#main-body').html(data).trigger("pagecreate").trigger("refresh");
    });
});

$("#gobtn").die('click').live('click', function () {
    $.get(getBaseURL() + "Paystream/SendMoney", function (data) {
        $('#main-body').html(data).trigger("pagecreate").trigger("refresh");
    });
});