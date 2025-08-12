function loadSections() {
    $.post("/Section/Get_Sections", {}, function (res) {
        let tr = "";
        if (res.error) {
            tr = `<tr><td colspan='2'>${res.error}</td></tr>`;
        } else {
            res.forEach(section => {
                tr += "<tr>";
                tr += `<td>${section.secName}</td>`;
                tr += `<td>${section.programName}</td>`;
                tr += "</tr>";
            });
        }
        $("#sectionTableBody").html(tr);
    }).fail(function (jqXHR, textStatus, errorThrown) {
        console.error(`Error fetching sections: ${textStatus}, ${errorThrown}`);
        $("#sectionTableBody").html("<tr><td colspan='2'>Error loading sections.</td></tr>");
    });
}

function loadPrograms() {
    $.post("/Section/Get_Programs", {}, function (res) {
        let options = "<option value=''>Select Program</option>";
        res.forEach(program => {
            options += `<option value="${program.id}">${program.name}</option>`;
        });
        $("#programId").html(options);
    }).fail(function (jqXHR, textStatus, errorThrown) {
        console.error(`Error fetching programs: ${textStatus}, ${errorThrown}`);
        alert("Error loading programs.");
    });
}

function openModal() {
    $("#addSectionModal").show();
}

function closeModal() {
    $("#addSectionModal").hide();
    $("#sectionName").val("");
    $("#programId").val("");
}

function addSection() {
    const sectionName = $("#sectionName").val().trim();
    const programId = $("#programId").val().trim();

    const errors = [];
    if (!sectionName) errors.push("Section name is required.");
    if (!programId) errors.push("Please select a program.");

    if (errors.length > 0) {
        alert(errors.join("\n"));
        return;
    }

    $.post("/Section/Add_Section", {
        sectionName,
        programId
    }, function (res) {
        if (res.success) {
            alert(res.message);
            closeModal();
            loadSections();
        } else {
            alert(res.message);
        }
    }).fail(function (jqXHR, textStatus, errorThrown) {
        console.error(`Error adding section: ${textStatus}, ${errorThrown}`);
        alert("Error adding section.");
    });
}

$(document).ready(function () {
    loadSections();
    loadPrograms();
    $("#addSectionModal").hide();
});