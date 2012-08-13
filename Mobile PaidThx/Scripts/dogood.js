/// <reference path="jquery-1.5.1-vsdoc.js" />
/// <reference path="jquery.validate-vsdoc.js" />
var getBaseURL = function () {
    return location.protocol + "//" + location.hostname + (location.port && ":" + location.port) + "/mobile/";
}

function toDonateScreen() {
    var homeUrl = getBaseURL() + 'DoGood/Donate';
    $.mobile.changePage(homeUrl);
}

function toPledgeScreen() {
    var homeUrl = getBaseURL() + 'DoGood/Pledge';
    $.mobile.changePage(homeUrl);
}

function toDonateAmountScreen() {
    var homeUrl = getBaseURL() + 'DoGood/AmountToDonate';
    $.mobile.changePage(homeUrl);
}

function toPledgeAmountScreen() {
    var homeUrl = getBaseURL() + 'DoGood/AmountToPledge';
    $.mobile.changePage(homeUrl);
}

function toAddOrgScreen() {
    var homeUrl = getBaseURL() + 'DoGood/AddOrg';
    $.mobile.changePage(homeUrl);
}

function toAddPledgeOrg() {
    var homeUrl = getBaseURL() + 'DoGood/AddOrgPledge';
    $.mobile.changePage(homeUrl);
}

function toWhomPledgeScreen() {
    var homeUrl = getBaseURL() + 'DoGood/AddToSend';
    $.mobile.changePage(homeUrl);
}

