#region .NET
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#endregion

using CalcFarm.AnalyticUtil.Entity;

namespace CalcFarm.AnalyticUtil
{
    interface ICalcService
    {
        IList<CalcResult> calculate(String instrumentId, Price price);
    }
}
