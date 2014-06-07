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
   public class SOActionInRole
    { 
        private Hashtable _ActionInRole = new Hashtable();
        private string _ConnectionStringKey;	// Database connection string key
        private string _NameOfAnonymousRole;    // name of role that ere to be consideres anonymoyus access
        
        
        
        /// <summary>
        /// Contains comma seperated list of role for ActionName
        /// </summary>
        public Hashtable ActionInRole
        {
            get
            {
                return (_ActionInRole);
            }
        }
        
        
        
        /// <summary>
        /// private constructor to crearte
        /// </summary>
        /// <param name="connectionStringKey"></param>
        /// <param name="nameOfAnonymousRole"></param>
        private SOActionInRole()
        {
            _ConnectionStringKey = AuthorizationMapping.GetAuthorizationMappingSettings().ConnectionStringKey;
            _NameOfAnonymousRole = AuthorizationMapping.GetAuthorizationMappingSettings().NameOfAnonymousRole;            
        }
        
        
        private void GetActionInRole(bool checkCache)
        {
            if (checkCache)
            {
                // check at cache for existence. if not exists then fetch from DB            
                _ActionInRole = (Hashtable)HttpRuntime.Cache.Get(SecurityConstant.ActionInRoleCacheDependency);
            }

            if (checkCache == false || _ActionInRole == null)
            {
                // reinitiate as they set to null                
                _ActionInRole = new Hashtable();

                string sSql = @"SELECT Action.ActionPath,Role.RoleName
                                FROM ActionInRole
                                INNER JOIN Action
                                ON ActionInRole.ActionId = Action.ActionId
                                INNER JOIN LS_Role
                                ON ActionInRole.RoleId = Role.RoleId
                                WHERE ActionInRole.IsDeleted = 0
                                Order By ActionPath";

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
                                string actionPath = Convert.ToString(dr["ActionPath"]);
                                string roleName = Convert.ToString(dr["RoleName"]);
                                // add to hashtable
                                if (_ActionInRole.ContainsKey(actionPath))
                                {

                                    _ActionInRole[actionPath] = (roleName == _NameOfAnonymousRole) ? _NameOfAnonymousRole : _ActionInRole[actionPath].ToString() + "," + roleName;
                                }
                                else
                                {
                                    _ActionInRole.Add(actionPath, roleName == _NameOfAnonymousRole ? _NameOfAnonymousRole : roleName);
                                }
                            }
                        }
                        // insert to cache                                                
                        HttpRuntime.Cache.Insert(SecurityConstant.ActionInRoleCacheDependency, _ActionInRole, new ActionInRoleCacheDependency(),
                                Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable,
                                new CacheItemRemovedCallback(OnActionInRoleChangedChanged));
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
        
        
        /// <summary>
        /// If the Cache is invalidated then reload the hashTable         
        /// </summary>
        /// <param name="key"></param>
        /// <param name="item"></param>
        /// <param name="reason"></param>
        void OnActionInRoleChangedChanged(string key, object item, CacheItemRemovedReason reason)
        {
            lock (this)
            {
                if (key.IndexOf(SecurityConstant.ActionInRoleCacheDependency) != -1 && reason == CacheItemRemovedReason.DependencyChanged)
                {
                    SOActionInRole objSOActionInRole = new SOActionInRole();
                    objSOActionInRole.GetActionInRole(false);
                }
            }
        }
        
        
        /// <summary>
        /// Get an instance of LSActionInRole with the Action Role list.
        /// </summary>
        /// <param name="connectionStringKey"></param>
        /// <param name="nameOfAnonymousRole"></param>
        /// <param name="actionName"></param>
        /// <returns></returns>
        public static string ActionIsInRole(string actionPath)
        {
            SOActionInRole objSOActionInRole = new SOActionInRole();
            objSOActionInRole.GetActionInRole(true);
            if (objSOActionInRole.ActionInRole.ContainsKey(actionPath))
            {
                return (objSOActionInRole.ActionInRole[actionPath].ToString());
            }
            else
                return string.Empty;
        }
        
    }
}
