using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UM.DataContext;
using UM.Interfaces;
using UM.Model;

namespace UM.Biz
{
    public class SecuritySettingBiz : ISecuritySetting
    {
        public List<SecuritySetting> GetListSecuritySetting()
        {
            return SecuritySettingDC.GetListSecuritySetting();
        }

        public string SaveSecuritySetting(SecuritySetting objSecuritySetting)
        {
            return SecuritySettingDC.SaveSecuritySetting(objSecuritySetting);
        }
    }
}
