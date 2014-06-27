#region .NET
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#endregion

using CalcFarm.AnalyticUtil;

namespace CalcFarm.SimpleTradingScreen
{
    class Program
    {
        static void Main(string[] args)
        {
            ApplicationController AppCtrl = new ApplicationController();
            CalcFarm.AnalyticUtil.AnalyticsSubscriberService AnalyticSrv = null;

            try
            {
                #region Configurations
                if (args != null && args.Length > 0)
                {
                    string QueueUrl = null;
                    bool DetailLog = false;

                    if (args[0].ToLower().Trim() == "detaillog")
                    {
                        DetailLog = true;
                    }

                    if (args.Length > 1)
                    {
                        QueueUrl = args[1];
                        if (!string.IsNullOrEmpty(QueueUrl))
                        {
                            QueueUrl = QueueUrl.Trim();
                        }
                        else
                        {
                            QueueUrl = "localhost";
                        }
                    }

                    AppCtrl.QueueUrl = QueueUrl;
                    AppCtrl.DetailLog = DetailLog;
                }
                else
                {
                    AppCtrl.LoadConfiguration();
                }
                #endregion

                AnalyticSrv = new AnalyticsSubscriberService(AppCtrl, AppCtrl.QueueUrl, AppCtrl.DetailLog);

                Console.WriteLine("Hit any key to exit");
                Console.ReadLine();
            }
            catch (Exception Ex)
            {
                AppCtrl.Fatal(ExceptionUtil.ExceptionToString(Ex));
            }

            return;
        }
    }
}
