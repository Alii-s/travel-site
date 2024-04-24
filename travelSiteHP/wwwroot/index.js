
document.addEventListener('htmx:afterSwap', function(event) {
    var carousel = document.querySelector('.owl');

    if (carousel) {
        $(carousel).owlCarousel({
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
    }
});

$(function () {
    $(document).scroll(function () {
        var $nav = $("#mainNavbar");
        var scrollDistance = 1;
        $nav.toggleClass("scrolled", $(this).scrollTop() > scrollDistance);
    });
});
// $(document).ready(function(){
//     $('.owl').owlCarousel({
//         loop:true,
//         autoplay:true,
//         autoplayTimeout:3000,
//         autoplayHoverPause:true,
//         margin:10,
//         dots:false,
//         nav:true,
//         responsive:{
//             0:{
//                 items:1
//             },
//             600:{
//                 items:2
//             },
//             1000:{
//                 items:3
//             }
//         }
//     });
// });

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

// Function to add a new carousel item
function addCarouselItem(imageUrl, title) {
    up.request('/api/add-carousel-item', {
        method: 'POST',
        data: {
            imageUrl: imageUrl,
            title: title
        },
        // Handle success
        success: function(response) {
            // Append the new item to the carousel
            $('#carousel').append(response.html);
        },
        // Handle errors
        error: function(xhr) {
            console.error('Error adding carousel item:', xhr.responseText);
            // Handle error display or logging
        }
    });
}

// Function to remove a carousel item
function removeCarouselItem(itemId) {
    up.request('/api/remove-carousel-item', {
        method: 'POST',
        data: {
            itemId: itemId
        },
        // Handle success
        success: function(response) {
            // Remove the item from the carousel
            $('#carousel-item-' + itemId).remove();
        },
        // Handle errors
        error: function(xhr) {
            console.error('Error removing carousel item:', xhr.responseText);
            // Handle error display or logging
        }
    });
}