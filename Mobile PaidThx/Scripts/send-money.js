﻿/// <reference path="jquery-1.5.1-vsdoc.js" />
/// <reference path="jquery.validate-vsdoc.js" />
var getBaseURL = function () {
    return location.protocol + "//" + location.hostname + (location.port && ":" + location.port) + "/mobile/";
}

function formatAmount(amount) {
    return "$" + amount.toFixed(2);
}

$(document).ready(function () {
    var amountSelected = 0.0;
    var recipient = "";
    var ref1 = null;
    var ref2 = null;

    var serviceUrl = getBaseURL() + 'Send';
    var testUrl = getBaseURL() + 'Send/SendData';

    $("#frmSendMoney").validate({
        submitHandler: function (form) {
            form.submit();
            alert("test");
        }
    });

    $("#gobtn-send").die('click').live('click', function () {
        amountSelected = $('#customAmountSend').val();
        var requestModel = {
            RecipientUri: recipient,
            Amount: amountSelected,
            Comments: null
        };

        var jsonData = $.toJSON(requestModel);

        $.ajax({
            type: 'POST',
            url: testUrl,
            data: jsonData,
            contentType: "application/json",
            dataType: "json",
            processData: false,
            success: function (data) {
                $.mobile.changePage(serviceUrl, {
                    type: "put",
                    data: data
                });
            },
            error: function (objRequest, next, errorThrown) {
                alert(next);
                $("#error-block").appendTo(next);
            }
        });
    });

    $("#submitContactSend").die('click').live('click', function () {
        ref1 = $("#email-send").val();
        ref2 = $("#phonenumber-send").val();

        if (ref2.length > 0) {
            recipient = ref2;
        }
        else if (ref1.length > 0) {
            recipient = ref1;
        }
        else {
            recipient = "";
        }

        var requestModel = {
            RecipientUri: recipient,
            Amount: amountSelected,
            Comments: null
        };

        var jsonData = $.toJSON(requestModel);

        $.ajax({
            type: 'POST',
            url: testUrl,
            data: jsonData,
            contentType: "application/json",
            dataType: "json",
            processData: false,
            success: function (data) {
                $.mobile.changePage(serviceUrl, {
                    type: "put",
                    data: data
                });
            },
            error: function (objRequest, next, errorThrown) {
                alert(next);
                $("#error-block").appendTo(next);
            }
        });
    });

    $("#onedollarbtnsend").die('click').live('click', function () {
        amountSelected = "1.00";
        var requestModel = {
            RecipientUri: recipient,
            Amount: amountSelected,
            Comments: null
        };

        var jsonData = $.toJSON(requestModel);

        $.ajax({
            type: 'POST',
            url: testUrl,
            data: jsonData,
            contentType: "application/json",
            dataType: "json",
            processData: false,
            success: function (data) {
                $.mobile.changePage(serviceUrl, {
                    type: "put",
                    data: data
                });
            },
            error: function (objRequest, next, errorThrown) {
                alert(next);
                $("#error-block").appendTo(next);
            }
        });
    });

    $("#fivedollarbtnsend").die('click').live('click', function () {
        amountSelected = "5.00";
        var requestModel = {
            RecipientUri: recipient,
            Amount: amountSelected,
            Comments: null
        };

        var jsonData = $.toJSON(requestModel);

        $.ajax({
            type: 'POST',
            url: testUrl,
            data: jsonData,
            contentType: "application/json",
            dataType: "json",
            processData: false,
            success: function (data) {
                $.mobile.changePage(serviceUrl, {
                    type: "put",
                    data: data
                });
            },
            error: function (objRequest, next, errorThrown) {
                alert(next);
                $("#error-block").appendTo(next);
            }
        });
    });

    $("#tendollarbtnsend").die('click').live('click', function () {
        amountSelected = "10.00";
        var requestModel = {
            RecipientUri: recipient,
            Amount: amountSelected,
            Comments: null
        };

        var jsonData = $.toJSON(requestModel);

        $.ajax({
            type: 'POST',
            url: testUrl,
            data: jsonData,
            contentType: "application/json",
            dataType: "json",
            processData: false,
            success: function (data) {
                $.mobile.changePage(serviceUrl, {
                    type: "put",
                    data: data
                });
            },
            error: function (objRequest, next, errorThrown) {
                alert(next);
                $("#error-block").appendTo(next);
            }
        });
    });

    $("#twentydollarbtnsend").die('click').live('click', function () {
        amountSelected = "20.00";
        var requestModel = {
            RecipientUri: recipient,
            Amount: amountSelected,
            Comments: null
        };

        var jsonData = $.toJSON(requestModel);

        $.ajax({
            type: 'POST',
            url: testUrl,
            data: jsonData,
            contentType: "application/json",
            dataType: "json",
            processData: false,
            success: function (data) {
                $.mobile.changePage(serviceUrl, {
                    type: "put",
                    data: data
                });
            },
            error: function (objRequest, next, errorThrown) {
                alert(next);
                $("#error-block").appendTo(next);
            }
        });
    });
});
