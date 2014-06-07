using Microsoft.Practices.EnterpriseLibrary.Data;
using SOProvider;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Security;

namespace SO.Provider
{
    public class SORoleProvider : RoleProvider
    {
        #region Private Members
        private string _ApplicationName;
        private string _ConnectionString;
        private string _ConnectionStringKey;
        #endregion
        #region Overridden Properties
        public override string ApplicationName
        {
            get
            {
                return _ApplicationName;
            }
            set
            {
                _ApplicationName = value;
            }
        }
        #endregion
        #region Initialize()
        /// <summary>
        /// initialize the provider by reading setting from configuration file
        /// </summary>
        /// <param name="name"></param>
        /// <param name="config"></param>
        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
                throw new ArgumentNullException("web.config setting is invalid for SoftOne.Provider.RoleProvider");

            if (string.IsNullOrEmpty(name))
                name = "RoleProvider";

            if (String.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "RoleProvider");
            }

            // get Role Provider setting from web.config
            _ApplicationName = string.IsNullOrEmpty(config["applicationName"]) ? HostingEnvironment.ApplicationVirtualPath : config["applicationName"].ToString();

            // get ConnectionString from web.config
            ConnectionStringSettings objConnectionStringSettings = ConfigurationManager.ConnectionStrings[config["connectionStringName"]];
            if (objConnectionStringSettings == null || string.IsNullOrEmpty(objConnectionStringSettings.ConnectionString.Trim()))
            {
                throw new ProviderException("Connection string cannot be blank at SoftOne.Provider.RoleProvider");
            }
            else
            {
                _ConnectionString = objConnectionStringSettings.ConnectionString;
                _ConnectionStringKey = config["connectionStringName"].ToString();
            }

