using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using UserManagementUI.ViewData.ActionViewData;
using UM.Interfaces;
using UM.Model;
using UM.Biz.Factory;

namespace UserManagementUI.Controllers
{
    public class ActionController : BaseController
    {
        #region Member Variables --------------------------
        ActionInfoViewData objActionInfoViewData;
        IActionInfo objIActionInfo;
        IRoleInfo objIRoleInfo;
        IModuleInfo objIModuleInfo;

        #endregion ------------------------Member Variables

        #region Constructor -------------------------------

        /// <summary>
        /// Initiate defalut constructor by new object
        /// </summary>
        public ActionController()
        {
            objActionInfoViewData = new ActionInfoViewData();
            objActionInfoViewData.ActionInfo = new ActionInfo();
            objActionInfoViewData.ActionInfo.ActionRoles = new List<ActionInRole>();
            objActionInfoViewData.ActionInfoList = new List<ActionInfo>();
            objActionInfoViewData.ActionInRole = new ActionInRole();
            objActionInfoViewData.ActionRoleList = new List<RoleInfo>();
            //Dynamically initializing Security BLL
            objIActionInfo = SecurityFactory.InitiateActionInfo();
            objIRoleInfo = SecurityFactory.InitiateRoleInfo();
            objIModuleInfo = SecurityFactory.InitiateModuleInfo();
        }

        #endregion -----------------------------Constructor

        #region ActionResult ------------------------------

        public ActionResult LoadActionList(string ModuleId)
        {
            //Load Module DropdownList
            List<ModuleInfo> ActionInfoList = objIModuleInfo.GetModuleList();
            ViewData["Modules"] = new SelectList(ActionInfoList, "ModuleId_PK", "ModuleName", ModuleId);
            //Load Group Dropdownlist
            List<GroupInfo> ActionGroups = new List<GroupInfo>();
            if (!string.IsNullOrEmpty(ModuleId))
            {
                ActionGroups = objIActionInfo.GetActionGroups(ModuleId);
            }
            ViewData["Groups"] = new SelectList(ActionGroups, "GroupId_PK", "GroupName");
            return View("ActionList", objActionInfoViewData);
        }
        /// <summary>
        /// Get action list
        /// If actionname parameter is empty then get all actions
        /// Else get parameter specific action
        /// </summary>
        /// <param name="ActionName"></param>
        /// <returns></returns>
        [OutputCache(Duration = 1, VaryByParam = "*")]
        public ActionResult ActionList(string ActionName, string ModuleId, string GroupId, string saved, string deleted)
        {
            if (!string.IsNullOrEmpty(saved))
            {
                ViewData["AlertMessage"] = GetMessageScript(SAVE);
            }
            if (!string.IsNullOrEmpty(deleted))
            {
                ViewData["AlertMessage"] = GetMessageScript(DELETE);
            }
            if (!string.IsNullOrEmpty(ModuleId) && !string.IsNullOrEmpty(GroupId))
            {
                //Load Module DropdownList
                List<ModuleInfo> ActionInfoList = objIModuleInfo.GetModuleList();
                ViewData["Modules"] = new SelectList(ActionInfoList, "ModuleId_PK", "ModuleName", ModuleId);
                //Load Group Dropdownlist
                List<GroupInfo> ActionGroups = new List<GroupInfo>();
                if (!string.IsNullOrEmpty(ModuleId))
                {
                    ActionGroups = objIActionInfo.GetActionGroups(ModuleId);
                }
                ViewData["Groups"] = new SelectList(ActionGroups, "GroupId_PK", "GroupName", GroupId);
                objActionInfoViewData.ActionInfoList = objIActionInfo.GetActions(ActionName, GroupId);
                return View("ActionList", objActionInfoViewData);
            }
            else if (!string.IsNullOrEmpty(ActionName))
            {
                List<ModuleInfo> ActionInfoList = objIModuleInfo.GetModuleList();
                ViewData["Modules"] = new SelectList(ActionInfoList, "ModuleId_PK", "ModuleName");
                List<GroupInfo> ActionGroups = new List<GroupInfo>();
                ViewData["Groups"] = new SelectList(ActionGroups, "GroupId_PK", "GroupName");
                objActionInfoViewData.ActionInfoList = objIActionInfo.GetActions(ActionName);
                return View("ActionList", objActionInfoViewData);
            }
            else if (GroupId != null)
            {
                if (GroupId == "")
                {
                    objActionInfoViewData.ActionInfoList = objIActionInfo.GetActions(ActionName, string.Empty);
                }
                else
                {
                    objActionInfoViewData.ActionInfoList = objIActionInfo.GetActions(ActionName, GroupId);
                }
                return View("ActionListPopup", objActionInfoViewData);
            }
            else
            {
                List<ModuleInfo> ActionInfoList = objIModuleInfo.GetModuleList();
                ViewData["Modules"] = new SelectList(ActionInfoList, "ModuleId_PK", "ModuleName");
                List<GroupInfo> ActionGroups = new List<GroupInfo>();
                ViewData["Groups"] = new SelectList(ActionGroups, "GroupId_PK", "GroupName");
                //objActionInfoViewData.ActionInfoList = objIActionInfo.GetActions(ActionName);
                return View("ActionList", objActionInfoViewData);
            }
        }

