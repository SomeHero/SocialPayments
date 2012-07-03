/// <reference path="jquery-1.5.1-vsdoc.js" />
/// <reference path="jquery.validate-vsdoc.js" />
function formatAmount(amount) {
    return "$" + amount.toFixed(2);
}
function formatDate(jsonDate) {
    var value = new Date(parseInt(jsonDate.substr(6)));
    var ap = "AM";
    if(value.getHours() > 11)
        ap = "PM";

    return value.getMonth() + 1 + "/" + value.getDate() + "/" + value.getFullYear() + " " + value.getHours() +":" + value.getMinutes() + ":" + value.getSeconds() + " " + ap;
}
var updatePayStreamAll = function () {
    var otherUri = $('#txtSearchPaystreamAll').val();
    var debits = $('#cbSentAll').is(':checked');
    var credits = $('#cbReceivedAll').is(':checked');
    var pending = $('#cbPendingAll').is(':checked');
    var complete = $('#cbCompleteAll').is(':checked');


    var serviceUrl = getBaseURL() + '/mobile/profile/UpdatePayStream'
    var paymentAttributes = {
        OtherUri: otherUri,
        Debits: debits,
        Credits: credits,
        Pending: pending,
        Complete: complete
    };

    var jsonData = $.toJSON(paymentAttributes);
    $.ajax({
        type: 'POST',
        url: serviceUrl,
        data: jsonData,
        contentType: "application/json",
        dataType: "json",
        processData: false,
        success: function (data) {
            $("#transactionsListAll").empty();

            if (data.length == 0) {
                $("#resultsFilteredAll").show();
            } else {
                $("#resultsFilteredAll").hide();
                $("#transactionTemplate").tmpl(data)
                    .appendTo("#transactionsListAll");
            }

        },
        error: function (objRequest, next, errorThrown) {
            alert(next);
            $("#error-block").appendTo(next);
        }
    });
}
var updatePayStreamRequests = function () {
    var otherUri = $('#txtSearchPaystreamRequests').val();
    var debits = $('#cbSentRequests').is(':checked');
    var credits = $('#cbReceivedRequests').is(':checked');
    var pending = $('#cbPendingRequests').is(':checked');
    var complete = $('#cbCompleteRequests').is(':checked');


    var serviceUrl = getBaseURL() + '/mobile/profile/UpdatePayStream'
    var paymentAttributes = {
        OtherUri: otherUri,
        Debits: debits,
        Credits: credits,
        Pending: pending,
        Complete: complete
    };

    var jsonData = $.toJSON(paymentAttributes);
    $.ajax({
        type: 'POST',
        url: serviceUrl,
        data: jsonData,
        contentType: "application/json",
        dataType: "json",
        processData: false,
        success: function (data) {
            $("#transactionsListRequests").empty();

            if (data.length == 0) {
                $("#resultsFilteredRequests").show();
            } else {
                $("#resultsFilteredRequests").hide();
                $("#transactionTemplate").tmpl(data)
                    .appendTo("#transactionsListRequests");
            }

        },
        error: function (objRequest, next, errorThrown) {
            alert(next);
            $("#error-block").appendTo(next);
        }
    });
}
var updatePayStreamPayments = function () {
    var otherUri = $('#txtSearchPaystreamPayments').val();
    var debits = $('#cbSentPayments').is(':checked');
    var credits = $('#cbReceivedPayments').is(':checked');
    var pending = $('#cbPendingPayments').is(':checked');
    var complete = $('#cbCompletePayments').is(':checked');


    var serviceUrl = getBaseURL() + '/mobile/profile/UpdatePayStream'
    var paymentAttributes = {
        OtherUri: otherUri,
        Debits: debits,
        Credits: credits,
        Pending: pending,
        Complete: complete
    };

    var jsonData = $.toJSON(paymentAttributes);
    $.ajax({
        type: 'POST',
        url: serviceUrl,
        data: jsonData,
        contentType: "application/json",
        dataType: "json",
        processData: false,
        success: function (data) {
            $("#transactionsListPayments").empty();

            if (data.length == 0) {
                $("#resultsFilteredPayments").show();
            } else {
                $("#resultsFilteredPayments").hide();
                $("#transactionTemplate").tmpl(data)
                    .appendTo("#transactionsListPayments");
            }

        },
        error: function (objRequest, next, errorThrown) {
            alert(next);
            $("#error-block").appendTo(next);
        }
    });
}
$(document).ready(function () {
    $("#txtSearchPaystreamAll").keyup(function () {
        if ($('#txtSearchPaystreamAll').val().length > 2 || $('#txtSearchPaystreamAll').val().length == 0)
            updatePayStreamAll();
    });
    $('#cbSentAll').click(function () {
        updatePayStreamAll();
    });
    $('#cbReceivedAll').click(function () {
        updatePayStreamAll();
    });
    $('#cbPendingAll').click(function () {
        updatePayStreamAll();
    });
    $('#cbCompleteAll').click(function () {
        updatePayStreamAll();
    });
    $("#txtSearchPaystreamRequests").keyup(function () {
        if ($('#txtSearchPaystreamRequests').val().length > 2 || $('#txtSearchPaystreamRequests').val().length == 0)
            updatePayStreamRequests();
    });
    $('#cbSentRequests').click(function () {
        updatePayStreamRequests();
    });
    $('#cbReceivedRequests').click(function () {
        updatePayStreamRequests();
    });
    $('#cbPendingRequests').click(function () {
        updatePayStreamRequests();
    });
    $('#cbCompleteRequests').click(function () {
        updatePayStreamRequests();
    });
    $("#txtSearchPaystreamPayments").keyup(function () {
        if ($('#txtSearchPaystreamPayments').val().length > 2 || $('#txtSearchPaystreamPayments').val().length == 0)
            updatePayStreamPayments();
    });
    $('#cbSentPayments').click(function () {
        updatePayStreamPayments();
    });
    $('#cbReceivedPayments').click(function () {
        updatePayStreamPayments();
    });
    $('#cbPendingPayments').click(function () {
        updatePayStreamPayments();
    });
    $('#cbCompletePayments').click(function () {
        updatePayStreamPayments();
    });
});
var getBaseURL = function () {
    return location.protocol + "//" + location.hostname +
      (location.port && ":" + location.port) + "/";
}