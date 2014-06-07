using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using UM.DataContext;
using UM.Interfaces;
using UM.Model;

namespace UM.Biz
{
    public class UserInfoBiz : IUserInfo
    {
        protected string SAVE = ConfigurationManager.AppSettings["SAVE"].ToString();
        protected string DELETE = ConfigurationManager.AppSettings["DELETE"].ToString();
        #region Methods ------------------------------
        //Methods related userInfo
       
        #region UserInfoBusiness methods -------------
        
        /// <summary>
        /// Get userList
        /// </summary>
        /// <param name="UserName"></param>
        /// <returns></returns>
        public List<UserInfo> GetUsers(string UserName)
        {
            return UserInfoDC.GetUsers(UserName);
        }

        /// <summary>
        /// Get specific user by name
        /// </summary>
        /// <param name="UserName"></param>
        /// <returns></returns>
        public UserInfo GetUserByName(string UserName)
        {
            return UserInfoDC.GetUserByName(UserName);
        }

        /// <summary>
        /// get user by ID
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public UserInfo GetUserByID(string UserID)
        {
            return UserInfoDC.GetUserByID(UserID);
        }

        /// <summary>
        /// Save user
        /// </summary>
        /// <param name="objUserInfo"></param>
        /// <returns></returns>
        public string SaveUser(UserInfo objUserInfo)
        {
            string vReturnMessage = string.Empty;
            //Set automated password
            string Password = Guid.NewGuid().ToString();
            objUserInfo.Password = (Password.Split('-'))[0];
            if (objUserInfo.UserRoles.Count == 0)
            {
                objUserInfo.UserRoles = GetUserInRolesByUserId(objUserInfo.UserID_PK);
            }
            //objUserInfo.Password = "softone_admin";
            vReturnMessage =  UserInfoDC.SaveUser(objUserInfo);            
            return vReturnMessage;
        }

        /// <summary>
        /// Authencate user and login to the system
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="Password"></param>
        /// <returns></returns>
        public bool AuthenticateUser(string UserName, string Password)
        {
            return UserInfoDC.AuthenticateUser(UserName, Password);
        }

        /// <summary>
        /// Reset user password
        /// </summary>
        /// <param name="UserName"></param>
        /// <returns></returns>
        public string ResetPassword(string UserName)
        {
            return UserInfoDC.ResetPassword(UserName);
        }

        /// <summary>
        /// Change user password
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="OldPassword"></param>
        /// <param name="NewPassword"></param>
        /// <returns></returns>
        public string ChangePassword(string UserName, string OldPassword, string NewPassword)
        {
            return UserInfoDC.ChangePassword(UserName, OldPassword, NewPassword);
        }

        /// <summary>
        /// Get NonAlphaNumChar from DB to show user
        /// At change password
        /// </summary>
        /// <returns></returns>
        public string GetNonAlphaNumChar()
        {
            return UserInfoDC.GetNonAlphaNumChar();
        }           
        #endregion ------------UserInfoBusiness methods

        //Methods related UserInRole

        #region UserInRole business methods-----------

        /// <summary>
        /// Gwt userRole by ID
        /// </summary>
        /// <param name="sUserID"></param>
        /// <returns></returns>
        public List<UserInRole> GetUserInRolesByUserId(string sUserID)
        {
            return UserInfoDC.GetUserInRolesByUserId(sUserID);
        }

       /// <summary>
       /// Save userRole
       /// </summary>
       /// <param name="UserID"></param>
       /// <param name="objUserInRole"></param>
       /// <returns></returns>
        public string SaveUserInRole(string UserID, string  RoleId)
        {
            return UserInfoDC.SaveUserInRole(UserID, RoleId, null);
        }
        #endregion ---------UserInRole business methods

        #endregion ---------------------------Methods
    }
}
