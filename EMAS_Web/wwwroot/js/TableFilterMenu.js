class TableFilterMenu {
    
    constructor(tableId, columnIndexes) {
        this.sortOrderDesc = null;
        this.sortColumnIndex = null;
        this.activeSortButton = null;
        this.table = document.querySelector(`table#${tableId}`);
        this.totalRows = this.table.querySelector('tbody').querySelectorAll('tr').length;
        this.visibleRowsCount = this.totalRows;

        if (!this.table) {
            throw new Error(`Таблица с id "${tableId}" не найдена.`);
        }

        this.filterOptions = new Map();

        columnIndexes.forEach(columnIndex => { this.filterOptions.set(columnIndex, []) });
    }
    
    sortTable(columnIndex, orderDescending) {
        const tbody = this.table.tBodies[0]; // Получаем tbody для сортировки строк
        const rows = Array.from(tbody.rows); // Преобразуем строки в массив для сортировки

        // Определяем, что сортировать - числа или строки
        const isNumeric = !isNaN(rows[0].cells[columnIndex].innerText.trim());

        // Сортируем строки

        if (orderDescending) {
            rows.sort((a, b) => {
                const cellA = a.cells[columnIndex].innerText.trim();
                const cellB = b.cells[columnIndex].innerText.trim();

                if (isNumeric) {
                    return parseFloat(cellB) - parseFloat(cellA); // По убыванию
                } else {
                    return cellB.localeCompare(cellA); // Обратный алфавитный порядок
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

        // Удаляем старые строки из tbody
        tbody.innerHTML = "";

        // Добавляем отсортированные строки обратно в таблицу
        rows.forEach(row => tbody.appendChild(row));
    }

    setOnlyOneButtonActive(buttonToSetActive, buttonsToSetDeactivate) {
        buttonsToSetDeactivate.forEach(btn => btn.classList.remove('active'));
        buttonToSetActive.classList.add('active');
    }

    filterTableWithParameters(divWithElements) {
        if (!this.activeSortButton) {
            this.activeSortButton = divWithElements.querySelector('button.sort-asc');
            this.setOnlyOneButtonActive(this.activeSortButton, this.table.querySelectorAll('.sort-asc, .sort-desc'));
        }

        //Задаём сортировку.
        const selectedSortOrderButton = divWithElements.querySelector('button.sort-asc.active, button.sort-desc.active');

        if (selectedSortOrderButton) {
            this.activeSortButton = selectedSortOrderButton;
            this.sortColumnIndex = divWithElements.dataset.columnIndex;
        }

        if (this.activeSortButton.classList.contains('sort-desc')) {
            this.sortOrderDesc = true;
        } else {
            this.sortOrderDesc = false;
        }

        //Дополняем список фильтров.
        const selectedFilterValues = Array.from(divWithElements.querySelectorAll('input[type=checkbox]:not(.d-none)'))
            .filter(el => el.style.display !== 'none' && el.checked);
        
        this.filterOptions.get(parseInt(divWithElements.dataset.columnIndex)).length = 0;

        selectedFilterValues.forEach(selectedFilterValue => { this.filterOptions.get(parseInt(divWithElements.dataset.columnIndex)).push(selectedFilterValue.value) });
        
        this.filterTable();
        this.sortTable(this.sortColumnIndex, this.sortOrderDesc);

        this.updateRowsCount();
    }

    closeFilterDropdown() {
        const shownDropdown = this.table.querySelector('div.dropdown-menu.filter-dropdown.show');
        if (shownDropdown) {
            shownDropdown.classList.remove('show');
            if (this.activeSortButton) {
                this.setOnlyOneButtonActive(this.activeSortButton, this.table.querySelectorAll('.sort-asc, .sort-desc'));
            }
        }
    }

    revertChagnes(divWithElements) {
        const checkBoxes = Array.from(divWithElements.querySelectorAll('input[type=checkbox].filter-value'));
        divWithElements.querySelector('#filter_searchBar').value = '';

        
        const filterValues = this.filterOptions.get(parseInt(divWithElements.dataset.columnIndex));

        checkBoxes.forEach(checkBox => {
            let matches;
            if (filterValues.length === 0) {
                matches = false;
            } else {
                matches = filterValues.some(filterValue => checkBox.value === filterValue.toLowerCase());
            }
            checkBox.checked = matches;
        });
    }

    setCheckAllChildElements(containerWithCheckboxes, checked) {
        const elements = Array.from(containerWithCheckboxes.querySelectorAll('input[type=checkbox]:not(.d-none)'))
            .filter(el => el.style.display !== 'none');

        elements.forEach(checkBox => {
            checkBox.checked = checked;
        });
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
                    return cellA.localeCompare(cellB);
                }
            });
        }
        else {
            rows.sort((a, b) => {
                const cellA = a.cells[columnIndex].innerText.trim();
                const cellB = b.cells[columnIndex].innerText.trim();
                if (isNumeric) {
                    return parseFloat(cellA) - parseFloat(cellB);
                } else {
                    return cellB.localeCompare(cellA);
                }
            });
        }

        tbody.innerHTML = "";

        rows.forEach(row => tbody.appendChild(row));
    }

    filterTable() {
        const rows = Array.from(this.table.tBodies[0].rows);

        this.visibleRowsCount = this.totalRows;

        rows.forEach(row => setDisplayNone(row, false));

        for (let row of rows) {
            for (let cell of row.cells) {

                if (!this.filterOptions.has(cell.cellIndex)) {
                    continue;
                }

                const filterValues = this.filterOptions.get(cell.cellIndex);

                if (filterValues.length === 0) {
                    continue;
                }
                
                const cellValue = cell.textContent.trim().toLowerCase();

                const matches = filterValues.some(filterValue => cellValue === filterValue.toLowerCase());

                if (!matches) {
                    setDisplayNone(row, true);
                    this.visibleRowsCount--;
                    break;
                }
            }
        }
    }

    updateRowsCount() {
        document.querySelector(`span#${this.table.id}_visibleRowsCount`).textContent = this.visibleRowsCount;
    }

    bindButtons() {
        // Привязка контекста для cancelFilterButton
        this.table.querySelectorAll(`button#cancelFilter_${this.table.id}`).forEach(cancelFilterButton => {
            cancelFilterButton.addEventListener('click', () => {
                this.closeFilterDropdown();
                this.revertChagnes(cancelFilterButton.closest('.filter-dropdown'));
            });
        });

        // Привязка контекста для applyFilterButton
        this.table.querySelectorAll(`button#applyFilter_${this.table.id}`).forEach(applyFilterButton => {
            applyFilterButton.addEventListener('click', () => {
                this.filterTableWithParameters(applyFilterButton.closest('.filter-dropdown'));
                this.closeFilterDropdown();
            });
        });

        // Привязка контекста для sortButton
        this.table.querySelectorAll('.sort-desc, .sort-asc').forEach(sortButton => {
            sortButton.addEventListener('click', (event) => {
                this.setOnlyOneButtonActive(event.currentTarget, this.table.querySelectorAll('.sort-asc, .sort-desc'));
            });
        });
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

function setDisplayNone(element, isNone) {
    for (let child of element.children) {
        setDisplayNone(child, isNone);
    }
    element.style.setProperty('display', isNone ? 'none' : '', 'important');
}

function getSelectedCheckboxValues(checkBoxContainer) {
    var selectedValues = [];
    var checkboxes = checkBoxContainer.querySelectorAll('input[type="checkbox"]:checked');
    checkboxes.forEach(function (checkbox) {
        selectedValues.push(checkbox.value);
    });
    return selectedValues;
}