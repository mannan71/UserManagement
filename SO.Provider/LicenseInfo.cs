using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace SOProvider
{
    public class LicenseInfo
    {
        //private static RegistryKey RK;
      //  private static IUserLicenseCheck objIUserLicenseCheck;
        private static int _LicenseType;
        //This _SubKey value can be get from web.config. because it will be differ from 
        //application to application. do it later. 
        private static string _SubKey = "SoftOne";
        

        public static bool CheckUserLicense(string pLicenseUserName)
        {
            //objIUserLicenseCheck = UserLicenseFactory.InitUserLicenseCheck();
            //string vLicenseKey = objIUserLicenseCheck.ReadLicenseKey(pLicenseUserName, _SubKey);
            //if (!String.IsNullOrEmpty(vLicenseKey) && objIUserLicenseCheck.IsLicenseValid(pLicenseUserName, vLicenseKey))
            //    return true;
            //else
            //    return false;
            return true;
        }
        public static bool IsCrossedMaxLimit(string pLicenseUserName, string pBranchId)
        {
            //objIUserLicenseCheck = UserLicenseFactory.InitUserLicenseCheck();
            //string vLicenseKey = objIUserLicenseCheck.ReadLicenseKey(pLicenseUserName, _SubKey);
            //int vCurrentAllowedNumber = objIUserLicenseCheck.GetAllowedNumberWithLk(pLicenseUserName, vLicenseKey);

            //if (_LicenseType == (int)EnumLicenseType.BranchLicense)
            //{
            //    //Check Max Branch/Working Unit

            //    ///What to do Next==================>>>>>>>>>>>>>>>>
            //    ///Now we have vCurrentAllowedNumber. We need to go to DB for checking
            //    ///How many Branches/Working units are there. Then compare with the 
            //    ///value of vCurrentAllowedNumber and take decision.

            //    ///Here pBranchId is taken as a parameter with a sense that, may be the user
            //    ///will enter data through DB console. So in that case, here it will be used
            //    ///whether the branch/working unit id is within the first vCurrentAllowedNumber
            //    ///of branches/working units. 
            //}
            //else if (_LicenseType == (int)EnumLicenseType.UserLicense)
            //{
            //    //Check Max Users

            //    ///What to do Next==================>>>>>>>>>>>>>>>>
            //    ///Now we have vCurrentAllowedNumber. We need to go to DB for checking
            //    ///How many users are currently logged in there. Then compare with the 
            //    ///value of vCurrentAllowedNumber and take decision. if required, design 
            //    ///db table in that way.
            //}

            return false;
        }
    }
}
