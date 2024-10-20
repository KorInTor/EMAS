class TableFilter {

    constructor(tableId) {
        this.tableId = tableId;
        
        this.filterMenus = new Map();
        this.activeSort;
        this.filterOptions = new Map();
        this.rangeOptions = new Map();
        this.divMenus = new Array();

        this.sortColumnIndex = 0;
        this.sortOrderDesc = true;
    }

    addNewMenu(divMenu) {
        const columnIndex = parseInt(divMenu.dataset.columnIndex);
        if (!columnIndex) {
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
            throw new Error(`Таблица с id "${tableId}" не найдена.`);
        }

        this.totalRows = this.table.querySelector('tbody').querySelectorAll('tr').length;
        this.visibleRowsCount = this.totalRows;
        this.updateRowsCount();
        this.bindButtons();
    }

    updateFilterValues() {
        this.divMenus.forEach(divMenu => {
            const columnIndex = parseInt(divMenu.dataset.columnIndex);
            if (this.filterOptions.has(columnIndex)) {
                this.extractFilterOptions(divMenu, columnIndex);
            }
            if (this.rangeOptions.has(columnIndex)) {
                this.extractRangeOptions(divMenu, columnIndex);
            }
        });
    }

    extractFilterOptions(divMenu, columnIndex) {
        const selectedFilterValues = getSelectedCheckboxValues(divMenu.querySelector('div.filter-options'));

        this.filterOptions.get(columnIndex).length = 0;

        selectedFilterValues.forEach(selectedFilterValue => { this.filterOptions.get(columnIndex).push(selectedFilterValue); });
    }

    extractRangeOptions(divMenu, columnIndex) {
        const rangeMinInput = divMenu.querySelector('input.rangeMin');
        const rangeMaxInput = divMenu.querySelector('input.rangeMax');
        const minValue = rangeMinInput.value;
        const maxValue = rangeMaxInput.value;

        const range = this.rangeOptions.get(columnIndex) || [];

        if (rangeMinInput.classList.contains('dateRange')) {
            range[0] = minValue ? new Date(minValue) : new Date(-8640000000000000);
            range[1] = maxValue ? new Date(maxValue) : new Date(8640000000000000);
        } else if (rangeMinInput.classList.contains('numRange')) {
            range[0] = minValue ? parseInt(minValue, 10) : Number.NEGATIVE_INFINITY;
            range[1] = maxValue ? parseInt(maxValue, 10) : Number.POSITIVE_INFINITY;
        } else {
            throw new Error('Value should be either a Number (as a string) or a Date (as a string)');
        }

        this.rangeOptions.set(columnIndex, range);
    }


    revertChanges(divMenu) {
        const columnIndex = parseInt(divMenu.dataset.columnIndex);
        if (this.filterOptions.has(columnIndex)) {
            const checkBoxes = Array.from(divMenu.querySelectorAll('input[type=checkbox].filter-value'));
            divMenu.querySelector('#filter_searchBar').value = '';

            const filterValues = this.filterOptions.get(columnIndex);

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
        if (this.rangeOptions.has(columnIndex)) {
            if (this.rangeOptions.get(columnIndex)[0] instanceof Date) {
                divMenu.querySelector('input.rangeMin').value = this.rangeOptions.get(columnIndex)[0].toISOString().split('T')[0];
                divMenu.querySelector('input.rangeMax').value = this.rangeOptions.get(columnIndex)[1].toISOString().split('T')[0];
            }
            if (typeof this.rangeOptions.get(columnIndex)[0] === "number") {
                divMenu.querySelector('input.rangeMin').value = this.rangeOptions.get(columnIndex)[0];
                divMenu.querySelector('input.rangeMax').value = this.rangeOptions.get(columnIndex)[1];
            }
        }
    }

    clearAllFilters() {

    }

    sortTable(columnIndex, orderDescending) {
        const tbody = this.table.tBodies[0];
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

    filterTableWithParameters() {
        this.setSortOrder();

        this.updateFilterValues();

        this.filterTable();

        this.sortTable(this.sortColumnIndex, this.sortOrderDesc);

        this.updateRowsCount();
    }

    findCheckedSortOrderRadio() {
        return this.divMenus.find(divMenu => {
            const checkedRadio = divMenu.querySelector(`div.sorting input[name="${this.tableId}_sorter"]:checked`);
            return checkedRadio;
        })?.querySelector(`div.sorting input[name="${this.tableId}_sorter"]:checked`) || NaN;
    }


    setSortOrder() {
        this.activeSortRadio = this.findCheckedSortOrderRadio();

        if (!this.activeSortRadio) {
            this.activeSortRadio = this.divMenus[0].querySelector('input.sort-desc');
            this.activeSortRadio.checked = true;
        }

        this.sortColumnIndex = parseInt(this.activeSortRadio.value);

        if (this.activeSortRadio.classList.contains('sort-desc')) {
            this.sortOrderDesc = true;
        } else {
            this.sortOrderDesc = false;
        }
    }

    setCheckAllChildElements(containerWithCheckboxes, checked) {
        const elements = Array.from(containerWithCheckboxes.querySelectorAll('input[type=checkbox]:not(.d-none)'))
            .filter(el => el.style.display !== 'none');

        elements.forEach(checkBox => {
            checkBox.checked = checked;
        });
    }

    filterTable() {
        const rows = Array.from(this.table.tBodies[0].rows);
        this.visibleRowsCount = this.totalRows;

        rows.forEach(row => {
            let matches = true;

            for (let cell of row.cells) {
                const filterValues = this.filterOptions.get(cell.cellIndex) || [];
                const rangeValues = this.rangeOptions.get(cell.cellIndex) || [];

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
    }

    matchesRange(minValue, maxValue, value) {
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

    matchesFilter(filterValues, value) {
        return filterValues.some(filterValue => value.trim().toLowerCase() === filterValue.trim().toLowerCase());
    }

    updateRowsCount() {
        document.querySelector(`span#${this.table.id}_visibleRowsCount`).textContent = this.visibleRowsCount;
    }

    bindButtons() {
        // Привязка контекста для кнопок cancel и apply
        this.divMenus.forEach(divMenu => {
            const applyButton = divMenu.querySelector(`button#applyFilter_${this.table.id}`);
            const cancelButton = divMenu.querySelector(`button#cancelFilter_${this.table.id}`);

            const dropdown = applyButton.closest('.filter-dropdown');

            applyButton.addEventListener('click', () => {
                this.filterTableWithParameters(dropdown);
                this.closeFilterDropdown(dropdown);
            });

            cancelButton.addEventListener('click', () => {
                this.revertChanges(dropdown);
                this.closeFilterDropdown(dropdown);
            });
        });
    }


    closeFilterDropdown(divMenu) {
        divMenu.classList.remove('show');
    }
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