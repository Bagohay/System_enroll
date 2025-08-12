
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace System_enroll.Controllers
{
    public class SectionController : Controller
    {
        string connStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Kent\source\repos\System_enroll\System_enroll\App_Data\StudentEntry.mdf;Integrated Security=True";

        public ActionResult Display_Section()
        {
            if (Session["UserNumber"] == null)
            {
                return RedirectToAction("Login_Page", "Home");
            }
            return View();
        }

        public ActionResult Get_Sections()
        {
            var sections = new List<object>();
            using (var db = new SqlConnection(connStr))
            {
                db.Open();
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"
                        SELECT s.SEC_ID, s.SEC_NAME, s.PROG_ID, p.NAME AS PROGRAM_NAME
                        FROM SECTION s
                        JOIN PROGRAMS p ON s.PROG_ID = p.ID";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            sections.Add(new
                            {
                                secId = reader["SEC_ID"].ToString(),
                                secName = reader["SEC_NAME"].ToString(),
                                progId = reader["PROG_ID"].ToString(),
                                programName = reader["PROGRAM_NAME"].ToString()
                            });
                        }
                    }
                }
            }
            return Json(sections, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Get_Programs()
        {
            var programs = new List<object>();
            using (var db = new SqlConnection(connStr))
            {
                db.Open();
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "SELECT ID, NAME FROM PROGRAMS ORDER BY NAME";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            programs.Add(new
                            {
                                id = reader["ID"].ToString(),
                                name = reader["NAME"].ToString()
                            });
                        }
                    }
                }
            }
            return Json(programs, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Add_Section()
        {
            string sectionName = Request["sectionName"];
            string programId = Request["programId"];

            if (string.IsNullOrEmpty(sectionName) || string.IsNullOrEmpty(programId))
            {
                return Json(new { success = false, message = "Section name and program are required." }, JsonRequestBehavior.AllowGet);
            }

            using (var db = new SqlConnection(connStr))
            {
                db.Open();
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"
                        INSERT INTO SECTION (SEC_NAME, PROG_ID)
                        VALUES (@secName, @progId)";
                    cmd.Parameters.AddWithValue("@secName", sectionName);
                    cmd.Parameters.AddWithValue("@progId", int.Parse(programId));

                    int rowsAffected = cmd.ExecuteNonQuery();
                    return Json(new
                    {
                        success = rowsAffected > 0,
                        message = rowsAffected > 0 ? "Section added successfully." : "Failed to add section."
                    }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult Update_Section()
        {
            string sectionId = Request["sectionId"];
            string sectionName = Request["sectionName"];
            string programId = Request["programId"];

            if (string.IsNullOrEmpty(sectionId) || string.IsNullOrEmpty(sectionName) || string.IsNullOrEmpty(programId))
            {
                return Json(new { success = false, message = "Section ID, name, and program are required." }, JsonRequestBehavior.AllowGet);
            }

            using (var db = new SqlConnection(connStr))
            {
                db.Open();
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"
                        UPDATE SECTION 
                        SET SEC_NAME = @secName, PROG_ID = @progId
                        WHERE SEC_ID = @secId";
                    cmd.Parameters.AddWithValue("@secName", sectionName);
                    cmd.Parameters.AddWithValue("@progId", int.Parse(programId));
                    cmd.Parameters.AddWithValue("@secId", int.Parse(sectionId));

                    int rowsAffected = cmd.ExecuteNonQuery();
                    return Json(new
                    {
                        success = rowsAffected > 0,
                        message = rowsAffected > 0 ? "Section updated successfully." : "Failed to update section."
                    }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult Delete_Section()
        {
            string sectionId = Request["sectionId"];

            if (string.IsNullOrEmpty(sectionId))
            {
                return Json(new { success = false, message = "Section ID is required." }, JsonRequestBehavior.AllowGet);
            }

            using (var db = new SqlConnection(connStr))
            {
                db.Open();
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"
                        DELETE FROM SECTION 
                        WHERE SEC_ID = @secId";
                    cmd.Parameters.AddWithValue("@secId", int.Parse(sectionId));

                    int rowsAffected = cmd.ExecuteNonQuery();
                    return Json(new
                    {
                        success = rowsAffected > 0,
                        message = rowsAffected > 0 ? "Section deleted successfully." : "Failed to delete section."
                    }, JsonRequestBehavior.AllowGet);
                }
            }
        }
    }
}