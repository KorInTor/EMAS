class TableFilter {

    constructor(tableId) {
        this.tableId = tableId;
        
        this.filterMenus = new Map();
        this.filterOptions = new Map();
        this.rangeOptions = new Map();
        this.divMenus = new Array();

        this.sortColumnIndex = 0;
        this.sortOrderDesc = true;
    }

    addNewMenu(divMenu) {
        const columnIndex = parseInt(divMenu.dataset.columnIndex);
        if (isNaN(columnIndex)) {
            throw new Error("Cannot Read Column Index");
        }

        this.filterMenus.set(columnIndex, divMenu);
        if (divMenu.querySelector('div.filter-options')) {
            this.filterOptions.set(columnIndex, []);
        }
        if (divMenu.querySelector('div.dateRange')) {
            this.rangeOptions.set(columnIndex, [new Date(-8640000000000000), new Date(8640000000000000)]);
        }
        if (divMenu.querySelector('div.numRange')) {
            this.rangeOptions.set(columnIndex, [Number.NEGATIVE_INFINITY, Number.POSITIVE_INFINITY]);
        }
        this.divMenus.push(divMenu);
    }

    build() {
        this.table = document.getElementById(this.tableId);
        if (!this.table) {
            throw new Error(`Таблица с id "${this.tableId}" не найдена.`);
        }

        this.totalRows = this.table.querySelector('tbody').querySelectorAll('tr').length;
        this.visibleRowsCount = this.totalRows;
        updateRowsCount(this.tableId, this.visibleRowsCount);
        bindButtons(this.divMenus, this.tableId, this);
    }

    updateFilterValues() {
        this.divMenus.forEach(divMenu => {
            const columnIndex = parseInt(divMenu.dataset.columnIndex);
            if (this.filterOptions.has(columnIndex)) {
                this.filterOptions.set(columnIndex, extractFilterOptions(divMenu))
            }
            if (this.rangeOptions.has(columnIndex)) {
                this.rangeOptions.set(columnIndex, extractRangeOptions(divMenu, columnIndex));
            }
        });
    }

    clearAllFilters() {

    }

}

function revertChanges(divMenu, tableFilterObject) {
    const columnIndex = parseInt(divMenu.dataset.columnIndex);
    if (tableFilterObject.filterOptions.has(columnIndex)) {
        const checkBoxes = Array.from(divMenu.querySelectorAll('input[type=checkbox].filter-value'));
        divMenu.querySelector('#filter_searchBar').value = '';

        const filterValues = tableFilterObject.filterOptions.get(columnIndex);

        checkBoxes.forEach(checkBox => {
            let matches;
            if (filterValues.length === 0) {
                matches = false;
            } else {
                matches = filterValues.some(filterValue => checkBox.value.toLowerCase() === filterValue.toLowerCase());
            }
            checkBox.checked = matches;
        });
    }
    if (tableFilterObject.rangeOptions.has(columnIndex)) {
        if (tableFilterObject.rangeOptions.get(columnIndex)[0] instanceof Date) {
            divMenu.querySelector('input.rangeMin').value = tableFilterObject.rangeOptions.get(columnIndex)[0].toISOString().split('T')[0];
            divMenu.querySelector('input.rangeMax').value = tableFilterObject.rangeOptions.get(columnIndex)[1].toISOString().split('T')[0];
        }
        if (typeof tableFilterObject.rangeOptions.get(columnIndex)[0] === "number") {
            divMenu.querySelector('input.rangeMin').value = tableFilterObject.rangeOptions.get(columnIndex)[0];
            divMenu.querySelector('input.rangeMax').value = tableFilterObject.rangeOptions.get(columnIndex)[1];
        }
    }
}

function sortTable(columnIndex, orderDescending, table) {
    const tbody = table.tBodies[0];
    const rows = Array.from(tbody.rows);

    const isNumeric = !isNaN(rows[0].cells[columnIndex].innerText.trim());

    if (orderDescending) {
        rows.sort((a, b) => {
            const cellA = a.cells[columnIndex].innerText.trim();
            const cellB = b.cells[columnIndex].innerText.trim();

            if (isNumeric) {
                return parseFloat(cellB) - parseFloat(cellA);
            } else {
                return cellB.localeCompare(cellA);
            }
        });
    }
    else {
        rows.sort((a, b) => {
            const cellA = a.cells[columnIndex].innerText.trim();
            const cellB = b.cells[columnIndex].innerText.trim();
            if (isNumeric) {
                return parseFloat(cellA) - parseFloat(cellB); // Для чисел
            } else {
                return cellA.localeCompare(cellB); // Для строк
            }
        });
    }

    tbody.innerHTML = "";

    rows.forEach(row => tbody.appendChild(row));
}

