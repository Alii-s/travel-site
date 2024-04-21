

$(function () {
    $(document).scroll(function () {
        var $nav = $("#mainNavbar");
        var scrollDistance = 1;
        $nav.toggleClass("scrolled", $(this).scrollTop() > scrollDistance);
    });
});
$(document).ready(function(){
    $('.owl').owlCarousel({
        loop:true,
        autoplay:true,
        autoplayTimeout:3000,
        autoplayHoverPause:true,
        margin:10,
        dots:false,
        nav:true,
        responsive:{
            0:{
                items:1
            },
            600:{
                items:2
            },
            1000:{
                items:3
            }
        }
    });
});

$(document).ready(function(){
    $('.owl1').owlCarousel({
        loop:true,
        autoplay:true,
        autoplayTimeout:3000,
        autoplayHoverPause:true,
        margin:10,
        dots:true,
        nav:true,
        responsive: {
            0: {
                items: 1 // Show only one item on small screens
            }
        }
    });
});