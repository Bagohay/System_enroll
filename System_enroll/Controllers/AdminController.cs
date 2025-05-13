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
    public class AdminController : Controller
    {
        string connStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Kent\source\repos\System_enroll\System_enroll\App_Data\StudentEntry.mdf;Integrated Security=True";
        public ActionResult Admin_Dashboard()
        {
            if (Session["UserNumber"] == null)
            {
                return RedirectToAction("Login_Page", "Home");
            }
            return View();
        }
   
        public ActionResult Admin_ManageEnrollees()
        {

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
                    cmd.CommandText = "SELECT U.US_NUMBER, U.US_FNAME, U.US_MNAME, U.US_LNAME, U.US_EMAIL, P.NAME, SE.STU_ENR_DATE, ES.ES_STATUS " +
                                      "FROM [dbo].[USER] U " +
                                      "INNER JOIN [dbo].[PROGRAMS] P ON P.ID = U.US_PROGRAM_ID " +
                                      "INNER JOIN [dbo].[STUDENT_ENROLLMENT] SE ON U.US_NUMBER = SE.STU_ENR_STU_NUMBER " +
                                      "INNER JOIN [dbo].[ENROLLMENT_STATUS] ES ON ES.ES_ID = SE.STU_ES_ID " +
                                      "WHERE ES.ES_STATUS = 'Pending'"; 

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
                                program = reader["NAME"].ToString(),
                                applicationDate = Convert.ToDateTime(reader["STU_ENR_DATE"]).ToString("MM/dd/yyyy"),
                                status = reader["ES_STATUS"].ToString()
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

            using (var db = new SqlConnection(connStr))
            {
                db.Open();
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;

                    // Map status to ES_ID
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
                            data.Add(new { mess = 1 });
                            return Json(data, JsonRequestBehavior.AllowGet);
                    }

                
                    cmd.CommandText = "UPDATE [dbo].[STUDENT_ENROLLMENT] SET STU_ES_ID = @statusId WHERE STU_ENR_STU_NUMBER = @studentId";
                    cmd.Parameters.AddWithValue("@statusId", statusId);
                    cmd.Parameters.AddWithValue("@studentId", studentId);

                    var ctr = cmd.ExecuteNonQuery();
                    if (ctr >= 1)
                    {
                        data.Add(new { mess = 0 });
                    }
                    else
                    {
                        data.Add(new { mess = 1 });
                    }
                }
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }


    }
}