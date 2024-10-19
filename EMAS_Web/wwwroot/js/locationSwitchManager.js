function setLocationState(locationIdValue) {
    setCookie("activeLocationId", locationIdValue, 3)
}

function getLocationState() {
    const locationId = getCookie("activeLocationId");
    if (locationId) {
        return parseInt(locationId);
    }
    return undefined;
}

function updateId(newId) {
    document.querySelectorAll('.locationLink').forEach(link => {
        link.href = link.href.replace(/locationId=\d+/, 'locationId=' + newId);
    });
    setLocationState(newId);
}

function switchLocationIfPossible(locationId) {
    const urlParams = new URLSearchParams(window.location.search);
    locationId = parseInt(locationId);
    if (isNaN(locationId)) {
        throw new Error('locatinoId is NaN');
    }
    if (urlParams.has('locationId')) {
        urlParams.set('locationId', locationId);
        window.location.search = urlParams.toString();
    }
}

function initLocationSwitch() {
    document.querySelectorAll('.locationLink').forEach(link => {
        link.href = link.href.replace(/locationId=\d+/, 'locationId=' + getLocationState());
    });
    document.querySelectorAll('input[type="radio"][name="locationPicker"]').forEach(locationSelector => {
        console.log(parseInt(locationSelector.value));
        console.log(getLocationState());
        if (parseInt(locationSelector.value) === getLocationState()) {
            locationSelector.checked = true;
        }
    });

}