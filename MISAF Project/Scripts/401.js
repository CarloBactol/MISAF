$(document).ajaxError(function (event, xhr, settings, error) {
    if (xhr.status === 401) {
        var response = xhr.responseJSON;
        if (response && response.Data) {
            toastr.error(response.Data.error); // Show "Session expired"
            setTimeout(function () {
                window.location.href = response.Data.redirect; // Redirect to /Login
            }, 2000);
        } else {
            toastr.error('Session expired. Please log in again.');
            setTimeout(function () {
                window.location.href = '/Login'; // Fallback redirect
            }, 2000);
        }
    } else {
        toastr.error('An error occurred. Please try again.');
    }
});