using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S1.CommonBiz;

namespace UM.Model
{
    public class ActionInfo : BaseModel
    {
        #region Own Members -------------------------

        /// <summary>
        /// Get or Set Table name
        /// </summary>
        public string TableName_TBL { get; set; }

        /// <summary>
        /// Get or Set ActionId
        /// </summary>
        public string  ActionId_PK { get; set; }

        /// <summary>
        /// Get or Set ActionName
        /// </summary>
        public string  ActionName { get; set; }

        /// <summary>
        /// Get or Set ActionPath
        /// </summary>
        public string  ActionPath { get; set; }

        /// <summary>
        /// Get or Set ModuleId
        /// </summary>
        public string ModuleId { get; set; }

        /// <summary>
        /// Get or Set GroupId
        /// </summary>
        public string GroupId { get; set; }

        /// <summary>
        /// Get or Set IsVisibleAction
        /// </summary>
        public bool IsVisibleAction { get; set; }

        /// <summary>
        /// Get or Set bool value to require log
        /// </summary>
        public bool RequireLogging { get; set; }

        /// <summary>
        /// Get or Set Log text
        /// </summary>
        public string LogText { get; set; }        

        /// <summary>
        /// Get or Set Action Description
        /// </summary>
        public string ActionDescription { get; set; }
        /// <summary>
        /// Get or Set ActionInRole list
        /// </summary>
        public List<ActionInRole> ActionRoles { get; set; }

        #endregion -----------------------Own Members
    }
}
