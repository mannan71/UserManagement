using System;
using System.Data.Common;
using System.Collections.Generic;
using System.Collections;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using System.Web;
using System.Web.Caching;
using System.Configuration.Provider;
using SO.Provider;

namespace SOProvider
{
    public class MenuInRole
    {
        #region Private Member Variables
        private Hashtable _MenuIdInRole = new Hashtable();
        private string _ConnectionStringKey;	// Database connection string key
        private string _NameOfAnonymousRole;    // name of role that ere to be consideres anonymoyus access
        #endregion
        #region Public Properties
        #region MenuIdInRole
        /// <summary>
        /// Contains comma seperated list of role for MenuId
        /// </summary>
        public Hashtable MenuIdInRole
        {
            get
            {
                return (_MenuIdInRole);
            }
        }
        #endregion        
        #endregion        
        #region Private Constructors
        public MenuInRole(DataTable dt)
        {
            _ConnectionStringKey = AuthorizationMapping.GetAuthorizationMappingSettings().ConnectionStringKey;
            _NameOfAnonymousRole = AuthorizationMapping.GetAuthorizationMappingSettings().NameOfAnonymousRole;
        }
        #endregion
        #region Private Methods
        private void GetMenuInRole(bool checkCache, DataTable dt)
        {
            if (checkCache)
            {
                // check at cache for existence. if not exists then fetch from DB
                _MenuIdInRole = (Hashtable)HttpRuntime.Cache.Get(SecurityConstant.MenuIdInRoleCacheDependency);
            }

            if (checkCache == false || _MenuIdInRole == null)
            {
                // reinitiate as they set to null
                _MenuIdInRole = new Hashtable();
                foreach (DataRow dr in dt.Rows)
                {
                    // MenuId -> key
                    if (dr["MenuId"] == null || dr["MenuId"].ToString().Trim() == string.Empty)
                        throw new ProviderException("Missing Node Id at SoftOne.Provider.SiteMapProvider");
                    Guid menuId = new Guid(dr["MenuId"].ToString());

                    // ActionName -> Description
                    string actionPath = string.Empty;
                    if (dr["ActionPath"] == null || dr["ActionPath"].ToString().Trim() == string.Empty)
                    {
                        actionPath = string.Empty;
                    }
                    else
                    {
                        actionPath = dr["ActionPath"].ToString();
                    }

                    // roleList -> Roles
                    List<string> roleList = new List<string>();
                    string roles = SOActionInRole.ActionIsInRole(actionPath);
                    if (!String.IsNullOrEmpty(roles)) // If roles were specified, turn the list into a string array
                    {
                        string[] arrRole = roles.Split(',');
                        for (int i = 0; i < arrRole.Length; i++)
                            roleList.Add(arrRole[i]);
                    }
                    // check whether it children has some roles that it does not have. If so
                    // then add children permission to here also
                    List<string> roleListChildren = FindChildrenRole(dt, menuId);
                    // add children roles to it's role collection
                    foreach (string childRole in roleListChildren)
                    {
                        if (!roleList.Contains(childRole))
                            roleList.Add(childRole);
                    }
                    // add to hash table
                    _MenuIdInRole.Add(menuId, roleList.ToArray());
                }
                // insert into to cache
                // cache dependency is not created here. It will expire when
                // SiteMap has expired and reload data from there.
                HttpRuntime.Cache.Insert(SecurityConstant.MenuIdInRoleCacheDependency, _MenuIdInRole, null,
                    Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration);                
            }
        }        
        #region FindChildrenRole()
        private List<string> FindChildrenRole(DataTable dt, Guid menuId)
        {
            List<string> roleList = new List<string>();
            // get children row for current node/menuId
            DataRow[] dr = dt.Select("ParentMenuId='" + menuId + "'");
            // check each child for their role. Save children's role at roleList
            for (int i = 0; i < dr.Length; i++)
            {
                // MenuId -> key
                if (dr[i]["MenuId"] == null || dr[i]["MenuId"].ToString().Trim() == string.Empty)
                    throw new ProviderException("Missing Node Id at SoftOne.Provider.SiteMapProvider");
                Guid childrenMenuId = new Guid(dr[i]["MenuId"].ToString());

                // ActionPath
                string actionPath = string.Empty;
                if (dr[i]["ActionPath"] == null || dr[i]["ActionPath"].ToString().Trim() == string.Empty)
                {
                    actionPath = string.Empty;
                }
                else
                {
                    actionPath = dr[i]["ActionPath"].ToString();
                }

                // Roles
                string roles = SOActionInRole.ActionIsInRole(actionPath);
                if (!String.IsNullOrEmpty(roles)) // If roles were specified, turn the list into a string array
                {
                    string[] arrRole = roles.Split(',');
                    for (int j = 0; j < arrRole.Length; j++)
                    {
                        if (!roleList.Contains(arrRole[j]))
                            roleList.Add(arrRole[j]);
                    }
                }
                // check whether it children has some roles that it does not have. If so
                // then add children permission to here also
                List<string> roleListChildren = FindChildrenRole(dt,childrenMenuId);

                // if roleList already does not have this role, add to roleList
                foreach (string childRole in roleListChildren)
                {
                    if (!roleList.Contains(childRole))
                        roleList.Add(childRole);
                }
            }

            return roleList;
        }
        #endregion
        #endregion
        #region Public Static Methods
        /// <summary>
        /// Get an instance of ActionInRole with the Action Role list.
        /// This will return a "*" for anonymous role
        /// </summary>
        /// <param name="connectionStringKey"></param>
        /// <param name="nameOfAnonymousRole"></param>
        /// <param name="dt"></param>
        /// <param name="menuId"></param>
        /// <returns></returns>
        public static string[] MenuIsInRole(DataTable dt, Guid menuId)
        {
            MenuInRole objMenuInRole = new MenuInRole(dt);
            objMenuInRole.GetMenuInRole(true,dt);
            if (objMenuInRole.MenuIdInRole.ContainsKey(menuId))
                return (string[])(objMenuInRole.MenuIdInRole[menuId]);
            else
                return null;
        }
        /// <summary>
        /// Get an instance of ActionInRole with the Action Role list.
        /// This will return a "*" for anonymous role
        /// </summary>
        /// <param name="connectionStringKey"></param>
        /// <param name="nameOfAnonymousRole"></param>
        /// <param name="dt"></param>        
        /// <returns></returns>
        public static void GetAllMenuIsInRole(DataTable dt)
        {
            MenuInRole objMenuInRole = new MenuInRole(dt);
            objMenuInRole.GetMenuInRole(false, dt);            
        }
        #endregion
    }
}