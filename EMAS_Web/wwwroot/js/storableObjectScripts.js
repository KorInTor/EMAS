function submitDeliveryRequest(idValues, locationId) {
    if (idValues.length === 0) {
        alert('����������, �������� ���� �� ���� �������.');
        return;
    }

    // �������� ������ ���������� �������
    var params = idValues.map(id => 'selectedIds=' + encodeURIComponent(id)).join('&');

    params += '&departureId=' + encodeURIComponent(locationId);

    // �������� GET-�������
    window.location.href = '/Delivery/Create?' + params;
}

function submitReservationRequest(idValues, locationId) {
    if (idValues.length === 0) {
        alert('����������, �������� ���� �� ���� �������.');
        return;
    }

    // �������� ������ ���������� �������
    var params = idValues.map(id => 'selectedIds=' + encodeURIComponent(id)).join('&');

    params += '&locationid=' + encodeURIComponent(locationId);

    // �������� GET-�������
    window.location.href = '/Reservation/Create?' + params;
}