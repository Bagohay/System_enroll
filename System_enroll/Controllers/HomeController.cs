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
        string connStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Kent\source\repos\System_enroll\System_enroll\App_Data\StudentEntry.mdf;Integrated Security=True";

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

                int programId;
                if (!int.TryParse(Request["programId"], out programId))
                {
                    System.Diagnostics.Debug.WriteLine("Failed to parse program ID: " + Request["programId"]);
                    data.Add(new { mess = 0, error = "Invalid program selection" });
                    return Json(data, JsonRequestBehavior.AllowGet);
                }

                Random random = new Random();
                string userNumber = DateTime.Now.Year.ToString() + "-" + random.Next(10000, 99999).ToString();

                string password = GenerateRandomPassword(8);

                using (var db = new SqlConnection(connStr))
                {
                    db.Open();
                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = @"INSERT INTO [USER] 
                            (US_NUMBER, US_FNAME, US_MNAME, US_LNAME, US_PASSWORD, US_EMAIL, US_PHONENUMBER, 
                             US_HOMEADDRESS, US_CITYADDRESS, US_CONGRESSDISTRICT, US_GEN_STUDENT, US_PROGRAM_ID, USER_ID)
                            VALUES 
                            (@usNumber, @fname, @mname, @lname, @password, @email, @phone, 
                             @homeAddress, @cityAddress, @congressDistrict, @firstGen, @programId, @userId)";

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
                        cmd.Parameters.AddWithValue("@programId", programId);
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
                System.Diagnostics.Debug.WriteLine("Exception in Registration_Page: " + ex.Message);
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
            List<SelectListItem> programs = new List<SelectListItem>();

            using (var db = new SqlConnection(connStr))
            {
                db.Open();
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "SELECT ID, NAME FROM PROGRAMS";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            programs.Add(new SelectListItem
                            {
                                Value = reader["ID"].ToString(),
                                Text = reader["NAME"].ToString()
                            });
                        }
                    }
                }
            }

            ViewBag.Programs = programs;
            return View();
        }

        public ActionResult Enrollment_Form()
        {
            if (Session["StudentNumber"] == null)
            {
                return RedirectToAction("Login_Page");
            }

            List<SelectListItem> programs = new List<SelectListItem>();
            List<SelectListItem> blockTypes = new List<SelectListItem>();

            using (var db = new SqlConnection(connStr))
            {
                db.Open();
                // Fetch programs
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "SELECT ID, NAME FROM PROGRAMS";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            programs.Add(new SelectListItem
                            {
                                Value = reader["ID"].ToString(),
                                Text = reader["NAME"].ToString()
                            });
                        }
                    }
                }

               
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "SELECT BLCK_Id, BLCK_TYPE FROM BLOCK_TYPE";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            blockTypes.Add(new SelectListItem
                            {
                                Value = reader["BLCK_Id"].ToString(),
                                Text = reader["BLCK_TYPE"].ToString()
                            });
                        }
                    }
                }
            }

            ViewBag.Programs = programs;
            ViewBag.BlockTypes = blockTypes;
            ViewBag.StudentNumber = Session["StudentNumber"];
            return View();
        }

        [HttpPost]
        public ActionResult Submit_Enrollment(FormCollection form)
        {
            var data = new List<object>();
            try
            {
                var studentNumber = Session["StudentNumber"]?.ToString();
                if (string.IsNullOrEmpty(studentNumber))
                {
                    studentNumber = Request["studentNumber"];
                    if (string.IsNullOrEmpty(studentNumber))
                    {
                        data.Add(new { mess = 0, error = "Student number not found. Please log in." });
                        return Json(data, JsonRequestBehavior.AllowGet);
                    }
                }

                int programId;
                if (!int.TryParse(Request["programId"], out programId))
                {
                    System.Diagnostics.Debug.WriteLine("Failed to parse program ID: " + Request["programId"]);
                    data.Add(new { mess = 0, error = "Invalid program selection" });
                    return Json(data, JsonRequestBehavior.AllowGet);
                }

                int blockId;
                if (!int.TryParse(Request["blockTypeId"], out blockId))
                {
                    System.Diagnostics.Debug.WriteLine("Failed to parse block type ID: " + Request["blockTypeId"]);
                    data.Add(new { mess = 0, error = "Invalid block type selection" });
                    return Json(data, JsonRequestBehavior.AllowGet);
                }

                var schoolYear = Request["schoolYear"];
                var semester = Request["semester"];
                var yearLevel = Request["yearLevel"];
                var studentStatus = Request["studentStatusId"];
                var schedule = Request["schedule"]; // DAY or NIGHT for Block students

                if (string.IsNullOrEmpty(schoolYear) || string.IsNullOrEmpty(semester) ||
                    string.IsNullOrEmpty(yearLevel) || string.IsNullOrEmpty(studentStatus))
                {
                    data.Add(new { mess = 0, error = "All required fields must be completed." });
                    return Json(data, JsonRequestBehavior.AllowGet);
                }

                // Validate schedule for Block students
                if (blockId == 1 && string.IsNullOrEmpty(schedule)) // Block requires schedule
                {
                    data.Add(new { mess = 0, error = "Schedule (DAY or NIGHT) is required for Block enrollment." });
                    return Json(data, JsonRequestBehavior.AllowGet);
                }

                // Validate schedule value
                if (blockId == 1 && schedule != "DAY" && schedule != "NIGHT")
                {
                    data.Add(new { mess = 0, error = "Invalid schedule selection. Choose DAY or NIGHT." });
                    return Json(data, JsonRequestBehavior.AllowGet);
                }

                // Non-Block should not have a schedule
                if (blockId == 2)
                {
                    schedule = null;
                }

                string semesterValue;
                switch (semester)
                {
                    case "1":
                        semesterValue = "1st";
                        break;
                    case "2":
                        semesterValue = "2nd";
                        break;
                    case "S":
                        semesterValue = "Summer";
                        break;
                    default:
                        semesterValue = semester;
                        break;
                }

                string yearLevelValue;
                switch (yearLevel)
                {
                    case "1":
                        yearLevelValue = "1st";
                        break;
                    case "2":
                        yearLevelValue = "2nd";
                        break;
                    case "3":
                        yearLevelValue = "3rd";
                        break;
                    case "4":
                        yearLevelValue = "4th";
                        break;
                    default:
                        yearLevelValue = yearLevel;
                        break;
                }

                string studentStatusValue;
                switch (studentStatus)
                {
                    case "1":
                        studentStatusValue = "New Student";
                        break;
                    case "2":
                        studentStatusValue = "Continuing";
                        break;
                    case "3":
                        studentStatusValue = "Returnee";
                        break;
                    case "4":
                        studentStatusValue = "Shiftee";
                        break;
                    case "5":
                        studentStatusValue = "Cross Enrollee";
                        break;
                    default:
                        studentStatusValue = studentStatus;
                        break;
                }

                decimal totalUnits = 18.0m;

                using (var db = new SqlConnection(connStr))
                {
                    db.Open();
                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = @"INSERT INTO STUDENT_ENROLLMENT 
                                         (STU_ENR_STU_NUMBER, STU_ENR_PROGRAM_ID, STU_ENR_SCHOOLYEAR, 
                                          STU_ENR_SEMESTER, STU_ENR_YEARLEVEL, STU_ENR_STATUS, 
                                          STU_BLCK_ID, STU_ES_ID, STU_ENR_TOTALUNITS, STU_ENR_SCHEDULE)
                                         VALUES 
                                         (@usNumber, @programId, @schoolYear, 
                                          @semester, @yearLevel, @status, 
                                          @blockId, @esId, @totalUnits, @schedule)";

                        cmd.Parameters.AddWithValue("@usNumber", studentNumber);
                        cmd.Parameters.AddWithValue("@programId", programId);
                        cmd.Parameters.AddWithValue("@schoolYear", schoolYear);
                        cmd.Parameters.AddWithValue("@semester", semesterValue);
                        cmd.Parameters.AddWithValue("@yearLevel", yearLevelValue);
                        cmd.Parameters.AddWithValue("@status", studentStatusValue);
                        cmd.Parameters.AddWithValue("@blockId", blockId);
                        cmd.Parameters.AddWithValue("@esId", 1); // Default ENROLLMENT_STATUS
                        cmd.Parameters.AddWithValue("@totalUnits", totalUnits);
                        cmd.Parameters.AddWithValue("@schedule", (object)schedule ?? DBNull.Value);

                        var result = cmd.ExecuteNonQuery();
                        if (result >= 1)
                        {
                            string programName = "";
                            string blockType = "";
                            using (var cmdProgram = db.CreateCommand())
                            {
                                cmdProgram.CommandType = CommandType.Text;
                                cmdProgram.CommandText = "SELECT NAME FROM PROGRAMS WHERE ID = @programId";
                                cmdProgram.Parameters.AddWithValue("@programId", programId);
                                var programResult = cmdProgram.ExecuteScalar();
                                if (programResult != null)
                                {
                                    programName = programResult.ToString();
                                }
                            }

                            using (var cmdBlock = db.CreateCommand())
                            {
                                cmdBlock.CommandType = CommandType.Text;
                                cmdBlock.CommandText = "SELECT BLCK_TYPE FROM BLOCK_TYPE WHERE BLCK_Id = @blockId";
                                cmdBlock.Parameters.AddWithValue("@blockId", blockId);
                                var blockResult = cmdBlock.ExecuteScalar();
                                if (blockResult != null)
                                {
                                    blockType = blockResult.ToString();
                                }
                            }

                            
                            string scheduleDisplay = blockId == 1
                                ? (schedule == "DAY" ? "7 AM - 4 PM" : "4 PM - 9 PM")
                                : "Irregular (Choose Own Schedule)";

                            data.Add(new
                            {
                                mess = 1,
                                studentNumber = studentNumber,
                                program = programName,
                                schoolYear = schoolYear,
                                semester = semesterValue,
                                yearLevel = yearLevelValue,
                                blockType = blockType,
                                schedule = scheduleDisplay,
                                units = totalUnits
                            });
                        }
                        else
                        {
                            data.Add(new { mess = 0, error = "Failed to save enrollment data. Please try again." });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception in Submit_Enrollment: " + ex.Message);
                data.Add(new { mess = 0, error = "An error occurred: " + ex.Message });
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Login_Page()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login_Page(string user, string password)
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
                            Session["UserNumber"] = reader["US_NUMBER"].ToString();
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

        public ActionResult Faculty_Dashboard()
        {
            if (Session["StudentNumber"] == null)
            {
                return RedirectToAction("Login_Page");
            }
            return View();
        }
    }
}