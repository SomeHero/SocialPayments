/// <reference path="../jquery-1.5.1-vsdoc.js" />
$(document).ready(function () {
    $("#chk-user-agreement").attr('checked', false);
    $("#form-wrapper form").submit(function (e) {
        var validator = $("#form-wrapper form").validate();
        if (validator.numberOfInvalids() == 0 && !$("#chk-user-agreement").is(':checked')) {
            $("#user-agreement-dialog").dialog({
                resizable: false,
                modal: true,
                title: 'PdThx - Terms and Conditions',
                buttons: {
                    "Accept": function () {
                        if ($("#chk-user-agreement").is(':checked')) {
                            $("#form-wrapper form").submit();
                        }
                        else {
                            $("#user-agreement-validation").show();
                        }
                    },
                    "Decline": function () {
                        $(this).dialog("close");
                        return false;
                    }
                }
            });
            return false;
        }
        if (validator.numberOfInvalids() > 0 || !$("#chk-user-agreement").is(':checked'))
            return false;
    });
});