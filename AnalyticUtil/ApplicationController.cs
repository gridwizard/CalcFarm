#region .NET
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
#endregion

using log4net;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace CalcFarm.AnalyticUtil
{
    public class ApplicationController
    {
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public string QueueUrl { get; set; }
        public bool DetailLog { get; set; }
        public int ThreadPoolSize { get; set; }
        public int MaxThreadPoolSize { get; set; }

        #region Logging related
        public void Info(string Message)
        {
            Logger.Info(Message);
            return;
        }

        public void Warn(string Message)
        {
            Logger.Warn(Message);
            return;
        }

        public void Error(string Message)
        {
            Logger.Error(Message);
            return;
        }

        public void Fatal(string Message)
        {
            Logger.Fatal(Message);
            return;
        }
        #endregion

        public virtual void LoadConfiguration()
        {
            string sTmp = null;
            bool bTmp = false;
            int nTmp = 0;

            sTmp = ConfigurationSettings.AppSettings["QueueUrl"] as string;
            if (!string.IsNullOrEmpty(sTmp))
            {
                QueueUrl = sTmp.Trim();
            }

            sTmp = ConfigurationSettings.AppSettings["DetailLog"] as string;
            if (!string.IsNullOrEmpty(sTmp))
            {
                sTmp = sTmp.Trim();
                if (Boolean.TryParse(sTmp, out bTmp))
                {
                    DetailLog = bTmp;
                }
                else
                {
                    DetailLog = false;
                }
            }

            sTmp = ConfigurationSettings.AppSettings["MaxThreadPoolSize"] as string;
            if (!string.IsNullOrEmpty(sTmp))
            {
                sTmp = sTmp.Trim();
                if (Int32.TryParse(sTmp, out nTmp))
                {
                    MaxThreadPoolSize = nTmp;
                }
                else
                {
                    MaxThreadPoolSize = Environment.ProcessorCount;
                }
            }

            ThreadPoolSize = Environment.ProcessorCount;
            ThreadPool.SetMaxThreads(ThreadPoolSize, ThreadPoolSize);

            Info("QueueUrl: " + QueueUrl);
            Info("DetailLog: " + DetailLog);
            Info("ThreadPoolSize: " + ThreadPoolSize);
            Info("MaxThreadPoolSize: " + MaxThreadPoolSize);

            return;
        }
    }
}
