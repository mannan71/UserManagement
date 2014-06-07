using System;
using System.Collections.Generic;
using System.Web;
using System.Data;
using Microsoft.Practices.EnterpriseLibrary.Data;
using UM.Model;
using S1.CommonBiz;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using SOProvider;
using System.Web.Security;
using S1.Common.DataContext;

namespace UM.DataContext
{
    public class RoleInfoDC
    {
        #region Methods -------------------------------------------------

        /// <summary>
        /// Get role list according to parameter
        /// If parameter is empty then get all roles of the application
        /// </summary>
        /// <param name="RoleName"></param>
        /// <returns></returns>
        public static List<RoleInfo> GetRoles(string RoleName)
        {
            List<RoleInfo> objListRoleInfo = new List<RoleInfo>();
            RoleInfo objRoleInfo;
            string sSql = @"SELECT RoleId,RoleName FROM Role";
            if (RoleName == SecurityConstant.SuperUserRole)
            {
                sSql += " WHERE RoleName Like (@RoleName)";
            }
            else
            {
                //If Role Name empty then get all roleInfo
                if (!string.IsNullOrEmpty(RoleName))
                {
                    sSql += " WHERE RoleName Like (@RoleName) and RoleName <> '"+ SecurityConstant.SuperUserRole + "'";
                }
                else
                {
                    sSql += " WHERE RoleName <> '" + SecurityConstant.SuperUserRole + "'";
                }
            }
            sSql += " ORDER BY RoleName";
            Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
            DbCommand cmd = db.GetSqlStringCommand(sSql);
            db.AddInParameter(cmd, "RoleName", DbType.String, RoleName + "%");

            using (DbConnection cn = db.CreateConnection())
            {
                cn.Open();
                try
                {
                    using (IDataReader dr = db.ExecuteReader(cmd))
                    {
                        while (dr.Read())
                        {
                            objRoleInfo = new RoleInfo();
                            objRoleInfo.RoleId_PK = dr["RoleId"].ToString();
                            objRoleInfo.RoleName = dr["RoleName"].ToString();
                            objListRoleInfo.Add(objRoleInfo);
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
            return objListRoleInfo;
        }

        /// <summary>
        /// Get specific role  by roleId
        /// </summary>
        /// <param name="RoleId"></param>
        /// <returns></returns>
        public static RoleInfo GetRoleById(string RoleId)
        {
            RoleInfo objRoleInfo = new RoleInfo();
            string sSql = @"SELECT RoleId,RoleName FROM Role
                            WHERE RoleId = @RoleId";
            Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
            DbCommand cmd = db.GetSqlStringCommand(sSql);
            db.AddInParameter(cmd, "RoleId", DbType.String, RoleId);
            using (DbConnection cn = db.CreateConnection())
            {
                cn.Open();
                try
                {
                    using (IDataReader dr = db.ExecuteReader(cmd))
                    {
                        while (dr.Read())
                        {
                            objRoleInfo.RoleId_PK = dr["RoleId"].ToString();
                            objRoleInfo.RoleName = dr["RoleName"].ToString();
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
            return objRoleInfo;
        }

        /// <summary>
        /// Save role 
        /// </summary>
        /// <param name="objRoleInfo"></param>
        /// <returns></returns>
        public static string SaveRole(RoleInfo objRoleInfo)
        {
            int vResult = 0;
            string vOut = EnumMessageId.SO251.ToString() + ": Exception Occured !";

            if (objRoleInfo.IsNew)
            {
                try
                {
                    List<RoleInfo> objExistingRoles = GetRoles(objRoleInfo.RoleName);
                    if (objExistingRoles == null || objExistingRoles.Count == 0)
                    {
                        Roles.CreateRole(objRoleInfo.RoleName);
                        vOut = EnumMessageId.SO101.ToString() + ": Role Save Successfully";
                    }
                    else
                    {
                        vOut = "::Failed to create role: " + objRoleInfo.RoleName + ". This role already exist::";
                    }

                }
                catch
                {
                    vOut = ":: Failed to create role :" + objRoleInfo.RoleName + " ::";
                }
            }
            else
            {
                RoleInfo objExistingRole = GetRoleById(objRoleInfo.RoleId_PK);

                string sSql = @"UPDATE Role
                                SET 
                                RoleName = @RoleName,                                
                                UserCode = @UserCode,
                                ActionDate = @ActionDate,
                                ActionType = @ActionType
                                Where RoleId = @RoleId";
                Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
                DbCommand cmd = db.GetSqlStringCommand(sSql);
                db.AddInParameter(cmd, "RoleName", DbType.String, objRoleInfo.RoleName);
                db.AddInParameter(cmd, "UserCode", DbType.String, SessionUtility.SessionContainer.USER_ID);
                db.AddInParameter(cmd, "ActionDate", DbType.DateTime, DateTime.Now);
                db.AddInParameter(cmd, "ActionType", DbType.String, DBAction.Update);
                db.AddInParameter(cmd, "RoleId", DbType.String, objRoleInfo.RoleId_PK);

                using (DbConnection cn = db.CreateConnection())
                {
                    cn.Open();
                    try
                    {
                        vResult = db.ExecuteNonQuery(cmd);
                        if (vResult > 0)//Executed and DB is updated
                        {
                            vOut = EnumMessageId.SO101.ToString() + ":";
                        }
                        else//Executed but did not hit to DB and also there is no exception.
                        {
                            vOut = EnumMessageId.SO200.ToString() + ": 0 Records Saved!";
                        }

                    }
                    catch(Exception ex)
                    {                       
                        bool rethrow = ExceptionPolicy.HandleException(ex, "Data Context Exception Policy");
                        if (rethrow)
                            throw new ApplicationException(FormatDCException.FormatDCMessage(ex));
                    }
                    finally
                    {
                        cn.Close();
                    }
                }
            }
            return vOut;
        }

        /// <summary>
        /// Get UserRoleList by UserId
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public static List<RoleInfo> GetUserRoleList(string UserId)
        {
            List<RoleInfo> objRoleInfoList = new List<RoleInfo>();           
            RoleInfo objRoleInfo;
            string sSql = @"SELECT DISTINCT Role.RoleId, Role.RoleName FROM Role INNER JOIN UserInRole ON Role.RoleId = UserInRole.RoleId INNER JOIN USER ON UserInRole.UserId = @UserId and UserInRole.IsDeleted ='0'  ORDER BY RoleName";            
      
            Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
            DbCommand cmd = db.GetSqlStringCommand(sSql);  
            db.AddInParameter(cmd, "UserId", DbType.String, UserId );

            using (IDataReader dr = db.ExecuteReader(cmd))
            {
                while (dr.Read())
                {
                    objRoleInfo = new RoleInfo();
                    objRoleInfo.RoleId_PK = dr["RoleId"].ToString();
                    objRoleInfo.RoleName = dr["RoleName"].ToString();
                    objRoleInfoList.Add(objRoleInfo);
                }
            }   
            return objRoleInfoList;
        }

        /// <summary>
        /// Get role wise actions
        /// </summary>
        /// <param name="RoleId"></param>
        /// <returns></returns>
        public static List<ActionInfo> GetRoleActions(string RoleId, string GroupId)
        {
            List<ActionInfo> objRoleActions = new List<ActionInfo>();
            ActionInfo objActionInfo;
            string sSql = @"SELECT DISTINCT Action.ActionId, Action.ActionName 
                            FROM Action 
                            INNER JOIN ActionInRole  
                            ON Action.ActionId = ActionInRole.ActionId 
                            AND ActionInRole.IsDeleted ='0'
                            INNER JOIN UserInRole 
                            ON ActionInRole.RoleId = @RoleId
                            AND UserInRole.UserId= @UserId 
                            AND UserInRole.IsDeleted ='0' 
                            WHERE Action.GroupId = @GroupId                          
                            ORDER BY ActionName";

            Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
            DbCommand cmd = db.GetSqlStringCommand(sSql);
            db.AddInParameter(cmd, "RoleId", DbType.String, RoleId);
            db.AddInParameter(cmd, "GroupId", DbType.String, GroupId);
            db.AddInParameter(cmd, "UserId", DbType.String, SessionUtility.SessionContainer.USER_ID);

            using (IDataReader dr = db.ExecuteReader(cmd))
            {
                while (dr.Read())
                {
                    objActionInfo = new ActionInfo();
                    objActionInfo.ActionId_PK = dr["ActionId"].ToString();
                    objActionInfo.ActionName = dr["ActionName"].ToString();
                    objRoleActions.Add(objActionInfo);
                }
            }
            return objRoleActions;           
        }

        #endregion ----------------------------------------------Methods
    }
}