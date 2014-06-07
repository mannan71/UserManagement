using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using UserManagementUI.ViewData.MenuViewData;
using UM.Interfaces;
using UM.Model;
using UM.Biz.Factory;
using System.Text;

namespace UserManagementUI.Controllers
{
    public class MenuController : BaseController
    {
        #region Member Variables ---------------------------------

        MenuInfoViewData objMenuInfoViewData;
        IMenuInfo objIMenuInfo;
        IActionInfo objIActionInfo;
        IModuleInfo objIModuleInfo;
        #endregion -------------------------------Member Variables

        #region Constructor --------------------------------------

        //Default constructor Initiate with empty object
        public MenuController()
        {
            objMenuInfoViewData = new MenuInfoViewData();
            objMenuInfoViewData.MenuItemInfoList = new List<MenuItemInfo>();
            objMenuInfoViewData.MenuItemInfo = new MenuItemInfo();
            objMenuInfoViewData.ActionInfo = new ActionInfo();
             //Dynamically initializing Security BLL
            objIMenuInfo = SecurityFactory.InitiateMenuInfo();
            objIActionInfo = SecurityFactory.InitiateActionInfo();
            objIModuleInfo = SecurityFactory.InitiateModuleInfo();
        }

        #endregion -----------------------------------Constructor

        #region ActionResult -------------------------------------
       
        /// <summary>
        /// Create Hierarchal tree
        /// </summary>
        /// <returns></returns>
        public ActionResult GenerateMenuTree(string popup)
        {
            List<MenuItemInfo> oItems = objIMenuInfo.GenerateMenuTree();
            StringBuilder sb = new StringBuilder(); 
            foreach (MenuItemInfo item in oItems)
            {               
                if (item.ChildMenu.Count > 0)
                {
                    sb.Append("<li class='collapsable last'> <div class='hitarea collapsable-hitarea'></div><A href='#' onClick=\"javascript:return NodeValue('" + item.MenuId_PK + "','" + item.MenuName + "');\">" + item.MenuName + "</A>");
                    sb.Append(System.Environment.NewLine + "<ul >" + System.Environment.NewLine);
                    CreateTree(sb, item.ChildMenu);
                    sb.Append("</ul>" + System.Environment.NewLine);
                }
                else
                {
                    sb.Append("<li class='last'><A href='#' onClick=\"javascript:return NodeValue('" + item.MenuId_PK + "','" + item.MenuName + "');\">" + item.MenuName + "</A>");
                }
                sb.Append("</li>" + System.Environment.NewLine);
            }

            ViewData["TreeView"] = sb.ToString();
            if (!string.IsNullOrEmpty(popup))
            {
                return View("MenuTreePopup", objMenuInfoViewData);
            }
            else
            {
                return View("MenuTree", objMenuInfoViewData);
            }
        }

        /// <summary>
        /// redirect to add menu view
        /// </summary>
        /// <returns></returns>
        public ActionResult NewMenu(string message, string saved, string deleted, string MenuId, string ModuleId)
        {
            if (!string.IsNullOrEmpty(message))
            {
                ViewData["AlertMessage"] = "<div class='error'>"+ message + "</div>";
            }
            if (!string.IsNullOrEmpty(saved))
            {
                ViewData["AlertMessage"] = GetMessageScript(SAVE);
            }
            if (!string.IsNullOrEmpty(deleted))
            {
                ViewData["AlertMessage"] = GetMessageScript(DELETE);
            }
            if (!string.IsNullOrEmpty(MenuId))
            {
                objMenuInfoViewData.MenuItemInfo = objIMenuInfo.GetMenuById(MenuId);
            }
            //Load Module Dropdownlist
            List<ModuleInfo> ActionInfoList = objIModuleInfo.GetModuleList();
            string SeletedModule = string.Empty;
            if (!string.IsNullOrEmpty(ModuleId))
            {
                SeletedModule = ModuleId;
            }
            else
            {
                SeletedModule = objMenuInfoViewData.MenuItemInfo.ModuleId;
            }
            ViewData["Modules"] = new SelectList(ActionInfoList, "ModuleId_PK", "ModuleName", SeletedModule);
            //Load Group Dropdownlist
            List<GroupInfo> ActionGroups = new List<GroupInfo>();
            if (!string.IsNullOrEmpty(SeletedModule))
            {
                ActionGroups = objIActionInfo.GetActionGroups(ModuleId);
            }
            ViewData["Groups"] = new SelectList(ActionGroups, "GroupId_PK", "GroupName", objMenuInfoViewData.MenuItemInfo.GroupId);          
            return View("AddMenu", objMenuInfoViewData);
        }

