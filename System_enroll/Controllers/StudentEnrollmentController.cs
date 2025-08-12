using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace System_enroll.Controllers
{
    public class StudentEnrollmentController : Controller
    {
        private readonly string connStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Kent\source\repos\System_enroll\System_enroll\App_Data\StudentEntry.mdf;Integrated Security=True";

        // Check if student has approved enrollment for a specific school year and semester
        private bool HasApprovedEnrollment(string studentNumber, string schoolYear, string semester)
        {
            using (var db = new SqlConnection(connStr))
            {
                db.Open();
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"SELECT COUNT(*) 
                                        FROM [dbo].[STUDENT_ENROLLMENT] 
                                        WHERE STU_ENR_STU_NUMBER = @studentNumber 
                                        AND STU_ENR_SCHOOLYEAR = @schoolYear 
                                        AND STU_ENR_SEMESTER = @semester 
                                        AND STU_ES_ID = 2"; // Approved status
                    cmd.Parameters.AddWithValue("@studentNumber", studentNumber);
                    cmd.Parameters.AddWithValue("@schoolYear", schoolYear);
                    cmd.Parameters.AddWithValue("@semester", semester);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        // Check if student has graded records for a specific year level and semester (regardless of school year)
        private bool HasGradedRecordForYearLevelAndSemester(string studentNumber, string yearLevel, string semester)
        {
            using (var db = new SqlConnection(connStr))
            {
                db.Open();
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"SELECT COUNT(*) 
                                        FROM [dbo].[STUDENT_ENROLLMENT] SE
                                        INNER JOIN [dbo].[STUDENT_COURSE_ENROLLMENT] SCE ON SCE.STU_ENR_ID = SE.STU_ENR_ID
                                        INNER JOIN [dbo].[SUBJECT_SCHEDULES] SS ON SCE.SCHED_ID = SS.SCHED_ID
                                        INNER JOIN [dbo].[PROGRAMS_COURSE] PC ON SS.PC_ID = PC.PC_ID
                                        INNER JOIN [dbo].[COURSES] C ON PC.COURSE_ID = C.ID
                                        INNER JOIN [dbo].[ACADEMIC_HISTORY] AH ON AH.STU_NUMBER = SE.STU_ENR_STU_NUMBER 
                                            AND AH.COURSE_ID = C.ID 
                                            AND AH.SCHOOL_YEAR = SE.STU_ENR_SCHOOLYEAR 
                                            AND AH.SEMESTER = SE.STU_ENR_SEMESTER
                                        WHERE SE.STU_ENR_STU_NUMBER = @studentNumber 
                                        AND SE.STU_ENR_YEARLEVEL = @yearLevel 
                                        AND SE.STU_ENR_SEMESTER = @semester 
                                        AND SE.STU_ES_ID = 2 
                                        AND SCE.IS_DROPPED = 0 
                                        AND AH.FINAL_GRADE IS NOT NULL";
                    cmd.Parameters.AddWithValue("@studentNumber", studentNumber);
                    cmd.Parameters.AddWithValue("@yearLevel", yearLevel);
                    cmd.Parameters.AddWithValue("@semester", semester);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        // Check if student has access to enroll for a specific school year and semester
        private bool HasEnrollmentAccess(string studentNumber, string schoolYear, string semester)
        {
            if (string.IsNullOrEmpty(studentNumber) || string.IsNullOrEmpty(schoolYear) || string.IsNullOrEmpty(semester))
            {
                return false;
            }

            using (var db = new SqlConnection(connStr))
            {
                db.Open();

                // Step 1: Check for pending enrollments in the target school year and semester
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"SELECT COUNT(*) 
                                        FROM [dbo].[STUDENT_ENROLLMENT] 
                                        WHERE STU_ENR_STU_NUMBER = @studentNumber 
                                        AND STU_ENR_SCHOOLYEAR = @schoolYear 
                                        AND STU_ENR_SEMESTER = @semester 
                                        AND STU_ES_ID = 1"; // Pending status
                    cmd.Parameters.AddWithValue("@studentNumber", studentNumber);
                    cmd.Parameters.AddWithValue("@schoolYear", schoolYear);
                    cmd.Parameters.AddWithValue("@semester", semester);
                    int pendingCount = Convert.ToInt32(cmd.ExecuteScalar());

                    if (pendingCount > 0)
                    {
                        return false; // Deny access if there is a pending enrollment for this period
                    }
                }

                // Step 2: Check for approved enrollments in the target school year and semester
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"SELECT COUNT(*) 
                                        FROM [dbo].[STUDENT_ENROLLMENT] 
                                        WHERE STU_ENR_STU_NUMBER = @studentNumber 
                                        AND STU_ENR_SCHOOLYEAR = @schoolYear 
                                        AND STU_ENR_SEMESTER = @semester 
                                        AND STU_ES_ID = 2"; // Approved status
                    cmd.Parameters.AddWithValue("@studentNumber", studentNumber);
                    cmd.Parameters.AddWithValue("@schoolYear", schoolYear);
                    cmd.Parameters.AddWithValue("@semester", semester);
                    int approvedCount = Convert.ToInt32(cmd.ExecuteScalar());

                    if (approvedCount > 0)
                    {
                        // Check if all subjects for this approved enrollment have grades
                        using (var cmd2 = db.CreateCommand())
                        {
                            cmd2.CommandType = CommandType.Text;
                            cmd2.CommandText = @"SELECT COUNT(*) 
                                                FROM [dbo].[STUDENT_COURSE_ENROLLMENT] SCE
                                                INNER JOIN [dbo].[SUBJECT_SCHEDULES] SS ON SCE.SCHED_ID = SS.SCHED_ID
                                                INNER JOIN [dbo].[PROGRAMS_COURSE] PC ON SS.PC_ID = PC.PC_ID
                                                INNER JOIN [dbo].[COURSES] C ON PC.COURSE_ID = C.ID
                                                LEFT JOIN [dbo].[ACADEMIC_HISTORY] AH ON AH.STU_NUMBER = @studentNumber 
                                                    AND AH.COURSE_ID = C.ID 
                                                    AND AH.SCHOOL_YEAR = @schoolYear 
                                                    AND AH.SEMESTER = @semester
                                                INNER JOIN [dbo].[STUDENT_ENROLLMENT] SE ON SCE.STU_ENR_ID = SE.STU_ENR_ID
                                                WHERE SE.STU_ENR_STU_NUMBER = @studentNumber 
                                                AND SE.STU_ENR_SCHOOLYEAR = @schoolYear 
                                                AND SE.STU_ENR_SEMESTER = @semester 
                                                AND SE.STU_ES_ID = 2 
                                                AND SCE.IS_DROPPED = 0 
                                                AND AH.HISTORY_ID IS NULL"; // Subjects without grades
                            cmd2.Parameters.AddWithValue("@studentNumber", studentNumber);
                            cmd2.Parameters.AddWithValue("@schoolYear", schoolYear);
                            cmd2.Parameters.AddWithValue("@semester", semester);
                            int ungradedSubjects = Convert.ToInt32(cmd2.ExecuteScalar());

                            // If all subjects have grades, allow enrollment for a different semester
                            return ungradedSubjects == 0;
                        }
                    }
                }

                // Step 3: Check for the most recent approved enrollment (from a previous period)
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"SELECT TOP 1 STU_ENR_ID, STU_ENR_SCHOOLYEAR, STU_ENR_SEMESTER 
                                        FROM [dbo].[STUDENT_ENROLLMENT] 
                                        WHERE STU_ENR_STU_NUMBER = @studentNumber 
                                        AND STU_ES_ID = 2 
                                        AND (STU_ENR_SCHOOLYEAR != @schoolYear OR STU_ENR_SEMESTER != @semester) 
                                        ORDER BY STU_ENR_DATE DESC"; // Most recent approved enrollment from a different period
                    cmd.Parameters.AddWithValue("@studentNumber", studentNumber);
                    cmd.Parameters.AddWithValue("@schoolYear", schoolYear);
                    cmd.Parameters.AddWithValue("@semester", semester);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            // No approved enrollments from previous periods; student can enroll
                            return true;
                        }

                        // Get details of the most recent approved enrollment
                        int stuEnrId = Convert.ToInt32(reader["STU_ENR_ID"]);
                        string prevSchoolYear = reader["STU_ENR_SCHOOLYEAR"].ToString();
                        string prevSemester = reader["STU_ENR_SEMESTER"].ToString();
                        reader.Close();

                        // Step 4: Check if all subjects for this previous enrollment have been graded
                        using (var cmd2 = db.CreateCommand())
                        {
                            cmd2.CommandType = CommandType.Text;
                            cmd2.CommandText = @"SELECT COUNT(*) 
                                                FROM [dbo].[STUDENT_COURSE_ENROLLMENT] SCE
                                                INNER JOIN [dbo].[SUBJECT_SCHEDULES] SS ON SCE.SCHED_ID = SS.SCHED_ID
                                                INNER JOIN [dbo].[PROGRAMS_COURSE] PC ON SS.PC_ID = PC.PC_ID
                                                INNER JOIN [dbo].[COURSES] C ON PC.COURSE_ID = C.ID
                                                LEFT JOIN [dbo].[ACADEMIC_HISTORY] AH ON AH.STU_NUMBER = @studentNumber 
                                                    AND AH.COURSE_ID = C.ID 
                                                    AND AH.SCHOOL_YEAR = @prevSchoolYear 
                                                    AND AH.SEMESTER = @prevSemester
                                                WHERE SCE.STU_ENR_ID = @stuEnrId 
                                                AND SCE.IS_DROPPED = 0 
                                                AND AH.HISTORY_ID IS NULL"; // Subjects without grades
                            cmd2.Parameters.AddWithValue("@studentNumber", studentNumber);
                            cmd2.Parameters.AddWithValue("@prevSchoolYear", prevSchoolYear);
                            cmd2.Parameters.AddWithValue("@prevSemester", prevSemester);
                            cmd2.Parameters.AddWithValue("@stuEnrId", stuEnrId);
                            int ungradedSubjects = Convert.ToInt32(cmd2.ExecuteScalar());

                            // Allow enrollment only if all subjects from the previous period are graded
                            return ungradedSubjects == 0;
                        }
                    }
                }
            }
        }

        public ActionResult Check_Enrollment_Access()
        {
            string studentNumber = Request["studentNumber"];
            string schoolYear = Request["schoolYear"];
            string semester = Request["semester"];

            if (Session["UserNumber"] == null || Session["IsLoggedIn"] == null || !(bool)Session["IsLoggedIn"])
            {
                return Json(new { hasAccess = false, message = "Session expired. Please log in again." }, JsonRequestBehavior.AllowGet);
            }
            if (Session["UserRole"]?.ToString() != "Student")
            {
                return Json(new { hasAccess = false, message = "Unauthorized access. Student role required." }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(studentNumber) || studentNumber != Session["UserNumber"].ToString())
            {
                return Json(new { hasAccess = false, message = "Invalid student number." }, JsonRequestBehavior.AllowGet);
            }

            // Validate school year and semester
            if (string.IsNullOrEmpty(schoolYear) || string.IsNullOrEmpty(semester))
            {
                return Json(new { hasAccess = false, message = "School year and semester are required to check enrollment access." }, JsonRequestBehavior.AllowGet);
            }

            bool hasAccess = HasEnrollmentAccess(studentNumber, schoolYear, semester);
            string message = hasAccess ? "" : "Enrollment denied: You have a pending enrollment, an active enrollment with ungraded subjects, or must complete grading for all subjects from your previous semester.";
            return Json(new { hasAccess, message }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Display_StudentEnrollment()
        {
            if (Session["UserNumber"] == null)
            {
                return RedirectToAction("Login_Page", "Home");
            }
            return View();
        }

        public ActionResult Display_SubjectLoad()
        {
            if (Session["UserNumber"] == null)
            {
                return RedirectToAction("Login_Page", "Home");
            }
            return View();
        }

        public ActionResult Get_Programs()
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
                        cmd.CommandText = "SELECT ID, NAME FROM PROGRAMS";
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                data.Add(new
                                {
                                    id = reader["ID"].ToString(),
                                    name = reader["NAME"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = "Failed to fetch programs: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Get_Sections()
        {
            string programId = Request["programId"];
            var data = new List<object>();
            try
            {
                using (var db = new SqlConnection(connStr))
                {
                    db.Open();
                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = @"SELECT SEC_ID, SEC_NAME 
                                            FROM SECTION 
                                            WHERE PROG_ID = @programId";
                        cmd.Parameters.AddWithValue("@programId", programId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                data.Add(new
                                {
                                    secId = reader["SEC_ID"].ToString(),
                                    secName = reader["SEC_NAME"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = "Failed to fetch sections: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Get_AvailableSubjects()
        {
            string studentNumber = Request["studentNumber"];
            string programId = Request["programId"];
            string semester = Request["semester"];
            string yearLevel = Request["yearLevel"];
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
                            SELECT SS.SCHED_ID, SS.MIS_CODE, C.COURSE_CODE, C.COURSE_NAME, C.UNITS,
                                   STRING_AGG(
                                       CONVERT(varchar, SSD.START_TIME, 100) + ' - ' + 
                                       CONVERT(varchar, SSD.END_TIME, 100) + ' (' + SSD.DAY + ')', ', '
                                   ) AS Time,
                                   STRING_AGG(SSD.DAY, ', ') AS Day,
                                   STRING_AGG(SSD.ROOM, ', ') AS Room
                            FROM PROGRAMS_COURSE PC
                            INNER JOIN COURSES C ON PC.COURSE_ID = C.ID
                            INNER JOIN SUBJECT_SCHEDULES SS ON SS.PC_ID = PC.PC_ID
                            INNER JOIN SUBJECT_SCHEDULE_DETAILS SSD ON SS.SCHED_ID = SSD.SCHED_ID
                            WHERE PC.PROGRAM_ID = @programId 
                            AND PC.SEMESTER = (CASE @semester WHEN 'First' THEN 1 ELSE 2 END)
                            AND PC.YEAR_LEVEL = @yearLevel
                            AND NOT EXISTS (
                                SELECT 1 FROM ACADEMIC_HISTORY AH 
                                WHERE AH.STU_NUMBER = @studentNumber 
                                AND AH.COURSE_ID = C.ID 
                                AND AH.FINAL_GRADE >= 1.0
                            )
                            GROUP BY SS.SCHED_ID, SS.MIS_CODE, C.COURSE_CODE, C.COURSE_NAME, C.UNITS";
                        cmd.Parameters.AddWithValue("@studentNumber", studentNumber);
                        cmd.Parameters.AddWithValue("@programId", programId);
                        cmd.Parameters.AddWithValue("@semester", semester);
                        cmd.Parameters.AddWithValue("@yearLevel", yearLevel);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                data.Add(new
                                {
                                    schedId = reader["SCHED_ID"].ToString(),
                                    msiCode = reader["MIS_CODE"].ToString(),
                                    courseCode = reader["COURSE_CODE"].ToString(),
                                    courseName = reader["COURSE_NAME"].ToString(),
                                    units = reader["UNITS"].ToString(),
                                    time = reader["Time"].ToString(),
                                    day = reader["Day"].ToString(),
                                    room = reader["Room"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = "Failed to fetch subjects: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Get_EnrolledSubjects()
        {
            string studentNumber = Request["studentNumber"];
            string schoolYear = Request["schoolYear"];
            string semester = Request["semester"];
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
                            SELECT SS.MIS_CODE, C.COURSE_CODE, C.COURSE_NAME, C.DESCRIPTION, C.UNITS,
                                   STRING_AGG(
                                       CONVERT(varchar, SSD.START_TIME, 100) + ' - ' + 
                                       CONVERT(varchar, SSD.END_TIME, 100) + ' (' + SSD.DAY + ')', ', '
                                   ) AS TIME,
                                   STRING_AGG(SSD.DAY, ', ') AS DAY,
                                   STRING_AGG(SSD.ROOM, ', ') AS ROOM
                            FROM STUDENT_COURSE_ENROLLMENT SCE
                            INNER JOIN STUDENT_ENROLLMENT SE ON SCE.STU_ENR_ID = SE.STU_ENR_ID
                            INNER JOIN SUBJECT_SCHEDULES SS ON SCE.SCHED_ID = SS.SCHED_ID
                            INNER JOIN SUBJECT_SCHEDULE_DETAILS SSD ON SS.SCHED_ID = SSD.SCHED_ID
                            INNER JOIN PROGRAMS_COURSE PC ON SS.PC_ID = PC.PC_ID
                            INNER JOIN COURSES C ON PC.COURSE_ID = C.ID
                            LEFT JOIN [dbo].[ACADEMIC_HISTORY] AH ON AH.STU_NUMBER = @studentNumber 
                                AND AH.COURSE_ID = C.ID 
                                AND AH.SCHOOL_YEAR = SE.STU_ENR_SCHOOLYEAR 
                                AND AH.SEMESTER = SE.STU_ENR_SEMESTER
                            WHERE SE.STU_ENR_STU_NUMBER = @studentNumber 
                            AND (SE.STU_ENR_SCHOOLYEAR = @schoolYear OR @schoolYear = '')
                            AND (SE.STU_ENR_SEMESTER = @semester OR @semester = '')
                            AND SCE.IS_DROPPED = 0
                            AND AH.HISTORY_ID IS NULL
                            GROUP BY SS.MIS_CODE, C.COURSE_CODE, C.COURSE_NAME, C.DESCRIPTION, C.UNITS";
                        cmd.Parameters.AddWithValue("@studentNumber", studentNumber);
                        cmd.Parameters.AddWithValue("@schoolYear", schoolYear ?? "");
                        cmd.Parameters.AddWithValue("@semester", semester ?? "");
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                data.Add(new
                                {
                                    msiCode = reader["MIS_CODE"].ToString(),
                                    courseCode = reader["COURSE_CODE"].ToString(),
                                    courseName = reader["COURSE_NAME"].ToString(),
                                    description = reader["DESCRIPTION"].ToString(),
                                    units = reader["UNITS"].ToString(),
                                    time = reader["TIME"].ToString(),
                                    day = reader["DAY"].ToString(),
                                    room = reader["ROOM"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = "Failed to fetch enrolled subjects: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ViewGroupedEnrolledSubjects()
        {
            string studentNumber = Session["UserNumber"]?.ToString();
            string schoolYear = Request["schoolYear"];
            string semester = Request["semester"];

            if (string.IsNullOrEmpty(studentNumber))
            {
                return Json(new { error = "Invalid student number." }, JsonRequestBehavior.AllowGet);
            }

            var groupedData = new Dictionary<string, List<object>>();
            try
            {
                using (var db = new SqlConnection(connStr))
                {
                    db.Open();
                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = @"
                            SELECT SE.STU_ENR_ID, S.SEC_NAME, P.NAME AS ProgramName, C.ID AS CourseId, 
                                   SS.MIS_CODE, C.COURSE_CODE, C.COURSE_NAME, C.UNITS,
                                   STRING_AGG(
                                       CONVERT(varchar, SSD.START_TIME, 100) + ' - ' + 
                                       CONVERT(varchar, SSD.END_TIME, 100) + ' (' + SSD.DAY + ')', ', '
                                   ) AS Time,
                                   STRING_AGG(SSD.DAY, ', ') AS Day,
                                   STRING_AGG(SSD.ROOM, ', ') AS Room
                            FROM STUDENT_ENROLLMENT SE
                            INNER JOIN SECTION S ON SE.SEC_ID = S.SEC_ID
                            INNER JOIN PROGRAMS P ON SE.STU_ENR_PROGRAM_ID = P.ID
                            INNER JOIN STUDENT_COURSE_ENROLLMENT SCE ON SCE.STU_ENR_ID = SE.STU_ENR_ID
                            INNER JOIN SUBJECT_SCHEDULES SS ON SCE.SCHED_ID = SS.SCHED_ID
                            INNER JOIN SUBJECT_SCHEDULE_DETAILS SSD ON SS.SCHED_ID = SSD.SCHED_ID
                            INNER JOIN PROGRAMS_COURSE PC ON SS.PC_ID = PC.PC_ID
                            INNER JOIN COURSES C ON PC.COURSE_ID = C.ID
                            LEFT JOIN [dbo].[ACADEMIC_HISTORY] AH ON AH.STU_NUMBER = @studentNumber 
                                AND AH.COURSE_ID = C.ID 
                                AND AH.SCHOOL_YEAR = SE.STU_ENR_SCHOOLYEAR 
                                AND AH.SEMESTER = SE.STU_ENR_SEMESTER
                            WHERE SE.STU_ENR_STU_NUMBER = @studentNumber
                            AND SE.STU_ENR_SCHOOLYEAR = @schoolYear
                            AND SE.STU_ENR_SEMESTER = @semester
                            AND SCE.IS_DROPPED = 0
                            AND AH.HISTORY_ID IS NULL
                            AND SE.STU_ES_ID = 2
                            GROUP BY SE.STU_ENR_ID, S.SEC_NAME, P.NAME, C.ID, SS.MIS_CODE, C.COURSE_CODE, C.COURSE_NAME, C.UNITS
                            ORDER BY S.SEC_NAME";
                        cmd.Parameters.AddWithValue("@studentNumber", studentNumber);
                        cmd.Parameters.AddWithValue("@schoolYear", schoolYear);
                        cmd.Parameters.AddWithValue("@semester", semester);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string sectionName = reader["SEC_NAME"].ToString();
                                string programName = reader["ProgramName"].ToString();
                                string displaySection = $"{programName} {sectionName}";

                                var subject = new
                                {
                                    courseId = reader["CourseId"].ToString(),
                                    msiCode = reader["MIS_CODE"].ToString(),
                                    subject = reader["COURSE_CODE"].ToString(),
                                    description = reader["COURSE_NAME"].ToString(),
                                    units = reader["UNITS"].ToString(),
                                    time = reader["Time"].ToString(),
                                    day = reader["Day"].ToString(),
                                    room = reader["Room"].ToString()
                                };

                                if (!groupedData.ContainsKey(displaySection))
                                {
                                    groupedData[displaySection] = new List<object>();
                                }
                                groupedData[displaySection].Add(subject);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = "Failed to fetch grouped enrolled subjects: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(groupedData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Get_SubjectLoad()
        {
            string studentNumber = Request["studentNumber"];
            string schoolYear = Request["schoolYear"];
            string semester = Request["semester"];

            if (Session["UserNumber"] == null || Session["IsLoggedIn"] == null || !(bool)Session["IsLoggedIn"])
            {
                return Json(new { error = "Session expired. Please log in again." }, JsonRequestBehavior.AllowGet);
            }
            if (Session["UserRole"]?.ToString() != "Student")
            {
                return Json(new { error = "Unauthorized access. Student role required." }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(studentNumber) || studentNumber != Session["UserNumber"].ToString())
            {
                return Json(new { error = "Invalid student number." }, JsonRequestBehavior.AllowGet);
            }

            using (var db = new SqlConnection(connStr))
            {
                db.Open();
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"SELECT COUNT(*) 
                                        FROM [dbo].[STUDENT_ENROLLMENT] 
                                        WHERE STU_ENR_STU_NUMBER = @studentNumber 
                                        AND STU_ES_ID = 1"; // Pending status
                    cmd.Parameters.AddWithValue("@studentNumber", studentNumber);
                    int pendingCount = Convert.ToInt32(cmd.ExecuteScalar());

                    if (pendingCount > 0)
                    {
                        return Json(new { error = "Your enrollment is pending approval. Please wait for admin approval." }, JsonRequestBehavior.AllowGet);
                    }
                }

                var data = new List<object>();
                try
                {
                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = @"
                            -- Irregular students
                            SELECT SS.MIS_CODE, C.COURSE_CODE, C.COURSE_NAME, C.DESCRIPTION, C.UNITS, 
                                   STRING_AGG(
                                       CONVERT(varchar, SSD.START_TIME, 100) + ' - ' + 
                                       CONVERT(varchar, SSD.END_TIME, 100) + ' (' + SSD.DAY + ')', ', '
                                   ) AS TIME,
                                   STRING_AGG(SSD.DAY, ', ') AS DAY,
                                   STRING_AGG(SSD.ROOM, ', ') AS ROOM
                            FROM STUDENT_COURSE_ENROLLMENT SCE
                            INNER JOIN STUDENT_ENROLLMENT SE ON SCE.STU_ENR_ID = SE.STU_ENR_ID
                            INNER JOIN SUBJECT_SCHEDULES SS ON SCE.SCHED_ID = SS.SCHED_ID
                            INNER JOIN SUBJECT_SCHEDULE_DETAILS SSD ON SS.SCHED_ID = SSD.SCHED_ID
                            INNER JOIN PROGRAMS_COURSE PC ON SS.PC_ID = PC.PC_ID
                            INNER JOIN COURSES C ON PC.COURSE_ID = C.ID
                            WHERE SE.STU_ENR_STU_NUMBER = @studentNumber 
                            AND SE.STU_ES_ID = 2
                            AND (SE.STU_ENR_SCHOOLYEAR = @schoolYear OR @schoolYear = '')
                            AND (SE.STU_ENR_SEMESTER = @semester OR @semester = '')
                            AND SCE.IS_DROPPED = 0
                            AND SE.STU_ENR_TYPE = 'Irregular'
                            GROUP BY SS.MIS_CODE, C.COURSE_CODE, C.COURSE_NAME, C.DESCRIPTION, C.UNITS

                            UNION

                            -- Regular students
                            SELECT SS.MIS_CODE, C.COURSE_CODE, C.COURSE_NAME, C.DESCRIPTION, C.UNITS, 
                                   STRING_AGG(
                                       CONVERT(varchar, SSD.START_TIME, 100) + ' - ' + 
                                       CONVERT(varchar, SSD.END_TIME, 100) + ' (' + SSD.DAY + ')', ', '
                                   ) AS TIME,
                                   STRING_AGG(SSD.DAY, ', ') AS DAY,
                                   STRING_AGG(SSD.ROOM, ', ') AS ROOM
                            FROM STUDENT_ENROLLMENT SE
                            INNER JOIN SECTION S ON SE.SEC_ID = S.SEC_ID
                            INNER JOIN SUBJECT_SCHEDULES SS ON SS.SEC_ID = S.SEC_ID
                            INNER JOIN SUBJECT_SCHEDULE_DETAILS SSD ON SS.SCHED_ID = SSD.SCHED_ID
                            INNER JOIN PROGRAMS_COURSE PC ON SS.PC_ID = PC.PC_ID
                            INNER JOIN COURSES C ON PC.COURSE_ID = C.ID
                            WHERE SE.STU_ENR_STU_NUMBER = @studentNumber 
                            AND SE.STU_ES_ID = 2
                            AND (SE.STU_ENR_SCHOOLYEAR = @schoolYear OR @schoolYear = '')
                            AND (SE.STU_ENR_SEMESTER = @semester OR @semester = '')
                            AND SE.STU_ENR_TYPE = 'Regular'
                            AND SE.SEC_ID IS NOT NULL
                            GROUP BY SS.MIS_CODE, C.COURSE_CODE, C.COURSE_NAME, C.DESCRIPTION, C.UNITS";
                        cmd.Parameters.AddWithValue("@studentNumber", studentNumber);
                        cmd.Parameters.AddWithValue("@schoolYear", schoolYear ?? "");
                        cmd.Parameters.AddWithValue("@semester", semester ?? "");
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                data.Add(new
                                {
                                    msiCode = reader["MIS_CODE"].ToString(),
                                    courseCode = reader["COURSE_CODE"].ToString(),
                                    courseName = reader["COURSE_NAME"].ToString(),
                                    description = reader["DESCRIPTION"].ToString(),
                                    units = reader["UNITS"].ToString(),
                                    time = reader["TIME"].ToString(),
                                    day = reader["DAY"].ToString(),
                                    room = reader["ROOM"].ToString()
                                });
                            }
                        }
                    }
                    if (data.Count == 0)
                    {
                        return Json(new { error = "No approved enrollment with assigned section found for the selected period." }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { error = "Failed to fetch subject load: " + ex.Message }, JsonRequestBehavior.AllowGet);
                }
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Enroll_Student()
        {
            string studentNumber = Request["studentNumber"];
            string programId = Request["programId"];
            string schoolYear = Request["schoolYear"];
            string semester = Request["semester"];
            string yearLevel = Request["yearLevel"];
            string isRegularStr = Request["isRegular"];
            string subjectSelectionIds = Request["subjectSelectionIds"];
            string enrollmentStatusId = Request["enrollmentStatusId"];

            var data = new List<object>();

            // Step 1: Validate session and user role
            if (Session["UserNumber"] == null || Session["IsLoggedIn"] == null || !(bool)Session["IsLoggedIn"] || Session["UserRole"]?.ToString() != "Student")
            {
                data.Add(new { success = false, message = "Session expired or unauthorized access." });
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            // Step 2: Validate input parameters
            if (string.IsNullOrEmpty(studentNumber) || string.IsNullOrEmpty(programId) || string.IsNullOrEmpty(schoolYear) || string.IsNullOrEmpty(semester) || string.IsNullOrEmpty(yearLevel))
            {
                data.Add(new { success = false, message = "Invalid enrollment details." });
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            if (!int.TryParse(programId, out int progId) || !int.TryParse(enrollmentStatusId, out int enrStatusId) || !bool.TryParse(isRegularStr, out bool isRegular))
            {
                data.Add(new { success = false, message = "Invalid input format." });
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            // Step 3: Check for existing graded records for the same year level and semester
            if (HasGradedRecordForYearLevelAndSemester(studentNumber, yearLevel, semester))
            {
                data.Add(new { success = false, message = "Enrollment denied: You have already completed and been graded for this year level and semester." });
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            // Step 4: Check for duplicate approved enrollment in the same school year and semester
            if (HasApprovedEnrollment(studentNumber, schoolYear, semester))
            {
                data.Add(new { success = false, message = "Enrollment denied: You already have an approved enrollment for this semester and school year." });
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            // Step 5: Check enrollment access (except for pending submissions)
            if (!HasEnrollmentAccess(studentNumber, schoolYear, semester) && enrStatusId != 1)
            {
                data.Add(new { success = false, message = "Enrollment denied: You have a pending enrollment or must complete grading for all subjects before enrolling again." });
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            // Step 6: Proceed with enrollment
            using (var db = new SqlConnection(connStr))
            {
                db.Open();
                using (var transaction = db.BeginTransaction())
                {
                    try
                    {
                        // Insert into STUDENT_ENROLLMENT (no SEC_ID at this stage)
                        int stuEnrId;
                        using (var cmd = db.CreateCommand())
                        {
                            cmd.Transaction = transaction;
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = @"INSERT INTO [dbo].[STUDENT_ENROLLMENT] 
                                                (STU_ENR_STU_NUMBER, STU_ENR_PROGRAM_ID, STU_ENR_SCHOOLYEAR, STU_ENR_SEMESTER, 
                                                STU_ENR_YEARLEVEL, STU_ENR_TYPE, STU_ES_ID) 
                                                VALUES (@studentNumber, @programId, @schoolYear, @semester, @yearLevel, 
                                                @studentType, @enrollmentStatusId);
                                                SELECT SCOPE_IDENTITY();";
                            cmd.Parameters.AddWithValue("@studentNumber", studentNumber);
                            cmd.Parameters.AddWithValue("@programId", progId);
                            cmd.Parameters.AddWithValue("@schoolYear", schoolYear);
                            cmd.Parameters.AddWithValue("@semester", semester);
                            cmd.Parameters.AddWithValue("@yearLevel", yearLevel);
                            cmd.Parameters.AddWithValue("@studentType", isRegular ? "Regular" : "Irregular");
                            cmd.Parameters.AddWithValue("@enrollmentStatusId", enrStatusId);
                            stuEnrId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        // For irregular students, insert selected subjects
                        if (!isRegular && !string.IsNullOrEmpty(subjectSelectionIds))
                        {
                            var schedIds = subjectSelectionIds.Split(',').Select(int.Parse).ToList();
                            foreach (var schedId in schedIds)
                            {
                                using (var cmd = db.CreateCommand())
                                {
                                    cmd.Transaction = transaction;
                                    cmd.CommandType = CommandType.Text;
                                    cmd.CommandText = @"INSERT INTO [dbo].[STUDENT_COURSE_ENROLLMENT] 
                                                        (STU_ENR_ID, SCHED_ID, IS_DROPPED) 
                                                        VALUES (@stuEnrId, @schedId, 0)";
                                    cmd.Parameters.AddWithValue("@stuEnrId", stuEnrId);
                                    cmd.Parameters.AddWithValue("@schedId", schedId);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

                        transaction.Commit();
                        data.Add(new { success = true, schoolYear, semester });
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        data.Add(new { success = false, message = "Error enrolling student: " + ex.Message });
                    }
                }
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}