using System;
using System.Security;
using System.Security.Principal;
using System.Web.Security;
using System.Collections;

namespace SOProvider
{
    /// <summary>
    /// Redefine the System.Security.Principal.Identity that comply with SoftOne standard.
    /// </summary>
    public class Identity : IIdentity
    {
        #region Private Members
        private string _username;
        private FormsAuthenticationTicket _tckt;
        ArrayList _roles = new ArrayList();
        #endregion
        #region Constructor
        /// <summary>
        /// private constructor to set the authentication ticket and username
        /// </summary>
        /// <param name="tckt"></param>
        private Identity(FormsAuthenticationTicket tckt)
        {
            this._tckt = tckt;
            this._username = tckt.Name;
        }
        #endregion
        #region IIdentity Members
        #region AuthenticationType
        /// <summary>
        /// Authentication type
        /// </summary>
        public string AuthenticationType
        {
            get 
            {
                return "SoftOne";
            }
        }
        #endregion
        #region IsAuthenticated
        /// <summary>
        /// Whether the user is authenticated or not
        /// </summary>
        public bool IsAuthenticated
        {
            get
            {
                return (_username.Length > 0);
            }
        }
        #endregion
        #region Name
        /// <summary>
        /// Return the user name
        /// </summary>
        public string Name
        {
            get 
            {
                return _username;
            }
        }
        #endregion
        #endregion
        #region Internal Methods
        internal bool IsInRole(string role)
        {
            return _roles.Contains(role);
        }
        #endregion
        #region Static Methods
        public static Identity GetIdentity(FormsAuthenticationTicket tckt)
        {
            return new Identity(tckt);
        }
        #endregion
    }
}