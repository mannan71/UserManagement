using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S1.CommonBiz;

namespace UM.Model
{
    public class WorkingUnit : BaseModel
    {
        #region Properties -------------------

        /// <summary>
        /// Get or Set Table name
        /// </summary>
        public string TableName_TBL { get; set; }

        /// <summary>
        /// Get or Set WorkingUnitCode
        /// </summary>
        public int  WorkingUnitCode_PK { get; set; }

        /// <summary>
        /// Get or Set WorkingUnitName
        /// </summary>
        public string  WorkingUnitName { get; set; }

        /// <summary>
        /// Get or Set WorkingUnitType
        /// </summary>
        public string  WorkingUnitType { get; set; }

        /// <summary>
        /// Get or Set ParentUnitCode
        /// </summary>
        public int  ParentUnitCode { get; set; }

        /// <summary>
        /// Get or Set ParentUnitName
        /// </summary>
        public string  ParentUnitName { get; set; }
        /// <summary>
        /// Get or Set WorkingUnitPatern
        /// </summary>
        public string WorkingUnitPatern { get; set; }

        /// <summary>
        /// Get or Set WorkingUnitAddress
        /// </summary>
        public string WorkingUnitAddress { get; set; }       
      
        /// <summary>
        /// Get or Set Logo
        /// </summary>
        public byte[] Logo { get; set; }

        /// <summary>
        /// Get or Set ChildItems
        /// </summary>
        public List<WorkingUnit> ChildItems { get; set; }
        #endregion -----------------Properties
    }
}
