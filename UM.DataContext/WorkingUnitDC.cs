using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UM.Model;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data.Common;
using System.Data;
using S1.CommonBiz;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using SOProvider;
using S1.Common.DataContext;

namespace UM.DataContext
{
    public class WorkingUnitDC
    {
        #region Methods --------------------

        #region WoringUnit -----------------

        /// <summary>
        /// Gat all WorkingUnitCode of application
        /// </summary>
        /// <returns></returns>
        public static List<WorkingUnit> GenerateWorkingUnitTree()
        {
            List<WorkingUnit> objWorkingUnitList = new List<WorkingUnit>();
            WorkingUnit objWorkingUnit;
            string sSql = "SELECT WorkingUnit.UserCode, WorkingUnit.WorkingUnitCode, WorkingUnit.WorkingUnitName, WorkingUnit.ParentUnitCode, WorkingUnit.WorkingUnitPatern, WorkingUnit.WorkingUnitType, WorkingUnit.WorkingUnitAddress  FROM WorkingUnit WHERE IsDeleted = 0";           
            Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
            DbCommand cmd = db.GetSqlStringCommand(sSql);          

            try
            {
                using (DbConnection cn = db.CreateConnection())
                {
                    cn.Open();
                    try
                    {
                        using (IDataReader dr = db.ExecuteReader(cmd))
                        {
                            while (dr.Read())
                            {
                                objWorkingUnit = new WorkingUnit();
                                objWorkingUnit.UserCode = dr["UserCode"].ToString();
                                objWorkingUnit.WorkingUnitCode_PK = Convert.ToInt32(dr["WorkingUnitCode"]);
                                objWorkingUnit.WorkingUnitName = dr["WorkingUnitName"].ToString();
                                if (!string.IsNullOrEmpty(dr["ParentUnitCode"].ToString()))
                                {
                                    objWorkingUnit.ParentUnitCode = Convert.ToInt32(dr["ParentUnitCode"]);
                                }                                
                                objWorkingUnit.WorkingUnitPatern = dr["WorkingUnitPatern"].ToString();
                                objWorkingUnit.WorkingUnitType = dr["WorkingUnitType"].ToString();
                                if (dr["WorkingUnitAddress"] != null)
                                {
                                    objWorkingUnit.WorkingUnitAddress = dr["WorkingUnitAddress"].ToString();
                                }
                                objWorkingUnitList.Add(objWorkingUnit);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //Handle execution related error
                    }
                    finally
                    {
                        cn.Close();
                    }
                }
            }
            catch
            {
                //Handle database connection related error
            }            
            return objWorkingUnitList;
        }

        /// <summary>
        /// Save WorkingUnit to DB
        /// Save both New and Updated WorkingUnit
        /// </summary>
        /// <param name="objWorkingUnit"></param>
        /// <returns></returns>
        public static string SaveWorkingUnit(WorkingUnit objWorkingUnit)
        {
            int vResult = 0;
            string vOut = EnumMessageId.SO251.ToString() + ": Exception Occured !";
            Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
            StringBuilder vComText = new StringBuilder();
            if (objWorkingUnit.IsNew)
            {
                vComText.Append("INSERT INTO WorkingUnit (UserCode, ActionDate, ActionType, WorkingUnitCode, WorkingUnitName, WorkingUnitType, ParentUnitCode, WorkingUnitPatern, WorkingUnitAddress, IsDeleted)");
                vComText.Append("VALUES(@UserCode, @ActionDate, @ActionType, @WorkingUnitCode, @WorkingUnitName, @WorkingUnitType, @ParentUnitCode, @WorkingUnitPatern, @WorkingUnitAddress, @IsDeleted)");               
            }
            else
            {
               vComText.Append("UPDATE WorkingUnit SET UserCode=@UserCode, ActionDate=@ActionDate, ActionType=@ActionType, WorkingUnitName=@WorkingUnitName, WorkingUnitType=@WorkingUnitType,ParentUnitCode=@ParentUnitCode, WorkingUnitPatern=@WorkingUnitPatern, WorkingUnitAddress=@WorkingUnitAddress WHERE WorkingUnitCode=@WorkingUnitCode"); 
            }
            DbCommand cmd = db.GetSqlStringCommand(vComText.ToString());
            db.AddInParameter(cmd, "UserCode", DbType.String, SessionUtility.SessionContainer.USER_ID);
            db.AddInParameter(cmd, "ActionDate", DbType.DateTime, DateTime.Now);
            db.AddInParameter(cmd, "ActionType", DbType.String, DBAction.Insert);
            db.AddInParameter(cmd, "WorkingUnitCode", DbType.String, objWorkingUnit.WorkingUnitCode_PK);
            db.AddInParameter(cmd, "WorkingUnitName", DbType.String, objWorkingUnit.WorkingUnitName);
            db.AddInParameter(cmd, "WorkingUnitType", DbType.String, objWorkingUnit.WorkingUnitType);
            db.AddInParameter(cmd, "ParentUnitCode", DbType.String, objWorkingUnit.ParentUnitCode);
            db.AddInParameter(cmd, "WorkingUnitPatern", DbType.String, objWorkingUnit.WorkingUnitPatern);
            db.AddInParameter(cmd, "WorkingUnitAddress", DbType.String, objWorkingUnit.WorkingUnitAddress);
            db.AddInParameter(cmd, "IsDeleted", DbType.Decimal, 0);
            using (DbConnection cn = db.CreateConnection())
            {
                cn.Open();
                try
                {
                    vResult =db.ExecuteNonQuery(cmd);
                    if (vResult > 0)//Executed and DB is updated
                    {
                        vOut = string.Empty;//EnumMessageId.S101.ToString() + ":";
                    }
                    else//Executed but did not hit to DB and also there is no exception.
                    {
                        vOut =  EnumMessageId.SO200.ToString() + ": 0 Records Saved!";
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
            return vOut;
        }      

        /// <summary>
        /// Set delete flag when user delete WorkingUnit
        /// </summary>
        /// <param name="WorkingUnitId"></param>
        /// <returns></returns>
        public static string DeleteWorkingUnit(int WorkingUnitId)
        {
            WorkingUnit objWorkingUnit = new WorkingUnit();
            objWorkingUnit.TableName_TBL = "WorkingUnit";
            objWorkingUnit.WorkingUnitCode_PK = WorkingUnitId;
            //if (CommonBaseDC.DeleteCheck(objWorkingUnit))
            //{
                if (!HasChild(WorkingUnitId))
                {
                    string sSql = "UPDATE WorkingUnit SET UserCode=@UserCode, ActionDate=@ActionDate, ActionType=@ActionType, IsDeleted =1 WHERE WorkingUnitCode=@WorkingUnitCode";
                    Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
                    DbCommand cmd = db.GetSqlStringCommand(sSql);
                    db.AddInParameter(cmd, "UserCode", DbType.String, SessionUtility.SessionContainer.USER_ID);
                    db.AddInParameter(cmd, "ActionDate", DbType.DateTime, DateTime.Now);
                    db.AddInParameter(cmd, "ActionType", DbType.String, DBAction.Update);
                    db.AddInParameter(cmd, "WorkingUnitCode", DbType.Int32, WorkingUnitId);
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
                    return ":: It is not possible to delete this Company ::";
                }
            //}
            //else
            //{
            //    return ":: It is not possible to delete this Company ::";
            //}
            return string.Empty;
        }

        /// <summary>
        /// Get WorkingUnit by UnitCode
        /// </summary>
        /// <param name="WorkingUnitCode"></param>
        /// <returns></returns>
        public static WorkingUnit GetWorkingUnitByUnitCode(int WorkingUnitCode)
        {
            WorkingUnit objWorkingUnit =  new WorkingUnit();
            string sSql = "SELECT WorkingUnit.UserCode, WorkingUnit.WorkingUnitCode, WorkingUnit.WorkingUnitName, WorkingUnit.ParentUnitCode, WorkingUnit.WorkingUnitPatern ,WorkingUnit.WorkingUnitType, WorkingUnit.WorkingUnitAddress FROM WorkingUnit WHERE WorkingUnit.WorkingUnitCode = @WorkingUnitCode AND IsDeleted = @IsDeleted";
            Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
            DbCommand cmd = db.GetSqlStringCommand(sSql);
            db.AddInParameter(cmd, "WorkingUnitCode", DbType.Int32, WorkingUnitCode);
            db.AddInParameter(cmd, "IsDeleted", DbType.Decimal, 0);
            using (DbConnection cn = db.CreateConnection())
            {
                cn.Open();
                try
                {
                    using (IDataReader dr = db.ExecuteReader(cmd))
                    {
                        while (dr.Read())
                        {
                            objWorkingUnit.UserCode = dr["UserCode"].ToString();
                            objWorkingUnit.WorkingUnitCode_PK = Convert.ToInt32(dr["WorkingUnitCode"].ToString());
                            objWorkingUnit.WorkingUnitName = dr["WorkingUnitName"].ToString();
                            if (!string.IsNullOrEmpty(dr["ParentUnitCode"].ToString()))
                            {
                                objWorkingUnit.ParentUnitCode = Convert.ToInt32(dr["ParentUnitCode"]);
                                objWorkingUnit.ParentUnitName = GetParentUnitName(objWorkingUnit.ParentUnitCode);
                            }                            
                            objWorkingUnit.WorkingUnitPatern = dr["WorkingUnitPatern"].ToString();
                            objWorkingUnit.WorkingUnitType = dr["WorkingUnitType"].ToString();
                            if (dr["WorkingUnitAddress"] != null)
                            {
                                objWorkingUnit.WorkingUnitAddress = dr["WorkingUnitAddress"].ToString();
                            }
                        }
                    }
                }
                catch 
                {
                    //Handle execution related error
                }
                finally
                {
                    cn.Close();
                }
            }
            return objWorkingUnit;
        }

        /// <summary>
        /// Get parent WorkingUnitName
        /// </summary>
        /// <param name="WorkingUnitCode"></param>
        /// <returns></returns>
        public static string GetParentUnitName(int WorkingUnitCode)
        {
            WorkingUnit objWorkingUnit = new WorkingUnit();
            string sSql = "SELECT  WorkingUnit.WorkingUnitName FROM WorkingUnit WHERE WorkingUnitCode=@WorkingUnitCode AND IsDeleted = 0";
            Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
            DbCommand cmd = db.GetSqlStringCommand(sSql);
            db.AddInParameter(cmd, "WorkingUnitCode", DbType.String, WorkingUnitCode);
            using (DbConnection cn = db.CreateConnection())
            {
                cn.Open();
                try
                {
                    using (IDataReader dr = db.ExecuteReader(cmd))
                    {
                        while (dr.Read())
                        {                           
                            return dr["WorkingUnitName"].ToString();
                        }
                    }
                }
                catch 
                {
                    //Handle execution related error
                }
                finally
                {
                    cn.Close();
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Check child exist of the Item
        /// </summary>
        /// <param name="WorkingUnitCode"></param>
        /// <returns></returns>
        public static bool HasChild(int WorkingUnitCode)
        {
            string sSql = "SELECT WorkingUnit.WorkingUnitCode FROM WorkingUnit WHERE WorkingUnit.ParentUnitCode = @ParentUnitCode AND IsDeleted = @IsDeleted";
            Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
            DbCommand cmd = db.GetSqlStringCommand(sSql);
            db.AddInParameter(cmd, "ParentUnitCode", DbType.Int32, WorkingUnitCode);
            db.AddInParameter(cmd, "IsDeleted", DbType.Decimal, 0);
            using (DbConnection cn = db.CreateConnection())
            {
                cn.Open();
                try
                {
                    using (IDataReader dr = db.ExecuteReader(cmd))
                    {
                        while (dr.Read())
                        {
                            return true;
                        }
                    }
                }
                catch
                {
                    //Handle execution related error
                }
                finally
                {
                    cn.Close();
                }
            }
            return false;
        }

        #endregion --------------WoringUnit

        #region WorkingUnitLogo-------------
        /// <summary>
        /// Save WorkingUnitLogo
        /// </summary>
        /// <param name="objWorkingUnitLogo"></param>
        /// <returns></returns>
        public static string SaveWorkingUnitLogo(WorkingUnitLogo objWorkingUnitLogo)
        {
            Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
            string sSql = "";
            if (objWorkingUnitLogo.IsNew)
            {
                sSql = @"insert into WorkingUnitLogo (UserCode, ActionDate, ActionType, WorkingUnitCode, Logo ) values (@UserCode, @ActionDate, @ActionType, @WorkingUnitCode,@Logo)";
                DbCommand cmd = db.GetSqlStringCommand(sSql);
                db.AddInParameter(cmd, "UserCode", DbType.String, SessionUtility.SessionContainer.USER_ID);
                db.AddInParameter(cmd, "ActionDate", DbType.DateTime, DateTime.Now);
                db.AddInParameter(cmd, "ActionType", DbType.String, DBAction.Insert);
                db.AddInParameter(cmd, "WorkingUnitCode", DbType.Int32, objWorkingUnitLogo.WorkingUnitCode_PK);
                db.AddInParameter(cmd, "Logo", DbType.Binary, objWorkingUnitLogo.Logo);
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
                sSql = @"update WorkingUnitLogo set UserCode=@UserCode, ActionDate =@ActionDate, ActionType=@ActionType, Logo= @Logo where WorkingUnitCode=@WorkingUnitCode";
                DbCommand cmd = db.GetSqlStringCommand(sSql);
                db.AddInParameter(cmd, "UserCode", DbType.String, SessionUtility.SessionContainer.USER_ID);
                db.AddInParameter(cmd, "ActionDate", DbType.DateTime, DateTime.Now);
                db.AddInParameter(cmd, "ActionType", DbType.String, DBAction.Update);
                db.AddInParameter(cmd, "WorkingUnitCode", DbType.Int32, objWorkingUnitLogo.WorkingUnitCode_PK);
                db.AddInParameter(cmd, "Logo", DbType.Binary, objWorkingUnitLogo.Logo);

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

        /// <summary>
        /// Delete WorkingUnitLogo
        /// </summary>
        /// <param name="WorkingUnitCode"></param>
        /// <returns></returns>
        public static string DeleteWorkingUnitLogo(int WorkingUnitCode)
        {           
            Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
            string sSql = "DELETE FROM WorkingUnitLogo  WHERE WorkingUnitCode=@WorkingUnitCode";
            DbCommand cmd = db.GetSqlStringCommand(sSql);
            db.AddInParameter(cmd, "WorkingUnitCode", DbType.Int32, WorkingUnitCode);
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
            return string.Empty;
        }

        /// <summary>
        /// Get WorkingUnitLogo By WorkingUnitCode
        /// </summary>
        /// <param name="WorkingUnitCode"></param>
        /// <returns></returns>
        public static WorkingUnitLogo GetWorkingUnitLogoById(int WorkingUnitCode)
        {
            WorkingUnitLogo objWorkingUnitLogo = new WorkingUnitLogo();
            string sSql = @"SELECT   r.WorkingUnitCode   ,c.WorkingUnitName  ,r.Logo  FROM WorkingUnitLogo r
                            inner join WorkingUnit c  on r.WorkingUnitCode=c.WorkingUnitCode  where r.WorkingUnitCode=" + WorkingUnitCode;

            Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
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
                            objWorkingUnitLogo.WorkingUnitCode_PK = Convert.ToInt32(dr["WorkingUnitCode"]);
                            if (!string.IsNullOrEmpty(dr["Logo"].ToString()))
                                objWorkingUnitLogo.Logo = (byte[])dr["Logo"];                          
                        
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
            return objWorkingUnitLogo;
        }


        private bool DeleteCheck(WorkingUnit objWorkingUnit)
        {

            string sql = "select count(WorkingUnitCode) from WorkingUnit where ParentUnitCode=@ParentUnitCode";
            Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
            StringBuilder vComText = new StringBuilder();
           
                vComText.Append(sql);
                //vComText.Append("VALUES(@UserCode, @ActionDate, @ActionType, @WorkingUnitCode, @WorkingUnitName, @WorkingUnitType, @ParentUnitCode, @WorkingUnitPatern, @WorkingUnitAddress, @IsDeleted)");
           
                //vComText.Append("UPDATE WorkingUnit SET UserCode=@UserCode, ActionDate=@ActionDate, ActionType=@ActionType, WorkingUnitName=@WorkingUnitName, WorkingUnitType=@WorkingUnitType,ParentUnitCode=@ParentUnitCode, WorkingUnitPatern=@WorkingUnitPatern, WorkingUnitAddress=@WorkingUnitAddress WHERE WorkingUnitCode=@WorkingUnitCode");
           
            DbCommand cmd = db.GetSqlStringCommand(vComText.ToString());
            db.AddInParameter(cmd, "ParentUnitCode", DbType.Int16, objWorkingUnit.WorkingUnitCode_PK);
            int count = Convert.ToInt16(cmd.ExecuteScalar() == DBNull.Value ? "0" : cmd.ExecuteScalar());
            if (count > 0)
                return false;
            else
                return true;
        }
        
        #endregion ------------WorkingUnitLogo
        #endregion -----------------Methods
    }
}
