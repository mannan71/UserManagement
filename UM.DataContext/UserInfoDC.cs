using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using S1.Common.DataContext;
using S1.CommonBiz;
using SOProvider;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using UM.Model;

namespace UM.DataContext
{
    public class UserInfoDC
    {
        #region Variables -------------------------



        #endregion -----------------------Variables

        #region Methods ---------------------------

        /// <summary>
        /// Get userlist
        /// If user nam eis empty then get all user
        /// Else get users based on parameter
        /// </summary>
        /// <param name="UserName"></param>
        /// <returns></returns>
        public static List<UserInfo> GetUsers(string UserName)
        {
            List<UserInfo> objUserList = new List<UserInfo>();
            UserInfo objUserInfo;
            UserInRole objUserInRole;
            string sSql = @"select DISTINCT User.UserId, User.UserName, Membership.Email  from User Inner Join Membership on User.UserId = Membership.UserId ";

            //If user name is empty then get all userInfo
            if (!string.IsNullOrEmpty(UserName))
            {
                sSql += " WHERE User.UserName Like (@UserName) ";
            }

            sSql += " ORDER BY User.UserName";

            Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
            DbCommand cmd = db.GetSqlStringCommand(sSql);
            db.AddInParameter(cmd, "UserName", DbType.String, UserName.Trim() + "%");

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
                                objUserInfo = new UserInfo();
                                objUserInRole = new UserInRole();
                                objUserInfo.UserID_PK = dr["UserId"].ToString();
                                objUserInfo.UserName = dr["UserName"].ToString();
                                objUserInfo.Email = dr["Email"].ToString();
                                objUserList.Add(objUserInfo);
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
            }
            catch
            {
                //Handle database connection related problem
            }
            return objUserList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="UserName"></param>
        /// <returns></returns>
        public static UserInfo GetUserByName(string UserName)
        {
            UserInfo objUserInfo = new UserInfo();
            UserInRole objUserInRole;
            string sSql = @"select DISTINCT User.UserId, User.UserName, Membership.Email,Membership.Comment,Membership.IsLockedOut, UserPassword.Password, UserInfo.CompanyCode, UserInfo.WorkingUnitCode, UserInfo.UserFullName from User Inner Join Membership on User.UserId = Membership.UserId INNER JOIN UserPassword ON User.UserId = UserPassword.UserId INNER JOIN UserInfo ON User.UserId = UserInfo.UserId INNER JOIN WorkingUnit ON UserInfo.WorkingUnitCode = WorkingUnit.WorkingUnitCode";
            sSql += " WHERE User.UserName = @UserName and UserPassword.ActionDate = (Select Max(ActionDate) from UserPassword WHERE UserPassword.UserId = User.UserId)";
            Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
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
                            objUserInfo = new UserInfo();
                            objUserInRole = new UserInRole();
                            objUserInfo.UserID_PK = dr["UserId"].ToString();
                            objUserInfo.UserName = dr["UserName"].ToString();
                            objUserInfo.Email = dr["Email"].ToString();
                            objUserInfo.Comment = dr["Comment"].ToString();
                            objUserInfo.IsLokedOut = Convert.ToBoolean(dr["IsLockedOut"]);
                            objUserInfo.Password = dr["Password"].ToString();
                            objUserInfo.UserRoles = GetUserInRolesByUserId(objUserInfo.UserID_PK);
                            objUserInfo.CompanyCode = Convert.ToInt32(dr["CompanyCode"]);
                            objUserInfo.WorkingUnitCode = Convert.ToInt32(dr["WorkingUnitCode"]);
                            objUserInfo.UserFullName = dr["UserFullName"].ToString();
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

            return objUserInfo;
        }

