using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UM.Model;

namespace UM.Interfaces
{
    public interface ISecuritySetting
    {
        List<SecuritySetting> GetListSecuritySetting();
        string SaveSecuritySetting(SecuritySetting objSecuritySetting);
    }
}
