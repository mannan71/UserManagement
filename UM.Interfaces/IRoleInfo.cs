using System;
using System.Collections.Generic;
using UM.Model;
using System.Security;

namespace UM.Interfaces
{
    public interface IRoleInfo
    {
        List<RoleInfo> GetRoles(string RoleName);
        RoleInfo GetRoleById(string RoleId);
        string SaveRole(RoleInfo objRoleInfo);
        List<RoleInfo> GetUserRoleList(string UserId);
        List<ActionInfo> GetRoleActions(string RoleId, string GroupName);
    }
}