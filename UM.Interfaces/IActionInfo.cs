using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UM.Model;

namespace UM.Interfaces
{
    public interface IActionInfo
    {
        #region Methods ------------------------------

        List<ActionInfo> GetActions(string ActionName);
        List<ActionInfo> GetActions(string ActionName,string GroupId);
        ActionInfo GetActionByID(string ActionId);
        string SaveAction(ActionInfo objAction);
        string DeleteAction(string ActionId);
        string SaveActionRoles(List<ActionInRole> objActionInRoleList);
        string SaveRoleActions(List<ActionInRole> objActionInRoleList, string GroupId);
        List<ActionInRole> GetActionRoles(string ActionId);
        List<GroupInfo> GetActionGroups(string ModuleId);       
        List<ActionInfo> GetActionsByGroupName(string GroupName);
        #endregion ----------------------------Methods
    }
}
