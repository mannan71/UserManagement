using System;
using S1.CommonBiz;
using System.Collections.Generic;
using System.Security;

namespace UM.Model
{
    public class RoleInfo : BaseModel
    {
        #region Private Member Variables
        private string _TableNm_TBL;
        private string _RoleId_PK;
        private string _RoleName;
        #endregion
        
        #region Public Properties
        public string TableNm_TBL
        {
            get 
            { 
                return _TableNm_TBL; 
            }
            set
            {
                if (value != _TableNm_TBL)
                    _TableNm_TBL = value;
            }
        }
        public string RoleId_PK
        {
            get { return _RoleId_PK; }
            set
            {
                if (value != _RoleId_PK)
                    _RoleId_PK = value;
            }
        }
        public string RoleName
        {
            get { return _RoleName; }
            set
            {
                if (value != _RoleName)
                    _RoleName = value;
            }
        }
        public List<ActionInfo> RoleActions { get; set; }
        #endregion
    }
}