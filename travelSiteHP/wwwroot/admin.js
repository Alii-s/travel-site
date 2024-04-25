const imgInput = document.querySelector("#imgExtension");
const nameInput = document.querySelector("#imageName");
const select = document.querySelector('#id');
const removeForm = document.querySelector('.remove');
const form = document.querySelector("form");
const fetchItems = () => {
    fetch('/api/items')
    .then(response => {
        // Check if response is successful
        if (!response.ok) {
            throw new Error('Network response was not ok');
        }
        // Parse response as JSON
        return response.json();
    })
    .then(data => {
        // Get the select element by id
        var select = document.getElementById('id');

        // Clear existing options
        select.innerHTML = '';
        var defaultOption = document.createElement('option');
        defaultOption.value = '';
        defaultOption.text = 'Please select an item';
        defaultOption.disabled = true;
        defaultOption.selected = true;
        defaultOption.hidden = true;
        defaultOption.value = '';
        select.appendChild(defaultOption);
        // Iterate over the items and populate the select dropdown
        data.forEach(function(item) {
            var option = document.createElement('option');
            option.value = item.id;
            option.text = item.title;
            select.appendChild(option);
        });
    })
    .catch(error => {
        // Handle errors
        console.error('Error fetching items:', error);
    });
    const selectedId = select.value;
    removeForm.action = '/api/remove/' + selectedId;
}
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

imgInput.addEventListener("change", validate);
nameInput.addEventListener("change", validate);
document.addEventListener('DOMContentLoaded', fetchItems);
select.addEventListener('change', function() {
    // Get the selected option's value
    const selectedId = this.value;
    removeForm.action = '/api/remove/' + selectedId;
});
removeForm.addEventListener('submit', function(event) {
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

