$(document).ready(function () {
    // Function to fetch and display schedules
    function viewData() {
        $.post("../Assign_Schedule/View_Schedules", {}, function (res) {
            if (res.error) {
                console.error("Error fetching schedules: ", res.error);
                alert(res.error);
                return;
            }
            var tr = "";
            for (var rec in res) {
                tr += "<tr>";
                tr += "<td>" + res[rec].programName + "</td>";
                tr += "<td>" + res[rec].courseName + "</td>";
                tr += "<td>" + res[rec].secName + "</td>";
                tr += "<td>" + res[rec].startTime + "</td>";
                tr += "<td>" + res[rec].endTime + "</td>";
                tr += "<td>" + res[rec].room + "</td>";
                tr += "<td>" + res[rec].facName + "</td>";
                tr += "<td class='action-buttons'>";
                tr += "<button class='schedule-btn schedule-edit-btn btnEditSchedule' data-pcid='" + res[rec].pcId + "' data-secid='" + res[rec].secName + "' data-starttime='" + res[rec].startTime + "' data-endtime='" + res[rec].endTime + "' data-room='" + res[rec].room + "' data-facid='" + res[rec].facId + "'>Edit</button>";
                tr += "<button class='schedule-btn schedule-delete-btn btnDeleteSchedule' data-pcid='" + res[rec].pcId + "' data-secid='" + res[rec].secName + "'>Delete</button>";
                tr += "</tr>";
            }
            $("#viewSchedule").html(tr);
            $("#vwTblSchedules").DataTable();
        }).fail(function (xhr, status, error) {
            console.error("Error fetching schedules: ", error);
            alert("Failed to load schedules: " + error);
        });
    }

    // Initial load of data
    viewData();

    // Function to populate dropdowns
    function populateDropdowns(modalPrefix = "", callback = null) {
        let promises = [];

        // Populate Programs
        let programsPromise = new Promise((resolve, reject) => {
            $.post("../Assign_Schedule/GetPrograms", {}, function (res) {
                if (res.error) {
                    console.error("Error fetching programs: ", res.error);
                    alert(res.error);
                    reject(res.error);
                    return;
                }
                var options = "<option value='' disabled selected>Select a program</option>";
                for (var rec in res) {
                    options += "<option value='" + res[rec].id + "'>" + res[rec].name + "</option>";
                }
                $("#" + modalPrefix + "programId").html(options);
                resolve();
            }).fail(function (xhr, status, error) {
                console.error("Error fetching programs: ", error);
                alert("Failed to load programs: " + error);
                reject(error);
            });
        });
        promises.push(programsPromise);

        // Populate Courses
        let coursesPromise = new Promise((resolve, reject) => {
            $.post("../Assign_Schedule/GetCourses", {}, function (res) {
                if (res.error) {
                    console.error("Error fetching courses: ", res.error);
                    alert(res.error);
                    reject(res.error);
                    return;
                }
                var options = "<option value='' disabled selected>Select a course</option>";
                for (var rec in res) {
                    options += "<option value='" + res[rec].id + "'>" + res[rec].name + "</option>";
                }
                $("#" + modalPrefix + "courseId").html(options);
                resolve();
            }).fail(function (xhr, status, error) {
                console.error("Error fetching courses: ", error);
                alert("Failed to load courses: " + error);
                reject(error);
            });
        });
        promises.push(coursesPromise);

        // Populate Sections
        let sectionsPromise = new Promise((resolve, reject) => {
            $.post("../Assign_Schedule/GetSections", {}, function (res) {
                if (res.error) {
                    console.error("Error fetching sections: ", res.error);
                    alert(res.error);
                    reject(res.error);
                    return;
                }
                var options = "<option value='' disabled selected>Select a section</option>";
                for (var rec in res) {
                    options += "<option value='" + res[rec].id + "'>" + res[rec].name + "</option>";
                }
                $("#" + modalPrefix + "secId").html(options);
                resolve();
            }).fail(function (xhr, status, error) {
                console.error("Error fetching sections: ", error);
                alert("Failed to load sections: " + error);
                reject(error);
            });
        });
        promises.push(sectionsPromise);

        // Populate Faculty
        let facultyPromise = new Promise((resolve, reject) => {
            $.post("../Assign_Schedule/GetFaculty", {}, function (res) {
                if (res.error) {
                    console.error("Error fetching faculty: ", res.error);
                    alert(res.error);
                    reject(res.error);
                    return;
                }
                var options = "<option value='' disabled selected>Select a faculty</option>";
                for (var rec in res) {
                    options += "<option value='" + res[rec].id + "'>" + res[rec].name + "</option>";
                }
                $("#" + modalPrefix + "facId").html(options);
                resolve();
            }).fail(function (xhr, status, error) {
                console.error("Error fetching faculty: ", error);
                alert("Failed to load faculty: " + error);
                reject(error);
            });
        });
        promises.push(facultyPromise);

        // Execute callback after all dropdowns are populated
        Promise.all(promises).then(() => {
            if (callback && typeof callback === 'function') {
                callback();
            }
        }).catch(error => {
            console.error("Error populating dropdowns:", error);
        });
    }

    // Open Add Schedule modal and populate dropdowns
    $("#addScheduleBtn").click(function () {
        console.log("Opening add modal");
        populateDropdowns();
        $("#addScheduleModal").addClass("show").show();
    });

    // Close modal
    $(".modal-close").click(function () {
        $(".modal").removeClass("show").hide();
        $("#addScheduleForm")[0].reset();
        $("#editScheduleForm")[0].reset();
    });

    // Close modal when clicking outside the modal content
    $(".modal").click(function (e) {
        if ($(e.target).hasClass('modal')) {
            $(this).removeClass("show").hide();
            $("#addScheduleForm")[0].reset();
            $("#editScheduleForm")[0].reset();
        }
    });

    // Handle Save button click (Add Schedule)
    $("#btnSaveSchedule").click(function () {
        var programId = $("#programId").val();
        var courseId = $("#courseId").val();
        var secId = $("#secId").val();
        var startTime = $("#startTime").val();
        var endTime = $("#endTime").val();
        var room = $("#room").val();
        var facId = $("#facId").val();

        // Validate inputs
        if (!programId || !courseId || !secId || !startTime || !endTime || !room || !facId) {
            alert("Please fill in all fields.");
            return;
        }

        $.post("../Assign_Schedule/AddSchedule", {
            programId: programId,
            courseId: courseId,
            secId: secId,
            startTime: startTime,
            endTime: endTime,
            room: room,
            facId: facId
        }, function (res) {
            if (res[0].mess == 0) {
                alert("Schedule successfully added");
                viewData();
                $("#addScheduleModal").removeClass("show").hide();
                $("#addScheduleForm")[0].reset();
            } else {
                alert("Error adding schedule: " + (res[0].error || "Unknown error"));
            }
        }).fail(function (xhr, status, error) {
            console.error("Error adding schedule: ", error);
            alert("Failed to add schedule: " + error);
        });
    });

    // Handle Edit button click
    $(document).on("click", ".btnEditSchedule", function () {
        var pcId = $(this).data("pcid");
        var secId = $(this).data("secid");
        var startTime = $(this).data("starttime");
        var endTime = $(this).data("endtime");
        var room = $(this).data("room");
        var facId = $(this).data("facid");

        // Populate the edit modal fields
        $("#editPcId").val(pcId);
        $("#editSecId").val(secId);
        $("#editStartTime").val(startTime);
        $("#editEndTime").val(endTime);
        $("#editRoom").val(room);

        // Populate faculty dropdown and set the selected value AFTER it's populated
        populateDropdowns("edit", function () {
            // This callback will be executed after all dropdowns are populated
            $("#editFacId").val(facId);
        });

        $("#editScheduleModal").addClass("show").show();
    });

    // Handle Update button click (Edit Schedule)
    $("#btnUpdateSchedule").click(function () {
        var pcId = $("#editPcId").val();
        var secId = $("#editSecId").val();
        var startTime = $("#editStartTime").val();
        var endTime = $("#editEndTime").val();
        var room = $("#editRoom").val();
        var facId = $("#editFacId").val();

        // Validate inputs
        if (!startTime || !endTime || !room || !facId) {
            alert("Please fill in all fields.");
            return;
        }

        $.post("../Assign_Schedule/UpdateSchedule", {
            pcId: pcId,
            secId: secId,
            startTime: startTime,
            endTime: endTime,
            room: room,
            facId: facId
        }, function (res) {
            if (res[0].mess == 0) {
                alert("Schedule successfully updated");
                viewData();
                $("#editScheduleModal").removeClass("show").hide();
                $("#editScheduleForm")[0].reset();
            } else {
                alert("Error updating schedule: " + (res[0].error || "Unknown error"));
            }
        }).fail(function (xhr, status, error) {
            console.error("Error updating schedule: ", error);
            alert("Failed to update schedule: " + error);
        });
    });

    // Handle Delete button click
    $(document).on("click", ".btnDeleteSchedule", function () {
        if (!confirm("Are you sure you want to delete this schedule?")) {
            return;
        }

        var pcId = $(this).data("pcid");
        var secId = $(this).data("secid");

        $.post("../Assign_Schedule/DeleteSchedule", {
            pcId: pcId,
            secId: secId
        }, function (res) {
            if (res[0].mess == 0) {
                alert("Schedule successfully deleted");
                viewData();
            } else {
                alert("Error deleting schedule: " + (res[0].error || "Unknown error"));
            }
        }).fail(function (xhr, status, error) {
            console.error("Error deleting schedule: ", error);
            alert("Failed to delete schedule: " + error);
        });
    });
});