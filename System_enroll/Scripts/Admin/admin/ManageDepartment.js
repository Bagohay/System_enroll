function loadDepartments() {
    $.get("/Course/Get_Departments", {}, function (res) {
        var tbody = "";
        for (var dept of res) {
            tbody += "<tr>";
            tbody += "<td>" + dept.deptId + "</td>";
            tbody += "<td>" + dept.deptName + "</td>";
            tbody += "<td>";
            tbody += "<a href='/Course/View_Programs?deptId=" + dept.deptId + "' class='btn btn-primary btn-sm'>View Programs</a> ";
            tbody += "<a href='/Course/View_Rooms?deptId=" + dept.deptId + "' class='btn btn-primary btn-sm'>View Rooms</a>";
            tbody += "</td>";
            tbody += "</tr>";
        }
        $("#departmentTableBody").html(tbody);
    }).fail(function (jqXHR, textStatus, errorThrown) {
        console.error("Error loading departments:", textStatus, errorThrown);
        $("#departmentTableBody").html("<tr><td colspan='3'>Error loading departments.</td></tr>");
    });
}

function openModal() {
    $("#addDepartmentModal").show();
}

function closeModal() {
    $("#addDepartmentModal").hide();
    $("#departmentName").val("");
}

function addDepartment() {
    var deptName = $("#departmentName").val().trim();
    if (!deptName) {
        alert("Please enter a department name");
        return;
    }

    $.post("/Course/Add_Department", {
        deptName: deptName
    }, function (res) {
        if (res.success) {
            alert(res.message);
            closeModal();
            loadDepartments();
        } else {
            alert(res.message);
        }
    }).fail(function (jqXHR, textStatus, errorThrown) {
        console.error("Error adding department:", textStatus, errorThrown);
        alert("Error adding department.");
    });
}

$(document).ready(function () {
    loadDepartments();
    $("#addDepartmentModal").hide();
});