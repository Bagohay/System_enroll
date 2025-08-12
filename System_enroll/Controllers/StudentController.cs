using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace System_enroll.Controllers
{
    public class StudentController : Controller
    {
        string connStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Kent\source\repos\System_enroll\System_enroll\App_Data\StudentEntry.mdf;Integrated Security=True";

        public ActionResult Student_Dashboard()
        {
            if (Session["UserNumber"] == null)
            {
                return RedirectToAction("Login_Page", "Home");
            }
            return View();
        }

        public ActionResult Profile_Page()
        {
            // Ensure critical session variables are set
            if (Session["UserNumber"] == null || Session["UserGenStudent"] == null)
            {
                return RedirectToAction("Login_Page", "Home");
            }
            return View();
        }

        public ActionResult UpdateProfile()
        {
            var data = new List<object>();
            try
            {
                if (Session["UserNumber"] == null)
                {
                    data.Add(new { mess = 1, error = "User not logged in." });
                    return Json(data, JsonRequestBehavior.AllowGet);
                }

                string userNumber = Session["UserNumber"].ToString();
                var firstName = Request["firstName"];
                var middleName = Request["middleName"] ?? "";
                var lastName = Request["lastName"];
                var email = Request["email"];
                var phone = Request["phone"];
                var homeAddress = Request["homeAddress"];
                var cityAddress = Request["cityAddress"];
                var congressDistrict = Request["congressDistrict"] ?? "";
                var firstGenStudent = Request["firstGenStudent"] == "true";

                if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) ||
                    string.IsNullOrEmpty(email) || string.IsNullOrEmpty(phone) ||
                    string.IsNullOrEmpty(homeAddress) || string.IsNullOrEmpty(cityAddress))
                {
                    data.Add(new { mess = 1, error = "Please fill in all required fields." });
                    return Json(data, JsonRequestBehavior.AllowGet);
                }

                using (var db = new SqlConnection(connStr))
                {
                    db.Open();

                    // Check if email is already used by another user
                    using (var checkCmd = db.CreateCommand())
                    {
                        checkCmd.CommandType = CommandType.Text;
                        checkCmd.CommandText = "SELECT COUNT(*) FROM [USER] WHERE US_EMAIL = @email AND US_NUMBER != @userNumber";
                        checkCmd.Parameters.AddWithValue("@email", email);
                        checkCmd.Parameters.AddWithValue("@userNumber", userNumber);

                        int existingCount = (int)checkCmd.ExecuteScalar();
                        if (existingCount > 0)
                        {
                            data.Add(new { mess = 1, error = "This email is already registered." });
                            return Json(data, JsonRequestBehavior.AllowGet);
                        }
                    }

                    // Update user profile
                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = @"UPDATE [USER] 
                        SET US_FNAME = @firstName, 
                            US_MNAME = @middleName, 
                            US_LNAME = @lastName, 
                            US_EMAIL = @email, 
                            US_PHONENUMBER = @phone, 
                            US_HOMEADDRESS = @homeAddress, 
                            US_CITYADDRESS = @cityAddress, 
                            US_CONGRESSDISTRICT = @congressDistrict, 
                            US_GEN_STUDENT = @firstGenStudent
                        WHERE US_NUMBER = @userNumber";

                        cmd.Parameters.AddWithValue("@firstName", firstName);
                        cmd.Parameters.AddWithValue("@middleName", middleName);
                        cmd.Parameters.AddWithValue("@lastName", lastName);
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@phone", phone);
                        cmd.Parameters.AddWithValue("@homeAddress", homeAddress);
                        cmd.Parameters.AddWithValue("@cityAddress", cityAddress);
                        cmd.Parameters.AddWithValue("@congressDistrict", congressDistrict);
                        cmd.Parameters.AddWithValue("@firstGenStudent", firstGenStudent);
                        cmd.Parameters.AddWithValue("@userNumber", userNumber);

                        var rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected >= 1)
                        {
                            // Update session variables
                            Session["UserFirstName"] = firstName;
                            Session["UserMiddleName"] = middleName;
                            Session["UserLastName"] = lastName;
                            Session["UserEmail"] = email;
                            Session["UserPhone"] = phone;
                            Session["UserHomeAddress"] = homeAddress;
                            Session["UserCityAddress"] = cityAddress;
                            Session["UserCongressDistrict"] = congressDistrict;
                            Session["UserGenStudent"] = firstGenStudent.ToString();
                            Session["UserFullName"] = $"{firstName} {lastName}";

                            data.Add(new { mess = 0 });
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"Update failed for user: {userNumber}. Rows affected: {rowsAffected}");
                            data.Add(new { mess = 1, error = "Failed to update profile." });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in UpdateProfile: {ex.Message} | StackTrace: {ex.StackTrace}");
                data.Add(new { mess = 1, error = $"An error occurred: {ex.Message}" });
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }

}