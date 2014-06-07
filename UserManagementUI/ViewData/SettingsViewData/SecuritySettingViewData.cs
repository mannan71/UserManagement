using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UM.Model;


namespace UserManagementUI.ViewData.SettingsViewData
{
    public class SecuritySettingViewData
    {
        #region ViewData about SecuritySetting -------------------------------

        /// <summary>
        ///  Get or Set SecuritySetting objectList
        /// </summary>
        public List<SecuritySetting> SecuritySettingList { get; set; }

        /// <summary>
        /// Get or Set SecuritySetting object data
        /// </summary>
        public SecuritySetting SecuritySetting { get; set; }

        #endregion -----------------------------ViewData about SecuritySetting
    }
}
