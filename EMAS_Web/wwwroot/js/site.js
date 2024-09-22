// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function setOnlyOneElementVisible(elemntToSetVisible, elementsToHide) {
    elementsToHide.forEach(element => {
        element.style.setProperty('display', 'none', 'important');
    });
    elemntToSetVisible.style.setProperty('display', '', 'important');
}
