using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
//THIS IS FOR REVISES PURPOSE ONLY TO AVOID ANY FURTHER HARD DUBBING OF CODE
namespace System_enroll.Controllers
{
    public class RevisesController : Controller
    {
        string connStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Kent\source\repos\System_enroll\System_enroll\App_Data\StudentEntry.mdf;Integrated Security=True";

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
                            SELECT SS.SCHED_ID, SS.PC_ID, SS.SEC_ID, S.SEC_NAME, SS.DAY, SS.START_TIME, SS.END_TIME, SS.ROOM, SS.FAC_ID,
                                   P.NAME AS ProgramName, C.COURSE_NAME, F.[FAC_ NAME] AS FAC_NAME
                            FROM SUBJECT_SCHEDULES SS
                            INNER JOIN PROGRAMS_COURSE PC ON SS.PC_ID = PC.PC_ID
                            INNER JOIN PROGRAMS P ON PC.PROGRAM_ID = P.ID
                            INNER JOIN COURSES C ON PC.COURSE_ID = C.ID
                            INNER JOIN FACULTY F ON SS.FAC_ID = F.FAC_ID
                            INNER JOIN SECTION S ON SS.SEC_ID = S.SEC_ID";
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            data.Add(new
                            {
                                schedId = reader["SCHED_ID"].ToString(),
                                pcId = reader["PC_ID"].ToString(),
                                secId = reader["SEC_ID"].ToString(),
                                secName = reader["SEC_NAME"].ToString(),
                                day = reader["DAY"].ToString(),
                                startTime = reader["START_TIME"].ToString(),
                                endTime = reader["END_TIME"].ToString(),
                                room = reader["ROOM"].ToString(),
                                facId = reader["FAC_ID"].ToString(),
                                programName = reader["ProgramName"].ToString(),
                                courseName = reader["COURSE_NAME"].ToString(),
                                facName = reader["FAC_NAME"].ToString()
                            });
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
                        SqlDataReader reader = cmd.ExecuteReader();
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
                        SqlDataReader reader = cmd.ExecuteReader();
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
                        SqlDataReader reader = cmd.ExecuteReader();
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
                        SqlDataReader reader = cmd.ExecuteReader();
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

        public ActionResult AddSchedule()
        {
            var data = new List<object>();
            var programId = Request["programId"];
            var courseId = Request["courseId"];
            var secId = Request["secId"];
            var day = Request["DAY"];
            var startTime = Request["startTime"];
            var endTime = Request["endTime"];
            var room = Request["room"];
            var facId = Request["facId"];

            // Log all received parameters for debugging
            System.Diagnostics.Debug.WriteLine($"AddSchedule received: programId={programId}, courseId={courseId}, secId={secId}, day={day}, startTime={startTime}, endTime={endTime}, room={room}, facId={facId}");

            // Validate day
            if (string.IsNullOrEmpty(day))
            {
                data.Add(new { mess = 1, error = "Day is empty. Please select a day." });
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            // Find PC_ID from PROGRAMS_COURSE
            int pcId = 0;
            try
            {
                using (var db = new SqlConnection(connStr))
                {
                    db.Open();
                    using (var cmd = db.CreateCommand())
                    {
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

                    // Insert into SUBJECT_SCHEDULES
                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "INSERT INTO SUBJECT_SCHEDULES (PC_ID, SEC_ID, DAY, START_TIME, END_TIME, ROOM, FAC_ID) " +
                                          "VALUES (@pcId, @secId, @day, @startTime, @endTime, @room, @facId); " +
                                          "SELECT SCOPE_IDENTITY();";
                        cmd.Parameters.AddWithValue("@pcId", pcId);
                        cmd.Parameters.AddWithValue("@secId", secId);
                        cmd.Parameters.AddWithValue("@day", day);
                        cmd.Parameters.AddWithValue("@startTime", startTime);
                        cmd.Parameters.AddWithValue("@endTime", endTime);
                        cmd.Parameters.AddWithValue("@room", room);
                        cmd.Parameters.AddWithValue("@facId", facId);
                        var schedId = cmd.ExecuteScalar();
                        if (schedId != null)
                        {
                            data.Add(new { mess = 0, schedId = schedId.ToString() });
                        }
                        else
                        {
                            data.Add(new { mess = 1, error = "Failed to insert schedule" });
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

        public ActionResult UpdateSchedule()
        {
            var data = new List<object>();

            int schedId;
            if (!int.TryParse(Request["schedId"], out schedId))
            {
                data.Add(new { mess = 1, error = "Invalid schedule identifier" });
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            var day = Request["DAY"];
            var startTime = Request["startTime"];
            var endTime = Request["endTime"];
            var room = Request["room"];
            var facId = Request["facId"];

            // Log all received parameters for debugging
            System.Diagnostics.Debug.WriteLine($"UpdateSchedule received: schedId={schedId}, day={day}, startTime={startTime}, endTime={endTime}, room={room}, facId={facId}");

            // Validate day
            if (string.IsNullOrEmpty(day))
            {
                data.Add(new { mess = 1, error = "Day is empty. Please select a day." });
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
                        cmd.CommandText = "UPDATE SUBJECT_SCHEDULES SET DAY = @day, START_TIME = @startTime, END_TIME = @endTime, ROOM = @room, FAC_ID = @facId " +
                                          "WHERE SCHED_ID = @schedId";
                        cmd.Parameters.AddWithValue("@day", day);
                        cmd.Parameters.AddWithValue("@startTime", startTime);
                        cmd.Parameters.AddWithValue("@endTime", endTime);
                        cmd.Parameters.AddWithValue("@room", room);
                        cmd.Parameters.AddWithValue("@facId", facId);
                        cmd.Parameters.AddWithValue("@schedId", schedId);
                        var ctr = cmd.ExecuteNonQuery();
                        if (ctr >= 1)
                        {
                            data.Add(new { mess = 0 });
                        }
                        else
                        {
                            data.Add(new { mess = 1, error = "Failed to update schedule" });
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

            int schedId;
            if (!int.TryParse(Request["schedId"], out schedId))
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
                        cmd.CommandText = "DELETE FROM SUBJECT_SCHEDULES WHERE SCHED_ID = @schedId";
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