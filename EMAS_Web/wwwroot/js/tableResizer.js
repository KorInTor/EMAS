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

            // Устанавливаем минимальную ширину
            const newWidthLeft = startWidthLeft + deltaX;
            const newWidthRight = startWidthRight - deltaX;

            if (newWidthLeft > 50 && newWidthRight > 50) { // Минимальная ширина столбца 50px
                leftCol.style.width = newWidthLeft + 'px';
                rightCol.style.width = newWidthRight + 'px';
            }
        }

        function onMouseUp() {
            // Убираем обработчики событий после отпускания мыши
            document.removeEventListener('mousemove', onMouseMove);
        }
    });
}
