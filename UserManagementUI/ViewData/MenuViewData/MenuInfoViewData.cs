using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UM.Model;


namespace UserManagementUI.ViewData.MenuViewData
{
    public class MenuInfoViewData
    {
        /// <summary>
        /// Get or Set list of MenuInfo
        /// </summary>
        public List<MenuItemInfo> MenuItemInfoList { get; set; }

        /// <summary>
        /// Get or Set MenuInfo
        /// </summary>
        public MenuItemInfo MenuItemInfo { get; set; }

        /// <summary>
        /// Get or Set ActionInfo
        /// </summary>
        public ActionInfo ActionInfo { get; set; }

        /// <summary>
        /// Get or Set action roles
        /// </summary>
        public List<RoleInfo> MenuRoleList { get; set; }
    }
}
