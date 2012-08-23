var getBaseURL = function () {
    return location.protocol + "//" + location.hostname + (location.port && ":" + location.port) + "/mobile/";
}


$(document).ready(function () {

    $(".notification-item").die('change').live('change', function () {

        var key = $(this).attr('id');
        var value = $(this).val();

        var requestModel = {
            Key: key,
            Value: value
        };

        var jsonData = $.toJSON(requestModel);
        var serviceUrl = getBaseURL() + 'Notifications/Index';

        $.ajax({
            type: 'POST',
            url: serviceUrl,
            data: jsonData,
            contentType: "application/json",
            dataType: "json",
            processData: false
        });
    });
});