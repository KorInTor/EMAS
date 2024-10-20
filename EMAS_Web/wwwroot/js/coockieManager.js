function setCookie(name, value, days) {
    let expires = "";
    if (days) {
        let date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));

        if (value instanceof Map) {
            value = JSON.stringify(Array.from(value.entries()));
        } else if (typeof value === 'object') {
            value = JSON.stringify(value);
        }

        expires = "; expires=" + date.toUTCString();
    }
    document.cookie = name + "=" + (value || "") + expires + "; path=/";
}

function getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) {
        const jsonValue = decodeURIComponent(parts.pop().split(';').shift());

        try {
            const parsedValue = JSON.parse(jsonValue);
            // Если это массив, преобразуем его обратно в Map
            if (Array.isArray(parsedValue)) {
                return new Map(parsedValue); // Преобразуем массив обратно в Map
            }
            return parsedValue; // Возвращаем обычный объект
        } catch (error) {
            console.error('Ошибка при парсинге cookie:', error);
        }
    }
    return null; // Возвращаем null, если cookie не найдено
}


function deleteCookie(name) {
    document.cookie = name + "=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/";
}
