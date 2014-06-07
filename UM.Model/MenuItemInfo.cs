using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S1.CommonBiz;

namespace UM.Model
{
    public class MenuItemInfo : BaseModel
    {
        #region MenuInfo Members --------------------

        /// <summary>
        /// Get or Set Table name
        /// </summary>
        public string TableName_TBL { get; set; }

        /// <summary>
        /// Get or Ser MenuId
        /// </summary>
        public string MenuId_PK { get; set; }

        /// <summary>
        /// Get or Set MenuName
        /// </summary>
        public string MenuName { get; set; }

        /// <summary>
        /// Get or Set SortOrder
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Get or Set IsSecurityMenu
        /// </summary>
        public bool IsSecurityMenu { get; set; }

        /// <summary>
        /// Get or Set FastPath
        /// </summary>
        public string FastPath { get; set; }

        /// <summary>
        /// Get or Set ParentMenuId
        /// </summary>
        public string ParentMenuId { get; set; }

        /// <summary>
        /// Get or Set ParentMenuName
        /// </summary>
        public string ParentMenuName { get; set; }
        /// <summary>
        /// Get or Set ActionId
        /// </summary>
        public string ActionId_FK { get; set; }

        /// <summary>
        /// Get or Set ActionName
        /// </summary>
        public string  ActionName { get; set; }

        /// <summary>
        /// Get or Set ModuleId
        /// </summary>
        public string ModuleId { get; set; }

        /// <summary>
        /// Get or Set GroupId
        /// </summary>
        public string GroupId { get; set; }

        /// <summary>
        /// Get or Set User friendly text
        /// </summary>
        public string MenuText { get; set; }

        /// <summary>
        /// Get or Set bool value is it module or not?
        /// </summary>
        public bool IsModule { get; set; }

        /// <summary>
        /// Get or Set bool value is it help menu or not?
        /// </summary>
        public bool IsHelpMenu { get; set; }

        /// <summary>
        /// Get or Set ChildMenu
        /// </summary>
        public List<MenuItemInfo> ChildMenu { get; set; }

        #endregion ------------------MenuInfo Members
    }
}
