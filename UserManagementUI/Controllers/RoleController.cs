using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using UM.Interfaces;
using UM.Model;
using UM.Biz.Factory;
using UserManagementUI.ViewData.RoleViewData;

namespace UserManagementUI.Controllers
{
    public class RoleController : BaseController
    {
        #region Properties ------------------------------

        RoleInfoListViewData objRoleInfoListViewData;
        RoleInfoViewData objRoleInfoViewData;
        IRoleInfo objIRoleInfo;
        IActionInfo objIActionInfo;
        IModuleInfo objIModuleInfo;
        #endregion _-------------------------- Properties

        #region Constructor -----------------------------

        /// <summary>
        /// Initiate Constructor
        /// To handle null reference exception 
        /// </summary>
        public RoleController()
        {
            // create instance of ViewData
            objRoleInfoListViewData = new RoleInfoListViewData();
            objRoleInfoListViewData.RoleInfoList = new List<RoleInfo>();
            objRoleInfoListViewData.RoleInfo = new RoleInfo();           
            objRoleInfoViewData = new RoleInfoViewData();
            objRoleInfoViewData.RoleInfo = new RoleInfo();
            objRoleInfoViewData.ActionInfo = new ActionInfo();
            objRoleInfoViewData.ActionInfoList = new List<ActionInfo>();
            //Dynamically initializing Security BLL
            objIRoleInfo = SecurityFactory.InitiateRoleInfo();
            objIActionInfo = SecurityFactory.InitiateActionInfo();
            objIModuleInfo = SecurityFactory.InitiateModuleInfo();
        }

        #endregion  --------------------------Constructor

        #region Acction Result --------------------------

        /// <summary>
        /// Get all role of the application
        /// </summary>
        /// <returns></returns>
        public ActionResult GetRoleList(string saved)
        {
            objRoleInfoListViewData.RoleInfoList = objIRoleInfo.GetRoles("");
            if (!string.IsNullOrEmpty(saved))
            {
                ViewData["AlertMessage"] = GetMessageScript(SAVE);
            }
            return View("Roles",objRoleInfoListViewData);
        }      

        /// <summary>
        /// Get parameterize  role accordimg to parameter
        /// It execute by like operation, at the end
        /// </summary>
        /// <param name="RoleName"></param>
        /// <returns></returns>
        public ActionResult GetRoleListByName(string RoleName)
        {
            objRoleInfoListViewData.RoleInfoList = objIRoleInfo.GetRoles(RoleName);
            return View("Roles", objRoleInfoListViewData);
        }
        
        /// <summary>
        /// Redirect to net role window/View
        /// </summary>
        /// <param name="RoleName"></param>
        /// <returns></returns>
        public ActionResult NewRole(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                ViewData["AlertMessage"] = GetMessageScript(message);//"<div class='error'> " + message + "</div>";
            }
            return View("Role", objRoleInfoViewData);
        }

        /// <summary>
        /// Get role by roleId
        /// </summary>
        /// <param name="RoleId"></param>
        /// <returns></returns>
        public ActionResult GetRoleById(string RoleId)
        {
            try
            {
                objRoleInfoViewData.RoleInfo = objIRoleInfo.GetRoleById(RoleId);
            }
            catch (ApplicationException ex)
            {
                throw;
            }
            return View("Role", objRoleInfoViewData);
        }

        /// <summary>
        /// Save role 
        /// </summary>
        /// <param name="objRoleInfo"></param>
        /// <param name="RoleId"></param>
        /// <returns></returns>
        public ActionResult SaveRole([Bind()] RoleInfo objRoleInfo, string RoleId)
        {
            ValidateModelState(objRoleInfo, RoleId);
            if (ModelState.IsValid)
            {
                objRoleInfo.RoleId_PK = RoleId;
                if (String.IsNullOrEmpty(objRoleInfo.RoleId_PK))
                {
                    objRoleInfo.IsNew = true;
                    objRoleInfo.RoleId_PK = Guid.NewGuid().ToString();
                }
                else
                {
                    objRoleInfo.IsNew = false;
                    objRoleInfo.RoleId_PK = RoleId;
                }

                objRoleInfo = (RoleInfo)SetObjectStatus(objRoleInfo);
                string ReturnMessage = objIRoleInfo.SaveRole(objRoleInfo);
                ReturnMessage = GetExcecutionMessage(ReturnMessage, SAVE);

                if (string.IsNullOrEmpty(ReturnMessage))
                {
                    return RedirectToAction("GetRoleList", new { @saved = "success"});
                }
                else
                {                    
                    return RedirectToAction("NewRole", new { @message = ReturnMessage });
                }                
            }
            else
            {
                return View("Role",objRoleInfoViewData);
            }
        }

