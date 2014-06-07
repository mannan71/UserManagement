using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using S1.Common.Model;
using S1.CommonBiz;
using System.Text;

namespace UserManagementUI.Controllers
{
    /// <summary>
    /// This Class will be inherited by each controller class.
    /// </summary>
    public abstract class BaseController : Controller
    {
        /// The followings are the common message keys which are read out from web config file.
        protected string SAVE = ConfigurationManager.AppSettings["SAVE"].ToString();
        protected string DELETE = ConfigurationManager.AppSettings["DELETE"].ToString();
        protected string NOTFOUND = ConfigurationManager.AppSettings["NOTFOUND"].ToString();
        protected string FOUND = ConfigurationManager.AppSettings["FOUND"].ToString();
        protected string NOTDELETE = ConfigurationManager.AppSettings["NOTDELETE"].ToString();
        protected string NOTEDIT = ConfigurationManager.AppSettings["NOTEDIT"].ToString();
        protected string MANDATORY = ConfigurationManager.AppSettings["MANDATORY"].ToString();
        protected string DATEFORMAT = ConfigurationManager.AppSettings["DATEFORMAT"].ToString();
        protected string DATEFORMATVALIDATION = ConfigurationManager.AppSettings["DATEFORMATVALIDATION"].ToString();
        protected string TIMEFORMATVALIDATION = ConfigurationManager.AppSettings["TIMEFORMATVALIDATION"].ToString();

        /// <summary>
        /// This Method return the JavaScript for alert message. It uses the ShowAlert(...) method of
        /// common JS file internally
        /// </summary>
        /// <param name="pMSG">The message which will be shown</param>
        /// <returns>Returns the script</returns>
        protected static string GetMessageScript(string pMSG)
        {
            string pReturnScript = null;
            if (!String.IsNullOrEmpty(pMSG))
                pReturnScript = "<script type=\"text/javascript\" language=\"javascript\">ShowAlert(\"" + pMSG + "\", \"\", \"\")</script>";
            return pReturnScript;
        }
        ///-------------------------------------------------------------------------------
        /// <summary>
        /// This method takes only that objects that implement IModelBase interface. It will then
        /// set the values of that IModelBase properties of that object. The object that is to 
        /// be sent as a parameter, must set the IsNew/IsDeleted properties before calling this Method. 
        /// </summary>
        /// <param name="objIModelBase">Object then implement IModelBase interface</param>
        /// <returns>Return the object of IModelBase. Need to cast the object</returns>
        protected static IModelBase SetObjectStatus(IModelBase objIModelBase)
        {
            if (objIModelBase.IsDeleted)
                objIModelBase.ActionType = "Delete";
            else if (objIModelBase.IsNew)
                objIModelBase.ActionType = "Insert";
            else
                objIModelBase.ActionType = "Update";

            objIModelBase.UserCode = SessionUtility.SessionContainer.USER_ID;
            //Locall declared other wise need to make new() the whole object
            string DATEFORMAT = ConfigurationManager.AppSettings["DATEFORMAT"].ToString();
            objIModelBase.ActionDate = CommonValidation.FormatDate(DateTime.Now.ToString(DATEFORMAT), true, true);
            objIModelBase.CompanyCode_FK = Convert.ToInt32(SessionUtility.SessionContainer.COMPANY_CODE);

            return objIModelBase;
        }
        ///-------------------------------------------------------------------------------
        /// <summary>
        /// If any Exception Occured that is not Handled then control will come here.
        /// Trace the Error and make a message and keep it to the session. This is the
        /// only place where Session is used rather than SessionUtility.SessionContainer.
        /// After that controls goes to the Error/General accroding to the web.config file.
        /// There this message is read from Session and shown to the user.
        /// Moreover as per the web.config setting this message is logged.
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void OnException(ExceptionContext filterContext)
        {            
            // just block the if condition if you want to see custom error even
            // in debug mode which will override the setting of web.config.
            if (filterContext.HttpContext.IsCustomErrorEnabled)
            {
                filterContext.ExceptionHandled = true;
                StringBuilder sb = new StringBuilder();
                sb.Append("Error in:");
                sb.Append(filterContext.HttpContext.Request.Url.ToString());
                sb.Append("<br> Error Message: <br>");
                sb.Append(filterContext.Exception.Message.ToString());
                sb.Append("<br> Stack Trace: <br>");
                sb.Append(filterContext.Exception.StackTrace.ToString());
                Session["GenericErrorMsg"] = sb.ToString();                
                this.RedirectToAction("General", "SecurityError").ExecuteResult(this.ControllerContext);                
                base.OnException(filterContext);
            }            
        }

        /// <summary>
        /// This Mehtod will read the message(Fixed format) check whether the operation is successfull or not.
        /// </summary>
        /// <param name="pMSG">Fixed format message which is get from BLL Layer. For example: SO200: Cannot compare</param>
        /// <param name="pOutMSG">This is the message which get from web config file.</param>
        /// <returns>Reads the message and returns true/false whether execution is successfull or not </returns>
        protected static string GetExcecutionMessage(string pMSG, string pOutMSG)
        {
            StringBuilder vSB = new StringBuilder();

            if (!IsSuccess(pMSG))
                vSB.Append("Operation Aborted !!! \\n");
            else
                return pOutMSG;
            return vSB.Append(pMSG).ToString();
        }
        protected static bool IsSuccess(string pMSG)
        {
            string[] vMSGParts = pMSG.Split(':');
            int pMsgId = Convert.ToInt32(vMSGParts[0].Substring(2, 3));
            if (pMsgId > 199)
                return false;
            else
                return true;
        }
    }
}