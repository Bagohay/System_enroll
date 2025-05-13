$(document).ready(function () {
    // Submit registration form
    $('#submitRegistration').click(function () {
        // Get the form values
        var firstName = $("#studentFname").val();
        var middleName = $("#studentMname").val();
        var lastName = $("#studentLname").val();
        var email = $("#emailAddress").val();
        var phoneNum = $("#phoneNumber").val();
        var homeAddress = $("#homeAddress").val();
        var cityAddress = $("#cityAddress").val();
        var congressDistrict = $("#congressDistrict").val();
        var programId = $("#programId").val();
        var isFirstGenStudent = $("#firstGenStudent").is(":checked");

        // Validate form fields
        let isValid = true;

        if (!firstName) {
            $("#studentFname").addClass("error");
            isValid = false;
        } else {
            $("#studentFname").removeClass("error");
        }

        if (!lastName) {
            $("#studentLname").addClass("error");
            isValid = false;
        } else {
            $("#studentLname").removeClass("error");
        }

        if (!email) {
            $("#emailAddress").addClass("error");
            isValid = false;
        } else {
            $("#emailAddress").removeClass("error");
        }

        if (!phoneNum) {
            $("#phoneNumber").addClass("error");
            isValid = false;
        } else {
            $("#phoneNumber").removeClass("error");
        }

        if (!homeAddress) {
            $("#homeAddress").addClass("error");
            isValid = false;
        } else {
            $("#homeAddress").removeClass("error");
        }

        if (!cityAddress) {
            $("#cityAddress").addClass("error");
            isValid = false;
        } else {
            $("#cityAddress").removeClass("error");
        }

        if (!programId) {
            $("#programId").addClass("error");
            isValid = false;
        } else {
            $("#programId").removeClass("error");
        }

        if (isValid) {
            // Send AJAX request to server
            $.ajax({
                url: "/Home/Registration_Page",
                type: "POST",
                data: {
                    firstname: firstName,
                    middlename: middleName,
                    lastname: lastName,
                    email: email,
                    phoneNum: phoneNum,
                    homeAddress: homeAddress,
                    cityAddress: cityAddress,
                    congressDistrict: congressDistrict,
                    programId: programId,
                    isFirstGenStudent: isFirstGenStudent
                },
                dataType: "json",
                success: function (res) {
                    console.log("Server response:", res);

                    if (res && res[0] && res[0].mess === 1) {
                        // Show success message with student number and password
                        var confirmationHtml = `
                            <div class="registration-confirmation">
                                <h2>Registration Successful!</h2>
                                <p>Your student account has been created. Please save your login credentials:</p>
                                
                                <div class="credentials-box">
                                    <div class="credential-row">
                                        <div class="credential-label">Student Number:</div>
                                        <div class="credential-value">${res[0].studentNumber}</div>
                                    </div>
                                    <div class="credential-row">
                                        <div class="credential-label">Password:</div>
                                        <div class="credential-value">${res[0].password}</div>
                                    </div>
                                </div>
                                
                                <p class="important-note">Please save these credentials to log into your student account.</p>
                                
                                <div class="action-buttons">
                                    <button id="continueToEnrollment" class="btn-primary">Continue to Enrollment</button>
                                    <button id="printCredentials" class="btn-secondary">Print Credentials</button>
                                </div>
                            </div>
                        `;

                        // Replace form with confirmation
                        $(".registration-form").html(confirmationHtml);

                        // Add event handler for continue button
                        $("#continueToEnrollment").click(function () {
                            window.location.href = "/Home/Enrollment_Form";
                        });

                        // Add print handler
                        $("#printCredentials").click(function () {
                            var printWindow = window.open('', '_blank');
                            printWindow.document.write(`
                                <html>
                                    <head>
                                        <title>Student Registration Credentials</title>
                                        <style>
                                            body { font-family: Arial, sans-serif; padding: 20px; }
                                            .header { text-align: center; margin-bottom: 30px; }
                                            .credentials { border: 1px solid #ccc; padding: 20px; margin: 20px auto; max-width: 500px; }
                                            .credential-row { margin-bottom: 15px; }
                                            .credential-label { font-weight: bold; }
                                            .note { text-align: center; margin-top: 30px; font-style: italic; }
                                        </style>
                                    </head>
                                    <body>
                                        <div class="header">
                                            <h1>Student Registration Credentials</h1>
                                            <p>${new Date().toLocaleDateString()}</p>
                                        </div>
                                        <div class="credentials">
                                            <div class="credential-row">
                                                <div class="credential-label">Student Number:</div>
                                                <div>${res[0].studentNumber}</div>
                                            </div>
                                            <div class="credential-row">
                                                <div class="credential-label">Password:</div>
                                                <div>${res[0].password}</div>
                                            </div>
                                        </div>
                                        <div class="note">
                                            <p>Please keep this information secure.</p>
                                        </div>
                                    </body>
                                </html>
                            `);
                            printWindow.document.close();
                            printWindow.print();
                        });
                    } else {
                        // Registration failed
                        var errorMsg = (res && res[0] && res[0].error) ?
                            res[0].error : "Registration failed. Please try again.";
                        alert(errorMsg);
                    }
                },
                error: function (xhr, status, error) {
                    console.error("AJAX Error:", xhr, status, error);
                    alert("An error occurred. Please try again later.");
                }
            });
        } else {
            alert("Please fill in all required fields correctly.");
        }
    });
});