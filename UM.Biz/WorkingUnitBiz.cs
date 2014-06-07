using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UM.DataContext;
using UM.Interfaces;
using UM.Model;

namespace UM.Biz
{
    public class WorkingUnitBiz : IWorkingUnit
    {
        #region Methods --------------------

        /// <summary>
        /// Get Hierarchal WorkingUnit
        /// </summary>
        /// <returns></returns>
        public List<WorkingUnit> GenerateWorkingUnitTree()
        {
            List<WorkingUnit> objWorkingUnitList = WorkingUnitDC.GenerateWorkingUnitTree();
            List<WorkingUnit> objWorkingUnitFinalList = new List<WorkingUnit>();
            WorkingUnit objWorkingUnit = null;
            for (int i = 0; i < objWorkingUnitList.Count; i++)
            {
                objWorkingUnit = objWorkingUnitList[i];
                if (objWorkingUnit.ParentUnitCode == 0 )
                {
                    objWorkingUnit.ChildItems = GenerateChildIWorkingUnit(objWorkingUnit.WorkingUnitCode_PK, objWorkingUnitList);
                    objWorkingUnitFinalList.Add(objWorkingUnit);
                    break;
                }
            }
            return objWorkingUnitFinalList;
        }

        /// <summary>
        /// Save WorkingUnit to DB
        /// Save both New nand Updated WorkingUnit
        /// </summary>
        /// <param name="objWorkingUnit"></param>
        /// <returns></returns>
        public string SaveWorkingUnit(WorkingUnit objWorkingUnit)
        {
            return WorkingUnitDC.SaveWorkingUnit(objWorkingUnit);
        }

        /// <summary>
        /// Save WorkingUnit Logo
        /// </summary>
        /// <param name="objWorkingUnitLogo"></param>
        /// <returns></returns>
        public string SaveWorkingUnitLogo(WorkingUnitLogo objWorkingUnitLogo)
        {
            return WorkingUnitDC.SaveWorkingUnitLogo(objWorkingUnitLogo);
        }

        /// <summary>
        /// Delete WorkingUnit 
        /// </summary>
        /// <param name="WorkingUnitCode"></param>
        /// <returns></returns>
        public string DeleteWorkingUnit(int WorkingUnitCode)
        {
            return WorkingUnitDC.DeleteWorkingUnit(WorkingUnitCode);
        }

        /// <summary>
        /// Delete WorkingUnitLogo
        /// </summary>
        /// <param name="WorkingUnitCode"></param>
        /// <returns></returns>
        public string DeleteWorkingUnitLogo(int WorkingUnitCode)
        {
            return WorkingUnitDC.DeleteWorkingUnitLogo(WorkingUnitCode);
        }

        /// <summary>
        /// Get WorkingUnit by UnitCode
        /// </summary>
        /// <param name="WorkingUnitCode"></param>
        /// <returns></returns>
        public WorkingUnit GetWorkingUnitByUnitCode(int WorkingUnitCode)
        {
            return WorkingUnitDC.GetWorkingUnitByUnitCode(WorkingUnitCode);
        }

        /// <summary>
        /// Get workingUnitLogo
        /// </summary>
        /// <param name="WorkingUnitCode"></param>
        /// <returns></returns>
        public WorkingUnitLogo GetWorkingUnitLogoById(int WorkingUnitCode)
        {
            return WorkingUnitDC.GetWorkingUnitLogoById(WorkingUnitCode);
        }

        /// <summary>
        /// Get Hierarchal child WorkingUnit
        /// </summary>
        /// <param name="UnitCode"></param>
        /// <param name="objWorkingUnitList"></param>
        /// <returns></returns>
        protected List<WorkingUnit> GenerateChildIWorkingUnit(int UnitCode, List<WorkingUnit> objWorkingUnitList)
        {
            List<WorkingUnit> oChildList = new List<WorkingUnit>();
            WorkingUnit objWorkingUnit = null;
            for (int i = 0; i < objWorkingUnitList.Count; i++)
            {
                objWorkingUnit = objWorkingUnitList[i];
                if (objWorkingUnit.ParentUnitCode == UnitCode)
                {
                    objWorkingUnit.ChildItems = GenerateChildIWorkingUnit(objWorkingUnit.WorkingUnitCode_PK, objWorkingUnitList);
                    oChildList.Add(objWorkingUnit);
                }                
            }
            return oChildList;
        }

        #endregion ------------------Methods
    }
}
