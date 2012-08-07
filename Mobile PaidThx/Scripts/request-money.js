/// <reference path="jquery-1.5.1-vsdoc.js" />
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
    $("#frmRequestMoney").validate({
        submitHandler: function (form) {
            form.submit();
        }
    });

    $("#gobtn-request").die('click').live('click', function () {
        amountSelected = $('#customAmountRequest').val();
        var requestModel = {
            RecipientUri: recipient,
            Amount: amountSelected,
            Comments: null
        };

        var jsonData = $.toJSON(requestModel);
        var serviceUrl = getBaseURL() + 'Request';
        var testUrl = getBaseURL() + 'Request/RequestData';

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

    $("#submitContactRequest").die('click').live('click', function () {
        ref1 = $("#email").val();
        ref2 = $("#phonenumber").val();

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
        var serviceUrl = getBaseURL() + 'Request';
        var testUrl = getBaseURL() + 'Request/RequestData';

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

    $("#onedollarbtnrequest").die('click').live('click', function () {
        amountSelected = "1.00";
        var requestModel = {
            RecipientUri: recipient,
            Amount: amountSelected,
            Comments: null
        };

        var jsonData = $.toJSON(requestModel);
        var serviceUrl = getBaseURL() + 'Request';
        var testUrl = getBaseURL() + 'Request/RequestData';

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

    $("#fivedollarbtnrequest").die('click').live('click', function () {
        amountSelected = "5.00";
        var requestModel = {
            RecipientUri: recipient,
            Amount: amountSelected,
            Comments: null
        };

        var jsonData = $.toJSON(requestModel);
        var serviceUrl = getBaseURL() + 'Request';
        var testUrl = getBaseURL() + 'Request/RequestData';

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

    $("#tendollarbtnrequest").die('click').live('click', function () {
        amountSelected = "10.00";
        var requestModel = {
            RecipientUri: recipient,
            Amount: amountSelected,
            Comments: null
        };

        var jsonData = $.toJSON(requestModel);
        var serviceUrl = getBaseURL() + 'Request';
        var testUrl = getBaseURL() + 'Request/RequestData';

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

    $("#twentydollarbtnrequest").die('click').live('click', function () {
        amountSelected = "20.00";
        var requestModel = {
            RecipientUri: recipient,
            Amount: amountSelected,
            Comments: null
        };

        var jsonData = $.toJSON(requestModel);
        var serviceUrl = getBaseURL() + 'Request';
        var testUrl = getBaseURL() + 'Request/RequestData';

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

