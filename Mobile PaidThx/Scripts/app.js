var webServicesController = (function ($, undefined) {
    var pub = {},
    webServicesBaseUrl = "",
    $this = $(this);

    pub.init = function(theWebServicesBaseUrl){
        webServicesBaseUrl = theWebServicesBaseUrl;
    };
    pub.getBaseURL = function () {
        return getBaseURL();
    };
    pub.getWebServicesBaseUrl = function () {
        return getWebServicesBaseUrl();
    };
    function getBaseURL() {
        return location.protocol + "//" + location.hostname + (location.port && ":" + location.port) + "/mobile/";
    }
    function getWebServicesBaseUrl() {
        return webServicesBaseUrl;
    }

    return pub;
} (jQuery));
var formattingController = (function($, undefined) {
    var pub = {},
    $this = $(this);

    pub.formatCurrency = function(val) {
        return $(val).formatCurrency();
    };

    return pub;
} (jQuery));
var validationController = (function ($, undefined) {
    var pub = {},
    $this = $(this);

    pub.isValidEmailAddress = function(emailAddress) {
        return isValidEmailAddress(emailAddress);
    };

    pub.isValidPhoneNumber = function(phoneNumber) {
        return isValidPhoneNumber(phoneNumber);
    };

    function isValidEmailAddress(emailAddress) {
        var pattern = new RegExp(/^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$/i);
        return pattern.test(emailAddress);
    }
    function isValidPhoneNumber(num) {
        var pattern = new RegExp(/\(?([0-9]{3})\)?([ .-]?)([0-9]{3})\2([0-9]{4})/);
        return pattern.test(num);
    }

    return pub;
} (jQuery));

var isAndroid = /android/i.test(navigator.userAgent.toLowerCase());
var isiPhone = /iphone/i.test(navigator.userAgent.toLowerCase());
var totalErrors = 0;
jQuery.expr[':'].focus = function( elem ) {
  return elem === document.activeElement && ( elem.type || elem.href );
};


