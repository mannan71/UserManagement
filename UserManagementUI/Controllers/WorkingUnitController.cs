using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using UserManagementUI.ViewData.UserViewData;
using UM.Interfaces;
using UM.Model;
using System.Text;
using UM.Biz.Factory;

namespace UserManagementUI.Controllers
{
    public class WorkingUnitController : BaseController
    {

        #region Member Variables ---------------------------------

        WorkingUnitViewData objWorkingUnitViewData;
        IWorkingUnit objIWorkingUnit;
        

        #endregion -------------------------------Member Variables

        #region Constructor --------------------------------------

        //Default constructor Initiate with empty object
        public WorkingUnitController()
        {
            objWorkingUnitViewData = new WorkingUnitViewData();
            objWorkingUnitViewData.WorkingUnitList = new List<WorkingUnit>();
            objWorkingUnitViewData.WorkingUnit = new WorkingUnit();
             //Dynamically initializing Security BLL
            objIWorkingUnit = SecurityFactory.InitiateWorkingUnit();
        }

        #endregion -----------------------------------Constructor

        #region ActionResult -------------------------------------
        
        /// <summary>
        /// Create Hierarichal Tree
        /// </summary>
        /// <returns></returns>
        public ActionResult GenerateTree()
        {
            List<WorkingUnit> oItems = objIWorkingUnit.GenerateWorkingUnitTree();
            StringBuilder sb = new StringBuilder();
            foreach (WorkingUnit item in oItems)
            {
                if (item.ChildItems.Count > 0)
                {
                    sb.Append("<li class='collapsable last'> <div class='hitarea collapsable-hitarea'></div><A href='#' onClick=\"javascript:return NodeValue('" + item.WorkingUnitCode_PK + "','" + item.WorkingUnitCode_PK + "','" + item.WorkingUnitName + "');\">" + item.WorkingUnitName + "</A>");
                    sb.Append(System.Environment.NewLine + "<ul >" + System.Environment.NewLine);
                    CreateTree(item.WorkingUnitCode_PK,sb, item.ChildItems);
                    sb.Append("</ul>" + System.Environment.NewLine);
                }
                else
                {
                    sb.Append("<li class='last'><A href='#' onClick=\"javascript:return NodeValue('" + item.WorkingUnitCode_PK + "','" + item.WorkingUnitCode_PK + "','" + item.WorkingUnitName + "');\">" + item.WorkingUnitName + "</A>");
                }
                sb.Append("</li>" + System.Environment.NewLine);
            }

            ViewData["TreeView"] = sb.ToString();   
            return View("WorkingUnitList", objWorkingUnitViewData);
        }

        /// <summary>
        /// Action to create workingUnit
        /// Redirect to WorkingUnit view
        /// </summary>
        /// <returns></returns>
        public ActionResult CreateWorkingUnit(string message, string  WorkingUnitCode)
        {
            if (!string.IsNullOrEmpty(message))
            {
                ViewData["AlertMessage"] = "<div class='error'>" + message + "</div>";
            }
            if (!string.IsNullOrEmpty( WorkingUnitCode))
            {
                objWorkingUnitViewData.WorkingUnit =  objIWorkingUnit.GetWorkingUnitByUnitCode(Convert.ToInt32(WorkingUnitCode));
            }
            ViewData.ModelState.Clear();
            objWorkingUnitViewData.WorkingUnit.WorkingUnitCode_PK = Convert.ToInt32(WorkingUnitCode);
            return View("NewWorkingUnit", objWorkingUnitViewData);
        }

        /// <summary>
        /// Redirect to WorkingUnitLogo View]
        /// To add LOGO for a comapany
        /// </summary>
        /// <param name="WorkingUnitCode"></param>
        /// <returns></returns>
        public ActionResult CreateWorkingUnitLogo(string WorkingUnitCode, string WorkingUnitName, string message, string saved)
        {
            if (!string.IsNullOrEmpty(message))
            {
                ViewData["AlertMessage"] = "<div class='error'>" + message + "</div>";
            }
            if (!string.IsNullOrEmpty(saved))
            {
                ViewData["AlertMessage"] = GetMessageScript(SAVE);
            }
            objWorkingUnitViewData.WorkingUnit.WorkingUnitCode_PK =Convert.ToInt32(WorkingUnitCode);
            objWorkingUnitViewData.WorkingUnit.WorkingUnitName = WorkingUnitName;
            return View("WorkingUnitLogo",objWorkingUnitViewData);
        }