        /// <summary>
        /// Save Menu Item
        /// </summary>
        /// <returns></returns>
        public ActionResult SaveMenu([Bind()] MenuItemInfo objMenuItemInfo, FormCollection frmCollection,string MenuId, string ActionId)
        {
            ValidateModelState(objMenuItemInfo);            
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(MenuId))
                {
                    objMenuItemInfo.IsNew = true;
                    objMenuItemInfo.MenuId_PK = Guid.NewGuid().ToString();
                    objMenuItemInfo.ActionId_FK = ActionId;
                }
                else
                {
                    objMenuItemInfo.IsNew = false;
                    objMenuItemInfo.MenuId_PK = MenuId;
                    objMenuItemInfo.ActionId_FK = ActionId;
                }
                objMenuItemInfo.ModuleId = frmCollection["Module"].ToString();
                objMenuItemInfo.GroupId = frmCollection["Group"].ToString();
                objMenuItemInfo = (MenuItemInfo)SetObjectStatus(objMenuItemInfo);
                if (objMenuItemInfo.MenuId_PK != objMenuItemInfo.ParentMenuId)
                {
                    string ReturnMessage = objIMenuInfo.SaveMenu(objMenuItemInfo);

                    if (string.IsNullOrEmpty(ReturnMessage))
                    {
                        return RedirectToAction("NewMenu", new { @saved = "success" });
                    }
                    else
                    {
                        return RedirectToAction("NewMenu", new { @message = ReturnMessage });
                    }
                }
                else
                {
                    return RedirectToAction("NewMenu", new { @message = "Menu and ParentMenu cannot be same." });
                }
                
            }
            //Load Module Dropdownlist
            List<ModuleInfo> ActionInfoList = objIModuleInfo.GetModuleList();
            ViewData["Modules"] = new SelectList(ActionInfoList, "ModuleId_PK", "ModuleName");
            //Load Group Dropdownlist
            List<GroupInfo> ActionGroups = new List<GroupInfo>();
            ViewData["Groups"] = new SelectList(ActionGroups, "GroupId_PK", "GroupName");
            return View("AddMenu", objMenuInfoViewData);
        }

        /// <summary>
        /// Set delete flag to an object
        /// </summary>
        /// <param name="MenuId"></param>
        /// <returns></returns>
        public ActionResult DeleteMenu(string MenuId)
        {
            if (!string.IsNullOrEmpty(MenuId))
            {
               string ReturnMessage =  objIMenuInfo.DeleteMenu(MenuId);
               if (string.IsNullOrEmpty(ReturnMessage))
               {
                   return RedirectToAction("NewMenu", new { @deleted = "success" });
               }
               else
               {
                   return RedirectToAction("NewMenu", new { @message = ReturnMessage });
               }
                
            }
            return View("AddMenu",objMenuInfoViewData);
        }
        #endregion -------------------------------ActionResult

        #region Methods ------------------------------------------

        /// <summary>
        /// Crete tree brance recursively
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="oItems"></param>
        /// <returns></returns>
        protected string CreateTree(StringBuilder sb, List<MenuItemInfo> oItems)
        {
            foreach (MenuItemInfo Child in oItems)
            {                
                if (Child.ChildMenu.Count > 0)
                {
                    sb.Append("<li class='collapsable last'> <div class='hitarea collapsable-hitarea'></div><A href='#' onClick=\"javascript:return NodeValue('" + Child.MenuId_PK + "','" + Child.MenuName + "');\">" + Child.MenuName + "</A>");
                    sb.Append(System.Environment.NewLine + "<ul >" + System.Environment.NewLine);
                    CreateTree(sb, Child.ChildMenu);
                    sb.Append("</ul>" + System.Environment.NewLine);
                }
                else
                {
                    sb.Append("<li class='last'><A href='#' onClick=\"javascript:return NodeValue('" + Child.MenuId_PK + "','" + Child.MenuName + "');\">" + Child.MenuName + "</A>");
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
        private void ValidateModelState(MenuItemInfo objMenuItemInfo)
        {
            if (string.IsNullOrEmpty(objMenuItemInfo.MenuName))
            {
                ModelState.AddModelError("MenuName", MANDATORY);
            }
            if (string.IsNullOrEmpty(objMenuItemInfo.ParentMenuId))
            {
                ModelState.AddModelError("ParentMenuName", MANDATORY);
            }          

        }

        #endregion ----------------------------------------Methods

    }
}
