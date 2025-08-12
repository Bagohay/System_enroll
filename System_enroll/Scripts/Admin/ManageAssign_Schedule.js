$(document).ready(function () {
    let scheduleDetails = [];

    function viewData() {
        $.post("../Assign_Schedule/View_Schedules", {}, function (res) {
            if (res.error) {
                console.error("Error fetching schedules: ", res.error);
                alert(res.error);
                return;
            }

            if ($.fn.DataTable.isDataTable('#vwTblSchedules')) {
                $('#vwTblSchedules').DataTable().destroy();
            }

            var tr = "";
            for (var rec in res) {
                tr += "<tr>";
                tr += "<td>" + res[rec].misCode + "</td>";
                tr += "<td>" + res[rec].programName + " " + res[rec].secName + "</td>";
                tr += "<td>" + res[rec].courseName + "</td>";
                tr += "<td>" + res[rec].secName + "</td>";
                tr += "<td>" + res[rec].day + "</td>";
                tr += "<td>" + res[rec].startTime + "</td>";
                tr += "<td>" + res[rec].endTime + "</td>";
                tr += "<td>" + (res[rec].room || "TBD") + "</td>";
                tr += "<td>" + (res[rec].facName || "TBD") + "</td>";
                tr += "<td class='action-buttons'>";
                tr += "<button class='schedule-btn schedule-edit-btn btnEditSchedule' data-schedid='" + res[rec].schedId + "' data-day='" + res[rec].day + "' data-starttime='" + res[rec].startTime + "' data-endtime='" + res[rec].endTime + "' data-room='" + res[rec].room + "' data-facid='" + res[rec].facId + "'>Edit</button>";
                tr += "<button class='schedule-btn schedule-delete-btn btnDeleteSchedule' data-schedid='" + res[rec].schedId + "'>Delete</button>";
                tr += "</td>";
                tr += "</tr>";
            }
            $("#viewSchedule").html(tr);
            $("#vwTblSchedules").DataTable();
        }).fail(function (xhr, status, error) {
            console.error("Error fetching schedules: ", error);
            alert("Failed to load schedules: " + error);
        });
    }

    viewData();

    function populateDropdowns(modalPrefix = "") {
        $.post("../Assign_Schedule/GetPrograms", {}, function (res) {
            if (res.error) {
                console.error("Error fetching programs: ", res.error);
                alert(res.error);
                return;
            }
            var options = "<option value='' disabled selected>Select a program</option>";
            for (var rec in res) {
                options += "<option value='" + res[rec].id + "'>" + res[rec].name + "</option>";
            }
            $("#" + modalPrefix + "programId").html(options);
        }).fail(function (xhr, status, error) {
            console.error("Error fetching programs: ", error);
            alert("Failed to load programs: " + error);
        });

        $.post("../Assign_Schedule/GetCourses", {}, function (res) {
            if (res.error) {
                console.error("Error fetching courses: ", res.error);
                alert(res.error);
                return;
            }
            var options = "<option value='' disabled selected>Select a course</option>";
            for (var rec in res) {
                options += "<option value='" + res[rec].id + "'>" + res[rec].name + "</option>";
            }
            $("#" + modalPrefix + "courseId").html(options);
        }).fail(function (xhr, status, error) {
            console.error("Error fetching courses: ", error);
            alert("Failed to load courses: " + error);
        });

        $.post("../Assign_Schedule/GetSections", {}, function (res) {
            if (res.error) {
                console.error("Error fetching sections: ", res.error);
                alert(res.error);
                return;
            }
            var options = "<option value='' disabled selected>Select a section</option>";
            for (var rec in res) {
                options += "<option value='" + res[rec].id + "'>" + res[rec].name + "</option>";
            }
            $("#" + modalPrefix + "secId").html(options);
        }).fail(function (xhr, status, error) {
            console.error("Error fetching sections: ", error);
            alert("Failed to load sections: " + error);
        });

        $.post("../Assign_Schedule/GetFaculty", {}, function (res) {
            if (res.error) {
                console.error("Error fetching faculty: ", res.error);
                alert(res.error);
                return;
            }
            var options = "<option value='' selected>Select a faculty (optional)</option>";
            for (var rec in res) {
                options += "<option value='" + res[rec].id + "'>" + res[rec].name + "</option>";
            }
            $("#" + modalPrefix + "facId").html(options);
        }).fail(function (xhr, status, error) {
            console.error("Error fetching faculty: ", error);
            alert("Failed to load faculty: " + error);
        });
    }

    function renderScheduleDetails(containerId, details) {
        var html = "";
        if (details.length === 0) {
            html = "<tr><td colspan='5'>No schedule details added.</td></tr>";
        } else {
            details.forEach(function (detail, index) {
                html += "<tr>";
                html += "<td>" + detail.day + "</td>";
                html += "<td>" + detail.startTime + "</td>";
                html += "<td>" + detail.endTime + "</td>";
                html += "<td>" + (detail.room || "TBD") + "</td>";
                html += "<td><button type='button' class='schedule-btn schedule-delete-btn remove-schedule-detail' data-index='" + index + "'>Remove</button></td>";
                html += "</tr>";
            });
        }
        $("#" + containerId).html(html);
    }

    $("#addScheduleBtn").click(function () {
        console.log("Opening add modal");
        scheduleDetails = [];
        populateDropdowns();
        renderScheduleDetails("scheduleDetailsBody", scheduleDetails);
        $("#addScheduleModal").addClass("show").show();
        $("#sectionSchedulesBody").html("");
    });

    $("#viewSchedulesBtn").click(function () {
        console.log("Opening view schedules modal");
        $.post("../Assign_Schedule/ViewGroupedSchedules", {}, function (res) {
            if (res.error) {
                console.error("Error fetching grouped schedules: ", res.error);
                alert(res.error);
                return;
            }

            var html = "";
            for (var section in res) {
                html += "<div class='section-group'>";
                html += "<h3>" + section + "</h3>";

                html += "<table class='section-schedule-table'>";
                html += "<thead>";
                html += "<tr>";
                html += "<th>MIS Code</th>";
                html += "<th>Course Code</th>";
                html += "<th class='description'>Descriptive Title</th>";
                html += "<th class='numeric'>Units</th>";
                html += "<th class='time-day'>Time and Day</th>";
                html += "<th>Room</th>";
                html += "</tr>";
                html += "</thead>";
                html += "<tbody>";

                var totalUnits = 0;
                for (var i = 0; i < res[section].length; i++) {
                    var course = res[section][i];
                    totalUnits += parseFloat(course.units);

                    var timeDayEntries = course.schedules.map(s => `${s.time} (${s.day})`).join("<br>");
                    var rooms = course.schedules.map(s => s.room || "TBD").join("<br>");

                    html += "<tr>";
                    html += "<td>" + course.misCode + "</td>";
                    html += "<td>" + course.subject + "</td>";
                    html += "<td>" + course.description + "</td>";
                    html += "<td class='numeric'>" + course.units + "</td>";
                    html += "<td class='time-day'>" + timeDayEntries + "</td>";
                    html += "<td>" + rooms + "</td>";
                    html += "</tr>";
                }

                html += "</tbody>";
                html += "<tfoot>";
                html += "<tr class='total-row'>";
                html += "<th colspan='3'>TOTAL</th>";
                html += "<th class='numeric'>" + totalUnits + "</th>";
                html += "<th colspan='2'></th>";
                html += "</tr>";
                html += "</tfoot>";
                html += "</table>";
                html += "</div>";
            }
            $("#groupedSchedules").html(html);
            $("#viewSchedulesModal").addClass("show").show();
        }).fail(function (xhr, status, error) {
            console.error("Error fetching grouped schedules: ", error);
            alert("Failed to load grouped schedules: " + error);
        });
    });

    $(".modal-close").click(function () {
        $(".modal").removeClass("show").hide();
        $("#addScheduleForm")[0].reset();
        $("#editScheduleForm")[0].reset();
        scheduleDetails = [];
        renderScheduleDetails("scheduleDetailsBody", scheduleDetails);
        renderScheduleDetails("editScheduleDetailsBody", scheduleDetails);
        $("#sectionSchedulesBody").html("");
        $("#groupedSchedules").html("");
    });

    $(".modal").click(function (e) {
        if ($(e.target).hasClass('modal')) {
            $(this).removeClass("show").hide();
            $("#addScheduleForm")[0].reset();
            $("#editScheduleForm")[0].reset();
            scheduleDetails = [];
            renderScheduleDetails("scheduleDetailsBody", scheduleDetails);
            renderScheduleDetails("editScheduleDetailsBody", scheduleDetails);
            $("#sectionSchedulesBody").html("");
            $("#groupedSchedules").html("");
        }
    });

    $("#checkSchedulesBtn").click(function () {
        var secId = $("#secId").val();
        if (!secId) {
            alert("Please select a section first.");
            return;
        }

        $.post("../Assign_Schedule/GetSectionSchedules", { secId: secId }, function (res) {
            if (res.error) {
                console.error("Error fetching section schedules: ", res.error);
                alert(res.error);
                return;
            }

            var tr = "";
            for (var rec in res) {
                tr += "<tr>";
                tr += "<td>" + res[rec].misCode + "</td>";
                tr += "<td>" + res[rec].courseName + "</td>";
                tr += "<td>" + res[rec].day + "</td>";
                tr += "<td>" + res[rec].startTime + "</td>";
                tr += "<td>" + res[rec].endTime + "</td>";
                tr += "<td>" + (res[rec].room || "TBD") + "</td>";
                tr += "</tr>";
            }
            $("#sectionSchedulesBody").html(tr);
        }).fail(function (xhr, status, error) {
            console.error("Error fetching section schedules: ", error);
            alert("Failed to load section schedules: " + error);
        });
    });

    $("#addScheduleDetailBtn").click(function () {
        var day = $("#day").val();
        var startTime = $("#startTime").val();
        var endTime = $("#endTime").val();
        var room = $("#room").val();

        if (!day || !startTime || !endTime) {
            alert("Please fill in day, start time, and end time.");
            return;
        }

        const validDays = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"];
        if (!validDays.includes(day)) {
            alert("Please select a valid day of the week.");
            return;
        }

        // Convert times to Date objects for comparison
        const start = new Date(`1970-01-01T${startTime}:00`);
        const end = new Date(`1970-01-01T${endTime}:00`);

        if (end <= start) {
            alert("End time must be after start time.");
            return;
        }

        // Check for overlap with existing schedule details
        for (let detail of scheduleDetails) {
            if (detail.day !== day) continue;

            const existingStart = new Date(`1970-01-01T${detail.startTime}:00`);
            const existingEnd = new Date(`1970-01-01T${detail.endTime}:00`);

            if (start < existingEnd && new Date(`1970-01-01T${startTime}:00`) < existingEnd) {
                alert(`Schedule overlaps on ${day} with existing time ${detail.startTime}-${detail.endTime}.`);
                return;
            }
        }

        // Add seconds to match backend expected format (HH:mm:ss)
        startTime = startTime + ":00";
        endTime = endTime + ":00";

        scheduleDetails.push({
            day: day,
            startTime: startTime,
            endTime: endTime,
            room: room || null
        });

        renderScheduleDetails("scheduleDetailsBody", scheduleDetails);
        $("#day").val("");
        $("#startTime").val("");
        $("#endTime").val("");
        $("#room").val("");
    });

    $(document).on("click", ".remove-schedule-detail", function () {
        var index = $(this).data("index");
        scheduleDetails.splice(index, 1);
        renderScheduleDetails($(this).closest("tbody").attr("id"), scheduleDetails);
    });

    $("#btnSaveSchedule").click(function (e) {
        e.preventDefault();
        var programId = $("#programId").val();
        var courseId = $("#courseId").val();
        var secId = $("#secId").val();
        var facId = $("#facId").val();

        if (!programId || !courseId || !secId) {
            alert("Please fill in program, course, and section.");
            return;
        }

        if (scheduleDetails.length === 0) {
            alert("Please add at least one schedule detail.");
            return;
        }

        $.post("../Assign_Schedule/AddSchedule", {
            programId: programId,
            courseId: courseId,
            secId: secId,
            facId: facId || null,
            scheduleDetails: JSON.stringify(scheduleDetails)
        }, function (res) {
            if (res[0].mess == 0) {
                alert("Schedule successfully added");
                viewData();
                $("#addScheduleModal").removeClass("show").hide();
                $("#addScheduleForm")[0].reset();
                scheduleDetails = [];
                renderScheduleDetails("scheduleDetailsBody", scheduleDetails);
                $("#sectionSchedulesBody").html("");
            } else {
                alert("Error adding schedule: " + (res[0].error || "Unknown error"));
                console.log("Server response: ", res);
            }
        }).fail(function (xhr, status, error) {
            console.error("Error adding schedule: ", error);
            alert("Failed to add schedule: " + error);
        });
    });

    $(document).on("click", ".btnEditSchedule", function () {
        var schedId = $(this).data("schedid");
        var day = $(this).data("day").split(", ");
        var startTime = $(this).data("starttime").split("; ");
        var endTime = $(this).data("endtime").split("; ");
        var room = $(this).data("room").split(", ");
        var facId = $(this).data("facid");

        // Convert AM/PM format to 24-hour HH:mm:ss for backend
        function convertTo24HourFormat(timeStr) {
            if (!timeStr) return "";
            const [time, period] = timeStr.split(" ");
            let [hours, minutes] = time.split(":").map(Number);
            if (period === "PM" && hours !== 12) hours += 12;
            if (period === "AM" && hours === 12) hours = 0;
            return `${hours.toString().padStart(2, "0")}:${minutes.toString().padStart(2, "0")}:00`;
        }

        scheduleDetails = [];
        for (var i = 0; i < day.length; i++) {
            // Convert the AM/PM time to 24-hour format
            var formattedStartTime = convertTo24HourFormat(startTime[i]);
            var formattedEndTime = convertTo24HourFormat(endTime[i]);

            scheduleDetails.push({
                day: day[i],
                startTime: formattedStartTime,
                endTime: formattedEndTime,
                room: room[i] === "TBD" ? null : room[i]
            });
        }

        $("#editSchedId").val(schedId);
        renderScheduleDetails("editScheduleDetailsBody", scheduleDetails);
        $("#editScheduleModal").addClass("show").show();

        $.post("../Assign_Schedule/GetFaculty", {}, function (res) {
            if (res.error) {
                console.error("Error fetching faculty: ", res.error);
                alert(res.error);
                return;
            }

            var options = "<option value='' " + (!facId ? "selected" : "") + ">Select a faculty (optional)</option>";
            var facIdFound = !facId;

            for (var rec in res) {
                var selected = (res[rec].id == facId) ? "selected" : "";
                options += "<option value='" + res[rec].id + "' " + selected + ">" + res[rec].name + "</option>";
                if (res[rec].id == facId) {
                    facIdFound = true;
                }
            }

            $("#editFacId").html(options);

            if (!facIdFound && facId) {
                console.warn("Faculty ID", facId, "not found in the faculty list");
            }
        }).fail(function (xhr, status, error) {
            console.error("Error fetching faculty: ", error);
            alert("Failed to load faculty: " + error);
        });
    });

    $("#addEditScheduleDetailBtn").click(function () {
        var day = $("#editDay").val();
        var startTime = $("#editStartTime").val();
        var endTime = $("#editEndTime").val();
        var room = $("#editRoom").val();

        if (!day || !startTime || !endTime) {
            alert("Please fill in day, start time, and end time.");
            return;
        }

        const validDays = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"];
        if (!validDays.includes(day)) {
            alert("Please select a valid day of the week.");
            return;
        }

        // Convert times to Date objects for comparison
        const start = new Date(`1970-01-01T${startTime}:00`);
        const end = new Date(`1970-01-01T${endTime}:00`);

        if (end <= start) {
            alert("End time must be after start time.");
            return;
        }

        // Check for overlap with existing schedule details
        for (let detail of scheduleDetails) {
            if (detail.day !== day) continue;

            const existingStart = new Date(`1970-01-01T${detail.startTime}:00`);
            const existingEnd = new Date(`1970-01-01T${detail.endTime}:00`);

            if (start < existingEnd && new Date(`1970-01-01T${startTime}:00`) < existingEnd) {
                alert(`Schedule overlaps on ${day} with existing time ${detail.startTime}-${detail.endTime}.`);
                return;
            }
        }

        // Add seconds to match backend expected format (HH:mm:ss)
        startTime = startTime + ":00";
        endTime = endTime + ":00";

        scheduleDetails.push({
            day: day,
            startTime: startTime,
            endTime: endTime,
            room: room || null
        });

        renderScheduleDetails("editScheduleDetailsBody", scheduleDetails);
        $("#editDay").val("");
        $("#editStartTime").val("");
        $("#editEndTime").val("");
        $("#editRoom").val("");
    });

    $("#btnUpdateSchedule").click(function (e) {
        e.preventDefault();
        var schedId = $("#editSchedId").val();
        var facId = $("#editFacId").val();

        if (!schedId) {
            alert("Missing schedule identifier. Please try again.");
            return;
        }

        if (scheduleDetails.length === 0) {
            alert("Please add at least one schedule detail.");
            return;
        }

        $.post("../Assign_Schedule/UpdateSchedule", {
            schedId: schedId,
            facId: facId || null,
            scheduleDetails: JSON.stringify(scheduleDetails)
        }, function (res) {
            if (res[0].mess == 0) {
                alert("Schedule successfully updated");
                viewData();
                $("#editScheduleModal").removeClass("show").hide();
                $("#editScheduleForm")[0].reset();
                scheduleDetails = [];
                renderScheduleDetails("editScheduleDetailsBody", scheduleDetails);
            } else {
                alert("Error updating schedule: " + (res[0].error || "Unknown error"));
                console.log("Server response: ", res);
            }
        }).fail(function (xhr, status, error) {
            console.error("Error updating schedule: ", error);
            alert("Failed to update schedule: " + error);
        });
    });

    $(document).on("click", ".btnDeleteSchedule", function () {
        if (!confirm("Are you sure you want to delete this schedule?")) {
            return;
        }

        var schedId = $(this).data("schedid");

        if (!schedId) {
            alert("Invalid schedule selected. Missing identifier.");
            return;
        }

        $.post("../Assign_Schedule/DeleteSchedule", {
            schedId: schedId
        }, function (res) {
            if (res[0].mess == 0) {
                alert("Schedule successfully deleted");
                viewData();
            } else {
                alert("Error deleting schedule: " + (res[0].error || "Unknown error"));
                console.log("Server response: ", res);
            }
        }).fail(function (xhr, status, error) {
            console.error("Error deleting schedule: ", error);
            alert("Failed to delete schedule: " + error);
        });
    });
});