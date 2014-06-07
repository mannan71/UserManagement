using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S1.CommonBiz;

namespace UM.Model
{
    public class ActionInRole : BaseModel
    {
        #region Own Members -------------------------

        /// <summary>
        /// Get or Set Table name
        /// </summary>
        public string TableName_TBL { get; set; }

        /// <summary>
        /// Get or Set ActionId
        /// </summary>
        public string ActionId { get; set; }

        /// <summary>
        /// Get or Set RoleId
        /// </summary>
        public string RoleId { get; set; }

        /// <summary>
        /// Get or Set ActionInRoleId
        /// </summary>
        public string  ActionRoleId_PK { get; set; }
        #endregion ----------------------- Own Members
    }
}