var paystreamController = (function ($, undefined) {
    var pub = {},
        userId = "",
        take = 15,
        skip = 0,
        page = 0,
        pageSize = 15,
        type = "",
        items = {},
        displayedRecords = 0,
        totalRecords = 0,
        currentHeader = "",
        $this = $(this);

    pub.init = function (theUserId) {

        userId = theUserId;

        //When news updated, display items in list
        $this.unbind("paystream.updated").bind("paystream.updated", function (e, paystreamItems) {
            displayPaystream(paystreamItems, false);
            if (totalRecords > displayedRecords)
                $("#pagingList").show();
            else
                $("#pagingList").hide();
        });
        $this.unbind("paystream.added").bind("paystream.added", function (e, paystreamItems) {
            displayPaystream(paystreamItems, true);
            if (totalRecords > displayedRecords)
                $("#pagingList").show();
            else
                $("#pagingList").hide();
        });
    };

    pub.searchAndDisplayPaystream = function (theType, callback) {
        //Starting loading animation
        $.mobile.showPageLoadingMsg("Loading Paystream");

        displayedRecords = 0;
        page = 0;
        skip = 0;
        type = theType;
        currentHeader = "";

        //Get news and add success callback using then
        searchPayStream(function () {
            //Stop loading animation on success
            $this.trigger("paystream.updated", items);
            $.mobile.hidePageLoadingMsg();
            if (callback)
                callback();
        });
    };

    pub.getAndDisplayMoreItems = function () {
        //Starting loading animation
        $.mobile.showPageLoadingMsg();

        //Get news and add success callback using then
        searchPayStream(function () {
            //Stop loading animation on success
            $this.trigger("paystream.added", items);
            $.mobile.hidePageLoadingMsg();
        });
    };

    pub.displayPaystreamDetail = function (id) {
        //Starting loading animation
        $.mobile.showPageLoadingMsg();

        //Get news and add success callback using then
        openOffersDialog(id, function () {
            //Stop loading animation on success
            $.mobile.hidePageLoadingMsg();
        });
    };
    pub.closeDetailDialog = function () {
        //Starting loading animation
        $.mobile.showPageLoadingMsg();

        //Get news and add success callback using then
        closeDetailDialog(function () {
            //Stop loading animation on success
            $.mobile.hidePageLoadingMsg();
        });
    };
    pub.cancelPayment = function (id) {
        showPinSwipe("CancelPayment", id);
    };
    pub.cancelRequest = function (id) {
        showPinSwipe("CancelRequest", id);
    };
    pub.acceptRequest = function (id) {
        showPinSwipe("AcceptRequest", id);
    };
    pub.rejectRequest = function (id) {
        showPinSwipe("RejectRequest", id);
    };
    function showPinSwipe(action, id) {
        closeDetailDialog(function () {

            $.mobile.changePage("/mobile/paystream/PopupPinSwipe?paystreamAction=" + action + "&messageId=" + id,
                {
                    type: "GET",
                    transition: "slideup",
                    reverse: "false",
                    changehash: "false"
                });
        });
    }


    function searchPayStream(callback) {
        //Get news via ajax and return jqXhr
        $.ajax({
            url: webServicesController.getWebServicesBaseUrl() + "Users/" + userId + "/PaystreamMessages?type=" + type + "&take=" + take + "&skip=" + skip + "&page=" + page + "&pageSize=" + pageSize,
            dataType: "json",
            success: function (data, textStatus, xhr) {

                //Publish that news has been updated & allow
                //the 2 subscribers to update the UI content
                totalRecords = data.TotalRecords;
                displayedRecords += data.Results.length;
                skip = displayedRecords;
                page += 1;
                items = data.Results;

                if (callback) callback(data);
            }
        });

    }

    function displayPaystream(paystreamItems, append) {
        //cache the list-view element for later use
        var $listview = $("#paystreamList");

        //Empty current list
        if (!append)
            $("#paystreamList li").not("#no-results").remove();

        var today = new XDate();
        var yesterday = today.clone().addDays(-1);
        var thisWeek = today.clone().getWeek();
        var thisWeekYear = today.clone().getYear();
        var lastWeek = thisWeek - 1;
        var lastWeekYear = thisWeekYear;
        if (lastWeek == 0) {
            lastWeek = 53;
            lastWeekYear -= 1;
        }
        var thisMonth = today.getMonth();
        var thisMonthYear = today.getYear();

        var thisYear = today.getYear();

        var headerText = "";
        //Use template to create items & add to list
        for (i = 0; i < items.length; i++) {
            var header = {};
            header.groupHeading = "";

            var createDate = new XDate(new Date($(items).get(i).createDate));
            var createDateWeek = createDate.getWeek();
            var createDateMonth = createDate.getMonth();
            var createDateYear = createDate.getYear();

            if (createDate.getMonth() == today.getMonth() && createDate.getDate() == today.getDate() && createDate.getYear() == today.getYear()) {
                headerText = "Today";
            } else if (createDate.getMonth() == today.getMonth() && createDate.getDate() == yesterday.getDate() && yesterday.getYear() == yesterday.getYear()) {
                headerText = "Yesterday";
            } else if (createDateWeek == thisWeek && createDateYear == thisWeekYear) {
                headerText = "This Week";
            } else if (createDateWeek == lastWeek && createDateYear == lastWeekYear) {
                headerText = "Last Week";
            } else if (createDateMonth == thisMonth && createDateYear == thisYear) {
                headerText = "This Month";
            } else if (createDateMonth == lastMonth && createDateYear == lastMonthYear) {
                headerText = "Last Month";
            } else {
                headerText = createDate.toString("MMMM") + " " + createDate.toString("yyyy")
            }


            if (currentHeader != headerText) {
                currentHeader = headerText;
                header.groupHeading = currentHeader;
                $("#paystreamHeader").tmpl(header).appendTo($("#paystreamList"));
            }

            $("#paystreamItem").tmpl($(items).get(i)).appendTo($("#paystreamList"));

        };
        //$("#paystreamItem").tmpl(items).appendTo($("#paystreamList"));


        //Call the listview jQuery UI Widget after adding 
        //items to the list allowing correct rendering
        $("#paystreamList").listview("refresh");

        if ($("#paystreamList li").size() === 0) {
            $("#paystreamList").append($("#no-results-holder").html());
            $("#paystreamList").listview("refresh");
            $('#paystreamList #no-results').fadeIn(500);
        } else if ($("#paystreamList li").size() === 1 && $("#paystreamList #no-results").length) {
            $('#paystreamList #no-results').fadeIn(500);
        } else {
            $('#paystreamList #no-results').fadeOut(250);
            $('#paystreamList #no-results').remove();
            $("#paystreamList").listview("refresh");
        }
    }

    function openOffersDialog(transactionId, callback) {
        var serviceUrl = webServicesController.getWebServicesBaseUrl() + "/Users/" + userId + "/PaystreamMessages/" + transactionId;

        $.ajax({
            url: serviceUrl,
            dataType: "json",
            processData: false,
            success: function (data) {
                $("#popup").empty();
                $("#detailTemplate").tmpl(data).appendTo("#popup");
                $('#popup').css('display', 'block');
                $('#overlay').fadeIn('fast', function () {
                    $('#popup').css('display', 'block');
                    $('#popup').animate({ 'left': '10px' }, 300);
                });

                $("#popup").page();

                if (callback) callback(data);
            },
            error: function (objRequest, next, errorThrown) {
                alert(next);
                $("#error-block").appendTo(next);
            }
        });

    }

    function closeDetailDialog(callback) {
        $('#popup').css('position', 'absolute');
        $('#popup').animate({ 'left': '100%' }, 300, function () {
            $('#popup').css('position', 'fixed');
            $('#popup').css('left', '100%');
            $('#overlay').fadeOut('fast');

            if (callback) callback();
        });
    };
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
    function cancelPayment(messageId, callback) {
        $.ajax({
            url: webServicesController.getWebServicesBaseUrl() + "PaystreamMessages/" + messageId + "/cancel_payment",
            dataType: "json",
            type: "POST",
            success: function (data, textStatus, xhr) {
                if (callback)
                    callback(data);
            }
        });
    }
    function cancelRequest(messageId, callback) {
        $.ajax({
            url: webServicesController.getWebServicesBaseUrl() + "PaystreamMessages/" + messageId + "/cancel_request",
            dataType: "json",
            type: "POST",
            success: function (data, textStatus, xhr) {
                if (callback)
                    callback(data);
            }
        });
    }
    function acceptRequest(messageId, callback) {
        $.ajax({
            url: webServicesController.getWebServicesBaseUrl() + "PaystreamMessages/" + messageId + "/accept_request",
            dataType: "json",
            type: "POST",
            success: function (data, textStatus, xhr) {
                if (callback)
                    callback(data);
            }
        });
    }
    function rejectRequest(messageId, callback) {
        $.ajax({
            url: webServicesController.getWebServicesBaseUrl() + "PaystreamMessages/" + messageId + "/reject_request",
            dataType: "json",
            type: "POST",
            success: function (data, textStatus, xhr) {
                if (callback)
                    callback(data);
            }
        });
    }
    return pub;
} (jQuery));
var contactsSearchController = (function ($, undefined) {
    var pub = {},
    $page = $("#send-contact-select-page");
    foundMeCodes = new Array(),
    $this = $(this);

    pub.init = function (page) {
        $page = page;
        hideMeCodes();
        hideNoResults();

        //When news updated, display items in list
        $this.unbind("meCodes.updated").bind("meCodes.updated", function (e, meCodes) {
            displayMeCodes(meCodes);
        });
    };

    pub.searchAndDisplayMeCodes = function (searchValue, type) {
        //Starting loading animation
        $.mobile.showPageLoadingMsg();

        //Get news and add success callback using then
        searchByMeCode(searchValue, type, function () {
            //Stop loading animation on success
            $.mobile.hidePageLoadingMsg();
        });
    };
    pub.clearMeCodes = function (searchValue) {
        clearMeCodes();
    };
    pub.showNoResults = function (searchVal) {
        updateNoResults(searchVal);
    };
    pub.hideNoResults = function (searchVal) {
        hideNoResults(searchVal);
    }

    function searchByMeCode(searchValue, type, callback) {
        //Get news via ajax and return jqXhr
        var serviceUrl = webServicesController.getWebServicesBaseUrl() + "Users/searchbymecode/" + searchValue;

        if (type)
            serviceUrl = serviceUrl + "?type=" + type;

        $.ajax({
            url: serviceUrl,
            dataType: "json",
            success: function (data, textStatus, xhr) {
                //Publish that news has been updated & allow
                //the 2 subscribers to update the UI content
                $this.trigger("meCodes.updated", data);
                if (callback) callback(data);
            }
        });
    }

    function displayMeCodes(meCodes) {
        //cache the list-view element for later use
        var $listview = $("#contactsList");
        var newMeCodes = new Array();

        if (meCodes) {
            $.each(meCodes.foundUsers, function (index, value) {
                if (!($.inArray(value.meCode, foundMeCodes) > -1)) {
                    newMeCodes.push(value);
                    foundMeCodes.push(value.meCode);
                }
            });
            //Use template to create items & add to list
            $("#meCodeItem").tmpl(newMeCodes).insertAfter($("#contactsList #me-codes-divider"));
        }
        else {
            clearMeCodes();
        }

        //Call the listview jQuery UI Widget after adding 
        //items to the list allowing correct rendering
        if (newMeCodes)
            $("#contactsList").listview("refresh");
    }
    function clearMeCodes() {
        $("#contactsList .mecode-recipient").remove();
        foundMeCodes = new Array();
    }
    function hideMeCodes() {
        $page.find("#me-codes-divider").hide();
        $page.find(".me-codes-receipient").hide();
    }
    function updateNoResults(searchVal) {
        //if none are found then fadeIn the `#no-results` element
        if (!$page.find("#contact-no-results").is(":visible")) {
            $page.find("#contact-no-results").toggle();
            $page.find("#contactsList").listview("refresh");
        }
        if (!$page.find("#contact-no-results-divider").is(":visible")) {
            $page.find("#contact-no-results-divider").toggle();
            $page.find("#contactsList").listview("refresh");
        }

        if (validationController.isValidEmailAddress(searchVal)) {
            $page.find("#results-header").text(searchVal);
            $page.find("#results-description").text("New Email Recipient");
            $page.find("#contact-new-recipient-uri").attr('recipient-uri', searchVal);
            $page.find("#contact-new-recipient-uri").attr('recipient-name', searchVal);
        } else if (validationController.isValidPhoneNumber(searchVal)) {
            $page.find("#results-header").text(searchVal);
            $page.find("#results-description").text("New Phone Recipient");
            $page.find("#contact-new-recipient-uri").attr('recipient-uri', searchVal);
            $page.find("#contact-new-recipient-uri").attr('recipient-name', searchVal);
        }
        else {
            $page.find("#results-header").text("No matches found");
            $page.find("#results-description").text("Continue type or check entry");
            $page.find("#contact-recipient-uri").val('');
            $page.find("#contact-new-recipient-uri").attr('recipient-name', '');
        }
    }
    function hideNoResults(searchVal) {
        $page.find("#contact-no-results-divider").fadeOut(250);
        $page.find("#contact-no-results").fadeOut(250);
    }

    return pub;
} (jQuery));

