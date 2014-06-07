using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using UM.Interfaces;
using UM.Model;
using UM.DataContext;

namespace UM.Biz
{
    public class MenuInfoBiz : IMenuInfo
    {

        protected string SAVE = ConfigurationManager.AppSettings["SAVE"].ToString();
        protected string DELETE = ConfigurationManager.AppSettings["DELETE"].ToString();

        #region IMenuInfo Members ---------------------------------------


        /// <summary>
        /// Get Hierarchal Menu list
        /// </summary>
        /// <returns></returns>
        public List<MenuItemInfo> GenerateMenuTree()
        {
            List<MenuItemInfo> objMenuInfoList = MenuInfoDC.GetMenuList();
            List<MenuItemInfo> objMenuInfohierarchicalList = new List<MenuItemInfo>();
            MenuItemInfo objMenuInfo;
            for (int i = 0; i < objMenuInfoList.Count; i++)
            {
                objMenuInfo = objMenuInfoList[i];
                if (Guid.Empty.ToString().Equals(objMenuInfo.ParentMenuId) || string.IsNullOrEmpty(objMenuInfo.ParentMenuId))
                {
                    objMenuInfo.ChildMenu = GenerateChildMenu(objMenuInfo.MenuId_PK, objMenuInfoList);
                    objMenuInfohierarchicalList.Add(objMenuInfo);
                    break;
                }
            }
            return objMenuInfohierarchicalList;            
        }

        /// <summary>
        /// Get Hierarchal childMenu List
        /// </summary>
        /// <param name="MenuId"></param>
        /// <param name="objMenuInfoList"></param>
        /// <returns></returns>
        protected List<MenuItemInfo> GenerateChildMenu(string MenuId, List<MenuItemInfo> objMenuInfoList)
        {
            List<MenuItemInfo> oChildMenuList = new List<MenuItemInfo>();
            MenuItemInfo objMenuInfo;
            for (int i = 0; i < objMenuInfoList.Count; i++)
            {
                objMenuInfo = objMenuInfoList[i];
                if (objMenuInfo.ParentMenuId == MenuId)
                {
                    objMenuInfo.ChildMenu = GenerateChildMenu(objMenuInfo.MenuId_PK, objMenuInfoList);
                    oChildMenuList.Add(objMenuInfo);
                }
            }
            return oChildMenuList;
        }

        /// <summary>
        /// Get Menu object by ID
        /// </summary>
        /// <param name="MenuId"></param>
        /// <returns></returns>
        public MenuItemInfo GetMenuById(string MenuId)
        {
            return MenuInfoDC.GetMenuById(MenuId);
        }

        /// <summary>
        /// Save Menu
        /// </summary>
        /// <param name="objMenuInfo"></param>
        /// <returns></returns>
        public string SaveMenu(MenuItemInfo objMenuInfo)
        {
            return MenuInfoDC.SaveMenu(objMenuInfo) + SAVE;
        }

        /// <summary>
        /// Delete specific menu
        /// </summary>
        /// <param name="MenuId"></param>
        /// <returns></returns>
        public string DeleteMenu(string MenuId)
        {
            return MenuInfoDC.DeleteMenu(MenuId) + DELETE;
        }


        #endregion -------------------------------------IMenuInfo Members
    }
}
