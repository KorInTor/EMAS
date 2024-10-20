// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener('DOMContentLoaded', function () {
    const locationSelectorButton = document.getElementById('locationSelectorButton');
    const locationSelectorMenu = document.getElementById('locationSelector');

    function updateMenuPosition() {
        const rect = locationSelectorButton.getBoundingClientRect();
        locationSelectorMenu.style.top = `${rect.bottom + 10}px`;
        locationSelectorMenu.style.left = `${rect.left}px`;
    }

    let resizeObserver = new ResizeObserver(entries => {
        for (let entry of entries) {
            updateMenuPosition();
        }
    });

    resizeObserver.observe(locationSelectorButton);

    window.addEventListener('resize', updateMenuPosition);
    window.addEventListener('scroll', updateMenuPosition);

    updateMenuPosition();
});



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

function isElememntHidden(element) {
    let style = window.getComputedStyle(element);
    return style.display === "none";
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

function buildRowForStorableObject(storableObject, addChangeButton, lastEventDateTime) {
    function makeCell(textContent, innerHTML) {
        const newCell = document.createElement('td');
        newCell.classList.add('border');
        newCell.classList.add('border-1');
        if (textContent) {
            newCell.textContent = textContent;
        }
        if (innerHTML) {
            newCell.innerHTML = innerHTML;
        }

        return newCell;
    }

    const newRow = document.createElement('tr');
    newRow.classList.add('d-table-row');

    newRow.appendChild(makeCell(undefined, `<input class="form-check-input" type="checkbox" id="checkbox_${storableObject.Id}" value="${storableObject.Id}" aria-label="...">`));

    newRow.appendChild(makeCell(storableObject.Id, undefined));

    switch (storableObject.StorableObjectType) {
        case 1://Equipment
            newRow.appendChild(makeCell(storableObject.Type, undefined));
            newRow.appendChild(makeCell(storableObject.Name, undefined));
            newRow.appendChild(makeCell(storableObject.Manufacturer, undefined));
            newRow.appendChild(makeCell(storableObject.Units, undefined));
            newRow.appendChild(makeCell(storableObject.Limit, undefined));
            newRow.appendChild(makeCell(storableObject.FactoryNumber, undefined));
            newRow.appendChild(makeCell(storableObject.RegistrationNumber, undefined));
            newRow.appendChild(makeCell(storableObject.Status, undefined));
            newRow.appendChild(makeCell(storableObject.Description, undefined));
            break;
        case 2://Material
            newRow.appendChild(makeCell(storableObject.Type, undefined));
            newRow.appendChild(makeCell(storableObject.Name, undefined));
            newRow.appendChild(makeCell(storableObject.Units, undefined));
            newRow.appendChild(makeCell(storableObject.Amount, undefined));
            newRow.appendChild(makeCell(storableObject.Extras, undefined));
            newRow.appendChild(makeCell(storableObject.InventoryNumber, undefined));
            newRow.appendChild(makeCell(storableObject.StorageType, undefined));
            newRow.appendChild(makeCell(storableObject.Comment, undefined));
            break;
    }

    newRow.appendChild(makeCell(FormatTimeAgo(lastEventDateTime,undefined)));

    const historyHtml = `   <form method="get" action="/Archive/History">
						            <input type="hidden" name="storableObjectId" value="${storableObject.Id}">
						            <button type="submit" class="btn btn-primary">История</button>
					            </form>`;
    newRow.appendChild(makeCell(undefined, historyHtml));

    if (addChangeButton) {
        const changeCellInnerHTML = `<form method="get" action="/StorableObject/EditEquipment">
								    <input type="hidden" name="objectId" value="${storableObject.Id}">
								    <button type="submit" class="btn btn-warning">Изменить</button>
							    </form>`;
        newRow.appendChild(makeCell(undefined, changeCellInnerHTML));
    }

    return newRow;
}

function FormatTimeAgo(pastDate)
{
    console.log(pastDate);
    if (!pastDate) {
        throw new Error('Now Pastdate');
    }
    timeSpan = TimeSpan.FromDates(Date.now(),pastDate,true);

    if (timeSpan.TotalDays >= 30) {
		months = timeSpan.totalDays / 30;
        return `${months} мес.`;
    }
    else if (timeSpan.TotalDays >= 7) {
		weeks = timeSpan.totalDays / 7;
        return `${weeks} нед.`;
    }
    else if (timeSpan.TotalDays >= 1) {
		days = timeSpan.totalDays;
        return `${days} дн.`;
    }
    else if (timeSpan.TotalHours >= 1) {
		hours = timeSpan.totalHours;
        return `${hours} час.`;
    }
    else if (timeSpan.TotalMinutes >= 1) {
		minutes = timeSpan.totalMinutes;
        return `${minutes} мин.`;
    }
    else {
		seconds = timeSpan.totalSeconds;
        return `${seconds} сек.`;
    }
}