        /// <summary>
        /// get GroupWise ActionList
        /// </summary>
        /// <param name="RoleId"></param>
        /// <param name="RoleName"></param>
        /// <returns></returns>
        public ActionResult GetListAction(string RoleId, string RoleName, string ModuleId, string GroupId, string message)
        {
            objRoleInfoViewData.RoleInfo.RoleId_PK = RoleId;
            objRoleInfoViewData.RoleInfo.RoleName = RoleName;
            string guid = Guid.Empty.ToString();
            if (!string.IsNullOrEmpty(GroupId))
            {
                objRoleInfoViewData.ActionInfo.GroupId = GroupId;
            }
            List<ModuleInfo> ActionInfoList = objIModuleInfo.GetModuleList();
            ViewData["Modules"] = new SelectList(ActionInfoList, "ModuleId_PK", "ModuleName", ModuleId);
            List<GroupInfo> ActionGroups = new List<GroupInfo>();
            if (!string.IsNullOrEmpty(ModuleId))
            {
                ActionGroups = objIActionInfo.GetActionGroups(ModuleId);
            }

            ViewData["Groups"] = new SelectList(ActionGroups, "GroupId_PK", "GroupName", GroupId);

            if (!string.IsNullOrEmpty(GroupId))
            {
                objRoleInfoViewData.RoleInfo.RoleActions = objIRoleInfo.GetRoleActions(RoleId, GroupId);
                objRoleInfoViewData.ActionInfoList = objIActionInfo.GetActionsByGroupName(GroupId);
            }

            if (!string.IsNullOrEmpty(message))
            {
                ViewData["AlertMessage"] = GetMessageScript(message);//"<div class='error'> " + message + "</div>";
            }
            return View("AssignAction", objRoleInfoViewData);
        }

        /// <summary>
        /// Save Actions in Role
        /// </summary>
        /// <param name="frmActions"></param>
        /// <returns></returns>
        public ActionResult SaveActionsInRole(FormCollection frmActions, string RoleId)
        {
            string ReturnMessage=string.Empty;
            List<ActionInRole> objRoleActionList = new List<ActionInRole>();
            string ModuleId = frmActions["Module"].ToString();
            string GroupId = frmActions["Group"].ToString();
            string[] ActionsId = frmActions["Actions"].TrimStart('_').Split('_');
            ActionInRole objActionInRole;
            for (int i = 0; i < ActionsId.Length; i++)
            {
                if (ActionsId[i].ToString() != "0")
                {
                    objActionInRole = new ActionInRole();
                    objActionInRole.ActionRoleId_PK = Guid.NewGuid().ToString();
                    objActionInRole.ActionId = ActionsId[i].ToString();
                    objActionInRole.RoleId = RoleId;
                    objActionInRole.IsNew = true;
                    objActionInRole = (ActionInRole)SetObjectStatus(objActionInRole);
                    objRoleActionList.Add(objActionInRole);
                }
            }
            if (!string.IsNullOrEmpty(GroupId))
            {
                ReturnMessage = GetExcecutionMessage(objIActionInfo.SaveRoleActions(objRoleActionList, GroupId), SAVE);
                //if (string.IsNullOrEmpty(ReturnMessage))
                //{
                //    //return RedirectToAction("GetRoleList", new { @saved = "success" });
                //    return RedirectToAction("GetListAction", new { @RoleId = RoleId, @RoleName = frmActions["RoleName"].ToString(), @ModuleId = ModuleId });
                //}
                //else
                //{
                //    return RedirectToAction("GetListAction", new { @RoleId = RoleId, @RoleName = frmActions["RoleName"].ToString(), @ModuleId = ModuleId });
                //}
            }
            return RedirectToAction("GetListAction", new { @RoleId = RoleId, @RoleName = frmActions["RoleName"].ToString(), @ModuleId = ModuleId, @message = ReturnMessage });
        }
        #endregion --------------------------Action Result

        #region Private Methods -------------------------

        /// <summary>
        /// Validate data of the object
        /// </summary>
        /// <param name="objRoleInfo"></param>
        /// <param name="RoleId"></param>
        private void ValidateModelState(RoleInfo objRoleInfo, string RoleId)
        {          
            if (objRoleInfo.RoleName.Trim().ToString().Length == 0)
                ModelState.AddModelError("RoleName", MANDATORY);
        }

        #endregion ------------------------Private Methods
    }
}