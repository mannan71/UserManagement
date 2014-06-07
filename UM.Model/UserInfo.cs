using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S1.CommonBiz;

namespace UM.Model
{
    public class UserInfo : BaseModel
    {    
        #region Public properties ----------------

        /// <summary>
        /// Get or Set Table name
        /// </summary>
        public string TableName_TBL { get; set; }

        /// <summary>
        ///  Get and Set userID
        /// </summary>
        public string UserID_PK { get; set; }

        /// <summary>
        /// Get and Set UserName
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Get or Set user email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Get or Set user password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Get or Set workingUnitCode
        /// </summary>
        public int CompanyCode { get; set; }

        /// <summary>
        /// Get or Set workingUnitCode
        /// </summary>
        public int WorkingUnitCode { get; set; }
       
        /// <summary>
        /// Get or Set IsLokedOut decision
        /// </summary>
        public bool IsLokedOut { get; set; }

        /// <summary>
        /// Get or Set IsFirstLogin
        /// </summary>
        public bool  IsFirstLogin { get; set; }

        /// <summary>
        /// Get or Set LastLoginDate
        /// </summary>
        public DateTime LastLoginDate { get; set; }

        /// <summary>
        /// Get or Set LastPasswordChangeDate
        /// </summary>
        public DateTime LastPasswordChangeDate { get; set; }
        
        /// <summary>
        /// Get or Set LastLockoutDate
        /// </summary>
        public DateTime LastLockoutDate { get; set; }

        /// <summary>
        /// Get or Set FailedPassAtmptCount
        /// </summary>
        public int FailedPassAtmptCount { get; set; }

        /// <summary>
        /// Get or Set FailedPassAnsAtmptCount
        /// </summary>
        public int FailedPassAnsAtmptCount { get; set; }

        /// <summary>
        /// Get or Set PasswordSalt
        /// </summary>
        public string PasswordSalt { get; set; }

        /// <summary>
        /// Get or Set PasswordQuestion
        /// </summary>
        public string  PasswordQuestion { get; set; }

        /// <summary>
        /// Get or Set PasswordAnswer
        /// </summary>
        public string PasswordAnswer { get; set; }

        /// <summary>
        /// Get or Set Comment
        /// </summary>
        public string  Comment { get; set; }

        /// <summary>
        /// Get or Set User Full Name
        /// This name get from user's profile
        /// </summary>
        public string UserFullName { get; set; }

        /// <summary>
        /// Get or Set ErrorMessage_VW
        /// </summary>
        public string ErrorMessage_VW { get; set; }

        /// <summary>
        /// Get or Set User Role
        /// </summary>
        public List<UserInRole> UserRoles { get; set; }

        #endregion =--------------------Public properties
    }
}
