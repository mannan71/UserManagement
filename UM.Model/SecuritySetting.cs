using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S1.CommonBiz;

namespace UM.Model
{
    public class SecuritySetting : BaseModel
    {
        #region Properties -----------------------

        /// <summary>
        /// Get or Set Table name
        /// </summary>
        public string TableName_TBL { get; set; }

        /// <summary>
        /// Get or Set SecuritySettingId
        /// </summary>
        public string SecuritySettingId_PK { get; set; }
        /// <summary>
        /// Get or Set MaxInvalidPassAtmpt
        /// </summary>
        public int  MaxInvalidPassAtmpt { get; set; }

        /// <summary>
        /// Get or Set ReqChangePassFirstLogin
        /// </summary>
        public bool ReqChangePassFirstLogin { get; set; }

        /// <summary>
        /// Get or Set MinRepeatPassAllowed
        /// </summary>
        public int MinRepeatPassAllowed { get; set; }

        /// <summary>
        /// Get or Set PassExpireDay
        /// </summary>
        public int PassExpireDay { get; set; }

        /// <summary>
        /// Get or Set MinReqPassLen
        /// </summary>
        public int MinReqPassLen { get; set; }

        /// <summary>
        /// Get or Set MinReqNonAlphaNumChar
        /// </summary>
        public int MinReqNonAlphaNumChar { get; set; }

        /// <summary>
        /// Get or Set NonAlphaNumChar
        /// </summary>
        public string NonAlphaNumChar { get; set; }
        #endregion ----------------------Properties
    }
}
