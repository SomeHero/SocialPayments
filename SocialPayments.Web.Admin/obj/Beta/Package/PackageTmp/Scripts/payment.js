/// <reference path="jquery-1.5.1-vsdoc.js" />
$(document).ready(function () {
    $('.create').click(function () {
        $("#dialog-create").dialog({
            autoOpen: true,
            height: 'auto',
            width: 800,
            modal: true
        });
    });
    $('.edit').click(function () {

        var serviceUrl = 'http://beta.paidthx.me/services/PaymentService/Payments/' + $(this).attr('paymentId') + '?apiKey=BDA11D91-7ADE-4DA1-855D-24ADFE39D174';

        $.ajax({
            type: 'Get',
            url: serviceUrl,
            dataType: 'json',
            format: 'json',
            success: function (data) {

                var paymentStatus = data["paymentStatus"];

                $("#dialog-edit #paymentId").val(data["paymentId"]);
                $("#dialog-edit #cancelPaymentId").val(data["paymentId"]);
                $("#dialog-edit #refundPaymentId").val(data["paymentId"]);
                $("#dialog-edit #paymentInfoPaymentId").text(data["paymentId"]);
                $("#dialog-edit #paymentStatus").text(data["paymentStatus"]);
                $("#dialog-edit #senderUri").text(data["fromMobileNumber"]);
                $("#dialog-edit #recipientUri").text(data['toMobileNumber']);
                $("#dialog-edit #edit-amount").val(data['amount']);
                $("#dialog-edit #view-amount").text(data['amount']);

                if (paymentStatus == 'Submitted' || paymentStatus == 'Pending') {
                    $("#dialog-edit #amount-edit-wrapper").show();
                    $("#dialog-edit #amount-view-wrapper").hide();
                } else {
                    $("#dialog-edit #amount-edit-wrapper").hide();
                    $("#dialog-edit #amount-view-wrapper").show();
                }
                $("#dialog-edit #comment").text(data['comment']);

                var paymentSubmittedDate = new Date(parseInt(data['paymentSubmittedDate'].substr(6)));
                $("#dialog-edit #paymentSubmitDate").text(paymentSubmittedDate.toLocaleDateString() + ' ' + paymentSubmittedDate.toLocaleTimeString());
                $("#dialog-edit #senderTransactionId").text(data['senderTransaction'].transactionId);
                $("#dialog-edit #senderTransactionAmount").text(data['senderTransaction'].amount);
                $("#dialog-edit #senderTransactionFromAccount").text(data['senderTransaction'].fromAccount.accountInformation);
                $("#dialog-edit #senderTransactionCategory").text(data['senderTransaction'].transactionCategory);
                $("#dialog-edit #senderTransactionStandardEntryClass").text(data['senderTransaction'].standardEntryClass);
                $("#dialog-edit #senderTransactionPaymentChannel").text(data['senderTransaction'].paymentChannel);
                if ($.trim(data['senderTransaction'].transactionStatus) == 'Paid') {
                    var senderTransactionSentDate = new Date(parseInt(data['senderTransaction'].transactionSentDate.substr(6)));
                    $("#dialog-edit #senderTransactionBatchId").text(data['senderTransaction'].transactionBatchId);
                    $("#dialog-edit #senderTransactionSentDate").text(senderTransactionSentDate.toLocalDateString() + ' ' + senderTransactionSentDate.toLocaleTimeString());
                }
                else {
                    $("#senderBatchWrapper").hide();
                }
                var senderTransactionCreateDate = new Date(parseInt(data['senderTransaction'].createDate.substr(6)));
                $("#dialog-edit #senderTransactionCreateDate").text(senderTransactionCreateDate.toLocaleDateString() + ' ' + senderTransactionCreateDate.toLocaleTimeString());

                if (data['senderTransaction'].lastUpdatedDate) {
                    var senderTransactionLastUpdatedDate = new Date(parseInt(data['senderTransaction'].lastUpdatedDate.substr(6)));
                    $("#dialog-edit #senderTransactionLastUpdatedDate").text(senderTransactionLastUpdatedDate.toLocaleDateString() + ' ' + senderTransactionLastUpdatedDate.toLocaleTimeString());
                }
                else
                    $("#dialog-edit #senderTransactionLastUpdatedDate").text('Never');
                $("#dialog-edit #senderTransactionTransactionStatus").text(data['senderTransaction'].transactionStatus);


                $("#dialog-edit #recipientTransactionId").text(data['recipientTransaction'].transactionId);
                $("#dialog-edit #recipientTransactionAmount").text(data['recipientTransaction'].amount);
                $("#dialog-edit #recipientTransactionFromAccount").text(data['recipientTransaction'].fromAccount.accountInformation);
                $("#dialog-edit #recipientTransactionCategory").text(data['recipientTransaction'].transactionCategory);
                $("#dialog-edit #recipientTransactionStandardEntryClass").text(data['recipientTransaction'].standardEntryClass);
                $("#dialog-edit #recipientTransactionPaymentChannel").text(data['recipientTransaction'].paymentChannel);

                var recipientTransactionCreateDate = new Date(parseInt(data['recipientTransaction'].createDate.substr(6)));
                $("#dialog-edit #recipientTransactionCreateDate").text(recipientTransactionCreateDate.toLocaleDateString() + ' ' + recipientTransactionCreateDate.toLocaleTimeString());
                if ($.trim(data['recipientTransaction'].transactionStatus) == 'Paid') {
                    var recipientTransactionSentDate = data['recipientTransaction'].transactionSentDate.substr(6);

                    $("#dialog-edit #recipientTransactionBatchId").text(data['recipientTransaction'].transactionBatchId);
                    $("#dialog-edit #recipientTransactionSentDate").text(recipientTransactionSentDate.toLocaleDateString() + ' ' + recipientTransactionSentDate.toLocaleTimeString());
                }
                else {
                    $("#recipientBatchWrapper").hide();
                }
                if (data['recipientTransaction'].lastUpdatedDate) {
                    var recipientTransactionLastUpdatedDate = new Date(parseInt(data['recipientTransaction'].lastUpdatedDate.substr(6)));
                    $("#dialog-edit #recipientTransactionLastUpdatedDate").text(recipientTransactionLastUpdatedDate.toLocaleDateString() + ' ' + recipientTransactionLastUpdatedDate.toLocaleTimeString());
                }
                else
                    $("#dialog-edit #recipientTransactionLastUpdatedDate").text('Never');
                $("#dialog-edit #recipientTransactionTransactionStatus").text(data['recipientTransaction'].transactionStatus);

                if (paymentStatus == "Submitted") {
                    $("#dialog-edit #saveChangesButton").show();
                    $("#dialog-edit #cancelPaymentButton").show();
                    $("#dialog-edit #refundPaymentButton").hide();
                } else if (paymentStatus == "Pending") {
                    $("#dialog-edit #saveChangesButton").show();
                    $("#dialog-edit #cancelPaymentButton").show();
                    $("#dialog-edit #refundPaymentButton").hide();
                } else if (paymentStatus == "Paid") {
                    $("#dialog-edit #saveChangesButton").hide();
                    $("#dialog-edit #cancelPaymentButton").hide();
                    $("#dialog-edit #refundPaymentButton").show();
                } else if (paymentStatus == "ReturnedNSF") {
                    $("#dialog-edit #saveChangesButton").hide();
                    $("#dialog-edit #cancelPaymentButton").hide();
                    $("#dialog-edit #refundPaymentButton").hide();
                } else if (paymentStatus == "Cancelled") {
                    $("#dialog-edit #saveChangesButton").hide();
                    $("#dialog-edit #cancelPaymentButton").hide();
                    $("#dialog-edit #refundPaymentButton").hide();
                }

                $("#dialog-edit").dialog({
                    autoOpen: true,
                    height: 500,
                    width: 800,
                    modal: true
                });
            }
        });
    });
});