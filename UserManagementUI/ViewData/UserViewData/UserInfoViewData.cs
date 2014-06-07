using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UM.Model;


namespace UserManagementUI.ViewData.UserViewData
{
    public class UserInfoViewData
    {
        #region ViewData about userinfo ----------------------------------------
        /// <summary>
        /// Get or Set UserInfo list
        /// </summary>
        public List<UserInfo> UserInfoList { get; set; }

        /// <summary>
        /// Get or Set UserInfo
        /// </summary>
        public UserInfo UserInfo { get; set; }

        #endregion ----------------------------------ViewData about userinfo list

        #region ViewData about UserInRole --------------------------------------

        /// <summary>
        /// Get or Set UserRoleList
        /// </summary>
        public List<RoleInfo> UserRoleList { get; set; }

        /// <summary>
        /// Get or Set UserInRole
        /// </summary>
        public UserInRole UserInRole { get; set; }

        #endregion ------------------------------------ViewData about UserInRole

        #region ViewData about WorkingUnit --------------------------------------

        /// <summary>
        /// Get or Set WorkingUnit
        /// </summary>
        public WorkingUnit WorkingUnit { get; set; }
        #endregion ------------------------------------ViewData about WorkingUnit
    }
}
