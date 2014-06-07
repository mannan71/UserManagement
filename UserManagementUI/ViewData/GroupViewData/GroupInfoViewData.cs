using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UM.Model;


namespace UserManagementUI.ViewData.GroupViewData
{
    public class GroupInfoViewData
    {
        public List<GroupInfo> GroupInfoList { get; set; }

        public GroupInfo GroupInfo { get; set; }

        public ModuleInfo ModuleInfo { get; set; }
    }
}
