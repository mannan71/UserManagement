using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UM.Model;


namespace UserManagementUI.ViewData.ActionViewData
{
    public class ActionInfoViewData
    {
        #region ViewData about ActionInfo --------------------------------------

        /// <summary>
        /// Get or Set ActionInfo collection
        /// </summary>
        public List<ActionInfo> ActionInfoList { get; set; }

        /// <summary>
        /// Get or Set ActionInfo
        /// </summary>
        public ActionInfo ActionInfo { get; set; }
        #endregion ------------------------------------ViewData about ActionInfo

        #region ViewData about ActionInRole ------------------------------------

        /// <summary>
        /// Get or Set action roles
        /// </summary>
        public List<RoleInfo> ActionRoleList { get; set; }

        /// <summary>
        /// Get or Set action in role
        /// </summary>
        public ActionInRole ActionInRole { get; set; }

        /// <summary>
        /// Get or Set RoleInfo
        /// </summary>
        public RoleInfo RoleInfo { get; set; }

        #endregion ------------------------------------ViewData about ActionInRole

        #region ViewData about ModuleInfo --------------------------------------

        /// <summary>
        /// Get or Set ModuleInfoList
        /// </summary>
        public List<ModuleInfo> ModuleInfoList { get; set; }
        
        /// <summary>
        /// Get Or Set ModuleInfo Object
        /// </summary>
        public ModuleInfo ModuleInfo { get; set; }
        #endregion ------------------------------------ViewData about ModuleInfo

        #region ViewData about GroupInfo ---------------------------------------

        /// <summary>
        /// Get or Set GroupInfo Object List
        /// </summary>
        public List<GroupInfo> GroupInfoList { get; set; }

        /// <summary>
        /// Get or Set GroupInfo Object
        /// </summary>
        public GroupInfo GroupInfo { get; set; }
        #endregion ------------------------------------ViewData about GroupInfo
    }
}
