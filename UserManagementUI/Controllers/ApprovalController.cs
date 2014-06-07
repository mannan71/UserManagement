using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using LS.CrudLog.BLL;
using LS.CrudLog.BLL.Factory;
using LS.CrudLog.Interface;
using LS.UI.ViewData.ApprovalViewData;
using LS.Common.Model;

namespace LS.Security.UI.Controllers
{
    public class ApprovalController : BaseController
    {
        //
        // GET: /Approval/

        CrudStatusViewData objCrudStatusViewData;

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Check(string pMessage)
        {
            objCrudStatusViewData = new CrudStatusViewData();
            IWebPage objIWebPage = CrudLogFactory.InitiateWebPage();
            objCrudStatusViewData.PageAction = "Check";
            objCrudStatusViewData.ListWebPage = new SelectList(objIWebPage.GetWebPageList("Check"), "WebPageId_PK", "WebPageName", "0");
            if (!string.IsNullOrEmpty(pMessage))
            {
                if (!string.IsNullOrEmpty(pMessage))
                {
                    ViewData["AlertMessage"] = GetMessageScript(pMessage);
                }
            }
            return View("Approval", objCrudStatusViewData);
        }

        public ActionResult Authorize(string pMessage)
        {
            objCrudStatusViewData = new CrudStatusViewData();
            IWebPage objIWebPage = CrudLogFactory.InitiateWebPage();
            objCrudStatusViewData.PageAction = "Authorize";
            objCrudStatusViewData.ListWebPage = new SelectList(objIWebPage.GetWebPageList("Authorize"), "WebPageId_PK", "WebPageName", "0");
            if (!string.IsNullOrEmpty(pMessage))
            {
                if (!string.IsNullOrEmpty(pMessage))
                {
                    ViewData["AlertMessage"] = GetMessageScript(pMessage);
                }
            }
            return View("Approval", objCrudStatusViewData);
        }

        public ActionResult Approve(string pMessage)
        {
            objCrudStatusViewData = new CrudStatusViewData();
            IWebPage objIWebPage = CrudLogFactory.InitiateWebPage();
            objCrudStatusViewData.PageAction = "Approve";
            objCrudStatusViewData.ListWebPage = new SelectList(objIWebPage.GetWebPageList("Approve"), "WebPageId_PK", "WebPageName", "0");
            if (!string.IsNullOrEmpty(pMessage))
            {
                if (!string.IsNullOrEmpty(pMessage))
                {
                    ViewData["AlertMessage"] = GetMessageScript(pMessage);
                }
            }
            return View("Approval", objCrudStatusViewData);
        }

        public ActionResult SearchCrudStatus(string pWebPageId_PK, string pPageAction)
        {
            objCrudStatusViewData = new CrudStatusViewData();
            IWebPage objIWebPage = CrudLogFactory.InitiateWebPage();
            ICrudStatus objICrudStatus = CrudLogFactory.InitiateCrudStatus();
            objCrudStatusViewData.PageAction = pPageAction;
            objCrudStatusViewData.ListWebPage = new SelectList(objIWebPage.GetWebPageList(pPageAction), "WebPageId_PK", "WebPageName", pWebPageId_PK);
            objCrudStatusViewData.ListCrudStatus = objICrudStatus.GetCrudStatus(pWebPageId_PK, pPageAction);
            return View("Approval", objCrudStatusViewData);
        }

        public ActionResult ChangeCrudStatus(string submitButton)
        {
            if (submitButton == "Cancel")
            {
                return View("../Home/Index");
            }
            string sMsg = string.Empty;
            //string sAction = "Check";

            objCrudStatusViewData = new CrudStatusViewData();
            objCrudStatusViewData.PageAction = Request.Form["PageAction"].ToString();
            //List<TPinAccount> ListTPinAccount = new List<TPinAccount>();
            //UpdateModel<List<TPinAccount>>(ListTPinAccount, objFormCollection.ToValueProvider());
            List<ICrudStatus> ListCrudStatus = new List<ICrudStatus>();
            int vRowCount = Convert.ToUInt16(Request.Form["RowCount"].ToString());
            for (int count = 0; count < vRowCount; count++)
            {
                if (Request.Form.GetValues("IsChecked_VW_" + count.ToString()) != null)
                {
                    ICrudStatus objCrudStatus = CrudLogFactory.InitiateCrudStatus();
                    objCrudStatus.IsChecked_VW = Convert.ToBoolean(Request.Form.GetValues("IsChecked_VW_" + count.ToString())[0]);
                    objCrudStatus.TransactionId_PK = Request.Form["TransactionId_PK_" + count.ToString()].ToString();
                    objCrudStatus.UserCode = SessionUtility.SessionContainer.USER_ID;
                   
                    if (objCrudStatus.IsChecked_VW)
                        ListCrudStatus.Add(objCrudStatus);
                }
            }
            if (ListCrudStatus.Count > 0)
            {
                ICrudStatus objCrudStatus = CrudLogFactory.InitiateCrudStatus();
                objCrudStatus.ChangeCrudStatus(ListCrudStatus, objCrudStatusViewData.PageAction, submitButton);
                switch (submitButton)
                {
                    case "Accept":
                        sMsg = "Accepted sucessfully";
                        break;
                    case "Deny":
                        sMsg = "Denied successfully";
                        break;
                    default:
                        break;
                }
            }
           

            if (!string.IsNullOrEmpty(sMsg))
                return RedirectToAction(objCrudStatusViewData.PageAction, new {@pMessage = sMsg });
            else
                return RedirectToAction(objCrudStatusViewData.PageAction, new { @pMessage = string.Empty });
        }
    
    }
}
