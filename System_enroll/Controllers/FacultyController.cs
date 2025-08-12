using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace System_enroll.Controllers
{
    public class FacultyController : Controller
    {
        string connStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Kent\source\repos\System_enroll\System_enroll\App_Data\StudentEntry.mdf;Integrated Security=True";

        public ActionResult Display_Faculties()
        {
            return View();
        }

        public ActionResult View_Faculties()
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
                        cmd.CommandText = "SELECT * FROM FACULTY";
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            data.Add(new
                            {
                                facId = reader["FAC_ID"].ToString(),
                                facName = reader["FAC_ NAME"].ToString(),
                                facSpecialization = reader["FAC_SPEACIALIZATION"].ToString(),
                                facContactInfo = reader["FAC_CONTACT_INFO"].ToString(),
                                facIsAdvisor = reader["FAC_IS_ADVISOR"].ToString().Trim()
                            });
                        }
                    }
                }
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { mess = 1, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult AddFaculty()
        {
            var data = new List<object>();
            try
            {
                var facName = Request["facName"];
                var facSpecialization = Request["facSpecialization"];
                var facContactInfo = Request["facContactInfo"];
                var facIsAdvisor = Request["facIsAdvisor"];
                using (var db = new SqlConnection(connStr))
                {
                    db.Open();
                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "INSERT INTO FACULTY ([FAC_ NAME], FAC_SPEACIALIZATION, FAC_CONTACT_INFO, FAC_IS_ADVISOR) VALUES (@facName, @facSpecialization, @facContactInfo, @facIsAdvisor)";
                        cmd.Parameters.AddWithValue("@facName", facName);
                        cmd.Parameters.AddWithValue("@facSpecialization", facSpecialization);
                        cmd.Parameters.AddWithValue("@facContactInfo", facContactInfo);
                        cmd.Parameters.AddWithValue("@facIsAdvisor", facIsAdvisor);
                        var ctr = cmd.ExecuteNonQuery();
                        if (ctr >= 1)
                        {
                            data.Add(new { mess = 0 });
                        }
                        else
                        {
                            data.Add(new { mess = 1, error = "No rows affected" });
                        }
                    }
                }
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { mess = 1, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult UpdateFaculty()
        {
            var data = new List<object>();
            try
            {
                var facName = Request["facName"];
                var facSpecialization = Request["facSpecialization"];
                var facContactInfo = Request["facContactInfo"];
                var facIsAdvisor = Request["facIsAdvisor"];
                var id = Request["id"];
                using (var db = new SqlConnection(connStr))
                {
                    db.Open();
                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "UPDATE FACULTY SET [FAC_ NAME] = @facName, FAC_SPEACIALIZATION = @facSpecialization, FAC_CONTACT_INFO = @facContactInfo, FAC_IS_ADVISOR = @facIsAdvisor WHERE FAC_ID = @id";
                        cmd.Parameters.AddWithValue("@facName", facName);
                        cmd.Parameters.AddWithValue("@facSpecialization", facSpecialization);
                        cmd.Parameters.AddWithValue("@facContactInfo", facContactInfo);
                        cmd.Parameters.AddWithValue("@facIsAdvisor", facIsAdvisor);
                        cmd.Parameters.AddWithValue("@id", id);
                        var ctr = cmd.ExecuteNonQuery();
                        if (ctr >= 1)
                        {
                            data.Add(new { mess = 0 });
                        }
                        else
                        {
                            data.Add(new { mess = 1, error = "No rows affected" });
                        }
                    }
                }
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { mess = 1, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DeleteFaculty()
        {
            var data = new List<object>();
            try
            {
                var id = Request["id"];
                using (var db = new SqlConnection(connStr))
                {
                    db.Open();
                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "DELETE FROM FACULTY WHERE FAC_ID = @id";
                        cmd.Parameters.AddWithValue("@id", id);
                        var ctr = cmd.ExecuteNonQuery();
                        if (ctr >= 1)
                        {
                            data.Add(new { mess = 0 });
                        }
                        else
                        {
                            data.Add(new { mess = 1, error = "No rows affected" });
                        }
                    }
                }
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { mess = 1, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}