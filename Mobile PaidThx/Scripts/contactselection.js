        $(document).ready(function () {

            //LAZY LOAD
            $(this).find("img.lazy").lazyload();

            //cache the lists for later use
            var contactlist = $("#contactList");

            $('.contact-top-input').bind('keyup', function () {

                //Runs every time user types in field

                //Empty the contact list element
                $("#contactList").empty();

                //Get search string
                var searchVal = $('.contact-top-input').val();

                //Are we looking for me codes?
                if (searchVal.substring(0, 1) == '$') {

                    //Yes - ME CODES
                    $("#listItemHolder #me-codes-divider").moveTo("#contactList");
                    //Move Search Helper
                    $("#listItemHolder #me-code-search-item").moveTo("#contactList");

                    if (searchVal.length > 3) {
                        //LONG ENOUGH TO LOOK
                        contactsSearchController.searchAndDisplayMeCodes(searchVal);
                    } else if (searchVal.length < 1) {
                        //BACKED DOWN TO NOTHING
                        $("#contactList #me-code-search-item").remove();
                        $("#contactList #me-codes-divider").remove();
                        $("#listItemHolder #contact-no-results").moveTo("#contactList");
                        contactsSearchController.clearMeCodes();
                    } else {
                        //STILL NOT LONG ENOUGH TO LOOK
                        contactsSearchController.clearMeCodes();
                    }
                }
                else {

                    //NO ME CODES - WE ARE SEARCHING OR ADDING NEW

                    //Is there at least 1 character being searched?
                    if (searchVal.length > 0) {

                        //Yes - check to see if any items in list (will be empty now..but used for filtering)
                        if (!$("#contactList li").length) {
                            $("#listItemHolder #search-item").moveTo("#contactList");
                            contactsSearchController.showNoResults(searchVal);
                        } else {
                            contactsSearchController.hideNoResults(searchVal);
                            $("#contactList #search-item").remove();
                        }

                    } else {
                        $("#listItemHolder #contact-no-results").moveTo("#contactList");
                        $("#contactList #search-item").remove();
                    }

                    //check to see if there is anything left in our contact list
                    if (!$("#contactList li").length) {
                        contactsSearchController.showNoResults(searchVal);

                    } else {
                        contactsSearchController.hideNoResults(searchVal);
                    }
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
        