        /// <summary>
        /// Save workingUnit to BD
        /// Save both new and edited unit
        /// </summary>
        /// <returns></returns>
        public ActionResult SaveWorkingUnit([Bind()] WorkingUnit objWorkingUnit)
        {
            ValidateModelState(objWorkingUnit);
            WorkingUnit objExistingUnit = new WorkingUnit();
            string returnMessage = string.Empty;
            if (ModelState.IsValid)
            {
                if (objWorkingUnit.WorkingUnitCode_PK > 0)
                {
                    objExistingUnit = objIWorkingUnit.GetWorkingUnitByUnitCode(Convert.ToInt32(objWorkingUnit.WorkingUnitCode_PK));
                    
                    objWorkingUnit = (WorkingUnit)SetObjectStatus(objWorkingUnit);
                    if (objExistingUnit.WorkingUnitCode_PK > 0)
                    {
                        objWorkingUnit.IsNew = false;
                       returnMessage = objIWorkingUnit.SaveWorkingUnit(objWorkingUnit);
                    }
                    else
                    {
                        objWorkingUnit.IsNew = true;
                        returnMessage = objIWorkingUnit.SaveWorkingUnit(objWorkingUnit);
                    }                   
                    if (string.IsNullOrEmpty(returnMessage))
                    {
                        ViewData["AlertMessage"] = GetMessageScript(SAVE);
                        objWorkingUnitViewData.WorkingUnit = objIWorkingUnit.GetWorkingUnitByUnitCode(Convert.ToInt32(objWorkingUnit.WorkingUnitCode_PK));
                    }
                    else
                    {
                        return RedirectToAction("CreateWorkingUnit", new { @message = returnMessage });
                    }
                }
            }
            return View("NewWorkingUnit", objWorkingUnitViewData);
        }

        /// <summary>
        /// Save WorkingUnitLogo
        /// </summary>
        /// <param name="objWorkingUnitLogo"></param>
        /// <param name="WorkingUnitCode"></param>
        /// <returns></returns>
        public ActionResult SaveWorkingUnitLogo([Bind()] WorkingUnitLogo objWorkingUnitLogo, int WorkingUnitCode)
        {
            if (WorkingUnitCode > 0)
            {               
                objWorkingUnitLogo.IsNew = true;
                objWorkingUnitLogo.WorkingUnitCode_PK = WorkingUnitCode;
                foreach (string inputTagName in Request.Files)
                {
                    HttpPostedFileBase file = Request.Files[inputTagName];
                    if (file.ContentLength > 0 && file.ContentLength <= 100000)
                    {
                        byte[] myImage = new byte[file.ContentLength];
                        file.InputStream.Read(myImage, 0, (int)file.ContentLength);
                        objWorkingUnitLogo.Logo = myImage;
                        objWorkingUnitLogo = (WorkingUnitLogo)SetObjectStatus(objWorkingUnitLogo);
                        string ReturnMessage = objIWorkingUnit.SaveWorkingUnitLogo(objWorkingUnitLogo);
                        if (string.IsNullOrEmpty(ReturnMessage))
                        {                           
                            return RedirectToAction("CreateWorkingUnitLogo", new { @WorkingUnitCode = objWorkingUnitLogo.WorkingUnitCode_PK, @WorkingUnitName = objWorkingUnitLogo.WorkingUnitName, @saved="Saved" });
                        }
                        else
                        {
                            return RedirectToAction("CreateWorkingUnitLogo", new { @WorkingUnitCode = objWorkingUnitLogo.WorkingUnitCode_PK, @WorkingUnitName = objWorkingUnitLogo.WorkingUnitName, @message = ReturnMessage });
                        }
                    }
                    else
                    {
                        return RedirectToAction("CreateWorkingUnitLogo", new { @WorkingUnitCode = objWorkingUnitLogo.WorkingUnitCode_PK, @WorkingUnitName = objWorkingUnitLogo.WorkingUnitName, @message = "File size is not supported" });
                    }
                }

            }
            return View("WorkingUnitLogo",objWorkingUnitViewData);
        }

