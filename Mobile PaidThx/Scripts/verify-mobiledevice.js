/// <reference path="jquery-1.5.1-vsdoc.js" />
$(document).ready(function () {
    $("#frmVerifyMobileDevice").validate({
        submitHandler: function (form) {
            form.submit();
        }
    });
});

var verifyMobileDevice = function (verificationData, onSuccess) {
    var serviceUrl = 'http://beta.paidthx.me/services/UserService/VerifyMobileDevice?apiKey=BDA11D91-7ADE-4DA1-855D-24ADFE39D174';
    var jsonData = $.toJSON(verificationData);

    $.ajax({
        type: "POST",
        url: serviceUrl,
        data: jsonData,
        contentType: "application/json",
        dataType: "json",
        processData: false,
        success: function (data) {
            onSuccess(data);
        },
        error: function (objRequest, next, errorThrown) {
            alert(objRequest.responseText);
        }
    });
}