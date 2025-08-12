$(document).ready(function () {
    // Notification system
    let activeNotification = null; // Track the currently active notification

    function showNotification(message, type = 'info', duration = 5000) {
        if (activeNotification && activeNotification.message === message && activeNotification.type === type) {
            return;
        }

        if (activeNotification) {
            closeNotification(activeNotification.id);
        }

        const notificationArea = $('#notificationArea');
        const notificationId = 'notification_' + Date.now();

        const icons = {
            success: 'fa-check-circle',
            error: 'fa-exclamation-triangle',
            warning: 'fa-exclamation-circle',
            info: 'fa-info-circle'
        };

        const notification = $(`
            <div id="${notificationId}" class="notification notification-${type}">
                <i class="fas ${icons[type]}"></i>
                <div>
                    <strong>${message}</strong>
                </div>
                <button onclick="closeNotification('${notificationId}')" style="margin-left: auto; background: none; border: none; font-size: 18px; cursor: pointer; opacity: 0.7;">
                    <i class="fas fa-times"></i>
                </button>
            </div>
        `);

        notificationArea.append(notification);
        notification.addClass('show');

        activeNotification = { id: notificationId, message: message, type: type };

        if (duration > 0) {
            setTimeout(() => {
                closeNotification(notificationId);
            }, duration);
        }
    }

    window.closeNotification = function (notificationId) {
        const notification = $('#' + notificationId);
        notification.removeClass('show');
        setTimeout(() => {
            notification.remove();
            if (activeNotification && activeNotification.id === notificationId) {
                activeNotification = null;
            }
        }, 300);
    };

    // Set loading state for button
    function setLoadingState(isLoading, buttonId = 'btnLoadSubjects', textId = 'btnText', spinnerId = 'loadingSpinner') {
        const btn = $(`#${buttonId}`);
        const btnText = $(`#${textId}`);
        const spinner = $(`#${spinnerId}`);

        if (isLoading) {
            btn.prop('disabled', true);
            btnText.text(buttonId === 'btnLoadSubjects' ? 'Loading...' : 'Generating...');
            spinner.show();
        } else {
            btn.prop('disabled', false);
            btnText.text(buttonId === 'btnLoadSubjects' ? 'Load Subjects' : 'View Certificate of Registration');
            spinner.hide();
        }
    }

    // Load subject load
    function loadSubjectLoad(schoolYear, semester) {
        setLoadingState(true);
        $("#subjectLoad").html('<tr><td colspan="7" style="text-align: center; padding: 20px;"><i class="fas fa-spinner fa-spin"></i> Loading subjects...</td></tr>');

        $.post("/StudentEnrollment/Get_SubjectLoad", {
            studentNumber: loggedInStudentNumber,
            schoolYear: schoolYear,
            semester: semester
        }, function (res) {
            setLoadingState(false);

            if (res.error) {
                console.log("Error loading subjects:", res.error);
                showNotification(res.error + " Please try a different school year or semester, or contact support if the issue persists.", 'error', 7000);
                $("#subjectLoad").html('<tr><td colspan="7" style="text-align: center; padding: 20px; color: #e53e3e;">Failed to load subjects: ' + res.error + '</td></tr>');
                return;
            }

            if ($.fn.DataTable.isDataTable('#subjectLoadTable')) {
                $('#subjectLoadTable').DataTable().destroy();
            }

            if (res.length === 0) {
                console.log("No subjects found for school year:", schoolYear, "semester:", semester);
                $("#subjectLoad").html('<tr><td colspan="7" style="text-align: center; padding: 20px; color: #718096;">No subjects found for the selected period. Please select a different school year or semester.</td></tr>');
                showNotification('No subjects found for the selected school year and semester. Try adjusting your selection.', 'info', 7000);
                return;
            }

            var tr = "";
            for (var rec of res) {
                var timeDisplay = rec.time ? rec.time.replace(/, /g, "<br>") : "";
                var dayDisplay = rec.day ? rec.day.replace(/, /g, "<br>") : "";
                var roomDisplay = rec.room ? rec.room.replace(/, /g, "<br>") : "";
                tr += `<tr>
                    <td>${rec.msiCode || 'N/A'}</td>
                    <td><strong>${rec.courseCode}</strong></td>
                    <td>${rec.courseName}</td>
                    <td style="text-align: center;">${rec.units}</td>
                    <td><small>${timeDisplay}</small></td>
                    <td><small>${dayDisplay}</small></td>
                    <td><small>${roomDisplay}</small></td>
                </tr>`;
            }
            $("#subjectLoad").html(tr);

            $("#subjectLoadTable").DataTable({
                pageLength: 10,
                responsive: true,
                autoWidth: false,
                language: {
                    search: "Search subjects:",
                    lengthMenu: "Show _MENU_ subjects per page",
                    info: "Showing _START_ to _END_ of _TOTAL_ subjects",
                    emptyTable: "No subjects available"
                },
                columnDefs: [
                    { width: "10%", targets: 0 }, // MSI Code
                    { width: "15%", targets: 1 }, // Subject Code
                    { width: "25%", targets: 2 }, // Subject
                    { width: "10%", targets: 3 }, // Units
                    { width: "20%", targets: 4 }, // Time
                    { width: "10%", targets: 5 }, // Day
                    { width: "10%", targets: 6 }  // Room
                ],
                initComplete: function () {
                    $('#subjectLoadTable').removeClass('dataTable');
                    $('#subjectLoadTable').addClass('section-table');

                    $('#subjectLoadTable_wrapper').find('.dataTables_length, .dataTables_filter, .dataTables_info, .dataTables_paginate').css({
                        'margin': '10px 0',
                        'font-size': '14px'
                    });

                    $('#subjectLoadTable_filter input').addClass('form-input').css({
                        'margin-left': '10px',
                        'padding': '8px 12px',
                        'border': '1px solid #e2e8f0',
                        'border-radius': '6px'
                    });

                    $('#subjectLoadTable_length select').addClass('form-select').css({
                        'margin': '0 10px',
                        'padding': '4px 8px',
                        'border': '1px solid #e2e8f0',
                        'border-radius': '4px'
                    });

                    console.log("DataTable initialized successfully with", res.length, "subjects");
                }
            });

            showNotification(`Loaded ${res.length} subjects successfully`, 'success', 3000);

            // Store the subject load data for certificate generation
            $("#btnViewCertificate").data("subjects", res);
            $("#btnViewCertificate").data("schoolYear", schoolYear);
            $("#btnViewCertificate").data("semester", semester);
        }).fail(function (xhr, status, error) {
            setLoadingState(false);
            console.log("AJAX request failed:", status, error);
            showNotification("Failed to load subjects: " + error + ". Please check your connection and try again.", 'error', 7000);
            $("#subjectLoad").html('<tr><td colspan="7" style="text-align: center; padding: 20px; color: #e53e3e;">Failed to load subjects: Network error</td></tr>');
        });
    }

    // Generate Certificate of Registration
    $("#btnViewCertificate").click(function () {
        const subjects = $(this).data("subjects");
        const schoolYear = $(this).data("schoolYear");
        const semester = $(this).data("semester");

        if (!subjects || subjects.length === 0) {
            showNotification("Please load subjects first before viewing the certificate.", 'warning', 5000);
            return;
        }

        // Populate certificate details
        $("#certSchoolYear").text(schoolYear);
        $("#certSemester").text(semester);

        // Format the current date
        const currentDate = new Date();
        const formattedDate = currentDate.toLocaleString('en-US', {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit',
            hour12: true
        });
        $("#certGeneratedDate").text(formattedDate);

        // Populate subjects in the certificate table
        let totalUnits = 0;
        let tr = "";
        for (const rec of subjects) {
            const timeDisplay = rec.time ? rec.time.replace(/, /g, "<br>") : "";
            const dayDisplay = rec.day ? rec.day.replace(/, /g, "<br>") : "";
            const roomDisplay = rec.room ? rec.room.replace(/, /g, "<br>") : "";
            const units = parseFloat(rec.units) || 0;
            totalUnits += units;

            tr += `<tr>
                <td>${rec.msiCode || 'N/A'}</td>
                <td>${rec.courseCode || 'N/A'}</td>
                <td>${rec.courseName}</td>
                <td style="text-align: center;">${rec.units}</td>
                <td><small>${timeDisplay}</small></td>
                <td><small>${dayDisplay}</small></td>
                <td><small>${roomDisplay}</small></td>
            </tr>`;
        }
        $("#certificateSubjects").html(tr);
        $("#certTotalUnits").text(totalUnits.toFixed(1));

        // Show the certificate container
        $("#certificateContainer").removeClass('hidden');
        showNotification("Certificate of Registration generated successfully.", 'success', 3000);

        // Scroll to the certificate
        $('html, body').animate({
            scrollTop: $("#certificateContainer").offset().top
        }, 500);
    });

    // Button click to load subjects
    $("#btnLoadSubjects").click(function () {
        var schoolYear = $("#schoolYear").val();
        var semester = $("#semester").val();
        if (!schoolYear || !semester) {
            showNotification("Please fill in both school year and semester.", 'warning');
            return;
        }
        const yearPattern = /^\d{4}-\睹4$/;
        if (!yearPattern.test(schoolYear)) {
            showNotification("School year format should be YYYY-YYYY (e.g., 2024-2025)", 'warning');
            return;
        }
        loadSubjectLoad(schoolYear, semester);
    });

    // Debounced school year validation
    let debounceTimeout;
    $("#schoolYear").on('input', function () {
        clearTimeout(debounceTimeout);
        const value = $(this).val();
        const yearPattern = /^\d{4}-\d{4}$/;

        debounceTimeout = setTimeout(() => {
            if (value && !yearPattern.test(value)) {
                $(this).addClass('error');
                showNotification('School year format should be YYYY-YYYY (e.g., 2024-2025)', 'warning', 3000);
            } else {
                $(this).removeClass('error');
                if (activeNotification && activeNotification.message === 'School year format should be YYYY-YYYY (e.g., 2024-2025)') {
                    closeNotification(activeNotification.id);
                }
            }
        }, 500);
    });

    // Auto-load subjects when school year or semester changes
    $("#schoolYear, #semester").change(function () {
        const schoolYear = $("#schoolYear").val();
        const semester = $("#semester").val();
        if (schoolYear && semester) {
            const yearPattern = /^\d{4}-\d{4}$/;
            if (!yearPattern.test(schoolYear)) {
                showNotification("School year format should be YYYY-YYYY (e.g., 2024-2025)", 'warning');
                return;
            }
            loadSubjectLoad(schoolYear, semester);
        }
    });

    // Initial load with default school year
    if (loggedInStudentNumber) {
        const currentYear = new Date().getFullYear();
        const currentMonth = new Date().getMonth(); // 0-11 (May is 4)
        // Assuming academic year starts in June (month 5), set default to current or previous year
        const defaultSchoolYear = currentMonth >= 5 ? `${currentYear}-${currentYear + 1}` : `${currentYear - 1}-${currentYear}`;
        $("#schoolYear").val(defaultSchoolYear);
        $("#semester").val("First");
        console.log("Setting default school year to:", defaultSchoolYear);
        loadSubjectLoad(defaultSchoolYear, "First");
    }
});