        /// <summary>
        /// Delete unit after clint confirmation
        /// After delte redirect to list/home
        /// </summary>
        /// <returns></returns>
        public ActionResult DeleteWorkingUnit(string WorkingUnitCode)
        {
           string returnMessage =  objIWorkingUnit.DeleteWorkingUnit(Convert.ToInt32(WorkingUnitCode));
           if (string.IsNullOrEmpty(returnMessage))
           {
               ViewData["AlertMessage"] = GetMessageScript(DELETE);
               return RedirectToAction("CreateWorkingUnit");
           }
           else
           {
               return RedirectToAction("CreateWorkingUnit", new { @message = returnMessage });
           }
           
        }

        /// <summary>
        /// Delete WorkingUnitLogo
        /// </summary>
        /// <param name="WorkingUnitCode"></param>
        /// <returns></returns>
        public ActionResult DeleteWorkingUnitLogo(int WorkingUnitCode)
        {
            string returnMessage = objIWorkingUnit.DeleteWorkingUnitLogo(WorkingUnitCode);
            if (string.IsNullOrEmpty(returnMessage))
            {
                ViewData["AlertMessage"] = GetMessageScript(DELETE);
                return RedirectToAction("CreateWorkingUnitLogo");
            }
            else
            {
                return RedirectToAction("CreateWorkingUnitLogo", new { @message = returnMessage });
            }
        }
        #endregion -------------------------------ActionResult

        #region Methods ------------------------------------------

        /// <summary>
        /// Crete tree brance recursively
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="oItems"></param>
        /// <returns></returns>
        protected string CreateTree(int workingUnitRoot, StringBuilder sb, List<WorkingUnit> oItems)
        {
            foreach (WorkingUnit Child in oItems)
            {

                if (Child.ChildItems.Count > 0)
                {
                    sb.Append("<li class='collapsable last'> <div class='hitarea collapsable-hitarea'></div><A href='#' onClick=\"javascript:return NodeValue('" + workingUnitRoot + "','" + Child.WorkingUnitCode_PK + "','" + Child.WorkingUnitName + "');\">" + Child.WorkingUnitName + "</A>");
                    sb.Append(System.Environment.NewLine + "<ul >" + System.Environment.NewLine);
                    CreateTree(workingUnitRoot,sb, Child.ChildItems);
                    sb.Append("</ul>" + System.Environment.NewLine);
                }
                else
                {
                    sb.Append("<li class='last'><A href='#' onClick=\"javascript:return NodeValue('" + workingUnitRoot + "','" + Child.WorkingUnitCode_PK + "','" + Child.WorkingUnitName + "');\">" + Child.WorkingUnitName + "</A>");
                }
                sb.Append("</li>" + System.Environment.NewLine);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Check required fields
        /// </summary>
        /// <param name="objUserInfo"></param>
        /// <param name="UserId"></param>
        private void ValidateModelState(WorkingUnit objWorkingUnit)
        {
            if (string.IsNullOrEmpty(objWorkingUnit.WorkingUnitName))
            {
                ModelState.AddModelError("WorkingUnitName", MANDATORY );
            }
            if (string.IsNullOrEmpty(objWorkingUnit.WorkingUnitPatern))
            {
                ModelState.AddModelError("WorkingUnitPatern", MANDATORY);
            }
            if (string.IsNullOrEmpty(objWorkingUnit.WorkingUnitType))
            {
                ModelState.AddModelError("WorkingUnitType", MANDATORY);
            }
            if (string.IsNullOrEmpty(objWorkingUnit.ParentUnitName))
            {
                ModelState.AddModelError("ParentUnitName", MANDATORY);
            }
        }

        #endregion ----------------------------------------Methods

    }
}
