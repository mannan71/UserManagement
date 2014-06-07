using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S1.CommonBiz;

namespace UM.Model
{
    public class UserInRole : BaseModel
    {
        #region Properties -----------------------

        /// <summary>
        /// Get or Set Table name
        /// </summary>
        public string TableName_TBL { get; set; }

        /// <summary>
        /// Get or Set RoleID
        /// </summary>
        public string RoleID { get; set; }

        /// <summary>
        /// Get and Set UserID
        /// </summary>
        public string UserID { get; set; }

        /// <summary>
        /// Get or Set UserInRoleId
        /// </summary>
        public string UserRoleID_PK { get; set; }

       
        #endregion ----------------------Properties
    }
}
