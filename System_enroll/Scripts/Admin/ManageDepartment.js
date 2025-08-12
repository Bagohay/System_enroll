$(document).ready(function () {
    // Initialize DataTable
    let table = $("#vwTblDepartments").DataTable({
        paging: true,
        searching: true,
        ordering: true,
        info: true,
        lengthChange: true,
        autoWidth: false,
        responsive: true
    });

    // Function to fetch and display departments
    function viewData() {
        $.post("/Course/Get_Departments", function (res) {
            // Clear existing table data
            if ($.fn.DataTable.isDataTable("#vwTblDepartments")) {
                table.destroy();
            }

            var tbody = "";
            for (var i = 0; i < res.length; i++) {
                tbody += "<tr class='department'>";
                tbody += "<td class='deptId'>" + res[i].deptId + "</td>";
                tbody += "<td class='deptName'>" + res[i].deptName + "</td>";
                tbody += "<td class='action-buttons'>";
                tbody += "<a href='/Course/View_Programs?deptId=" + res[i].deptId + "' class='department-btn department-view-btn'>View Programs</a>";
                tbody += "<button class='department-btn department-edit-btn' data-id='" + res[i].deptId + "' data-deptname='" + res[i].deptName + "'>Edit</button>";
                tbody += "<button class='department-btn department-delete-btn' data-id='" + res[i].deptId + "'>Delete</button>";
                tbody += "</td>";
                tbody += "</tr>";
            }
            $("#departmentTableBody").html(tbody);

            // Reinitialize DataTable after updating the table body
            table = $("#vwTblDepartments").DataTable({
                paging: true,
                searching: true,
                ordering: true,
                info: true,
                lengthChange: true,
                autoWidth: false,
                responsive: true
            });
        });
    }

    // Initial load of data
    viewData();

    // Open Add Department modal
    $(document).on("click", "#addDepartmentBtn", function () {
        $("#addDepartmentModal").css("display", "flex").addClass("show");
    });

    // Close modal
    $(document).on("click", ".modal-close", function () {
        $(this).closest(".modal").css("display", "none").removeClass("show");
        $("#addDeptName").val(""); // Clear add modal input
        $("#editDeptName").val(""); // Clear edit modal input
    });

    // Close modal when clicking outside the modal content
    $(document).on("click", ".modal", function (e) {
        if ($(e.target).hasClass('modal')) {
            $(this).css("display", "none").removeClass("show");
            $("#addDeptName").val("");
            $("#editDeptName").val("");
        }
    });

    // Submit Add Department
    $(document).on("click", "#btnAddDepartment", function () {
        var deptName = $("#addDeptName").val().trim();

        if (!deptName) {
            alert("Please enter a department name.");
            return;
        }

        $.post("/Course/Add_Department", { deptName: deptName }, function (res) {
            if (res.success) {
                alert("Department successfully added");
                viewData();
                $("#addDepartmentModal").css("display", "none").removeClass("show");
                $("#addDeptName").val("");
            } else {
                alert("Error adding department: " + res.message);
            }
        }).fail(function () {
            alert("Failed to communicate with the server. Please try again.");
        });
    });

    // Open Edit Department modal
    $(document).on("click", ".department-edit-btn", function () {
        var deptId = $(this).data("id");
        var deptName = $(this).data("deptname");

        $("#editDeptId").val(deptId);
        $("#editDeptName").val(deptName);

        $("#editDepartmentModal").css("display", "flex").addClass("show");
    });

    // Submit Edit Department
    $(document).on("click", "#btnUpdateDepartment", function () {
        var deptId = $("#editDeptId").val();
        var deptName = $("#editDeptName").val().trim();

        if (!deptId || !deptName) {
            alert("Please fill in all required fields.");
            return;
        }

        $.post("/Course/Update_Department", { deptId: deptId, deptName: deptName }, function (res) {
            if (res.success) {
                alert("Department ID " + deptId + " successfully updated");
                viewData();
                $("#editDepartmentModal").css("display", "none").removeClass("show");
                $("#editDeptName").val("");
            } else {
                alert("Error updating department: " + res.message);
            }
        }).fail(function () {
            alert("Failed to communicate with the server. Please try again.");
        });
    });

    // Delete Department
    $(document).on("click", ".department-delete-btn", function () {
        var deptId = $(this).data("id");
        if (confirm("Are you sure you want to delete department with ID: " + deptId + "?")) {
            $.post("/Course/Delete_Department", { deptId: deptId }, function (res) {
                if (res.success) {
                    alert("Department ID " + deptId + " is deleted");
                    viewData();
                } else {
                    alert("Error deleting department: " + res.message);
                }
            }).fail(function () {
                alert("Failed to communicate with the server. Please try again.");
            });
        }
    });
});