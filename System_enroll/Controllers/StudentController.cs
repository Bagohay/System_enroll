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
    public class StudentController : Controller
    {
        public ActionResult Student_Dashboard()
        {
            if (Session["UserNumber"] == null)
            {
                return RedirectToAction("Login_Page", "Home");
            }
            return View();
        }
    }
}