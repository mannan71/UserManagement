using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.License
{
    public interface IUserLicenseCheck
    {
        int GetAllowedNumberWithLk(string pLicenseUserName, string pUserLK);
        int GetLicenseType(string pLicenseUserName, string pUserLK);
        bool IsLicenseValid(string pLicenseUserName, string pUserLK);
        string ReadLicenseKey(string pLicenseUserName, string pSubKey);
    }
}
