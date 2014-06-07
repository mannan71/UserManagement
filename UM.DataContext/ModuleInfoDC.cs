using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UM.Model;
using System.Data;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using S1.CommonBiz;
using SOProvider;


namespace UM.DataContext
{
    /// <summary>
    /// Data Context Object for Module entity
    /// </summary>
    public class ModuleInfoDC
    {
        /// <summary>
        /// Get all modules of the application
        /// </summary>
        /// <returns></returns>
        public static List<ModuleInfo> GetModuleList()
        {
            List<ModuleInfo> objModuleInfoList = new List<ModuleInfo>();
            ModuleInfo objModuleInfo;
            string Sql = @"Select ModuleId, ModuleName, SortOrder
                            From Module
                            Order By ModuleName ";
             Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
            DbCommand cmd = db.GetSqlStringCommand(Sql);
            using (DbConnection cn = db.CreateConnection())
            {
                cn.Open();
                try
                {
                    using (IDataReader dr = db.ExecuteReader(cmd))
                    {
                        while (dr.Read())
                        {
                            objModuleInfo = new ModuleInfo();
                            objModuleInfo.ModuleId_PK = dr["ModuleId"].ToString();
                            objModuleInfo.ModuleName = dr["ModuleName"].ToString();
                            objModuleInfo.SortOrder = Convert.ToInt32(dr["SortOrder"]);
                            objModuleInfoList.Add(objModuleInfo);
                        }
                    }
                }
                catch(Exception ex)
                {
                    throw;
                }
                finally
                {
                    cn.Close();
                }
            }
            return objModuleInfoList;
        }

        /// <summary>
        /// Get Specific module by ModuleId
        /// </summary>
        /// <param name="ModuleId"></param>
        /// <returns></returns>
        public static ModuleInfo GetModuleById(string ModuleId)
        {
            ModuleInfo objModuleInfo = new ModuleInfo();
            string Sql = @"Select ModuleId, ModuleName, SortOrder --, RowId
                            From Module 
                            WHERE ModuleId=@ModuleId ";
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
                            objModuleInfo = new ModuleInfo();
                            objModuleInfo.ModuleId_PK = dr["ModuleId"].ToString();
                            objModuleInfo.ModuleName = dr["ModuleName"].ToString();
                            objModuleInfo.SortOrder = Convert.ToInt32(dr["SortOrder"]);
                            //objModuleInfo.RowId = dr["RowId"].ToString();
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
            return objModuleInfo;
        }

        /// <summary>
        /// Save Models to DB
        /// </summary>
        /// <param name="objModuleInfo"></param>
        /// <returns></returns>
        public static string SaveModule(ModuleInfo objModuleInfo)
        {
            objModuleInfo.TableName_TBL = "Module";
            if (objModuleInfo.IsNew)
            {
                //Add modules to DB

                ModuleInfo objExistingModuleInfo = GetModuleByName(objModuleInfo.ModuleName);
                if (objExistingModuleInfo == null || string.IsNullOrEmpty(objExistingModuleInfo.ModuleId_PK))
                {
                    //string Sql = @"INSERT INTO Module (ModuleId, UserCode, ActionDate, ActionType, ModuleName, SortOrder, RowId )
                    //                VALUES (@ModuleId, @UserCode, @ActionDate, @ActionType, @ModuleName, @SortOrder, @RowId)";
                    string Sql = @"INSERT INTO Module (ModuleId, UserCode, ActionDate, ActionType, ModuleName, SortOrder)
                                    VALUES (@ModuleId, @UserCode, @ActionDate, @ActionType, @ModuleName, @SortOrder)";
                    Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
                    DbCommand cmd = db.GetSqlStringCommand(Sql);
                    objModuleInfo.RowId = Guid.NewGuid().ToString();
                    db.AddInParameter(cmd, "ModuleId", DbType.String, objModuleInfo.ModuleId_PK);
                    db.AddInParameter(cmd, "UserCode", DbType.String, SessionUtility.SessionContainer.USER_ID);
                    db.AddInParameter(cmd, "ActionDate", DbType.DateTime, objModuleInfo.ActionDate);
                    db.AddInParameter(cmd, "ActionType", DbType.String, DBAction.Insert.ToString());
                    db.AddInParameter(cmd, "ModuleName", DbType.String, objModuleInfo.ModuleName);
                    db.AddInParameter(cmd, "SortOrder", DbType.Decimal, objModuleInfo.SortOrder);
                    //db.AddInParameter(cmd, "RowId", DbType.String, objModuleInfo.RowId);
                    using (DbConnection cn = db.CreateConnection())
                    {
                        cn.Open();
                        // For logging
                        DbTransaction transaction = cn.BeginTransaction();
                        try
                        {
                            db.ExecuteNonQuery(cmd,transaction);
                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
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
                    return ":: Failed to create Module : " + objModuleInfo.ModuleName + ". Module already exist.";
                }
            }
            else
            {
                ModuleInfo objModuleInfo_old = GetModuleById(objModuleInfo.ModuleId_PK);
                //Update Module Information
                string Sql = @"UPDATE Module SET 
                                UserCode = @UserCode, 
                                ActionDate = @ActionDate, 
                                ActionType = @ActionType, 
                                ModuleName = @ModuleName, 
                                SortOrder = @SortOrder                                   
                                WHERE ModuleId=@ModuleId";
                Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
                DbCommand cmd = db.GetSqlStringCommand(Sql);
                db.AddInParameter(cmd, "ModuleId", DbType.String, objModuleInfo.ModuleId_PK);
                db.AddInParameter(cmd, "UserCode", DbType.String, SessionUtility.SessionContainer.USER_ID);
                db.AddInParameter(cmd, "ActionDate", DbType.DateTime, objModuleInfo.ActionDate);
                db.AddInParameter(cmd, "ActionType", DbType.String, DBAction.Update.ToString());
                db.AddInParameter(cmd, "ModuleName", DbType.String, objModuleInfo.ModuleName);
                db.AddInParameter(cmd, "SortOrder", DbType.Decimal, objModuleInfo.SortOrder);
                using (DbConnection cn = db.CreateConnection())
                {
                    cn.Open();
                    //For Logging
                    DbTransaction transaction = cn.BeginTransaction();
                    try
                    {
                        db.ExecuteNonQuery(cmd, transaction);

                          transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
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
        protected static ModuleInfo GetModuleByName(string ModuleName)
        {
            ModuleInfo objModuleInfo = new ModuleInfo();
            string Sql = @"Select ModuleId, ModuleName, SortOrder
                            From Module 
                            WHERE ModuleName=@ModuleName ";
            Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
            DbCommand cmd = db.GetSqlStringCommand(Sql);
            db.AddInParameter(cmd, "ModuleName", DbType.String, ModuleName);
            using (DbConnection cn = db.CreateConnection())
            {
                cn.Open();
                try
                {
                    using (IDataReader dr = db.ExecuteReader(cmd))
                    {
                        while (dr.Read())
                        {
                            objModuleInfo = new ModuleInfo();
                            objModuleInfo.ModuleId_PK = dr["ModuleId"].ToString();
                            objModuleInfo.ModuleName = dr["ModuleName"].ToString();
                            objModuleInfo.SortOrder = Convert.ToInt32(dr["SortOrder"]);
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
            return objModuleInfo;
        }
    }
}
