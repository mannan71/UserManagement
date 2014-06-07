using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using UserManagementUI.ViewData.ModuleViewData;
using UM.Model;
using UM.Interfaces;
using UM.Biz.Factory;

namespace UserManagementUI.Controllers
{
    public class ModuleController : BaseController
    {
        ModuleInfoViewData objModuleInfoViewData;
        IModuleInfo objIModuleInfo;
        public ModuleController()
        {
            objModuleInfoViewData = new ModuleInfoViewData();
            objModuleInfoViewData.ModuleInfoList = new List<ModuleInfo>();
            objModuleInfoViewData.ModuleInfo = new ModuleInfo();
            objIModuleInfo = SecurityFactory.InitiateModuleInfo();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="saved"></param>
        /// <returns></returns>
        public ActionResult LoadModule(string saved)
        {
            if (!string.IsNullOrEmpty(saved))
            {
                ViewData["AlertMessage"] = GetMessageScript(SAVE);
            }
            objModuleInfoViewData.ModuleInfoList = objIModuleInfo.GetModuleList();
            return View("Modules", objModuleInfoViewData);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public ActionResult CreateModule(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                ViewData["AlertMessage"] = "<div class='error'>" + message + "</div>";
            }
            return View("NewModule", objModuleInfoViewData);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ModuleId"></param>
        /// <returns></returns>
        public ActionResult GetModuleById(string ModuleId)
        {
            if(!string.IsNullOrEmpty(ModuleId))
            {
                objModuleInfoViewData.ModuleInfo = objIModuleInfo.GetModuleById(ModuleId);
            }
            return View("NewModule", objModuleInfoViewData);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objModuleInfo"></param>
        /// <param name="ModuleId"></param>
        /// <returns></returns>
        public ActionResult SaveModule([Bind()] ModuleInfo objModuleInfo, string ModuleId)
        {
            if (string.IsNullOrEmpty(objModuleInfo.ModuleName))
            {
                ModelState.AddModelError("ModuleName", MANDATORY);
            }
            if (objModuleInfo.SortOrder <= 0)
            {
                ModelState.AddModelError("SortOrder", MANDATORY);
            }

            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(ModuleId))
                {
                    objModuleInfo.ModuleId_PK = Guid.NewGuid().ToString();
                    objModuleInfo.IsNew = true;
                }
                else
                {
                    objModuleInfo.IsNew = false;
                    objModuleInfo.ModuleId_PK = ModuleId;
                }
                objModuleInfo = (ModuleInfo)SetObjectStatus(objModuleInfo);
                string ReturnMessage = objIModuleInfo.SaveModule(objModuleInfo);
                if (string.IsNullOrEmpty(ReturnMessage))
                {
                    return RedirectToAction("LoadModule", new {@saved = "Saved" });
                }
                else
                {
                    return RedirectToAction("CreateModule", new { @message = ReturnMessage });
                }

            }
            return View("NewModule", objModuleInfoViewData);
        }
    }
}