function setCheckAllChildElements(containerWithCheckboxes, checked) {
    const elements = Array.from(containerWithCheckboxes.querySelectorAll('input[type=checkbox]:not(.d-none)'))
        .filter(el => el.style.display !== 'none');

    elements.forEach(checkBox => {
        checkBox.checked = checked;
    });
}

function extractFilterOptions(divMenu) {
    const selectedFilterValues = getSelectedCheckboxValues(divMenu.querySelector('div.filter-options'));

    const filterValues = [];

    selectedFilterValues.forEach(selectedFilterValue => { filterValues.push(selectedFilterValue); });

    return filterValues;
}

function extractRangeOptions(divMenu) {
    const rangeMinInput = divMenu.querySelector('input.rangeMin');
    const rangeMaxInput = divMenu.querySelector('input.rangeMax');
    const minValue = rangeMinInput.value;
    const maxValue = rangeMaxInput.value;

    const range = [];

    if (rangeMinInput.classList.contains('dateRange')) {
        range[0] = minValue ? new Date(minValue) : new Date(-8640000000000000);
        range[1] = maxValue ? new Date(maxValue) : new Date(8640000000000000);
    } else if (rangeMinInput.classList.contains('numRange')) {
        range[0] = minValue ? parseInt(minValue, 10) : Number.NEGATIVE_INFINITY;
        range[1] = maxValue ? parseInt(maxValue, 10) : Number.POSITIVE_INFINITY;
    } else {
        throw new Error('Value should be either a Number (as a string) or a Date (as a string)');
    }

    return range;
}

function filterTable(filterOptions, rangeOptions, table, totalRows) {
    const rows = Array.from(table.tBodies[0].rows);
    let visibleRowsCount = totalRows;

    rows.forEach(row => {
        let matches = true;

        for (let cell of row.cells) {
            const filterValues = filterOptions.get(cell.cellIndex) || [];
            const rangeValues = rangeOptions.get(cell.cellIndex) || [];

            if (filterValues.length === 0 && rangeValues.length === 0) continue;

            const cellValue = cell.textContent;

            if (filterValues.length > 0) {
                matches = this.matchesFilter(filterValues, cellValue);
            }
            if (matches && rangeValues.length > 0) {
                matches = this.matchesRange(rangeValues[0], rangeValues[1], cellValue);
            }

            if (!matches) break;
        }
        setDisplayNone(row, !matches);
        if (!matches) this.visibleRowsCount--;
    });

    return visibleRowsCount;
}

function matchesRange(minValue, maxValue, value) {
    if (minValue instanceof Date) {
        const [day, month, year, time] = value.split(/[\s.]+/);
        const isoString = `${year}-${month}-${day}T${time}`;
        const dateValue = new Date(isoString);
        return dateValue >= minValue && dateValue <= maxValue;
    }
    if (typeof minValue === "number") {
        const numValue = Number(value);
        return numValue >= minValue && numValue <= maxValue;
    }
    throw new Error('Value should be either a Number (as a string) or a Date (as a string)');
}

function matchesFilter(filterValues, value) {
    return filterValues.some(filterValue => value.trim().toLowerCase() === filterValue.trim().toLowerCase());
}

function updateRowsCount(tableId, visibleRowsCount) {
    document.querySelector(`span#${tableId}_visibleRowsCount`).textContent = visibleRowsCount;
}

function getCheckedSortRadio(divMenus, tableId) {
    for (const divMenu of divMenus) {
        const checkedRadio = divMenu.querySelector(`input[name="${tableId}_sorter"]:checked`);
        if (checkedRadio) {
            return checkedRadio;
        }
    }
    return null;
}

