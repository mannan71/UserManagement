using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UM.Model;


namespace UserManagementUI.ViewData.ModuleViewData
{
    /// <summary>
    /// View data object which is bound with view
    /// </summary>
    public class ModuleInfoViewData
    {
        /// <summary>
        /// Get or Set ModuleInfo List
        /// </summary>
        public List<ModuleInfo> ModuleInfoList { get; set; }

        /// <summary>
        /// Get or Set ModuleInfo object
        /// </summary>
        public ModuleInfo ModuleInfo { get; set; }
    }
}
