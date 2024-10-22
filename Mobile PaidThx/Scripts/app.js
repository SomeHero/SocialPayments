//add center to jquery object
$.fn.center = function () {
    this.css("position", "absolute");
    this.css("left", ($(window).width() - this.width()) / 2 + $(window).scrollLeft() + "px");
    return this;
};


//add moveTo to jquery object
(function ($) {
    $.fn.moveTo = function (selector) {
        return this.each(function () {
            var cl = $(this).clone();
            $(cl).appendTo(selector);
            //$(this).remove();
        });
    };
})(jQuery);


var webServicesController = (function ($, undefined) {
    var pub = {},
    webServicesBaseUrl = "",
    apiKey = "",
    $this = $(this);

    pub.init = function(theWebServicesBaseUrl, theApiKey){
        webServicesBaseUrl = theWebServicesBaseUrl;
        apiKey = theApiKey;
    };
    pub.getBaseURL = function () {
        return getBaseURL();
    };
    pub.getWebServicesBaseUrl = function () {
        return getWebServicesBaseUrl();
    };
    pub.getApiKey = function () {
        return getApiKey();
    };
    function getBaseURL() {
        return location.protocol + "//" + location.hostname + (location.port && ":" + location.port) + "/mobile/";
    }
    function getWebServicesBaseUrl() {
        return webServicesBaseUrl;
    }
    function getApiKey() {
        return apiKey;
    }

    return pub;
} (jQuery));

//FORMATTING CONTROLLER
var formattingController = (function($, undefined) {
    var pub = {},
    $this = $(this);

    pub.formatCurrency = function(val) {
        return $(val).formatCurrency();
    };

    return pub;
} (jQuery));

//VALIDATION CONTROLLER
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

//HELPERS
var isAndroid = /android/i.test(navigator.userAgent.toLowerCase());
var isiPhone = /iphone/i.test(navigator.userAgent.toLowerCase());

jQuery.expr[':'].focus = function( elem ) {
  return elem === document.activeElement && ( elem.type || elem.href );
};

