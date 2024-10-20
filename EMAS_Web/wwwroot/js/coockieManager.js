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
            // ���� ��� ������, ����������� ��� ������� � Map
            if (Array.isArray(parsedValue)) {
                return new Map(parsedValue); // ����������� ������ ������� � Map
            }
            return parsedValue; // ���������� ������� ������
        } catch (error) {
            console.error('������ ��� �������� cookie:', error);
        }
    }
    return null; // ���������� null, ���� cookie �� �������
}


function deleteCookie(name) {
    document.cookie = name + "=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/";
}
