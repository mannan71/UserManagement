using System;
using System.Collections.Generic;
using UM.DataContext;
using UM.Interfaces;
using UM.Model;


namespace UM.Biz
{
    public class RoleInfoBiz : IRoleInfo
    {
        #region IRoleInfo Members  ------------------------------

        /// <summary>
        /// Get role list of the application
        /// </summary>
        /// <param name="RoleInfo"></param>
        /// <returns></returns>
        public List<RoleInfo> GetRoles(string RoleInfo)
        {
            return RoleInfoDC.GetRoles(RoleInfo);
        }

        /// <summary>
        /// Get specific role by roleId
        /// </summary>
        /// <param name="RoleId"></param>
        /// <returns></returns>
        public RoleInfo GetRoleById(string RoleId)
        {
            return RoleInfoDC.GetRoleById(RoleId);
        }

        /// <summary>
        /// Save Role
        /// </summary>
        /// <param name="objRoleInfo"></param>
        /// <returns></returns>
        public string SaveRole(RoleInfo objRoleInfo)
        {
            try
            {               
                 return RoleInfoDC.SaveRole(objRoleInfo);                           
            }
            catch
            {                
                throw;
            }
            return "";
        }

        /// <summary>
        /// Get role list of the specific user
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public List<RoleInfo> GetUserRoleList(string UserId)
        {
          return  RoleInfoDC.GetUserRoleList(UserId);
        }

        /// <summary>
        /// Get role wise Actions
        /// </summary>
        /// <param name="RoleId"></param>
        /// <returns></returns>
        public List<ActionInfo> GetRoleActions(string RoleId, string GroupName)
        {
            return RoleInfoDC.GetRoleActions(RoleId, GroupName);
        }
        #endregion ------------------------------IRoleInfo Members
    }
}