function filterTableWithParameters(tableFilterObject) {
    let activeRadio = getCheckedSortRadio(tableFilterObject.divMenus, tableFilterObject.tableId);
    if (!activeRadio) {
        for (const divMenu of tableFilterObject.divMenus) {
            activeRadio = divMenu.querySelector(`input[name="${tableFilterObject.tableId}_sorter"]`);
            if (activeRadio) {
                activeRadio.checked = true;
                break;
            }
        }
    } 

    tableFilterObject.sortOrderDesc = activeRadio.classList.contains('sort-desc');
    tableFilterObject.sortColumnIndex = Number(activeRadio.value);

    tableFilterObject.updateFilterValues();

    tableFilterObject.visibleRowsCount = filterTable(tableFilterObject.filterOptions, tableFilterObject.rangeOptions, tableFilterObject.table, tableFilterObject.totalRows);

    sortTable(tableFilterObject.sortColumnIndex, tableFilterObject.sortOrderDesc, tableFilterObject.table);

    updateRowsCount(tableFilterObject.tableId, tableFilterObject.visibleRowsCount);

}

function bindButtons(divMenus, tableId, tableFilterObject) {
    // Привязка контекста для кнопок cancel и apply
    divMenus.forEach(divMenu => {
        const applyButton = divMenu.querySelector(`button#applyFilter_${tableId}`);
        const cancelButton = divMenu.querySelector(`button#cancelFilter_${tableId}`);

        applyButton.addEventListener('click', () => {
            filterTableWithParameters(tableFilterObject);
            closeFilterDropdown(divMenu);
        });

        cancelButton.addEventListener('click', () => {
            revertChanges(divMenu ,tableFilterObject);
            closeFilterDropdown(divMenu);
        });
    });
}

function closeFilterDropdown(divMenu) {
    divMenu.classList.remove('show');
}

function setCheckAllVisibleChildElements(containerWithCheckboxes, checked) {

    for (let checkbox of Array.from(containerWithCheckboxes.querySelectorAll('input[type=checkbox]'))) {
        if (window.getComputedStyle(checkbox).display === 'none') {
            continue;
        }
        checkbox.checked = checked;
    }

}

function filterList(filterValue, elementWithList) {
    const listItems = elementWithList.querySelectorAll('li');

    listItems.forEach(listItem => {
        const checkBox = listItem.querySelector('input[type=checkbox]');
        setDisplayNone(listItem, !checkBox.value.toLowerCase().includes(filterValue.toLowerCase()));
    });

}

function getSelectedCheckboxValues(checkBoxContainer) {
    var selectedValues = [];
    if (!checkBoxContainer) {
        throw new Error('No checkBoxContainer Provided');
    }
    var checkboxes = checkBoxContainer.querySelectorAll('input[type="checkbox"]:checked');
    if (!checkboxes) {
        throw new Error('There is no checkboxes founded');
    }
    checkboxes.forEach(function (checkbox) {
        selectedValues.push(checkbox.value);
    });
    return selectedValues;
}

function loadSavedFilterData(tableFilterObject) {
    const MAP_PREFIX = "Map:";
    const savedData = getCookie(tableFilterObject.tableId, true);

    if (!savedData) {
        console.warn("No saved filter data found for table:", tableFilterObject.tableId);
        return false;
    }

    const jsonValue = savedData.split("^");
    if (jsonValue.length < 3) {
        console.error("Invalid saved filter data format");
        return false;
    }

    tableFilterObject.filterOptions = new Map(JSON.parse(jsonValue[1].substring(MAP_PREFIX.length)));
    tableFilterObject.rangeOptions = new Map(JSON.parse(jsonValue[2].substring(MAP_PREFIX.length)));

    return true;
}

function saveFilterData(tableFilterObject) {
    if (!tableFilterObject) {
        throw new Error("tableFilter is undefined");
    }
    if (!(tableFilterObject instanceof TableFilter)) {
        throw new Error("Unsupported type");
    }

    const MAP_PREFIX = "Map:";
    const saveData = tableFilterObject.tableId + "^"
        + MAP_PREFIX + JSON.stringify(Array.from(tableFilterObject.filterOptions.entries()))
        + "^"
        + MAP_PREFIX + JSON.stringify(Array.from(tableFilterObject.rangeOptions.entries()));

    setCookie(tableFilterObject.tableId, saveData, 1);
}
