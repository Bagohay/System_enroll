$(document).ready(function () {
    // Show/hide schedule dropdown based on block type
    $("#blockTypeId").change(function () {
        var blockTypeId = $(this).val();
        if (blockTypeId === '1') { // Block
            $('#scheduleGroup').show();
            $('#schedule').attr('required', 'required');
        } else { // Non-Block
            $('#scheduleGroup').hide();
            $('#schedule').removeAttr('required').val('');
        }
    });

    // Program dropdown change handler
    $("#programId").change(function () {
        console.log("Program ID changed to:", $(this).val());

        var programId = $(this).val();
        if (programId === '1' || programId === '2') {
            $('#majorIdGroup').show();
        } else {
            $('#majorIdGroup').hide();
        }
    });

    // Submit enrollment form
    $('#submitEnrollment').click(function () {
        // Get the form values
        var studentNumber = $("#studentNumber").val();
        var programId = $("#programId").val();
        var schoolYear = $("#schoolYear").val();
        var semester = $("#semester").val();
        var yearLevel = $("#yearLevel").val();
        var studentStatusId = $("#studentType").val();
        var blockTypeId = $("#blockTypeId").val();
        var schedule = $("#schedule").val();

        // Debug logs
        console.log("Form submission - Student Number:", studentNumber);
        console.log("Form Data:", {
            studentNumber: studentNumber,
            programId: programId,
            schoolYear: schoolYear,
            semester: semester,
            yearLevel: yearLevel,
            studentStatusId: studentStatusId,
            blockTypeId: blockTypeId,
            schedule: schedule
        });

        // Validate form fields
        let isValid = true;

        if (!programId) {
            $("#programId").addClass("error");
            isValid = false;
        } else {
            $("#programId").removeClass("error");
        }

        if (!schoolYear) {
            $("#schoolYear").addClass("error");
            isValid = false;
        } else {
            $("#schoolYear").removeClass("error");
        }

        if (!semester) {
            $("#semester").addClass("error");
            isValid = false;
        } else {
            $("#semester").removeClass("error");
        }

        if (!yearLevel) {
            $("#yearLevel").addClass("error");
            isValid = false;
        } else {
            $("#yearLevel").removeClass("error");
        }

        if (!studentStatusId) {
            $("#studentType").addClass("error");
            isValid = false;
        } else {
            $("#studentType").removeClass("error");
        }

        if (!blockTypeId) {
            $("#blockTypeId").addClass("error");
            isValid = false;
        } else {
            $("#blockTypeId").removeClass("error");
        }

        // Validate schedule for Block
        if (blockTypeId === '1' && !schedule) {
            $("#schedule").addClass("error");
            isValid = false;
        } else {
            $("#schedule").removeClass("error");
        }

        if (isValid) {
            // Send AJAX request to server
            $.ajax({
                url: "../Home/Submit_Enrollment",
                type: "POST",
                data: {
                    studentNumber: studentNumber,
                    programId: programId,
                    schoolYear: schoolYear,
                    semester: semester,
                    yearLevel: yearLevel,
                    studentStatusId: studentStatusId,
                    blockTypeId: blockTypeId,
                    schedule: schedule
                },
                dataType: "json",
                success: function (res) {
                    console.log("Server response:", res);

                    if (res && res[0] && res[0].mess === 1) {
                        // Create HTML for enrollment confirmation
                        var confirmationHtml = `
                            <div class="enrollment-confirmation">
                                <div class="confirmation-header">
                                    <h2>Enrollment Successful!</h2>
                                    <p>Your enrollment has been processed successfully.</p>
                                </div>
                                
                                <div class="confirmation-details">
                                    <div class="detail-row">
                                        <div class="detail-label">Student Number:</div>
                                        <div class="detail-value">${res[0].studentNumber}</div>
                                    </div>
                                    <div class="detail-row">
                                        <div class="detail-label">Program:</div>
                                        <div class="detail-value">${res[0].program}</div>
                                    </div>
                                    <div class="detail-row">
                                        <div class="detail-label">School Year:</div>
                                        <div class="detail-value">${res[0].schoolYear}</div>
                                    </div>
                                    <div class="detail-row">
                                        <div class="detail-label">Semester:</div>
                                        <div class="detail-value">${res[0].semester}</div>
                                    </div>
                                    <div class="detail-row">
                                        <div class="detail-label">Year Level:</div>
                                        <div class="detail-value">${res[0].yearLevel}</div>
                                    </div>
                                    <div class="detail-row">
                                        <div class="detail-label">Enrollment Type:</div>
                                        <div class="detail-value">${res[0].blockType}</div>
                                    </div>
                                    <div class="detail-row">
                                        <div class="detail-label">Schedule:</div>
                                        <div class="detail-value">${res[0].schedule}</div>
                                    </div>
                                    <div class="detail-row">
                                        <div class="detail-label">Total Units:</div>
                                        <div class="detail-value">${res[0].units}</div>
                                    </div>
                                </div>
                                
                                <div class="confirmation-actions">
                                    <button id="printConfirmation" class="btn-secondary">Print Confirmation</button>
                                    <a href="../Student/Dashboard" class="btn-primary">Go to Dashboard</a>
                                </div>
                                
                                <p class="confirmation-note">Please print or save this confirmation for your records.</p>
                            </div>
                        `;

                        // Replace form with confirmation
                        $(".enrollment-form").html(confirmationHtml);

                        // Add print handler
                        $("#printConfirmation").click(function () {
                            var printWindow = window.open('', '_blank');
                            printWindow.document.write(`
                                <html>
                                    <head>
                                        <title>Enrollment Confirmation</title>
                                        <style>
                                            body { font-family: Arial, sans-serif; padding: 20px; }
                                            .header { text-align: center; margin-bottom: 30px; }
                                            .details { border: 1px solid #ccc; padding: 20px; margin: 20px auto; max-width: 500px; }
                                            .detail-row { margin-bottom: 15px; display: flex; }
                                            .detail-label { width: 150px; font-weight: bold; }
                                            .footer { text-align: center; margin-top: 30px; font-style: italic; }
                                        </style>
                                    </head>
                                    <body>
                                        <div class="header">
                                            <h1>Student Enrollment Confirmation</h1>
                                            <p>${new Date().toLocaleDateString()}</p>
                                        </div>
                                        <div class="details">
                                            <div class="detail-row">
                                                <div class="detail-label">Student Number:</div>
                                                <div>${res[0].studentNumber}</div>
                                            </div>
                                            <div class="detail-row">
                                                <div class="detail-label">Program:</div>
                                                <div>${res[0].program}</div>
                                            </div>
                                            <div class="detail-row">
                                                <div class="detail-label">School Year:</div>
                                                <div>${res[0].schoolYear}</div>
                                            </div>
                                            <div class="detail-row">
                                                <div class="detail-label">Semester:</div>
                                                <div>${res[0].semester}</div>
                                            </div>
                                            <div class="detail-row">
                                                <div class="detail-label">Year Level:</div>
                                                <div>${res[0].yearLevel}</div>
                                            </div>
                                            <div class="detail-row">
                                                <div class="detail-label">Enrollment Type:</div>
                                                <div>${res[0].blockType}</div>
                                            </div>
                                            <div class="detail-row">
                                                <div class="detail-label">Schedule:</div>
                                                <div>${res[0].schedule}</div>
                                            </div>
                                            <div class="detail-row">
                                                <div class="detail-label">Total Units:</div>
                                                <div>${res[0].units}</div>
                                            </div>
                                        </div>
                                        <div class="footer">
                                            <p>This serves as your official enrollment confirmation.</p>
                                        </div>
                                    </body>
                                </html>
                            `);
                            printWindow.document.close();
                            printWindow.print();
                        });
                    } else {
                        // Enrollment failed
                        var errorMsg = (res && res[0] && res[0].error) ?
                            res[0].error : "Enrollment failed. Please try again.";
                        alert(errorMsg);

                        if (res && res[0] && res[0].error && res[0].error.includes("Student number not found")) {
                            window.location.href = "../Home/Login_Page";
                        }
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