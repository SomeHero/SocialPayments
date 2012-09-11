
var meCodeSearchController = (function ($, undefined) {
    var pub = {},
    $this = $(this);

    pub.init = function (listview) {

        //When news updated, display items in list
        $this.unbind("meCodes.updated").bind("meCodes.updated", function (e, meCodes) {
            displayMeCodes(meCodes);
        });
    };

    pub.searchAndDisplayMeCodes = function () {
        //Starting loading animation
        $.mobile.showPageLoadingMsg();

        //Get news and add success callback using then
        searchByMeCode(function () {
            //Stop loading animation on success
            $.mobile.hidePageLoadingMsg();
        });
    };

    function searchByMeCode(callback) {
        //Get news via ajax and return jqXhr
        $.ajax({
            url: "http://23.21.203.171/api/internal/api/Users/searchbymecode/$jam",
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

        //Empty current list
        $listview.empty();

        //Use template to create items & add to list
        $("#meCodeItem").tmpl(meCodes.foundUsers).appendTo($("#contactsList"));

        //Call the listview jQuery UI Widget after adding 
        //items to the list allowing correct rendering
        $("#contactsList").listview("refresh");
    }

    return pub;
} (jQuery));

$(document).on('pageshow', '[data-role=page]', function () {

    $('form').bind('firstinvalid', function (e) {
        return false;
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
                    if (errorList.length) {
                        var s = errorList.shift();
                        var n = [];
                        n.push(s);
                        this.errorList = n;
                    }
                    this.defaultShowErrors();
                },
                onkeyup: false,
                onfocusout: function (element) { $(element).valid(); },
                errorClass: 'error',
                validClass: 'valid',
                /*rules: {
                    password: {
                        required: true,
                        minlength: 6,
                        passwrdvalidator: true
                    },
                    confirmPassword: {
                        required: true,
                        equalTo: "#registration-form #password"
                    },
                    email: {
                        required: true,
                        email: true
                    }
                },*/
                messages: {
                    password: {
                        required: "Please create a password to protect your account.",
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
                    var elem = $(element);
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
                                event: false,
                                solo: true,
                                ready: true
                            },
                            hide: false,
                            style: {
                                classes: 'ui-tooltip-red qtip ui-tooltip-default ui-tooltip-rounded ui-tooltip-shadow ui-tooltip-light tip-error'
                            }
                        }).qtip('destroy').qtip({
                            overwrite: true,
                            content: error,
                            position: {
                                my: 'bottom center',
                                at: 'top center'
                            },
                            show: {
                                event: false,
                                solo: true,
                                ready: true
                            },
                            hide: false,
                            style: {
                                classes: 'ui-tooltip-red qtip ui-tooltip-default ui-tooltip-shadow ui-tooltip-light tip-error',
                                width: "88%"

                            }
                        });
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

    //$('input, textarea').placeholder();

});
