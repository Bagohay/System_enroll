$(document).ready(function () {
    let enrollmentStatus = null;
    let pollingInterval = null;

    // Enhanced notification system
    function showNotification(message, type = 'info', duration = 5000) {
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
        }, 300);
    };

    function updateStatusCard(status, title, message, showProgress = false) {
        const card = $('#enrollmentStatusCard');
        const icon = $('#statusIcon');
        const titleEl = $('#statusTitle');
        const messageEl = $('#statusMessage');
        const progress = $('#enrollmentProgress');

        card.removeClass('status-pending status-approved status-denied status-new');

        switch (status) {
            case 'pending':
                card.addClass('status-pending');
                icon.removeClass().addClass('fas fa-clock status-icon');
                titleEl.text(title || 'Enrollment Pending');
                messageEl.text(message || 'Your enrollment is pending admin approval. Please wait for approval.');
                updateProgressSteps(3);
                break;
            case 'approved':
                card.addClass('status-approved');
                icon.removeClass().addClass('fas fa-check-circle status-icon');
                titleEl.text(title || 'Enrollment Approved');
                messageEl.text(message || 'Your enrollment has been approved! You can view your subjects in the modal below.');
                updateProgressSteps(4);
                break;
            case 'denied':
                card.addClass('status-denied');
                icon.removeClass().addClass('fas fa-times-circle status-icon');
                titleEl.text(title || 'Enrollment Access Restricted');
                messageEl.text(message || 'You cannot enroll at this time. Please contact the admin for assistance.');
                updateProgressSteps(0);
                break;
            case 'new':
                card.addClass('status-new');
                icon.removeClass().addClass('fas fa-user-plus status-icon');
                titleEl.text(title || 'Ready to Enroll');
                messageEl.text(message || 'Welcome! You can now submit your enrollment request.');
                updateProgressSteps(1);
                break;
            default:
                card.addClass('status-new');
                icon.removeClass().addClass('fas fa-user-graduate status-icon');
                titleEl.text(title || 'Checking Status...');
                messageEl.text(message || 'Please wait while we check your enrollment status.');
                updateProgressSteps(0);
        }

        if (showProgress) {
            progress.show();
        }
    }

    function updateProgressSteps(currentStep) {
        for (let i = 1; i <= 4; i++) {
            const step = $(`#step${i}`);
            step.removeClass('active completed');

            if (i < currentStep) {
                step.addClass('completed');
                step.html('<i class="fas fa-check"></i>');
            } else if (i === currentStep) {
                step.addClass('active');
                step.text(i);
            } else {
                step.text(i);
            }
        }
    }

    function checkEnrollmentAccess(callback) {
        const schoolYear = $("#schoolYear").val() || '';
        const semester = $("#semester").val() || '';

        if (!schoolYear || !semester) {
            updateStatusCard('denied', 'Missing Information', 'Please select a school year and semester to check enrollment access.');
            $("#enrollmentForm").addClass('form-disabled');
            showNotification('Please select a school year and semester.', 'warning', 0);
            if (callback) callback();
            return;
        }

        $.post("/StudentEnrollment/Check_Enrollment_Access", {
            studentNumber: loggedInStudentNumber,
            schoolYear: schoolYear,
            semester: semester
        }, function (res) {
            enrollmentStatus = res;

            // Check if student has enrolled subjects that are ungraded
            checkEnrolledSubjectsStatus(schoolYear, semester, function (hasUngradedSubjects) {
                if (hasUngradedSubjects) {
                    updateStatusCard('approved', 'Enrollment Approved', 'Your enrollment has been approved! You can view your subjects in the modal below.', true);
                    $("#enrollmentForm").addClass('form-disabled');
                    showNotification('You have an active enrollment with ungraded subjects.', 'info');
                    if (pollingInterval) {
                        clearInterval(pollingInterval);
                        pollingInterval = null;
                    }
                    if (callback) callback();
                    return;
                }

                // Continue with normal enrollment status check
                if (!res.hasAccess) {
                    if (res.message.includes('pending')) {
                        updateStatusCard('pending', null, res.message, true);
                        $("#enrollmentForm").addClass('form-disabled');
                        showNotification('You have a pending enrollment request. Please wait for admin approval.', 'warning', 0);

                        if (!pollingInterval) {
                            pollingInterval = setInterval(() => {
                                $.post("/StudentEnrollment/Check_Enrollment_Access", {
                                    studentNumber: loggedInStudentNumber,
                                    schoolYear: schoolYear,
                                    semester: semester
                                }, function (pollRes) {
                                    enrollmentStatus = pollRes;
                                    if (!pollRes.message.includes('pending')) {
                                        clearInterval(pollingInterval);
                                        pollingInterval = null;

                                        if (pollRes.hasAccess) {
                                            updateStatusCard('new', 'Ready to Enroll', 'You can now submit your enrollment request.', true);
                                            $("#enrollmentForm").removeClass('form-disabled');
                                            showNotification('You can now submit your enrollment request!', 'success');
                                        } else {
                                            updateStatusCard('denied', null, pollRes.message, true);
                                            $("#enrollmentForm").addClass('form-disabled');
                                            showNotification(pollRes.message, 'error', 0);
                                        }
                                    }
                                }).fail(function (xhr, status, error) {
                                    showNotification("Failed to check enrollment status: " + error, 'error');
                                });
                            }, 5000);
                        }
                    } else {
                        updateStatusCard('denied', null, res.message, true);
                        $("#enrollmentForm").addClass('form-disabled');
                        showNotification(res.message, 'error', 0);
                    }
                } else {
                    updateStatusCard('new', null, null, true);
                    $("#enrollmentForm").removeClass('form-disabled');
                    showNotification('You can now submit your enrollment request!', 'success');
                }

                if (callback) callback();
            });
        }).fail(function (xhr, status, error) {
            updateStatusCard('denied', 'Connection Error', 'Failed to check enrollment access. Please refresh the page.');
            $("#enrollmentForm").addClass('form-disabled');
            showNotification("Failed to check enrollment access: " + error, 'error', 0);
        });
    }

    function checkEnrolledSubjectsStatus(schoolYear, semester, callback) {
        $.post("/StudentEnrollment/Get_EnrolledSubjects", {
            studentNumber: loggedInStudentNumber,
            schoolYear: schoolYear,
            semester: semester
        }, function (res) {
            const hasUngradedSubjects = res && !res.error && res.length > 0;
            if (callback) callback(hasUngradedSubjects);
        }).fail(function () {
            if (callback) callback(false);
        });
    }

    function setLoadingState(isLoading) {
        const btn = $('#btnEnroll');
        const btnText = $('#btnText');
        const spinner = $('#loadingSpinner');

        if (isLoading) {
            btn.prop('disabled', true);
            btnText.text('Processing...');
            spinner.show();
        } else {
            btn.prop('disabled', false);
            btnText.text('Enroll Student');
            spinner.hide();
        }
    }

    function loadPrograms() {
        $.post("/StudentEnrollment/Get_Programs", {}, function (res) {
            if (res.error) {
                showNotification(res.error, 'error');
                return;
            }
            var options = "<option value=''>Select Program</option>";
            for (var rec of res) {
                options += `<option value='${rec.id}'>${rec.name}</option>`;
            }
            $("#programId").html(options);
        }).fail(function (xhr, status, error) {
            showNotification("Failed to load programs: " + error, 'error');
        });
    }

    function loadSubjectSelections(programId, semester, yearLevel) {
        $("#subjectSelection").html('<tr><td colspan="7" style="text-align: center; padding: 20px;"><i class="fas fa-spinner fa-spin"></i> Loading subjects...</td></tr>');

        $.post("/StudentEnrollment/Get_AvailableSubjects", {
            studentNumber: loggedInStudentNumber,
            programId: programId,
            semester: semester,
            yearLevel: yearLevel
        }, function (res) {
            if (res.error) {
                showNotification(res.error, 'error');
                $("#subjectSelection").html('<tr><td colspan="7" style="text-align: center; padding: 20px; color: #e53e3e;">Failed to load subjects</td></tr>');
                return;
            }

            if ($.fn.DataTable.isDataTable('#subjectSelectionTable')) {
                $('#subjectSelectionTable').DataTable().destroy();
            }

            if (res.length === 0) {
                $("#subjectSelection").html('<tr><td colspan="7" style="text-align: center; padding: 20px; color: #718096;">No available subjects found for the selected criteria.</td></tr>');
                return;
            }

            var tr = "";
            for (var rec of res) {
                tr += `<tr>
                    <td style="text-align: center;">
                        <input type='checkbox' class='subject-select' data-schedid='${rec.schedId}' />
                    </td>
                    <td><strong>${rec.courseCode}</strong></td>
                    <td>${rec.courseName}</td>
                    <td>${rec.units}</td>
                    <td><small>${rec.time}</small></td>
                    <td><small>${rec.day}</small></td>
                    <td><small>${rec.room}</small></td>
                </tr>`;
            }
            $("#subjectSelection").html(tr);

            $("#subjectSelectionTable").DataTable({
                pageLength: 10,
                responsive: true,
                autoWidth: false,
                language: {
                    search: "Search subjects:",
                    lengthMenu: "Show _MENU_ subjects per page",
                    info: "Showing _START_ to _END_ of _TOTAL_ subjects",
                    emptyTable: "No subjects available"
                },
                initComplete: function () {
                    $('#subjectSelectionTable').removeClass('dataTable');
                    $('#subjectSelectionTable').addClass('section-table');

                    $('#subjectSelectionTable_wrapper').find('.dataTables_length, .dataTables_filter, .dataTables_info, .dataTables_paginate').css({
                        'margin': '10px 0',
                        'font-size': '14px'
                    });

                    $('#subjectSelectionTable_filter input').addClass('form-input').css({
                        'margin-left': '10px',
                        'padding': '8px 12px',
                        'border': '1px solid #e2e8f0',
                        'border-radius': '6px'
                    });

                    $('#subjectSelectionTable_length select').addClass('form-select').css({
                        'margin': '0 10px',
                        'padding': '4px 8px',
                        'border': '1px solid #e2e8f0',
                        'border-radius': '4px'
                    });
                }
            });

            showNotification(`Found ${res.length} available subjects`, 'info', 3000);
        }).fail(function (xhr, status, error) {
            showNotification("Failed to load subjects: " + error, 'error');
            $("#subjectSelection").html('<tr><td colspan="7" style="text-align: center; padding: 20px; color: #e53e3e;">Failed to load subjects</td></tr>');
        });
    }

    function viewGroupedEnrolledSubjects() {
        const schoolYear = $("#schoolYear").val();
        const semester = $("#semester").val();

        if (!schoolYear || !semester) {
            showNotification("Please select a school year and semester to view enrolled subjects.", "warning");
            return;
        }

        $.post("/StudentEnrollment/ViewGroupedEnrolledSubjects", {
            schoolYear: schoolYear,
            semester: semester
        }, function (res) {
            if (res.error) {
                showNotification(res.error, 'error');
                return;
            }

            var html = "";
            for (var section in res) {
                html += "<div class='section-group'>";
                html += "<h3>" + section + "</h3>";

                // Group subjects by course
                var subjectsByCourse = {};
                for (var i = 0; i < res[section].length; i++) {
                    var subject = res[section][i];
                    var courseKey = subject.courseId;

                    if (!subjectsByCourse[courseKey]) {
                        subjectsByCourse[courseKey] = {
                            subject: subject.subject,
                            description: subject.description,
                            units: parseFloat(subject.units),
                            time: subject.time,
                            day: subject.day,
                            room: subject.room
                        };
                    }
                }

                // Build the table for this section
                html += "<table class='section-schedule-table'>";
                html += "<thead>";
                html += "<tr>";
                html += "<th>Course Code</th>";
                html += "<th class='description'>Descriptive Title</th>";
                html += "<th class='numeric'>Units</th>";
                html += "<th class='time-day'>Time and Day</th>";
                html += "<th>Room</th>";
                html += "</tr>";
                html += "</thead>";
                html += "<tbody>";

                var totalUnits = 0;
                for (var courseKey in subjectsByCourse) {
                    var course = subjectsByCourse[courseKey];
                    totalUnits += course.units;

                    // Split time, day, and room for display
                    var timeEntries = course.time ? course.time.replace(/, /g, "<br>") : "";
                    var roomEntries = course.room ? course.room.replace(/, /g, "<br>") : "";

                    html += "<tr>";
                    html += "<td>" + course.subject + "</td>";
                    html += "<td>" + course.description + "</td>";
                    html += "<td class='numeric'>" + course.units + "</td>";
                    html += "<td class='time-day'>" + timeEntries + "</td>";
                    html += "<td>" + roomEntries + "</td>";
                    html += "</tr>";
                }

                html += "</tbody>";
                html += "<tfoot>";
                html += "<tr class='total-row'>";
                html += "<th colspan='2'>TOTAL</th>";
                html += "<th class='numeric'>" + totalUnits + "</th>";
                html += "<th colspan='2'></th>";
                html += "</tr>";
                html += "</tfoot>";
                html += "</table>";
                html += "</div>";
            }

            if (!html) {
                html = "<p>No enrolled subjects found for the selected period.</p>";
            }

            $("#groupedEnrolledSubjects").html(html);
            $("#viewEnrolledSubjectsModal").addClass("show").show();
        }).fail(function (xhr, status, error) {
            showNotification("Failed to load enrolled subjects: " + error, 'error');
        });
    }

    $("#isRegular").change(function () {
        var isRegular = $(this).val() === "true";
        $("#subjectSelectionGroup").toggle(!isRegular);

        if (!isRegular && $("#programId").val() && $("#semester").val() && $("#yearLevel").val()) {
            loadSubjectSelections(
                $("#programId").val(),
                $("#semester").val(),
                $("#yearLevel").val()
            );
        }
    });

    $("#programId, #semester, #yearLevel").change(function () {
        if ($("#isRegular").val() === "false" && $("#programId").val() && $("#semester").val() && $("#yearLevel").val()) {
            loadSubjectSelections(
                $("#programId").val(),
                $("#semester").val(),
                $("#yearLevel").val()
            );
        }
    });

    $("#btnEnroll").click(function () {
        var programId = $("#programId").val();
        var schoolYear = $("#schoolYear").val();
        var semester = $("#semester").val();
        var yearLevel = $("#yearLevel").val();
        var isRegular = $("#isRegular").val() === "true";
        var subjectSelectionIds = [];

        if (!programId || !schoolYear || !semester || !yearLevel) {
            showNotification("Please fill in all required fields before submitting.", 'warning');
            return;
        }

        if (!isRegular) {
            $(".subject-select:checked").each(function () {
                subjectSelectionIds.push($(this).data("schedid"));
            });

            if (subjectSelectionIds.length === 0) {
                showNotification("Please select at least one subject for irregular student enrollment.", 'warning');
                return;
            }
        }

        setLoadingState(true);

        $.post("/StudentEnrollment/Enroll_Student", {
            studentNumber: loggedInStudentNumber,
            programId: programId,
            schoolYear: schoolYear,
            semester: semester,
            yearLevel: yearLevel,
            isRegular: isRegular.toString(),
            subjectSelectionIds: subjectSelectionIds.join(","),
            enrollmentStatusId: "1"
        }, function (res) {
            setLoadingState(false);

            if (res && res.length > 0 && res[0]?.success) {
                showNotification("Enrollment request submitted successfully! Redirecting to your Certificate of Registration...", 'success', 3000);
                updateStatusCard('pending', 'Enrollment Submitted', 'Your enrollment request has been submitted and is awaiting admin approval.', true);

                $("#enrollmentForm")[0].reset();
                $("#subjectSelectionGroup").hide();
                $("#enrollmentForm").addClass('form-disabled');

                // Redirect to CertificateOfRegistration action after a short delay
                setTimeout(function () {
                    window.location.href = "/StudentEnrollment/Display_SubjectLoad?studentNumber=" + encodeURIComponent(loggedInStudentNumber) + "&schoolYear=" + encodeURIComponent(schoolYear) + "&semester=" + encodeURIComponent(semester);
                }, 3000);
            } else {
                const errorMessage = res && res.length > 0 && res[0]?.message
                    ? res[0].message
                    : "Unknown error occurred during enrollment submission.";
                showNotification("Error submitting enrollment: " + errorMessage, 'error');

                // If the error is due to existing graded records, keep the form enabled so the student can adjust their input
                if (errorMessage.includes("You have already completed and been graded for this year level and semester")) {
                    $("#enrollmentForm").removeClass('form-disabled');
                } else {
                    $("#enrollmentForm").addClass('form-disabled');
                }
            }
        }).fail(function (xhr, status, error) {
            setLoadingState(false);
            showNotification("Failed to submit enrollment request: " + error, 'error');
            $("#enrollmentForm").addClass('form-disabled');
        });
    });

    $("#enrollmentForm").on('reset', function () {
        $("#subjectSelectionGroup").hide();
        $("#subjectSelection").html('');
        if ($.fn.DataTable.isDataTable('#subjectSelectionTable')) {
            $('#subjectSelectionTable').DataTable().destroy();
        }
    });

    $("#schoolYear").on('input', function () {
        const value = $(this).val();
        const yearPattern = /^\d{4}-\d{4}$/;

        if (value && !yearPattern.test(value)) {
            $(this).addClass('error');
            showNotification('School year format should be YYYY-YYYY (e.g., 2024-2025)', 'warning', 3000);
        } else {
            $(this).removeClass('error');
        }
    });

    $("#schoolYear, #semester").change(function () {
        const schoolYear = $("#schoolYear").val();
        const semester = $("#semester").val();

        if (schoolYear && semester && loggedInStudentNumber) {
            checkEnrollmentAccess();
        }
    });

    $("#viewEnrolledSubjectsBtn").click(function () {
        viewGroupedEnrolledSubjects();
    });

    $(".modal-close").click(function () {
        $(".modal").removeClass("show").hide();
        $("#groupedEnrolledSubjects").html("");
    });

    $(".modal").click(function (e) {
        if ($(e.target).hasClass('modal')) {
            $(this).removeClass("show").hide();
            $("#groupedEnrolledSubjects").html("");
        }
    });

    function initializeHelpSystem() {
        $("#programId").attr('title', 'Select your academic program');
        $("#schoolYear").attr('title', 'Enter school year in format: YYYY-YYYY');
        $("#semester").attr('title', 'Select the semester for enrollment');
        $("#yearLevel").attr('title', 'Select your current year level');
        $("#isRegular").attr('title', 'Regular students follow standard curriculum, irregular students can select specific subjects');
    }

    $(document).ajaxError(function (event, xhr, settings, thrownError) {
        if (xhr.status === 0) {
            showNotification('Network connection lost. Please check your internet connection.', 'error', 0);
        } else if (xhr.status === 500) {
            showNotification('Server error occurred. Please try again later.', 'error');
        } else if (xhr.status === 404) {
            showNotification('The requested service was not found. Please contact support.', 'error');
        }
    });

    function initializeApp() {
        $("#subjectSelectionGroup").hide();
        initializeHelpSystem();
        loadPrograms();

        if (loggedInStudentNumber) {
            const currentYear = new Date().getFullYear();
            const defaultSchoolYear = currentYear + "-" + (currentYear + 1);
            $("#schoolYear").val(defaultSchoolYear);
            $("#semester").val("First");
            checkEnrollmentAccess();
        }

        setTimeout(() => {
            if (!enrollmentStatus || enrollmentStatus.hasAccess) {
                showNotification('Welcome to the Student Enrollment System!', 'info', 4000);
            }
        }, 500);
    }

    initializeApp();
});