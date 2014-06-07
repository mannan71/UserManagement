using System;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace UserManagementUI.Controllers
{
    public class SecurityErrorController : BaseController
    {
        /// <summary>
        /// Handle error to redirect custom error page
        /// </summary>
        /// <returns></returns>
        [HandleError]
        [OutputCache(Location = OutputCacheLocation.None)]
        public ActionResult General()
        {            
            return View("GenericErrorPage");
        }
    }
}