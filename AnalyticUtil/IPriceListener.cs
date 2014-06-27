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
    interface IPriceListener
    {
        void handlePrice(String instrumentId, Price price);
    }
}
