$(document).ready(function () {
    $('#submitRegistration').click(function () {
        var firstName = $("#studentFname").val().trim();
        var middleName = $("#studentMname").val().trim();
        var lastName = $("#studentLname").val().trim();
        var email = $("#emailAddress").val().trim();
        var phoneNum = $("#phoneNumber").val().trim();
        var homeAddress = $("#homeAddress").val().trim();
        var cityAddress = $("#cityAddress").val().trim();
        var congressDistrict = $("#congressDistrict").val().trim();
        var isFirstGenStudent = $("#firstGenStudent").is(":checked");

        let isValid = true;

        // Clear previous error styling
        $(".form-control").removeClass("error");

        // Validate required fields with debugging
        console.log("First Name:", firstName);
        if (!firstName) {
            console.log("First Name validation failed");
            $("#studentFname").addClass("error");
            isValid = false;
        }

        console.log("Last Name:", lastName);
        if (!lastName) {
            console.log("Last Name validation failed");
            $("#studentLname").addClass("error");
            isValid = false;
        }

        console.log("Email:", email);
        if (!email) {
            console.log("Email empty validation failed");
            $("#emailAddress").addClass("error");
            isValid = false;
        } else if (!isValidEmail(email)) {
            console.log("Email format validation failed");
            $("#emailAddress").addClass("error");
            isValid = false;
        }

        console.log("Phone Number:", phoneNum);
        if (!phoneNum) {
            console.log("Phone Number validation failed");
            $("#phoneNumber").addClass("error");
            isValid = false;
        }

        console.log("Home Address:", homeAddress);
        if (!homeAddress) {
            console.log("Home Address validation failed");
            $("#homeAddress").addClass("error");
            isValid = false;
        }

        console.log("City Address:", cityAddress);
        if (!cityAddress) {
            console.log("City Address validation failed");
            $("#cityAddress").addClass("error");
            isValid = false;
        }

        console.log("Final isValid state:", isValid);

        if (isValid) {
            // Show loading indicator or disable button to prevent double submission
            $("#submitRegistration").prop("disabled", true).text("Processing...");

            $.post("/Home/Registration_Page", {
                firstname: firstName,
                middlename: middleName,
                lastname: lastName,
                email: email,
                phoneNum: phoneNum,
                homeAddress: homeAddress,
                cityAddress: cityAddress,
                congressDistrict: congressDistrict,
                isFirstGenStudent: isFirstGenStudent
            }, function (res) {
                console.log("Server response:", res);
                $("#submitRegistration").prop("disabled", false).text("Register Now");

                if (res && res[0] && res[0].mess === 1) {
                    var confirmationHtml = `
                        <div class="success-message">Registration Successful! Your account has been created.</div>
                        <div class="credentials-box">
                            <h3>Your Login Credentials</h3>
                            <p><strong>Student Number:</strong> ${res[0].studentNumber}</p>
                            <p><strong>Password:</strong> ${res[0].password}</p>
                            <p>Please save these credentials to log into your student account.</p>
                            <button id="printCredentials" class="btn-secondary">Print Credentials</button>
                            <a href="/Home/Login_Page" class="btn-primary" style="margin-left: 10px;">Go to Login Page</a>
                        </div>
                    `;

                    $(".registration-form").html(confirmationHtml);

                    $("#printCredentials").click(function () {
                        var printWindow = window.open('', '_blank');
                        printWindow.document.write(`
                            <h2>Student Registration Credentials</h2>
                            <p>Registration Date: ${new Date().toLocaleDateString()}</p>
                            <p><strong>Student Number:</strong> ${res[0].studentNumber}</p>
                            <p><strong>Password:</strong> ${res[0].password}</p>
                            <p>Please keep this information secure.</p>
                        `);
                        printWindow.document.close();
                        printWindow.print();
                    });
                } else {
                    var errorMsg = (res && res[0] && res[0].error) ?
                        res[0].error : "Registration failed. Please try again.";
                    alert(errorMsg);
                }
            }).fail(function (xhr, status, error) {
                console.error("POST Error:", xhr.responseText);
                console.error("Status:", status);
                console.error("Error:", error);
                $("#submitRegistration").prop("disabled", false).text("Register Now");
                alert("An error occurred. Please try again later.");
            });
        } else {
            alert("Please fill in all required fields correctly.");
        }
    });

    // Helper function to validate email format
    function isValidEmail(email) {
        var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(email);
    }
}); 