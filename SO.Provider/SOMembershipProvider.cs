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
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Security;

namespace SO.Provider
{
    public class SOMembershipProvider : MembershipProvider
    {
        #region Constants
        private const int _RandomMinLimit = 3;
        private const int _RandomMaxLimit = 10;
        #endregion
        #region Private Members
        private string _ApplicationName;
        private int _MaxInvalidPasswordAttempts;
        private int _MinRequiredNonAlphanumericCharacters;
        private int _MinRequiredPasswordLength;
        private MembershipPasswordFormat _PasswordFormat;
        private bool _RequiresQuestionAndAnswer;
        private bool _RequiresUniqueEmail;
        private bool _EnablePasswordReset;
        private bool _EnablePasswordRetrieval;
        private string _NonAlphanumericCharacters;
        private bool _RequiredChangePasswordFirstLogin;
        private int _MinRepeatPassAllowed;
        private int _PassExpireDay;
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
        public override int MaxInvalidPasswordAttempts
        {
            get
            {
                return _MaxInvalidPasswordAttempts;
            }
        }
        public override int MinRequiredNonAlphanumericCharacters
        {
            get
            {
                return _MinRequiredNonAlphanumericCharacters;
            }
        }
        public override int MinRequiredPasswordLength
        {
            get
            {
                return _MinRequiredPasswordLength;
            }
        }
        public override int PasswordAttemptWindow
        {
            get
            {
                throw new NotImplementedException("The property PasswordAttemptWindow is not supported by SoftOne.Provider.MembershipProvider");
            }
        }
        public override MembershipPasswordFormat PasswordFormat
        {
            get
            {
                return _PasswordFormat;
            }
        }
        public override string PasswordStrengthRegularExpression
        {
            get
            {
                throw new NotImplementedException("The property PasswordStrengthRegularExpression is not supported by SoftOne.Provider.MembershipProvider");
            }
        }
        public override bool RequiresQuestionAndAnswer
        {
            get
            {
                return _RequiresQuestionAndAnswer;
            }
        }
        public override bool RequiresUniqueEmail
        {
            get
            {
                return _RequiresUniqueEmail;
            }
        }
        public override bool EnablePasswordReset
        {
            get
            {
                return _EnablePasswordReset;
            }
        }
        public override bool EnablePasswordRetrieval
        {
            get
            {
                return _EnablePasswordRetrieval;
            }
        }
        #endregion
        #region Other Public Properties
        public string NonAlphanumericCharacters
        {
            get
            {
                return _NonAlphanumericCharacters;
            }
        }
        public bool RequiredChangePasswordFirstLogin
        {
            get
            {
                return _RequiredChangePasswordFirstLogin;
            }
        }
        public int MinRepeatPassAllowed
        {
            get
            {
                return _MinRepeatPassAllowed;
            }
        }
        public int PassExpireDay
        {
            get
            {
                return _PassExpireDay;
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
                throw new ArgumentNullException("web.config setting is invalid for SoftOne.Provider.MembershipProvider");
            if (string.IsNullOrEmpty(name))
                name = "MembershipProvider";
            if (String.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "MembershipProvider");
            }

            // get ConnectionString from web.config
            ConnectionStringSettings objConnectionStringSettings = ConfigurationManager.ConnectionStrings[config["connectionStringName"]];
            if (objConnectionStringSettings == null || string.IsNullOrEmpty(objConnectionStringSettings.ConnectionString.Trim()))
            {
                throw new ProviderException("Connection string cannot be blank at SoftOne.Provider.MembershipProvider");
            }
            else
            {
                _ConnectionString = objConnectionStringSettings.ConnectionString;
                _ConnectionStringKey = config["connectionStringName"].ToString();
            }

            // get Security Setting from DB which can be altered by user whuch are overrideble from web.config
            GetSecuritySetting();

            // get Membership Provider setting from web.config
            _ApplicationName = GetConfigValue(config["applicationName"], HostingEnvironment.ApplicationVirtualPath);
            _MaxInvalidPasswordAttempts = Convert.ToInt32(GetConfigValue(config["maxInvalidPasswordAttempts"], _MaxInvalidPasswordAttempts.ToString()));
            _MinRequiredNonAlphanumericCharacters = Convert.ToInt32(GetConfigValue(config["minRequiredNonAlphanumericCharacters"], _MinRequiredNonAlphanumericCharacters.ToString()));
            _MinRequiredPasswordLength = Convert.ToInt32(GetConfigValue(config["minRequiredPasswordLength"], _MinRequiredPasswordLength.ToString()));
            _PasswordFormat = GetPasswordFormat(GetConfigValue(config["passwordFormat"], _PasswordFormat.ToString()));
            _RequiresQuestionAndAnswer = Convert.ToBoolean(GetConfigValue(config["requiresQuestionAndAnswer"], "false"));
            _RequiresUniqueEmail = Convert.ToBoolean(GetConfigValue(config["requiresUniqueEmail"], "true"));
            _EnablePasswordReset = Convert.ToBoolean(GetConfigValue(config["enablePasswordReset"], "true"));
            _EnablePasswordRetrieval = Convert.ToBoolean(GetConfigValue(config["enablePasswordRetrieval"], "false"));
            _NonAlphanumericCharacters = Convert.ToString(GetConfigValue(config["nonAlphanumericCharacters"], _NonAlphanumericCharacters));
            _RequiredChangePasswordFirstLogin = Convert.ToBoolean(GetConfigValue(config["requiredChangePasswordFirstLogin"], _RequiredChangePasswordFirstLogin.ToString()));
            _MinRepeatPassAllowed = Convert.ToInt32(GetConfigValue(config["passwordRepeatAllowed"], _MinRepeatPassAllowed.ToString()));
            _PassExpireDay = Convert.ToInt32(GetConfigValue(config["passwordExpireDays"], _PassExpireDay.ToString()));


            // call base method
            base.Initialize(name, config);
        }
        #endregion
        #region Overridden Methods
        #region ChangePassword()
        /// <summary>
        /// Change Password by the logged in user
        /// </summary>
        /// <param name="username"></param>
        /// <param name="oldPassword"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            if (!Membership.ValidateUser(username, oldPassword))
                return false;

            // check password validity of Provider
            ValidatePasswordEventArgs args = new ValidatePasswordEventArgs(username, newPassword, true);
            OnValidatingPassword(args);
            if (args.Cancel)
                return false;
            string UserId = (GetUser(username, false)).ProviderUserKey.ToString();
            if (!string.IsNullOrEmpty(UserId))
            {
                if (SetPassword(UserId, username, newPassword))
                {
                    if (SOSession.RequiredChangePassword)
                    {
                        ChangeLoginFlag(UserId, _ConnectionStringKey);
                    }
                    return true;
                }
            }
            return false;
        }
        #endregion
        #region ChangePasswordQuestionAndAnswer()
        /// <summary>
        /// Change password question & answer
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="newPasswordQuestion"></param>
        /// <param name="newPasswordAnswer"></param>
        /// <returns></returns>
        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            bool isSuccess = false;

