using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S1.CommonBiz;

namespace UM.Model
{
    /// <summary>
    /// Entity object associate with ModuleInfo
    /// </summary>
    public class ModuleInfo :BaseModel
    {

        #region ModuleInfo Members --------------------

        /// <summary>
        /// Get or Set Table name
        /// </summary>
        public string TableName_TBL { get; set; }

        /// <summary>
        /// Get or Set ModuleId
        /// </summary>
        public string ModuleId_PK { get; set; }

        /// <summary>
        /// Get or Set ModuleName
        /// </summary>
        public string ModuleName { get; set; }

        /// <summary>
        /// Get or Set SortOrder
        /// </summary>
        public int SortOrder { get; set; }

        
        // For Crud Logging (Razidul)
        public string RowId { get; set; }
        
        #endregion ------------------ModuleInfo Members
    }
}
