using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using UserManagementUI.ViewData.GroupViewData;
using UM.Interfaces;
using UM.Model;
using UM.Biz.Factory;

namespace UserManagementUI.Controllers
{
    public class GroupController : BaseController
    {
        GroupInfoViewData objGroupInfoViewData;
        IGroupInfo objIGroupInfo;
        IModuleInfo objIModuleInfo;

        /// <summary>
        /// 
        /// </summary>
        public GroupController()
        {
            objGroupInfoViewData = new GroupInfoViewData();
            objGroupInfoViewData.GroupInfoList = new List<GroupInfo>();
            objGroupInfoViewData.GroupInfo = new GroupInfo();
            objGroupInfoViewData.ModuleInfo = new ModuleInfo();
            //
            objIGroupInfo = SecurityFactory.InitiateGroupInfo();
            objIModuleInfo = SecurityFactory.InitiateModuleInfo();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ModuleId"></param>
        /// <param name="saved"></param>
        /// <returns></returns>
        public ActionResult LoadWorkGroup(string ModuleId, string saved)
        {
            if (!string.IsNullOrEmpty(saved))
            {
                ViewData["AlertMessage"] = GetMessageScript(SAVE);
            }
            //Load Module Dropdownlist
            List<ModuleInfo> ActionInfoList = objIModuleInfo.GetModuleList();
            ViewData["Modules"] = new SelectList(ActionInfoList, "ModuleId_PK", "ModuleName", ModuleId);
            objGroupInfoViewData.ModuleInfo.ModuleId_PK = ModuleId;
            objGroupInfoViewData.GroupInfoList = objIGroupInfo.GetGroupListByModuleId(ModuleId);
            return View("Groups", objGroupInfoViewData);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ModuleId"></param>
        /// <param name="GroupId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public ActionResult GetGroupById(string ModuleId, string GroupId, string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                ViewData["AlertMessage"] = "<div class='error'>" + message + "</div>";
            }
            objGroupInfoViewData.ModuleInfo = objIModuleInfo.GetModuleById(ModuleId);
            objGroupInfoViewData.GroupInfo = objIGroupInfo.GetGroupById(GroupId);
            return View("NewGroup", objGroupInfoViewData);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objGroupInfo"></param>
        /// <param name="ModuleId"></param>
        /// <param name="GroupId"></param>
        /// <returns></returns>
        public ActionResult SaveWorkGroup([Bind()] GroupInfo objGroupInfo,  string ModuleId, string GroupId)
        {
            
            if (!string.IsNullOrEmpty(ModuleId))
            {
                objGroupInfo.ModuleId = ModuleId;
            }
            else
            {
                ModelState.AddModelError("ModuleId", MANDATORY);
            }
            if (string.IsNullOrEmpty(objGroupInfo.GroupName))
            {
                ModelState.AddModelError("GroupName", MANDATORY);
            }
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(GroupId))
                {
                    objGroupInfo.IsNew = true;
                    objGroupInfo.GroupId_PK = Guid.NewGuid().ToString();
                }
                else
                {
                    objGroupInfo.IsNew = false;
                    objGroupInfo.GroupId_PK = GroupId;
                }
                objGroupInfo = (GroupInfo)SetObjectStatus(objGroupInfo);
                string ReturnMessage = objIGroupInfo.SaveGroupInfo(objGroupInfo);
                if (string.IsNullOrEmpty(ReturnMessage))
                {
                    return RedirectToAction("LoadWorkGroup", new {@ModuleId =ModuleId,  @saved = "Saved" });
                }
                else
                {
                    return RedirectToAction("GetGroupById", new {@ModuleId =ModuleId,  @message = ReturnMessage });
                }
            }
            return RedirectToAction("LoadWorkGroup", new { @ModuleId = objGroupInfo.ModuleId});
        }
    }
}
