/// <reference path="../jquery-1.5.1-vsdoc.js" />
$(document).ready(function () {
    $("#btnAuthenticateMobileDevice").click(function () {
        //Send SMS Codes
        $.ajax({
                type:'Post',
                url: '',
                success: function (data) {}
            });
    $("#authenticate-mobile-dialog").dialog({
        resizable: false,
        width: 500,
        modal: true,
        title: 'Authentication Mobile Device'
    });
});
});