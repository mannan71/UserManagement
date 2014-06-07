using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UM.Model;

namespace UM.Interfaces
{
    public interface IUserInfo
    {
        #region UserRelated API implement in business class--------------------------

        List<UserInfo> GetUsers(string UserName);
        UserInfo GetUserByName(string UserName);
        UserInfo GetUserByID(string UserID);
        string SaveUser(UserInfo objUserInfo);
        string ResetPassword(string UserName);
        bool AuthenticateUser(string UserName, string Password);
        string ChangePassword(string UserName, string OldPassword, string NewPassword);
        string GetNonAlphaNumChar();       
        
        #endregion ----------------------------------------------------UserRelated API

        #region UserRole related API implement in business class---------------------

        //UserInRole Realted API
        List<UserInRole> GetUserInRolesByUserId(string sUserID);
        string SaveUserInRole(string UserID, string RoleId);

        #endregion ---------------------------------------------UserRole related API
    }
}
