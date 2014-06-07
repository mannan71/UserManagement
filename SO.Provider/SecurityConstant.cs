using System;
using System.Configuration;

namespace SOProvider
{
    public class SecurityConstant
    {
        #region Internal Cache Constant for Security Dependency
        /// <summary>
        /// cache the SiteMap
        /// </summary>
        internal const string SiteMapCacheDependency = "__SiteMapCacheDependency";
        /// <summary>
        /// cache the MenuId with role
        /// </summary>
        internal const string MenuIdInRoleCacheDependency = "__MenuIdInRoleCacheDependency";
        /// <summary>
        /// cache the Action with role
        /// </summary>
        internal const string ActionInRoleCacheDependency = "__ActionInRoleCacheDependency";
        /// <summary>
        /// cache username with role
        /// </summary>
        internal const string UserInRoleCacheDependency = "__UserInRoleCacheDependency";
        #endregion
        #region Internal Constant FormAuthenticationTicketLifetime
        /// <summary>
        /// Lifetime for FormAuthenticationTicket. Set to 2 hours i,e, 120 minutes 
        /// </summary>
        internal const int FormAuthenticationTicketLifetime = 120;
        #endregion
        #region Public Constant RewriteURL        
        /// <summary>
        /// This will rewrite the url according to IIS setting at web.config.
        /// </summary>
        public static string RewriteURL = ConfigurationManager.AppSettings["REWRITEURL"].ToString();
        #endregion
        #region Public Constant SuperUserName
        /// <summary>
        /// Name of the super user to set system parameter and configuration.
        /// This user will not be available for customer.
        /// </summary>
        public const string SuperUserName = "softone";
        #endregion
        #region Public Constant SuperUserRole
        /// <summary>
        /// Name of the super user role to set system parameter and configuration
        /// This role will not be available for custoemr.
        /// </summary>
        public const string SuperUserRole = "SuperAdmin";
        #endregion
        #region Public Constant SysUserRole
        /// <summary>
        /// Name of the system user role.
        /// This role will be available to IT user usually.
        /// </summary>
        public const string SysUserRole = "SysAdmin";
        #endregion

    }
}