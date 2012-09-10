/*
 * jQuery validate.password plug-in 1.0
 *
 * http://bassistance.de/jquery-plugins/jquery-plugin-validate.password/
 *
 * Copyright (c) 2009 Jörn Zaefferer
 *
 * $Id$
 *
 * Dual licensed under the MIT and GPL licenses:
 *   http://www.opensource.org/licenses/mit-license.php
 *   http://www.gnu.org/licenses/gpl.html
 */
(function ($) {

    var LOWER = /[a-z]/,
		UPPER = /[A-Z]/,
		DIGIT = /[0-9]/,
		DIGITS = /[0-9].*[0-9]/,
		SPECIAL = /[^a-zA-Z0-9]/,
		SAME = /^(.)\1+$/;

    function rating(rate, message) {
        return {
            rate: rate,
            messageKey: message
        };
    }

    function uncapitalize(str) {
        return str.substring(0, 1).toLowerCase() + str.substring(1);
    }

    $.validator.passwordRating = function (password, username) {
        if (!password)
            return rating(0, "nothing");
       if (password.length < 6)
            return rating(0, "too-short");
        if (SAME.test(password))
            return rating(1, "very-weak");

        var lower = LOWER.test(password),
			upper = UPPER.test(uncapitalize(password)),
			digit = DIGIT.test(password),
			digits = DIGITS.test(password),
			special = SPECIAL.test(password);

        if (lower && upper && digit && special) {
            $('#confirmPasword').fadeIn(500);
            return rating(4, "strong");
        }
        if ((lower || upper) && digit && special) {
            $('#confirmPasword').fadeIn(500);
            return rating(3, "good");
        }
        if ((lower || upper) && digit && !special) {
            return rating(2, "need-special");
        }
        if ((lower || upper) && !digit && special) {
            return rating(2, "need-number");
        }
        return rating(2, "weak");
    }

    $.validator.passwordRating.messages = {
        "similar-to-username": "Too similar to email",
        "too-short": "Too Short",
        "nothing": "Start Typing...",
        "very-weak": "Very weak",
        "need-special": "Needs a Special Char. (!$%&...)",
        "need-number": "Needs a Number (0-9)",
        "weak": "Too Weak",
        "good": "Good",
        "strong": "Very Strong"
    }

    $.validator.addMethod("password", function (value, element, usernameField) {
        // use untrimmed value
        var password = element.value,
        // get username for comparison, if specified
			username = $(typeof usernameField != "boolean" ? usernameField : []);

        var rating = $.validator.passwordRating(password, username.val());
        // update message for this field

        var meter = $(".password-meter");

        meter.find(".password-meter-bar").removeClass().addClass("password-meter-bar").addClass("password-meter-" + rating.messageKey);
        meter.find(".password-meter-message")
		.removeClass()
		.addClass("password-meter-message")
		.addClass("password-meter-message-" + rating.messageKey)
		.text($.validator.passwordRating.messages[rating.messageKey]);
        // display process bar instead of error message

        return rating.rate > 2;
    }, "&nbsp;");
    // manually add class rule, to make username param optional
    $.validator.classRuleSettings.password = { password: true };

})(jQuery);
