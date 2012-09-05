
jQuery(document).ready(function ($) {

    /* Use this js doc for all application specific JS */

    /* TABS --------------------------------- */
    /* Remove if you don't need :) */
    /*
    function activateTab($tab) {
    var $activeTab = $tab.closest('dl').find('a.active'),
    contentLocation = $tab.attr("href") + 'Tab';

    //Make Tab Active
    $activeTab.removeClass('active');
    $tab.addClass('active');

    //Show Tab Content
    $(contentLocation).closest('.tabs-content').children('li').hide();
    $(contentLocation).css('display', 'block');
    }

    $('dl.tabs').each(function () {
    //Get all tabs
    var tabs = $(this).children('dd').children('a');
    tabs.click(function (e) {
    activateTab($(this));
    });
    });
    */
    if (window.location.hash) {
        activateTab($('a[href="' + window.location.hash + '"]'));
    }

    /* ALERT BOXES ------------ */
    $(".alert-box").delegate("a.close", "click", function (event) {
        event.preventDefault();
        $(this).closest(".alert-box").fadeOut(function (event) {
            $(this).remove();
        });
    });


    /* PLACEHOLDER FOR FORMS ------------- */
    /* Remove this and jquery.placeholder.min.js if you don't need :) */

    $('input, textarea').placeholder();

    /* TOOLTIPS ------------ */

    /* SCREEN - SPECIFIC */
      /*
    (function () {
        // initializes touch and scroll events
        var supportTouch = $.support.touch,
                scrollEvent = "touchmove scroll",
                touchStartEvent = supportTouch ? "touchstart" : "mousedown",
                touchStopEvent = supportTouch ? "touchend" : "mouseup",
                touchMoveEvent = supportTouch ? "touchmove" : "mousemove";


                      // handles swipeup and swipedown
        $.event.special.swipeupdown = {
            setup: function () {
                var thisObject = this;
                var $this = $(thisObject);

                $this.bind(touchStartEvent, function (event) {
                    var data = event.originalEvent.touches ?
                            event.originalEvent.touches[0] :
                            event,
                            start = {
                                time: (new Date).getTime(),
                                coords: [data.pageX, data.pageY],
                                origin: $(event.target)
                            },
                            stop;

                    function moveHandler(event) {
                        if (!start) {
                            return;
                        }

                        var data = event.originalEvent.touches ?
                                event.originalEvent.touches[0] :
                                event;
                        stop = {
                            time: (new Date).getTime(),
                            coords: [data.pageX, data.pageY]
                        };

                        // prevent scrolling
                        if (Math.abs(start.coords[1] - stop.coords[1]) > 10) {
                            event.preventDefault();
                        }
                    }

                    $this
                            .bind(touchMoveEvent, moveHandler)
                            .one(touchStopEvent, function (event) {
                                $this.unbind(touchMoveEvent, moveHandler);
                                if (start && stop) {
                                    if (stop.time - start.time < 1000 &&
                                    Math.abs(start.coords[1] - stop.coords[1]) > 30 &&
                                    Math.abs(start.coords[0] - stop.coords[0]) < 75) {
                                        start.origin
                                        .trigger("swipeupdown")
                                        .trigger(start.coords[1] > stop.coords[1] ? "swipeup" : "swipedown");
                                    }
                                }
                                start = stop = undefined;
                            });
                });
            }
        };

        //Adds the events to the jQuery events special collection
        $.each({
            swipedown: "swipeupdown",
            swipeup: "swipeupdown"
        }, function (event, sourceEvent) {
            $.event.special[event] = {
                setup: function () {
                    $(this).bind(sourceEvent, $.noop);
                }
            };
        });

    })();


    $("#quicksends").live('swipeup', function () {
        var widthy = $('#quicksends').height();
        $('.quicksend-container').animate({
            height: (widthy) + 'px'
        }, {
            duration: 300,
            complete: function () {

            }
        });
    });
    $("#quicksends").live('swipedown', function () {
        $('.quicksend-container').animate({
            height: '100px'
        }, {

            duration: 300,
            complete: function () {

            }
        });
    });
    */


});

