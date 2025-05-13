$(document).ready(function () {
    $("#loginBtn").click(function (e) {
        e.preventDefault();

        var username = $("#username").val().trim();
        var password = $("#password").val().trim();

        $("#errorMessage").text(""); 

        var isValid = true;
        if (!username) {
            $("#errorMessage").text("Student number is required.");
            isValid = false;
        }
        if (!password) {
            $("#errorMessage").text("Password is required.");
            isValid = false;
        }

        if (isValid) {
            $.post("../Home/Login_Page", {
                user: username,
                password: password
            }, function (res) {
                if (res.success) {
                    
                    window.location.href = res.redirectUrl;
                } else {
                    
                    $("#errorMessage").text(res.error || "Invalid username or password.");
                }
            }).fail(function () {
                $("#errorMessage").text("An error occurred. Please try again.");
            });
        }
    });
});