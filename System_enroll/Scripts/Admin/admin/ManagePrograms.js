function loadPrograms() {
    var urlParams = new URLSearchParams(window.location.search);
    var deptId = urlParams.get('deptId') || $("#hiddenDeptId").val();
    console .log("Loading programs for department ID:", deptId);
    if (!deptId)

{
    $("#programTableBody").html("<tr><td colspan='6'>Department ID is missing.</td></tr>");
    return;
}

$.post("/Course/Get_Programs", {
        deptId: deptId
    }, function (res) {
        console.log("Response received:", res);

        if (res.error) {
            $("#programTableBody").html("<tr><td colspan='6'>" + res.error + "</td></tr>");
            return;
        }

        if (res.length === 0) {
            $("#programTableBody").html("<tr><td colspan='6'>No programs found for this department.</td></tr>");
            return;
        }

        var tbody = "";
        for (var i = 0; i < res.length; i++) {
            var program = res[i];
            tbody += "<tr class='program'>";
            tbody += "<td class='name'>" + program.Name + "</td>";
            tbody += "<td class='code'>" + program.Code + "</td>";
            tbody += "<td class='degree'>" + program.DegreeType + "</td>";
            tbody += "<td class='duration'>" + program.Duration + "</td>";
            tbody += "<td class='department'>" + program.DeptName + "</td>";
            tbody += "<td>";
            tbody += "<a href='javascript:void(0)' class='btn btn-primary' onclick='openAddCourseModal(" + program.Id + ")'>+ Course</a> ";
            tbody += "<a href='/Course/View_Courses?programId=" + program.Id + "&deptId=" + deptId + "' class='btn btn-primary'>View Courses</a>";
            tbody += "</td>";
            tbody += "</tr>";
        }
        $("#programTableBody").html(tbody);
    }).fail(function () {
        $("#programTableBody").html("<tr><td colspan='6'>Error loading programs.</td></tr>");
    });
}

function openAddProgramModal() {
    $("#addProgramModal").show();
}

function closeAddProgramModal() {
    $("#addProgramModal").hide();
    $("#programName").val("");
    $("#programCode").val("");
    $("#degreeType").val("Bachelors");
    $("#duration").val("");
}

function addProgram() {
    var urlParams = new URLSearchParams(window.location.search);
    var deptId = urlParams.get('deptId') || $("#hiddenDeptId").val();
    if (!deptId)

{
    alert("Department ID is missing.");
    return;
}

var programName = $("#programName").val().trim();
var programCode = $("#programCode").val().trim();
var degreeType = $("#degreeType").val();
var duration = $("#duration").val().trim();

if (!programName || !programCode || !duration) {
    alert("Please fill in all required fields (Program Name, Code, and Duration).");
    return;
}

if (isNaN(duration) || duration <= 0) {
    alert("Duration must be a positive number.");
    return;
}

$.post("/Course/Add_Program", {
        deptId: deptId,
        name: programName,
        code: programCode,
        degreeType: degreeType,
        duration: duration
    }, function (res) {
        if (res.success) {
            alert(res.message);
            closeAddProgramModal();
            loadPrograms();
        } else {
            alert(res.message);
        }
    }).fail(function () {
        alert("Error adding program.");
    });
}

function openAddCourseModal(programId) {
    $("#programId").val(programId);
    $("#addCourseModal").show();
    $.post("/Course/Get_Courses", {}, function (res) {
        var options = "<option value=''>Select a course</option>";
        if (!res.error && res.length > 0) {
            res.forEach(function (course) {
                options += `<option value="${course.id}">${course.courseCode} - ${course.courseName}</option>`;
            });
        }
        $("#courseId").html(options);
    }).fail(function () {
        $("#courseId").html("<option value=''>Error loading courses</option>");
    });
}

function closeAddCourseModal() {
    $("#addCourseModal").hide();
    $("#programId").val("");
    $("#courseId").val("");
    $("#semester").val("");
    $("#yearLevel").val("");
    $("#isRequired").prop("checked", true);
    $("#isElective").prop("checked", false);
}

function addCourseToProgram() {
    var programId = $("#programId").val();
    var courseId = $("#courseId").val();
    var semester = $("#semester").val().trim();
    var yearLevel = $("#yearLevel").val().trim();
    var isRequired = $("#isRequired").prop("checked");
    var isElective = $("#isElective").prop("checked");
    if (!programId || !courseId || !semester || !yearLevel)

{
    alert("Please fill in all required fields (Course, Semester, Year Level).");
    return;
}

var semesterInt = parseInt(semester);
var yearLevelInt = parseInt(yearLevel);
if (isNaN(semesterInt) || semesterInt < 1 || semesterInt > 2) {
    alert("Semester must be 1 or 2.");
    return;
}

if (isNaN(yearLevelInt) || yearLevelInt < 1 || yearLevelInt > 5) {
    alert("Year level must be between 1 and 5.");
    return;
}

$.post("/Course/Add_Program_Course", {
        programId: programId,
        courseId: courseId,
        semester: semesterInt,
        yearLevel: yearLevelInt,
        isRequired: isRequired,
        isElective: isElective
    }, function (res) {
        if (res.success) {
            alert(res.message);
            closeAddCourseModal();
        } else {
            alert(res.message);
        }
    }).fail(function () {
        alert("Error adding course to program.");
    });
}

$(document).ready(function () {
    $("#addProgramModal").hide();
    $("#addCourseModal").hide();
    loadPrograms();
});
