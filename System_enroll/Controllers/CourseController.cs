using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace System_enroll.Controllers
{
    public class CourseController : Controller
    {
        string connStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Kent\source\repos\System_enroll\System_enroll\App_Data\StudentEntry.mdf;Integrated Security=True";

        public ActionResult Display_Dispartment()
        {
            if (Session["UserNumber"] == null)
            {
                return RedirectToAction("Login_Page", "Home");
            }
            return View();
        }

        public ActionResult Get_Departments()
        {
            var data = new List<object>();
            using (var db = new SqlConnection(connStr))
            {
                try
                {
                    db.Open();
                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "SELECT DEPT_ID, DEPT_NAME FROM [dbo].[DEPARTMENT] ORDER BY DEPT_ID";
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                data.Add(new
                                {
                                    deptId = reader["DEPT_ID"].ToString(),
                                    deptName = reader["DEPT_NAME"].ToString()
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in Get_Departments: {ex.Message}");
                    return Json(new { error = "Error fetching departments." }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Add_Department()
        {
            string deptName = Request["deptName"];

            try
            {
                using (var db = new SqlConnection(connStr))
                {
                    db.Open();
                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "INSERT INTO [dbo].[DEPARTMENT] (DEPT_NAME) VALUES (@deptName)";
                        cmd.Parameters.AddWithValue("@deptName", deptName);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return Json(new { success = true, message = "Department added successfully." }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new { success = false, message = "Failed to add department." }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in Add_Department: {ex.Message}");
                return Json(new { success = false, message = "Error adding department." }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult View_Programs(int? deptId)
        {
            if (!deptId.HasValue)
            {
                TempData["ErrorMessage"] = "Department ID is required to view programs.";
                return RedirectToAction("Display_Dispartment");
            }

            Debug.WriteLine($"View_Programs called with deptId: {deptId}");
            ViewBag.DeptId = deptId.Value;
            return View();
        }

        public ActionResult Get_Programs()
        {
            int deptId;
            if (!int.TryParse(Request["deptId"], out deptId))
            {
                return Json(new { error = "Invalid department ID" }, JsonRequestBehavior.AllowGet);
            }

            var programs = new List<object>();
            using (var db = new SqlConnection(connStr))
            {
                try
                {
                    db.Open();
                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = @"
                            SELECT p.ID, p.NAME, p.CODE, p.DEGREE_TYPE, p.DURATION, d.DEPT_NAME 
                            FROM [dbo].[PROGRAMS] p 
                            JOIN [dbo].[DEPARTMENT] d ON p.DEPT_ID = d.DEPT_ID 
                            WHERE p.DEPT_ID = @deptId";
                        cmd.Parameters.AddWithValue("@deptId", deptId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                programs.Add(new
                                {
                                    Id = reader["ID"].ToString(),
                                    Name = reader["NAME"].ToString(),
                                    Code = reader["CODE"]?.ToString()?.Trim() ?? reader["NAME"].ToString().Substring(0, Math.Min(4, reader["NAME"].ToString().Length)).ToUpper(),
                                    DegreeType = reader["DEGREE_TYPE"]?.ToString() ?? "Bachelors",
                                    Duration = reader["DURATION"]?.ToString() ?? "4",
                                    DeptName = reader["DEPT_NAME"].ToString()
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in Get_Programs: {ex.Message}");
                    return Json(new { error = $"Error fetching programs: {ex.Message}" }, JsonRequestBehavior.AllowGet);
                }
            }

            Debug.WriteLine($"Programs found: {programs.Count}");
            return Json(programs, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Add_Program()
        {
            int deptId;
            if (!int.TryParse(Request["deptId"], out deptId))
            {
                return Json(new { success = false, message = "Invalid department ID" }, JsonRequestBehavior.AllowGet);
            }

            string name = Request["name"];
            string code = Request["code"];
            string degreeType = Request["degreeType"];
            string duration = Request["duration"];

            int durationInt;
            if (!int.TryParse(duration, out durationInt))
            {
                durationInt = 4; // Default duration if parsing fails
            }

            try
            {
                using (var db = new SqlConnection(connStr))
                {
                    db.Open();
                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = @"
                            INSERT INTO [dbo].[PROGRAMS] (NAME, CODE, DEGREE_TYPE, DURATION, DEPT_ID)
                            VALUES (@name, @code, @degreeType, @duration, @deptId)";
                        cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@code", code);
                        cmd.Parameters.AddWithValue("@degreeType", degreeType);
                        cmd.Parameters.AddWithValue("@duration", durationInt);
                        cmd.Parameters.AddWithValue("@deptId", deptId);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return Json(new { success = true, message = "Program added successfully." }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new { success = false, message = "Failed to add program." }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in Add_Program: {ex.Message}");
                return Json(new { success = false, message = $"Error adding program: {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult View_Rooms(int? deptId)
        {
            if (!deptId.HasValue)
            {
                TempData["ErrorMessage"] = "Department ID is required to view rooms.";
                return RedirectToAction("Display_Dispartment");
            }

            ViewBag.DeptId = deptId.Value;
            return View();
        }
    }
}