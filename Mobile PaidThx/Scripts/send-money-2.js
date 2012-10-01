//wait to do event binding until the page is being initialized
$(document).delegate('[data-role="page"]', 'pageinit', function () {

    //cache the list-view element for later use
    var $listview = $(this).find('[data-role="listview"]');

    //delegate event binding since the search widget is added dynamically
    //bind to the `keyup` event so we can check the value of the input after the user types
    $(this).delegate('input[data-type="search"]', 'keyup', function () {

        //check to see if there are any visible list-items that are not the `#no-results` element
        if ($listview.children(':visible').not('#no-results').length === 0) {

            //if none are found then fadeIn the `#no-results` element
            $('#no-results').fadeIn(500);
        } else {

            //if results are found then fadeOut the `#no-results` element which has no effect if it's already hidden
            $('#no-results').fadeOut(250);
        }
    });
});​
