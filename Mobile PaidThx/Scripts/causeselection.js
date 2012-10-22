$(document).ready(function () {

//cause selection
    $('#contactList').on("click", ".contact-organization", function (event) {

        $(this).parents("form:first").submit();

    });

    //LAZY LOAD
    $("#contactList").find("img.lazy").lazyload();

    //Load no contacts if none there to start
    if ($("#contactList li").length < 1) {
        $("#listItemHolder #contact-no-results").moveTo("#contactList");
        $("#listItemHolder #facebook").moveTo("#contactList");
    }

    //cache the lists for later use
    var contactlist = $("#contactList");

    $('.contact-top-input').on('keyup', function (e) {

        //Remove all helper items
        $("#contactList .searchHelper").remove();


        //check if already moved
        if (itemsMovedController.isMoved()) {
            //moved
        } else {

            //move all remaining contacts to the searchList
            $("#contactList li").moveTo("#searchListHolder");
            itemsMovedController.updateMoved();

        }

        //empty the contact list element
        $("#contactList").empty();

        //get search string
        var searchVal = $('.contact-top-input').val();

        // +++++++++++++++++++++++++ME CODE SECTION+++++++++++++++++++++++++

        //Are we looking for me codes?
        if (searchVal.substring(0, 1) == '$') {

            //Yes - ME CODES
            $("#listItemHolder #me-codes-divider").moveTo("#contactList");
           

            if (searchVal.length > 3) {
                //LONG ENOUGH TO LOOK
                contactsSearchController.searchAndDisplayMeCodes(searchVal, "Non-Profits");
            } else if (searchVal.length < 1) {
                //BACKED DOWN TO NOTHING
                $("#contactList #me-code-search-item").remove();
                $("#contactList #me-codes-divider").remove();
                $("#listItemHolder #contact-no-results").moveTo("#contactList");
                contactsSearchController.clearMeCodes();
            } else {
                //STILL NOT LONG ENOUGH TO LOOK
                contactsSearchController.clearMeCodes();
                //Move Search Helper
                $("#listItemHolder #me-code-search-item").moveTo("#contactList");
            }


            // +++++++++++++++++++++++++SEARCHING/ADDING NEW AREA +++++++++++++++++++++++++
        }
        else {

            //reset filter within search area
            $("#searchListHolder li").show().removeClass("filteredOut").addClass("notFiltered");

            //Is there at least 1 character being searched?
            if (searchVal.length > 0) {

                //Yes - check to see if any items in contact list
                if (!$("#searchListHolder li").length) {

                    //no there are not contacts in the list
                    $("#listItemHolder #search-item").moveTo("#contactList");

                    //yes there are contacts in the list
                } else {
                    //we have contacts to search through, let's filter
                    var filter = searchVal;

                    //filter them out
                    $("#searchListHolder").find("li:icontains(" + filter + ")").show().removeClass("filteredOut").addClass("notFiltered");
                    $("#searchListHolder").find("li:not(:icontains(" + filter + "))").hide().removeClass("notFiltered").addClass("filteredOut");

                    //are any filtered list items visible?
                    if ($("#searchListHolder li.notFiltered").length > 0) {
                        $("#searchListHolder li.notFiltered").moveTo("#contactList");
                        //hide all list dividers
                        $("#contactList li.list-divider").hide().removeClass("notFiltered").addClass("filteredOut");
                    } else {
                        $("#listItemHolder #cause-no-results").moveTo("#contactList");
                       
                    }



                }

            } else {

                //no there is not at least 1 character being searched
                //are any filtered list items visible?
                if ($("#searchListHolder li.notFiltered").length > 0) {
                    $("#searchListHolder li").moveTo("#contactList");
                    //show all list dividers
                    $("#contactList li.list-divider").show().removeClass("filteredOut").addClass("notFiltered");
                } else {
                    $("#listItemHolder #cause-no-results").moveTo("#contactList");
                }
            }

            //LAZY LOAD
            $("#contactList").find("img.lazy").lazyload();

            //HIDE LOADING
            $('#page-loader').stop().animate({
                opacity: 0.7
            }, 300, function () {
                $('#page-loader').hide();
            });

        }
    });

    contactlist.on("click", ".contact-recipient-uri", function (event) {

        $(this).parents("form:first").submit();

    });

    contactlist.on("click", ".contact-new-recipient", function (event) {
        if ($(this).attr('data-uri-valid') == '1') {
            $(this).parents("form:first").submit();
        }
    });
});
        