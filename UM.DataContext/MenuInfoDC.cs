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
    public class MenuInfoDC
    {

        #region Methods ----------------------------------

        /// <summary>
        /// Get all menu of the application
        /// </summary>
        /// <returns></returns>
        public static List<MenuItemInfo> GetMenuList()
        {
            List<MenuItemInfo> objMenuItemInfoList = new List<MenuItemInfo>();
            MenuItemInfo objMenuItemInfo;
            string Sql = "SELECT MenuId, MenuName,IsSecurityMenu, ParentMenuId, ModuleId, GroupId, ActionId,SortOrder FROM Menu WHERE IsDeleted = 0 ORDER BY SortOrder ASC";
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
                            objMenuItemInfo = new MenuItemInfo();
                            objMenuItemInfo.MenuId_PK = dr["MenuId"].ToString();
                            objMenuItemInfo.MenuName = dr["SortOrder"].ToString() + " - " + dr["MenuName"].ToString();
                            objMenuItemInfo.ModuleId = dr["ModuleId"].ToString();
                            objMenuItemInfo.GroupId = dr["GroupId"].ToString();
                            objMenuItemInfo.IsSecurityMenu = Convert.ToBoolean(dr["IsSecurityMenu"]);
                            objMenuItemInfo.ParentMenuId = dr["ParentMenuId"].ToString();                            
                            objMenuItemInfo.ActionId_FK = dr["ActionId"].ToString();
                            objMenuItemInfoList.Add(objMenuItemInfo);
                        }
                    }
                }
                catch
                {
                    //Handle database execution related problem
                }
                finally
                {
                    cn.Close();
                }
            }

            return objMenuItemInfoList;
        }

        /// <summary>
        /// Get menu by Id
        /// </summary>
        /// <param name="MenuId"></param>
        /// <returns></returns>
        public static MenuItemInfo GetMenuById(string MenuId)
        {
            MenuItemInfo objMenuItemInfo = new MenuItemInfo();
            string Sql = "SELECT MenuId, MenuName,MenuText, IsSecurityMenu, ParentMenuId, ActionId, ModuleId,IsModule,IsHelpMenu, GroupId, SortOrder, FastPath FROM Menu WHERE MenuId=@MenuId";
            Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
            DbCommand cmd = db.GetSqlStringCommand(Sql);
            db.AddInParameter(cmd, "MenuId", DbType.String, MenuId);
            using (DbConnection cn = db.CreateConnection())
            {
                cn.Open();
                try
                {
                    using (IDataReader dr = db.ExecuteReader(cmd))
                    {
                        while (dr.Read())
                        {
                            objMenuItemInfo = new MenuItemInfo();
                            objMenuItemInfo.MenuId_PK = dr["MenuId"].ToString();
                            objMenuItemInfo.MenuName = dr["MenuName"].ToString();
                            if (dr["MenuText"] != DBNull.Value)
                            {
                                objMenuItemInfo.MenuText = dr["MenuText"].ToString();
                            }
                            objMenuItemInfo.IsSecurityMenu = Convert.ToBoolean(dr["IsSecurityMenu"]);
                            objMenuItemInfo.ParentMenuId = dr["ParentMenuId"].ToString();
                            if (!string.IsNullOrEmpty(objMenuItemInfo.ParentMenuId))
                            {
                                objMenuItemInfo.ParentMenuName = GetParentMenuName(objMenuItemInfo.ParentMenuId);
                            }
                            objMenuItemInfo.ActionId_FK = dr["ActionId"].ToString();
                            objMenuItemInfo.ModuleId = dr["ModuleId"].ToString();
                            objMenuItemInfo.IsModule = Convert.ToBoolean(dr["IsModule"]);
                            objMenuItemInfo.IsHelpMenu = Convert.ToBoolean(dr["IsHelpMenu"]);
                            objMenuItemInfo.GroupId = dr["GroupId"].ToString();
                            if (!string.IsNullOrEmpty(objMenuItemInfo.ActionId_FK))
                            {
                                objMenuItemInfo.ActionName = ActionInfoDC.GetActionName(objMenuItemInfo.ActionId_FK);
                            }
                            objMenuItemInfo.SortOrder =Convert.ToInt32(  dr["SortOrder"]);
                            objMenuItemInfo.FastPath = dr["FastPath"].ToString();
                        }
                    }
                }
                catch
                {
                    //Handle database execution related problem
                }
                finally
                {
                    cn.Close();
                }
            }
            return objMenuItemInfo;
        }

        /// <summary>
        /// Get Parent Menu Name
        /// </summary>
        /// <param name="MenuId"></param>
        /// <returns></returns>
        public static string GetParentMenuName(string MenuId)
        {
            string Sql = "SELECT MenuId, MenuName FROM Menu Where MenuId=@MenuId AND IsDeleted = 0";
            Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
            DbCommand cmd = db.GetSqlStringCommand(Sql);
            db.AddInParameter(cmd, "MenuId", DbType.String, MenuId);
            using (DbConnection cn = db.CreateConnection())
            {
                cn.Open();
                try
                {
                    using (IDataReader dr = db.ExecuteReader(cmd))
                    {
                        while (dr.Read())
                        {
                            return dr["MenuName"].ToString();
                           
                        }
                    }
                }
                catch
                {
                    //Handle database execution related problem
                }
                finally
                {
                    cn.Close();
                }
            }

            return string.Empty;
        }
       
        /// <summary>
        /// Save menu
        /// </summary>
        /// <param name="objMenuInfo"></param>
        /// <returns></returns>
        public static string SaveMenu(MenuItemInfo objMenuInfo)
        {
            int vResult = 0;
            string vOut = EnumMessageId.SO251.ToString() + ": Exception Occured !";
            Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
            StringBuilder vComText = new StringBuilder();
            if (objMenuInfo.IsNew)
            {
                vComText.Append("INSERT INTO  Menu (UserCode,ActionDate,ActionType,MenuId,MenuName,MenuText,ModuleId,GroupId, SortOrder,IsSecurityMenu,FastPath,ParentMenuId,ActionId,IsModule,IsHelpMenu,Isdeleted) ");
                vComText.Append("VALUES (@UserCode,@ActionDate,@ActionType,@MenuId,@MenuName,@MenuText,@ModuleId,@GroupId,@SortOrder,@IsSecurityMenu,@FastPath,@ParentMenuId,@ActionId,@IsModule,@IsHelpMenu,@IsDeleted)");
            }
            else//Update Menu
            {
                vComText.Append(@"UPDATE  Menu SET UserCode =@UserCode, ActionDate =@ActionDate,
                                        ActionType=@ActionType, MenuName=@MenuName,MenuText=@MenuText,ModuleId=@ModuleId,GroupId=@GroupId, SortOrder=@SortOrder, IsModule=@IsModule,IsHelpMenu=@IsHelpMenu,
                                        IsSecurityMenu=@IsSecurityMenu, FastPath=@FastPath, ParentMenuId=@ParentMenuId, ActionId=@ActionId WHERE MenuId=@MenuId ");
            }

            DbCommand cmd = db.GetSqlStringCommand(vComText.ToString());
            db.AddInParameter(cmd, "UserCode", DbType.String, SessionUtility.SessionContainer.USER_ID);
            db.AddInParameter(cmd, "ActionDate", DbType.DateTime, DateTime.Now);
            db.AddInParameter(cmd, "ActionType", DbType.String, "Insert");
            db.AddInParameter(cmd, "MenuId", DbType.String, objMenuInfo.MenuId_PK);
            db.AddInParameter(cmd, "MenuName", DbType.String, objMenuInfo.MenuName);
            db.AddInParameter(cmd, "MenuText", DbType.String, objMenuInfo.MenuText);
            if (string.IsNullOrEmpty(objMenuInfo.ModuleId))
            {
                db.AddInParameter(cmd, "ModuleId", DbType.String, null);
            }
            else
            {
                db.AddInParameter(cmd, "ModuleId", DbType.String, objMenuInfo.ModuleId);
            }
            if (string.IsNullOrEmpty(objMenuInfo.GroupId))
            {
                db.AddInParameter(cmd, "GroupId", DbType.String, null);
            }
            else
            {
                db.AddInParameter(cmd, "GroupId", DbType.String, objMenuInfo.GroupId);
            }
            db.AddInParameter(cmd, "SortOrder", DbType.Decimal, objMenuInfo.SortOrder);
            db.AddInParameter(cmd, "IsSecurityMenu", DbType.Decimal, objMenuInfo.IsSecurityMenu);
            db.AddInParameter(cmd, "FastPath", DbType.String, objMenuInfo.FastPath);
            db.AddInParameter(cmd, "ParentMenuId", DbType.String, objMenuInfo.ParentMenuId);
            if (string.IsNullOrEmpty(objMenuInfo.ActionId_FK))
            {
                db.AddInParameter(cmd, "ActionId", DbType.String, null);
            }
            else
            {
                db.AddInParameter(cmd, "ActionId", DbType.String, objMenuInfo.ActionId_FK);
            }
            db.AddInParameter(cmd, "IsModule", DbType.Decimal, objMenuInfo.IsModule);
            db.AddInParameter(cmd, "IsHelpMenu", DbType.Decimal, objMenuInfo.IsHelpMenu);
            db.AddInParameter(cmd, "IsDeleted", DbType.Decimal, objMenuInfo.IsDeleted);

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
            SOSession.FlushCacheDependency();
            return vOut;
        }

        /// <summary>
        /// Delete menu
        /// </summary>
        /// <param name="MenuId"></param>
        /// <returns></returns>
        public static string DeleteMenu(string MenuId)
        {
            int vResult = 0;
            string vOut = EnumMessageId.SO251.ToString() + ": Exception Occured !";

            MenuItemInfo objMenuItemInfo = new MenuItemInfo();
            objMenuItemInfo.TableName_TBL = "Menu";
            objMenuItemInfo.MenuId_PK = MenuId;
            if (CommonBaseDC.DeleteCheck(objMenuItemInfo))
            {
                if (!HasChild(MenuId))
                {
                    objMenuItemInfo = GetMenuById(MenuId);
                    if (!string.IsNullOrEmpty(objMenuItemInfo.ParentMenuId))
                    {
                        string sSql = "UPDATE Menu SET ActionDate=@ActionDate, ActionType=@ActionType, IsDeleted=@IsDeleted WHERE MenuId=@MenuId ";
                        Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
                        DbCommand cmd = db.GetSqlStringCommand(sSql);
                        db.AddInParameter(cmd, "MenuId", DbType.String, MenuId);
                        db.AddInParameter(cmd, "ActionDate", DbType.String, DateTime.Now);
                        db.AddInParameter(cmd, "ActionType", DbType.String, DBAction.Update);
                        db.AddInParameter(cmd, "IsDeleted", DbType.Decimal, 1);
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
                    }
                    else
                    {
                        vOut = ":: It is not possible to delete root menu. ::";
                    }
                }
                else
                {
                    vOut = ":: To delete parent menu, first delete child menu(s). ::";
                }
            }
            else
            {
                vOut = ":: Failed to delete menu  on account of foreign key reference. ::";
            }
            return vOut;
        }

        /// <summary>
        /// Check ChildMenu of a menuItem
        /// </summary>
        /// <param name="MenuId"></param>
        /// <returns></returns>
        protected static bool HasChild(string MenuId)
        {
            string Sql = "SELECT MenuId, MenuName FROM Menu WHERE ParentMenuId=@ParentMenuId";
            Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
            DbCommand cmd = db.GetSqlStringCommand(Sql);
            db.AddInParameter(cmd, "ParentMenuId", DbType.String, MenuId);
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
                    //Handle database execution related problem
                }
                finally
                {
                    cn.Close();
                }
            }
            return false;
        }
        #endregion  -------------------------------Methods
    }
}
