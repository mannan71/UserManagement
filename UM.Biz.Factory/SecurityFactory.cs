using System;
using System.Configuration;
using System.Reflection;
using UM.Interfaces;
using System.Security;

namespace UM.Biz.Factory
{
    public class SecurityFactory
    {
        private static string vAssemblyName = ConfigurationManager.AppSettings["Security"].ToString();

        private static string GetVClassName(string pClassName)
        {
            return vAssemblyName + "." + pClassName;
        }
        public static IRoleInfo InitiateRoleInfo()
        {
           // string vAssemblyName = ConfigurationManager.AppSettings["Security"].ToString();
           // string vClassName = vAssemblyName + ".RoleInfoBiz";
            //return (IRoleInfo)Assembly.Load(vAssemblyName).CreateInstance(vClassName);

            return (IRoleInfo)Assembly.Load(vAssemblyName).CreateInstance(GetVClassName("RoleInfoBiz"));

        }

        /// <summary>
        /// Initiate UserInfo interface
        /// </summary>
        /// <returns></returns>
        public static IUserInfo InitiateUserInfo()
        {
            string vAssemblyName = ConfigurationManager.AppSettings["Security"].ToString();
            string vClassName = vAssemblyName + ".UserInfoBiz";
            return (IUserInfo)Assembly.Load(vAssemblyName).CreateInstance(vClassName);

            //return (IUserInfo)Assembly.Load("UM.Biz").CreateInstance("UM.Biz.UserInfoBiz");
        }

        /// <summary>
        /// Initiate ActionInfo Interface
        /// </summary>
        /// <returns></returns>
        public static IActionInfo InitiateActionInfo()
        {
            string vAssemblyName = ConfigurationManager.AppSettings["Security"].ToString();
            string vClassName = vAssemblyName + ".ActionInfoBiz";
            return (IActionInfo)Assembly.Load(vAssemblyName).CreateInstance(vClassName);
        }

        /// <summary>
        /// Initiate MenuInfo Interface
        /// </summary>
        /// <returns></returns>
        public static IMenuInfo InitiateMenuInfo()
        {
            string vAssemblyName = ConfigurationManager.AppSettings["Security"].ToString();
            string vClassName = vAssemblyName + ".MenuInfoBiz";
            return (IMenuInfo)Assembly.Load(vAssemblyName).CreateInstance(vClassName);
        }   

        /// <summary>
        /// Initiate WorkingUnit Interface
        /// </summary>
        /// <returns></returns>
        public static IWorkingUnit InitiateWorkingUnit()
        {
            string vAssemblyName = ConfigurationManager.AppSettings["Security"].ToString();
            string vClassName = vAssemblyName + ".WorkingUnitBiz";
            return (IWorkingUnit)Assembly.Load(vAssemblyName).CreateInstance(vClassName);
        }

        /// <summary>
        /// Initiate Security Setting Interface
        /// </summary>
        /// <returns></returns>
        public static ISecuritySetting InitiateSecuritySetting()
        {
            string vAssemblyName = ConfigurationManager.AppSettings["Security"].ToString();
            string vClassName = vAssemblyName + ".SecuritySettingBiz";
            return (ISecuritySetting)Assembly.Load(vAssemblyName).CreateInstance(vClassName);
        }

        /// <summary>
        /// Initiate ModuleInfo Interface
        /// </summary>
        /// <returns></returns>
        public static IModuleInfo InitiateModuleInfo()
        {
            string vAssemblyName = ConfigurationManager.AppSettings["Security"].ToString();
            string vClassName = vAssemblyName + ".ModuleInfoBiz";
            return (IModuleInfo)Assembly.Load(vAssemblyName).CreateInstance(vClassName);
        }

        /// <summary>
        /// Initiate GroupInfo Interface
        /// </summary>
        /// <returns></returns>
        public static IGroupInfo InitiateGroupInfo()
        {
            string vAssemblyName = ConfigurationManager.AppSettings["Security"].ToString();
            string vClassName = vAssemblyName + ".GroupInfoBiz";
            return (IGroupInfo)Assembly.Load(vAssemblyName).CreateInstance(vClassName);
        }
    }
}
