/// <reference path="jquery-1.5.1-vsdoc.js" />
$(document).ready(function () {
    $("#frmSetupACHAccount").validate({
        submitHandler: function (form) {
            form.submit();
        }
    });
});

var setupACHAccount = function(achAccountData, onSuccess) {
    var serviceUrl = 'http://beta.paidthx.me/services/PaymentAccountService/PaymentAccounts?apiKey=BDA11D91-7ADE-4DA1-855D-24ADFE39D174';
    var jsonData = $.toJSON(achAccountData);

    $.ajax({
        type: "POST",
        url: serviceUrl,
        data: jsonData,
        contentType: "application/json",
        dataType: "json",
        processData: false,
        success: function (data) {
            onSuccess(data)
        },
        error: function (objRequest, next, errorThrown) {
            alert(objRequest.responseText);
        }
    });
}