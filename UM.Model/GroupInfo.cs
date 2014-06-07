using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S1.CommonBiz;

namespace UM.Model
{
    /// <summary>
    /// Entity Object associate with GroupInfo
    /// </summary>
    public class GroupInfo : BaseModel
    {
        #region GroupInfo Members --------------------

        /// <summary>
        /// Get or Set Table name
        /// </summary>
        public string TableName_TBL { get; set; }

        /// <summary>
        /// Get or Set GroupId
        /// </summary>
        public string GroupId_PK { get; set; }

        /// <summary>
        /// Get or Set GroupName
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Get or Set ModuleId
        /// </summary>
        public string ModuleId { get; set; }

        #endregion ------------------GroupInfo Members
    }
}
