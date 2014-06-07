using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using UserManagementUI.ViewData.SettingsViewData;
using UM.Model;
using UM.Interfaces;
using UM.Biz.Factory;

namespace UserManagementUI.Controllers
{
    public class SettingController : BaseController
    {
        #region Properties ---------------------------------

        SecuritySettingViewData objSecuritySettingViewData;
        ISecuritySetting objISecuritySetting;

        #endregion -------------------------------Properties

        #region Constructor --------------------------------
        public SettingController()
        {
            objSecuritySettingViewData = new SecuritySettingViewData();
            objSecuritySettingViewData.SecuritySetting = new SecuritySetting();
            objSecuritySettingViewData.SecuritySettingList = new List<SecuritySetting>();
            //Dynamically initializing Security BLL
            objISecuritySetting = SecurityFactory.InitiateSecuritySetting();
        }

        #endregion ------------------------------Constructor

        /// <summary>
        /// To create application security setting
        /// Redirect to security setting VIEW
        /// </summary>
        /// <returns></returns>       
        public ActionResult CreateSecuritySetting()
        {
            objSecuritySettingViewData.SecuritySetting = objISecuritySetting.GetListSecuritySetting()[0];
            return View("SecuritySetting", objSecuritySettingViewData);
        }

        /// <summary>
        /// Save security Setting Information to DB
        /// Save at new and Update time
        /// </summary>
        /// <returns></returns>
        public ActionResult SaveSecuritySetting([Bind()] SecuritySetting objSecuritySetting, string SecuritySettingId)
        {
            ValidateModelState(objSecuritySetting);
            if (ViewData.ModelState.IsValid)
            {
                objSecuritySetting.SecuritySettingId_PK = SecuritySettingId;
                objSecuritySetting = (SecuritySetting)SetObjectStatus(objSecuritySetting);
                string ReturnMessage = objISecuritySetting.SaveSecuritySetting(objSecuritySetting);
                if (string.IsNullOrEmpty(ReturnMessage))
                {
                    ViewData["AlertMessage"] = GetMessageScript(SAVE);
                    objSecuritySettingViewData.SecuritySetting = objISecuritySetting.GetListSecuritySetting()[0];
                }
            }
            return View("SecuritySetting", objSecuritySettingViewData);
        }

        /// <summary>
        /// Validate object
        /// </summary>
        /// <param name="objSecuritySetting"></param>
        private void ValidateModelState(SecuritySetting objSecuritySetting)
        {
            if (objSecuritySetting.MaxInvalidPassAtmpt <= 0)
            {
                ModelState.AddModelError("MaxInvalidPassAtmpt", MANDATORY);
            }
            if (objSecuritySetting.MinRepeatPassAllowed <= 0)
            {
                ModelState.AddModelError("MinRepeatPassAllowed", MANDATORY);
            }
            if (objSecuritySetting.PassExpireDay <= 0)
            {
                ModelState.AddModelError("PassExpireDay", MANDATORY);
            }
            if (objSecuritySetting.MinReqPassLen <= 0)
            {
                ModelState.AddModelError("MinReqPassLen", MANDATORY);
            }
            if (objSecuritySetting.MinReqNonAlphaNumChar <= 0)
            {
                ModelState.AddModelError("MinReqNonAlphaNumChar", MANDATORY);
            }
            if (string.IsNullOrEmpty(objSecuritySetting.NonAlphaNumChar))
            {
                ModelState.AddModelError("NonAlphaNumChar", MANDATORY);
            }
        }
    }
}
