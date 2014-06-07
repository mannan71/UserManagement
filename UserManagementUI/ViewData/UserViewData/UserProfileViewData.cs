using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

using System.Collections.Generic;
using UM.Model;

namespace UserManagementUI.ViewData.UserViewData
{
    public class UserProfileViewData
    {

        #region Properties -------------------------------------
        /// <summary>
        /// Get or Set User Profile List
        /// It may come from HR Module by access WebService
        /// </summary>
        public List<UserProfile> UserProfileList { get; set; }

        /// <summary>
        /// Get or Set User's profile
        /// </summary>
        public UserProfile UserProfile { get; set; }

        /// <summary>
        /// Get or Set PagingNoList
        /// </summary>
        public string PagingNoList { get; set; }
        #endregion -----------------------------------Properties
    }
}
