$(document).ready(function () {
    // Open Edit Profile modal
    window.openEditModal = function () {
        $("#editProfileModal").addClass("show").show();
    };

    // Close Edit Profile modal
    window.closeEditModal = function () {
        $("#editProfileModal").removeClass("show").hide();
        $("#editProfileForm")[0].reset();
    };

    // Close modal when clicking outside
    $("#editProfileModal").click(function (e) {
        if ($(e.target).hasClass('modal')) {
            $(this).removeClass("show").hide();
            $("#editProfileForm")[0].reset();
        }
    });

    // Submit Edit Profile
    $("#editProfileForm").submit(function (e) {
        e.preventDefault();

        var formData = {
            firstName: $("#firstName").val(),
            middleName: $("#middleName").val(),
            lastName: $("#lastName").val(),
            email: $("#email").val(),
            phone: $("#phone").val(),
            homeAddress: $("#homeAddress").val(),
            cityAddress: $("#cityAddress").val(),
            congressDistrict: $("#congressDistrict").val(),
            firstGenStudent: $("#firstGenStudent").val()
        };

        // Validate inputs
        if (!formData.firstName || !formData.lastName || !formData.email ||
            !formData.phone || !formData.homeAddress || !formData.cityAddress) {
            alert("Please fill in all required fields.");
            return;
        }

        $.post("/Student/UpdateProfile", formData, function (res) {
            if (res[0].mess === 0) {
                alert("Profile successfully updated");
                location.reload();
            } else {
                alert("Error updating profile: " + res[0].error);
            }
        }).fail(function (xhr, status, error) {
            console.error("Error updating profile: ", error);
            alert("Failed to update profile: " + error);
        });
    });
});