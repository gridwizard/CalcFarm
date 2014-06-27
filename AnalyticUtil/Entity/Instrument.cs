using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalcFarm.AnalyticUtil.Entity
{
    [Serializable]
    public class Instrument
    {
        public string InstIdentifier { get; set; }
        public AssetClass InstAssetClass { get; set; }
        public string InstCcy { get; set; }

        public IList<Instrument> Underliers { get; set; }

        public override string ToString()
        {
            StringBuilder buffer = new StringBuilder();
            buffer.Append(", Identifier: ").Append(InstIdentifier)
                .Append(", AssetClass: ").Append(InstAssetClass)
                .Append(", InstCcy: ").Append(InstCcy);
            return buffer.ToString();
        }
    }
}
