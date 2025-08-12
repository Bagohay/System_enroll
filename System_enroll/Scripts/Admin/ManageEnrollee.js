$(document).ready(function () {
    function loadSections(programId, selectElement) {
        // Ensure programId is a valid number
        if (!programId || programId === 'undefined' || isNaN(programId) || programId == 0) {
            console.error("Invalid programId: " + programId);
            selectElement.html("<option value=''>No Sections Available</option>");
            return;
        }

        $.post("/StudentEnrollment/Get_Sections", { programId: programId }, function (res) {
            if (res.error) {
                console.error("Error loading sections: " + res.error);
                alert("Failed to load sections: " + res.error);
                return;
            }
            var options = "<option value=''>Select Section</option>";
            for (var rec of res) {
                options += `<option value='${rec.secId}'>${rec.secName}</option>`;
            }
            selectElement.html(options);
        }).fail(function (xhr, status, error) {
            console.error("Failed to load sections: Status=" + status + ", Error=" + error);
            selectElement.html("<option value=''>Failed to load sections</option>");
        });
    }

    function loadPendingEnrollees() {
        $.post("/Admin/View_Students", {}, function (res) {
            if (res.error) {
                alert(res.error);
                return;
            }
            var tr = "";
            for (var rec of res) {
                if (rec.status.toLowerCase() !== "pending" && rec.status.toLowerCase() !== "rejected") {
                    continue;
                }
                console.log("Processing enrollee: ", rec.studId, "ProgramId: ", rec.programId, "Type:", typeof rec.programId);

                tr += "<tr class='enrollee'>";
                tr += "<td class='idno'>" + rec.studId + "</td>";
                tr += "<td class='name'>" + rec.lastname + ", " + rec.firstname + "</td>";
                tr += "<td class='email'>" + rec.email + "</td>";
                tr += "<td class='program' data-program-id='" + (rec.programId || '') + "'>" + rec.programName + "</td>";
                tr += "<td class='school-year'>" + rec.schoolYear + "</td>";
                tr += "<td class='semester'>" + rec.semester + "</td>";
                tr += "<td class='type'>" + rec.studentType + "</td>";
                tr += "<td class='application-date'>" + rec.applicationDate + "</td>";
                tr += "<td>";
                tr += "<select class='status-select' onchange=\"handleStatusChange('" + rec.studId + "', this.value, this.parentNode.parentNode)\">";
                tr += "<option value='pending'" + (rec.status.toLowerCase() === 'pending' ? " selected" : "") + ">Pending</option>";
                tr += "<option value='accepted'" + (rec.status.toLowerCase() === 'accepted' ? " selected" : "") + ">Accepted</option>";
                tr += "<option value='rejected'" + (rec.status.toLowerCase() === 'rejected' ? " selected" : "") + ">Rejected</option>";
                tr += "</select>";
                tr += "</td>";
                tr += "<td>";
                if (rec.studentType.toLowerCase() === 'regular') {
                    var currentSectionId = rec.sectionId || '';
                    tr += "<select class='section-select'>";
                    tr += "<option value=''>Loading sections...</option>";
                    tr += "</select>";
                } else {
                    tr += "N/A";
                }
                tr += "</td>";
                tr += "<td><button class='view-btn' onclick=\"viewStudentDetails('" + rec.studId + "')\">View</button></td>";
                tr += "</tr>";
            }
            $("#pendingEnrollees").html(tr);

            // Load sections for regular students after DOM is updated
            $("#pendingEnrollees .enrollee").each(function () {
                var type = $(this).find('.type').text();
                var programElement = $(this).find('.program');
                var programId = parseInt(programElement.data('program-id'));
                var currentSectionId = null;

                // Find the corresponding record to get current section
                for (var rec of res) {
                    if (rec.studId === $(this).find('.idno').text()) {
                        currentSectionId = rec.sectionId;
                        break;
                    }
                }

                console.log("Enrollee type: ", type, "ProgramId: ", programId, "Current Section:", currentSectionId);

                if (type.toLowerCase() === 'regular' && programId && !isNaN(programId) && programId > 0) {
                    var $sectionSelect = $(this).find('.section-select');

                    // Load sections and set current selection
                    $.post("/StudentEnrollment/Get_Sections", { programId: programId }, function (sectionRes) {
                        if (sectionRes.error) {
                            console.error("Error loading sections: " + sectionRes.error);
                            $sectionSelect.html("<option value=''>No Sections Available</option>");
                            return;
                        }
                        var options = "<option value=''>Select Section</option>";
                        for (var secRec of sectionRes) {
                            var selected = (currentSectionId && currentSectionId == secRec.secId) ? " selected" : "";
                            options += `<option value='${secRec.secId}'${selected}>${secRec.secName}</option>`;
                        }
                        $sectionSelect.html(options);
                    }).fail(function (xhr, status, error) {
                        console.error("Failed to load sections: Status=" + status + ", Error=" + error);
                        $sectionSelect.html("<option value=''>Failed to load sections</option>");
                    });
                } else if (type.toLowerCase() === 'regular') {
                    console.warn("Skipping section load for enrollee due to invalid programId: ", programId);
                    $(this).find('.section-select').html("<option value=''>No Sections Available</option>");
                }
            });
        }).fail(function (xhr, status, error) {
            console.error("Failed to load pending enrollees: Status=" + status + ", Error=" + error);
            alert("Failed to load pending enrollees: " + error);
        });
    }

    function loadApprovedStudents() {
        $.post("/Admin/View_Students", {}, function (res) {
            if (res.error) {
                alert(res.error);
                return;
            }

            // Destroy existing DataTable if it exists
            if ($.fn.DataTable.isDataTable('#approvedStudentsTable')) {
                $('#approvedStudentsTable').DataTable().destroy();
            }

            var tr = "";
            for (var rec of res) {
                if (rec.status.toLowerCase() !== 'accepted') {
                    continue;
                }
                tr += "<tr class='enrollee'>";
                tr += "<td class='idno'>" + rec.studId + "</td>";
                tr += "<td class='name'>" + rec.lastname + ", " + rec.firstname + "</td>";
                tr += "<td class='email'>" + rec.email + "</td>";
                tr += "<td class='program'>" + rec.programName + "</td>";
                tr += "<td class='school-year'>" + rec.schoolYear + "</td>";
                tr += "<td class='semester'>" + rec.semester + "</td>";
                tr += "<td class='type'>" + rec.studentType + "</td>";
                tr += "<td class='application-date'>" + rec.applicationDate + "</td>";
                tr += "<td class='status'>" + rec.status + "</td>";
                tr += "<td class='section'>" + (rec.sectionName || 'N/A') + "</td>";
                tr += "</tr>";
            }
            $("#approvedStudents").html(tr);

            // Initialize DataTable
            try {
                $("#approvedStudentsTable").DataTable({
                    responsive: true,
                    pageLength: 10,
                    order: [[0, 'asc']] // Sort by Student ID
                });
            } catch (e) {
                console.error("DataTable initialization failed:", e);
            }
        }).fail(function (xhr, status, error) {
            console.error("Failed to load approved students: Status=" + status + ", Error=" + error);
            alert("Failed to load approved students: " + error);
        });
    }

    window.handleStatusChange = function (studId, status, row) {
        var sectionId = $(row).find('.section-select').val();
        var studentType = $(row).find('.type').text().toLowerCase();

        // Validate section requirement for regular students being accepted
        if (status === 'accepted' && studentType === 'regular' && (!sectionId || sectionId === '')) {
            alert("Please select a section for regular students before approving.");
            $(row).find('.status-select').val('pending');
            return;
        }

        // Show confirmation dialog
        if (!confirm(`Are you sure you want to change status to "${status}" for student ${studId}?`)) {
            // Reset to previous value - we'll need to track this
            $(row).find('.status-select').val('pending'); // Default fallback
            return;
        }

        $.post("/Admin/Update_Enrollment_Status", {
            StudentId: studId,
            status: status,
            sectionId: sectionId || ''
        }, function (res) {
            if (res.length > 0 && res[0].mess === 0) {
                alert("Status for student " + studId + " updated to " + status +
                    (res[0].sectionName && res[0].sectionName !== 'N/A' ? " (Section: " + res[0].sectionName + ")" : ""));
                loadPendingEnrollees();
                loadApprovedStudents();
            } else {
                alert("Failed to update status: " + (res[0]?.error || "Unknown error"));
                $(row).find('.status-select').val('pending');
            }
        }).fail(function (xhr, status, error) {
            console.error("Failed to update status: Status=" + status + ", Error=" + error);
            alert("Failed to update status: " + error);
            $(row).find('.status-select').val('pending');
        });
    };

    window.viewStudentDetails = function (studId) {
        // TODO: Implement student details view
        alert("View details for student: " + studId);
    };

    // Initialize tables
    loadPendingEnrollees();
    loadApprovedStudents();
});