#region .NET
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#endregion

using NUnit.Framework;

namespace CalcFarmNunitTest
{
    [TestFixture]
    public class PerformanceTest
    {
        public const string PROJROOT = @"D:\tmp\CalcFarm";

        [Test]
        public void AffirmCalcServerInPaceWithPriceServer()
        {
            string CalcServerPerformanceDumpFile = PROJROOT + @"\CalcServer\bin\Release\CalcServerStatistics.log";
            string CalcServerPath = PROJROOT + @"\CalcServer\bin\Release\CalcServer.exe";
            string PriceServerPath = PROJROOT + @"\PriceServer\bin\Release\PriceServer.exe";
            
            // Delete previous CalcServer performance dump file "CalcServerStatistics.log"
            File.Delete(CalcServerPerformanceDumpFile);

            // Kick start CalcServer from command prompt
            System.Diagnostics.Process CalcServerProc = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo CalcServerStartInfo = new System.Diagnostics.ProcessStartInfo();
            CalcServerStartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            CalcServerStartInfo.FileName = CalcServerPath;
            CalcServerProc.StartInfo = CalcServerStartInfo;
            CalcServerProc.Start();

            // Kick start PriceServer from command prompt
            System.Diagnostics.Process PriceServerProc = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo PriceServerStartInfo = new System.Diagnostics.ProcessStartInfo();
            PriceServerStartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            PriceServerStartInfo.FileName = PriceServerPath;
            PriceServerProc.StartInfo = PriceServerStartInfo;
            PriceServerProc.Start();

            // Wait ~30 sec, kill CalcServer and PriceServer
            Thread.Sleep(1000 * 30);
            CalcServerProc.Kill();
            PriceServerProc.Kill();

            // Parse dump file "CalcServerStatistics.log", asset that "Gap" < 1k during test duration
            string[] Lines = File.ReadAllLines(CalcServerPerformanceDumpFile);
            string sTmp = null;
            int nTmp;
            int Index = 0;
            int Gap = 0;
            int i = 0;
            foreach (string Line in Lines)
            {
                if (!string.IsNullOrEmpty(Line))
                {
                    Index = Line.LastIndexOf(' ');
                    sTmp = Line.Substring(Index + 1);
                    if (!string.IsNullOrEmpty(sTmp))
                    {
                        sTmp = sTmp.Replace("k", "");
                        if (Int32.TryParse(sTmp, out nTmp))
                        {
                            Gap += nTmp;

                            i++;
                        }
                    }
                }
            }

            double AverageGap = Gap / i;
            Console.WriteLine(AverageGap);
            Assert.LessOrEqual(AverageGap, 10);

            return;
        }
    }
}
