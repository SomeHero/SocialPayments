/// <reference path="jquery-1.5.1-vsdoc.js" />
/// <reference path="jquery.validate-vsdoc.js" />
function formatAmount(amount) {
    return "$" + amount.toFixed(2);
}

function formatUri(userUri) {
    if (isNaN(userUri.substring(0, userUri.length))) {
        return userUri;
    }
    else {
        return "(" + userUri.substring(0, 3) + ") " + userUri.substring(3, 6) + "-" + userUri.substring(6, userUri.length);
    }
}

function formatDialogDate(jsonDate) {
    var value = new Date(parseInt(jsonDate.substr(6)));
    var ap = "AM";
    if (value.getHours() > 11)
        ap = "PM";

    return value.getMonth() + 1 + "/" + value.getDate() + "/" + value.getFullYear() + " at " + value.getHours() + ":" + value.getMinutes() + " " + ap;
}

function formatDate(jsonDate) {
    var value = new Date(parseInt(jsonDate.substr(6)));
    var today = new Date();

    if (value.getMonth() == today.getMonth() && value.getDate() == today.getDate() && value.getYear() == today.getYear()) {
        if (value.getMinutes() == today.getMinutes()) {
            return (today.getSeconds() - value.getSeconds()) + " seconds ago";
        }
        else if (value.getHours() == today.getHours()) {
            if (today.getMinutes() - value.getMinutes() == 1) {
                return "1 minute ago";
            }
            else {
                return (today.getMinutes() - value.getMinutes()) + " minutes ago";
            }
        }
        else {
            if (today.getHours() - value.getHours() == 1) {
                return "1 hour ago";
            }
            else {
                return (today.getHours() - value.getHours()) + " hours ago";
            }
        }
    }
    else {

        var ap = "AM";
        if (value.getHours() > 11)
            ap = "PM";

        return value.getMonth() + 1 + "/" + value.getDate() + "/" + value.getFullYear() + " @ " + value.getHours() + ":" + value.getMinutes() + " " + ap;
    }
}

function openOffersDialog(transactionId) {
    var serviceUrl = getBaseURL() + 'Profile/UpdatePayStreamDialog/' + transactionId;

    $.ajax({
        url: serviceUrl,
        dataType: "json",
        processData: false,
        success: function (data) {
            $("#popup").empty();
            $("#dialogTemplate").tmpl(data).appendTo("#popup");
        },
        error: function (objRequest, next, errorThrown) {
            alert(next);
            $("#error-block").appendTo(next);
        }
    });

    $('#overlay').fadeIn('fast', function () {
        $('#popup').css('display', 'block');
        $('#popup').animate({ 'left': '5%' }, 500);
    });
}


function closeOffersDialog(prospectElementID) {
    $(function ($) {
        $(document).ready(function () {
            $('#' + prospectElementID).css('position', 'absolute');
            $('#' + prospectElementID).animate({ 'left': '100%' }, 500, function () {
                $('#' + prospectElementID).css('position', 'fixed');
                $('#' + prospectElementID).css('left', '100%');
                $('#overlay').fadeOut('fast');
            });
        });
    });
}

var updatePayStreamAll = function () {
    var otherUri = $('#txtSearchPaystreamAll').val();
    var debits = $('#cbSentAll').is(':checked');
    var credits = $('#cbReceivedAll').is(':checked');
    var pending = $('#cbPendingAll').is(':checked');
    var complete = $('#cbCompleteAll').is(':checked');

    var serviceUrl = getBaseURL() + 'Profile/UpdatePayStream';
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

    var serviceUrl = getBaseURL() + 'Profile/UpdatePayStreamRequests'
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
    var serviceUrl = getBaseURL() + 'Profile/UpdatePayStreamPayments';

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

var updatePayStreamRequests = function () {
    var otherUri = $('#txtSearchPaystreamAlerts').val();
    var serviceUrl = getBaseURL() + 'Profile/UpdatePayStreamAlerts'
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
            $("#transactionsListAlerts").empty();

            if (data.length == 0) {
                $("#resultsFilteredRequests").show();
            } else {
                $("#resultsFilteredRequests").hide();
                $("#transactionTemplate").tmpl(data)
                    .appendTo("#transactionsListAlerts");
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

    $("#txtSearchPaystreamAlerts").keyup(function () {
        if ($('#txtSearchPaystreamAlerts').val().length > 2 || $('#txtSearchPaystreamAlerts').val().length == 0)
            updatePayStreamAlerts();
    });
});
var getBaseURL = function () {
    return location.protocol + "//" + location.hostname +
      (location.port && ":" + location.port) + "/mobile/";
}