//PAYSTREAM CONTROLLER
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
        $page = null,
        $listview = null,
        $this = $(this);

    pub.init = function (theUserId) {

        userId = theUserId;
        $page = $("#paystream-index"),
        $listview = $("#paystreamList"),

        //When news updated, display items in list
        $this.unbind("paystream.updated").bind("paystream.updated", function (e, paystreamItems) {
            displayPaystream(paystreamItems, false);

            if (totalRecords > displayedRecords) {
                showMoreResults();
            } else {
                hideMoreResults();
            }
        });

        $this.unbind("paystream.added").bind("paystream.added", function (e, paystreamItems) {
            displayPaystream(paystreamItems, true);
            if (totalRecords > displayedRecords) {
                showMoreResults();
            } else {
                hideMoreResults();
            }
        });
    };

    pub.searchAndDisplayPaystream = function (theType, callback) {
        displayedRecords = 0;
        page = 0;
        skip = 0;
        type = theType;
        currentHeader = "";

        //Get news and add success callback using then
        searchPayStream(function () {
            //Stop loading animation on success
            $this.trigger("paystream.updated", items);
            if (displayedRecords == 0) {
                showNoResults();
            } else {
                hideNoResults();
            }
            if (callback) {
                callback();
            }
        });
    };

    pub.getAndDisplayMoreItems = function () {
        //Get more items
        searchPayStream(function () {
            //Stop loading animation on success
            $this.trigger("paystream.added", items);
        });
    };

    pub.displayPaystreamDetail = function (id) {
        //Get news and add success callback using then
        openOffersDialog(id, function () {
        });
    };
    pub.closeDetailDialog = function () {
        //Get news and add success callback using then
        closeDetailDialog(function () {
        });
    };
    pub.showNoResults = function (val) {
        showNoResults();
    };
    pub.hideNoResults = function (val) {
        hideNoResults();
    };
    pub.showMoreResults = function () {
        if (totalRecords > displayedRecords) {
            showMoreResults();
        }
    };
    pub.hideMoreResults = function () {
        hideMoreResults();
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
            url: webServicesController.getWebServicesBaseUrl() + "Users/" + userId + "/PaystreamMessages?type=" + type + "&take=" + take + "&skip=" + skip + "&page=" + page + "&pageSize=" + pageSize + "&apikey=" + webServicesController.getApiKey(),
            dataType: "json",
            error: function (data, textStatus, xhr) {
               //hiding for now alert(textStatus);
                if (callback) {
                    callback(data);
                }
            },
            success: function (data, textStatus, xhr) {

                //Publish that news has been updated & allow
                //the 2 subscribers to update the UI content
                totalRecords = data.TotalRecords;
                displayedRecords += data.Results.length;
                skip = displayedRecords;
                page += 1;
                items = data.Results;

                if (callback) {
                    callback(data);
                }
            }
        });

    }

    function displayPaystream(paystreamItems, append) {
        //Empty current list
        if (!append) {
            $listview.find(".removable").remove();
        }

        var today = moment().format('DDD');
        var yesterday = today - 1;
        var thisWeek = moment().format('w'); 
        var thisWeekYear = moment().format('YYYY');
        var lastWeek = thisWeek - 1;
        var lastWeekYear = thisWeekYear;
        if (lastWeek == 0) {
            lastWeek = 53;
            lastWeekYear -= 1;
        }
        var thisMonth = moment().format('M'); 
        var lastMonth = thisMonth - 1;
        var thisMonthYear = moment().format('YYYY');
        var lastMonthYear = thisMonthYear;
        if (lastMonth == 0) {
            lastMonth = 12;
            lastMonthYear -= 1;
        }
        var thisYear = moment().format('YYYY');

        var headerText = "";
        //Use template to create items & add to list
        for (i = 0; i < items.length; i++) {
            var header = {};
            header.groupHeading = "";

            var createDate = moment.utc($(items).get(i).createDate).local().format('DDD');
            var createDateWeek = moment.utc($(items).get(i).createDate).local().format('w');
            var createDateMonth = moment.utc($(items).get(i).createDate).local().format('M');
            var createDateYear = moment.utc($(items).get(i).createDate).local().format('YYYY');

            if (createDateMonth == thisMonth && createDate == today && createDateYear == thisYear) {
                headerText = "Today";
            } else if (createDateMonth == thisMonth && createDate == yesterday && createDateYear == moment().subtract('days', 1).format('YYYY')) {
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
                headerText = moment.utc($(items).get(i).createDate).local().format('ddd, MMM Do, YY');
            }


            if (currentHeader != headerText) {
                currentHeader = headerText;
                header.groupHeading = currentHeader;
                $("#paystreamHeader").tmpl(header).insertBefore($listview.find(".more-results"));
            }

            $("#paystreamItem").tmpl($(items).get(i)).insertBefore($listview.find(".more-results"));

        }
    }
    function showMoreResults() {
        $listview.find(".more-results").show();
    }
    function hideMoreResults() {
        $listview.find(".more-results").hide();
    }
    function showNoResults() {
        $listview.find("#paystream-no-results-divider").show();
        $listview.find("#paystream-no-results").show();
    }
    function hideNoResults() {
        $listview.find("#paystream-no-results-divider").hide();
        $listview.find("#paystream-no-results").hide();

    }
    function openOffersDialog(transactionId, callback) {
        var serviceUrl = webServicesController.getWebServicesBaseUrl() + "/Users/" + userId + "/PaystreamMessages/" + transactionId + "?apikey=" + webServicesController.getApiKey();

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

                if (callback) {
                    callback(data);
                }
            },
            error: function (objRequest, next, errorThrown) {
                //hiding for now alert(next);
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
            if (callback) {
                callback();
            }
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

    function cancelPayment(messageId, callback) {
        $.ajax({
            url: webServicesController.getWebServicesBaseUrl() + "PaystreamMessages/" + messageId + "/cancel_payment",
            dataType: "json",
            type: "POST",
            success: function (data, textStatus, xhr) {
                if (callback) {
                    callback(data);
                }
            }
        });
    }

    function cancelRequest(messageId, callback) {
        $.ajax({
            url: webServicesController.getWebServicesBaseUrl() + "PaystreamMessages/" + messageId + "/cancel_request",
            dataType: "json",
            type: "POST",
            success: function (data, textStatus, xhr) {
                if (callback) {
                    callback(data);
                }
            }
        });
    }

    function acceptRequest(messageId, callback) {
        $.ajax({
            url: webServicesController.getWebServicesBaseUrl() + "PaystreamMessages/" + messageId + "/accept_request",
            dataType: "json",
            type: "POST",
            success: function (data, textStatus, xhr) {
                if (callback) {
                    callback(data);
                }
            }
        });
    }

    function rejectRequest(messageId, callback) {
        $.ajax({
            url: webServicesController.getWebServicesBaseUrl() + "PaystreamMessages/" + messageId + "/reject_request",
            dataType: "json",
            type: "POST",
            success: function (data, textStatus, xhr) {
                if (callback) {
                    callback(data);
                }
            }
        });
    }
    return pub;
} (jQuery));

