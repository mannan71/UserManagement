using System;
using System.Security;
using System.Security.Principal;
using System.Web;
using System.Threading;

namespace SOProvider
{
    /// <summary>
    /// Redefine the System.Security.Principal.IPrincipal that comply with SoftOne standard. 
    /// </summary>
    public class Principal : IPrincipal
    {
        #region Private Members
        Identity _Identity;
        string[] _roles;
        #endregion
        #region Constructors
        /// <summary>
        /// private constructor to set the identity and principal
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="roles"></param>
        private Principal(Identity identity, string[] roles)
        {
            this._Identity = identity;
            this._roles = roles;

            AppDomain currentDomain = Thread.GetDomain();
            currentDomain.SetPrincipalPolicy(PrincipalPolicy.UnauthenticatedPrincipal);            
            IPrincipal oldPrincipal = Thread.CurrentPrincipal;
            HttpContext.Current.User = this;            
            try
            {
                if (!(oldPrincipal.GetType() == typeof(Principal)))
                    currentDomain.SetThreadPrincipal(this);
            }
            catch
            {
                // failed, but we don't care because there's nothing
                // we can do in this case
            }
        }
        #endregion
        #region IPrincipal Members
        #region Identity
        /// <summary>
        /// Identity of current principal
        /// </summary>
        public IIdentity Identity
        {
            get
            {
                return _Identity;
            }
        }
        #endregion
        #region IsInRole
        public bool IsInRole(string role)
        {
            return _Identity.IsInRole(role);
        }
        #endregion
        #endregion
        #region Static Methods
        public static void GetPrincipal(Identity identity, string[] roles)
        {
            new Principal(identity, roles);
        }
        #endregion
    }
}