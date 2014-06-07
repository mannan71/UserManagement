using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SOProvider;
using System.Configuration;
using UM.Interfaces;
using UM.Model;
using UM.DataContext;

namespace UM.Biz
{
    public class ActionInfoBiz : IActionInfo
    {
        protected string SAVE = ConfigurationManager.AppSettings["SAVE"].ToString();
        protected string DELETE = ConfigurationManager.AppSettings["DELETE"].ToString();
        #region IActionInfo Members -------------------------

        /// <summary>
        /// Get application action
        /// If parameter is empty get all action
        /// Else get action according to parameter
        /// </summary>
        /// <param name="ActionName"></param>
        /// <returns></returns>
        public List<ActionInfo> GetActions(string ActionName)
        {
            return GetActions(ActionName, string.Empty);
        }

        /// <summary>
        ///  Get application action
        /// If parameter is empty get all action
        /// Else get action according to parameter
        /// </summary>
        /// <param name="ActionName"></param>
        /// <param name="ActionGroupName"></param>
        /// <returns></returns>
        public List<ActionInfo> GetActions(string ActionName, string GroupId)
        {
            return ActionInfoDC.GetActions(ActionName, GroupId);
        }
        /// <summary>
        /// Get action by actionID
        /// </summary>
        /// <param name="ActionId"></param>
        /// <returns></returns>
        public ActionInfo GetActionByID(string ActionId)
        {
            return ActionInfoDC.GetActionByID(ActionId);
        }

        /// <summary>
        /// Save action data
        /// </summary>
        /// <param name="objAction"></param>
        /// <returns></returns>
        public string SaveAction(ActionInfo objAction)
        {
            string vRetunMessage = string.Empty;
            if (objAction != null)
            {
                vRetunMessage = ActionInfoDC.SaveAction(objAction);
            }
            return vRetunMessage;
        }

        /// <summary>
        /// Delete action data
        /// </summary>
        /// <param name="ActionId"></param>
        /// <returns></returns>
        public string DeleteAction(string ActionId)
        {
            string vRetunMessage = string.Empty;
            vRetunMessage = ActionInfoDC.DeleteAction(ActionId);
            if(vRetunMessage.Length < 7)
            {
                vRetunMessage = DELETE;
            }
            return vRetunMessage;
        }

        /// <summary>
        /// Save roles of the action
        /// </summary>
        /// <param name="objActionInRoleList"></param>
        /// <returns></returns>
        public string SaveActionRoles(List<ActionInRole> objActionInRoleList)
        {
            List<ActionInRole> objList = new List<ActionInRole>();
            ActionInRole objActionInRole = null;
            ActionInRole objOldActionInRole = null;

            //Get all Existing roles of the specific action
            List<ActionInRole> objExistingRoles = ActionInfoDC.GetActionRoles(objActionInRoleList[0].ActionId, true);


            //Add SuperUser role with Action roles
            //This is default role for superadmin
            List<RoleInfo> objSuperUserRole = RoleInfoDC.GetRoles(SecurityConstant.SuperUserRole);
            objActionInRole = new ActionInRole();
            objActionInRole.ActionRoleId_PK = Guid.NewGuid().ToString();
            objActionInRole.ActionId = objActionInRoleList[0].ActionId;
            objActionInRole.RoleId = objSuperUserRole[0].RoleId_PK;

            objOldActionInRole = objExistingRoles.Find(AIR => AIR.RoleId == objActionInRole.RoleId && AIR.ActionId == objActionInRole.ActionId);
            if (objOldActionInRole == null)
            {
                objActionInRoleList.Add(objActionInRole);
            }
            else
            {               
                objExistingRoles.RemoveAt( objExistingRoles.FindIndex(AIR => AIR.RoleId == objActionInRole.RoleId && AIR.ActionId == objActionInRole.ActionId));
            }
           
            for (int i = 0; i < objActionInRoleList.Count; i++)
            {
                objActionInRole = objActionInRoleList[i];
                objOldActionInRole = objExistingRoles.Find(AIR => AIR.RoleId == objActionInRole.RoleId && AIR.ActionId == objActionInRole.ActionId);
                if (!string.IsNullOrEmpty(objActionInRole.RoleId) && objOldActionInRole == null)
                {
                    //Add new ActionInRole which add to action
                    objActionInRole.IsNew = true;
                    objList.Add(objActionInRole);
                }
                else if (!string.IsNullOrEmpty(objActionInRole.RoleId) && objOldActionInRole != null)
                {
                    //Add existing ActionInRole which add again to Action                    
                    objExistingRoles.Remove(objOldActionInRole);
                    if (objOldActionInRole.IsDeleted)
                    {
                        objOldActionInRole.IsDirty = true;
                        objOldActionInRole.IsNew = false;
                        objList.Add(objOldActionInRole);
                    }
                }
            }
            for (int i = 0; i < objExistingRoles.Count; i++)
            {
                //Add existing ActionInRole which remove from Action
                objActionInRole = objExistingRoles[i];               
                objActionInRole.IsNew = false;
                objList.Add(objActionInRole);
            }
            return ActionInfoDC.SaveActionRoles(objList);
        }

        /// <summary>
        /// Save Actions of the role
        /// </summary>
        /// <param name="objActionInRoleList"></param>
        /// <returns></returns>
        public string SaveRoleActions(List<ActionInRole> objActionInRoleList, string GroupId)
        {
            return ActionInfoDC.SaveRoleActions(objActionInRoleList, GroupId);
        }
        /// <summary>
        /// Get all roles of the action
        /// </summary>
        /// <param name="ActionId"></param>
        /// <returns></returns>
        public List<ActionInRole> GetActionRoles(string ActionId)
        {
            return ActionInfoDC.GetActionRoles(ActionId, false);
        }

        /// <summary>
        /// Get all action groups according to module name
        /// </summary>
        /// <returns></returns>
        public List<GroupInfo> GetActionGroups(string ModuleId)
        {
            return ActionInfoDC.GetActionGroups(ModuleId);
        }

        /// <summary>
        /// Get group wise Actions
        /// </summary>
        /// <param name="GroupName"></param>
        /// <returns></returns>
        public List<ActionInfo> GetActionsByGroupName(string GroupName)
        {
            return ActionInfoDC.GetActionsByGroupId(GroupName);
        }
        #endregion -----------------------IActionInfo Members
    }
}
