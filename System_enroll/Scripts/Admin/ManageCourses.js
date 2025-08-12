function loadCourses() {
    $.post("/Course/Get_Courses", {}, function (res) {
        let tr = "";
        if (res.error) {
            tr = `<tr><td colspan='6'>${res.error}</td></tr>`;
        } else {
            res.forEach(course => {
                tr += "<tr class='course'>";
                tr += `<td>${course.id}</td>`;
                tr += `<td class='courseCode'>${course.courseCode}</td>`;
                tr += `<td class='courseName'>${course.courseName}</td>`;
                tr += `<td class='units'>${course.units}</td>`;
                tr += `<td class='yearLevel'>${course.yearLevel || "N/A"}</td>`;
                tr += "<td class='action-buttons'>";
                tr += `<button class='course-btn view-prerequisites-btn' onclick='openPrerequisiteModal(${course.id}, \"${course.courseName}\")'>View Prerequisites</button>`;
                tr += `<button class='course-btn edit-btn' onclick='openEditModal(${course.id})'>Edit</button>`;
                tr += `<button class='course-btn delete-btn' onclick='deleteCourse(${course.id})'>Delete</button>`;
                tr += "</td>";
                tr += "</tr>";
            });
        }
        $("#courseTableBody").html(tr);

        if ($.fn.DataTable.isDataTable("#courseTable")) {
            $("#courseTable").DataTable().destroy();
        }

        $("#courseTable").DataTable({
            paging: true,
            searching: true,
            ordering: true,
            info: true,
            lengthChange: true
        });
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

function openEditModal(courseId) {
    $.post("/Course/Get_Courses", {}, function (res) {
        const course = res.find(c => c.id == courseId);
        if (course) {
            $("#editCourseId").val(course.id);
            $("#editCourseCode").val(course.courseCode);
            $("#editCourseName").val(course.courseName);
            $("#editUnits").val(course.units);
            $("#editYearLevel").val(course.yearLevel || "");
            $("#editDescription").val(course.description || "");
            $("#editLecHours").val(course.lecHours || "");
            $("#editLabHours").val(course.labHours || "");
            $("#editTotalHours").val(course.totalHours || "");
            $("#editCourseModal").show();
        }
    }).fail(function () {
        alert("Error fetching course details.");
    });
}

function closeEditModal() {
    $("#editCourseModal").hide();
    $("#editCourseId").val("");
    $("#editCourseCode").val("");
    $("#editCourseName").val("");
    $("#editUnits").val("");
    $("#editYearLevel").val("");
    $("#editDescription").val("");
    $("#editLecHours").val("");
    $("#editLabHours").val("");
    $("#editTotalHours").val("");
}

function updateCourse() {
    const courseId = $("#editCourseId").val();
    const courseCode = $("#editCourseCode").val().trim();
    const courseName = $("#editCourseName").val().trim();
    const units = $("#editUnits").val().trim();
    const yearLevel = $("#editYearLevel").val().trim();
    const description = $("#editDescription").val().trim();
    const lecHours = $("#editLecHours").val().trim();
    const labHours = $("#editLabHours").val().trim();
    const totalHours = $("#editTotalHours").val().trim();

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

    $.post("/Course/Update_Course", {
        courseId,
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
            closeEditModal();
            loadCourses();
        } else {
            alert(res.message);
        }
    }).fail(function () {
        alert("Error updating course.");
    });
}

function deleteCourse(courseId) {
    if (confirm("Are you sure you want to delete this course?")) {
        $.post("/Course/Delete_Course", { courseId }, function (res) {
            if (res.success) {
                alert(res.message);
                loadCourses();
            } else {
                alert(res.message);
            }
        }).fail(function () {
            alert("Error deleting course.");
        });
    }
}

function openPrerequisiteModal(courseId, courseName) {
    $("#prereqCourseId").val(courseId);
    $("#prereqCourseName").text(courseName);
    $("#prerequisites").empty();

    $.post("/Course/Get_Program_Courses", { programId: 0 }, function (programRes) {
        const course = programRes.find(c => c.Id == courseId);
        const existingPrereqs = course && course.Prerequisites !== "None" ? course.Prerequisites.split(", ").map(p => p.trim()) : [];

        $.post("/Course/Get_Courses_Except", { courseId: courseId }, function (res) {
            let options = "";
            res.forEach(course => {
                const isSelected = existingPrereqs.includes(course.courseCode) ? "selected" : "";
                options += `<option value="${course.id}" ${isSelected}>${course.courseCode} - ${course.courseName}</option>`;
            });
            $("#prerequisites").html(options);
            $("#prerequisites").prop("multiple", true);
            $("#addPrerequisiteModal").show();
        }).fail(function (jqXHR, textStatus, errorThrown) {
            console.error(`Error fetching courses: ${textStatus}, ${errorThrown}`);
            alert("Error loading prerequisite courses.");
        });
    }).fail(function () {
        alert("Error fetching existing prerequisites.");
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
            loadCourses();
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
    $("#editCourseModal").hide();
    $("#addPrerequisiteModal").hide();
});