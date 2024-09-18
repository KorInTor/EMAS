function submitDeliveryRequest(idValues, locationId) {
    if (idValues.length === 0) {
        alert('Пожалуйста, выберите хотя бы один элемент.');
        return;
    }

    // Создание строки параметров запроса
    var params = idValues.map(id => 'selectedIds=' + encodeURIComponent(id)).join('&');

    params += '&departureId=' + encodeURIComponent(locationId);

    // Отправка GET-запроса
    window.location.href = '/Delivery/Create?' + params;
}

function submitReservationRequest(idValues, locationId) {
    if (idValues.length === 0) {
        alert('Пожалуйста, выберите хотя бы один элемент.');
        return;
    }

    // Создание строки параметров запроса
    var params = idValues.map(id => 'selectedIds=' + encodeURIComponent(id)).join('&');

    params += '&locationid=' + encodeURIComponent(locationId);

    // Отправка GET-запроса
    window.location.href = '/Reservation/Create?' + params;
}