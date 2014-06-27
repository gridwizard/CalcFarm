using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalcFarm.AnalyticUtil.Entity
{
    [Serializable]
    public class EqtyUpdPnlCalcResult : CalcResult
    {
        public double UnrealisedPnl { get; set; }

        public override string ToString()
        {
            StringBuilder buffer = new StringBuilder();
            buffer.Append(base.ToString())
                .Append(", UnrealisedPnl: ").Append(UnrealisedPnl);
            return buffer.ToString();
        }
    }
}
