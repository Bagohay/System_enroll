using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

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
            try
            {
                using (var db = new SqlConnection(connStr))
                {
                    db.Open();
                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "SELECT DEPT_ID, DEPT_NAME FROM DEPARTMENT ORDER BY DEPT_ID";
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
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Get_Departments: {ex.Message}");
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Add_Department()
        {
            string deptName = Request["deptName"];
            if (string.IsNullOrEmpty(deptName))
            {
                return Json(new { success = false, message = "Department name is required." }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                using (var db = new SqlConnection(connStr))
                {
                    db.Open();
                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "SELECT COUNT(*) FROM DEPARTMENT WHERE DEPT_NAME = @deptName";
                        cmd.Parameters.AddWithValue("@deptName", deptName);
                        int count = (int)cmd.ExecuteScalar();
                        if (count > 0)
                        {
                            return Json(new { success = false, message = "Department already exists." }, JsonRequestBehavior.AllowGet);
                        }

                        cmd.CommandText = "INSERT INTO DEPARTMENT (DEPT_NAME) VALUES (@deptName)";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@deptName", deptName);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return Json(new
                        {
                            success = rowsAffected > 0,
                            message = rowsAffected > 0 ? "Department added successfully." : "Failed to add department."
                        }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Add_Department: {ex.Message}");
                return Json(new { success = false, message = $"Error: {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Update_Department()
        {
            string deptId = Request["deptId"];
            string deptName = Request["deptName"];

            if (string.IsNullOrEmpty(deptId) || string.IsNullOrEmpty(deptName))
            {
                return Json(new { success = false, message = "Department ID and name are required." }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                using (var db = new SqlConnection(connStr))
                {
                    db.Open();
                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "SELECT COUNT(*) FROM DEPARTMENT WHERE DEPT_NAME = @deptName AND DEPT_ID != @deptId";
                        cmd.Parameters.AddWithValue("@deptName", deptName);
                        cmd.Parameters.AddWithValue("@deptId", int.Parse(deptId));
                        int count = (int)cmd.ExecuteScalar();
                        if (count > 0)
                        {
                            return Json(new { success = false, message = "Department name already exists." }, JsonRequestBehavior.AllowGet);
                        }

                        cmd.CommandText = "UPDATE DEPARTMENT SET DEPT_NAME = @deptName WHERE DEPT_ID = @deptId";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@deptName", deptName);
                        cmd.Parameters.AddWithValue("@deptId", int.Parse(deptId));
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return Json(new
                        {
                            success = rowsAffected > 0,
                            message = rowsAffected > 0 ? "Department updated successfully." : "Failed to update department."
                        }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Update_Department: {ex.Message}");
                return Json(new { success = false, message = $"Error: {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Delete_Department()
        {
            string deptId = Request["deptId"];

            if (string.IsNullOrEmpty(deptId))
            {
                return Json(new { success = false, message = "Department ID is required." }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                using (var db = new SqlConnection(connStr))
                {
                    db.Open();
                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "DELETE FROM DEPARTMENT WHERE DEPT_ID = @deptId";
                        cmd.Parameters.AddWithValue("@deptId", int.Parse(deptId));
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return Json(new
                        {
                            success = rowsAffected > 0,
                            message = rowsAffected > 0 ? "Department deleted successfully." : "Failed to delete department."
                        }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Delete_Department: {ex.Message}");
                return Json(new { success = false, message = $"Error: {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult View_Programs(int deptId)
        {
            ViewBag.DeptId = deptId;
            return View();
        }

        public ActionResult Get_Programs()
        {
            int deptId = int.Parse(Request["deptId"]);
            var programs = new List<object>();

            using (var db = new SqlConnection(connStr))
            {
                db.Open();
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"
                        SELECT p.ID, p.NAME, p.CODE, p.DEGREE_TYPE, p.DURATION, d.DEPT_NAME 
                        FROM PROGRAMS p 
                        JOIN DEPARTMENT d ON p.DEPT_ID = d.DEPT_ID 
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

            return Json(programs, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Add_Program()
        {
            int deptId = int.Parse(Request["deptId"]);
            string name = Request["name"];
            string code = Request["code"];
            string degreeType = Request["degreeType"];
            string duration = Request["duration"];

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(code) || string.IsNullOrEmpty(degreeType))
            {
                return Json(new { success = false, message = "Program name, code, and degree type are required." }, JsonRequestBehavior.AllowGet);
            }

            int durationInt = int.TryParse(duration, out int result) ? result : 4;

            using (var db = new SqlConnection(connStr))
            {
                db.Open();
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "SELECT COUNT(*) FROM PROGRAMS WHERE CODE = @code";
                    cmd.Parameters.AddWithValue("@code", code);
                    int count = (int)cmd.ExecuteScalar();
                    if (count > 0)
                    {
                        return Json(new { success = false, message = "Program code already exists." }, JsonRequestBehavior.AllowGet);
                    }

                    cmd.CommandText = @"
                        INSERT INTO PROGRAMS (NAME, CODE, DEGREE_TYPE, DURATION, DEPT_ID)
                        VALUES (@name, @code, @degreeType, @duration, @deptId)";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@code", code);
                    cmd.Parameters.AddWithValue("@degreeType", degreeType);
                    cmd.Parameters.AddWithValue("@duration", durationInt);
                    cmd.Parameters.AddWithValue("@deptId", deptId);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    return Json(new
                    {
                        success = rowsAffected > 0,
                        message = rowsAffected > 0 ? "Program added successfully." : "Failed to add program."
                    }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult Add_Program_Course()
        {
            int programId = int.Parse(Request["programId"]);
            int courseId = int.Parse(Request["courseId"]);
            int semester = int.Parse(Request["semester"]);
            int yearLevel = int.Parse(Request["yearLevel"]);
            bool isRequired = Request["isRequired"] == "true";
            bool isElective = Request["isElective"] == "true";

            if (semester < 1 || semester > 2 || yearLevel < 1 || yearLevel > 5)
            {
                return Json(new { success = false, message = "Semester must be 1 or 2. Year level must be between 1 and 5." }, JsonRequestBehavior.AllowGet);
            }

            using (var db = new SqlConnection(connStr))
            {
                db.Open();
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"
                        INSERT INTO PROGRAMS_COURSE (PROGRAM_ID, COURSE_ID, SEMESTER, YEAR_LEVEL, IS_REQUIRED, IS_ELECTIVE)
                        VALUES (@programId, @courseId, @semester, @yearLevel, @isRequired, @isElective)";
                    cmd.Parameters.AddWithValue("@programId", programId);
                    cmd.Parameters.AddWithValue("@courseId", courseId);
                    cmd.Parameters.AddWithValue("@semester", semester);
                    cmd.Parameters.AddWithValue("@yearLevel", yearLevel);
                    cmd.Parameters.AddWithValue("@isRequired", isRequired);
                    cmd.Parameters.AddWithValue("@isElective", isElective);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    return Json(new
                    {
                        success = rowsAffected > 0,
                        message = rowsAffected > 0 ? "Course added to program successfully." : "Failed to add course to program."
                    }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult Display_Courses()
        {
            if (Session["UserNumber"] == null)
            {
                return RedirectToAction("Login_Page", "Home");
            }
            return View();
        }

        public ActionResult Get_Courses()
        {
            var courses = new List<object>();
            using (var db = new SqlConnection(connStr))
            {
                db.Open();
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"
                        SELECT ID, COURSE_CODE, COURSE_NAME, UNITS, YEAR_LEVEL, DESCRIPTION, LEC_HOURS, LAB_HOURS, TOTAL_HOURS
                        FROM COURSES";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            courses.Add(new
                            {
                                id = reader["ID"].ToString(),
                                courseCode = reader["COURSE_CODE"].ToString(),
                                courseName = reader["COURSE_NAME"].ToString(),
                                units = reader["UNITS"].ToString(),
                                yearLevel = reader["YEAR_LEVEL"] != DBNull.Value ? reader["YEAR_LEVEL"].ToString() : null,
                                description = reader["DESCRIPTION"] != DBNull.Value ? reader["DESCRIPTION"].ToString() : null,
                                lecHours = reader["LEC_HOURS"] != DBNull.Value ? reader["LEC_HOURS"].ToString() : null,
                                labHours = reader["LAB_HOURS"] != DBNull.Value ? reader["LAB_HOURS"].ToString() : null,
                                totalHours = reader["TOTAL_HOURS"] != DBNull.Value ? reader["TOTAL_HOURS"].ToString() : null
                            });
                        }
                    }
                }
            }
            return Json(courses, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Add_Course()
        {
            string courseCode = Request["courseCode"];
            string courseName = Request["courseName"];
            string units = Request["units"];
            string yearLevel = Request["yearLevel"];
            string description = Request["description"];
            string lecHours = Request["lecHours"];
            string labHours = Request["labHours"];
            string totalHours = Request["totalHours"];

            if (string.IsNullOrEmpty(courseCode) || string.IsNullOrEmpty(courseName) || string.IsNullOrEmpty(units))
            {
                return Json(new { success = false, message = "Course code, name, and units are required." }, JsonRequestBehavior.AllowGet);
            }

            decimal unitsDecimal;
            if (!decimal.TryParse(units, out unitsDecimal))
            {
                return Json(new { success = false, message = "Units must be a valid number." }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                using (var db = new SqlConnection(connStr))
                {
                    db.Open();
                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "SELECT COUNT(*) FROM COURSES WHERE COURSE_CODE = @courseCode";
                        cmd.Parameters.AddWithValue("@courseCode", courseCode);
                        int count = (int)cmd.ExecuteScalar();
                        if (count > 0)
                        {
                            return Json(new { success = false, message = "Course code already exists." }, JsonRequestBehavior.AllowGet);
                        }

                        cmd.CommandText = @"
                            INSERT INTO COURSES (COURSE_CODE, COURSE_NAME, UNITS, YEAR_LEVEL, DESCRIPTION, LEC_HOURS, LAB_HOURS, TOTAL_HOURS)
                            VALUES (@courseCode, @courseName, @units, @yearLevel, @description, @lecHours, @labHours, @totalHours)";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@courseCode", courseCode);
                        cmd.Parameters.AddWithValue("@courseName", courseName);
                        cmd.Parameters.AddWithValue("@units", unitsDecimal);
                        cmd.Parameters.AddWithValue("@yearLevel", string.IsNullOrEmpty(yearLevel) ? (object)DBNull.Value : yearLevel);
                        cmd.Parameters.AddWithValue("@description", string.IsNullOrEmpty(description) ? (object)DBNull.Value : description);
                        cmd.Parameters.AddWithValue("@lecHours", string.IsNullOrEmpty(lecHours) ? (object)DBNull.Value : lecHours);
                        cmd.Parameters.AddWithValue("@labHours", string.IsNullOrEmpty(labHours) ? (object)DBNull.Value : labHours);
                        cmd.Parameters.AddWithValue("@totalHours", string.IsNullOrEmpty(totalHours) ? (object)DBNull.Value : totalHours);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return Json(new
                        {
                            success = rowsAffected > 0,
                            message = rowsAffected > 0 ? "Course added successfully." : "Failed to add course."
                        }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Add_Course: {ex.Message}");
                return Json(new { success = false, message = $"Error: {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Update_Course()
        {
            string courseId = Request["courseId"];
            string courseCode = Request["courseCode"];
            string courseName = Request["courseName"];
            string units = Request["units"];
            string yearLevel = Request["yearLevel"];
            string description = Request["description"];
            string lecHours = Request["lecHours"];
            string labHours = Request["labHours"];
            string totalHours = Request["totalHours"];

            if (string.IsNullOrEmpty(courseId) || string.IsNullOrEmpty(courseCode) || string.IsNullOrEmpty(courseName) || string.IsNullOrEmpty(units))
            {
                return Json(new { success = false, message = "Course ID, code, name, and units are required." }, JsonRequestBehavior.AllowGet);
            }

            decimal unitsDecimal;
            if (!decimal.TryParse(units, out unitsDecimal))
            {
                return Json(new { success = false, message = "Units must be a valid number." }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                using (var db = new SqlConnection(connStr))
                {
                    db.Open();
                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "SELECT COUNT(*) FROM COURSES WHERE COURSE_CODE = @courseCode AND ID != @courseId";
                        cmd.Parameters.AddWithValue("@courseCode", courseCode);
                        cmd.Parameters.AddWithValue("@courseId", int.Parse(courseId));
                        int count = (int)cmd.ExecuteScalar();
                        if (count > 0)
                        {
                            return Json(new { success = false, message = "Course code already exists." }, JsonRequestBehavior.AllowGet);
                        }

                        cmd.CommandText = @"
                            UPDATE COURSES 
                            SET COURSE_CODE = @courseCode, 
                                COURSE_NAME = @courseName, 
                                UNITS = @units, 
                                YEAR_LEVEL = @yearLevel, 
                                DESCRIPTION = @description, 
                                LEC_HOURS = @lecHours, 
                                LAB_HOURS = @labHours, 
                                TOTAL_HOURS = @totalHours 
                            WHERE ID = @courseId";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@courseId", int.Parse(courseId));
                        cmd.Parameters.AddWithValue("@courseCode", courseCode);
                        cmd.Parameters.AddWithValue("@courseName", courseName);
                        cmd.Parameters.AddWithValue("@units", unitsDecimal);
                        cmd.Parameters.AddWithValue("@yearLevel", string.IsNullOrEmpty(yearLevel) ? (object)DBNull.Value : yearLevel);
                        cmd.Parameters.AddWithValue("@description", string.IsNullOrEmpty(description) ? (object)DBNull.Value : description);
                        cmd.Parameters.AddWithValue("@lecHours", string.IsNullOrEmpty(lecHours) ? (object)DBNull.Value : lecHours);
                        cmd.Parameters.AddWithValue("@labHours", string.IsNullOrEmpty(labHours) ? (object)DBNull.Value : labHours);
                        cmd.Parameters.AddWithValue("@totalHours", string.IsNullOrEmpty(totalHours) ? (object)DBNull.Value : totalHours);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return Json(new
                        {
                            success = rowsAffected > 0,
                            message = rowsAffected > 0 ? "Course updated successfully." : "Failed to update course."
                        }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Update_Course: {ex.Message}");
                return Json(new { success = false, message = $"Error: {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Delete_Course()
        {
            string courseId = Request["courseId"];

            if (string.IsNullOrEmpty(courseId))
            {
                return Json(new { success = false, message = "Course ID is required." }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                using (var db = new SqlConnection(connStr))
                {
                    db.Open();
                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "DELETE FROM COURSES WHERE ID = @courseId";
                        cmd.Parameters.AddWithValue("@courseId", int.Parse(courseId));
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return Json(new
                        {
                            success = rowsAffected > 0,
                            message = rowsAffected > 0 ? "Course deleted successfully." : "Failed to delete course."
                        }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Delete_Course: {ex.Message}");
                return Json(new { success = false, message = $"Error: {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Get_Courses_Except()
        {
            int courseId;
            if (!int.TryParse(Request["courseId"], out courseId))
            {
                return Json(new { success = false, message = "Invalid course ID." }, JsonRequestBehavior.AllowGet);
            }

            var courses = new List<object>();
            using (var db = new SqlConnection(connStr))
            {
                db.Open();
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"
                        SELECT ID, COURSE_CODE, COURSE_NAME
                        FROM COURSES
                        WHERE ID != @courseId";
                    cmd.Parameters.AddWithValue("@courseId", courseId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            courses.Add(new
                            {
                                id = reader["ID"].ToString(),
                                courseCode = reader["COURSE_CODE"].ToString(),
                                courseName = reader["COURSE_NAME"].ToString()
                            });
                        }
                    }
                }
            }
            return Json(courses, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Save_Prerequisites()
        {
            int courseId;
            if (!int.TryParse(Request["courseId"], out courseId))
            {
                return Json(new { success = false, message = "Invalid course ID." }, JsonRequestBehavior.AllowGet);
            }

            string prerequisiteIds = Request["prerequisiteIds"];
            var prereqIds = string.IsNullOrEmpty(prerequisiteIds)
                ? new List<int>()
                : prerequisiteIds.Split(',')
                    .Select(id => int.TryParse(id, out int result) ? result : -1)
                    .Where(id => id != -1)
                    .ToList();

            using (var db = new SqlConnection(connStr))
            {
                db.Open();
                using (var transaction = db.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = db.CreateCommand())
                        {
                            cmd.Transaction = transaction;
                            cmd.CommandType = CommandType.Text;

                            var existingPrereqs = new List<int>();
                            cmd.CommandText = "SELECT PREREQUISITE_ID FROM COURSE_PREREQUISITES WHERE COURSE_ID = @courseId";
                            cmd.Parameters.AddWithValue("@courseId", courseId);
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    existingPrereqs.Add(reader.GetInt32(0));
                                }
                            }

                            foreach (var prereqId in prereqIds)
                            {
                                if (!existingPrereqs.Contains(prereqId))
                                {
                                    cmd.Parameters.Clear();
                                    cmd.CommandText = @"
                                        INSERT INTO COURSE_PREREQUISITES (COURSE_ID, PREREQUISITE_ID)
                                        VALUES (@courseId, @prereqId)";
                                    cmd.Parameters.AddWithValue("@courseId", courseId);
                                    cmd.Parameters.AddWithValue("@prereqId", prereqId);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                        transaction.Commit();
                        return Json(new { success = true, message = "Prerequisites saved successfully." }, JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        System.Diagnostics.Debug.WriteLine($"Error in Save_Prerequisites: {ex.Message}");
                        return Json(new { success = false, message = $"Error saving prerequisites: {ex.Message}" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
        }

        public ActionResult View_Courses(int programId, int deptId)
        {
            ViewBag.ProgramId = programId;
            ViewBag.DeptId = deptId;
            return View();
        }

        public ActionResult Get_Program_Courses()
        {
            int programId = int.Parse(Request["programId"]);
            var courses = new List<object>();

            using (var db = new SqlConnection(connStr))
            {
                db.Open();
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"
                        SELECT 
                            pc.PC_ID,
                            c.ID,
                            c.COURSE_CODE,
                            c.COURSE_NAME,
                            c.UNITS,
                            pc.SEMESTER,
                            pc.YEAR_LEVEL,
                            pc.IS_REQUIRED,
                            pc.IS_ELECTIVE,
                            c.DESCRIPTION,
                            c.LEC_HOURS,
                            c.LAB_HOURS,
                            c.TOTAL_HOURS,
                            STUFF((
                                SELECT ', ' + c2.COURSE_CODE
                                FROM COURSE_PREREQUISITES cp
                                JOIN COURSES c2 ON cp.PREREQUISITE_ID = c2.ID
                                WHERE cp.COURSE_ID = c.ID
                                FOR XML PATH('')
                            ), 1, 2, '') AS Prerequisites
                        FROM PROGRAMS_COURSE pc
                        JOIN COURSES c ON pc.COURSE_ID = c.ID
                        WHERE pc.PROGRAM_ID = @programId
                        ORDER BY pc.YEAR_LEVEL, pc.SEMESTER, c.COURSE_CODE";
                    cmd.Parameters.AddWithValue("@programId", programId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            courses.Add(new
                            {
                                PcId = reader["PC_ID"].ToString(),
                                Id = reader["ID"].ToString(),
                                CourseCode = reader["COURSE_CODE"].ToString(),
                                CourseName = reader["COURSE_NAME"].ToString(),
                                Units = reader["UNITS"].ToString(),
                                Semester = reader["SEMESTER"].ToString(),
                                YearLevel = reader["YEAR_LEVEL"].ToString(),
                                IsRequired = Convert.ToBoolean(reader["IS_REQUIRED"]),
                                IsElective = Convert.ToBoolean(reader["IS_ELECTIVE"]),
                                Description = reader["DESCRIPTION"] != DBNull.Value ? reader["DESCRIPTION"].ToString() : null,
                                LecHours = reader["LEC_HOURS"] != DBNull.Value ? reader["LEC_HOURS"].ToString() : "0",
                                LabHours = reader["LAB_HOURS"] != DBNull.Value ? reader["LAB_HOURS"].ToString() : "0",
                                TotalHours = reader["TOTAL_HOURS"] != DBNull.Value ? reader["TOTAL_HOURS"].ToString() : "0",
                                Prerequisites = reader["Prerequisites"] != DBNull.Value ? reader["Prerequisites"].ToString() : "None"
                            });
                        }
                    }
                }
            }

            return Json(courses, JsonRequestBehavior.AllowGet);
        }
    }
}