            // check whether RequiresQuestionAndAnswer is enabled for this provider
            if (!this.RequiresQuestionAndAnswer)
                throw new ProviderException("Please set RequiresQuestionAndAnswer to true at web.config to enable the method ChangePasswordQuestionAndAnswer at SoftOne.Provider.MembershipProvider");

            // check whether user is authenticated
            if (!Membership.ValidateUser(username, password))
                return false;

            string sSql = @"UPDATE Membership
                            SET 
                            PasswordQuestion = @PasswordQuestion,
                            PasswordAnswer = @PasswordAnswer,
                            UserCode = @UserCode,
                            ActionDate = @ActionDate,
                            ActionType = @ActionType
                            Where UserName = @UserName";

            Database db = DatabaseFactory.CreateDatabase(_ConnectionStringKey);
            DbCommand cmd = db.GetSqlStringCommand(sSql);
            db.AddInParameter(cmd, "PasswordQuestion", DbType.String, newPasswordQuestion);
            db.AddInParameter(cmd, "PasswordAnswer", DbType.String, EncodeText(SaltTextFromStoredSaltedText(newPasswordAnswer, username)));
            db.AddInParameter(cmd, "UserName", DbType.String, username);
            db.AddInParameter(cmd, "UserCode", DbType.String, SOSession.UserCode);
            db.AddInParameter(cmd, "ActionDate", DbType.DateTime, DateTime.Now);
            db.AddInParameter(cmd, "ActionType", DbType.String, DBAction.Update.ToString());
            using (DbConnection cn = db.CreateConnection())
            {
                cn.Open();
                try
                {
                    int rowsAffetced = db.ExecuteNonQuery(cmd);
                    if (rowsAffetced > 0)
                        isSuccess = true;
                }
                catch
                {
                    isSuccess = false;
                    throw;
                }
                finally
                {
                    cn.Close();
                }
            }
            return isSuccess;
        }
        #endregion
        #region CreateUser()
        /// <summary>
        /// Create an user
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="email"></param>
        /// <param name="passwordQuestion"></param>
        /// <param name="passwordAnswer"></param>
        /// <param name="isApproved"></param>
        /// <param name="providerUserKey"></param>
        /// <param name="status"></param>
        /// <returns>MembershipUser</returns>
        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            // Password validation is blocked when creating user. Because, during user creation the password
            // is system provided. Later on the password must be resetted by ResetPassword()
            /*
            // check password validity of Provider
            ValidatePasswordEventArgs args = new ValidatePasswordEventArgs(username, password, true);
            OnValidatingPassword(args);
            if (args.Cancel)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }*/
            // check email validity of uniquness
            if (RequiresUniqueEmail && GetUserNameByEmail(email).Length != 0)
            {
                status = MembershipCreateStatus.DuplicateEmail;
                return null;
            }
            // check username availability
            MembershipUser u = GetUser(username, false);
            if (u == null) // if username is vailable create the user
            {
                // DateTime createDate = DateTime.Now;
                if (providerUserKey == null)
                {
                    providerUserKey = Guid.NewGuid();
                }
                else
                {
                    if (!(providerUserKey is Guid))
                    {
                        status = MembershipCreateStatus.InvalidProviderUserKey;
                        return null;
                    }
                }

                // get the password salting text
                string passwordSaltedText = GeneratePasswordSaltingText();
                // salt the password
                string saltedPassword = SaltText(password, passwordSaltedText);
                // salt the password answer
                string saltedPasswordAnswer = SaltText(passwordAnswer, passwordSaltedText);

                // insert info @DB
                Database db = DatabaseFactory.CreateDatabase(_ConnectionStringKey);
                // User
                string sSqlUser = @"INSERT INTO User 
                                    (UserId,UserName,
                                    UserCode,ActionDate,ActionType)
                                    VALUES
                                    (@UserId,@UserName,
                                    @UserCode,@ActionDate,@ActionType)";

                Guid userId = Guid.NewGuid();
                DbCommand cmdUser = db.GetSqlStringCommand(sSqlUser);
                db.AddInParameter(cmdUser, "UserId", DbType.String, userId.ToString());
                db.AddInParameter(cmdUser, "UserName", DbType.String, username);
                db.AddInParameter(cmdUser, "UserCode", DbType.String, SOSession.UserCode);
                db.AddInParameter(cmdUser, "ActionDate", DbType.DateTime, DateTime.Now);
                db.AddInParameter(cmdUser, "ActionType", DbType.String, DBAction.Insert.ToString());

                // Membership
                string sSqlMembership = @"INSERT INTO Membership
                                        (UserId,IsLockedOut,IsFirstLogin,
                                        LastLoginDate,LastPasswordChangeDate,FailedPassAtmptCount,
                                        LastLockoutDate,FailedPassAnsAtmptCount,PasswordSalt,Email,
                                        PasswordQuestion,PasswordAnswer,
                                        UserCode,ActionDate,ActionType)
                                        VALUES
                                        (@UserId,@IsLockedOut,@IsFirstLogin,
                                        @LastLoginDate,@LastPasswordChangeDate,@FailedPassAtmptCount,
                                        @LastLockoutDate,@FailedPassAnsAtmptCount,@PasswordSalt,@Email,
                                        @PasswordQuestion,@PasswordAnswer,
                                        @UserCode,@ActionDate,@ActionType)
                                        ";
                DbCommand cmdMembership = db.GetSqlStringCommand(sSqlMembership);
                db.AddInParameter(cmdMembership, "UserId", DbType.String, userId.ToString());
                db.AddInParameter(cmdMembership, "IsLockedOut", DbType.Decimal, Convert.ToDecimal(isApproved));
                db.AddInParameter(cmdMembership, "IsFirstLogin", DbType.Decimal, 1);
                db.AddInParameter(cmdMembership, "LastLoginDate", DbType.DateTime, new DateTime(1800, 1, 1));
                db.AddInParameter(cmdMembership, "LastPasswordChangeDate", DbType.DateTime, new DateTime(1800, 1, 1));
                db.AddInParameter(cmdMembership, "FailedPassAtmptCount", DbType.Decimal, 0);
                db.AddInParameter(cmdMembership, "LastLockoutDate", DbType.DateTime, new DateTime(1800, 1, 1));
                db.AddInParameter(cmdMembership, "FailedPassAnsAtmptCount", DbType.Decimal, 0);
                db.AddInParameter(cmdMembership, "PasswordSalt", DbType.String, EncodeToBase64String(passwordSaltedText));
                db.AddInParameter(cmdMembership, "Email", DbType.String, email.ToLower());
                db.AddInParameter(cmdMembership, "PasswordQuestion", DbType.String, passwordQuestion);
                db.AddInParameter(cmdMembership, "PasswordAnswer", DbType.String, EncodeText(saltedPasswordAnswer));
                db.AddInParameter(cmdMembership, "UserCode", DbType.String, SOSession.UserCode);
                db.AddInParameter(cmdMembership, "ActionDate", DbType.DateTime, DateTime.Now);
                db.AddInParameter(cmdMembership, "ActionType", DbType.String, DBAction.Insert.ToString());

                //UserPassword
                string sSqlUserPassword = @"INSERT INTO UserPassword
                                            (UserId,Password,
                                            UserCode,ActionDate,ActionType)
                                            VALUES
                                            (@UserId,@Password,
                                            @UserCode,@ActionDate,@ActionType)";
                DbCommand cmdUserPassword = db.GetSqlStringCommand(sSqlUserPassword);
                db.AddInParameter(cmdUserPassword, "UserId", DbType.String, userId.ToString());
                db.AddInParameter(cmdUserPassword, "Password", DbType.String, EncodeText(saltedPassword));
                db.AddInParameter(cmdUserPassword, "UserCode", DbType.String, SOSession.UserCode);
                db.AddInParameter(cmdUserPassword, "ActionDate", DbType.DateTime, DateTime.Now);
                db.AddInParameter(cmdUserPassword, "ActionType", DbType.String, DBAction.Insert.ToString());

                using (DbConnection cn = db.CreateConnection())
                {
                    cn.Open();
                    DbTransaction tr = cn.BeginTransaction();
                    try
                    {
                        // User
                        db.ExecuteNonQuery(cmdUser, tr);
                        // Membership
                        db.ExecuteNonQuery(cmdMembership, tr);
                        // UserPassword
                        db.ExecuteNonQuery(cmdUserPassword, tr);
                        tr.Commit();
                        status = MembershipCreateStatus.Success;
                    }
                    catch
                    {
                        tr.Rollback();
                        status = MembershipCreateStatus.UserRejected;
                    }
                    finally
                    {
                        cn.Close();
                    }
                }
                if (status == MembershipCreateStatus.Success)
                    return GetUser(username, false);
                return null;
            }
            else // username is already selected
            {
                status = MembershipCreateStatus.DuplicateUserName;
                return null;
            }
        }
        #endregion
        #region DeleteUser()
        /// <summary>
        /// This method is not implemented. Deletion of user is not allowed.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="deleteAllRelatedData"></param>
        /// <returns></returns>
        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            throw new NotImplementedException("The method DeleteUser is not supported by SoftOne.Provider.MembershipProvider");
        }
        #endregion
        #region FindUsersByEmail()
        /// <summary>
        /// This method is not implemented
        /// </summary>
        /// <param name="emailToMatch"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException("The method FindUsersByEmail is not supported by SoftOne.Provider.MembershipProvider");
        }
        #endregion
        #region FindUsersByName()
        /// <summary>
        /// This method is not implemented
        /// </summary>
        /// <param name="usernameToMatch"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException("The method FindUsersByEmail is not supported by SoftOne.Provider.MembershipProvider");
        }
        #endregion
        #region GetAllUsers()
        /// <summary>
        /// Return Membership Users within the page size
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <returns>MembershipUserCollection</returns>
        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            MembershipUserCollection users = new MembershipUserCollection();
            totalRecords = GetNumberofUsers(null);
            if (totalRecords <= 0)
                return users;

            int counter = 0;
            int startIndex = pageSize * pageIndex;
            int endIndex = startIndex + pageSize - 1;

            string sSql = @"SELECT User.UserId, User.UserName, 
                            Email, PasswordQuestion,Comment, 
                            IsLockedOut, User.ActionDate, 
                            LastLoginDate,
                            LastPasswordChangeDate, LastLockoutDate
                            FROM User 
                            Inner Join Membership
                            ON User.UserId = Membership.UserId
                            Order By UserName";
            Database db = DatabaseFactory.CreateDatabase(_ConnectionStringKey);
            DbCommand cmd = db.GetSqlStringCommand(sSql);

            while (counter >= startIndex && counter <= endIndex)
            {
                users.Add(GetMembershipUser(db, cmd));
                counter++;
            }

            return users;
        }
        #endregion
        #region GetNumberOfUsersOnline()
        /// <summary>
        /// Returns number of user online i,e, number of active/unlocked user
        /// </summary>
        /// <returns>Number Of Users Online</returns>
        public override int GetNumberOfUsersOnline()
        {
            return GetNumberofUsers(false);
        }
        #endregion
        #region GetPassword()
        /// <summary>
        /// return password in plain text.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="answer"></param>
        /// <returns></returns>
        public override string GetPassword(string username, string answer)
        {
            string password = string.Empty;

            if (!this.RequiresQuestionAndAnswer)
                throw new ProviderException("Please set RequiresQuestionAndAnswer to true at web.config to enable the method GetPassword at SoftOne.Provider.MembershipProvider");

            if (!this.EnablePasswordRetrieval)
                throw new ProviderException("Please set EnablePasswordRetrieval to true at web.config to enable the method GetPassword at SoftOne.Provider.MembershipProvider");

            if (this.PasswordFormat == MembershipPasswordFormat.Hashed)
                throw new ProviderException("Cannot retrieve hashed password at SoftOne.Provider.MembershipProvider");

            // get password from DB
            password = Getpassword(username, answer);

            if (string.IsNullOrEmpty(password)) // update failed password answer attempt count and enable/disable user
            {
                UpdateFailureCount(username, false);
            }
            // get the password after decode and then unsalt
            return UnSaltTextFromStoredSaltedText(DecodeText(password), username);
        }
        #endregion
        #region GetUser()
        /// <summary>
        /// Get Membership User
        /// </summary>
        /// <param name="username">user unique name</param>
        /// <param name="userIsOnline">Set true if only active user is required. 
        /// Otherwise all user will be checked</param>
        /// <returns>MembershipUser</returns>
        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            string sSql = @"SELECT User.UserId, User.UserName, 
                            Email, PasswordQuestion,Comment, 
                            IsLockedOut, User.ActionDate, 
                            LastLoginDate,
                            LastPasswordChangeDate, LastLockoutDate
                            FROM User 
                            Inner Join Membership
                            ON User.UserId = Membership.UserId
                            WHERE User.UserName = @UserName";

            // add this parameter only if active user is required
            if (userIsOnline)
                sSql += " AND IsLockedOut = @IsLockedOut";

            Database db = DatabaseFactory.CreateDatabase(_ConnectionStringKey);
            DbCommand cmd = db.GetSqlStringCommand(sSql);
            db.AddInParameter(cmd, "UserName", DbType.String, username);

            if (userIsOnline)
                db.AddInParameter(cmd, "IsLockedOut", DbType.Decimal, Convert.ToDecimal(userIsOnline));


            return GetMembershipUser(db, cmd);
        }
        /// <summary>
        /// Get Membership User
        /// </summary>
        /// <param name="providerUserKey">The unique id for the user</param>
        /// <param name="userIsOnline">Set true if only active user is required</param>
        /// <returns>MembershipUser</returns>
        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            string sSql = @"SELECT User.UserId, User.UserName, 
                            Email, PasswordQuestion,Comment, 
                            IsLockedOut, User.ActionDate, 
                            LastLoginDate,
                            LastPasswordChangeDate, LastLockoutDate
                            FROM User 
                            Inner Join Membership
                            ON User.UserId = Membership.UserId
                            WHERE IsLockedOut = @IsLockedOut
                            AND User.UserId = @UserId";

            Database db = DatabaseFactory.CreateDatabase(_ConnectionStringKey);
            DbCommand cmd = db.GetSqlStringCommand(sSql);
            db.AddInParameter(cmd, "IsLockedOut", DbType.Decimal, Convert.ToDecimal(userIsOnline));
            db.AddInParameter(cmd, "UserId", DbType.String, providerUserKey.ToString());

            return GetMembershipUser(db, cmd);
        }
        #endregion
        #region GetUserNameByEmail()
        /// <summary>
        /// Get User from email 
        /// </summary>
        /// <param name="email"></param>
        /// <returns>UserName</returns>
        public override string GetUserNameByEmail(string email)
        {
            string userName = string.Empty;
            string sSql = @"SELECT UserName
                            FROM User
                            Inner Join Membership
                            ON User.UserId = Membership.UserId
                            WHERE Membership.Email = @Email";

            Database db = DatabaseFactory.CreateDatabase(_ConnectionStringKey);
            DbCommand cmd = db.GetSqlStringCommand(sSql);
            db.AddInParameter(cmd, "Email", DbType.String, email.ToLower());
            using (DbConnection cn = db.CreateConnection())
            {
                cn.Open();
                try
                {
                    using (IDataReader dr = db.ExecuteReader(cmd))
                    {
                        while (dr.Read())
                        {
                            userName = Convert.ToString(dr["UserName"]);
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

            return userName;
        }
        #endregion
        #region ResetPassword()
        /// <summary>
        /// reset user password
        /// </summary>
        /// <param name="username">username</param>
        /// <param name="answer">password secret answer.
        /// if RequiresQuestionAndAnswer set to false at web.config then send null 
        /// secret answer 
        /// </param>
        /// <returns></returns>
        public override string ResetPassword(string username, string answer)
        {
            string newPassword = string.Empty;

            if (!this.EnablePasswordReset)
            {
                throw new ProviderException("Please set EnablePasswordReset to true at web.config to enable the method ResetPassword at SoftOne.Provider.MembershipProvider");
            }

            if (this.RequiresQuestionAndAnswer && string.IsNullOrEmpty(answer))
            {
                throw new ProviderException("Please provide valid password secret question answer for ResetPassword at SoftOne.Provider.MembershipProvider");
            }

            if (this.RequiresQuestionAndAnswer)
            {
                // get password from DB, this will succeed only for valid answer
                string password = Getpassword(username, answer);

                if (string.IsNullOrEmpty(password)) // update failed password answer attempt count and enable/disable user
                {
                    UpdateFailureCount(username, false);
                    // as invalid answer is provide we are not trying to reset the password
                    return string.Empty;
                }
            }

            // generate a new password and check it validity with exisiting rule.
            // continue generation until get a valid password
            ValidatePasswordEventArgs args = null;
            do
            {
                newPassword = Membership.GeneratePassword(this.MinRequiredPasswordLength, this.MinRequiredNonAlphanumericCharacters);
                // check password validity of Provider
                args = new ValidatePasswordEventArgs(username, newPassword, true);
                OnValidatingPassword(args);
            } while (args.Cancel);

            // set the new password.
            string userId = (GetUser(username, false)).ProviderUserKey.ToString();
            if (!string.IsNullOrEmpty(userId))
            {
                if (SetPassword(userId, username, newPassword))
                    return newPassword;
            }

            return string.Empty;
        }
        #endregion
        #region UnlockUser()
        /// <summary>
        /// unlock or approved an user
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public override bool UnlockUser(string userName)
        {
            return LockUser(userName, false);
        }
        #endregion
        #region UpdateUser()
        /// <summary>
        /// Only update the user email and comment about the user.
        /// To update Password, Password Question & Answer and enable/disable user
        /// please use the other methods.
        /// </summary>
        /// <param name="user">MembershipUser</param>
        public override void UpdateUser(MembershipUser user)
        {
            string sSql = @"UPDATE Membership 
                            SET 
                            Email = @Email,
                            Comment = @Comment,
                            UserCode = @UserCode,
                            ActionDate = @ActionDate,
                            ActionType = @ActionType
                            Where UserId = @UserId";

            Database db = DatabaseFactory.CreateDatabase(_ConnectionStringKey);
            DbCommand cmd = db.GetSqlStringCommand(sSql);
            db.AddInParameter(cmd, "UserId", DbType.String, Membership.GetUser(user.UserName).ProviderUserKey);
            db.AddInParameter(cmd, "Email", DbType.String, user.Email.ToLower());
            db.AddInParameter(cmd, "Comment", DbType.String, user.Comment);
            db.AddInParameter(cmd, "UserCode", DbType.String, SOSession.UserCode);
            db.AddInParameter(cmd, "ActionDate", DbType.DateTime, DateTime.Now);
            db.AddInParameter(cmd, "ActionType", DbType.String, DBAction.Update.ToString());
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
        #region ValidateUser()
        /// <summary>
        /// Validate user Authentication providing given username & password.
        /// User must not be locked.
        /// This will also create the user identity at the system thread to make
        /// the user authenticated into the system.
        /// </summary>
        /// <param name="username">username to login</param>
        /// <param name="password">password in plain text</param>
        /// <returns></returns>
        public override bool ValidateUser(string username, string password)
        {
            bool isValid = false;
            bool isFirstLogin = false;
            string userId = string.Empty;

            string sSql = @"SELECT User.UserId,Membership.IsFirstLogin
                            FROM User
                            INNER Join Membership
                            ON User.UserId = Membership.UserId
                            INNER Join UserPassword
                            ON User.UserId = UserPassword.UserId
                            WHERE UserName = @UserName
                            AND Password = @Password
                            AND UserPassword.ActionDate = 
                                        (
                                        SELECT MAX(UserPassword.ActionDate)AS ActionDate
                                        FROM UserPassword
                                        INNER JOIN User
                                        ON UserPassword.UserId = User.UserId
                                        WHERE UserName = @UserName
                                        ) 
                            AND IsLockedOut = 0";

            Database db = DatabaseFactory.CreateDatabase(_ConnectionStringKey);
            DbCommand cmd = db.GetSqlStringCommand(sSql);
            db.AddInParameter(cmd, "UserName", DbType.String, username);
            db.AddInParameter(cmd, "Password", DbType.String, EncodeText(SaltTextFromStoredSaltedText(password, username)));
            using (DbConnection cn = db.CreateConnection())
            {
                cn.Open();
                try
                {
                    using (IDataReader dr = db.ExecuteReader(cmd))
                    {
                        while (dr.Read())
                        {
                            isFirstLogin = Convert.ToBoolean(Convert.ToDecimal(dr["IsFirstLogin"]));
                            userId = Convert.ToString(dr["UserId"]);
                            isValid = true;
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

            if (isValid)
            {
                // Set user Identity & Principal along with FormAuthenticationTicket
                SOSession.Login(userId, username, _ConnectionStringKey);
                // check whetehr user has to change password if it is first login                
                if (this.RequiredChangePasswordFirstLogin && isFirstLogin)
                {
                    SOSession.RequiredChangePassword = true;
                }
                // check whetehr user has to change password has expired
                if (IsPasswordExpired(username))
                {
                    SOSession.RequiredChangePassword = true;
                }
                ResetFailureCount(username, true);
            }
            else // update failed password attempt count and enable/disable user
            {
                UpdateFailureCount(username, true);
            }

            return isValid;
        }
        #endregion
        #region OnValidatingPassword()
        /// <summary>
        /// validating a password whether it match with given criteria
        /// </summary>
        /// <param name="e"></param>
        protected override void OnValidatingPassword(ValidatePasswordEventArgs e)
        {
            // set the default to a valid password
            e.Cancel = false;
            // get the encoded form of current password            
            string encodedPassword = EncodeText(SaltTextFromStoredSaltedText(e.Password, e.UserName));

            // check whether this password is allowed within the scope of
            // last n password            
            string sSql = @"SELECT Password
                            FROM UserPassword                            
                            INNER JOIN User
                            ON UserPassword.UserId = User.UserId
                            WHERE UserName = @UserName
                            ORDER BY UserPassword.ActionDate DESC";

            Database db = DatabaseFactory.CreateDatabase(_ConnectionStringKey);
            DbCommand cmd = db.GetSqlStringCommand(sSql);
            db.AddInParameter(cmd, "UserName", DbType.String, e.UserName);
            using (DbConnection cn = db.CreateConnection())
            {
                cn.Open();
                try
                {
                    using (IDataReader dr = db.ExecuteReader(cmd))
                    {
                        int i = 1;
                        // check the last ith only
                        while (dr.Read() && i <= this.MinRepeatPassAllowed)
                        {
                            if (dr.IsDBNull(dr.GetOrdinal("Password")))
                                break;
                            // if password matches with last N then deny the password
                            if (encodedPassword == Convert.ToString(dr["Password"]))
                            {
                                e.Cancel = true;
                                break;
                            }
                            i++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw (ex);
                }
                finally
                {
                    cn.Close();
                }
            }

            // check requirement of password length
            if (e.Password.Length < _MinRequiredPasswordLength)
                e.Cancel = true;

            // check requirement of non alphanumeric char
            char[] arrNonAlphaNum = _NonAlphanumericCharacters.ToCharArray();
            char[] arrPassword = e.Password.ToCharArray();
            int nonAlphaNumCount = 0;

            for (int i = 0; i < arrPassword.Length; i++)
            {
                for (int j = 0; j < arrNonAlphaNum.Length; j++)
                {
                    if (arrPassword[i] == arrNonAlphaNum[j])
                    {
                        nonAlphaNumCount++;
                        break;
                    }
                }
            }

            if (nonAlphaNumCount < _MinRequiredNonAlphanumericCharacters)
                e.Cancel = true;

            base.OnValidatingPassword(e);
        }
        #endregion
        #endregion
        #region Private Helper Methods
        #region GetSecuritySetting()
        /// <summary>
        /// Get Security Setting from DB.This are user recognisible setting.
        /// But can be overridden by web.config
        /// </summary>
        private void GetSecuritySetting()
        {
            string sSql = @"SELECT MaxInvalidPassAtmpt,ReqChangePassFirstLogin,MinRepeatPassAllowed,PassExpireDay,MinReqPassLen,MinReqNonAlphaNumChar,NonAlphaNumChar
                            FROM SecuritySetting";
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
                            _MaxInvalidPasswordAttempts = Convert.ToInt32(dr["MaxInvalidPassAtmpt"]);
                            _RequiredChangePasswordFirstLogin = Convert.ToBoolean(dr["ReqChangePassFirstLogin"]);
                            _MinRepeatPassAllowed = Convert.ToInt32(dr["MinRepeatPassAllowed"]);
                            _PassExpireDay = Convert.ToInt32(dr["PassExpireDay"]);
                            _MinRequiredPasswordLength = Convert.ToInt32(dr["MinReqPassLen"]);
                            _MinRequiredNonAlphanumericCharacters = Convert.ToInt32(dr["MinReqNonAlphaNumChar"]);
                            _NonAlphanumericCharacters = Convert.ToString(dr["NonAlphaNumChar"]);
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
        #region GetConfigValue()
        /// <summary>
        /// Get value from web.config.If null then the default value is returned.
        /// </summary>
        /// <param name="configValue"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private string GetConfigValue(string configValue, string defaultValue)
        {
            if (String.IsNullOrEmpty(configValue))
                return defaultValue;

            return configValue;
        }
        #endregion
        #region GetPasswordFormat()
        /// <summary>
        /// Get Password Format set by Membership provider.
        /// Only Hased,Encrypted and Clear are supported.
        /// </summary>
        /// <param name="passwordFormat"></param>
        /// <returns></returns>
        private MembershipPasswordFormat GetPasswordFormat(string passwordFormat)
        {
            switch (passwordFormat)
            {
                case "Hashed":
                    return MembershipPasswordFormat.Hashed;
                case "Encrypted":
                    return MembershipPasswordFormat.Encrypted;
                case "Clear":
                    return MembershipPasswordFormat.Clear;
                default:
                    throw new ProviderException("Password format not supported:" + passwordFormat);
            }
        }
        #endregion
        #region EncodeToBase64String()
        /// <summary>
        /// encode a plain text to Base64 string
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private string EncodeToBase64String(string text)
        {
            //return Convert.ToBase64String(EncryptPassword(Encoding.Unicode.GetBytes(text)));
            return Convert.ToBase64String(Encoding.Unicode.GetBytes(text));
        }
        #endregion
        #region DecodeFromBase64String()
        /// <summary>
        /// decode a Base64 string to plain text
        /// </summary>
        /// <param name="encodedText"></param>
        /// <returns></returns>
        private string DecodeFromBase64String(string encodedText)
        {
            //return Encoding.Unicode.GetString(DecryptPassword(Convert.FromBase64String(encodedText)));
            return Encoding.Unicode.GetString(Convert.FromBase64String(encodedText));
        }
        #endregion
        #region EncodeText()
        /// <summary>
        /// Encode any plain text depending on the PaswordFormat provided on web.config
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        private string EncodeText(string text)
        {
            string encodedText = string.Empty;
            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    encodedText = text;
                    break;
                case MembershipPasswordFormat.Encrypted:
                    encodedText = EncodeToBase64String(text);
                    break;
                case MembershipPasswordFormat.Hashed:
                    //use MD5 Hash Algorithom
                    byte[] clearBytes = new UnicodeEncoding().GetBytes(text);
                    byte[] hashedBytes = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(clearBytes);
                    encodedText = BitConverter.ToString(hashedBytes);
                    break;
                default:
                    throw new ProviderException("Unsupported password format at SoftOne.Provider.MembershipProvider : " + PasswordFormat);
            }
            return encodedText;
        }
        #endregion
        #region DecodeText()
        /// <summary>
        /// Decode text based on the PasswordFormat on web.config
        /// </summary>
        /// <param name="encodedPassword"></param>
        /// <returns></returns>
        private string DecodeText(string encodedText)
        {
            string decodedText = string.Empty;

            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    decodedText = encodedText;
                    break;
                case MembershipPasswordFormat.Encrypted:
                    decodedText = DecodeFromBase64String(encodedText);
                    break;
                case MembershipPasswordFormat.Hashed:
                    throw new ProviderException("Cannot decode a hashed password/text at SoftOne.Provider.MembershipProvider");
                default:
                    throw new ProviderException("Unsupported password format at SoftOne.Provider.MembershipProvider : " + PasswordFormat);
            }
            return decodedText;
        }
        #endregion
        #region GeneratePasswordSaltingText()
        /// <summary>
        /// generate a random password salting string
        /// </summary>
        /// <returns></returns>
        public string GeneratePasswordSaltingText()
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            int size = random.Next(_RandomMinLimit, _RandomMaxLimit);
            char ch = char.MinValue;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            return builder.ToString();
        }
        #endregion
        #region SaltText()
        /// <summary>
        /// salt a text
        /// </summary>
        /// <param name="textToSalt">the text to be salted in plain format</param>
        /// <param name="saltedText">the salting text in plain format</param>        
        /// <returns></returns>
        private string SaltText(string textToSalt, string saltedText)
        {
            return (saltedText + textToSalt + saltedText);
        }
        #endregion
        #region UnSaltText()
        /// <summary>
        /// unsalt a text        
        /// </summary>
        /// <param name="textToSalt">the text to unsalt in plain format</param>
        /// <param name="saltedText">the salting text in plain format</param>        
        /// <returns></returns>
        private string UnSaltText(string textToUnSalt, string saltedText)
        {
            // extract the leading SaltedText (i,e, length of saltedText)
            textToUnSalt = textToUnSalt.Remove(0, saltedText.Length);
            // extract the trailing SaltedText (i,e, length of saltedText)
            textToUnSalt = textToUnSalt.Remove(textToUnSalt.Length - saltedText.Length, saltedText.Length);
            return textToUnSalt;
        }
        #endregion
        #region SaltTextFromStoredSaltedText()
        /// <summary>
        /// salt a text after getting the SaltedText from db for the specified username
        /// </summary>
        /// /// <param name="textToSalt">the text to be salted in plain format</param>
        /// <param name="username">username to get the user's SaltedText</param>        
        /// <returns></returns>
        private string SaltTextFromStoredSaltedText(string textToSalt, string username)
        {
            string saltedText = string.Empty;

            string sSql = @"SELECT Membership.PasswordSalt
                            FROM User
                            INNER Join Membership
                            ON User.UserId = Membership.UserId
                            WHERE UserName = @UserName";

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
                            saltedText = Convert.ToString(dr["PasswordSalt"]);
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

            // decrypt the saltedText
            saltedText = DecodeFromBase64String(saltedText);
            // salt the text
            return SaltText(textToSalt, saltedText);
        }
        #endregion
        #region UnSaltTextFromStoredSaltedText()
        /// <summary>
        /// UnSalt a text after getting the SaltedText from db for the specified username
        /// </summary>        
        /// </summary>
        /// <param name="textToUnSalt">the text to unsalt in plain format</param>
        /// <param name="username">username to get the user's SaltedText</param>   
        /// <returns></returns>
        private string UnSaltTextFromStoredSaltedText(string textToUnSalt, string username)
        {
            string SaltedText = string.Empty;

            string sSql = @"SELECT Membership.SaltedText
                            FROM User
                            INNER Join Membership
                            ON User.UserId = Membership.UserId
                            WHERE UserName = @UserName";

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
                            SaltedText = Convert.ToString(dr["SaltedText"]);
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

            // decrypt the saltedText
            SaltedText = DecodeFromBase64String(SaltedText);
            // unsalt the text            
            return UnSaltText(textToUnSalt, SaltedText);
        }
        #endregion
        #region GetMembershipUser()
        /// <summary>
        /// Get a Membership User from DB
        /// </summary>
        /// <param name="db"></param>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private MembershipUser GetMembershipUser(Database db, DbCommand cmd)
        {
            MembershipUser mu = null;
            using (DbConnection cn = db.CreateConnection())
            {
                cn.Open();
                try
                {
                    using (IDataReader dr = db.ExecuteReader(cmd))
                    {
                        while (dr.Read())
                        {
                            object pproviderUserKey = Convert.ToString(dr["UserId"]);
                            string userName = Convert.ToString(dr["UserName"]);
                            string email = Convert.ToString(dr["Email"]);
                            string passwordQuestion = dr.IsDBNull(dr.GetOrdinal("PasswordQuestion")) ? string.Empty : Convert.ToString(dr["PasswordQuestion"]);
                            string comment = Convert.ToString(dr["comment"]);
                            decimal isLockedOut = Convert.ToDecimal(dr["IsLockedOut"]);
                            //decimal isApproved = !isLockedOut;
                            DateTime creationDate = Convert.ToDateTime(dr["ActionDate"]);
                            DateTime lastLoginDate = Convert.ToDateTime(dr["LastLoginDate"]);
                            //DateTime LastActivityDate = LastLoginDate;
                            DateTime lastPasswordChangeDate = Convert.ToDateTime(dr["LastPasswordChangeDate"]);
                            DateTime lastLockoutDate = Convert.ToDateTime(dr["LastLockoutDate"]);

                            // create the Membership object
                            mu = new MembershipUser(this.Name,
                                                    userName,
                                                    pproviderUserKey,
                                                    email,
                                                    passwordQuestion,
                                                    comment,
                                                    !Convert.ToBoolean(isLockedOut),
                                                    Convert.ToBoolean(isLockedOut),
                                                    creationDate,
                                                    lastLoginDate,
                                                    lastLoginDate,
                                                    lastPasswordChangeDate,
                                                    lastLockoutDate);
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

            return mu;
        }
        #endregion
        #region GetNumberofUsers()
        /// <summary>
        /// Return number of users
        /// </summary>
        /// <param name="IsLockedOut">whether we want locked user or unlocked user
        /// if nothing mentioned all users will return</param>
        /// <returns></returns>
        private int GetNumberofUsers(bool? IsLockedOut)
        {
            int numberOfUsers = 0;
            string sSql = @"SELECT COUNT(UserId) AS NumberOfUsers
                            FROM Membership";

            Database db = DatabaseFactory.CreateDatabase(_ConnectionStringKey);
            DbCommand cmd = db.GetSqlStringCommand(sSql);

            if (IsLockedOut.HasValue)
            {
                sSql += @"Where IsLockedOut = @IsLockedOut";
                db.AddInParameter(cmd, "IsLockedOut", DbType.Decimal, Convert.ToDecimal(IsLockedOut));
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
                            numberOfUsers = Convert.ToInt32(dr["NumberOfUsers"]);
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
            return numberOfUsers;
        }
        #endregion
        #region GetPassword()
        /// <summary>
        /// Get the encrypted password from DB
        /// </summary>
        /// <param name="username"></param>
        /// <param name="answer"></param>
        /// <returns>Encrypted Password</returns>
        public string Getpassword(string username, string answer)
        {
            string password = string.Empty;

            string sSql = @"SELECT Password
                            FROM UserPassword
                            INNER JOIN Membership
                            ON UserPassword.UserId = Membership.UserId
                            INNER JOIN User
                            ON UserPassword.UserId = User.UserId
                            WHERE UserName = @UserName
                            AND PasswordAnswer = @PasswordAnswer
                            AND UserPassword.ActionDate = 
                                        (
                                        SELET MAX(ActionDate)
                                        FROM UserPassword
                                        INNER JOIN User
                                        ON UserPassword.UserId = User.UserId
                                        WHERE UserName = @UserName
                                        )";

            Database db = DatabaseFactory.CreateDatabase(_ConnectionStringKey);
            DbCommand cmd = db.GetSqlStringCommand(sSql);
            db.AddInParameter(cmd, "UserName", DbType.String, username);
            db.AddInParameter(cmd, "PasswordAnswer", DbType.String, EncodeText(SaltTextFromStoredSaltedText(answer, username)));
            using (DbConnection cn = db.CreateConnection())
            {
                cn.Open();
                try
                {
                    using (IDataReader dr = db.ExecuteReader(cmd))
                    {
                        while (dr.Read())
                        {
                            password = Convert.ToString(dr["Password"]);
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
            return password;
        }
        #endregion
        #region SetPassword()
        /// <summary>
        /// Set user password into DB
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        private bool SetPassword(string userId, string username, string newPassword)
        {
            bool isSuccess = false;

            Database db = DatabaseFactory.CreateDatabase(_ConnectionStringKey);

            string sSqlUserPassword = @"INSERT INTO UserPassword
                                        (UserId,UserCode,ActionDate,ActionType,Password)
                                        VALUES
                                        (@UserId,@UserCode,@ActionDate,@ActionType,@Password)";
            DbCommand cmdUserPassword = db.GetSqlStringCommand(sSqlUserPassword);
            db.AddInParameter(cmdUserPassword, "UserId", DbType.String, userId);
            db.AddInParameter(cmdUserPassword, "Password", DbType.String, EncodeText(SaltTextFromStoredSaltedText(newPassword, username)));
            db.AddInParameter(cmdUserPassword, "UserCode", DbType.String, SOSession.UserCode);
            db.AddInParameter(cmdUserPassword, "ActionDate", DbType.DateTime, DateTime.Now);
            db.AddInParameter(cmdUserPassword, "ActionType", DbType.String, DBAction.Insert.ToString());

            string sSqlMembership = @"UPDATE Membership
                                        SET
                                        LastPasswordChangeDate = @LastPasswordChangeDate,
                                        UserCode = @UserCode,
                                        ActionDate = @ActionDate,
                                        ActionType = @ActionType
                                        Where UserId = @UserId";

            DbCommand cmdMembership = db.GetSqlStringCommand(sSqlMembership);
            db.AddInParameter(cmdMembership, "LastPasswordChangeDate", DbType.String, DateTime.Now);
            db.AddInParameter(cmdMembership, "UserId", DbType.String, SOSession.UserCode);
            db.AddInParameter(cmdMembership, "UserCode", DbType.String, SOSession.UserCode);
            db.AddInParameter(cmdMembership, "ActionDate", DbType.DateTime, DateTime.Now);
            db.AddInParameter(cmdMembership, "ActionType", DbType.String, DBAction.Update.ToString());

            using (DbConnection cn = db.CreateConnection())
            {
                cn.Open();
                DbTransaction tr = cn.BeginTransaction();
                try
                {
                    int rowsAffetced = db.ExecuteNonQuery(cmdUserPassword);
                    if (rowsAffetced > 0)
                    {
                        rowsAffetced = db.ExecuteNonQuery(cmdMembership);
                        isSuccess = true;
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
            return isSuccess;
        }
        #endregion
        #region UpdateFailureCount()
        /// <summary>
        /// Update the failure count of password or password answer and lock/unlock
        /// user accordingly
        /// </summary>
        /// <param name="username"></param>
        /// <param name="isPasswordFailure"></param>
        private void UpdateFailureCount(string username, bool isPasswordFailure)
        {
            // get current attempt count
            int iFailureCount = 0;
            string sSqlAttemptCount = string.Empty;
            string sSqlMembership = string.Empty;

            if (isPasswordFailure)
            {
                sSqlAttemptCount = @"SELECT FailedPassAtmptCount
                                    FROM Membership
                                    Where UserId =
                                                (
                                                SELECT UserId
                                                FROM User
                                                WHERE UserName = @UserName
                                                )";
            }
            else
            {
                sSqlAttemptCount = @"SELECT FailedPassAnsAtmptCount
                                    FROM Membership
                                    Where UserId =
                                                (
                                                SELECT UserId
                                                FROM User
                                                WHERE UserName = @UserName
                                                )";
            }

            Database dbAttemptCount = DatabaseFactory.CreateDatabase(_ConnectionStringKey);
            DbCommand cmdAttemptCount = dbAttemptCount.GetSqlStringCommand(sSqlAttemptCount);
            dbAttemptCount.AddInParameter(cmdAttemptCount, "UserName", DbType.String, username);
            using (DbConnection cn = dbAttemptCount.CreateConnection())
            {
                cn.Open();
                try
                {
                    using (IDataReader dr = dbAttemptCount.ExecuteReader(cmdAttemptCount))
                    {
                        while (dr.Read())
                        {
                            iFailureCount = Convert.ToInt32(dr["FailedPassAtmptCount"]);
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

            // update counter
            if (isPasswordFailure)
            {
                sSqlMembership = @"UPDATE Membership
                                        SET 
                                        FailedPassAtmptCount = @FailedPassAtmptCount,
                                        ActionDate = @ActionDate,
                                        ActionType = @ActionType
                                        Where UserId =                                                     
                                                    (
                                                    SELECT UserId
                                                    FROM User
                                                    WHERE UserName = @UserName
                                                    )";
            }
            else
            {
                sSqlMembership = @"UPDATE Membership
                                        SET 
                                        FailedPassAnsAtmptCount = @FailedPassAnsAtmptCount,
                                        ActionDate = @ActionDate,
                                        ActionType = @ActionType
                                        Where UserId =                                                     
                                                    (
                                                    SELECT UserId
                                                    FROM User
                                                    WHERE UserName = @UserName
                                                    )";
            }
            Database dbMembership = DatabaseFactory.CreateDatabase(_ConnectionStringKey);
            DbCommand cmdMembership = dbMembership.GetSqlStringCommand(sSqlMembership);

            if (isPasswordFailure)
                dbMembership.AddInParameter(cmdMembership, "FailedPassAtmptCount", DbType.Decimal, Convert.ToDecimal(iFailureCount + 1));
            else
                dbMembership.AddInParameter(cmdMembership, "FailedPassAnsAtmptCount", DbType.Decimal, Convert.ToDecimal(iFailureCount + 1));

            dbMembership.AddInParameter(cmdMembership, "UserName", DbType.String, username);
            dbMembership.AddInParameter(cmdMembership, "ActionDate", DbType.DateTime, DateTime.Now);
            dbMembership.AddInParameter(cmdMembership, "ActionType", DbType.String, DBAction.Update.ToString());
            using (DbConnection cn = dbMembership.CreateConnection())
            {
                cn.Open();
                try
                {
                    dbMembership.ExecuteNonQuery(cmdMembership);
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

            // lock user if failed password attempt count exceed maximum
            if (iFailureCount > this.MaxInvalidPasswordAttempts)
                LockUser(username, true);
        }
        #endregion
        #region ResetFailureCount()
        /// <summary>
        /// Reset the failure count of password or password answer and to 0
        /// </summary>
        /// <param name="username"></param>
        /// <param name="isPasswordFailure"></param>
        private void ResetFailureCount(string username, bool isPasswordFailure)
        {
            // get current attempt count            
            string sSqlAttemptCount = string.Empty;
            string sSqlMembership = string.Empty;

            // update counter
            if (isPasswordFailure)
            {
                sSqlMembership = @"UPDATE Membership
                                        SET 
                                        FailedPassAtmptCount = @FailedPassAtmptCount,
                                        ActionDate = @ActionDate,
                                        ActionType = @ActionType
                                        Where UserId =                                                     
                                                    (
                                                    SELECT UserId
                                                    FROM User
                                                    WHERE UserName = @UserName
                                                    )";
            }
            else
            {
                sSqlMembership = @"UPDATE Membership
                                        SET 
                                        FailedPassAnsAtmptCount = @FailedPassAnsAtmptCount,
                                        ActionDate = @ActionDate,
                                        ActionType = @ActionType
                                        Where UserId =                                                     
                                                    (
                                                    SELECT UserId
                                                    FROM User
                                                    WHERE UserName = @UserName
                                                    )";
            }
            Database dbMembership = DatabaseFactory.CreateDatabase(_ConnectionStringKey);
            DbCommand cmdMembership = dbMembership.GetSqlStringCommand(sSqlMembership);

            if (isPasswordFailure)
                dbMembership.AddInParameter(cmdMembership, "FailedPassAtmptCount", DbType.Decimal, 0);
            else
                dbMembership.AddInParameter(cmdMembership, "FailedPassAnsAtmptCount", DbType.Decimal, 0);

            dbMembership.AddInParameter(cmdMembership, "UserName", DbType.String, username);
            dbMembership.AddInParameter(cmdMembership, "ActionDate", DbType.DateTime, DateTime.Now);
            dbMembership.AddInParameter(cmdMembership, "ActionType", DbType.String, DBAction.Update.ToString());
            using (DbConnection cn = dbMembership.CreateConnection())
            {
                cn.Open();
                try
                {
                    dbMembership.ExecuteNonQuery(cmdMembership);
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
        #region LockUser()
        /// <summary>
        /// Lock/Inactive or Unlock/Active an user
        /// </summary>
        /// <param name="username"></param>
        /// <param name="Lock"></param>
        /// <returns></returns>
        private bool LockUser(string username, bool lockUser)
        {
            bool isSuccess = false;
            string sSql = @"UPDATE Membership 
                            SET 
                            IsLockedOut = @IsLockedOut,                            
                            ActionDate = @ActionDate,
                            ActionType = @ActionType";
            if (!lockUser)
            {
                sSql += ", FailedPassAtmptCount = @FailedPassAtmptCount";
            }
            sSql += " Where UserId = @UserId";
            Database db = DatabaseFactory.CreateDatabase(_ConnectionStringKey);
            DbCommand cmd = db.GetSqlStringCommand(sSql);
            db.AddInParameter(cmd, "UserId", DbType.String, GetUser(username, false).ProviderUserKey.ToString());
            db.AddInParameter(cmd, "IsLockedOut", DbType.Decimal, Convert.ToDecimal(lockUser));
            db.AddInParameter(cmd, "ActionDate", DbType.DateTime, DateTime.Now);
            db.AddInParameter(cmd, "ActionType", DbType.String, DBAction.Update.ToString());
            db.AddInParameter(cmd, "FailedPassAtmptCount", DbType.Decimal, Convert.ToDecimal(lockUser));
            using (DbConnection cn = db.CreateConnection())
            {
                cn.Open();
                try
                {
                    int rowsAffetced = db.ExecuteNonQuery(cmd);
                    if (rowsAffetced > 0)
                        isSuccess = true;
                }
                catch
                {
                    isSuccess = false;
                    throw;
                }
                finally
                {
                    cn.Close();
                }
            }
            return isSuccess;
        }
        #endregion
        #region IsPasswordExpired()
        /// <summary>
        /// check whether password is expired for specified user
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private bool IsPasswordExpired(string username)
        {
            DateTime dtLastPassChangeDate = DateTime.MinValue;
            DateTime dtToday = DateTime.Now;

            string sSql = @"SELECT LastPasswordChangeDate
                            FROM Membership                            
                            INNER JOIN User
                            ON Membership.UserId = User.UserId
                            WHERE UserName = @UserName";

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
                            dtLastPassChangeDate = Convert.ToDateTime(dr["LastPasswordChangeDate"]);
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
            // now check with PassExpireDay
            if (dtLastPassChangeDate != new DateTime(1800, 1, 1))
            {
                int daysElapased = dtToday.Subtract(dtLastPassChangeDate).Days;
                if (daysElapased > this.PassExpireDay)
                    return true;
                return false;
            }
            else
            {
                return false;
            }
        }
        #endregion
        #region ChangeLoginFlag()
        /// <summary>
        /// After first login change first login flag
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="connectionStringKey"></param>
        public static void ChangeLoginFlag(string userId, string connectionStringKey)
        {
            string sSqlMembership = @"UPDATE Membership SET IsFirstLogin = @IsFirstLogin Where UserId = @UserId";
            Database db = DatabaseFactory.CreateDatabase(connectionStringKey);
            DbCommand cmdMembership = db.GetSqlStringCommand(sSqlMembership);
            db.AddInParameter(cmdMembership, "UserId", DbType.String, userId);
            db.AddInParameter(cmdMembership, "IsFirstLogin", DbType.Decimal, 0);
            using (DbConnection cn = db.CreateConnection())
            {
                cn.Open();
                DbTransaction tr = cn.BeginTransaction();
                try
                {
                    //Membership
                    db.ExecuteNonQuery(cmdMembership);
                }
                catch
                {
                    //
                }
                finally
                {
                    cn.Close();
                }

            }
        }

        #endregion
        #endregion
    }
}
