// File: ViewStudentHistory.js
// Place this in Scripts/Grading/

$(document).ready(function () {
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

    // Load academic history
    function loadAcademicHistory() {
        $.post("/Grading/Get_AcademicHistory", { studentNumber: loggedInStudentNumber }, function (res) {
            if (res.error) {
                showNotification(res.error, 'error');
                return;
            }

            if ($.fn.DataTable.isDataTable('#historyTable')) {
                $('#historyTable').DataTable().destroy();
            }

            if (res.length === 0) {
                $("#historyList").html('<tr><td colspan="6" style="text-align: center; padding: 20px; color: #718096;">No academic history found.</td></tr>');
                return;
            }

            var tr = "";
            for (var rec of res) {
                tr += `<tr>
                    <td>${rec.courseCode}</td>
                    <td>${rec.courseName}</td>
                    <td>${rec.schoolYear}</td>
                    <td>${rec.semester}</td>
                    <td>${rec.finalGrade}</td>
                    <td>${rec.remarks}</td>
                </tr>`;
            }
            $("#historyList").html(tr);

            $("#historyTable").DataTable({
                pageLength: 10,
                responsive: true,
                language: {
                    search: "Search history:",
                    info: "Showing _START_ to _END_ of _TOTAL_ records"
                }
            });
        }).fail(function (xhr, status, error) {
            showNotification("Failed to load academic history: " + error, 'error');
        });
    }

    // Initialize
    loadAcademicHistory();
});