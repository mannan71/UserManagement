using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UM.Model;

namespace UM.Interfaces
{
    /// <summary>
    /// Interface object which implement in ModuleInfoBiz
    /// </summary>
    public interface IModuleInfo
    {
        #region Methods Signature------------------------------
        List<ModuleInfo> GetModuleList();
        ModuleInfo GetModuleById(string ModuleId);
        string SaveModule(ModuleInfo objModuleInfo);

        #endregion ---------------------------Methods Signature
    }
}
