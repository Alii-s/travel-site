const imgInput = document.querySelector("#imgExtension");
const nameInput = document.querySelector("#imageName");
const removeForm = document.querySelector('.remove');
const select = document.querySelector('#id');
const owl = document.querySelector('.owl');
function initializeCarousel() {
    var carousel = document.querySelector('.owl');
    console.log('Carousel found')
    if (carousel) {
        $(carousel).owlCarousel({
            loop: true,
            autoplay: true,
            autoplayTimeout: 3000,
            autoplayHoverPause: true,
            margin: 10,
            dots: false,
            nav: true,
            responsive: {
                0: {
                    items: 1
                },
                600: {
                    items: 2
                },
                1000: {
                    items: 3
                }
            }
        });
    }
}
document.addEventListener('htmx:afterSwap', function (event) {
    initializeCarousel();
});

$(function () {
    $(document).scroll(function () {
        var $nav = $("#mainNavbar");
        var scrollDistance = 1;
        $nav.toggleClass("scrolled", $(this).scrollTop() > scrollDistance);
    });
});

$(document).ready(function () {
    $('.owl1').owlCarousel({
        loop: true,
        autoplay: true,
        autoplayTimeout: 3000,
        autoplayHoverPause: true,
        margin: 10,
        dots: true,
        nav: true,
        responsive: {
            0: {
                items: 1 // Show only one item on small screens
            }
        }
    });
});


function validate() {
    if (nameInput.value === "") {
        document.querySelector(".nameValidation").classList.remove("d-none");
        return false;
    } else {
        document.querySelector(".nameValidation").classList.add("d-none");
    }
    if (imgInput.value === "" || !imgInput.value.match(/\.(jpeg|png|jpg|gif)$/)) {
        document.querySelector(".imgValidation").classList.remove("d-none");
        return false;
    } else {
        document.querySelector(".imgValidation").classList.add("d-none");
    }
    return true;
}
select.addEventListener('change', function () {
    // Get the selected option's value
    const selectedId = this.value;
    removeForm.action = '/api/remove/' + selectedId;
});
removeForm.addEventListener('submit', function (event) {
    event.preventDefault();
    const selectedId = select.value;
    if (selectedId !== "") {
        fetch(removeForm.action, {
            method: 'DELETE',
        })
            .then(response => {
                if (response.ok) {
                    console.log('Item removed');
                }
            });
    }
});
imgInput.addEventListener("change", validate);
nameInput.addEventListener("change", validate);