/// <reference path="jquery-1.5.1-vsdoc.js" />
/// <reference path="jquery.validate-vsdoc.js" />
/// <reference path="patternlock.js" />
$(document).ready(function () {

    $("#patternlock").livequery(function () {
        patternlock.generate(document.getElementById("patternlock"));
        document.getElementById("pinswipesubmit").style.display = 'none';
    });
});