#region .NET
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#endregion

#region Third Party
using CalcFarm.AnalyticUtil.Entity;
#endregion

namespace CalcFarm.AnalyticUtil
{
    public class TestDataUtil
    {
        static long MaxPublishId = 0;

        static IDictionary<string, DateTime> LastCalc = null;
        static EquityPrice[] LastPrices = null;

        static string[] GetTestSymbolList()
        {
            string[] Symbols = new string[20]; // For illustration purpose, only two constituents under HSI

            Symbols[0] = "0001HK";
            Symbols[1] = "0002HK";
            Symbols[2] = "0003HK";
            Symbols[3] = "0004HK";
            Symbols[4] = "0005HK";
            Symbols[5] = "0006HK";
            Symbols[6] = "0011HK";
            Symbols[7] = "0012HK";
            Symbols[8] = "0013HK";
            Symbols[9] = "0016HK";
            Symbols[10] = "0017HK";
            Symbols[11] = "0019HK";
            Symbols[12] = "0023HK";
            Symbols[13] = "0027HK";
            Symbols[14] = "0066HK";
            Symbols[15] = "0083HK";
            Symbols[16] = "0101HK";
            Symbols[17] = "0135HK";
            Symbols[18] = "0144HK";
            Symbols[19] = "0151HK";

            return Symbols;
        }

        static double GetStartingPrice(int i)
        {
            if (i == 0)
            {
                return 134.80;
            }
            else if (i == 1)
            {
                return 62.85;
            }
            else if (i == 2)
            {
                return 16.64;
            }
            else if (i == 3)
            {
                return 55.35;
            }
            else if (i == 4)
            {
                return 79.95;
            }
            else if (i == 6)
            {
                return 66.60;
            }
            else if (i == 7)
            {
                return 126.30;
            }
            else if (i == 8)
            {
                return 46.00;
            }
            else if (i == 9)
            {
                return 105.50;
            }
            else if (i == 10)
            {
                return 105.50;
            }
            else if (i == 11)
            {
                return 8.80;
            }
            else if (i == 12)
            {
                return 94.05;
            }
            else if (i == 13)
            {
                return 32.00;
            }
            else if (i == 14)
            {
                return 57.65;
            }
            else if (i == 15)
            {
                return 29.10;
            }
            else if (i == 16)
            {
                return 12.40;
            }
            else if (i == 17)
            {
                return 23.55;
            }
            else if (i == 18)
            {
                return 12.78;
            }
            else if (i == 19)
            {
                return 24.00;
            }
            else if (i == 20)
            {
                return 10.12;
            }
            else
            {
                return 0;
            }
        }

        static double GetNextPrice(string Symbol, double startingPrice, double prevPrice)
        {
            Random rnd = new Random(Symbol.GetHashCode() + DateTime.Now.Second);
            double nextPrice = 0;

            DateTime ThisIter = DateTime.Now;
            DateTime PrevIter;
            TimeSpan timeDiff;
            if (LastCalc.ContainsKey(Symbol))
            {
                PrevIter = LastCalc[Symbol];
                timeDiff = ThisIter.Subtract(PrevIter);
            }
            else
            {
                timeDiff = new TimeSpan(0); // i.e. no drift for first iteration
            }
            
            // Assume nominal uptrend drift 10% price annual, and cheaper stock has higher drift rate
            double nominalDayDriftRate = 1 + 0.10/365;
            double instDriftFactor = startingPrice<100? (100 - startingPrice)/100 * nominalDayDriftRate : nominalDayDriftRate;

            // Assume noise max 15% daily change
            double noiseFactor = rnd.Next(-15, 15) / 100;

            nextPrice = 
                prevPrice
                + instDriftFactor * (timeDiff.TotalSeconds / (24 * 60 * 60)) * prevPrice  // Drift
                + (noiseFactor * prevPrice);    // Noise

            LastCalc[Symbol] = ThisIter;

            return nextPrice;
        }

        public static Price[] GenerateTestData()
        {
            string[] Symbols = GetTestSymbolList();
            double StartingPrice, Bid, Offer, LastTrade;
            int BidSize, OfferSize;
            EquityPrice px;
            EquityPrice lastPx;

            Random rnd = null;

            if (LastPrices == null)
            {
                LastPrices = new EquityPrice[Symbols.Length];
            }

            if (LastCalc == null)
            {
                LastCalc = new Dictionary<string, DateTime>();
            }

            for (int i = 0; i < Symbols.Length; i++)
            {
                rnd = new Random(i);

                StartingPrice = GetStartingPrice(i);

                BidSize = rnd.Next(0, 100) * 100;
                OfferSize = rnd.Next(0, 100) * 100;
                if (LastPrices[i] != null)
                {
                    lastPx = LastPrices[i];
                    Bid = GetNextPrice(Symbols[i], StartingPrice, lastPx.Bid);
                    Offer = GetNextPrice(Symbols[i], StartingPrice, lastPx.Bid);
                    LastTrade = GetNextPrice(Symbols[i], StartingPrice, lastPx.LastTradePx);
                }
                else
                {
                    Bid = GetNextPrice(Symbols[i], StartingPrice, StartingPrice);
                    Offer = GetNextPrice(Symbols[i], StartingPrice, StartingPrice);
                    LastTrade = GetNextPrice(Symbols[i], StartingPrice, StartingPrice);
                }

                px = new EquityPrice() { 
                    PublishId = MaxPublishId+1,

                    InstIdentifier = Symbols[i], 
                    InstCcy = "HKD",
                    InstAssetClass = AssetClass.EquityPx,
                    Bid = Bid, 
                    BidSize = BidSize,
                    Offer = Offer, 
                    OfferSize = OfferSize,
                    LastTradePx = LastTrade,
                    LastTradeTime = DateTime.Now.AddSeconds(rnd.Next(-60, 0)),
                    Revc = DateTime.Now };

                LastPrices[i] = px;
            }

            return LastPrices;
        }
    }
}
