using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S1.CommonBiz;

namespace UM.Model
{
    public class WorkingUnitLogo : BaseModel
    {
       
        #region Public properties-------------------------

        /// <summary>
        /// Get or Set Table name
        /// </summary>
        public string TableName_TBL { get; set; }

        /// <summary>
        /// Get or Set WorkingUnitCode_PK
        /// </summary>
        public int WorkingUnitCode_PK { get; set; }

        /// <summary>
        /// Get or Set WorkingUnitName
        /// </summary>
        public string WorkingUnitName { get; set; }

        /// <summary>
        /// Get or Set Logo
        /// </summary>
        public byte[] Logo { get; set; }        

        #endregion --------------------Public properties
    }
}
