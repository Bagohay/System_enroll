

$(document).ready(function () {
    let currentStuEnrId = null;

    // Notification system (consistent with your existing implementation)
    let activeNotification = null;

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

    function setLoadingState(isLoading, btnId, btnTextId, spinnerId, text) {
        const btn = $(btnId);
        const btnText = $(btnTextId);
        const spinner = $(spinnerId);

        if (isLoading) {
            btn.prop('disabled', true);
            btnText.text('Processing...');
            spinner.show();
        } else {
            btn.prop('disabled', false);
            btnText.text(text);
            spinner.hide();
        }
    }

    // Load students with approved enrollments
    function loadStudents() {
        $.post("/Grading/Get_StudentsForGrading", {}, function (res) {
            if (res.error) {
                showNotification(res.error, 'error');
                return;
            }

            if ($.fn.DataTable.isDataTable('#studentsTable')) {
                $('#studentsTable').DataTable().destroy();
            }

            if (res.length === 0) {
                $("#studentsList").html('<tr><td colspan="6" style="text-align: center; padding: 20px; color: #718096;">No students with approved enrollments found.</td></tr>');
                return;
            }

            var tr = "";
            for (var rec of res) {
                tr += `<tr>
                    <td>${rec.studentNumber}</td>
                    <td>${rec.studentName}</td>
                    <td>${rec.programName}</td>
                    <td>${rec.schoolYear}</td>
                    <td>${rec.semester}</td>
                    <td>
                        <button class="section-btn grade-btn" data-stu-enr-id="${rec.stuEnrId}" data-student-name="${rec.studentName}">
                            <i class="fas fa-edit"></i> Grade
                        </button>
                    </td>
                </tr>`;
            }
            $("#studentsList").html(tr);

            $("#studentsTable").DataTable({
                pageLength: 10,
                responsive: true,
                language: {
                    search: "Search students:",
                    info: "Showing _START_ to _END_ of _TOTAL_ students"
                }
            });
        }).fail(function (xhr, status, error) {
            showNotification("Failed to load students: " + error, 'error');
        });
    }

    // Load subjects for grading
    function loadSubjectsForGrading(stuEnrId, studentName) {
        currentStuEnrId = stuEnrId;
        $("#studentName").text(studentName);
        $("#gradingSection").show();

        $.post("/Grading/Get_SubjectsForGrading", { stuEnrId: stuEnrId }, function (res) {
            if (res.error) {
                showNotification(res.error, 'error');
                $("#subjectsList").html('<tr><td colspan="5" style="text-align: center; padding: 20px; color: #e53e3e;">Failed to load subjects</td></tr>');
                return;
            }

            if ($.fn.DataTable.isDataTable('#subjectsTable')) {
                $('#subjectsTable').DataTable().destroy();
            }

            if (res.length === 0) {
                $("#subjectsList").html('<tr><td colspan="5" style="text-align: center; padding: 20px; color: #718096;">No subjects found for grading.</td></tr>');
                return;
            }

            var tr = "";
            for (var rec of res) {
                tr += `<tr>
                    <td>${rec.courseCode}</td>
                    <td>${rec.courseName}</td>
                    <td>${rec.units}</td>
                    <td>
                        <input type="number" class="grade-input" data-sce-id="${rec.sceId}" value="${rec.grade || ''}" min="0" max="4" step="0.1" style="width: 80px; padding: 5px;" />
                    </td>
                    <td>
                        <button class="section-btn save-grade-btn" data-sce-id="${rec.sceId}">
                            <i class="fas fa-save"></i> Save
                        </button>
                    </td>
                </tr>`;
            }
            $("#subjectsList").html(tr);

            $("#subjectsTable").DataTable({
                pageLength: 10,
                responsive: true,
                language: {
                    search: "Search subjects:",
                    info: "Showing _START_ to _END_ of _TOTAL_ subjects"
                }
            });
        }).fail(function (xhr, status, error) {
            showNotification("Failed to load subjects: " + error, 'error');
        });
    }

    // Save grade
    $(document).on('click', '.save-grade-btn', function () {
        const sceId = $(this).data('sce-id');
        const gradeInput = $(`input[data-sce-id="${sceId}"]`);
        const grade = gradeInput.val();

        if (!grade) {
            showNotification("Please enter a grade.", 'warning');
            return;
        }

        $.post("/Grading/Update_Grade", { sceId: sceId, grade: grade }, function (res) {
            if (res.success) {
                showNotification(res.message, 'success');
            } else {
                showNotification(res.message, 'error');
            }
        }).fail(function (xhr, status, error) {
            showNotification("Failed to save grade: " + error, 'error');
        });
    });

    // Finalize grades
    $("#btnFinalizeGrades").click(function () {
        if (!currentStuEnrId) {
            showNotification("No enrollment selected.", 'warning');
            return;
        }

        setLoadingState(true, '#btnFinalizeGrades', '#btnTextFinalize', '#loadingSpinnerFinalize', 'Finalize Grades');

        $.post("/Grading/Finalize_Grades", { stuEnrId: currentStuEnrId }, function (res) {
            setLoadingState(false, '#btnFinalizeGrades', '#btnTextFinalize', '#loadingSpinnerFinalize', 'Finalize Grades');
            if (res.success) {
                showNotification(res.message, 'success');
                $("#gradingSection").hide();
                currentStuEnrId = null;
                loadStudents();
            } else {
                showNotification(res.message, 'error');
            }
        }).fail(function (xhr, status, error) {
            setLoadingState(false, '#btnFinalizeGrades', '#btnTextFinalize', '#loadingSpinnerFinalize', 'Finalize Grades');
            showNotification("Failed to finalize grades: " + error, 'error');
        });
    });

    // Handle grade button click
    $(document).on('click', '.grade-btn', function () {
        const stuEnrId = $(this).data('stu-enr-id');
        const studentName = $(this).data('student-name');
        loadSubjectsForGrading(stuEnrId, studentName);
    });

    // Initialize
    loadStudents();
});