$(document).ready(function () {
    var amountSelected = 0.0;
    var recipient = "";
    var organization = "";
    var orgnizationid = "";
    var ref1 = null;
    var ref2 = null;

    var pledgeAmountSelected = 0.0;
    var pledgeRecipient = "";
    var pledgeOrganization = "";

    var serviceUrl = getBaseURL() + 'DoGood/Donate';
    var testUrl = getBaseURL() + 'DoGood/DonateData';
    var pledgeUrl = getBaseURL() + 'DoGood/Pledge';

    $(".organization").die('click').live('click', function () {

        var orgId = $(this).attr('data-value');
        var orgName = $(this).attr('organization-name');
        organizationid = orgId;
        organization = orgName;

        var donateModel = {
            Organization: organization,
            OrganizationId: organizationid,
            Amount: amountSelected,
            PledgerUri: recipient
        };

        var jsonData = $.toJSON(donateModel);

        $.ajax({
            type: 'POST',
            url: testUrl,
            data: jsonData,
            contentType: "application/json",
            dataType: "json",
            processData: false,
            success: function (data) {
                $.mobile.changePage(pledgeUrl, {
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

    $(".organization-donate").die('click').live('click', function () {

        var orgId = $(this).attr('data-value');
        var orgName = $(this).attr('organization-name');
        organizationid = orgId;
        organization = orgName;

        var donateModel = {
            Organization: organization,
            OrganizationId: organizationid,
            Amount: amountSelected
        };

        var jsonData = $.toJSON(donateModel);

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

    $("#gobtn-donate").die('click').live('click', function () {
        amountSelected = $('#customAmountDonate').val();
        var donateModel = {
            Organization: organization,
            OrganizationId: organizationid,
            Amount: amountSelected
        };

        var jsonData = $.toJSON(donateModel);

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

    $("#submitContactPledge").die('click').live('click', function () {
        ref1 = $("#email-pledge").val();
        ref2 = $("#phonenumber-pledge").val();

        if (ref2.length > 0) {
            recipient = ref2;
        }
        else if (ref1.length > 0) {
            recipient = ref1;
        }
        else {
            recipient = "";
        }
        var donateModel = {
            Organization: organization,
            OrganizationId: orgnizationid,
            Amount: amountSelected,
            PledgerUri: recipient
        };

        var jsonData = $.toJSON(donateModel);

        $.ajax({
            type: 'POST',
            url: testUrl,
            data: jsonData,
            contentType: "application/json",
            dataType: "json",
            processData: false,
            success: function (data) {
                $.mobile.changePage(pledgeUrl, {
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

    $("#onedollarbtndonate").die('click').live('click', function () {
        amountSelected = 1.00;
        var donateModel = {
            Organization: organization,
            OrganizationId: organizationid,
            Amount: amountSelected
        };

        var jsonData = $.toJSON(donateModel);

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

    $("#fivedollarbtndonate").die('click').live('click', function () {
        amountSelected = 5.00;
        var donateModel = {
            Organization: organization,
            OrganizationId: organizationid,
            Amount: amountSelected
        };

        var jsonData = $.toJSON(donateModel);

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

    $("#tendollarbtndonate").die('click').live('click', function () {
        amountSelected = 10.00;
        var donateModel = {
            Organization: organization,
            OrganizationId: organizationid,
            Amount: amountSelected
        };

        var jsonData = $.toJSON(donateModel);

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

    $("#twentydollarbtndonate").die('click').live('click', function () {
        amountSelected = 20.00;
        var donateModel = {
            Organization: organization,
            OrganizationId: organizationid,
            Amount: amountSelected
        };

        var jsonData = $.toJSON(donateModel);

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


    // pledge 
    $("#gobtn-pledge").die('click').live('click', function () {
        amountSelected = $('#customAmountPledge').val();
        var donateModel = {
            Organization: organization,
            OrganizationId: organizationid,
            Amount: amountSelected,
            PledgerUri: recipient
        };

        var jsonData = $.toJSON(donateModel);

        $.ajax({
            type: 'POST',
            url: testUrl,
            data: jsonData,
            contentType: "application/json",
            dataType: "json",
            processData: false,
            success: function (data) {
                $.mobile.changePage(pledgeUrl, {
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

    $("#onedollarbtnpledge").die('click').live('click', function () {
        amountSelected = 1.00;
        var donateModel = {
            Organization: organization,
            OrganizationId: organizationid,
            Amount: amountSelected,
            PledgerUri: recipient
        };

        var jsonData = $.toJSON(donateModel);

        $.ajax({
            type: 'POST',
            url: testUrl,
            data: jsonData,
            contentType: "application/json",
            dataType: "json",
            processData: false,
            success: function (data) {
                $.mobile.changePage(pledgeUrl, {
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

    $("#fivedollarbtnpledge").die('click').live('click', function () {
        amountSelected = 5.00;
        var donateModel = {
            Organization: organization,
            OrganizationId: organizationid,
            Amount: amountSelected,
            PledgerUri: recipient
        };

        var jsonData = $.toJSON(donateModel);

        $.ajax({
            type: 'POST',
            url: testUrl,
            data: jsonData,
            contentType: "application/json",
            dataType: "json",
            processData: false,
            success: function (data) {
                $.mobile.changePage(pledgeUrl, {
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

    $("#tendollarbtnpledge").die('click').live('click', function () {
        amountSelected = 10.00;
        var donateModel = {
            Organization: organization,
            OrganizationId: organizationid,
            Amount: amountSelected,
            PledgerUri: recipient
        };

        var jsonData = $.toJSON(donateModel);

        $.ajax({
            type: 'POST',
            url: testUrl,
            data: jsonData,
            contentType: "application/json",
            dataType: "json",
            processData: false,
            success: function (data) {
                $.mobile.changePage(pledgeUrl, {
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

    $("#twentydollarbtnpledge").die('click').live('click', function () {
        amountSelected = 20.00;
        var donateModel = {
            Organization: organization,
            OrganizationId: organizationid,
            Amount: amountSelected,
            PledgerUri: recipient
        };

        var jsonData = $.toJSON(donateModel);

        $.ajax({
            type: 'POST',
            url: testUrl,
            data: jsonData,
            contentType: "application/json",
            dataType: "json",
            processData: false,
            success: function (data) {
                $.mobile.changePage(pledgeUrl, {
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

    $("#donateMoneyBtn").die('click').live('click', function () {
        var theAmount = $("#amountToDonate").val();
        var theRecipient = $("#org-name-donate").text();
        var theComments = $("#txtComments-Donate").val();
        var theOrgId = $("#theOrgId").val();

        var donateModel = {
            Organization: theRecipient,
            OrganizationId: theOrgId,
            Amount: theAmount,
            Comments: theComments
        };

        var jsonData = $.toJSON(donateModel);
        var pinswipeUrl = getBaseURL() + "DoGood/PinswipeDonate";
        $.ajax({
            type: 'POST',
            url: testUrl,
            data: jsonData,
            contentType: "application/json",
            dataType: "json",
            processData: false,
            success: function (data) {
                $.mobile.changePage(pinswipeUrl, {
                    type: "post",
                    data: data
                });
            },
            error: function (objRequest, next, errorThrown) {
                alert(next);
                $("#error-block").appendTo(next);
            }
        });
    });
    $("#pledgeMoneyBtn").die('click').live('click', function () {
        var theAmount = $("#amountToPledge").val();
        var theOrganization = $("#org-name-pledge").text();
        var theComments = $("#txtComments-Pledge").val();
        var thePledger = $("#onBehalfOf").text();
        var theOrgId = $("#theOrgId-Pledge").val();

        var donateModel = {
            Organization: theOrganization,
            OrganizationId: theOrgId,
            Amount: theAmount,
            PledgerUri: thePledger,
            Comments: theComments
        };

        var jsonData = $.toJSON(donateModel);
        var pinswipeUrl = getBaseURL() + "DoGood/PinswipePledge";
        $.ajax({
            type: 'POST',
            url: testUrl,
            data: jsonData,
            contentType: "application/json",
            dataType: "json",
            processData: false,
            success: function (data) {
                $.mobile.changePage(pinswipeUrl, {
                    type: "post",
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
