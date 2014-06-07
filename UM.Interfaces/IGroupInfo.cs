using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UM.Model;

namespace UM.Interfaces
{
    /// <summary>
    /// Interface Object which implement in GroupInfoBiz
    /// </summary>
    public interface IGroupInfo
    {

        #region Methods Signature------------------------------
        List<GroupInfo> GetGroupListByModuleId(string ModuleId);
        GroupInfo GetGroupById(string GroupId);
        string SaveGroupInfo(GroupInfo objGroupInfo);

        #endregion ---------------------------Methods Signature

    }
}
