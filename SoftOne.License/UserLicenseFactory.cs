using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.License
{
    public class UserLicenseFactory
    {
        public UserLicenseFactory();

        public static IUserLicenseCheck InitUserLicenseCheck();
    }
}
