$(document).ready(function () {
    // Function to fetch and display sections
    function viewData() {
        $.post("/Section/Get_Sections", {}, function (res) {
            if (res.error) {
                console.error("Error loading section data: ", res.error);
                alert("Error loading section data: " + res.error);
                return;
            }

            // Clear any existing DataTable
            if ($.fn.DataTable.isDataTable('#vwTblSections')) {
                $('#vwTblSections').DataTable().destroy();
            }

            var tr = "";
            for (var rec in res) {
                tr += "<tr>";
                tr += "<td>" + res[rec].secName + "</td>";
                tr += "<td>" + res[rec].programName + "</td>";
                tr += "<td class='action-buttons'>";
                tr += "<button class='section-btn section-edit-btn' data-id='" + res[rec].secId + "' data-secname='" + res[rec].secName + "' data-progid='" + res[rec].progId + "'>Edit</button>";
                tr += "<button class='section-btn section-delete-btn' data-id='" + res[rec].secId + "' data-secname='" + res[rec].secName + "'>Delete</button>";
                tr += "</td>";
                tr += "</tr>";
            }
            $("#viewSection").html(tr);
            $("#vwTblSections").DataTable();
        }).fail(function (xhr, status, error) {
            console.error("Error fetching sections: ", error);
            alert("Failed to load section data: " + error);
        });
    }

    // Load programs into dropdowns
    function loadPrograms() {
        $.post("/Section/Get_Programs", {}, function (res) {
            var options = "<option value=''>Select Program</option>";
            for (var rec in res) {
                options += "<option value='" + res[rec].id + "'>" + res[rec].name + "</option>";
            }
            $("#addSecProgramId").html(options);
            $("#editSecProgramId").html(options);
        }).fail(function (xhr, status, error) {
            console.error("Error fetching programs: ", error);
            alert("Failed to load programs: " + error);
        });
    }

    // Initial load of data
    viewData();
    loadPrograms();

    // Open Add Section modal
    $("#addSectionBtn").click(function () {
        console.log("Opening add modal");
        $("#addSectionModal").addClass("show").show();
    });

    // Close modal
    $(".modal-close").click(function () {
        $(".modal").removeClass("show").hide();
        $("#addSectionForm")[0].reset();
        $("#editSectionForm")[0].reset();
    });

    // Close modal when clicking outside the modal content
    $(".modal").click(function (e) {
        if ($(e.target).hasClass('modal')) {
            $(this).removeClass("show").hide();
            $("#addSectionForm")[0].reset();
            $("#editSectionForm")[0].reset();
        }
    });

    // Submit Add Section
    $("#btnAddSection").click(function () {
        var secName = $("#addSecName").val();
        var programId = $("#addSecProgramId").val();

        // Validate inputs
        if (!secName || !programId) {
            alert("Please fill in all required fields.");
            return;
        }

        $.post("/Section/Add_Section", {
            sectionName: secName,
            programId: programId
        }, function (res) {
            if (res.success) {
                alert("Section successfully added");
                viewData();
                $("#addSectionModal").removeClass("show").hide();
                $("#addSectionForm")[0].reset();
            } else {
                alert("Error adding section: " + (res.message || "Unknown error"));
            }
        }).fail(function (xhr, status, error) {
            console.error("Error adding section: ", error);
            alert("Failed to add section: " + error);
        });
    });

    // Open Edit Section modal
    $(document).on("click", ".section-edit-btn", function () {
        var sectionId = $(this).data("id");
        var secName = $(this).data("secname");
        var progId = $(this).data("progid");

        $("#editSectionId").val(sectionId);
        $("#editSecName").val(secName);
        $("#editSecProgramId").val(progId);

        $("#editSectionModal").addClass("show").show();
    });

    // Submit Edit Section
    $("#btnUpdateSection").click(function () {
        var sectionId = $("#editSectionId").val();
        var secName = $("#editSecName").val();
        var programId = $("#editSecProgramId").val();

        // Validate inputs
        if (!sectionId || !secName || !programId) {
            alert("Please fill in all required fields.");
            return;
        }

        $.post("/Section/Update_Section", {
            sectionId: sectionId,
            sectionName: secName,
            programId: programId
        }, function (res) {
            if (res.success) {
                alert("Section ID " + sectionId + " successfully updated");
                viewData();
                $("#editSectionModal").removeClass("show").hide();
                $("#editSectionForm")[0].reset();
            } else {
                alert("Error updating section: " + (res.message || "Unknown error"));
            }
        }).fail(function (xhr, status, error) {
            console.error("Error updating section: ", error);
            alert("Failed to update section: " + error);
        });
    });

    // Delete Section
    $(document).on("click", ".section-delete-btn", function () {
        var sectionId = $(this).data("id");
        var secName = $(this).data("secname");
        if (confirm("Are you sure you want to delete section: " + secName + "?")) {
            $.post("/Section/Delete_Section", {
                sectionId: sectionId
            }, function (res) {
                if (res.success) {
                    alert("Section " + secName + " is deleted");
                    viewData();
                } else {
                    alert("Error deleting section: " + (res.message || "Unknown error"));
                }
            }).fail(function (xhr, status, error) {
                console.error("Error deleting section: ", error);
                alert("Failed to delete section: " + error);
            });
        }
    });
});