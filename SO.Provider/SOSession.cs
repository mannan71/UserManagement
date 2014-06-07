using System;
using System.Web;
using System.Web.Security;
using System.Security.Principal;
using System.Threading;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data.Common;
using System.Data;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using S1.CommonBiz;
using S1.Common.Model;
using S1.Common.DataContext;

namespace SOProvider
{
    /// <summary>
    /// Contains the static Session specific members and methods.
    /// </summary>
    public class SOSession
    {
        #region UserCode
        public static string UserCode
        {
            get
            {
                return SessionUtility.SessionContainer.USER_ID;
            }
        }
        #endregion
        #region RequiredChangePassword
        private static bool _RequiredChangePassword;
        /// <summary>
        /// whether user need to change password.
        /// if true redirect user to change password window.
        /// </summary>
        public static bool RequiredChangePassword
        {
            set
            {
                _RequiredChangePassword = value;
            }
            get
            {
                return _RequiredChangePassword;
            }
        }
        #endregion
        #region Static Methods
        #region Internal
        #region Login()
        /// <summary>
        /// Set user Identity & Principal along with FormAuthenticationTicket
        /// </summary>
        /// <param name="username"></param>
        internal static void Login(string userId, string username, string connectionStringKey)
        {
            // Getting the Roles from Role Provider
            string[] roles = Roles.GetRolesForUser(username);
            // Initialize the Forms Authentication according to configuration settings.
            FormsAuthentication.Initialize();
            // Creating Forms Authentication ticket.
            // This ticket will work through out the browser life time.
            FormsAuthenticationTicket tckt = new FormsAuthenticationTicket(username, true, SecurityConstant.FormAuthenticationTicketLifetime);
            FormsAuthentication.SetAuthCookie(username, false);
            // Creating an Identity obejct with the ticket
            Identity identity = Identity.GetIdentity(tckt);
            // Setting Principal with the Identity and Roles
            Principal.GetPrincipal(identity, roles);
            // Get current user to fill session
            GetUserInfoByName(username, connectionStringKey);
            //Get Defult Warehouse Code For User
            GetWarehouseCodeForUser(userId);
            // Get current user WorkingUnit hierarchy
            GetWorkingUnitHierarchy(connectionStringKey);
            //DataRow row = SessionUtility.SessionContainer.WORKING_UNIT_COLLECTION.Rows[SessionUtility.SessionContainer.WORKING_UNIT_COLLECTION.Rows.Count - 1];
            //SessionUtility.SessionContainer.COMPANY_CODE = row["WorkingUnitCode"].ToString();
        }
        /// <summary>
        /// Get Defult Warehouse Code For User
        /// Add by Abdul Mannan at 11-Feb-2013
        /// </summary>
        /// <param name="userId"></param>
        private static void GetWarehouseCodeForUser(string userId)
        {
            try
            {
                List<WarehouseWiseUserMap> objWarehouseWiseUserMap = WarehouseWiseUserMapDC.GetWarehouseUserMapByUserID(userId);
                string vWarehouseCode = "";
                if (objWarehouseWiseUserMap.Count > 0)
                    vWarehouseCode = objWarehouseWiseUserMap.Find(vWWUP => vWWUP.IsDefault == 1).WarehouseCode;
                SessionUtility.SessionContainer.WAREHOUSE_CODE = vWarehouseCode;
            }
            catch
            {
                SessionUtility.SessionContainer.WAREHOUSE_CODE = null;
            }
        }
        public string GetWarehouseName(string pWarehouseCode)
        {
            List<WarehouseWiseUserMap> objWarehouseWiseUserMap = WarehouseWiseUserMapDC.GetWarehouseUserMapByUserID(SessionUtility.SessionContainer.USER_ID);
            string WarehouseName = "";
            //TODO: fixing bug -563 by Shahrima on 26-11-2013
            if (String.IsNullOrEmpty(pWarehouseCode))
                pWarehouseCode = SessionUtility.SessionContainer.WAREHOUSE_CODE;
            if (objWarehouseWiseUserMap.Count > 0 && !String.IsNullOrEmpty(pWarehouseCode))
            {
                WarehouseName = objWarehouseWiseUserMap.Find(vWWUP => vWWUP.WarehouseCode == pWarehouseCode).WareHouseName_VW;
                return "Warehouse:" + WarehouseName + "(" + pWarehouseCode + ")";
            }
            
            return string.Empty;
        }
        #endregion
        #endregion
        #region Public
        #region Logout()
        /// <summary>
        /// Clear user Identity & Principal along with FormAuthenticationTicket -- think to make it internal
        /// </summary>
        public static void Logout()
        {
            // create Empty Roles
            string[] roles = { "", "" };
            // create Empty Identity
            GenericIdentity identity = new System.Security.Principal.GenericIdentity("", "");
            // creating Empty Pricipal
            GenericPrincipal pricipal = new System.Security.Principal.GenericPrincipal(identity, roles);
            // assigning Empty Principal
            Thread.CurrentPrincipal = pricipal;
            HttpContext.Current.User = pricipal;
            // expiring FormAuthentication
            FormsAuthentication.SignOut();
            // removing all session values
            HttpContext.Current.Session.Abandon();
            //Remove Session data
            SOSession.RequiredChangePassword = false;
        }
        #endregion
        #region FlushCache()
        /// <summary>
        /// Flush Cache related to security context.
        /// </summary>
        public static void FlushCacheDependency()
        {
            try
            {
                // ActionInRole
                string path = ConfigurationManager.AppSettings["CACHE_DEPENDENCY_FILE_PATH"] + SecurityConstant.ActionInRoleCacheDependency + @"\" + SecurityConstant.ActionInRoleCacheDependency;
                if (File.Exists(path))
                    File.Delete(path);
                FileStream fsAction = File.Create(path);
                fsAction.Close();
                // UserInRole
                path = ConfigurationManager.AppSettings["CACHE_DEPENDENCY_FILE_PATH"] + SecurityConstant.UserInRoleCacheDependency + @"\" + SecurityConstant.UserInRoleCacheDependency;
                if (File.Exists(path))
                    File.Delete(path);
                FileStream fsUser = File.Create(path);
                fsUser.Close();
                // SiteMap
                path = ConfigurationManager.AppSettings["CACHE_DEPENDENCY_FILE_PATH"] + SecurityConstant.SiteMapCacheDependency + @"\" + SecurityConstant.SiteMapCacheDependency;
                if (File.Exists(path))
                    File.Delete(path);
                FileStream fsSiteMap = File.Create(path);
                fsSiteMap.Close();
            }
            catch
            {
                throw;
            }
            /*
            // do not break the order
            HttpContext.Current.Cache.Remove(SecurityConstant.SiteMapCacheDependency);
            HttpContext.Current.Cache.Remove(SecurityConstant.MenuIdInRoleCacheDependency);
            HttpContext.Current.Cache.Remove(SecurityConstant.ActionInRoleCacheDependency);
            HttpContext.Current.Cache.Remove(SecurityConstant.UserInRoleCacheDependency);
            */

        }
        #endregion
        #region WorkingUnit
        /// <summary>
        /// Getting Working Unit by Working Unit Patern
        /// </summary>
        /// <param name="workingUnitPatern"></param>
        /// <returns></returns>
        public int? GetUserWorkingUnitByPattern(string workingUnitPatern)
        {
            if (SessionUtility.SessionContainer.WORKING_UNIT_COLLECTION != null)
            {
                DataTable dtSession = SessionUtility.SessionContainer.WORKING_UNIT_COLLECTION;
                DataRow[] arDataRow = dtSession.Select("WorkingUnitPatern='" + workingUnitPatern + "'");
                //DataRow dr = dtSession.Select("WorkingUnitPatern='" + workingUnitPatern + "'")[0];
                if (arDataRow != null && arDataRow.Length > 0)
                {
                    DataRow dr = arDataRow[0];
                    return Convert.ToInt32(dr["WorkingUnitCode"]);
                }
            }
            return null;
        }

