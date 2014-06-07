using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UM.Model;

namespace UM.Interfaces
{
    public interface IWorkingUnit
    {
        #region Method Signature ------------------

        List<WorkingUnit> GenerateWorkingUnitTree();
        string SaveWorkingUnit(WorkingUnit objWorkingUnit);
        string DeleteWorkingUnit(int WorkingUnitCode);
        WorkingUnit GetWorkingUnitByUnitCode(int WorkingUnitCode);

        string SaveWorkingUnitLogo(WorkingUnitLogo objWorkingUnitLogo);
        string DeleteWorkingUnitLogo(int WorkingUnitCode);
        WorkingUnitLogo GetWorkingUnitLogoById(int WorkingUnitCode);

        #endregion -------------Method Signature

    }
}
