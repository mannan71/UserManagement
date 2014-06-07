using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using System.Data;
using System.Collections;

using SOProvider;
using UM.Model;
using S1.CommonBiz;
using S1.Common.DataContext;

namespace UM.DataContext
{
    public class ActionInfoDC
    {
        #region Methods ------------------------

        /// <summary>
        ///  Get application action
        /// If parameter is empty get all action
        /// Else get action according to parameter
        /// </summary>
        /// <param name="ActionName"></param>
        /// <param name="GroupId"></param>
        /// <returns></returns>
        private static ActionInfo GetActions(string ActionName)
        {
            ActionInfo objActionInfo = new ActionInfo();
            string sSql = "SELECT ActionId, ActionName, GroupId, ActionPath From Action WHERE IsDeleted = 0";
            if (!string.IsNullOrEmpty(ActionName))
            {
                sSql += " AND ActionName = (@ActionName)";
            }           
            sSql += " ORDER BY ActionName";
            Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
            DbCommand cmd = db.GetSqlStringCommand(sSql);            
            if (!string.IsNullOrEmpty(ActionName))
            {
                db.AddInParameter(cmd, "ActionName", DbType.String, ActionName.Trim());
            }
            using (DbConnection cn = db.CreateConnection())
            {
                cn.Open();
                try
                {
                    using (IDataReader dr = db.ExecuteReader(cmd))
                    {
                        while (dr.Read())
                        {                            
                            objActionInfo.ActionId_PK = dr["ActionId"].ToString();
                            objActionInfo.ActionName = dr["ActionName"].ToString();
                            objActionInfo.GroupId = dr["GroupId"].ToString();
                            objActionInfo.ActionPath = dr["ActionPath"].ToString();                            
                        }
                    }
                }
                catch
                {
                    //Handle Execution related problem
                }
                finally
                {
                    cn.Close();
                }
            }
            return objActionInfo;
        }
        /// <summary>
        ///  Get application action
        /// If parameter is empty get all action
        /// Else get action according to parameter
        /// </summary>
        /// <param name="ActionName"></param>
        /// <param name="GroupId"></param>
        /// <returns></returns>
        public static List<ActionInfo> GetActions(string ActionName, string GroupId)
        {
            List<ActionInfo> objActionInfoList = new List<ActionInfo>();
            ActionInfo objActionInfo;
            string sSql = "SELECT ActionId, ActionName, GroupId, ActionPath From Action WHERE IsDeleted = 0";
            if (!string.IsNullOrEmpty(ActionName))
            {
                sSql += " AND ActionName Like (@ActionName)";
            }
            if (!string.IsNullOrEmpty(GroupId))
            {
                sSql += " AND GroupId =@GroupId";
            }
            sSql += " ORDER BY ActionName";
            Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
            DbCommand cmd = db.GetSqlStringCommand(sSql);
            db.AddInParameter(cmd, "GroupId", DbType.String, GroupId);
            if (!string.IsNullOrEmpty(ActionName))
            {
                db.AddInParameter(cmd, "ActionName", DbType.String, ActionName.Trim() + "%");
            }
            using (DbConnection cn = db.CreateConnection())
            {
                cn.Open();
                try
                {
                    using (IDataReader dr = db.ExecuteReader(cmd))
                    {
                        while (dr.Read())
                        {
                            objActionInfo = new ActionInfo();
                            objActionInfo.ActionId_PK = dr["ActionId"].ToString();
                            objActionInfo.ActionName = dr["ActionName"].ToString();
                            objActionInfo.GroupId = dr["GroupId"].ToString();
                            objActionInfo.ActionPath = dr["ActionPath"].ToString();
                            objActionInfoList.Add(objActionInfo);
                        }
                    }
                }
                catch
                {
                    //Handle Execution related problem
                }
                finally
                {
                    cn.Close();
                }
            }
            return objActionInfoList;
        }
        /// <summary>
        /// Get Action Groups
        /// </summary>
        /// <returns></returns>
        public static List<GroupInfo> GetActionGroups(string ModuleId)
        {
            List<GroupInfo> objGroups = new List<GroupInfo>();
            GroupInfo objGroupInfo;
            string sSql = @"Select  GroupId, GroupName
                            From Group";
            if (!string.IsNullOrEmpty(ModuleId))
            {
                sSql += " WHERE ModuleId=@ModuleId";
            }           
            Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
            DbCommand cmd = db.GetSqlStringCommand(sSql);
            db.AddInParameter(cmd, "ModuleId", DbType.String, ModuleId);
            using (DbConnection cn = db.CreateConnection())
            {
                cn.Open();
                try
                {
                    using (IDataReader dr = db.ExecuteReader(cmd))
                    {
                        while (dr.Read())
                        {
                            objGroupInfo = new GroupInfo();
                            objGroupInfo.GroupId_PK = dr["GroupId"].ToString();
                            objGroupInfo.GroupName = dr["GroupName"].ToString();
                            objGroups.Add(objGroupInfo);
                        }
                    }
                }
                catch
                {
                    //Handle Execution related problem
                }
                finally
                {
                    cn.Close();
                }
            }
            return objGroups;
        }

