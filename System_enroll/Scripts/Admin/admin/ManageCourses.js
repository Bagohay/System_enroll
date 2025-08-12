function loadCourses() {
    $.post("/Course/Get_Courses", {}, function (res) {
        let tr = "";
        if (res.error) {
            tr = `<tr><td colspan='5'>${res.error}</td></tr>`;
        } else {
            res.forEach(course => {
                tr += "<tr class='course'>";
                tr += `<td class='courseCode'>${course.courseCode}</td>`;
                tr += `<td class='courseName'>${course.courseName}</td>`;
                tr += `<td class='yearLevel'>${course.yearLevel || "N/A"}</td>`;
                tr += `<td class='units'>${course.units}</td>`;
                tr += "<td>";
                tr += `<button class='btn btn-primary btn-sm' onclick='openPrerequisiteModal(${course.id}, \"${course.courseName}\")'>Add Prerequisites</button>`;
                tr += "</td>";
                tr += "</tr>";
            });
        }
        $("#courseTableBody").html(tr);
        $("#courseTable").DataTable();
    }).fail(function (jqXHR, textStatus, errorThrown) {
        handleAjaxError(jqXHR, textStatus, errorThrown, "#courseTableBody");
    });
}

function openModal() {
    $("#addCourseModal").show();
}

function closeModal() {
    $("#addCourseModal").hide();
    $("#courseCode").val("");
    $("#courseName").val("");
    $("#units").val("");
    $("#yearLevel").val("");
    $("#description").val("");
    $("#lecHours").val("");
    $("#labHours").val("");
    $("#totalHours").val("");
}

function addCourse() {
    const courseCode = $("#courseCode").val().trim();
    const courseName = $("#courseName").val().trim();
    const units = $("#units").val().trim();
    const yearLevel = $("#yearLevel").val().trim();
    const description = $("#description").val().trim();
    const lecHours = $("#lecHours").val().trim();
    const labHours = $("#labHours").val().trim();
    const totalHours = $("#totalHours").val().trim();

    const errors = [];
    if (!courseCode) errors.push("Course code is required.");
    if (!courseName) errors.push("Course name is required.");
    if (!units) errors.push("Units are required.");

    const unitsValue = parseFloat(units);
    if (units && (isNaN(unitsValue) || unitsValue <= 0)) {
        errors.push("Units must be a positive number.");
    }

    if (errors.length > 0) {
        alert(errors.join("\n"));
        return;
    }

    $.post("/Course/Add_Course", {
        courseCode,
        courseName,
        units,
        yearLevel,
        description,
        lecHours,
        labHours,
        totalHours
    }, function (res) {
        if (res.success) {
            alert(res.message);
            closeModal();
            loadCourses();
        } else {
            alert(res.message);
        }
    }).fail(function (jqXHR, textStatus, errorThrown) {
        console.error(`Error adding course: ${textStatus}, ${errorThrown}`);
        alert("Error adding course.");
    });
}

function openPrerequisiteModal(courseId, courseName) {
    $("#prereqCourseId").val(courseId);
    $("#prereqCourseName").text(courseName);
    $("#prerequisites").empty();

    $.post("/Course/Get_Courses_Except", {
        courseId: courseId
    }, function (res) {
        let options = "";
        res.forEach(course => {
            options += `<option value="${course.id}">${course.courseCode} - ${course.courseName}</option>`;
        });
        $("#prerequisites").html(options);
        $("#addPrerequisiteModal").show();
    }).fail(function (jqXHR, textStatus, errorThrown) {
        console.error(`Error fetching courses: ${textStatus}, ${errorThrown}`);
        alert("Error loading prerequisite courses.");
    });
}

function closePrerequisiteModal() {
    $("#addPrerequisiteModal").hide();
    $("#prereqCourseId").val("");
    $("#prereqCourseName").text("");
    $("#prerequisites").empty();
}

function addPrerequisites() {
    const courseId = $("#prereqCourseId").val();
    const selectedOptions = $("#prerequisites").val();
    const prerequisiteIds = selectedOptions ? selectedOptions.join(",") : "";

    $.post("/Course/Save_Prerequisites", {
        courseId: courseId,
        prerequisiteIds: prerequisiteIds
    }, function (res) {
        if (res.success) {
            alert(res.message);
            closePrerequisiteModal();
        } else {
            alert(res.message);
        }
    }).fail(function (jqXHR, textStatus, errorThrown) {
        console.error(`Error adding prerequisites: ${textStatus}, ${errorThrown}`);
        alert("Error adding prerequisites.");
    });
}

$(document).ready(function () {
    loadCourses();
    $("#addCourseModal").hide();
    $("#addPrerequisiteModal").hide();
});