        /// <summary>
        /// Save Action
        /// </summary>
        /// <param name="actionId"></param>
        /// <returns></returns>
        public ActionResult SaveAction([Bind()] ActionInfo objActionInfo, FormCollection frmCollection, string ActionId)
        {
            ValidateModelState(objActionInfo);
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(ActionId))
                {
                    objActionInfo.IsNew = true;
                    objActionInfo.ActionId_PK = Guid.NewGuid().ToString();
                }
                else
                {
                    objActionInfo.IsNew = false;
                    objActionInfo.ActionId_PK = ActionId.ToString();
                }
                objActionInfo.ModuleId = frmCollection["Module"].ToString();
                objActionInfo.GroupId = frmCollection["Group"].ToString();
                objActionInfo = (ActionInfo)SetObjectStatus(objActionInfo);
                string ReturnMessage = objIActionInfo.SaveAction(objActionInfo);
                if (string.IsNullOrEmpty(ReturnMessage))
                {
                    //return RedirectToAction("ActionList", new { @saved ="success"  });
                    return RedirectToAction("NewAction");
                }
                else
                {
                    ReturnMessage = GetExcecutionMessage(ReturnMessage, SAVE);
                    return RedirectToAction("NewAction", new { @message = ReturnMessage });
                }

            }
            return RedirectToAction("NewAction");
        }

        /// <summary>
        /// Load assign role
        /// </summary>
        /// <returns></returns>
        public ActionResult AssignRole(string ActionId, string message)
        {
            objActionInfoViewData.ActionInfo = objIActionInfo.GetActionByID(ActionId);
            objActionInfoViewData.ActionInfo.ActionRoles = objActionInfoViewData.ActionInfo.ActionRoles;
            objActionInfoViewData.ActionRoleList = objIRoleInfo.GetRoles(string.Empty);
            if (objActionInfoViewData.ActionInfo.ActionRoles == null)
            {
                objActionInfoViewData.ActionInfo.ActionRoles = new List<ActionInRole>();
            }
            if (!string.IsNullOrEmpty(message))
            {
                ViewData["AlertMessage"] = "<div class='error'>" + message + "</div>";
            }
            return View("AssignRole", objActionInfoViewData);
        }

        /// <summary>
        /// Save action specific role
        /// </summary>
        /// <returns></returns>        
        public ActionResult SaveActionRoles(string ActionId, FormCollection objForm)
        {
            string[] roleIds = objForm["Roles"].TrimStart('_').Split('_');
            List<ActionInRole> objActionInRoleList = new List<ActionInRole>();
            ActionInRole objActionInRole;
            for (int i = 0; i < roleIds.Length; i++)
            {
                if (!string.IsNullOrEmpty(roleIds[i].ToString()))
                {
                    objActionInRole = new ActionInRole();
                    objActionInRole.ActionRoleId_PK = Guid.NewGuid().ToString();
                    objActionInRole.ActionId = ActionId;
                    objActionInRole.RoleId = roleIds[i].ToString();
                    objActionInRole.IsNew = true;
                    objActionInRole = (ActionInRole)SetObjectStatus(objActionInRole);
                    objActionInRoleList.Add(objActionInRole);
                }
                else
                {
                    objActionInRole = new ActionInRole();
                    objActionInRole.ActionId = ActionId;
                    objActionInRoleList.Add(objActionInRole);
                }
            }

            string ReturnMessage = objIActionInfo.SaveActionRoles(objActionInRoleList);
            if (string.IsNullOrEmpty(ReturnMessage))
            {
                return RedirectToAction("ActionList", new { @saved = "success" });
            }
            else
            {
                return RedirectToAction("AssignRole", new { @ActionId = ActionId, @message = ReturnMessage });
            }

        }

        /// <summary>
        /// Get new action view
        /// </summary>
        /// <returns></returns>
        public ActionResult NewAction(string message, string ModuleId, string GroupId)
        {
            if (!string.IsNullOrEmpty(message))
            {
                ViewData["AlertMessage"] = GetMessageScript(message);
            }
            //Load Module Dropdownlist
            List<ModuleInfo> ActionInfoList = objIModuleInfo.GetModuleList();
            ViewData["Modules"] = new SelectList(ActionInfoList, "ModuleId_PK", "ModuleName", ModuleId);
            //Load Group DropdownList
            List<GroupInfo> ActionGroups = new List<GroupInfo>();
            if (!string.IsNullOrEmpty(ModuleId))
            {
                ActionGroups = objIActionInfo.GetActionGroups(ModuleId);
            }

            ViewData["Groups"] = new SelectList(ActionGroups, "GroupId_PK", "GroupName", GroupId);

            return View("NewAction", objActionInfoViewData);
        }

        /// <summary>
        /// Get Action data and View action info
        /// </summary>
        /// <param name="ActionId"></param>
        /// <returns></returns>
        public ActionResult EditAction(string ActionId, string ModuleId)
        {
            ActionInfo objActionInfo = objIActionInfo.GetActionByID(ActionId);
            objActionInfoViewData.ActionInfo = objActionInfo;
            //Load Module Dropdownlist
            List<ModuleInfo> ActionInfoList = objIModuleInfo.GetModuleList();

            string SelectedValue = string.Empty;
            if (!string.IsNullOrEmpty(ModuleId))
            {
                SelectedValue = ModuleId;
            }
            else
            {
                SelectedValue = objActionInfo.ModuleId;
            }
            ViewData["Modules"] = new SelectList(ActionInfoList, "ModuleId_PK", "ModuleName", SelectedValue);
            //Load Group DropdownList
            List<GroupInfo> ActionGroups = new List<GroupInfo>();
            if (!string.IsNullOrEmpty(objActionInfo.ModuleId))
            {
                ActionGroups = objIActionInfo.GetActionGroups(SelectedValue);
            }
            ViewData["Groups"] = new SelectList(ActionGroups, "GroupId_PK", "GroupName", objActionInfo.GroupId);

            return View("NewAction", objActionInfoViewData);
        }

        /// <summary>
        /// Delete Action
        /// </summary>
        /// <param name="ActionID"></param>
        /// <returns></returns>       
        public ActionResult DeleteAction(string ActionId)
        {
            string ReturnMessage = objIActionInfo.DeleteAction(ActionId);
            if (string.IsNullOrEmpty(ReturnMessage))
            {
                return RedirectToAction("ActionList", new { @deleted = "success" });
            }
            else
            {
                return RedirectToAction("NewAction", new { @message = ReturnMessage });
            }

        }

        /// <summary>
        /// Get Action Groups
        /// </summary>
        /// <returns></returns>
        public ActionResult GetActionGroups()
        {
            objActionInfoViewData.GroupInfoList = objIActionInfo.GetActionGroups(string.Empty);
            return View("ActionGroupPopup", objActionInfoViewData);
        }

        #endregion

        #region Methods -----------------------------------
        /// <summary>
        /// Check required fields
        /// </summary>
        /// <param name="objUserInfo"></param>
        /// <param name="UserId"></param>
        private void ValidateModelState(ActionInfo objActionInfo)
        {
            if (string.IsNullOrEmpty(objActionInfo.ActionName.Trim().ToString()))
            {
                ModelState.AddModelError("ActionName", MANDATORY);
            }
            if (string.IsNullOrEmpty(objActionInfo.ActionPath.Trim().ToString()))
            {
                ModelState.AddModelError("ActionPath", MANDATORY);
            }
        }
        #endregion ---------------------------------Methods
    }
}
