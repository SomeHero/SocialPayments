/// <reference path="jquery-1.5.1-vsdoc.js" />
/// <reference path="jquery.validate-vsdoc.js" />

$(document).ready(function () {
    $("#frmRegister").validate({
        submitHandler: function (form) {
            form.submit();
        }
    });
});

var registerUser = function (registrationData, onSuccess) {

    var serviceUrl = 'http://beta.paidthx.me/services/UserService/Register?apiKey=BDA11D91-7ADE-4DA1-855D-24ADFE39D174';
    var jsonData = $.toJSON(registrationData);

    $.ajax({
        type: "POST",
        url: "http://beta.paidthx.me/services/UserService/Register?apiKey=BDA11D91-7ADE-4DA1-855D-24ADFE39D174",
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