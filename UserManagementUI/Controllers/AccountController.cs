using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UserManagementUI.Controllers
{
    public class AccountController : Controller
    {
        //
        // GET: /Account/
        public ActionResult Index()
        {
            return View("LogIn");
        }

        public ActionResult Login(string username, string password, bool? rememberMe)
        {
            @ViewBag.Title = "Login";//baki...........
            // Basic parameter validation
            string errors = string.Empty;
            //Non-POST requests should just display the Login form 
            if (Request.HttpMethod != "POST")
            {
                return View("LogIn");
            }

           return RedirectToAction("Index", "Home");
        }
    }
}
