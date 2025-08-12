$(document).ready(function () {
    // Function to fetch and display faculties
    function viewData() {
        $.post("../Faculty/View_Faculties", {}, function (res) {
            if (res.mess === 1) {
                console.error("Error loading faculty data: ", res.error);
                alert("Error loading faculty data: " + res.error);
                return;
            }
            // Clear any existing DataTable
            if ($.fn.DataTable.isDataTable('#vwTblFaculties')) {
                $('#vwTblFaculties').DataTable().destroy();
            }

            var tr = "";
            for (var rec in res) {
                tr += "<tr>";
                tr += "<td>" + res[rec].facId + "</td>";
                tr += "<td>" + res[rec].facName + "</td>";
                tr += "<td>" + res[rec].facSpecialization + "</td>";
                tr += "<td>" + res[rec].facContactInfo + "</td>";
                tr += "<td>" + res[rec].facIsAdvisor + "</td>";
                tr += "<td class='action-buttons'>";
                tr += "<button class='faculty-btn faculty-edit-btn' data-id='" + res[rec].facId + "' data-facname='" + res[rec].facName + "' data-facspecialization='" + res[rec].facSpecialization + "' data-faccontactinfo='" + res[rec].facContactInfo + "' data-facisadvisor='" + res[rec].facIsAdvisor + "'>Edit</button>";
                tr += "<button class='faculty-btn faculty-delete-btn' data-id='" + res[rec].facId + "'>Delete</button>";
               
                tr += "</td>";
                tr += "</tr>";
            }
            $("#viewFaculty").html(tr);
            $("#vwTblFaculties").DataTable();
        }).fail(function (xhr, status, error) {
            console.error("Error fetching faculties: ", error);
            alert("Failed to load faculty data: " + error);
        });
    }

    // Initial load of data
    viewData();

    // Open Add Faculty modal
    $("#addFacultyBtn").click(function () {
        console.log("Opening add modal");
        $("#addFacultyModal").addClass("show").show();
    });

    // Close modal
    $(".modal-close").click(function () {
        $(".modal").removeClass("show").hide();
        $("#addFacultyForm")[0].reset();
        $("#editFacultyForm")[0].reset();
    });

    // Close modal when clicking outside the modal content
    $(".modal").click(function (e) {
        if ($(e.target).hasClass('modal')) {
            $(this).removeClass("show").hide();
            $("#addFacultyForm")[0].reset();
            $("#editFacultyForm")[0].reset();
        }
    });

    // Submit Add Faculty
    $("#btnAddFaculty").click(function () {
        var facName = $("#addFacName").val();
        var facSpecialization = $("#addFacSpecialization").val();
        var facContactInfo = $("#addFacContactInfo").val();
        var facIsAdvisor = $("#addFacIsAdvisor").val();

        // Validate inputs
        if (!facName || !facSpecialization || !facContactInfo) {
            alert("Please fill in all required fields.");
            return;
        }

        $.post("../Faculty/AddFaculty", {
            facName: facName,
            facSpecialization: facSpecialization,
            facContactInfo: facContactInfo,
            facIsAdvisor: facIsAdvisor
        }, function (res) {
            if (res[0].mess === 0) {
                alert("Faculty successfully added");
                viewData();
                $("#addFacultyModal").removeClass("show").hide();
                $("#addFacultyForm")[0].reset();
            } else {
                alert("Error adding faculty: " + (res[0].error || "Unknown error"));
            }
        }).fail(function (xhr, status, error) {
            console.error("Error adding faculty: ", error);
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

        $("#editFacultyModal").addClass("show").show();
    });

    // Submit Edit Faculty
    $("#btnUpdateFaculty").click(function () {
        var facultyId = $("#editFacultyId").val();
        var facName = $("#editFacName").val();
        var facSpecialization = $("#editFacSpecialization").val();
        var facContactInfo = $("#editFacContactInfo").val();
        var facIsAdvisor = $("#editFacIsAdvisor").val();

        // Validate inputs
        if (!facultyId || !facName || !facSpecialization || !facContactInfo) {
            alert("Please fill in all required fields.");
            return;
        }

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
                $("#editFacultyModal").removeClass("show").hide();
                $("#editFacultyForm")[0].reset();
            } else {
                alert("Error updating faculty: " + (res[0].error || "Unknown error"));
            }
        }).fail(function (xhr, status, error) {
            console.error("Error updating faculty: ", error);
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
                    viewData();
                } else {
                    alert("Error deleting faculty: " + (res[0].error || "Unknown error"));
                }
            }).fail(function (xhr, status, error) {
                console.error("Error deleting faculty: ", error);
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