        /// <summary>
        /// Get user by userID
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public static UserInfo GetUserByID(string UserID)
        {
            UserInfo objUserInfo = new UserInfo();
            UserInRole objUserInRole;
            string sSql = @"select DISTINCT User.UserId, User.UserName, 
                            Membership.Email,Membership.Comment,Membership.IsLockedOut,
                            UserPassword.Password, UserInfo.CompanyCode, 
                            UserInfo.WorkingUnitCode, UserInfo.UserFullName
                            from User 
                            Inner Join Membership 
                            on User.UserId = Membership.UserId 
                            INNER JOIN UserPassword 
                            ON User.UserId = UserPassword.UserId 
                            INNER JOIN UserInfo 
                            ON User.UserId = UserInfo.UserId 
                            INNER JOIN WorkingUnit 
                            ON UserInfo.WorkingUnitCode = WorkingUnit.WorkingUnitCode
                            WHERE User.UserId = @UserID 
                            and UserPassword.ActionDate = 
                                (Select Max(ActionDate) 
                                from UserPassword 
                                WHERE UserPassword.UserId = @UserID)";
            Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
            DbCommand cmd = db.GetSqlStringCommand(sSql);
            db.AddInParameter(cmd, "UserID", DbType.String, UserID);

            using (DbConnection cn = db.CreateConnection())
            {
                cn.Open();
                try
                {
                    using (IDataReader dr = db.ExecuteReader(cmd))
                    {
                        while (dr.Read())
                        {
                            objUserInfo = new UserInfo();
                            objUserInRole = new UserInRole();
                            objUserInfo.UserID_PK = dr["UserId"].ToString();
                            objUserInfo.UserName = dr["UserName"].ToString();
                            objUserInfo.Email = dr["Email"].ToString();
                            objUserInfo.Comment = dr["Comment"].ToString();
                            objUserInfo.IsLokedOut = Convert.ToBoolean(dr["IsLockedOut"]);
                            objUserInfo.Password = dr["Password"].ToString();
                            objUserInfo.UserRoles = GetUserInRolesByUserId(objUserInfo.UserID_PK);
                            objUserInfo.CompanyCode = Convert.ToInt32(dr["CompanyCode"]);
                            objUserInfo.WorkingUnitCode = Convert.ToInt32(dr["WorkingUnitCode"]);
                            objUserInfo.UserFullName = dr["UserFullName"].ToString();
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

            return objUserInfo;
        }

        /// <summary>
        /// Save user
        /// </summary>
        /// <param name="objUserInfo"></param>
        /// <returns></returns>
        public static string SaveUser(UserInfo objUserInfo)
        {
            int vResult = 0;
            string vOut = EnumMessageId.SO251.ToString() + ": Exception Occured !";
            StringBuilder vComText = new StringBuilder();

            Hashtable htExistingRoles = new Hashtable();
            Hashtable htNewUserRoleList = new Hashtable();

            //This condition is added for checking user is active or not
            //For active user in UI we set active=true but in DB we save IsLokedOut=false
            //In MVC, view is tight bound so we change this way
            if (objUserInfo.IsLokedOut)
            {
                objUserInfo.IsLokedOut = false;
            }
            else
            {
                objUserInfo.IsLokedOut = true;
            }
            //IF new then execute following code
            //This code use to create object
            if (objUserInfo.IsNew)
            {
                MembershipCreateStatus status = MembershipCreateStatus.Success;

                Membership.CreateUser(objUserInfo.UserName.Trim(), objUserInfo.Password, objUserInfo.Email, "NA", "NA", objUserInfo.IsLokedOut, out status);
                if (status == MembershipCreateStatus.Success)
                {
                    MembershipUser oUser = new MembershipUser("SOMembershipProvider", objUserInfo.UserName.Trim(), objUserInfo.UserID_PK, objUserInfo.Email, "NA", objUserInfo.Comment, !objUserInfo.IsLokedOut, objUserInfo.IsLokedOut, Convert.ToDateTime(objUserInfo.ActionDate), Convert.ToDateTime(objUserInfo.LastLoginDate), Convert.ToDateTime(objUserInfo.ActionDate), Convert.ToDateTime(objUserInfo.LastPasswordChangeDate), Convert.ToDateTime(objUserInfo.LastLockoutDate));
                    //update user to update user by comment
                    Membership.UpdateUser(oUser);

                    vComText.Append("INSERT INTO UserInfo (UserId,UserCode,ActionDate,ActionType,CompanyCode,WorkingUnitCode,UserFullName)");
                    vComText.Append(" VALUES");
                    vComText.Append("(@UserId,@UserCode,@ActionDate,@ActionType,@CompanyCode,@WorkingUnitCode,@UserFullName)");

                    Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
                    DbCommand cmd = db.GetSqlStringCommand(vComText.ToString());
                    db.AddInParameter(cmd, "UserId", DbType.String, oUser.ProviderUserKey);
                    db.AddInParameter(cmd, "UserCode", DbType.String, SessionUtility.SessionContainer.USER_ID);
                    db.AddInParameter(cmd, "ActionDate", DbType.DateTime, DateTime.Now);
                    db.AddInParameter(cmd, "ActionType", DbType.String, "Insert");
                    db.AddInParameter(cmd, "CompanyCode", DbType.String, objUserInfo.CompanyCode);
                    db.AddInParameter(cmd, "WorkingUnitCode", DbType.String, objUserInfo.WorkingUnitCode);
                    db.AddInParameter(cmd, "UserFullName", DbType.String, objUserInfo.UserFullName);
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
                    System.Threading.Thread.Sleep(1000);
                    //Reset UserPassword
                    objUserInfo.Password = ResetPassword(objUserInfo.UserName.Trim());

                    //Assign role to user
                    for (int i = 0; i < objUserInfo.UserRoles.Count; i++)
                    {
                        SaveUserInRole(oUser.ProviderUserKey.ToString(), objUserInfo.UserRoles[i].RoleID, null);
                    }
                }
                else
                {
                    vOut = EnumMessageId.SO200.ToString() + ": \"Failed to create user " + objUserInfo.UserName + "\" " + status + "\"\"";
                }

            }
            else
            {
                vOut = UpdateUser(objUserInfo);
            }

            //Clear cache of the user
            SOSession.FlushCacheDependency();
            return vOut;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objUserInfo"></param>
        /// <returns></returns>
        public static string UpdateUser(UserInfo objUserInfo)
        {
            int vResult = 0;
            string vOut = EnumMessageId.SO251.ToString() + ": Exception Occured !";
            StringBuilder vComText = new StringBuilder();

            Hashtable htExistingRoles = new Hashtable();
            Hashtable htNewUserRoleList = new Hashtable();
            //This code use to update object

            string sSqlUser = @"UPDATE User   SET UserCode = @UserCode, ActionDate = @ActionDate, ActionType = @ActionType, UserName = @UserName Where UserId = @UserId";
            Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
            DbCommand cmdUser = db.GetSqlStringCommand(sSqlUser);
            db.AddInParameter(cmdUser, "UserName", DbType.String, objUserInfo.UserName);
            db.AddInParameter(cmdUser, "UserCode", DbType.String, SessionUtility.SessionContainer.USER_ID);
            db.AddInParameter(cmdUser, "ActionDate", DbType.DateTime, objUserInfo.ActionDate);
            db.AddInParameter(cmdUser, "ActionType", DbType.String, "Update");
            db.AddInParameter(cmdUser, "UserId", DbType.String, objUserInfo.UserID_PK);

            string sSqlMembership = @"UPDATE Membership SET Email = @Email,Comment = @Comment,UserCode = @UserCode,ActionDate = @ActionDate,ActionType = @ActionType ,IsLockedOut = @IsLockedOut Where UserId = @UserId";

            DbCommand cmdMembership = db.GetSqlStringCommand(sSqlMembership);
            db.AddInParameter(cmdMembership, "UserId", DbType.String, objUserInfo.UserID_PK);
            db.AddInParameter(cmdMembership, "Email", DbType.String, objUserInfo.Email.ToLower());
            db.AddInParameter(cmdMembership, "Comment", DbType.String, objUserInfo.Comment);
            db.AddInParameter(cmdMembership, "UserCode", DbType.String, SessionUtility.SessionContainer.USER_ID);
            db.AddInParameter(cmdMembership, "ActionDate", DbType.DateTime, DateTime.Now);
            db.AddInParameter(cmdMembership, "ActionType", DbType.String, "Update");
            db.AddInParameter(cmdMembership, "IsLockedOut", DbType.Decimal, objUserInfo.IsLokedOut);

            string sSqlUserInfo = "UPDATE UserInfo SET UserCode= @UserCode, ActionDate = @ActionDate ,ActionType = @ActionType, CompanyCode = @CompanyCode, WorkingUnitCode = @WorkingUnitCode, UserFullName = @UserFullName Where UserId = @UserId";

            DbCommand cmdUserInfo = db.GetSqlStringCommand(sSqlUserInfo);
            db.AddInParameter(cmdUserInfo, "UserId", DbType.String, objUserInfo.UserID_PK);
            db.AddInParameter(cmdUserInfo, "UserCode", DbType.String, SessionUtility.SessionContainer.USER_ID);
            db.AddInParameter(cmdUserInfo, "ActionDate", DbType.DateTime, DateTime.Now);
            db.AddInParameter(cmdUserInfo, "ActionType", DbType.String, "Update");
            db.AddInParameter(cmdUserInfo, "CompanyCode", DbType.String, objUserInfo.CompanyCode);
            db.AddInParameter(cmdUserInfo, "WorkingUnitCode", DbType.String, objUserInfo.WorkingUnitCode);
            db.AddInParameter(cmdUserInfo, "UserFullName", DbType.String, objUserInfo.UserFullName);

            //Get Existing Roles of the User
            List<UserInRole> objExistingRoles = GetUserInRolesByUserId(objUserInfo.UserID_PK);
            if (objExistingRoles.Count > 0)
            {
                foreach (UserInRole objUserInRole in objExistingRoles)
                {
                    if (!htExistingRoles.Contains(objUserInRole.RoleID))
                    {
                        htExistingRoles.Add(objUserInRole.RoleID, objUserInRole);
                    }
                }
            }
            //Get all Upcoming Roles of the Action
            if (objUserInfo.UserRoles.Count > 0)
            {
                foreach (UserInRole objUserInRole in objUserInfo.UserRoles)
                {
                    if (!htNewUserRoleList.Contains(objUserInRole.RoleID))
                    {
                        htNewUserRoleList.Add(objUserInRole.RoleID, objUserInRole);
                    }
                }
            }

            using (DbConnection cn = db.CreateConnection())
            {
                cn.Open();
                DbTransaction tr = cn.BeginTransaction();
                try
                {
                    //User
                    vResult = db.ExecuteNonQuery(cmdUser, tr);
                    if (vResult > 0)//Executed and DB is updated
                    {
                        vOut = EnumMessageId.SO101.ToString() + ":";
                    }
                    else//Executed but did not hit to DB and also there is no exception.
                    {
                        vOut = EnumMessageId.SO200.ToString() + ": 0 Records Saved!";
                    }
                    //Membership
                    vResult = db.ExecuteNonQuery(cmdMembership, tr);
                    if (vResult > 0)//Executed and DB is updated
                    {
                        vOut = EnumMessageId.SO101.ToString() + ":";
                    }
                    else//Executed but did not hit to DB and also there is no exception.
                    {
                        vOut = EnumMessageId.SO200.ToString() + ": 0 Records Saved!";
                    }
                    //UserInfo
                    vResult = db.ExecuteNonQuery(cmdUserInfo, tr);
                    if (vResult > 0)//Executed and DB is updated
                    {
                        vOut = EnumMessageId.SO101.ToString() + ":";
                    }
                    else//Executed but did not hit to DB and also there is no exception.
                    {
                        vOut = EnumMessageId.SO200.ToString() + ": 0 Records Saved!";
                    }

                    //Assign role to the user                            
                    for (int i = 0; i < objUserInfo.UserRoles.Count; i++)
                    {
                        if (!htExistingRoles.Contains(objUserInfo.UserRoles[i].RoleID))
                        {
                            SaveUserInRole(objUserInfo.UserID_PK, objUserInfo.UserRoles[i].RoleID, tr);
                        }
                    }
                    //Update roles which removed from the User 
                    for (int i = 0; i < objExistingRoles.Count; i++)
                    {
                        if (!htNewUserRoleList.Contains(objExistingRoles[i].RoleID))
                        {
                            //To Delete role of the User                   
                            string sSqlUserRole = "UPDATE UserInRole SET UserCode=@UserCode, ActionDate=@ActionDate, ActionType=@ActionType, IsDeleted = @IsDeleted  Where UserRoleId = @UserRoleId";
                            DbCommand cmdUserRole = db.GetSqlStringCommand(sSqlUserRole);
                            db.AddInParameter(cmdUserRole, "UserCode", DbType.String, SessionUtility.SessionContainer.USER_ID);
                            db.AddInParameter(cmdUserRole, "ActionDate", DbType.DateTime, DateTime.Now);
                            db.AddInParameter(cmdUserRole, "ActionType", DbType.String, "Update");
                            db.AddInParameter(cmdUserRole, "IsDeleted", DbType.String, 1);
                            db.AddInParameter(cmdUserRole, "UserRoleId", DbType.String, objExistingRoles[i].UserRoleID_PK);

                            vResult = db.ExecuteNonQuery(cmdUserRole, tr);
                            if (vResult > 0)//Executed and DB is updated
                            {
                                vOut = EnumMessageId.SO101.ToString() + ":";
                            }
                            else//Executed but did not hit to DB and also there is no exception.
                            {
                                vOut = EnumMessageId.SO200.ToString() + ": 0 Records Saved!";
                            }
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
            return vOut;
        }

        /// <summary>
        /// Reset user password
        /// Thought after each and every creation it is needed to reset password
        /// </summary>
        /// <param name="UserName"></param>
        /// <returns></returns>
        public static string ResetPassword(string UserName)
        {
            return Membership.Provider.ResetPassword(UserName, "NA");
        }

        /// <summary>
        /// Get NonAlphaNumChar from DB
        /// </summary>
        /// <returns></returns>
        public static string GetNonAlphaNumChar()
        {
            string NonAlphaNumChar = string.Empty;
            string sSql = @"SELECT NonAlphaNumChar FROM SecuritySetting";
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
                            NonAlphaNumChar = dr["NonAlphaNumChar"].ToString();
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
            return NonAlphaNumChar;
        }

        /// <summary>
        /// Change user password
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="OldPassword"></param>
        /// <param name="NewPassword"></param>
        /// <returns></returns>
        public static string ChangePassword(string UserName, string OldPassword, string NewPassword)
        {
            string vOut = EnumMessageId.SO251.ToString() + ": Exception Occured !";

            if (Membership.Provider.ChangePassword(UserName, OldPassword, NewPassword))
            {
                vOut = ":Password Changed Successfully";
            }
            else
            {
                vOut = "Password is not valid. Please insert valid password.";
            }
            return vOut;
        }

        /// <summary>
        /// A user can exist in seperate role so get all roles of the user
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="UserName"></param>
        /// <returns></returns>
        public static List<UserInRole> GetUserInRolesByUserId(string UserID)
        {
            List<UserInRole> objUserRoles = new List<UserInRole>();
            UserInRole objUserInRole;
            string Sql = "Select DISTINCT UserInRole.UserRoleId, UserInRole.UserId, UserInRole.RoleId from UserInRole Where UserInRole.UserId=@UserId and UserInRole.IsDeleted = 0";
            Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
            DbCommand cmd = db.GetSqlStringCommand(Sql);
            db.AddInParameter(cmd, "UserId", DbType.String, UserID.Trim());
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
                                objUserInRole = new UserInRole();
                                objUserInRole.UserRoleID_PK = dr["UserRoleId"].ToString();
                                objUserInRole.UserID = dr["UserId"].ToString();
                                objUserInRole.RoleID = dr["RoleId"].ToString();
                                objUserRoles.Add(objUserInRole);
                            }
                        }
                    }
                    catch
                    {
                        //handle execution exception
                    }
                    finally
                    {
                        cn.Close();
                    }
                }
            }
            catch
            {
                //Handle database connection exception
            }

            return objUserRoles;
        }

        /// <summary>
        ///  Save userRole
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="RoleId"></param>
        /// <returns></returns>
        public static string SaveUserInRole(string UserID, string RoleId, DbTransaction tr)
        {
            int vResult = 0;
            string vOut = EnumMessageId.SO251.ToString() + ": Exception Occured !";

            StringBuilder vComText = new StringBuilder();
            vComText.Append("INSERT INTO UserInRole (UserCode,ActionDate,ActionType,UserId,RoleId,UserRoleId,IsDeleted)");
            vComText.Append(" VALUES");
            vComText.Append("(@UserCode,@ActionDate,@ActionType,@UserId,@RoleId,@UserRoleId,@IsDeleted)");

            Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
            DbCommand cmd = db.GetSqlStringCommand(vComText.ToString());
            db.AddInParameter(cmd, "UserId", DbType.String, UserID);
            db.AddInParameter(cmd, "UserCode", DbType.String, SessionUtility.SessionContainer.USER_ID);
            db.AddInParameter(cmd, "ActionDate", DbType.DateTime, DateTime.Now);
            db.AddInParameter(cmd, "ActionType", DbType.String, "Insert");
            db.AddInParameter(cmd, "RoleId", DbType.String, RoleId);
            db.AddInParameter(cmd, "UserRoleId", DbType.String, Guid.NewGuid().ToString());
            db.AddInParameter(cmd, "IsDeleted", DbType.Decimal, Convert.ToDecimal(0));
            using (DbConnection cn = db.CreateConnection())
            {
                cn.Open();

                try
                {
                    if (tr != null)
                    {
                        vResult = db.ExecuteNonQuery(cmd, tr);
                        if (vResult > 0)//Executed and DB is updated
                        {
                            vOut = EnumMessageId.SO101.ToString() + ":";
                        }
                        else//Executed but did not hit to DB and also there is no exception.
                        {
                            vOut = EnumMessageId.SO200.ToString() + ": 0 Records Saved!";
                        }
                    }
                    else
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
            return vOut;
        }

        /// <summary>
        /// Authenticate user and login to the system
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="Password"></param>
        /// <returns></returns>
        public static bool AuthenticateUser(string UserName, string Password)
        {
            bool Success = false;
            if (Membership.ValidateUser(UserName, Password))
            {
                Success = true;
            }
            return Success;
        }

        #endregion ----------------------------Methods
    }
}