        /// <summary>
        /// Get action by actionID
        /// </summary>
        /// <param name="ActionId"></param>
        /// <returns></returns>
        public static ActionInfo GetActionByID(string ActionId)
        {
            ActionInfo objActionInfo = new ActionInfo();
            string sSql = "SELECT ActionId, ActionName,ActionDescription, GroupId, ActionPath, ModuleId, GroupId,RequiredLogging,LogText, IsVisibleAction FROM Action WHERE ActionId=@ActionId AND IsDeleted = 0";
            Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
            DbCommand cmd = db.GetSqlStringCommand(sSql);
            db.AddInParameter(cmd, "ActionId", DbType.String, ActionId.Trim() );
            using (DbConnection cn = db.CreateConnection())
            {
                cn.Open();
                try
                {
                    using (IDataReader dr = db.ExecuteReader(cmd))
                    {
                        while (dr.Read())
                        {
                            objActionInfo = new ActionInfo();
                            objActionInfo.ActionId_PK = dr["ActionId"].ToString();
                            objActionInfo.ActionName = dr["ActionName"].ToString();
                            if (dr["ActionDescription"] != DBNull.Value)
                            {
                                objActionInfo.ActionDescription = dr["ActionDescription"].ToString();
                            }
                            objActionInfo.GroupId = dr["GroupId"].ToString();
                            objActionInfo.ActionPath = dr["ActionPath"].ToString();
                            objActionInfo.ModuleId = dr["ModuleId"].ToString();
                            objActionInfo.GroupId = dr["GroupId"].ToString();
                            objActionInfo.RequireLogging = Convert.ToBoolean(dr["RequiredLogging"]);
                            if (dr["LogText"] != DBNull.Value)
                            {
                                objActionInfo.LogText = dr["LogText"].ToString();
                            }
                            objActionInfo.ActionRoles = GetActionRoles(objActionInfo.ActionId_PK, false);
                            objActionInfo.IsVisibleAction = Convert.ToBoolean(dr["IsVisibleAction"]);
                        }
                    }
                }
                catch
                {
                    //Handle Execution related problem
                }
                finally
                {
                    cn.Close();
                }
            }
            return objActionInfo;
        }
        
        /// <summary>
        /// Get group wise actions
        /// </summary>
        /// <param name="GroupName"></param>
        /// <returns></returns>
        public static List<ActionInfo> GetActionsByGroupId(string GroupId)
        {
            List<ActionInfo> objActionInfoList = new List<ActionInfo>();
            ActionInfo objActionInfo;
            string sSql = "SELECT ActionId, ActionName, ActionPath From Action WHERE IsDeleted = 0";
            if (!string.IsNullOrEmpty(GroupId))
            {
                sSql += " AND GroupId = @GroupId ";
            }
            sSql += " ORDER BY Action.ActionName";

            Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
            DbCommand cmd = db.GetSqlStringCommand(sSql);
            if (!string.IsNullOrEmpty(GroupId))
            {
                db.AddInParameter(cmd, "GroupId", DbType.String, GroupId);
            }
            using (DbConnection cn = db.CreateConnection())
            {
                cn.Open();
                try
                {
                    using (IDataReader dr = db.ExecuteReader(cmd))
                    {
                        while (dr.Read())
                        {
                            objActionInfo = new ActionInfo();
                            objActionInfo.ActionId_PK = dr["ActionId"].ToString();
                            objActionInfo.ActionName = dr["ActionName"].ToString();
                            objActionInfo.ActionPath = dr["ActionPath"].ToString();
                            objActionInfoList.Add(objActionInfo);
                        }
                    }
                }
                catch
                {
                    //Handle Execution related problem
                }
                finally
                {
                    cn.Close();
                }
            }
            return objActionInfoList;
        }

