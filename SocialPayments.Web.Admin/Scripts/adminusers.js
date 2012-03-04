﻿/// <reference path="jquery-1.5.1-vsdoc.js" />
$(document).ready(function () {
    $('.create').click(function () {
        $("#dialog-create").dialog({
            autoOpen: true,
            height: 'auto',
            width: 350,
            modal: true
        });
    });
    $('.edit').click(function () {

        var serviceUrl = 'http://beta.paidthx.me/services/UserService/Users/' + $(this).attr('userId') + '?apiKey=BDA11D91-7ADE-4DA1-855D-24ADFE39D174';

        $.ajax({
            type: 'Get',
            url: serviceUrl,
            success: function (data) {
                $("#dialog-edit #userId").val(data["userId"]);
                $("#dialog-edit #name").val(data['userName']);
                $("#dialog-edit #email").val(data['emailAddress']);
                $("#dialog-edit").dialog({
                    autoOpen: true,
                    height: 'auto',
                    width: 350,
                    modal: true
                });
            }
        });
    });
});