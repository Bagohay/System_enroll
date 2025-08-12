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
    public class HomeController : Controller
    {
        string connStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\StudentEntry.mdf;Integrated Security=True";

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }

        public ActionResult Programs()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Registration_Page(FormCollection form)
        {
            var data = new List<object>();
            try
            {
                var fname = Request["firstname"];
                var mname = Request["middlename"] ?? "";
                var lname = Request["lastname"];
                var email = Request["email"];
                var phonenumber = Request["phoneNum"];
                var HomeAddress = Request["homeAddress"];
                var CityAddress = Request["cityAddress"];
                var CongressDistrict = Request["congressDistrict"] ?? "";
                var Genstudent = Request["isFirstGenStudent"];

                // Debug output
                System.Diagnostics.Debug.WriteLine($"Received form data - FirstName: {fname}, LastName: {lname}, Email: {email}");

                if (string.IsNullOrEmpty(fname) || string.IsNullOrEmpty(lname) ||
                    string.IsNullOrEmpty(email) || string.IsNullOrEmpty(phonenumber) ||
                    string.IsNullOrEmpty(HomeAddress) || string.IsNullOrEmpty(CityAddress))
                {
                    data.Add(new { mess = 0, error = "Please fill in all required fields." });
                    return Json(data, JsonRequestBehavior.AllowGet);
                }

                Random random = new Random();
                string userNumber = DateTime.Now.Year.ToString() + "-" + random.Next(10000, 99999).ToString();

                string password = GenerateRandomPassword(8);

                using (var db = new SqlConnection(connStr))
                {
                    db.Open();

                    using (var checkCmd = db.CreateCommand())
                    {
                        checkCmd.CommandType = CommandType.Text;
                        checkCmd.CommandText = "SELECT COUNT(*) FROM [USER] WHERE US_EMAIL = @email";
                        checkCmd.Parameters.AddWithValue("@email", email);

                        int existingCount = (int)checkCmd.ExecuteScalar();
                        if (existingCount > 0)
                        {
                            data.Add(new { mess = 0, error = "This email is already registered in our system." });
                            return Json(data, JsonRequestBehavior.AllowGet);
                        }
                    }

                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = @"INSERT INTO [USER] 
                    (US_NUMBER, US_FNAME, US_MNAME, US_LNAME, US_PASSWORD, US_EMAIL, US_PHONENUMBER, 
                     US_HOMEADDRESS, US_CITYADDRESS, US_CONGRESSDISTRICT, US_GEN_STUDENT, USER_ID)
                    VALUES 
                    (@usNumber, @fname, @mname, @lname, @password, @email, @phone, 
                     @homeAddress, @cityAddress, @congressDistrict, @firstGen, @userId)";

                        cmd.Parameters.AddWithValue("@usNumber", userNumber);
                        cmd.Parameters.AddWithValue("@fname", fname);
                        cmd.Parameters.AddWithValue("@mname", mname);
                        cmd.Parameters.AddWithValue("@lname", lname);
                        cmd.Parameters.AddWithValue("@password", password);
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@phone", phonenumber);
                        cmd.Parameters.AddWithValue("@homeAddress", HomeAddress);
                        cmd.Parameters.AddWithValue("@cityAddress", CityAddress);
                        cmd.Parameters.AddWithValue("@congressDistrict", CongressDistrict);
                        cmd.Parameters.AddWithValue("@firstGen", Genstudent == "true" ? true : false);
                        cmd.Parameters.AddWithValue("@userId", 1);

                        var ctr = cmd.ExecuteNonQuery();
                        if (ctr >= 1)
                        {
                            Session["StudentNumber"] = userNumber;

                            data.Add(new
                            {
                                mess = 1,
                                studentNumber = userNumber,
                                password = password
                            });
                        }
                        else
                        {
                            data.Add(new { mess = 0, error = "Database insertion failed" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception in Registration_Page: " + ex.Message + " | StackTrace: " + ex.StackTrace);
                data.Add(new { mess = 0, error = "An error occurred: " + ex.Message });
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        private string GenerateRandomPassword(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            Random random = new Random();
            StringBuilder password = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                password.Append(chars[random.Next(chars.Length)]);
            }

            return password.ToString();
        }

        [HttpGet]
        public ActionResult Registration_Page()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Login_Page()
        {
            
            return View();
        }

        [HttpPost]
        public ActionResult Login_Page(string user, string password)
        {
            try
            {
                using (var db = new SqlConnection(connStr))
                {
                    db.Open();
                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = @"SELECT U.*, UA.USER_ROLE, UA.USER_ID
                                       FROM [USER] U 
                                       INNER JOIN USER_ACCOUNT UA ON U.USER_ID = UA.USER_ID 
                                       WHERE U.US_NUMBER = @username AND U.US_PASSWORD = @password";
                        cmd.Parameters.AddWithValue("@username", user);
                        cmd.Parameters.AddWithValue("@password", password);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Store all relevant user data in session variables
                                Session["UserNumber"] = reader["US_NUMBER"].ToString();
                                Session["UserFirstName"] = reader["US_FNAME"].ToString();
                                Session["UserLastName"] = reader["US_LNAME"].ToString();

                                if (reader["US_MNAME"] != DBNull.Value)
                                    Session["UserMiddleName"] = reader["US_MNAME"].ToString();

                                Session["UserEmail"] = reader["US_EMAIL"].ToString();
                                Session["UserPhone"] = reader["US_PHONENUMBER"].ToString();
                                Session["UserHomeAddress"] = reader["US_HOMEADDRESS"].ToString();
                                Session["UserCityAddress"] = reader["US_CITYADDRESS"].ToString();

                                if (reader["US_CONGRESSDISTRICT"] != DBNull.Value)
                                    Session["UserCongressDistrict"] = reader["US_CONGRESSDISTRICT"].ToString();

                                Session["UserGenStudent"] = reader["US_GEN_STUDENT"].ToString();
                                Session["UserFullName"] = $"{reader["US_FNAME"]} {reader["US_LNAME"]}";
                                Session["UserRole"] = reader["USER_ROLE"].ToString();
                                Session["UserID"] = reader["USER_ID"].ToString();
                                Session["IsLoggedIn"] = true;

                                int userType = Convert.ToInt32(reader["USER_ID"]);

                                string redirectUrl;

                                switch (userType)
                                {
                                    case 1:
                                        redirectUrl = Url.Action("Student_Dashboard", "Student");
                                        break;
                                    case 2:
                                        redirectUrl = Url.Action("Faculty_Dashboard", "Home");
                                        break;
                                    case 3:
                                        redirectUrl = Url.Action("Admin_Dashboard", "Admin");
                                        break;
                                    default:
                                        return Json(new { success = false, error = "Invalid user type." });
                                }

                                return Json(new { success = true, redirectUrl = redirectUrl });
                            }
                            else
                            {
                                return Json(new { success = false, error = "Invalid username or password." });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception in Login_Page: " + ex.Message + " | StackTrace: " + ex.StackTrace);
                return Json(new { success = false, error = "An error occurred: " + ex.Message });
            }
        }

        public ActionResult TestDbConnection()
        {
            try
            {
                using (var db = new SqlConnection(connStr))
                {
                    db.Open();
                    return Json(new { success = true, message = "Database connection successful" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}