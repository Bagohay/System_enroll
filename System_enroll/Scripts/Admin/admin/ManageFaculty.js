$(document).ready(function () {
    // Initialize DataTable once
    var table = $("#vwTblFaculties").DataTable({
        responsive: true
    });

    function viewData() {
        $.post("../Faculty/View_Faculties", {}, function (res) {
            if (res.mess === 1) {
                alert("Error loading faculty data: " + res.error);
                return;
            }
            // Clear existing rows
            table.clear();
            // Add new rows
            for (var rec in res) {
                // Escape data to prevent XSS
                var facName = $("<div>").text(res[rec].facName).html();
                var facSpecialization = $("<div>").text(res[rec].facSpecialization).html();
                var facContactInfo = $("<div>").text(res[rec].facContactInfo).html();
                var facIsAdvisor = $("<div>").text(res[rec].facIsAdvisor).html();

                var rowData = [
                    res[rec].facId,
                    facName,
                    facSpecialization,
                    facContactInfo,
                    facIsAdvisor,
                    "<button class='faculty-btn faculty-edit-btn' data-id='" + res[rec].facId + "' data-facname='" + facName + "' data-facspecialization='" + facSpecialization + "' data-faccontactinfo='" + facContactInfo + "' data-facisadvisor='" + facIsAdvisor + "'>Edit</button>" +
                    "<button class='faculty-btn faculty-delete-btn' data-id='" + res[rec].facId + "'>Delete</button>" +
                    "<button class='faculty-btn faculty-view-btn' data-id='" + res[rec].facId + "'>View Section</button>"
                ];
                table.row.add(rowData);
            }
            // Redraw table
            table.draw();
        }).fail(function (xhr, status, error) {
            alert("Failed to load faculty data: " + error);
        });
    }

    viewData();

    // Open Add Faculty modal
    $("#addFacultyBtn").click(function () {
        $("#addFacultyModal").show();
        $("#addFacName").val("");
        $("#addFacSpecialization").val("");
        $("#addFacContactInfo").val("");
        $("#addFacIsAdvisor").val("No");
    });

    // Close modals
    $(".modal-close").click(function () {
        $("#addFacultyModal").hide();
        $("#editFacultyModal").hide();
    });

    // Submit Add Faculty
    $("#btnAddFaculty").click(function () {
        var facName = $("#addFacName").val();
        var facSpecialization = $("#addFacSpecialization").val();
        var facContactInfo = $("#addFacContactInfo").val();
        var facIsAdvisor = $("#addFacIsAdvisor").val();

        $.post("../Faculty/AddFaculty", {
            facName: facName,
            facSpecialization: facSpecialization,
            facContactInfo: facContactInfo,
            facIsAdvisor: facIsAdvisor
        }, function (res) {
            if (res[0].mess === 0) {
                alert("Faculty successfully added");
                viewData();
                $("#addFacultyModal").hide();
            } else {
                alert("Error adding faculty: " + res[0].error);
            }
        }).fail(function (xhr, status, error) {
            alert("Failed to add faculty: " + error);
        });
    });

    // Open Edit Faculty modal
    $(document).on("click", ".faculty-edit-btn", function () {
        var facultyId = $(this).data("id");
        var facName = $(this).data("facname");
        var facSpecialization = $(this).data("facspecialization");
        var facContactInfo = $(this).data("faccontactinfo");
        var facIsAdvisor = $(this).data("facisadvisor");

        $("#editFacultyId").val(facultyId);
        $("#editFacName").val(facName);
        $("#editFacSpecialization").val(facSpecialization);
        $("#editFacContactInfo").val(facContactInfo);
        $("#editFacIsAdvisor").val(facIsAdvisor);
        $("#editFacultyModal").show();
    });

    // Submit Edit Faculty
    $("#btnUpdateFaculty").click(function () {
        var facultyId = $("#editFacultyId").val();
        var facName = $("#editFacName").val();
        var facSpecialization = $("#editFacSpecialization").val();
        var facContactInfo = $("#editFacContactInfo").val();
        var facIsAdvisor = $("#editFacIsAdvisor").val();

        $.post("../Faculty/UpdateFaculty", {
            id: facultyId,
            facName: facName,
            facSpecialization: facSpecialization,
            facContactInfo: facContactInfo,
            facIsAdvisor: facIsAdvisor
        }, function (res) {
            if (res[0].mess === 0) {
                alert("Faculty ID " + facultyId + " successfully updated");
                viewData();
                $("#editFacultyModal").hide();
            } else {
                alert("Error updating faculty: " + res[0].error);
            }
        }).fail(function (xhr, status, error) {
            alert("Failed to update faculty: " + error);
        });
    });

    // Delete Faculty
    $(document).on("click", ".faculty-delete-btn", function () {
        var facultyId = $(this).data("id");
        if (confirm("Are you sure you want to delete faculty with ID: " + facultyId + "?")) {
            $.post("../Faculty/DeleteFaculty", {
                id: facultyId
            }, function (res) {
                if (res[0].mess === 0) {
                    alert("Faculty ID " + facultyId + " is deleted");
                } else {
                    alert("Error deleting faculty: " + res[0].error);
                }
                viewData();
            }).fail(function (xhr, status, error) {
                alert("Failed to delete faculty: " + error);
            });
        }
    });

    // View Section (placeholder)
    $(document).on("click", ".faculty-view-btn", function () {
        var facultyId = $(this).data("id");
        alert("View Section for faculty ID: " + facultyId + " (Not implemented yet)");
    });
});