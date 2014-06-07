using System;
using System.Collections.Generic;
using UM.Model;


namespace UserManagementUI.ViewData.RoleViewData
{
    public class RoleInfoViewData
    {
        public RoleInfo RoleInfo
        {
            get;
            set;
        }

        public List<ActionInfo> ActionInfoList { get; set; }
        public ActionInfo ActionInfo { get; set; }
    }
}