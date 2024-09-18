function makeResizable(table) {
    const cols = table.querySelectorAll('thead th');
    const resizers = table.querySelectorAll('thead .resizer');

    if (resizers.length === 0) {
        console.warn('No resizers found in thead');
        return;
    }

    resizers.forEach((resizer) => {
        let startX, startWidthLeft, startWidthRight;
        const leftCol = resizer.parentElement;
        const rightCol = leftCol.nextElementSibling;

        if (!leftCol || !rightCol) {
            console.warn('Resizer must be between two columns');
            return;
        }

        resizer.addEventListener('mousedown', (e) => {
            startX = e.pageX;
            startWidthLeft = leftCol.offsetWidth;
            startWidthRight = rightCol.offsetWidth;

            // Добавляем обработчики событий для движения мыши и отпускания
            document.addEventListener('mousemove', onMouseMove);
            document.addEventListener('mouseup', onMouseUp, { once: true });
        });

        function onMouseMove(e) {
            const deltaX = e.pageX - startX;
            leftCol.style.width = (startWidthLeft + deltaX) + 'px';
            rightCol.style.width = (startWidthRight - deltaX) + 'px';
        }

        function onMouseUp() {
            // Убираем обработчики событий после отпускания мыши
            document.removeEventListener('mousemove', onMouseMove);
        }
    });
}