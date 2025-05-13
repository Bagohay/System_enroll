function loadPendingEnrollees() {
    $.post("/Admin/View_Students", {}, function (res) {
        var tr = "";
        for (var rec of res) {
            tr += "<tr class='enrollee'>";
            tr += "<td class='idno'>" + rec.studId + "</td>";
            tr += "<td class='name'>" + rec.lastname + ", " + rec.firstname + "</td>";
            tr += "<td class='email'>" + rec.email + "</td>";
            tr += "<td class='program'>" + rec.program + "</td>";
            tr += "<td class='application-date'>" + rec.applicationDate + "</td>";
            tr += "<td>";
            tr += "<select class='status-select' onchange=\"handleStatusChange('" + rec.studId + "', this.value)\">";
            tr += "<option value='pending'" + (rec.status.toLowerCase() === 'pending' ? " selected" : "") + ">Pending</option>";
            tr += "<option value='accepted'" + (rec.status.toLowerCase() === 'accepted' ? " selected" : "") + ">Accepted</option>";
            tr += "<option value='rejected'" + (rec.status.toLowerCase() === 'rejected' ? " selected" : "") + ">Rejected</option>";
            tr += "</select>";
            tr += "</td>";
            tr += "</tr>";
        }
        $("#pendingEnrollees").html(tr);
    });
}
 
function handleStatusChange(studId, status) {
    $.post("/Admin/Update_Enrollment_Status", {
        StudentId:studId,
        status: status,
    }, function (res) {
        if (res.length > 0 && res[0].mess === 0) {
            alert("Status for student " + studId + " updated to " + newStatus);
            loadPendingEnrollees(); 
        } else {
            alert("Failed to update status");
        }

    });
}
loadPendingEnrollees();
