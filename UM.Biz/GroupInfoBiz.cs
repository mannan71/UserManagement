using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UM.DataContext;
using UM.Interfaces;
using UM.Model;

namespace UM.Biz
{
    /// <summary>
    /// Business object to Implement IGroupInfo Interface
    /// </summary>
    public class GroupInfoBiz : IGroupInfo
    {

        #region IGroupInfo Members

        public List<GroupInfo> GetGroupListByModuleId(string ModuleId)
        {
           return  GroupInfoDC.GetGroupListByModuleId(ModuleId);
        }

        public string SaveGroupInfo(GroupInfo objGroupInfo)
        {
            return GroupInfoDC.SaveGroupInfo(objGroupInfo);
        }

        public GroupInfo GetGroupById(string GroupId)
        {
            return GroupInfoDC.GetGroupById(GroupId);
        }
        #endregion
    }
}
