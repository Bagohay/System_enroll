function loadPrograms() {
    var urlParams = new URLSearchParams(window.location.search);
    var deptId = urlParams.get('deptId');

    if (!deptId) {
        deptId = $("#hiddenDeptId").val();
    }

    if (!deptId) {
        console.error("Department ID is missing.");
        $("#programTableBody").html("<tr><td colspan='6'>Error: Department ID is required.</td></tr>");
        return;
    }

   
    console.log("Loading programs for department ID:", deptId);

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
            tbody += "<a href='/Course/View_Courses?programId=" + program.Id + "' class='btn btn-primary'>+ Course</a> ";
            tbody += "<a href='/Course/View_Courses?programId=" + program.Id + "' class='btn btn-primary'>View Courses</a>";
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
    var deptId = urlParams.get('deptId');

   
    if (!deptId) {
        deptId = $("#hiddenDeptId").val();
    }

    if (!deptId) {
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

$(document).ready(function () {
    
    $("#addProgramModal").hide();

  
    loadPrograms();
});