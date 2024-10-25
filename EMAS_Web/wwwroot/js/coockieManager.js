function setCookie(name, value, days) {
    let expires = "";
    if (days) {
        let date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));

        if (value instanceof Map) {
            value = JSON.stringify(Array.from(value.entries()));
            value = "Map:" + value;
        } else if (typeof value === 'object') {
            value = JSON.stringify(value);
        }

        expires = "; expires=" + date.toUTCString();
    }
    document.cookie = name + "=" + encodeURIComponent(value || "") + expires + "; path=/";
}

function getCookie(name, returnJson = false) {
    const MAP_PREFIX = "Map:";
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);

    if (parts.length === 2) {
        let jsonValue = decodeURIComponent(parts.pop().split(';').shift());

        if (returnJson) {
            return jsonValue;
        }

        try {
            if (jsonValue.startsWith(MAP_PREFIX)) {
                jsonValue = jsonValue.substring(MAP_PREFIX.length);
                const parsedValue = JSON.parse(jsonValue);
                return new Map(parsedValue);
            }

            return JSON.parse(jsonValue);

        } catch (error) {
            console.error('Ошибка при парсинге cookie:', error);
        }
    }
    return null;
}

function deleteCookie(name) {
    document.cookie = name + "=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/";
}
