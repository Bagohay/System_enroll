$(document).ready(function () {
    loadCourses();
});

function loadCourses() {
    var programId = $("#hiddenProgramId").val();
    console.log("Loading courses for program ID:", programId);

    if (!programId) {
        $("#courseTables").html("<p>Program ID is missing.</p>");
        return;
    }

    $.post("/Course/Get_Program_Courses", { programId: programId }, function (res) {
        console.log("Response received:", res);

        if (res.error) {
            $("#courseTables").html("<p>" + res.error + "</p>");
            return;
        }

        if (res.length === 0) {
            $("#courseTables").html("<p>No courses found for this program.</p>");
            return;
        }

        // Group courses by year and semester
        var groupedCourses = {};
        for (var i = 0; i < res.length; i++) {
            var course = res[i];
            var year = course.YearLevel;
            var semester = course.Semester;

            if (!groupedCourses[year]) {
                groupedCourses[year] = {};
            }
            if (!groupedCourses[year][semester]) {
                groupedCourses[year][semester] = [];
            }
            groupedCourses[year][semester].push(course);
        }

        // Sort years and semesters
        var years = Object.keys(groupedCourses).sort();
        var html = "";

        // Generate tables for each year and semester
        years.forEach(function (year) {
            html += "<div class='year-header'>" + getYearLabel(year) + " YEAR</div>";
            var semesters = Object.keys(groupedCourses[year]).sort();
            semesters.forEach(function (semester) {
                var courses = groupedCourses[year][semester];
                html += "<div class='semester-header'>" + getSemesterLabel(semester) + " SEMESTER</div>";
                html += "<table class='table'>";
                html += "<thead><tr>";
                html += "<th>Course Code</th>";
                html += "<th>Descriptive Title</th>";
                html += "<th>Co-/Prerequisite</th>";
                html += "<th>Units</th>";
                html += "<th colspan='3'>Hours</th>";
                html += "<th>Remarks</th>";
                html += "</tr><tr>";
                html += "<th></th><th></th><th></th><th></th>";
                html += "<th>Lec</th><th>Lab</th><th>Total</th><th></th>";
                html += "</tr></thead>";
                html += "<tbody>";

                // Add course rows
                var totalUnits = 0, totalLecHours = 0, totalLabHours = 0, totalHours = 0;
                courses.forEach(function (course) {
                    html += "<tr>";
                    html += "<td>" + course.CourseCode + "</td>";
                    html += "<td>" + course.CourseName + "</td>";
                    html += "<td>" + course.Prerequisites + "</td>";
                    html += "<td>" + course.Units + "</td>";
                    html += "<td>" + course.LecHours + "</td>";
                    html += "<td>" + course.LabHours + "</td>";
                    html += "<td>" + course.TotalHours + "</td>";
                    html += "<td></td>"; // Remarks column (empty for now)
                    html += "</tr>";

                    // Accumulate totals
                    totalUnits += parseFloat(course.Units) || 0;
                    totalLecHours += parseInt(course.LecHours) || 0;
                    totalLabHours += parseInt(course.LabHours) || 0;
                    totalHours += parseInt(course.TotalHours) || 0;
                });

                // Add total row
                html += "<tr class='total-row'>";
                html += "<td colspan='3'>TOTAL</td>";
                html += "<td>" + totalUnits.toFixed(1) + "</td>";
                html += "<td>" + totalLecHours + "</td>";
                html += "<td>" + totalLabHours + "</td>";
                html += "<td>" + totalHours + "</td>";
                html += "<td></td>";
                html += "</tr>";

                html += "</tbody></table>";
            });
        });

        $("#courseTables").html(html);
    }).fail(function () {
        $("#courseTables").html("<p>Error loading courses.</p>");
    });
}

function getYearLabel(year) {
    switch (parseInt(year)) {
        case 1: return "FIRST";
        case 2: return "SECOND";
        case 3: return "THIRD";
        case 4: return "FOURTH";
        case 5: return "FIFTH";
        default: return year + "TH";
    }
}

function getSemesterLabel(semester) {
    switch (parseInt(semester)) {
        case 1: return "1ST";
        case 2: return "2ND";
        default: return semester + "TH";
    }
}