            // call base method
            base.Initialize(name, config);
        }
        #endregion
        #region Overridden Methods
        #region AddUsersToRoles
        /// <summary>
        /// add list of users to a list of roles
        /// </summary>
        /// <param name="usernames">List of users</param>
        /// <param name="roleNames">List of roles</param>
        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            // check whether roleName exist
            foreach (string roleName in roleNames)
            {
                if (!RoleExists(roleName))
                {
                    throw new ProviderException("Role name not found at SoftOne.Provider.RoleProvider: " + roleName);
                }
            }

            // check whether userName exist and whether user is already assigned the role
            foreach (string userName in usernames)
            {
                if (userName.Contains(","))
                {
                    throw new ArgumentException("User names cannot contain commas at SoftOne.Provider.RoleProvider");
                }
                if (string.IsNullOrEmpty(GetUserId(userName)))
                {
                    throw new ArgumentException("User name does not exist at SoftOne.Provider.RoleProvider");
                }
                foreach (string rolename in roleNames)
                {
                    if (IsUserInRole(userName, rolename))
                    {
                        throw new ProviderException("User is already in role at SoftOne.Provider.RoleProvider.\n" + "username:" + userName + "\nrolename:" + rolename);
                    }
                }
            }

            // insert user and role foreach 
            string sSql = @"INSERT INTO UserInRole
                            (RoleId,UserId,UserCode,ActionDate,ActionType,Password)
                            VALUES
                            (@RoleId,@RoleName,@UserCode,@ActionDate,@ActionType,@Password)";

            Database db = DatabaseFactory.CreateDatabase(_ConnectionStringKey);
            DbCommand cmd = db.GetSqlStringCommand(sSql);
            using (DbConnection cn = db.CreateConnection())
            {
                cn.Open();
                DbTransaction tr = cn.BeginTransaction();
                try
                {
                    foreach (string username in usernames)
                    {
                        foreach (string roleName in roleNames)
                        {
                            db.AddInParameter(cmd, "RoleId", DbType.String, GetRoleId(roleName));
                            db.AddInParameter(cmd, "UserId", DbType.String, GetUserId(username));
                            db.AddInParameter(cmd, "UserCode", DbType.String, SOSession.UserCode);
                            db.AddInParameter(cmd, "ActionDate", DbType.DateTime, DateTime.Now);
                            db.AddInParameter(cmd, "ActionType", DbType.String, DBAction.Insert.ToString());

                            db.ExecuteNonQuery(cmd);
                        }
                    }
                    tr.Commit();
                }
                catch
                {
                    tr.Rollback();
                    throw;
                }
                finally
                {
                    cn.Close();
                }
            }
        }
        #endregion
        #region CreateRole
        /// <summary>
        /// Create a new role
        /// </summary>
        /// <param name="roleName"></param>
        public override void CreateRole(string roleName)
        {
            if (roleName.Contains(","))
            {
                throw new ArgumentException("Role names cannot contain commas at SoftOne.Provider.RoleProvider");
            }
            if (RoleExists(roleName))
            {
                throw new ProviderException("Role name already exists at SoftOne.Provider.RoleProvider");
            }

            string sSql = @"INSERT INTO Role
                            (RoleId,RoleName,UserCode,ActionDate,ActionType)
                            VALUES
                            (@RoleId,@RoleName,@UserCode,@ActionDate,@ActionType)";

            Database db = DatabaseFactory.CreateDatabase(_ConnectionStringKey);
            DbCommand cmd = db.GetSqlStringCommand(sSql);
            db.AddInParameter(cmd, "RoleId", DbType.String, Guid.NewGuid().ToString());
            db.AddInParameter(cmd, "RoleName", DbType.String, roleName);
            db.AddInParameter(cmd, "UserCode", DbType.String, SOSession.UserCode);
            db.AddInParameter(cmd, "ActionDate", DbType.DateTime, DateTime.Now);
            db.AddInParameter(cmd, "ActionType", DbType.String, DBAction.Insert.ToString());

            using (DbConnection cn = db.CreateConnection())
            {
                cn.Open();
                try
                {
                    db.ExecuteNonQuery(cmd);
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
        #endregion
        #region DeleteRole()
        /// <summary>
        /// Delete a role.
        /// This method is not supported.
        /// </summary>
        /// <param name="roleName">name of role</param>
        /// <param name="throwOnPopulatedRole">whether throw exception if role already has user</param>
        /// <returns></returns>
        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotSupportedException("Deletion of role is not supported at SoftOne.Provider.RoleProvider");
        }
        #endregion
        #region FindUsersInRole()
        /// <summary>
        /// Find users in a role where username like the usernameToMatch.
        /// This method is not supported.
        /// </summary>
        /// <param name="roleName"></param>
        /// <param name="usernameToMatch"></param>
        /// <returns></returns>
        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotSupportedException("The method FindUsersInRole is not supported at SoftOne.Provider.RoleProvider");
        }
        #endregion
        #region GetAllRoles()
        /// <summary>
        /// get the list of available roles
        /// </summary>
        /// <returns></returns>
        public override string[] GetAllRoles()
        {
            List<string> li = new List<string>();

            string sSql = @"SELECT Role.RoleName,
                            FROM Role";

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
                            li.Add(Convert.ToString(dr["RoleName"]));
                        }
                    }
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
            return li.ToArray();
        }
        #endregion
        #region GetRolesForUser()
        /// <summary>
        /// Get roles list for user
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public override string[] GetRolesForUser(string username)
        {
            List<string> li = new List<string>();

            string sSql = @"SELECT Role.RoleName
                            FROM Role
                            INNER JOIN UserInRole
                            ON Role.RoleId = UserInRole.RoleId
                            WHERE UserInRole.UserId = @UserId
                            AND IsDeleted <> 1";

            Database db = DatabaseFactory.CreateDatabase(_ConnectionStringKey);
            DbCommand cmd = db.GetSqlStringCommand(sSql);
            db.AddInParameter(cmd, "UserId", DbType.String, GetUserId(username));

            using (DbConnection cn = db.CreateConnection())
            {
                cn.Open();
                try
                {
                    using (IDataReader dr = db.ExecuteReader(cmd))
                    {
                        while (dr.Read())
                        {
                            li.Add(Convert.ToString(dr["RoleName"]));
                        }
                    }
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
            return li.ToArray();
        }
        #endregion
        #region GetUsersInRole()
        /// <summary>
        /// Get users in a role
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public override string[] GetUsersInRole(string roleName)
        {
            List<string> li = new List<string>();

            string sSql = @"SELECT User.UserName,
                            FROM User
                            INNER JOIN UserInRole
                            WHERE User.UserId = UserInRole.UserId
                            WHERE RoleId = @RoleId";

            Database db = DatabaseFactory.CreateDatabase(_ConnectionStringKey);
            DbCommand cmd = db.GetSqlStringCommand(sSql);
            db.AddInParameter(cmd, "RoleId", DbType.String, GetRoleId(roleName));

            using (DbConnection cn = db.CreateConnection())
            {
                cn.Open();
                try
                {
                    using (IDataReader dr = db.ExecuteReader(cmd))
                    {
                        while (dr.Read())
                        {
                            li.Add(Convert.ToString(dr["UserName"]));
                        }
                    }
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
            return li.ToArray();
        }
        #endregion
        #region IsUserInRole
        /// <summary>
        /// whetehr the user belongs to the role or not
        /// </summary>
        /// <param name="username"></param>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public override bool IsUserInRole(string username, string roleName)
        {
            /* Get the data from UserInRole. Because this do automatic caching and 
               avoid unnecessary database fetch. However, the performance is also
               boosted by setting cacheRolesInCookie="true" at Role provider at config
               however cookie has size limitation so our cahce mechanism works when
               cookie is disabled or role is higher than max cookie length */
            return SOUserInRole.UserIsInRole(roleName);
        }
        #endregion
        #region RemoveUsersFromRoles()
        /// <summary>
        /// Remove list of users from list of roles
        /// </summary>
        /// <param name="usernames"></param>
        /// <param name="roleNames"></param>
        /// <remarks>As this is a direct deletion from DB so no record can be kept
        /// for the delete time and user action</remarks>
        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            // check whether roleName exist
            foreach (string roleName in roleNames)
            {
                if (!RoleExists(roleName))
                {
                    throw new ProviderException("Role name not found at SoftOne.Provider.RoleProvider: " + roleName);
                }
            }

            // check whether userName exist and whether user is already assigned the role
            foreach (string userName in usernames)
            {
                if (userName.Contains(","))
                {
                    throw new ArgumentException("User names cannot contain commas at SoftOne.Provider.RoleProvider");
                }
                if (string.IsNullOrEmpty(GetUserId(userName)))
                {
                    throw new ArgumentException("User name does not exist at SoftOne.Provider.RoleProvider");
                }
                foreach (string rolename in roleNames)
                {
                    if (IsUserInRole(userName, rolename))
                    {
                        throw new ProviderException("User is already in role at SoftOne.Provider.RoleProvider.\n" + "username:" + userName + "\nrolename:" + rolename);
                    }
                }
            }

            // delete user and role foreach 
            string sSql = @"DELETE FROM UserInRole
                            WHERE UserId = @UserId
                            AND RoleId = @RoleId";

            Database db = DatabaseFactory.CreateDatabase(_ConnectionStringKey);
            DbCommand cmd = db.GetSqlStringCommand(sSql);
            using (DbConnection cn = db.CreateConnection())
            {
                cn.Open();
                DbTransaction tr = cn.BeginTransaction();
                try
                {
                    foreach (string username in usernames)
                    {
                        foreach (string roleName in roleNames)
                        {
                            db.AddInParameter(cmd, "UserId", DbType.String, GetUserId(username));
                            db.AddInParameter(cmd, "RoleId", DbType.String, GetRoleId(roleName));
                            db.ExecuteNonQuery(cmd);
                        }
                    }
                    tr.Commit();
                }
                catch
                {
                    tr.Rollback();
                    throw;
                }
                finally
                {
                    cn.Close();
                }
            }
        }
        #endregion
        #region RoleExists()
        /// <summary>
        /// Whether this role exists
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public override bool RoleExists(string roleName)
        {
            if (!string.IsNullOrEmpty(GetRoleId(roleName)))
                return true;
            return false;
        }
        #endregion
        #endregion
        #region Private Helper Methods
        #region GetUserId()
        /// <summary>
        /// Get an UserId
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private string GetUserId(string username)
        {
            string userId = string.Empty;
            string sSql = @"SELECT User.UserId                            
                            FROM User                             
                            WHERE User.UserName = @UserName";

            Database db = DatabaseFactory.CreateDatabase(_ConnectionStringKey);
            DbCommand cmd = db.GetSqlStringCommand(sSql);
            db.AddInParameter(cmd, "UserName", DbType.String, username);
            using (DbConnection cn = db.CreateConnection())
            {
                cn.Open();
                try
                {
                    using (IDataReader dr = db.ExecuteReader(cmd))
                    {
                        while (dr.Read())
                        {
                            userId = Convert.ToString(dr["UserId"]);
                        }
                    }
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
            return userId;
        }
        #endregion
        #region GetRoleId()
        /// <summary>
        /// Get a RoleId
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        private string GetRoleId(string roleName)
        {
            string roleId = string.Empty;
            string sSql = @"SELECT Role.RoleId                             
                            FROM Role                             
                            WHERE Role.RoleName = @RoleName";

            Database db = DatabaseFactory.CreateDatabase(_ConnectionStringKey);
            DbCommand cmd = db.GetSqlStringCommand(sSql);
            db.AddInParameter(cmd, "RoleName", DbType.String, roleName);
            using (DbConnection cn = db.CreateConnection())
            {
                cn.Open();
                try
                {
                    using (IDataReader dr = db.ExecuteReader(cmd))
                    {
                        while (dr.Read())
                        {
                            roleId = Convert.ToString(dr["RoleName"]);
                        }
                    }
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
            return roleId;
        }
        #endregion
        #endregion
    }
}
