#region .NET
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace CalcFarm.AnalyticUtil.Entity
{
    [Serializable]
    public class EquityPrice : Price
    {
        public double Bid { get; set; }
        public double BidSize { get; set; }
        public double Offer { get; set; }
        public double OfferSize { get; set; }
        public double LastTradePx { get; set; }
        public DateTime LastTradeTime { get; set; }
        public double Open { get; set; }
        public double Close { get; set; }

        public DateTime Revc { get; set; }

        public override string ToString()
        {
            StringBuilder buffer = new StringBuilder();
            buffer.Append(base.ToString())
                .Append(", LastTradePx: ").Append(LastTradePx)
                .Append(", LastTradeTime: ").Append(LastTradeTime)
                .Append(", Bid: ").Append(Bid)
                .Append(", BidSize: ").Append(BidSize)
                .Append(", Offer: ").Append(Offer)
                .Append(", OfferSize: ").Append(OfferSize);
            return buffer.ToString();
        }
    }
}
