// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function setOnlyOneElementVisible(elemntToSetVisible, elementsToHide) {
    elementsToHide.forEach(element => {
        element.style.setProperty('display', 'none', 'important');
    });
    elemntToSetVisible.style.setProperty('display', '', 'important');
}

function setDisplayNone(element, isNone) {
    for (let child of element.children) {
        setDisplayNone(child, isNone);
    }
    element.style.setProperty('display', isNone ? 'none' : '', 'important');
}

function setDisplayNoneSingle(element, isNone) {
    element.style.setProperty('display', isNone ? 'none' : '', 'important');
}

function addEventToast(newEvent) {
    const toastElement = document.createElement('div');
    toastElement.classList.add('toast');
    toastElement.setAttribute('role', 'alert');
    toastElement.setAttribute('data-bs-autohide', 'false');
    toastElement.setAttribute('aria-live', 'assertive');
    toastElement.setAttribute('aria-atomic', 'true');

    let headerText = "Новое событие: ";
    switch (newEvent.EventType) {
        case 6:
            headerText = headerText + "Добавление";
            break;
        case 1:
            headerText = headerText + "Доставка завершена";
            break;
        case 2:
            headerText = headerText + "Отправление";
            break;
        case 4:
            headerText = headerText + "Зарезервировано";
            break;
        case 5:
            headerText = headerText + "Резерв Оконочен";
            break;
        case 3:
            headerText = headerText + "Реализация";
            break;
        case 7:
            headerText = headerText + "Данные изменены";
            break;
        default:
            console.log(`Unexpected newEvent Type ${newEvent.EventType}.`);
    }

    const toastHead = document.createElement('div');
    toastHead.classList.add('toast-header');
    toastHead.innerHTML = `
    <strong class="me-auto">${headerText}</strong>
    <button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Close"></button>
    `;

    const toastBody = document.createElement('div');
    toastBody.classList.add('toast-body');
    toastBody.appendChild(buildnewEventToastBody(newEvent));

    toastElement.appendChild(toastHead);
    toastElement.appendChild(toastBody);
    document.querySelector('.toast-container').appendChild(toastElement);

    const toast = new bootstrap.Toast(toastElement);
    toast.show();
}

function buildnewEventToastBody(newEvent)
{
    function makeListItem(textContent) {
        const infoItem = document.createElement('li');
        infoItem.classList.add('list-group-item');
        infoItem.textContent = textContent;
        return infoItem;
    }

    const infoList = document.createElement('ul');
    infoList.classList.add('list-group');
    infoList.classList.add('list-group-flush');

    infoList.appendChild(makeListItem("Сотрудник: " + newEvent.employeeInfo));

    infoList.appendChild(makeListItem("Дата и время: " + newEvent.DateTime));

    switch (newEvent.EventType) {
        case 6:
            infoList.appendChild(makeListItem("На " + newEvent.LocationName));
            break;
        case 1:
            infoList.appendChild(makeListItem("Отправлено из " + newEvent.DepartureName));
            infoList.appendChild(makeListItem("На " + newEvent.DestinationName));
            infoList.appendChild(makeListItem("Коментарий " + newEvent.Comment));
            break;
        case 2:
            infoList.appendChild(makeListItem("Отправлено из " + newEvent.DepartureName));
            infoList.appendChild(makeListItem("На " + newEvent.DestinationName));
            infoList.appendChild(makeListItem("Коментарий " + newEvent.Comment));
            break;
        case 4:
            infoList.appendChild(makeListItem("На " + newEvent.LocationName));
            infoList.appendChild(makeListItem("Коментарий " + newEvent.Comment));
            break;
        case 5:
            infoList.appendChild(makeListItem("На " + newEvent.LocationName));
            infoList.appendChild(makeListItem("Коментарий " + newEvent.Comment));
            break;
        case 3:
            infoList.appendChild(makeListItem("На " + newEvent.LocationName));
            infoList.appendChild(makeListItem("Коментарий " + newEvent.Comment));
            break;
        case 7:
            
            break;
        default:
            console.log(`Unexpected newEvent Type ${newEvent.EventType}.`);
    }

    return infoList;
}