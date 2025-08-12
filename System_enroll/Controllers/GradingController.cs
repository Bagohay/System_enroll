using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace System_enroll.Controllers 
{
    public class GradingController : Controller
    {
        private readonly string connStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Kent\source\repos\System_enroll\System_enroll\App_Data\StudentEntry.mdf;Integrated Security=True";

        // Display the admin grading page
        public ActionResult ManageGrading()
        {
            if (Session["UserNumber"] == null || Session["IsLoggedIn"] == null || !(bool)Session["IsLoggedIn"])
            {
                return RedirectToAction("Login_Page", "Home");
            }
            if (Session["UserRole"]?.ToString() != "Admin")
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // Display the student academic history page
        public ActionResult ViewStudentHistory()
        {
            if (Session["UserNumber"] == null || Session["IsLoggedIn"] == null || !(bool)Session["IsLoggedIn"])
            {
                return RedirectToAction("Login_Page", "Home");
            }
            if (Session["UserRole"]?.ToString() != "Student")
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // Get enrolled students for grading (Admin)
        public ActionResult Get_StudentsForGrading()
        {
            if (Session["UserNumber"] == null || Session["IsLoggedIn"] == null || !(bool)Session["IsLoggedIn"])
            {
                return Json(new { error = "Session expired. Please log in again." }, JsonRequestBehavior.AllowGet);
            }
            if (Session["UserRole"]?.ToString() != "Admin")
            {
                return Json(new { error = "Unauthorized access. Admin role required." }, JsonRequestBehavior.AllowGet);
            }

            var data = new List<object>();
            try
            {
                using (var db = new SqlConnection(connStr))
                {
                    db.Open();
                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = @"
                    SELECT SE.STU_ENR_ID, SE.STU_ENR_STU_NUMBER, U.US_FNAME, U.US_LNAME, 
                           SE.STU_ENR_SCHOOLYEAR, SE.STU_ENR_SEMESTER, P.NAME AS ProgramName
                    FROM STUDENT_ENROLLMENT SE
                    INNER JOIN [USER] U ON SE.STU_ENR_STU_NUMBER = U.US_NUMBER
                    INNER JOIN PROGRAMS P ON SE.STU_ENR_PROGRAM_ID = P.ID
                    WHERE SE.STU_ES_ID = 2"; // Approved enrollments only
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                data.Add(new
                                {
                                    stuEnrId = reader["STU_ENR_ID"].ToString(),
                                    studentNumber = reader["STU_ENR_STU_NUMBER"].ToString(),
                                    studentName = $"{reader["US_FNAME"]} {reader["US_LNAME"]}",
                                    schoolYear = reader["STU_ENR_SCHOOLYEAR"].ToString(),
                                    semester = reader["STU_ENR_SEMESTER"].ToString(),
                                    programName = reader["ProgramName"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = "Failed to fetch students for grading: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // Get subjects for a specific enrollment to grade (Admin)
        public ActionResult Get_SubjectsForGrading()
        {
            string stuEnrId = Request["stuEnrId"];
            if (string.IsNullOrEmpty(stuEnrId))
            {
                return Json(new { error = "Invalid enrollment ID." }, JsonRequestBehavior.AllowGet);
            }

            var data = new List<object>();
            try
            {
                using (var db = new SqlConnection(connStr))
                {
                    db.Open();
                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = @"
                            SELECT SCE.SCE_ID, C.COURSE_CODE, C.COURSE_NAME, C.UNITS, SCE.GRADE
                            FROM STUDENT_COURSE_ENROLLMENT SCE
                            INNER JOIN SUBJECT_SCHEDULES SS ON SCE.SCHED_ID = SS.SCHED_ID
                            INNER JOIN PROGRAMS_COURSE PC ON SS.PC_ID = PC.PC_ID
                            INNER JOIN COURSES C ON PC.COURSE_ID = C.ID
                            WHERE SCE.STU_ENR_ID = @stuEnrId AND SCE.IS_DROPPED = 0";
                        cmd.Parameters.AddWithValue("@stuEnrId", stuEnrId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                data.Add(new
                                {
                                    sceId = reader["SCE_ID"].ToString(),
                                    courseCode = reader["COURSE_CODE"].ToString(),
                                    courseName = reader["COURSE_NAME"].ToString(),
                                    units = reader["UNITS"].ToString(),
                                    grade = reader["GRADE"] != DBNull.Value ? reader["GRADE"].ToString() : null
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = "Failed to fetch subjects for grading: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // Update grade for a subject (Admin)
        public ActionResult Update_Grade()
        {
            string sceId = Request["sceId"];
            string grade = Request["grade"];

            if (string.IsNullOrEmpty(sceId) || string.IsNullOrEmpty(grade))
            {
                return Json(new { success = false, message = "Invalid input." }, JsonRequestBehavior.AllowGet);
            }

            if (!decimal.TryParse(grade, out decimal gradeValue) || gradeValue < 0 || gradeValue > 4)
            {
                return Json(new { success = false, message = "Grade must be between 0.0 and 4.0." }, JsonRequestBehavior.AllowGet);
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
                            UPDATE STUDENT_COURSE_ENROLLMENT
                            SET GRADE = @grade
                            WHERE SCE_ID = @sceId";
                        cmd.Parameters.AddWithValue("@sceId", sceId);
                        cmd.Parameters.AddWithValue("@grade", gradeValue);
                        cmd.ExecuteNonQuery();
                    }
                }
                return Json(new { success = true, message = "Grade updated successfully." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to update grade: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Finalize grades into ACADEMIC_HISTORY (Admin)
        public ActionResult Finalize_Grades()
        {
            string stuEnrId = Request["stuEnrId"];
            if (string.IsNullOrEmpty(stuEnrId))
            {
                return Json(new { success = false, message = "Invalid enrollment ID." }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                using (var db = new SqlConnection(connStr))
                {
                    db.Open();
                    using (var transaction = db.BeginTransaction())
                    {
                        try
                        {
                            // Get enrollment details
                            string studentNumber, schoolYear, semester;
                            using (var cmd = db.CreateCommand())
                            {
                                cmd.Transaction = transaction;
                                cmd.CommandType = CommandType.Text;
                                cmd.CommandText = @"
                                    SELECT STU_ENR_STU_NUMBER, STU_ENR_SCHOOLYEAR, STU_ENR_SEMESTER
                                    FROM STUDENT_ENROLLMENT
                                    WHERE STU_ENR_ID = @stuEnrId";
                                cmd.Parameters.AddWithValue("@stuEnrId", stuEnrId);
                                using (var reader = cmd.ExecuteReader())
                                {
                                    if (!reader.Read())
                                    {
                                        transaction.Rollback();
                                        return Json(new { success = false, message = "Enrollment not found." }, JsonRequestBehavior.AllowGet);
                                    }
                                    studentNumber = reader["STU_ENR_STU_NUMBER"].ToString();
                                    schoolYear = reader["STU_ENR_SCHOOLYEAR"].ToString();
                                    semester = reader["STU_ENR_SEMESTER"].ToString();
                                    reader.Close();
                                }
                            }

                            // Get subjects and grades
                            var subjectsToFinalize = new List<(int courseId, decimal grade)>();
                            using (var cmd = db.CreateCommand())
                            {
                                cmd.Transaction = transaction;
                                cmd.CommandType = CommandType.Text;
                                cmd.CommandText = @"
                                    SELECT SCE.SCE_ID, SCE.GRADE, PC.COURSE_ID
                                    FROM STUDENT_COURSE_ENROLLMENT SCE
                                    INNER JOIN SUBJECT_SCHEDULES SS ON SCE.SCHED_ID = SS.SCHED_ID
                                    INNER JOIN PROGRAMS_COURSE PC ON SS.PC_ID = PC.PC_ID
                                    WHERE SCE.STU_ENR_ID = @stuEnrId AND SCE.IS_DROPPED = 0";
                                cmd.Parameters.AddWithValue("@stuEnrId", stuEnrId);
                                using (var reader = cmd.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        if (reader["GRADE"] == DBNull.Value)
                                        {
                                            transaction.Rollback();
                                            return Json(new { success = false, message = "All subjects must have grades before finalizing." }, JsonRequestBehavior.AllowGet);
                                        }
                                        subjectsToFinalize.Add((
                                            courseId: Convert.ToInt32(reader["COURSE_ID"]),
                                            grade: Convert.ToDecimal(reader["GRADE"])
                                        ));
                                    }
                                }
                            }

                            // Insert into ACADEMIC_HISTORY
                            foreach (var subject in subjectsToFinalize)
                            {
                                using (var cmd = db.CreateCommand())
                                {
                                    cmd.Transaction = transaction;
                                    cmd.CommandType = CommandType.Text;
                                    cmd.CommandText = @"
                                        INSERT INTO ACADEMIC_HISTORY (STU_NUMBER, COURSE_ID, SCHOOL_YEAR, SEMESTER, FINAL_GRADE, REMARKS)
                                        VALUES (@stuNumber, @courseId, @schoolYear, @semester, @finalGrade, @remarks)";
                                    cmd.Parameters.AddWithValue("@stuNumber", studentNumber);
                                    cmd.Parameters.AddWithValue("@courseId", subject.courseId);
                                    cmd.Parameters.AddWithValue("@schoolYear", schoolYear);
                                    cmd.Parameters.AddWithValue("@semester", semester);
                                    cmd.Parameters.AddWithValue("@finalGrade", subject.grade);
                                    cmd.Parameters.AddWithValue("@remarks", subject.grade >= 1.0m && subject.grade <= 3.0m ? "Passed" : "Failed");
                                    cmd.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();
                            return Json(new { success = true, message = "Grades finalized successfully." }, JsonRequestBehavior.AllowGet);
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            return Json(new { success = false, message = "Failed to finalize grades: " + ex.Message }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to finalize grades: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Get academic history for a student (Student)
        public ActionResult Get_AcademicHistory()
        {
            string studentNumber = Request["studentNumber"];
            if (Session["UserNumber"] == null || Session["IsLoggedIn"] == null || !(bool)Session["IsLoggedIn"])
            {
                return Json(new { error = "Session expired. Please log in again." }, JsonRequestBehavior.AllowGet);
            }
            if (Session["UserRole"]?.ToString() != "Student" || studentNumber != Session["UserNumber"].ToString())
            {
                return Json(new { error = "Unauthorized access." }, JsonRequestBehavior.AllowGet);
            }

            var data = new List<object>();
            try
            {
                using (var db = new SqlConnection(connStr))
                {
                    db.Open();
                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = @"
                            SELECT AH.HISTORY_ID, C.COURSE_CODE, C.COURSE_NAME, AH.SCHOOL_YEAR, AH.SEMESTER, 
                                   AH.FINAL_GRADE, AH.REMARKS
                            FROM ACADEMIC_HISTORY AH
                            INNER JOIN COURSES C ON AH.COURSE_ID = C.ID
                            WHERE AH.STU_NUMBER = @studentNumber";
                        cmd.Parameters.AddWithValue("@studentNumber", studentNumber);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                data.Add(new
                                {
                                    historyId = reader["HISTORY_ID"].ToString(),
                                    courseCode = reader["COURSE_CODE"].ToString(),
                                    courseName = reader["COURSE_NAME"].ToString(),
                                    schoolYear = reader["SCHOOL_YEAR"].ToString(),
                                    semester = reader["SEMESTER"].ToString(),
                                    finalGrade = reader["FINAL_GRADE"].ToString(),
                                    remarks = reader["REMARKS"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = "Failed to fetch academic history: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}