using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UM.Model;


namespace UserManagementUI.ViewData.UserViewData
{
    public class WorkingUnitViewData
    {

        #region ViewData about WorkingUnit -----------------

        /// <summary>
        /// Get or Set WorkingUnit list
        /// </summary>
        public List<WorkingUnit> WorkingUnitList { get; set; }

        /// <summary>
        /// Get or Set WorkingUnit
        /// </summary>
        public WorkingUnit WorkingUnit { get; set; }

        /// <summary>
        /// Get or Set WorkingUnitLogo
        /// </summary>
        public WorkingUnitLogo WorkingUnitLogo { get; set; }

        #endregion ---------------ViewData about WorkingUnit
    }
}
