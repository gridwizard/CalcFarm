using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalcFarm.AnalyticUtil.Entity
{
    [Serializable]
    public abstract class CalcResult
    {
        public int PositionId { get; set; }
        public Price Px { get; set; }

        public string Message { get; set; }

        public String getInstrumentId()
        {
            return Px.InstIdentifier;
        }

        public override string ToString()
        {
            StringBuilder buffer = new StringBuilder();
            buffer.Append("PositionId: ").Append(PositionId)
                .Append(", Px: ").Append(Px.ToString())
                .Append(", Message: ").Append(Message);
            return buffer.ToString();
        }
    }
}
