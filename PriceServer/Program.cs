#region .NET
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#endregion

using CalcFarm.AnalyticUtil;
using CalcFarm.AnalyticUtil.Entity;

namespace CalcFarm.PriceServer
{
    class Program
    {
        static PriceServerApplicationController AppCtrl = new PriceServerApplicationController();

        static System.Threading.TimerCallback cb = new System.Threading.TimerCallback(ReportStatistics);
        static System.Threading.Timer tmr = new Timer(cb, null, 0, 1000); // Start timer
        static int CountPublishes, CountPublishesCulmulative = 0;

        static void ReportStatistics(object e)
        {
            StringBuilder buffer = new StringBuilder();
            buffer.Append("CountPublishes: ").Append(CountPublishes / 1000).Append("k, CountPublishesCulmulative: ").Append(CountPublishesCulmulative / 1000).Append("k");
            AppCtrl.Info(buffer.ToString());
            
            // Resets
            CountPublishes = 0;

            return;
        }

        static void Main(string[] args)
        {
            CalcFarm.AnalyticUtil.MarketDataService MktDataSrv = null;
            Price[] Prices = null;
            
            StringBuilder buffer = new StringBuilder();

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

                MktDataSrv = new MarketDataService(AppCtrl.QueueUrl);

                CountPublishes = 0;
                CountPublishesCulmulative = 0;
                while (true)
                {
                    if (AppCtrl.MaxCountPublishesCulmulative != 0 && CountPublishesCulmulative > AppCtrl.MaxCountPublishesCulmulative)
                    {
                        Console.WriteLine("Stop publishing, reaching limit MaxCountPublishesCulmulative: " + AppCtrl.MaxCountPublishesCulmulative);
                        tmr.Change(Timeout.Infinite, Timeout.Infinite); // Stop timer
                        break;
                    }

                    Prices = TestDataUtil.GenerateTestData();
                    foreach (Price px in Prices)
                    {
                        if (CountPublishes > AppCtrl.PerSecPublishThrottle)
                        {
                            break;
                        }

                        // On this test/dev machine with Intel 2.6GHz Single Processor with 4GB RAM, PriceServer publishes about 10,000 price updates per sec.
                        MktDataSrv.PublishPrice(px);

                        CountPublishes++;
                        CountPublishesCulmulative++;

                        if (AppCtrl.DetailLog) // For debugging only, this will slow publish rate significantly.
                        {
                            buffer.Clear();
                            buffer.Append("Published price for ").Append(px.ToString());
                            AppCtrl.Info(buffer.ToString());
                        }
                    }
                    Prices = null;
                }

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