        /// <summary>
        /// Get Action Name
        /// </summary>
        /// <param name="ActionId"></param>
        /// <returns></returns>
        public static string GetActionName(string ActionId)
        {
            string sSql = "SELECT ActionId, ActionName FROM Action WHERE ActionId=@ActionId AND IsDeleted = 0";
            Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
            DbCommand cmd = db.GetSqlStringCommand(sSql);
            db.AddInParameter(cmd, "ActionId", DbType.String, ActionId);
            using (DbConnection cn = db.CreateConnection())
            {
                cn.Open();
                try
                {
                    using (IDataReader dr = db.ExecuteReader(cmd))
                    {
                        while (dr.Read())
                        {
                            return dr["ActionName"].ToString();
                        }
                    }
                }
                catch
                {
                    //Handle Execution related problem
                }
                finally
                {
                    cn.Close();
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Save action data
        /// </summary>
        /// <param name="objAction"></param>
        /// <returns></returns>
        public static string SaveAction(ActionInfo objAction)
        {
            int vResult = 0;
            string vOut = EnumMessageId.SO251.ToString() + ": Exception Occured !";

            Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
            StringBuilder vComText = new StringBuilder();

            if (objAction.IsNew)
            {
                ActionInfo ExistingAction = GetActions(objAction.ActionName);
                if (ExistingAction == null || string.IsNullOrEmpty(ExistingAction.ActionId_PK))
                {
                    vComText.Append("INSERT INTO Action(UserCode, ActionDate, ActionType, ActionId, ActionName, ActionDescription, ModuleId, GroupId, ActionPath,IsVisibleAction,RequiredLogging,LogText,IsDeleted)");
                    vComText.Append("VALUES");
                    vComText.Append("(@UserCode,@ActionDate,@ActionType,@ActionId,@ActionName,@ActionDescription,@ModuleId, @GroupId,@ActionPath,@IsVisibleAction,@RequiredLogging,@LogText,@IsDeleted)");
                }
                else
                {
                    vOut = EnumMessageId.SO251.ToString() + ":Failed to Create Action: '" + objAction.ActionName + "'. Action already exist";
                }
            }
            else
            {
                vComText.Append("Update Action SET ActionDate =@ActionDate , ActionType =@ActionType, ActionName = @ActionName,ActionDescription=@ActionDescription, ModuleId=@ModuleId, GroupId=@GroupId, ActionPath = @ActionPath,RequiredLogging=@RequiredLogging,LogText=@LogText, IsVisibleAction = @IsVisibleAction");
                vComText.Append(" WHERE ActionId=@ActionId");
            }

            DbCommand cmd = db.GetSqlStringCommand(vComText.ToString());
            db.AddInParameter(cmd, "UserCode", DbType.String, SessionUtility.SessionContainer.USER_ID);
            db.AddInParameter(cmd, "ActionDate", DbType.DateTime, DateTime.Now);
            db.AddInParameter(cmd, "ActionType", DbType.String, "Insert");
            db.AddInParameter(cmd, "ActionId", DbType.String, objAction.ActionId_PK);
            db.AddInParameter(cmd, "ActionName", DbType.String, objAction.ActionName);
            db.AddInParameter(cmd, "ActionDescription", DbType.String, objAction.ActionDescription);
            db.AddInParameter(cmd, "ModuleId", DbType.String, objAction.ModuleId);
            db.AddInParameter(cmd, "GroupId", DbType.String, objAction.GroupId);
            db.AddInParameter(cmd, "ActionPath", DbType.String, objAction.ActionPath);
            db.AddInParameter(cmd, "IsVisibleAction", DbType.Decimal, Convert.ToBoolean(objAction.IsVisibleAction));
            db.AddInParameter(cmd, "RequiredLogging", DbType.Decimal, Convert.ToBoolean(objAction.RequireLogging));
            db.AddInParameter(cmd, "LogText", DbType.String, objAction.LogText);
            db.AddInParameter(cmd, "IsDeleted", DbType.Decimal, Convert.ToBoolean(objAction.IsDeleted));
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
                catch (Exception ex)
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
            //Clear cache
            SOSession.FlushCacheDependency();
            return vOut;
        }

        /// <summary>
        /// Delete action data
        /// </summary>
        /// <param name="ActionId"></param>
        /// <returns></returns>
        public static string DeleteAction(string ActionId)
        {
            int vResult = 0;
            string vOut = EnumMessageId.SO251.ToString() + ": Exception Occured !";

            ActionInfo objActionInfo = new ActionInfo();
            objActionInfo.TableName_TBL = "Action";
            objActionInfo.ActionId_PK = ActionId;
            objActionInfo.IsDeleted = true;           
            if (CommonBaseDC.DeleteCheck(objActionInfo))
            {               
                string sSql = " UPDATE Action SET UserCode=@UserCode,ActionDate=@ActionDate,IsDeleted=@IsDeleted WHERE ActionId=@ActionId";
                Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
                DbCommand cmd = db.GetSqlStringCommand(sSql);
                db.AddInParameter(cmd, "UserCode", DbType.String, SessionUtility.SessionContainer.USER_ID);
                db.AddInParameter(cmd, "ActionDate", DbType.DateTime, DateTime.Now);
                db.AddInParameter(cmd, "IsDeleted", DbType.Decimal, 1);
                db.AddInParameter(cmd, "ActionId", DbType.String, ActionId.ToString());
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
                    catch (Exception ex)
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
            else
            {
                vOut = ":: Failed to Delete Action on account of foreign key reference. ::";
            }
            //Clear cache
            SOSession.FlushCacheDependency();
            return vOut;
        }

        /// <summary>
        /// Save action specific roles
        /// </summary>
        /// <param name="objActionInRoleList"></param>
        /// <returns></returns>
        public static string SaveActionRoles(List<ActionInRole> objActionInRoleList)
        {
            int vResult = 0;
            string vOut = EnumMessageId.SO251.ToString() + ": Exception Occured !";
            StringBuilder vComText ;

            ActionInRole objActionInRole;
            Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
            using (DbConnection cn = db.CreateConnection())
            {
                cn.Open();
                DbTransaction tr = cn.BeginTransaction();
                try
                {
                    for (int i = 0; i < objActionInRoleList.Count; i++)
                    {
                        vComText = new StringBuilder();
                        objActionInRole = objActionInRoleList[i];
                        if (objActionInRole.IsNew)
                        {
                            vComText.Append("INSERT INTO ActionInRole  (UserCode, ActionDate, ActionType ,ActionId ,RoleId ,IsDeleted ,ActionRoleId)");
                            vComText.Append("VALUES");
                            vComText.Append("(@UserCode, @ActionDate, @ActionType ,@ActionId ,@RoleId ,@IsDeleted ,@ActionRoleId)");
                        }
                        //Update roles which removed from the action
                        //Also Update roles which removed from action and again add
                        else
                        {
                            if (objActionInRole.IsDirty)
                            {
                                if (objActionInRole.IsDeleted)
                                {
                                    vComText.Append("UPDATE ActionInRole SET UserCode=@UserCode, ActionDate=@ActionDate, ActionType=@ActionType, IsDeleted = '0'   Where ActionRoleId = @ActionRoleId");
                                }
                            }
                            else
                            {
                                vComText.Append("UPDATE ActionInRole SET UserCode=@UserCode, ActionDate=@ActionDate, ActionType=@ActionType, IsDeleted = '1'   Where ActionRoleId = @ActionRoleId");
                            }
                        }

                        DbCommand cmd = db.GetSqlStringCommand(vComText.ToString());
                        db.AddInParameter(cmd, "UserCode", DbType.String, SessionUtility.SessionContainer.USER_ID);
                        db.AddInParameter(cmd, "ActionDate", DbType.DateTime, DateTime.Now);
                        if (objActionInRole.IsNew)
                        {
                            db.AddInParameter(cmd, "ActionType", DbType.String, "Insert");
                        }
                        else
                        {
                            db.AddInParameter(cmd, "ActionType", DbType.String, "Update");
                        }
                        db.AddInParameter(cmd, "ActionId", DbType.String, objActionInRole.ActionId);
                        db.AddInParameter(cmd, "RoleId", DbType.String, objActionInRole.RoleId);
                        db.AddInParameter(cmd, "IsDeleted", DbType.Decimal, Convert.ToBoolean(objActionInRole.IsDeleted));
                        db.AddInParameter(cmd, "ActionRoleId", DbType.String, objActionInRole.ActionRoleId_PK);


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
                    tr.Commit();
                }
                catch (Exception ex)
                {
                    tr.Rollback();
                    bool rethrow = ExceptionPolicy.HandleException(ex, "Data Context Exception Policy");
                    if (rethrow)
                        throw new ApplicationException(FormatDCException.FormatDCMessage(ex));
                }
                finally
                {
                    cn.Close();
                }

            }
            //Clear cache of the user
            SOSession.FlushCacheDependency();
            return vOut;
        }

        /// <summary>
        /// Save role specific actions
        /// </summary>
        /// <param name="objActionInRoleList"></param>
        /// <returns></returns>
        public static string SaveRoleActions(List<ActionInRole> objActionInRoleList, string GroupId)
        {
            int vResult = 0;
            string vOut = EnumMessageId.SO251.ToString() + ": Exception Occured !";

            ActionInRole objActionInRole;
            Hashtable htExistingActions = new Hashtable();
            Hashtable htNewActionList = new Hashtable();

            Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);

            //Get all Existing Actions of the specific Role
            List<ActionInRole> objExistingRoles = GetRoleActions(objActionInRoleList[0].RoleId, GroupId);
            if (objExistingRoles.Count > 0)
            {
                foreach (ActionInRole objTempActionInRole in objExistingRoles)
                {
                    if (!htExistingActions.Contains(objTempActionInRole.ActionId))
                    {
                        htExistingActions.Add(objTempActionInRole.ActionId, objTempActionInRole);
                    }
                }
            }
            //Get all Upcoming Roles of the Action
            if (objActionInRoleList.Count > 0)
            {
                foreach (ActionInRole objTempActionInRole in objActionInRoleList)
                {
                    if (!string.IsNullOrEmpty(objTempActionInRole.ActionId) && !htNewActionList.Contains(objTempActionInRole.ActionId))
                    {
                        htNewActionList.Add(objTempActionInRole.ActionId, objTempActionInRole);
                    }
                }
            }

            //Create new roles of the action
            for (int i = 0; i < objActionInRoleList.Count; i++)
            {
                objActionInRole = objActionInRoleList[i];
                if (!string.IsNullOrEmpty(objActionInRole.ActionId) && !htExistingActions.Contains(objActionInRole.ActionId))
                {
                    string sSQL = "INSERT INTO ActionInRole  (UserCode, ActionDate, ActionType ,ActionId ,RoleId ,IsDeleted ,ActionRoleId)";
                    sSQL += "VALUES";
                    sSQL += "(@UserCode, @ActionDate, @ActionType ,@ActionId ,@RoleId ,@IsDeleted ,@ActionRoleId)";

                    DbCommand cmd = db.GetSqlStringCommand(sSQL);
                    db.AddInParameter(cmd, "UserCode", DbType.String, SessionUtility.SessionContainer.USER_ID);
                    db.AddInParameter(cmd, "ActionDate", DbType.DateTime, DateTime.Now);
                    db.AddInParameter(cmd, "ActionType", DbType.String, "Insert");
                    db.AddInParameter(cmd, "ActionId", DbType.String, objActionInRole.ActionId);
                    db.AddInParameter(cmd, "RoleId", DbType.String, objActionInRole.RoleId);
                    db.AddInParameter(cmd, "IsDeleted", DbType.Decimal, Convert.ToBoolean(objActionInRole.IsDeleted));
                    db.AddInParameter(cmd, "ActionRoleId", DbType.String, objActionInRole.ActionRoleId_PK);
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
                        catch (Exception ex)
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
            }            
            //Update roles which removed from the action 
            for (int i = 0; i < objExistingRoles.Count; i++)
            {
                objActionInRole = objExistingRoles[i];
                if (!htNewActionList.Contains(objActionInRole.ActionId))
                {
                    string sSqlUserRole = "UPDATE ActionInRole SET UserCode=@UserCode, ActionDate=@ActionDate, ActionType=@ActionType, IsDeleted = '1'   Where ActionRoleId = @ActionRoleId";
                    DbCommand cmdUserRole = db.GetSqlStringCommand(sSqlUserRole);
                    db.AddInParameter(cmdUserRole, "ActionRoleId", DbType.String, objActionInRole.ActionRoleId_PK);
                    db.AddInParameter(cmdUserRole, "UserCode", DbType.String, SessionUtility.SessionContainer.USER_ID);
                    db.AddInParameter(cmdUserRole, "ActionDate", DbType.DateTime, DateTime.Now);
                    db.AddInParameter(cmdUserRole, "ActionType", DbType.String, "Update");
                    using (DbConnection cn = db.CreateConnection())
                    {
                        cn.Open();

                        try
                        {
                            vResult = db.ExecuteNonQuery(cmdUserRole);
                            if (vResult > 0)//Executed and DB is updated
                            {
                                vOut = EnumMessageId.SO101.ToString() + ":";
                            }
                            else//Executed but did not hit to DB and also there is no exception.
                            {
                                vOut = EnumMessageId.SO200.ToString() + ": 0 Records Saved!";
                            }
                        }
                        catch (Exception ex)
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
            }

            //Clear cache of the user
            SOSession.FlushCacheDependency();
            return vOut;
        }

        /// <summary>
        /// Get all roles of the action
        /// </summary>
        /// <param name="ActionId"></param>
        /// <returns></returns>
        public static List<ActionInRole> GetActionRoles(string ActionId, bool pReAssign)
        {
            List<ActionInRole> objActionInRoleList = new List<ActionInRole>();
            ActionInRole objActionInRole;
            StringBuilder vComText = new StringBuilder();
            vComText.Append("SELECT ActionId, RoleId, ActionRoleId, IsDeleted from ActionInRole WHERE ActionId=@ActionId");
            if(!pReAssign)
            {
                vComText.Append(" AND IsDeleted = 0");
            }
            Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
            DbCommand cmd = db.GetSqlStringCommand(vComText.ToString());
            db.AddInParameter(cmd, "ActionId", DbType.String, ActionId.Trim() );
            using (DbConnection cn = db.CreateConnection())
            {
                cn.Open();
                try
                {
                    using (IDataReader dr = db.ExecuteReader(cmd))
                    {
                        while (dr.Read())
                        {
                            objActionInRole = new ActionInRole();
                            objActionInRole.ActionId = dr["ActionId"].ToString();
                            objActionInRole.RoleId = dr["RoleId"].ToString();
                            objActionInRole.ActionRoleId_PK = dr["ActionRoleId"].ToString();
                            objActionInRole.IsDeleted = Convert.ToBoolean(dr["IsDeleted"]);
                            objActionInRoleList.Add(objActionInRole);
                        }
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
            return objActionInRoleList;
        }

        /// <summary>
        /// Get all actions of the role
        /// </summary>
        /// <param name="RoleId"></param>
        /// <returns></returns>
        public static List<ActionInRole> GetRoleActions(string RoleId)
        {
            return GetRoleActions(RoleId, string.Empty);
        }

       /// <summary>
        /// Get actions according of the roleId and GroupId
       /// </summary>
       /// <param name="RoleId"></param>
        /// <param name="GroupId"></param>
       /// <returns></returns>
        public static List<ActionInRole> GetRoleActions(string RoleId, string GroupId)
        {
            List<ActionInRole> objActionInRoleList = new List<ActionInRole>();
            ActionInRole objActionInRole;
            string sSql = string.Empty;
            if (!string.IsNullOrEmpty(GroupId))
            {
                sSql = "SELECT ActionInRole.ActionId, ActionInRole.RoleId, ActionRoleId from ActionInRole INNER JOIN Action ON ActionInRole.ActionId = Action.ActionId AND Action.GroupId=@GroupId WHERE RoleId=@RoleId AND ActionInRole.IsDeleted = 0";
            }
            else
            {
                sSql = "SELECT ActionId, RoleId, ActionRoleId from ActionInRole WHERE RoleId=@RoleId AND IsDeleted = 0";
            }
            Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
            DbCommand cmd = db.GetSqlStringCommand(sSql);
            db.AddInParameter(cmd, "RoleId", DbType.String, RoleId);
            db.AddInParameter(cmd, "GroupId", DbType.String, GroupId);            
            using (DbConnection cn = db.CreateConnection())
            {
                cn.Open();
                try
                {
                    using (IDataReader dr = db.ExecuteReader(cmd))
                    {
                        while (dr.Read())
                        {
                            objActionInRole = new ActionInRole();
                            objActionInRole.ActionId = dr["ActionId"].ToString();
                            objActionInRole.RoleId = dr["RoleId"].ToString();
                            objActionInRole.ActionRoleId_PK = dr["ActionRoleId"].ToString();
                            objActionInRoleList.Add(objActionInRole);
                        }
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
            return objActionInRoleList;
        }
        #endregion ----------------------Methods
    }
}