//PINSWIPE RESIZE CONTROLLER

var pinswipeResizeController = (function ($, undefined) {
    var pub = {},
    $this = $(this);
    pub.resizePINs = function () {
        $(".hackstyle").remove();
        var widthy = $(".patternlockcontainer").width(); //replaces $(window).width();
        divwidth = widthy * .95;
        $("#resizerHack").append("<div class=\"hackstyle\"><style>div.patternlocklinesdiagonalcontainer { height:" + divwidth + "px ; width:" + divwidth + "px ;}div.patternlocklinesverticalcontainer { height:" + divwidth + "px ; width:" + divwidth + "px ;}div.patternlocklineshorizontalcontainer { height:" + divwidth + "px ; width:" + divwidth + "px ;}div.patternlockbuttoncontainer { height:" + divwidth + "px ; width:" + divwidth + "px ;}</div>")
        $(".patternlockcontainer > div").center();
    };

    return pub;
} (jQuery));


$(document).on('pageshow', '[data-role=page]', function () {

    //add center to jquery object
    $.fn.center = function () {
        this.css("position", "absolute");
        this.css("left", ($(window).width() - this.width()) / 2 + $(window).scrollLeft() + "px");
        return this;
    }

    $('form').bind('firstinvalid', function (e) {
        return false;
    });

    //resize PINs on window resize
    $(window).resize(function () {
        pinswipeResizeController.resizePINs();
    });


    //Create all custom rules

    //Validate Password
    $.validator.addMethod("passwrdvalidator", function (value) {
        return (/^(?=.*\d)(?=.*[a-z])(?=.*[A-Z]).{6,30}$/).test(value);
    });

    //Confirm Account
    $.validator.addMethod("pwmatch", function (value) {
        return value == $("#registration-form #password").val();
    }, "Please enter the same password as above");


    //Confirm Account
    $.validator.addMethod("accountmatch", function (value) {
        return value == $("#AccountNumber").val();
    }, "Confirm Account Number must match Account Number");

    //Account Format
    $.validator.addMethod("accountformat", function (value) {
        return (/^(?=.*[0-9]).{4,17}$/).test(value);
    }, "Please enter a valid bank account number (4-17 digits).  You can find this info on a check or in your online banking.");

    try {
        //Validate any form that might need validation
        if ($("form.validateMe").length > 0) {
            $("form.validateMe").validate({
                submitHandler: function (form) {
                    form.submit();
                },
                invalidHandler: function (form, validator) {
                    //invalid         

                },
                showErrors: function (errorMap, errorList) {
                    /*
                    if (errorList.length) {
                    var s = errorList.shift();
                    var n = [];
                    n.push(s);
                    this.errorList = n;
                    }
                    totalErrors = errorList.length;
                    if (errorList.length) {
                    this.errorList = errorList.reverse();
                    }
                    */
                    this.defaultShowErrors();
                },
                onkeyup: false,
                onfocusout: function (element) { $(element).valid(); },
                errorClass: 'error',
                validClass: 'valid',
                messages: {
                    password: {
                        required: "Password must contain: 6+ characters, at least 1 uppercase letter, and 1 number.",
                        passwrdvalidator: "Password must contain: 6+ characters, at least 1 uppercase letter, and 1 number."
                    },
                    confirmPassword: {
                        required: "Please confirm your password",
                        pwmatch: "Please enter the same password as above"
                    },
                    email: {
                        required: "Please enter your email address",
                        email: "Please enter a valid email address"
                    }
                },
                errorPlacement: function (error, element) {

                    var formm = $(element).closest("form");
                    var elem = $(element);
                    formElements = [];

                    $(formm).find(':input').each(function () {
                        formElements.push();
                    })

                    // Check we have a valid error message
                    if (!error.is(':empty')) {

                        // Apply the tooltip only if it isn't valid
                        elem.filter(':not(.valid)').qtip({
                            overwrite: true,
                            content: error,
                            position: {
                                my: 'bottom center',
                                at: 'top center'
                            },
                            show: {
                                event: 'focus',
                                solo: true,
                                ready: true
                            },
                            hide: {
                                event: 'unfocus'
                            },
                            style: {
                                classes: 'ui-tooltip-red qtip ui-tooltip-default ui-tooltip-rounded ui-tooltip-shadow ui-tooltip-light tip-error',
                                width: "88%"
                            },
                            events: {
                                show: function (event, api) {

                                    if (!$(elem).is(":focus")) {
                                        event.preventDefault();
                                    }
                                }
                            }
                        })// If we have a tooltip on this element already, just update its content
					.qtip('option', 'content.text', error);
                    } else {
                        elem.qtip('destroy');
                    }
                },
                success: $.noop
            });

        } else { } //end if
    } catch (e) {
        //catching
        return false;
    }

});
