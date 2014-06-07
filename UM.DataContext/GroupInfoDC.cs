using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data.Common;
using System.Data;
using SOProvider;
using UM.Model;
using S1.CommonBiz;

namespace UM.DataContext
{
    /// <summary>
    /// Data Context class to collaborate with GroupInfo Object
    /// </summary>
    public class GroupInfoDC
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ModuleId"></param>
        /// <returns></returns>
        public static List<GroupInfo> GetGroupListByModuleId(string ModuleId)
        {
            List<GroupInfo> objGroupInfoList = new List<GroupInfo>();
            GroupInfo objGroupInfo;
            string Sql = @"Select GroupId, GroupName, ModuleId
                            From Group
                            WHERE ModuleId=@ModuleId";
            Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
            DbCommand cmd = db.GetSqlStringCommand(Sql);
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
                            objGroupInfo.ModuleId = dr["ModuleId"].ToString();
                            objGroupInfoList.Add(objGroupInfo);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
                finally
                {
                    cn.Close();
                }
            }
            return objGroupInfoList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objGroupInfo"></param>
        /// <returns></returns>
        public static string SaveGroupInfo(GroupInfo objGroupInfo)
        {
            GroupInfo objExistingGroupInfo = GetGroupByName(objGroupInfo.GroupName);
            if (objGroupInfo.IsNew)
            {
                if (objExistingGroupInfo == null || string.IsNullOrEmpty(objExistingGroupInfo.GroupId_PK))
                {

                    string Sql = @"INSERT INTO Group (GroupId, UserCode, ActionDate, ActionType, ModuleId, GroupName)
                           VALUES (@GroupId, @UserCode, @ActionDate, @ActionType, @ModuleId, @GroupName)";
                    Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
                    DbCommand cmd = db.GetSqlStringCommand(Sql);
                    db.AddInParameter(cmd, "GroupId", DbType.String, objGroupInfo.GroupId_PK);
                    db.AddInParameter(cmd, "UserCode", DbType.String, SessionUtility.SessionContainer.USER_ID);
                    db.AddInParameter(cmd, "ActionDate", DbType.DateTime, objGroupInfo.ActionDate);
                    db.AddInParameter(cmd, "ActionType", DbType.String, DBAction.Insert.ToString());
                    db.AddInParameter(cmd, "ModuleId", DbType.String, objGroupInfo.ModuleId);
                    db.AddInParameter(cmd, "GroupName", DbType.String, objGroupInfo.GroupName);
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
                else
                {
                    return ":: Faled to create Group :" +objGroupInfo.GroupName+ ". This group already exist.";
                }
            }
            else
            {
                string Sql = @"UPDATE Group SET 
                                UserCode    = @UserCode,
                                ActionDate  = @ActionDate,
                                ActionType  = @ActionType,
                                ModuleId    = @ModuleId,
                                GroupName   = @GroupName
                                WHERE GroupId = @GroupId";
                Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
                DbCommand cmd = db.GetSqlStringCommand(Sql);
                db.AddInParameter(cmd, "GroupId", DbType.String, objGroupInfo.GroupId_PK);
                db.AddInParameter(cmd, "UserCode", DbType.String, SessionUtility.SessionContainer.USER_ID);
                db.AddInParameter(cmd, "ActionDate", DbType.DateTime, objGroupInfo.ActionDate);
                db.AddInParameter(cmd, "ActionType", DbType.String, DBAction.Update.ToString());
                db.AddInParameter(cmd, "ModuleId", DbType.String, objGroupInfo.ModuleId);
                db.AddInParameter(cmd, "GroupName", DbType.String, objGroupInfo.GroupName);
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
           
            return string.Empty;
        }

        public static GroupInfo GetGroupById(string GroupId)
        {
            GroupInfo objGroupInfo = new GroupInfo();
            string Sql = @"Select GroupId, GroupName, ModuleId
                            From Group
                            WHERE GroupId=@GroupId";
            Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
            DbCommand cmd = db.GetSqlStringCommand(Sql);
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
                            objGroupInfo = new GroupInfo();
                            objGroupInfo.GroupId_PK = dr["GroupId"].ToString();
                            objGroupInfo.GroupName = dr["GroupName"].ToString();
                            objGroupInfo.ModuleId = dr["ModuleId"].ToString();                          
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
            return objGroupInfo;
        }

        protected static GroupInfo GetGroupByName(string GroupName)
        {
            GroupInfo objGroupInfo = new GroupInfo();
            string Sql = @"Select GroupId, GroupName, ModuleId
                            From Group
                            WHERE GroupName=@GroupName";
            Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
            DbCommand cmd = db.GetSqlStringCommand(Sql);
            db.AddInParameter(cmd, "GroupName", DbType.String, GroupName);
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
                            objGroupInfo.ModuleId = dr["ModuleId"].ToString();
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
            return objGroupInfo;
        }

    }
}
