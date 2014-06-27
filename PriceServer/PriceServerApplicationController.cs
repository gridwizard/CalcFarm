#region .NET
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
#endregion

using CalcFarm.AnalyticUtil;

namespace CalcFarm.PriceServer
{
    public class PriceServerApplicationController : ApplicationController
    {
        private const int DEFAULT_PERSEC_PUBLISH_THROTTLE = 3000;

        public int PerSecPublishThrottle { get; set; }
        public int MaxCountPublishesCulmulative { get; set; }

        public override void LoadConfiguration()
        {
            string sTmp = null;
            int nTmp = 0;

            sTmp = ConfigurationSettings.AppSettings["PerSecPublishThrottle"] as string;
            if (!string.IsNullOrEmpty(sTmp))
            {
                sTmp = sTmp.Trim();
                if (Int32.TryParse(sTmp, out nTmp))
                {
                    PerSecPublishThrottle = nTmp;
                }
                else
                {
                    PerSecPublishThrottle = DEFAULT_PERSEC_PUBLISH_THROTTLE;
                }
            }

            sTmp = ConfigurationSettings.AppSettings["MaxCountPublishesCulmulative"] as string;
            if (!string.IsNullOrEmpty(sTmp))
            {
                sTmp = sTmp.Trim();
                if (Int32.TryParse(sTmp, out nTmp))
                {
                    MaxCountPublishesCulmulative = nTmp;
                }
                else
                {
                    MaxCountPublishesCulmulative = 0;
                }
            }

            Info("PerSecPublishThrottle: " + PerSecPublishThrottle);
            Info("MaxCountPublishesCulmulative: " + MaxCountPublishesCulmulative);

            base.LoadConfiguration();

            return;
        }
    }
}