        /// <summary>
        /// Get WorkingUnitName/CompanyName according to pattern
        /// </summary>
        /// <param name="workingUnitPatern"></param>
        /// <returns></returns>
        public string GetUserWorkingUnitNameByPattern(string workingUnitPatern)
        {
            if (SessionUtility.SessionContainer.WORKING_UNIT_COLLECTION != null)
            {
                DataTable dtSession = SessionUtility.SessionContainer.WORKING_UNIT_COLLECTION;
                DataRow[] arDataRow = dtSession.Select("WorkingUnitPatern='" + workingUnitPatern + "'");
                //DataRow dr = dtSession.Select("WorkingUnitPatern='" + workingUnitPatern + "'")[0];
                if (arDataRow != null && arDataRow.Length > 0)
                {
                    DataRow dr = arDataRow[0];
                    return dr["WorkingUnitName"].ToString();
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Get WorkingUnitAddress/CompanyAddress according to pattern
        /// </summary>
        /// <param name="workingUnitPatern"></param>
        /// <returns></returns>
        public string GetUserWorkingUnitAddressByPattern(string workingUnitPatern)
        {
            if (SessionUtility.SessionContainer.WORKING_UNIT_COLLECTION != null)
            {
                DataTable dtSession = SessionUtility.SessionContainer.WORKING_UNIT_COLLECTION;
                DataRow[] arDataRow = dtSession.Select("WorkingUnitPatern='" + workingUnitPatern + "'");
                //DataRow dr = dtSession.Select("WorkingUnitPatern='" + workingUnitPatern + "'")[0];
                if (arDataRow != null && arDataRow.Length > 0)
                {
                    DataRow dr = arDataRow[0];
                    return dr["WorkingUnitAddress"].ToString();
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Getting Working Unit by Working Unit Type
        /// </summary>
        /// <param name="workingUnitType"></param>
        /// <returns></returns>
        public int? GetUserWorkingUnitByType(string workingUnitType)
        {
            if (SessionUtility.SessionContainer.WORKING_UNIT_COLLECTION != null)
            {
                DataTable dtSession = SessionUtility.SessionContainer.WORKING_UNIT_COLLECTION;
                DataRow[] arDataRow = dtSession.Select("WorkingUnitType='" + workingUnitType + "'");
                //DataRow dr = dtSession.Select("WorkingUnitType='" + workingUnitType + "'")[0];
                if (arDataRow != null && arDataRow.Length > 0)
                {
                    DataRow dr = arDataRow[0];
                    return Convert.ToInt32(dr["WorkingUnitCode"]);
                }
            }
            return null;
        }

        /// <summary>
        /// Get WorkingUnitName/CompanyName according to Type
        /// </summary>
        /// <param name="workingUnitType"></param>
        /// <returns></returns>
        public string GetUserWorkingUnitNameByType(string workingUnitType)
        {
            if (SessionUtility.SessionContainer.WORKING_UNIT_COLLECTION != null)
            {
                DataTable dtSession = SessionUtility.SessionContainer.WORKING_UNIT_COLLECTION;
                DataRow[] arDataRow = dtSession.Select("WorkingUnitType='" + workingUnitType + "'");
                //DataRow dr = dtSession.Select("WorkingUnitType='" + workingUnitType + "'")[0];
                if (arDataRow != null && arDataRow.Length > 0)
                {
                    DataRow dr = arDataRow[0];
                    return dr["WorkingUnitName"].ToString();
                }
            }
            return string.Empty;
        }

        /// <summary>
        ///  Get WorkingUnitAddress/CompanyAddress according to Type
        /// </summary>
        /// <param name="workingUnitType"></param>
        /// <returns></returns>
        public string GetUserWorkingUnitAddressByType(string workingUnitType)
        {
            if (SessionUtility.SessionContainer.WORKING_UNIT_COLLECTION != null)
            {
                DataTable dtSession = SessionUtility.SessionContainer.WORKING_UNIT_COLLECTION;
                DataRow[] arDataRow = dtSession.Select("WorkingUnitType='" + workingUnitType + "'");
                //DataRow dr = dtSession.Select("WorkingUnitType='" + workingUnitType + "'")[0];
                if (arDataRow != null && arDataRow.Length > 0)
                {
                    DataRow dr = arDataRow[0];
                    return dr["WorkingUnitAddress"].ToString();
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Get working unit name by working unit code
        /// </summary>
        /// <param name="WorkingUnitCode"></param>
        /// <returns></returns>
        public string GetUserWorkingUnitName(string WorkingUnitCode)
        {
            if (SessionUtility.SessionContainer.WORKING_UNIT_COLLECTION != null)
            {
                DataTable dtSession = SessionUtility.SessionContainer.WORKING_UNIT_COLLECTION;
                DataRow[] arDataRow = dtSession.Select("WorkingUnitCode='" + WorkingUnitCode + "'");
                if (arDataRow != null && arDataRow.Length > 0)
                {
                    DataRow dr = arDataRow[0];
                    return dr["WorkingUnitName"].ToString();
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Get working unit Id by working unit code
        /// </summary>
        /// <param name="WorkingUnitCode"></param>
        /// <returns></returns>
        public string GetUserWorkingUnitId(string WorkingUnitCode)
        {
            if (SessionUtility.SessionContainer.WORKING_UNIT_COLLECTION != null)
            {
                DataTable dtSession = SessionUtility.SessionContainer.WORKING_UNIT_COLLECTION;
                DataRow[] arDataRow = dtSession.Select("WorkingUnitCode='" + WorkingUnitCode + "'");
                if (arDataRow != null && arDataRow.Length > 0)
                {
                    DataRow dr = arDataRow[0];
                    return dr["WorkingUnitId"].ToString();
                }
            }
            return string.Empty;
        }
        #endregion
        #endregion
        #region Private
        #region GetUserInfoByName()
        /// <summary>
        /// Get UserInfo
        /// </summary>
        /// <param name="UserName"></param>
        /// <returns></returns>
        private static void GetUserInfoByName(string UserName, string connectionStringKey)
        {
            string sSql = @"select DISTINCT User.UserId, User.UserName, 
                            Membership.Email,Membership.IsLockedOut, 
                            UserInfo.CompanyCode,UserInfo.WorkingUnitCode , WorkingUnit.WorkingUnitId
                            from User 
                            Inner Join Membership 
                            on User.UserId = Membership.UserId 
                            INNER JOIN UserInfo 
                            ON User.UserId = UserInfo.UserId 
                            INNER JOIN WorkingUnit 
                            ON UserInfo.WorkingUnitCode = WorkingUnit.WorkingUnitCode
                            WHERE User.UserName = @UserName ";
            Database db = DatabaseFactory.CreateDatabase(connectionStringKey);
            DbCommand cmd = db.GetSqlStringCommand(sSql);
            db.AddInParameter(cmd, "UserName", DbType.String, UserName);
            using (DbConnection cn = db.CreateConnection())
            {
                cn.Open();
                try
                {
                    using (IDataReader dr = db.ExecuteReader(cmd))
                    {
                        while (dr.Read())
                        {
                            // Set value to Session                
                            SessionUtility.SessionContainer.USER_ID = dr["UserId"].ToString();
                            SessionUtility.SessionContainer.USER_NM = dr["UserName"].ToString();
                            SessionUtility.SessionContainer.COMPANY_CODE = Convert.ToInt32(dr["CompanyCode"]).ToString();
                            SessionUtility.SessionContainer.WORKING_UNIT_CODE = Convert.ToInt32(dr["WorkingUnitCode"]).ToString();
                            SessionUtility.SessionContainer.WORKING_UNIT_ID = dr["WorkingUnitId"].ToString();
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
        }

        #endregion
        #region Working Unit Hierarchy
        /// <summary>
        /// Retrieve working unit hierarchy
        /// </summary>
        /// <param name="connectionStringKey"></param>
        private static void GetWorkingUnitHierarchy(string connectionStringKey)
        {
            // fetch from DB and fill DataTable
            string sSql = @"SELECT WorkingUnitCode, WorkingUnitId, ParentUnitCode,WorkingUnitName,WorkingUnitPatern,WorkingUnitType,WorkingUnitAddress
                            FROM WorkingUnit
                            WHERE IsDeleted = 0";
            DataTable dtCurrent = new DataTable();
            Database db = DatabaseFactory.CreateDatabase(connectionStringKey);
            DbCommand cmd = db.GetSqlStringCommand(sSql);
            using (DbConnection cn = db.CreateConnection())
            {
                cn.Open();
                try
                {
                    dtCurrent = (db.ExecuteDataSet(cmd)).Tables[0];
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

            // create the DataTable structure for kept into Session
            DataTable dtSession = new DataTable();
            dtSession.Columns.Add("WorkingUnitCode");
            dtSession.Columns.Add("WorkingUnitId");
            dtSession.Columns.Add("WorkingUnitName");
            dtSession.Columns.Add("WorkingUnitAddress");
            dtSession.Columns.Add("WorkingUnitPatern");
            dtSession.Columns.Add("WorkingUnitType");

            // get user working unit code data
            int workingUnitCode = Convert.ToInt32(SessionUtility.SessionContainer.WORKING_UNIT_CODE);
            DataRow drCurrent = dtCurrent.Select("WorkingUnitCode='" + workingUnitCode + "'")[0];
            int parentUnitCode = Convert.ToInt32(drCurrent["ParentUnitCode"]);

            // create the current row
            DataRow drSession = dtSession.NewRow();
            drSession["WorkingUnitCode"] = drCurrent["workingUnitCode"];
            drSession["WorkingUnitId"] = drCurrent["WorkingUnitId"];
            drSession["WorkingUnitName"] = drCurrent["WorkingUnitName"];
            drSession["WorkingUnitAddress"] = drCurrent["WorkingUnitAddress"];
            drSession["WorkingUnitPatern"] = drCurrent["WorkingUnitPatern"];
            drSession["WorkingUnitType"] = drCurrent["WorkingUnitType"];
            dtSession.Rows.Add(drSession);

            // search for parent detail if any and add to table
            if (parentUnitCode != 0)
                dtSession.Rows.Add(GetWorkingUnitHierarchyParent(parentUnitCode, dtCurrent, dtSession));

            SessionUtility.SessionContainer.WORKING_UNIT_COLLECTION = dtSession;
        }
        /// <summary>
        /// Find parent info of current unit
        /// </summary>
        /// <param name="workingUnitCode"></param>
        /// <param name="dtCurrent"></param>
        /// <param name="dtSession"></param>
        /// <returns></returns>
        private static DataRow GetWorkingUnitHierarchyParent(int workingUnitCode, DataTable dtCurrent, DataTable dtSession)
        {
            // get user working unit code data
            DataRow drCurrent = dtCurrent.Select("WorkingUnitCode='" + workingUnitCode + "'")[0];
            int parentUnitCode = Convert.ToInt32(drCurrent["ParentUnitCode"]);

            DataRow drSession = dtSession.NewRow();
            drSession["WorkingUnitCode"] = drCurrent["workingUnitCode"];
            drSession["WorkingUnitId"] = drCurrent["WorkingUnitId"];
            drSession["WorkingUnitName"] = drCurrent["WorkingUnitName"];
            drSession["WorkingUnitAddress"] = drCurrent["WorkingUnitAddress"];
            drSession["WorkingUnitPatern"] = drCurrent["WorkingUnitPatern"];
            drSession["WorkingUnitType"] = drCurrent["WorkingUnitType"];

            // search for parent detail if any and add to table
            if (parentUnitCode != 0)
                dtSession.Rows.Add(GetWorkingUnitHierarchyParent(parentUnitCode, dtCurrent, dtSession));

            return drSession;
        }
        #endregion
        #endregion
        #endregion
    }
}