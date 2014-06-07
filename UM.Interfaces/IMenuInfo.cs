using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UM.Model;

namespace UM.Interfaces
{
    public interface IMenuInfo
    {
        #region Methods Signature------------------------------

        List<MenuItemInfo> GenerateMenuTree();
        string SaveMenu(MenuItemInfo objMenuInfo);
        string DeleteMenu(string MenuId);
        MenuItemInfo GetMenuById(string MenuId);

        #endregion ---------------------------Methods Signature
    }
}
