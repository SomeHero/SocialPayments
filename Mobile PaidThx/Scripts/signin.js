/// <reference path="jquery-1.5.1-vsdoc.js" />
/// <reference path="jquery.validate-vsdoc.js" />

$(document).ready(function () {
    $("#frmSignIn").validate({
        submitHandler: function (form) {
            form.submit();
        }
    });
});