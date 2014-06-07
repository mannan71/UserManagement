using Microsoft.Practices.EnterpriseLibrary.Data;
using SOProvider;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;

namespace SO.Provider
{
  public class SOUserInRole
    {
        #region Private Member Variables
        private Hashtable _UserInRole = new Hashtable();
        private string _ConnectionStringKey;	// Database connection string key
        private string _NameOfAnonymousRole;    // name of role that ere to be consideres anonymoyus access
        #endregion
        #region Public Properties
        /// <summary>
        /// HashTable containing user to role mapping. Roles are seperated by comma 
        /// for an user
        /// </summary>
        public Hashtable UserInRole
        {
            get
            {
                return _UserInRole;
            }
        }
        #endregion        
        #region Private Constructors
        /// <summary>
        /// private constructor
        /// </summary>
        /// <param name="connectionStringKey"></param>
        private SOUserInRole()
        {
            _ConnectionStringKey = AuthorizationMapping.GetAuthorizationMappingSettings().ConnectionStringKey;
            _NameOfAnonymousRole = AuthorizationMapping.GetAuthorizationMappingSettings().NameOfAnonymousRole;            
        }
        #endregion
        #region Private Methods
        private void GetUserInRole(bool checkCache)
        {
            if (checkCache)
            {
                // check at cache for existence. if not exists then fetch from DB
                _UserInRole = (Hashtable)HttpRuntime.Cache.Get(SecurityConstant.UserInRoleCacheDependency);
            }

            if (checkCache == false || _UserInRole == null)
            {
                // reinitiate as they set to null
                _UserInRole = new Hashtable();

                string sSql = @"SELECT UserName, RoleName
                                FROM UserInRole
                                INNER JOIN User
                                ON UserInRole.UserId = User.UserId
                                INNER JOIN Role
                                ON UserInRole.RoleId = Role.RoleId
                                WHERE UserInRole.IsDeleted = 0";

                Database db = DatabaseFactory.CreateDatabase(_ConnectionStringKey);
                DbCommand cmd = db.GetSqlStringCommand(sSql);
                using (DbConnection cn = db.CreateConnection())
                {
                    cn.Open();
                    try
                    {
                        using (IDataReader dr = db.ExecuteReader(cmd))
                        {
                            while (dr.Read())
                            {
                                string username = Convert.ToString(dr["UserName"]).ToLower();
                                string roleName = Convert.ToString(dr["RoleName"]);
                                // add to hashtable
                                if (_UserInRole.ContainsKey(username))
                                    _UserInRole[username] = _UserInRole[username].ToString() + "," + roleName;
                                else
                                    _UserInRole.Add(username, roleName);
                            }
                        }
                        // insert to cache                        
                        HttpRuntime.Cache.Insert(SecurityConstant.UserInRoleCacheDependency, _UserInRole, new UserInRoleCacheDependency(),
                                Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable,
                                new CacheItemRemovedCallback(OnUserInRoleChangedChanged));
                    }
                    catch
                    {
                        throw;
                    }
                    finally
                    {
                        cn.Close();
                    }
                }
            }
        }
        #endregion
        #region Protected Methods : OnUserInRoleChangedChanged()
        /// <summary>
        /// If the Cache is invalidated then reload the hashTable         
        /// </summary>
        /// <param name="key"></param>
        /// <param name="item"></param>
        /// <param name="reason"></param>
        void OnUserInRoleChangedChanged(string key, object item, CacheItemRemovedReason reason)
        {
            lock (this)
            {
                if (key.IndexOf(SecurityConstant.ActionInRoleCacheDependency) != -1 && reason == CacheItemRemovedReason.DependencyChanged)
                {
                    SOUserInRole objSOUserInRole = new SOUserInRole();
                    objSOUserInRole.GetUserInRole(false);
                }
            }
        }
        #endregion
        #region Static Methods
        /// <summary>
        /// Return true if the logged on user has the role
        /// </summary>		
        /// <param name="connectionStringKey">Connection string to connect to DB</param>
        /// <param name="RoleName">Role name to check</param>
        /// <returns></returns>
        public static bool UserIsInRole(string RoleName)
        {
            // get domain  user name by trimming domain name from user name
            string[] arrSpilittedValue = HttpContext.Current.User.Identity.Name.Split('\\');
            string username = arrSpilittedValue[arrSpilittedValue.Length - 1];
            if (!string.IsNullOrEmpty(username))
            {
                // get user roles and check for param role
                SOUserInRole objSOUserInRole = new SOUserInRole();
                objSOUserInRole.GetUserInRole(true);
                string[] roles = objSOUserInRole.UserInRole[username.ToLower()].ToString().Split(',');
                for (int i = 0; i < roles.Length; i++)
                {
                    if (roles[i].Equals(RoleName))
                        return true;
                }
            }
            return false;                
        }
        /// <summary>
        /// Return true  if the parameter user has role
        /// </summary>
        /// <param name="connectionStringKey"></param>
        /// <param name="RoleName"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        public static bool UserIsInRole(string RoleName, string username)
        {
            if (!string.IsNullOrEmpty(username))
            {
                // get user roles and check for param role
                SOUserInRole objSOUserInRole = new SOUserInRole();
                objSOUserInRole.GetUserInRole(true);
                string[] roles = objSOUserInRole.UserInRole[username.ToLower()].ToString().Split(',');
                for (int i = 0; i < roles.Length; i++)
                {
                    if (roles[i].Equals(RoleName))
                        return true;
                }
            }
            return false;
        }
        #endregion
    }
}