var contactsSearchController = (function ($, undefined) {
    var pub = {},
    foundMeCodes = [],
    $this = $(this);

    pub.init = function () {
        $this.unbind("meCodes.updated").bind("meCodes.updated", function (e, meCodes) {
            displayMeCodes(meCodes);
        });
    };
    pub.searchAndDisplayMeCodes = function (searchValue, type) {
        searchByMeCode(searchValue, type, function () {
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
    };

    function searchByMeCode(searchValue, type, callback) {
        var serviceUrl = webServicesController.getWebServicesBaseUrl() + "Users/searchbymecode/" + searchValue + "?apikey=" + webServicesController.getApiKey();

        if (type) {
            serviceUrl = serviceUrl + "?type=" + type;
        }

        $.ajax({
            url: serviceUrl,
            dataType: "json",
            success: function (data, textStatus, xhr) {
                //$(document).trigger("meCodes.updated", data);
                displayMeCodes(data);
                if (callback) {
                    callback(data);
                }
            }
        });
    }

    function displayMeCodes(meCodes) {
        //cache contact list element for later use
        var $contactList = $("#contactList");
        var newMeCodes = [];

        //Did we get some matching me codes back?
        if (meCodes.foundUsers.length > 0) {
            $.each(meCodes.foundUsers, function (index, value) {
                //if (!($.inArray(value.meCode, foundMeCodes) > -1)) {
                    newMeCodes.push(value);
                    foundMeCodes.push(value.meCode);
                //}
            });

            //Remove ME Code Search Item
            $("#contactList #me-code-search-item").remove();
            //Use template to create items & add to list
            $("#meCodeItem").tmpl(newMeCodes).insertAfter($("#contactList #me-codes-divider"));
        }
        else {
            //Remove ME Code Search Item
            $("#contactList #me-code-search-item").remove();

            //Add ME Code NONE Item
            $("#listItemHolder #me-code-none-item").moveTo("#contactList");
            clearMeCodes();
        }
    }
    function clearMeCodes() {
        $("#contactList .me-code-recipient").remove();
        foundMeCodes = [];
    }
    function hideMeCodes() {
        $("#contactList #me-codes-divider").remove();
        $("#contactList #me-code-search-item").remove();
        $("#contactList .me-code-recipient").remove();
    }
    function updateNoResults(searchVal) {
        if (validationController.isValidEmailAddress(searchVal)) {
            $("#contactList #search-item #results-header").text(searchVal);
            $("#contactList #search-item #results-description").text("New Email Recipient");
            $("#contactList #search-item #contact-new-recipient-uri").val(searchVal);
            $("#contactList #search-item #contact-new-recipient-name").val(searchVal);
            $("#contactList #search-item #contact-new-recipient-link").attr('data-uri-valid', '1');
            //Remove NoLink Class
            $("#contactList #search-item").removeClass("nolink");
        } else if (validationController.isValidPhoneNumber(searchVal)) {
            $("#contactList #search-item #results-header").text(searchVal);
            $("#contactList #search-item #results-description").text("New Phone Recipient");
            $("#contactList #search-item #contact-new-recipient-uri").val(searchVal);
            $("#contactList #search-item #contact-new-recipient-name").val(searchVal);
            $("#contactList #search-item #contact-new-recipient-link").attr('data-uri-valid', '1');
            //Remove NoLink Class
            $("#contactList #search-item").removeClass("nolink");
        } else {
            $("#contactList #search-item #results-header").text("Keep typing...");
            $("#contactList #search-item #results-description").text("to add new phone or email");
            $("#contactList #search-item #contact-new-recipient-uri").val('');
            $("#contactList #search-item #contact-new-recipient-name").val('');
            $("#contactList #search-item #contact-new-recipient-link").attr('data-uri-valid', '0');
            //Remove NoLink Class
            $("#contactList #search-item").addClass("nolink");
        }
    }
    function hideNoResults(searchVal) {

    }

    return pub;
} (jQuery));

//PINSWIPE RESIZE CONTROLLER

var pinswipeResizeController = (function ($, undefined) {
    var pub = {},
    $this = $(this);
    pub.resizePINs = function () {
        var widthy = $(".patternlockcontainer").width(); 
        divwidth = widthy * 0.95;
        $('.patternlockcontainer > div').css('height', (divwidth));
        $('.patternlockcontainer > div').css('width', (divwidth));
        $(".patternlockcontainer > div").css("position", "absolute");
        $('.patternlockcontainer > div').css("left", ($('div#pinHolder').width() - $('.patternlockcontainer > div').width()) / 2);
        $('.patternlockcontainer').css("opacity", "0");
        hideAJAXLoader();
        $('#pinHolder').animate({
            opacity: 1
        }, 400, function () {
        hideAJAXLoader();
        $('.patternlockcontainer').animate({
            opacity: 1
        }, 500, function () {
        $('.patternlockcontainer').css("opacity", "1");
});
        });
    };

    return pub;
} (jQuery));


//GLOBAL MOVED VAR

var itemsMovedController = (function ($, undefined) {
    var pub = {},
    $this = $(this);
    pub.moved = false;
    pub.isMoved = function () {
        return pub.moved;
    };
    pub.updateMoved = function () {
        pub.moved = true;
    };

    return pub;
} (jQuery));


//DATE CONTROLLER
function getDateNow(){
   var now = moment();
   return now;
}

var datetimenow = moment();

//HIDING PHONE BROWSER ADDRESS BAR

function hideAddressBar() {
    if (!window.location.hash) {
        if (document.height < $(window).height()) {
            $('div.page').css("min-height", ($(window).height() + 50) + 'px');
        }

        setTimeout(function () { window.scrollTo(0, 1); }, 50);
    }
}

window.addEventListener("load", function () { if (!window.pageYOffset) { hideAddressBar(); } });
window.addEventListener("orientationchange", hideAddressBar);



 //CONFIRMATION MESSAGE
function confirmAlert(message) {

    var $confirmalert = $('<div class="alert-holder"><a name="confirm-scroll" id="scroll-target"></a><div id="confirm-alert" class="confirm alert">' + message + '</div></div>');

    if ($('.validation-summary-errors').is(':visible')) {
        //do nothing
    } else {

        if ($('.top-panel').is(':visible')) {
            $($confirmalert).insertAfter($('div.page .pd-content .top-panel'));
        } else {
        $('div.page .pd-content').prepend($($confirmalert));
    }
    $('html,body').animate({ scrollTop: $("#scroll-target").offset().top }, 'fast');

    setTimeout(function () {

    $('.alert-holder').animate({
            height: 'toggle',
            opacity: 'toggle'
        }, 300, 'swing', function () {
            $('.alert-holder').remove();
        });
    }, 5000);
    }

}


//LOADING FUNCTIONS
function showFullLoader(message) {
    if (message) {
        $(".the-loading-text").text(message);
    } else {
        $(".the-loading-text").text("WORKING");
    }
$('.loader-holder-full').fadeIn();
}

function showAJAXLoader(message) {
    if (message) {
        $(".aj-loading-text").text(message);
    } else {
        //do nothing
    }
    $('#page-loader').css("opacity", "0.7");
    $('#page-loader').show().stop().animate({
        opacity: 1
    }, 300, function () {

    });
}

function hideAJAXLoader() {
    $('#page-loader').stop().animate({
        opacity: 0.7
    }, 300, function () {
        $('#page-loader').hide();
    });
}





// ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
// ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
//LOAD ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
$(document).ready(function () {

    //Make contains expression case agnostic
    jQuery.expr[':'].contains = function (a, i, m) {
        return (a.textContent || a.innerText || "").toUpperCase().indexOf(m[3].toUpperCase()) >= 0;
    };

    jQuery.expr[':'].icontains = function (a, i, m) {
        return jQuery(a).text().toUpperCase()
      .indexOf(m[3].toUpperCase()) >= 0;
    };

//FORMAT MOMENT CALENDAR SETTINGS
    moment.calendar = {
        lastDay: '[Yesterday at] LT',
        sameDay: '[Today at] LT',
        nextDay: '[Tomorrow at] LT',
        lastWeek: '[last] dddd [at] LT',
        nextWeek: 'dddd [at] LT',
        sameElse: 'L [at] LT'
    };

   

    //SHOW GENERIC LOADER FOR LONG LOAD EVENTS
    $('.showloader').die().live("click", function () {
        $('.loader-holder-full').fadeIn();
    });

    //AJAX LOADER
    var $loadingguy = $('<div id="page-mask"></div><div id="page-loader"><div class="loader"><img src="/mobile/Content/images/ajax-loader.gif")" alt="loader" /></div></div>');

    $('div.page').append($loadingguy);

    $(this).ajaxStop(function () {
        $('#page-loader').stop().animate({
            opacity: 0.7
        }, 300, function () {
            $('#page-loader').hide();
        });
    });
    $(this).ajaxStart(function () {
        $('#page-loader').css("opacity", "0.7");
        $('#page-loader').show().stop().animate({
            opacity: 1
        }, 300, function () {

        });
    });

    //HIDE PRELOADER WHEN PAGES LOAD
    $('.loader-holder-full').fadeOut();

    //BACK BUTTONS
    $('.btn-back.history').live('click', function () {
        history.back(); return false;
    });

    //hide empty top panels
    $('.top-panel:empty').hide();

    $(window).bind('resize', function (event) {

        //resize PINs on window resize
        pinswipeResizeController.resizePINs();

        //my attempt to recreate jquery mobile full page
        var content_height = $('div.page').height(),
            window_height = $(this).height();
        $('div.page').css('min-height', (window_height + 50));
        event.stopImmediatePropagation();

    }).trigger("resize");



    //Create all custom rules
    $.validator.addMethod('phone', function (value) {
        return /^[01]?[- .]?\(?[2-9]\d{2}\)?[- .]?\d{3}[- .]?\d{4}$/.test(value);
    }, 'Please enter a valid 10 digit phone number');

    //Validate Password
    $.validator.addMethod("passwrdvalidator", function (value) {
        return (/^(?=.*\d)(?=.*[a-z])(?=.*[A-Z]).{6,30}$/).test(value);
    }, "Password must contain: 6+ characters, at least 1 uppercase letter, and 1 number");

    //Confirm PW
    $.validator.addMethod("pwmatch", function (value) {
        return value == $("#registration-form #password").val();
    }, "Please enter the same password as above");

    //Generic Match
    $.validator.addMethod("matcher", function (value) {
        return value == $(".matchee").val();
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
                    //SHOW PRELOADER
                    $('.loader-holder-full').fadeIn();
                    form.submit();
                },
                invalidHandler: function (form, validator) {
                    //invalid         

                },
                showErrors: function (errorMap, errorList) {
                    this.defaultShowErrors();
                },
                onkeyup: false,
                onfocusout: function (element) { $(element).valid(); },
                errorClass: 'error',
                validClass: 'valid',
                messages: {
                    password: {
                        required: "Password must contain: 6+ characters, at least 1 uppercase letter, and 1 number.",
                        passwrdvalidator: "Password must contain: 6+ characters, at least 1 uppercase letter, and 1 number"
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
                    });

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
