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
    /// Business object for Module entity which implement IModuleInfo object
    /// </summary>
    public class ModuleInfoBiz :IModuleInfo
    {

        #region IModuleInfo Members

        public List<ModuleInfo> GetModuleList()
        {
           return ModuleInfoDC.GetModuleList();
        }

        public ModuleInfo GetModuleById(string ModuleId)
        {
            return ModuleInfoDC.GetModuleById(ModuleId);
        }

        public string SaveModule(ModuleInfo objModuleInfo)
        {
            return ModuleInfoDC.SaveModule(objModuleInfo);
        }

        #endregion
    }
}
