$(document).ready(function () {
    $('#loginBtn').click(function () {
        var username = $('#username').val();
        var password = $('#password').val();

        if (!username || !password) {
            $('#errorMessage').text('Please enter both username and password.');
            return;
        }

        $('#errorMessage').text('');

        $.post('/Home/Login_Page', {
            user: username,
            password: password
        }, function (response) {
            if (response.success) {
                window.location.href = response.redirectUrl;
            } else {
                $('#errorMessage').text(response.error || 'Login failed. Please try again.');
            }
        }).fail(function (xhr, status, error) {
            $('#errorMessage').text('An error occurred. Please try again later.');
            console.error("Login error:", error);
        });
    });

    $('#password').keypress(function (e) {
        if (e.which == 13) {
            $('#loginBtn').click();
        }
    });
});