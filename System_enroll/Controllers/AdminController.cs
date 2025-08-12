using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace System_enroll.Controllers
{
    public class AdminController : Controller
    {
        private readonly string connStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Kent\source\repos\System_enroll\System_enroll\App_Data\StudentEntry.mdf;Integrated Security=True";

        public ActionResult Admin_Dashboard()
        {
            if (Session["UserNumber"] == null || Session["IsLoggedIn"] == null || !(bool)Session["IsLoggedIn"] || Session["UserRole"]?.ToString() != "Admin")
            {
                TempData["ErrorMessage"] = "Session expired or unauthorized access.";
                return RedirectToAction("Login_Page", "Home");
            }
            return View();
        }

        public ActionResult Admin_ManageEnrollees()
        {
            if (Session["UserNumber"] == null || Session["IsLoggedIn"] == null || !(bool)Session["IsLoggedIn"] || Session["UserRole"]?.ToString() != "Admin")
            {
                TempData["ErrorMessage"] = "Session expired or unauthorized access.";
                return RedirectToAction("Login_Page", "Home");
            }
            return View();
        }

        public ActionResult View_Students()
        {
            var data = new List<object>();
            using (var db = new SqlConnection(connStr))
            {
                db.Open();
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"SELECT U.US_NUMBER, U.US_FNAME, U.US_MNAME, U.US_LNAME, U.US_EMAIL, 
                                        P.ID AS PROGRAM_ID, P.NAME AS PROGRAM_NAME, 
                                        SE.STU_ENR_DATE, SE.STU_ENR_SCHOOLYEAR, SE.STU_ENR_SEMESTER, ES.ES_STATUS, 
                                        SE.STU_ENR_TYPE, S.SEC_NAME, SE.SEC_ID
                                        FROM [dbo].[USER] U 
                                        INNER JOIN [dbo].[STUDENT_ENROLLMENT] SE ON U.US_NUMBER = SE.STU_ENR_STU_NUMBER 
                                        INNER JOIN [dbo].[PROGRAMS] P ON P.ID = SE.STU_ENR_PROGRAM_ID 
                                        INNER JOIN [dbo].[ENROLLMENT_STATUS] ES ON ES.ES_ID = SE.STU_ES_ID 
                                        LEFT JOIN [dbo].[SECTION] S ON S.SEC_ID = SE.SEC_ID
                                        WHERE ES.ES_STATUS IN ('Pending', 'Rejected', 'Accepted')";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            data.Add(new
                            {
                                studId = reader["US_NUMBER"].ToString(),
                                firstname = reader["US_FNAME"].ToString(),
                                middlename = reader["US_MNAME"].ToString(),
                                lastname = reader["US_LNAME"].ToString(),
                                email = reader["US_EMAIL"].ToString(),
                                programId = reader["PROGRAM_ID"] != DBNull.Value ? Convert.ToInt32(reader["PROGRAM_ID"]) : 0,
                                programName = reader["PROGRAM_NAME"].ToString(),
                                applicationDate = Convert.ToDateTime(reader["STU_ENR_DATE"]).ToString("MM/dd/yyyy"),
                                schoolYear = reader["STU_ENR_SCHOOLYEAR"].ToString(),
                                semester = reader["STU_ENR_SEMESTER"].ToString(),
                                status = reader["ES_STATUS"].ToString(),
                                studentType = reader["STU_ENR_TYPE"].ToString(),
                                sectionName = reader["SEC_NAME"]?.ToString() ?? "N/A",
                                sectionId = reader["SEC_ID"] != DBNull.Value ? Convert.ToInt32(reader["SEC_ID"]) : (int?)null
                            });
                        }
                    }
                }
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Update_Enrollment_Status()
        {
            var data = new List<object>();
            var studentId = Request["StudentId"];
            var status = Request["status"];
            var sectionIdStr = Request["sectionId"];

            if (string.IsNullOrEmpty(studentId) || string.IsNullOrEmpty(status))
            {
                data.Add(new { mess = 1, error = "Invalid student ID or status" });
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            // Parse sectionId safely
            int? sectionId = null;
            if (!string.IsNullOrEmpty(sectionIdStr) && int.TryParse(sectionIdStr, out int parsedSectionId))
            {
                sectionId = parsedSectionId;
            }

            bool isRegular = false;
            string sectionName = "N/A";

            using (var db = new SqlConnection(connStr))
            {
                db.Open();

                // Check if student enrollment exists and get type
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"SELECT STU_ENR_TYPE 
                                        FROM [dbo].[STUDENT_ENROLLMENT] 
                                        WHERE STU_ENR_STU_NUMBER = @studentId 
                                        AND STU_ES_ID IN (1, 3)"; // Pending or Rejected
                    cmd.Parameters.AddWithValue("@studentId", studentId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            isRegular = reader["STU_ENR_TYPE"].ToString().ToLower() == "regular";
                        }
                        else
                        {
                            data.Add(new { mess = 1, error = "No pending or rejected enrollment found for this student" });
                            return Json(data, JsonRequestBehavior.AllowGet);
                        }
                    }
                }

                // Validate section requirement for regular students
                if (status.ToLower() == "accepted" && isRegular && !sectionId.HasValue)
                {
                    data.Add(new { mess = 1, error = "Section is required for regular students before approval" });
                    return Json(data, JsonRequestBehavior.AllowGet);
                }

                // Get status ID
                int statusId;
                switch (status.ToLower())
                {
                    case "pending":
                        statusId = 1;
                        break;
                    case "accepted":
                        statusId = 2;
                        break;
                    case "rejected":
                        statusId = 3;
                        break;
                    default:
                        data.Add(new { mess = 1, error = "Invalid status value" });
                        return Json(data, JsonRequestBehavior.AllowGet);
                }

                // If accepting a regular student, use transaction to handle both enrollment update and course assignment
                if (status.ToLower() == "accepted" && isRegular && sectionId.HasValue)
                {
                    using (var transaction = db.BeginTransaction())
                    {
                        try
                        {
                            // Get enrollment ID
                            int stuEnrId;
                            using (var cmd = db.CreateCommand())
                            {
                                cmd.Transaction = transaction;
                                cmd.CommandType = CommandType.Text;
                                cmd.CommandText = @"SELECT STU_ENR_ID 
                                                    FROM [dbo].[STUDENT_ENROLLMENT] 
                                                    WHERE STU_ENR_STU_NUMBER = @studentId 
                                                    AND STU_ES_ID IN (1, 3)";
                                cmd.Parameters.AddWithValue("@studentId", studentId);
                                stuEnrId = Convert.ToInt32(cmd.ExecuteScalar());
                            }

                            // Get section name
                            using (var cmd = db.CreateCommand())
                            {
                                cmd.Transaction = transaction;
                                cmd.CommandType = CommandType.Text;
                                cmd.CommandText = @"SELECT SEC_NAME 
                                                    FROM [dbo].[SECTION] 
                                                    WHERE SEC_ID = @sectionId";
                                cmd.Parameters.AddWithValue("@sectionId", sectionId.Value);
                                sectionName = cmd.ExecuteScalar()?.ToString() ?? "N/A";
                            }

                            // Update enrollment status and assign section
                            using (var cmd = db.CreateCommand())
                            {
                                cmd.Transaction = transaction;
                                cmd.CommandType = CommandType.Text;
                                cmd.CommandText = @"UPDATE [dbo].[STUDENT_ENROLLMENT] 
                                                    SET STU_ES_ID = @statusId, SEC_ID = @sectionId 
                                                    WHERE STU_ENR_STU_NUMBER = @studentId 
                                                    AND STU_ES_ID IN (1, 3)";
                                cmd.Parameters.AddWithValue("@statusId", statusId);
                                cmd.Parameters.AddWithValue("@studentId", studentId);
                                cmd.Parameters.AddWithValue("@sectionId", sectionId.Value);

                                var rowsAffected = cmd.ExecuteNonQuery();
                                if (rowsAffected == 0)
                                {
                                    data.Add(new { mess = 1, error = "No pending or rejected enrollment found for this student" });
                                    transaction.Rollback();
                                    return Json(data, JsonRequestBehavior.AllowGet);
                                }
                            }

                            // Assign all subjects for the section to the student
                            using (var cmd = db.CreateCommand())
                            {
                                cmd.Transaction = transaction;
                                cmd.CommandType = CommandType.Text;
                                cmd.CommandText = @"INSERT INTO [dbo].[STUDENT_COURSE_ENROLLMENT] 
                                                    (STU_ENR_ID, SCHED_ID, IS_DROPPED)
                                                    SELECT @stuEnrId, SCHED_ID, 0
                                                    FROM [dbo].[SUBJECT_SCHEDULES]
                                                    WHERE SEC_ID = @sectionId";
                                cmd.Parameters.AddWithValue("@stuEnrId", stuEnrId);
                                cmd.Parameters.AddWithValue("@sectionId", sectionId.Value);
                                cmd.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            data.Add(new { mess = 0, sectionName });
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            data.Add(new { mess = 1, error = "Failed to assign subjects or update enrollment: " + ex.Message });
                            return Json(data, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                else
                {
                    // Simple status update (no section assignment needed or irregular student)
                    if (status.ToLower() == "accepted" && sectionId.HasValue)
                    {
                        using (var cmd = db.CreateCommand())
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = @"SELECT SEC_NAME 
                                               FROM [dbo].[SECTION] 
                                               WHERE SEC_ID = @sectionId";
                            cmd.Parameters.AddWithValue("@sectionId", sectionId.Value);
                            sectionName = cmd.ExecuteScalar()?.ToString() ?? "N/A";
                        }
                    }

                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = @"UPDATE [dbo].[STUDENT_ENROLLMENT] 
                                            SET STU_ES_ID = @statusId, SEC_ID = @sectionId 
                                            WHERE STU_ENR_STU_NUMBER = @studentId 
                                            AND STU_ES_ID IN (1, 3)";
                        cmd.Parameters.AddWithValue("@statusId", statusId);
                        cmd.Parameters.AddWithValue("@studentId", studentId);
                        cmd.Parameters.AddWithValue("@sectionId", sectionId.HasValue ? (object)sectionId.Value : DBNull.Value);

                        var rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected >= 1)
                        {
                            data.Add(new { mess = 0, sectionName });
                        }
                        else
                        {
                            data.Add(new { mess = 1, error = "No pending or rejected enrollment found for this student" });
                        }
                    }
                }
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}