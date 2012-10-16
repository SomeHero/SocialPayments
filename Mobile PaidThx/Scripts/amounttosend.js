$(document).ready(function () {

    var currentAmount = "0.00";

    $(this).delegate('#customAmount', 'keydown', function () {
        if (event.shiftKey) {
            event.preventDefault();
            return;
        }
        if (event.keyCode == 46 || event.keyCode == 8) {
            currentAmount = currentAmount.substring(0, currentAmount.length - 1);

            currentAmount = currentAmount.replace('.', '');
            currentAmount = currentAmount.replace(',', '');

            if (currentAmount.length < 3)
                currentAmount = '0' + currentAmount;

            //number before the decimal point
            num = currentAmount.substring(0, currentAmount.length - 2);
            //number after the decimal point
            dec = currentAmount.substring(currentAmount.length - 2);

            //connect both parts while comma-ing the first half
            currentAmount = num + "." + dec;

            $(this).val(currentAmount);
            event.preventDefault();

            return;
        }
        else {
            if (event.keyCode < 95) {
                if (event.keyCode < 48 || event.keyCode > 57) {
                    event.preventDefault();
                    return;
                }
            }
            else {
                if (event.keyCode < 96 || event.keyCode > 105) {
                    event.preventDefault();
                    return;
                }
            }
        }
        currentAmount = currentAmount + String.fromCharCode(event.keyCode);

        if (currentAmount.charAt(0) == '0')
            currentAmount = currentAmount.slice(1);

        currentAmount = currentAmount.replace('.', '');
        currentAmount = currentAmount.replace(',', '');

        //number before the decimal point
        num = currentAmount.substring(0, currentAmount.length - 2);
        //number after the decimal point
        dec = currentAmount.substring(currentAmount.length - 2);

        //connect both parts while comma-ing the first half
        currentAmount = num + "." + dec;

        $(this).val(currentAmount);
        event.preventDefault();
    });
});