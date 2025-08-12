using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace System_enroll.Controllers
{
    public class Assign_ScheduleController : Controller
    {
        private readonly string connStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Kent\source\repos\System_enroll\System_enroll\App_Data\StudentEntry.mdf;Integrated Security=True";

        public ActionResult Assign_Schedule()
        {
            if (Session["UserNumber"] == null)
            {
                return RedirectToAction("Login_Page", "Home");
            }
            return View();
        }

        public ActionResult View_Schedules()
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
                        cmd.CommandText = @"
                            SELECT SS.SCHED_ID, SS.PC_ID, SS.SEC_ID, SS.FAC_ID, SS.MIS_CODE, S.SEC_NAME,
                                   P.NAME AS ProgramName, C.COURSE_NAME, F.[FAC_ NAME] AS FAC_NAME
                            FROM Subject_SCHEDULES SS
                            INNER JOIN PROGRAMS_COURSE PC ON SS.PC_ID = PC.PC_ID
                            INNER JOIN PROGRAMS P ON PC.PROGRAM_ID = P.ID
                            INNER JOIN COURSES C ON PC.COURSE_ID = C.ID
                            LEFT JOIN FACULTY F ON SS.FAC_ID = F.FAC_ID
                            INNER JOIN SECTION S ON SS.SEC_ID = S.SEC_ID";
                        using (var reader = cmd.ExecuteReader())
                        {
                            var schedules = new Dictionary<int, dynamic>();
                            while (reader.Read())
                            {
                                int schedId = Convert.ToInt32(reader["SCHED_ID"]);
                                schedules[schedId] = new
                                {
                                    schedId = schedId.ToString(),
                                    pcId = reader["PC_ID"].ToString(),
                                    secId = reader["SEC_ID"].ToString(),
                                    secName = reader["SEC_NAME"].ToString(),
                                    facId = reader["FAC_ID"]?.ToString(),
                                    misCode = reader["MIS_CODE"].ToString(),
                                    programName = reader["ProgramName"].ToString(),
                                    courseName = reader["COURSE_NAME"].ToString(),
                                    facName = reader["FAC_NAME"]?.ToString()
                                };
                            }
                            reader.Close();

                            foreach (var sched in schedules)
                            {
                                using (var cmd2 = db.CreateCommand())
                                {
                                    cmd2.CommandType = CommandType.Text;
                                    cmd2.CommandText = @"
                                        SELECT SSD.DAY, SSD.START_TIME, SSD.END_TIME, SSD.ROOM
                                        FROM Subject_SCHEDULE_DETAILS SSD
                                        WHERE SSD.SCHED_ID = @schedId
                                        ORDER BY 
                                            CASE SSD.DAY
                                                WHEN 'Monday' THEN 1
                                                WHEN 'Tuesday' THEN 2
                                                WHEN 'Wednesday' THEN 3
                                                WHEN 'Thursday' THEN 4
                                                WHEN 'Friday' THEN 5
                                                WHEN 'Saturday' THEN 6
                                                WHEN 'Sunday' THEN 7
                                                ELSE 8
                                            END";
                                    cmd2.Parameters.AddWithValue("@schedId", sched.Key);

                                    var days = new List<string>();
                                    var startTimes = new List<string>();
                                    var endTimes = new List<string>();
                                    var rooms = new List<string>();

                                    using (var reader2 = cmd2.ExecuteReader())
                                    {
                                        while (reader2.Read())
                                        {
                                            string day = reader2["DAY"].ToString();
                                            TimeSpan startTime = (TimeSpan)reader2["START_TIME"];
                                            TimeSpan endTime = (TimeSpan)reader2["END_TIME"];

                                            int startHour = startTime.Hours;
                                            int startMinute = startTime.Minutes;
                                            string startPeriod = startHour >= 12 ? "PM" : "AM";
                                            startHour = startHour % 12 == 0 ? 12 : startHour % 12;
                                            string startTimeFormatted = $"{startHour}:{startMinute:D2} {startPeriod}";

                                            int endHour = endTime.Hours;
                                            int endMinute = endTime.Minutes;
                                            string endPeriod = endHour >= 12 ? "PM" : "AM";
                                            endHour = endHour % 12 == 0 ? 12 : endHour % 12;
                                            string endTimeFormatted = $"{endHour}:{endMinute:D2} {endPeriod}";

                                            days.Add(day);
                                            startTimes.Add(startTimeFormatted);
                                            endTimes.Add(endTimeFormatted);
                                            rooms.Add(reader2["ROOM"]?.ToString() ?? "TBD");
                                        }
                                    }

                                    data.Add(new
                                    {
                                        schedId = sched.Value.schedId,
                                        pcId = sched.Value.pcId,
                                        secId = sched.Value.secId,
                                        secName = sched.Value.secName,
                                        day = string.Join(", ", days),
                                        startTime = string.Join("; ", startTimes),
                                        endTime = string.Join("; ", endTimes),
                                        room = string.Join(", ", rooms),
                                        facId = sched.Value.facId,
                                        misCode = sched.Value.misCode,
                                        programName = sched.Value.programName,
                                        courseName = sched.Value.courseName,
                                        facName = sched.Value.facName ?? "TBD"
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = "Failed to fetch schedules: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ViewGroupedSchedules()
        {
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
                            SELECT SS.SCHED_ID, SS.SEC_ID, SS.MIS_CODE, S.SEC_NAME,
                                   P.NAME AS ProgramName, C.ID AS CourseId, C.COURSE_CODE, C.COURSE_NAME, C.UNITS
                            FROM Subject_SCHEDULES SS
                            INNER JOIN PROGRAMS_COURSE PC ON SS.PC_ID = PC.PC_ID
                            INNER JOIN PROGRAMS P ON PC.PROGRAM_ID = P.ID
                            INNER JOIN COURSES C ON PC.COURSE_ID = C.ID
                            INNER JOIN SECTION S ON SS.SEC_ID = S.SEC_ID
                            ORDER BY S.SEC_NAME";
                        using (var reader = cmd.ExecuteReader())
                        {
                            var schedules = new Dictionary<string, List<(int SchedId, int CourseId, string CourseCode, string CourseName, string Units, string MisCode)>>();
                            while (reader.Read())
                            {
                                string sectionName = reader["SEC_NAME"].ToString();
                                string programName = reader["ProgramName"].ToString();
                                string displaySection = $"{programName} {sectionName}";

                                if (!schedules.ContainsKey(displaySection))
                                {
                                    schedules[displaySection] = new List<(int, int, string, string, string, string)>();
                                }

                                schedules[displaySection].Add((
                                    Convert.ToInt32(reader["SCHED_ID"]),
                                    Convert.ToInt32(reader["CourseId"]),
                                    reader["COURSE_CODE"].ToString(),
                                    reader["COURSE_NAME"].ToString(),
                                    reader["UNITS"].ToString(),
                                    reader["MIS_CODE"].ToString()
                                ));
                            }
                            reader.Close();

                            foreach (var section in schedules)
                            {
                                groupedData[section.Key] = new List<object>();
                                var courseSchedules = new Dictionary<int, (string Subject, string Description, string Units, string MisCode, List<(string Time, string Day, string Room)>)>();

                                foreach (var sched in section.Value)
                                {
                                    if (!courseSchedules.ContainsKey(sched.CourseId))
                                    {
                                        courseSchedules[sched.CourseId] = (sched.CourseCode, sched.CourseName, sched.Units, sched.MisCode, new List<(string, string, string)>());
                                    }

                                    using (var cmd2 = db.CreateCommand())
                                    {
                                        cmd2.CommandType = CommandType.Text;
                                        cmd2.CommandText = @"
                                            SELECT SSD.DAY, SSD.START_TIME, SSD.END_TIME, SSD.ROOM
                                            FROM Subject_SCHEDULE_DETAILS SSD
                                            WHERE SSD.SCHED_ID = @schedId
                                            ORDER BY 
                                                CASE SSD.DAY
                                                    WHEN 'Monday' THEN 1
                                                    WHEN 'Tuesday' THEN 2
                                                    WHEN 'Wednesday' THEN 3
                                                    WHEN 'Thursday' THEN 4
                                                    WHEN 'Friday' THEN 5
                                                    WHEN 'Saturday' THEN 6
                                                    WHEN 'Sunday' THEN 7
                                                    ELSE 8
                                                END";
                                        cmd2.Parameters.AddWithValue("@schedId", sched.SchedId);

                                        using (var reader2 = cmd2.ExecuteReader())
                                        {
                                            while (reader2.Read())
                                            {
                                                TimeSpan startTime = (TimeSpan)reader2["START_TIME"];
                                                TimeSpan endTime = (TimeSpan)reader2["END_TIME"];

                                                int startHour = startTime.Hours;
                                                int startMinute = startTime.Minutes;
                                                string startPeriod = startHour >= 12 ? "PM" : "AM";
                                                startHour = startHour % 12 == 0 ? 12 : startHour % 12;
                                                string startTimeFormatted = $"{startHour}:{startMinute:D2} {startPeriod}";

                                                int endHour = endTime.Hours;
                                                int endMinute = endTime.Minutes;
                                                string endPeriod = endHour >= 12 ? "PM" : "AM";
                                                endHour = endHour % 12 == 0 ? 12 : endHour % 12;
                                                string endTimeFormatted = $"{endHour}:{endMinute:D2} {endPeriod}";

                                                string timeFormatted = $"{startTimeFormatted} - {endTimeFormatted}";

                                                courseSchedules[sched.CourseId].Item5.Add((
                                                    timeFormatted,
                                                    reader2["DAY"].ToString(),
                                                    reader2["ROOM"]?.ToString() ?? "TBD"
                                                ));
                                            }
                                        }
                                    }
                                }

                                foreach (var course in courseSchedules)
                                {
                                    groupedData[section.Key].Add(new
                                    {
                                        courseId = course.Key.ToString(),
                                        subject = course.Value.Subject,
                                        description = course.Value.Description,
                                        units = course.Value.Units,
                                        misCode = course.Value.MisCode,
                                        schedules = course.Value.Item5.Select(s => new
                                        {
                                            time = s.Time,
                                            day = s.Day,
                                            room = s.Room
                                        }).ToList()
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = "Failed to fetch grouped schedules: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(groupedData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSectionSchedules(string secId)
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
                        cmd.CommandText = @"
                            SELECT SS.SCHED_ID, SS.MIS_CODE, C.COURSE_NAME
                            FROM Subject_SCHEDULES SS
                            INNER JOIN PROGRAMS_COURSE PC ON SS.PC_ID = PC.PC_ID
                            INNER JOIN COURSES C ON PC.COURSE_ID = C.ID
                            WHERE SS.SEC_ID = @secId";
                        cmd.Parameters.AddWithValue("@secId", secId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            var schedules = new Dictionary<int, (string CourseName, string MisCode)>();
                            while (reader.Read())
                            {
                                schedules[Convert.ToInt32(reader["SCHED_ID"])] = (
                                    reader["COURSE_NAME"].ToString(),
                                    reader["MIS_CODE"].ToString()
                                );
                            }
                            reader.Close();

                            foreach (var sched in schedules)
                            {
                                using (var cmd2 = db.CreateCommand())
                                {
                                    cmd2.CommandType = CommandType.Text;
                                    cmd2.CommandText = @"
                                        SELECT SSD.DAY, SSD.START_TIME, SSD.END_TIME, SSD.ROOM
                                        FROM.Subject_SCHEDULE_DETAILS SSD
                                        WHERE SSD.SCHED_ID = @schedId
                                        ORDER BY 
                                            CASE SSD.DAY
                                                WHEN 'Monday' THEN 1
                                                WHEN 'Tuesday' THEN 2
                                                WHEN 'Wednesday' THEN 3
                                                WHEN 'Thursday' THEN 4
                                                WHEN 'Friday' THEN 5
                                                WHEN 'Saturday' THEN 6
                                                WHEN 'Sunday' THEN 7
                                                ELSE 8
                                            END";
                                    cmd2.Parameters.AddWithValue("@schedId", sched.Key);

                                    var days = new List<string>();
                                    var startTimes = new List<string>();
                                    var endTimes = new List<string>();
                                    var rooms = new List<string>();

                                    using (var reader2 = cmd2.ExecuteReader())
                                    {
                                        while (reader2.Read())
                                        {
                                            TimeSpan startTime = (TimeSpan)reader2["START_TIME"];
                                            TimeSpan endTime = (TimeSpan)reader2["END_TIME"];

                                            int startHour = startTime.Hours;
                                            int startMinute = startTime.Minutes;
                                            string startPeriod = startHour >= 12 ? "PM" : "AM";
                                            startHour = startHour % 12 == 0 ? 12 : startHour % 12;
                                            string startTimeFormatted = $"{startHour}:{startMinute:D2} {startPeriod}";

                                            int endHour = endTime.Hours;
                                            int endMinute = endTime.Minutes;
                                            string endPeriod = endHour >= 12 ? "PM" : "AM";
                                            endHour = endHour % 12 == 0 ? 12 : endHour % 12;
                                            string endTimeFormatted = $"{endHour}:{endMinute:D2} {endPeriod}";

                                            days.Add(reader2["DAY"].ToString());
                                            startTimes.Add(startTimeFormatted);
                                            endTimes.Add(endTimeFormatted);
                                            rooms.Add(reader2["ROOM"]?.ToString() ?? "TBD");
                                        }
                                    }

                                    data.Add(new
                                    {
                                        schedId = sched.Key.ToString(),
                                        courseName = sched.Value.CourseName,
                                        misCode = sched.Value.MisCode,
                                        day = string.Join(", ", days),
                                        startTime = string.Join("; ", startTimes),
                                        endTime = string.Join("; ", endTimes),
                                        room = string.Join(", " ,rooms)
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = "Failed to fetch section schedules: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPrograms()
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
            if (data.Count == 0)
            {
                return Json(new { error = "No programs found in the database" }, JsonRequestBehavior.AllowGet);
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCourses()
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
                        cmd.CommandText = "SELECT ID, COURSE_NAME FROM COURSES";
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                data.Add(new
                                {
                                    id = reader["ID"].ToString(),
                                    name = reader["COURSE_NAME"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = "Failed to fetch courses: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
            if (data.Count == 0)
            {
                return Json(new { error = "No courses found in the database" }, JsonRequestBehavior.AllowGet);
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSections()
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
                        cmd.CommandText = "SELECT SEC_ID, SEC_NAME FROM SECTION";
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                data.Add(new
                                {
                                    id = reader["SEC_ID"].ToString(),
                                    name = reader["SEC_NAME"].ToString()
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
            if (data.Count == 0)
            {
                return Json(new { error = "No sections found in the database" }, JsonRequestBehavior.AllowGet);
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetFaculty()
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
                        cmd.CommandText = "SELECT FAC_ID, [FAC_ NAME] AS FAC_NAME FROM FACULTY";
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                data.Add(new
                                {
                                    id = reader["FAC_ID"].ToString(),
                                    name = reader["FAC_NAME"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = "Failed to fetch faculty: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
            if (data.Count == 0)
            {
                return Json(new { error = "No faculty found in the database" }, JsonRequestBehavior.AllowGet);
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        private string GenerateMisCode()
        {
            return "MIS-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
        }

        public ActionResult AddSchedule()
        {
            var data = new List<object>();
            var programId = Request["programId"];
            var courseId = Request["courseId"];
            var secId = Request["secId"];
            var facId = Request["facId"];
            var scheduleDetailsJson = Request["scheduleDetails"];

            if (string.IsNullOrEmpty(programId) || string.IsNullOrEmpty(courseId) || string.IsNullOrEmpty(secId) || string.IsNullOrEmpty(scheduleDetailsJson))
            {
                data.Add(new { mess = 1, error = "Required fields (program, course, section, schedule details) are missing." });
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            var scheduleDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ScheduleDetail>>(scheduleDetailsJson);
            if (scheduleDetails == null || scheduleDetails.Count == 0)
            {
                data.Add(new { mess = 1, error = "At least one schedule detail is required." });
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            foreach (var detail in scheduleDetails)
            {
                if (string.IsNullOrEmpty(detail.Day) || string.IsNullOrEmpty(detail.StartTime) || string.IsNullOrEmpty(detail.EndTime))
                {
                    data.Add(new { mess = 1, error = "Day, start time, and end time are required." });
                    return Json(data, JsonRequestBehavior.AllowGet);
                }

                if (!TimeSpan.TryParse(detail.StartTime, out TimeSpan start) || !TimeSpan.TryParse(detail.EndTime, out TimeSpan end))
                {
                    data.Add(new { mess = 1, error = $"Invalid time format for schedule on {detail.Day}." });
                    return Json(data, JsonRequestBehavior.AllowGet);
                }

                if (end <= start)
                {
                    data.Add(new { mess = 1, error = $"End time must be after start time for schedule on {detail.Day}." });
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
            }

            // Check for overlaps within the new schedule details
            for (int i = 0; i < scheduleDetails.Count; i++)
            {
                for (int j = i + 1; j < scheduleDetails.Count; j++)
                {
                    var detail1 = scheduleDetails[i];
                    var detail2 = scheduleDetails[j];

                    if (detail1.Day != detail2.Day) continue;

                    TimeSpan start1 = TimeSpan.Parse(detail1.StartTime);
                    TimeSpan end1 = TimeSpan.Parse(detail1.EndTime);
                    TimeSpan start2 = TimeSpan.Parse(detail2.StartTime);
                    TimeSpan end2 = TimeSpan.Parse(detail2.EndTime);

                    if (start1 < end2 && start2 < end1)
                    {
                        data.Add(new { mess = 1, error = $"Schedule details overlap on {detail1.Day} between {detail1.StartTime}-{detail1.EndTime} and {detail2.StartTime}-{detail2.EndTime}." });
                        return Json(data, JsonRequestBehavior.AllowGet);
                    }
                }
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
                            int pcId = 0;
                            using (var cmd = db.CreateCommand())
                            {
                                cmd.Transaction = transaction;
                                cmd.CommandType = CommandType.Text;
                                cmd.CommandText = "SELECT PC_ID FROM PROGRAMS_COURSE WHERE PROGRAM_ID = @programId AND COURSE_ID = @courseId";
                                cmd.Parameters.AddWithValue("@programId", programId);
                                cmd.Parameters.AddWithValue("@courseId", courseId);
                                var result = cmd.ExecuteScalar();
                                if (result != null)
                                {
                                    pcId = Convert.ToInt32(result);
                                }
                                else
                                {
                                    data.Add(new { mess = 1, error = "Invalid Program or Course selection" });
                                    return Json(data, JsonRequestBehavior.AllowGet);
                                }
                            }

                            double totalScheduledHours = 0;
                            using (var cmd = db.CreateCommand())
                            {
                                cmd.Transaction = transaction;
                                cmd.CommandType = CommandType.Text;
                                cmd.CommandText = @"
                                    SELECT SSD.START_TIME, SSD.END_TIME
                                    FROM Subject_SCHEDULES SS
                                    INNER JOIN Subject_SCHEDULE_DETAILS SSD ON SS.SCHED_ID = SSD.SCHED_ID
                                    WHERE SS.PC_ID = @pcId AND SS.SEC_ID = @secId";
                                cmd.Parameters.AddWithValue("@pcId", pcId);
                                cmd.Parameters.AddWithValue("@secId", secId);
                                using (var reader = cmd.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        TimeSpan existingStart = (TimeSpan)reader["START_TIME"];
                                        TimeSpan existingEnd = (TimeSpan)reader["END_TIME"];
                                        totalScheduledHours += (existingEnd - existingStart).TotalHours;
                                    }
                                }
                            }

                            double newScheduledHours = 0;
                            foreach (var detail in scheduleDetails)
                            {
                                TimeSpan start = TimeSpan.Parse(detail.StartTime);
                                TimeSpan end = TimeSpan.Parse(detail.EndTime);
                                newScheduledHours += (end - start).TotalHours;
                            }

                            double courseTotalHours = 0;
                            using (var cmd = db.CreateCommand())
                            {
                                cmd.Transaction = transaction;
                                cmd.CommandType = CommandType.Text;
                                cmd.CommandText = "SELECT TOTAL_HOURS FROM COURSES WHERE ID = @courseId";
                                cmd.Parameters.AddWithValue("@courseId", courseId);
                                var result = cmd.ExecuteScalar();
                                if (result != null)
                                {
                                    courseTotalHours = Convert.ToDouble(result);
                                }
                                else
                                {
                                    data.Add(new { mess = 1, error = "Course not found" });
                                    return Json(data, JsonRequestBehavior.AllowGet);
                                }
                            }

                            if (totalScheduledHours + newScheduledHours > courseTotalHours)
                            {
                                data.Add(new { mess = 1, error = $"Adding these schedules would exceed course hours limit." });
                                return Json(data, JsonRequestBehavior.AllowGet);
                            }

                            foreach (var detail in scheduleDetails)
                            {
                                using (var cmd = db.CreateCommand())
                                {
                                    cmd.Transaction = transaction;
                                    cmd.CommandType = CommandType.Text;
                                    cmd.CommandText = @"
                                        SELECT COUNT(*)
                                        FROM Subject_SCHEDULES SS
                                        INNER JOIN Subject_SCHEDULE_DETAILS SSD ON SS.SCHED_ID = SSD.SCHED_ID
                                        WHERE SS.SEC_ID = @secId
                                        AND SSD.DAY = @day
                                        AND (
                                            (CAST(@startTime AS TIME) < SSD.END_TIME AND CAST(@endTime AS TIME) > SSD.START_TIME)
                                        )";
                                    cmd.Parameters.AddWithValue("@secId", secId);
                                    cmd.Parameters.AddWithValue("@day", detail.Day);
                                    cmd.Parameters.AddWithValue("@startTime", detail.StartTime);
                                    cmd.Parameters.AddWithValue("@endTime", detail.EndTime);
                                    int overlapCount = Convert.ToInt32(cmd.ExecuteScalar());
                                    if (overlapCount > 0)
                                    {
                                        data.Add(new { mess = 1, error = $"Section schedule conflict on {detail.Day} from {detail.StartTime} to {detail.EndTime}." });
                                        return Json(data, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }

                            // Only check for faculty conflicts if a faculty is assigned
                            if (!string.IsNullOrEmpty(facId) && facId != "null")
                            {
                                foreach (var detail in scheduleDetails)
                                {
                                    using (var cmd = db.CreateCommand())
                                    {
                                        cmd.Transaction = transaction;
                                        cmd.CommandType = CommandType.Text;
                                        cmd.CommandText = @"
                                            SELECT COUNT(*)
                                            FROM Subject_SCHEDULES SS
                                            INNER JOIN Subject_SCHEDULE_DETAILS SSD ON SS.SCHED_ID = SSD.SCHED_ID
                                            WHERE SS.FAC_ID = @facId
                                            AND SSD.DAY = @day
                                            AND (
                                                (CAST(@startTime AS TIME) < SSD.END_TIME AND CAST(@endTime AS TIME) > SSD.START_TIME)
                                            )";
                                        cmd.Parameters.AddWithValue("@facId", facId);
                                        cmd.Parameters.AddWithValue("@day", detail.Day);
                                        cmd.Parameters.AddWithValue("@startTime", detail.StartTime);
                                        cmd.Parameters.AddWithValue("@endTime", detail.EndTime);
                                        int overlapCount = Convert.ToInt32(cmd.ExecuteScalar());
                                        if (overlapCount > 0)
                                        {
                                            data.Add(new { mess = 1, error = $"Faculty schedule conflict on {detail.Day} from {detail.StartTime} to {detail.EndTime}." });
                                            return Json(data, JsonRequestBehavior.AllowGet);
                                        }
                                    }
                                }
                            }

                            // Only check for room conflicts if a room is specified
                            foreach (var detail in scheduleDetails)
                            {
                                if (!string.IsNullOrEmpty(detail.Room) && detail.Room != "null")
                                {
                                    using (var cmd = db.CreateCommand())
                                    {
                                        cmd.Transaction = transaction;
                                        cmd.CommandType = CommandType.Text;
                                        cmd.CommandText = @"
                                            SELECT COUNT(*)
                                            FROM Subject_SCHEDULES SS
                                            INNER JOIN Subject_SCHEDULE_DETAILS SSD ON SS.SCHED_ID = SSD.SCHED_ID
                                            WHERE SSD.ROOM = @room
                                            AND SSD.DAY = @day
                                            AND (
                                                (CAST(@startTime AS TIME) < SSD.END_TIME AND CAST(@endTime AS TIME) > SSD.START_TIME)
                                            )";
                                        cmd.Parameters.AddWithValue("@room", detail.Room);
                                        cmd.Parameters.AddWithValue("@day", detail.Day);
                                        cmd.Parameters.AddWithValue("@startTime", detail.StartTime);
                                        cmd.Parameters.AddWithValue("@endTime", detail.EndTime);
                                        int overlapCount = Convert.ToInt32(cmd.ExecuteScalar());
                                        if (overlapCount > 0)
                                        {
                                            data.Add(new { mess = 1, error = $"Room {detail.Room} is already booked on {detail.Day} from {detail.StartTime} to {detail.EndTime}." });
                                            return Json(data, JsonRequestBehavior.AllowGet);
                                        }
                                    }
                                }
                            }

                            string misCode = GenerateMisCode();
                            int schedId;
                            using (var cmd = db.CreateCommand())
                            {
                                cmd.Transaction = transaction;
                                cmd.CommandType = CommandType.Text;
                                cmd.CommandText = @"
                                    INSERT INTO Subject_SCHEDULES (PC_ID, SEC_ID, FAC_ID, MIS_CODE) 
                                    VALUES (@pcId, @secId, @facId, @misCode); 
                                    SELECT SCOPE_IDENTITY();";
                                cmd.Parameters.AddWithValue("@pcId", pcId);
                                cmd.Parameters.AddWithValue("@secId", secId);
                                cmd.Parameters.AddWithValue("@facId", (string.IsNullOrEmpty(facId) || facId == "null") ? DBNull.Value : (object)facId);
                                cmd.Parameters.AddWithValue("@misCode", misCode);
                                schedId = Convert.ToInt32(cmd.ExecuteScalar());
                            }

                            foreach (var detail in scheduleDetails)
                            {
                                using (var cmd = db.CreateCommand())
                                {
                                    cmd.Transaction = transaction;
                                    cmd.CommandType = CommandType.Text;
                                    cmd.CommandText = @"
                                        INSERT INTO Subject_SCHEDULE_DETAILS (SCHED_ID, DAY, START_TIME, END_TIME, ROOM) 
                                        VALUES (@schedId, @day, @startTime, @endTime, @room)";
                                    cmd.Parameters.AddWithValue("@schedId", schedId);
                                    cmd.Parameters.AddWithValue("@day", detail.Day);
                                    cmd.Parameters.AddWithValue("@startTime", detail.StartTime);
                                    cmd.Parameters.AddWithValue("@endTime", detail.EndTime);
                                    cmd.Parameters.AddWithValue("@room", (string.IsNullOrEmpty(detail.Room) || detail.Room == "null") ? DBNull.Value : (object)detail.Room);
                                    cmd.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();
                            data.Add(new { mess = 0, schedId = schedId.ToString(), misCode });
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            data.Add(new { mess = 1, error = "Error adding schedule: " + ex.Message });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                data.Add(new { mess = 1, error = "Error adding schedule: " + ex.Message });
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public class ScheduleDetail
        {
            public string Day { get; set; }
            public string StartTime { get; set; }
            public string EndTime { get; set; }
            public string Room { get; set; }
        }

        public ActionResult UpdateSchedule()
        {
            var data = new List<object>();
            if (!int.TryParse(Request["schedId"], out int schedId))
            {
                data.Add(new { mess = 1, error = "Invalid schedule identifier" });
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            var facId = Request["facId"];
            var scheduleDetailsJson = Request["scheduleDetails"];

            if (string.IsNullOrEmpty(scheduleDetailsJson))
            {
                data.Add(new { mess = 1, error = "Schedule details are required." });
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            var scheduleDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ScheduleDetail>>(scheduleDetailsJson);
            if (scheduleDetails == null || scheduleDetails.Count == 0)
            {
                data.Add(new { mess = 1, error = "At least one schedule detail is required." });
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            foreach (var detail in scheduleDetails)
            {
                if (string.IsNullOrEmpty(detail.Day) || string.IsNullOrEmpty(detail.StartTime) || string.IsNullOrEmpty(detail.EndTime))
                {
                    data.Add(new { mess = 1, error = "Day, start time, and end time are required." });
                    return Json(data, JsonRequestBehavior.AllowGet);
                }

                if (!TimeSpan.TryParse(detail.StartTime, out TimeSpan start) || !TimeSpan.TryParse(detail.EndTime, out TimeSpan end))
                {
                    data.Add(new { mess = 1, error = $"Invalid time format for schedule on {detail.Day}." });
                    return Json(data, JsonRequestBehavior.AllowGet);
                }

                if (end <= start)
                {
                    data.Add(new { mess = 1, error = $"End time must be after start time for schedule on {detail.Day}." });
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
            }

            // Check for overlaps within the updated schedule details
            for (int i = 0; i < scheduleDetails.Count; i++)
            {
                for (int j = i + 1; j < scheduleDetails.Count; j++)
                {
                    var detail1 = scheduleDetails[i];
                    var detail2 = scheduleDetails[j];

                    if (detail1.Day != detail2.Day) continue;

                    TimeSpan start1 = TimeSpan.Parse(detail1.StartTime);
                    TimeSpan end1 = TimeSpan.Parse(detail1.EndTime);
                    TimeSpan start2 = TimeSpan.Parse(detail2.StartTime);
                    TimeSpan end2 = TimeSpan.Parse(detail2.EndTime);

                    if (start1 < end2 && start2 < end1)
                    {
                        data.Add(new { mess = 1, error = $"Schedule details overlap on {detail1.Day} between {detail1.StartTime}-{detail1.EndTime} and {detail2.StartTime}-{detail2.EndTime}." });
                        return Json(data, JsonRequestBehavior.AllowGet);
                    }
                }
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
                            int pcId = 0, secId = 0;
                            using (var cmd = db.CreateCommand())
                            {
                                cmd.Transaction = transaction;
                                cmd.CommandType = CommandType.Text;
                                cmd.CommandText = "SELECT PC_ID, SEC_ID FROM Subject_SCHEDULES WHERE SCHED_ID = @schedId";
                                cmd.Parameters.AddWithValue("@schedId", schedId);
                                using (var reader = cmd.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        pcId = Convert.ToInt32(reader["PC_ID"]);
                                        secId = Convert.ToInt32(reader["SEC_ID"]);
                                    }
                                    else
                                    {
                                        data.Add(new { mess = 1, error = "Schedule not found" });
                                        return Json(data, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }

                            double totalScheduledHours = 0;
                            using (var cmd = db.CreateCommand())
                            {
                                cmd.Transaction = transaction;
                                cmd.CommandType = CommandType.Text;
                                cmd.CommandText = @"
                                    SELECT SSD.START_TIME, SSD.END_TIME
                                    FROM Subject_SCHEDULES SS
                                    INNER JOIN Subject_SCHEDULE_DETAILS SSD ON SS.SCHED_ID = SSD.SCHED_ID
                                    WHERE SS.PC_ID = @pcId AND SS.SEC_ID = @secId AND SS.SCHED_ID != @schedId";
                                cmd.Parameters.AddWithValue("@pcId", pcId);
                                cmd.Parameters.AddWithValue("@secId", secId);
                                cmd.Parameters.AddWithValue("@schedId", schedId);
                                using (var reader = cmd.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        TimeSpan start = (TimeSpan)reader["START_TIME"];
                                        TimeSpan end = (TimeSpan)reader["END_TIME"];
                                        totalScheduledHours += (end - start).TotalHours;
                                    }
                                }
                            }

                            double newScheduledHours = 0;
                            foreach (var detail in scheduleDetails)
                            {
                                TimeSpan start = TimeSpan.Parse(detail.StartTime);
                                TimeSpan end = TimeSpan.Parse(detail.EndTime);
                                newScheduledHours += (end - start).TotalHours;
                            }

                            double courseTotalHours = 0;
                            using (var cmd = db.CreateCommand())
                            {
                                cmd.Transaction = transaction;
                                cmd.CommandType = CommandType.Text;
                                cmd.CommandText = @"
                                    SELECT C.TOTAL_HOURS
                                    FROM COURSES C
                                    INNER JOIN PROGRAMS_COURSE PC ON C.ID = PC.COURSE_ID
                                    WHERE PC.PC_ID = @pcId";
                                cmd.Parameters.AddWithValue("@pcId", pcId);
                                var result = cmd.ExecuteScalar();
                                if (result != null)
                                {
                                    courseTotalHours = Convert.ToDouble(result);
                                }
                                else
                                {
                                    data.Add(new { mess = 1, error = "Course not found" });
                                    return Json(data, JsonRequestBehavior.AllowGet);
                                }
                            }

                            if (totalScheduledHours + newScheduledHours > courseTotalHours)
                            {
                                data.Add(new { mess = 1, error = $"Total scheduled hours exceed course limit." });
                                return Json(data, JsonRequestBehavior.AllowGet);
                            }

                            foreach (var detail in scheduleDetails)
                            {
                                using (var cmd = db.CreateCommand())
                                {
                                    cmd.Transaction = transaction;
                                    cmd.CommandType = CommandType.Text;
                                    cmd.CommandText = @"
                                        SELECT COUNT(*)
                                        FROM Subject_SCHEDULES SS
                                        INNER JOIN Subject_SCHEDULE_DETAILS SSD ON SS.SCHED_ID = SSD.SCHED_ID
                                        WHERE SS.SEC_ID = @secId
                                        AND SSD.DAY = @day
                                        AND SS.SCHED_ID != @schedId
                                        AND (
                                            (CAST(@startTime AS TIME) < SSD.END_TIME AND CAST(@endTime AS TIME) > SSD.START_TIME)
                                        )";
                                    cmd.Parameters.AddWithValue("@secId", secId);
                                    cmd.Parameters.AddWithValue("@day", detail.Day);
                                    cmd.Parameters.AddWithValue("@startTime", detail.StartTime);
                                    cmd.Parameters.AddWithValue("@endTime", detail.EndTime);
                                    cmd.Parameters.AddWithValue("@schedId", schedId);
                                    int overlapCount = Convert.ToInt32(cmd.ExecuteScalar());
                                    if (overlapCount > 0)
                                    {
                                        data.Add(new { mess = 1, error = $"Section schedule conflict on {detail.Day} from {detail.StartTime} to {detail.EndTime}." });
                                        return Json(data, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }

                            // Only check for faculty conflicts if a faculty is assigned
                            if (!string.IsNullOrEmpty(facId) && facId != "null")
                            {
                                foreach (var detail in scheduleDetails)
                                {
                                    using (var cmd = db.CreateCommand())
                                    {
                                        cmd.Transaction = transaction;
                                        cmd.CommandType = CommandType.Text;
                                        cmd.CommandText = @"
                                            SELECT COUNT(*)
                                            FROM Subject_SCHEDULES SS
                                            INNER JOIN Subject_SCHEDULE_DETAILS SSD ON SS.SCHED_ID = SSD.SCHED_ID
                                            WHERE SS.FAC_ID = @facId
                                            AND SSD.DAY = @day
                                            AND SS.SCHED_ID != @schedId
                                            AND (
                                                (CAST(@startTime AS TIME) < SSD.END_TIME AND CAST(@endTime AS TIME) > SSD.START_TIME)
                                            )";
                                        cmd.Parameters.AddWithValue("@facId", facId);
                                        cmd.Parameters.AddWithValue("@day", detail.Day);
                                        cmd.Parameters.AddWithValue("@startTime", detail.StartTime);
                                        cmd.Parameters.AddWithValue("@endTime", detail.EndTime);
                                        cmd.Parameters.AddWithValue("@schedId", schedId);
                                        int overlapCount = Convert.ToInt32(cmd.ExecuteScalar());
                                         if (overlapCount > 0)
                                        {
                                            data.Add(new { mess = 1, error = $"Faculty schedule conflict on {detail.Day} from {detail.StartTime} to {detail.EndTime}." });
                                            return Json(data, JsonRequestBehavior.AllowGet);
                                        }
                                    }
                                }
                            }

                            // Only check for room conflicts if a room is specified
                            foreach (var detail in scheduleDetails)
                            {
                                if (!string.IsNullOrEmpty(detail.Room) && detail.Room != "null")
                                {
                                    using (var cmd = db.CreateCommand())
                                    {
                                        cmd.Transaction = transaction;
                                        cmd.CommandType = CommandType.Text;
                                        cmd.CommandText = @"
                                            SELECT COUNT(*)
                                            FROM Subject_SCHEDULES SS
                                            INNER JOIN Subject_SCHEDULE_DETAILS SSD ON SS.SCHED_ID = SSD.SCHED_ID
                                            WHERE SSD.ROOM = @room
                                            AND SSD.DAY = @day
                                            AND SS.SCHED_ID != @schedId
                                            AND (
                                                (CAST(@startTime AS TIME) < SSD.END_TIME AND CAST(@endTime AS TIME) > SSD.START_TIME)
                                            )";
                                        cmd.Parameters.AddWithValue("@room", detail.Room);
                                        cmd.Parameters.AddWithValue("@day", detail.Day);
                                        cmd.Parameters.AddWithValue("@startTime", detail.StartTime);
                                        cmd.Parameters.AddWithValue("@endTime", detail.EndTime);
                                        cmd.Parameters.AddWithValue("@schedId", schedId);
                                        int overlapCount = Convert.ToInt32(cmd.ExecuteScalar());
                                        if (overlapCount > 0)
                                        {
                                            data.Add(new { mess = 1, error = $"Room {detail.Room} is already booked on {detail.Day} from {detail.StartTime} to {detail.EndTime}." });
                                            return Json(data, JsonRequestBehavior.AllowGet);
                                        }
                                    }
                                }
                            }

                            using (var cmd = db.CreateCommand())
                            {
                                cmd.Transaction = transaction;
                                cmd.CommandType = CommandType.Text;
                                cmd.CommandText = "UPDATE Subject_SCHEDULES SET FAC_ID = @facId WHERE SCHED_ID = @schedId";
                                cmd.Parameters.AddWithValue("@facId", (string.IsNullOrEmpty(facId) || facId == "null") ? DBNull.Value : (object)facId);
                                cmd.Parameters.AddWithValue("@schedId", schedId);
                                cmd.ExecuteNonQuery();
                            }

                            using (var cmd = db.CreateCommand())
                            {
                                cmd.Transaction = transaction;
                                cmd.CommandType = CommandType.Text;
                                cmd.CommandText = "DELETE FROM Subject_SCHEDULE_DETAILS WHERE SCHED_ID = @schedId";
                                cmd.Parameters.AddWithValue("@schedId", schedId);
                                cmd.ExecuteNonQuery();
                            }

                            foreach (var detail in scheduleDetails)
                            {
                                using (var cmd = db.CreateCommand())
                                {
                                    cmd.Transaction = transaction;
                                    cmd.CommandType = CommandType.Text;
                                    cmd.CommandText = @"
                                        INSERT INTO Subject_SCHEDULE_DETAILS (SCHED_ID, DAY, START_TIME, END_TIME, ROOM) 
                                        VALUES (@schedId, @day, @startTime, @endTime, @room)";
                                    cmd.Parameters.AddWithValue("@schedId", schedId);
                                    cmd.Parameters.AddWithValue("@day", detail.Day);
                                    cmd.Parameters.AddWithValue("@startTime", detail.StartTime);
                                    cmd.Parameters.AddWithValue("@endTime", detail.EndTime);
                                    cmd.Parameters.AddWithValue("@room", (string.IsNullOrEmpty(detail.Room) || detail.Room == "null") ? DBNull.Value : (object)detail.Room);
                                    cmd.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();
                            data.Add(new { mess = 0 });
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            data.Add(new { mess = 1, error = "Error updating schedule: " + ex.Message });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                data.Add(new { mess = 1, error = "Error updating schedule: " + ex.Message });
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteSchedule()
        {
            var data = new List<object>();
            if (!int.TryParse(Request["schedId"], out int schedId))
            {
                data.Add(new { mess = 1, error = "Invalid schedule identifier" });
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            try
            {
                using (var db = new SqlConnection(connStr))
                {
                    db.Open();
                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "DELETE FROM Subject_SCHEDULES WHERE SCHED_ID = @schedId";
                        cmd.Parameters.AddWithValue("@schedId", schedId);
                        var ctr = cmd.ExecuteNonQuery();
                        if (ctr >= 1)
                        {
                            data.Add(new { mess = 0 });
                        }
                        else
                        {
                            data.Add(new { mess = 1, error = "Failed to delete schedule" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                data.Add(new { mess = 1, error = "Error deleting schedule: " + ex.Message });
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}