#region .NET
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#endregion

#region Third Party
using CalcFarm.AnalyticUtil.Entity;
#endregion

namespace CalcFarm.AnalyticUtil
{
    public class QuantLib
    {
        // Each additional 0.33 ms in calculation you need add additional CPU/processor to stay in pace with PriceServer 3k/sec
        public static IList<CalcResult> fastCalcOnly(String instrumentId, Price price)
        {
            IList<CalcResult> Results;

            switch (price.InstAssetClass)
            {
                case AssetClass.EquityPx:
                    Results = handlePriceEquity(instrumentId, price);
                    break;
                case AssetClass.Fx:
                    Results = handlePriceFx(instrumentId, price);
                    break;
                case AssetClass.Rates:
                    Results = handlePriceRates(instrumentId, price);
                    break;
                case AssetClass.ForwardFuture:
                    Results = handlePriceForwardFuture(instrumentId, price);
                    break;
                default:
                    throw new ArgumentException("Unsupported Asset Class: " + price.InstAssetClass);
            }

            return Results;
        }

        protected static IList<CalcResult> handlePriceEquity(String instrumentId, Price price)
        {
            EquityPrice EqtyPrice = price as EquityPrice;

            /* Example, retrieve relevant positions where position.InstId == instrumentId
                For each position, update unrealized Pnl.
                You can't run expensive operations such as derivatives sensitivities (Delta/Gamma/Vega/Theta/Rho...etc).
                Also, be careful with the number of positions you need published back to message queue! This has big impact on how many CPU/Processors you will need.
             */
            IList<CalcResult> Results = new List<CalcResult>();
            // For example, dummy calc to update pnl
            Random rnd = new Random();
            for (int i = 0; i < 5; i++)
            {
                int PositionId = rnd.Next(1000, 10000);
                int QTY = rnd.Next(0, 1000);
                double AvgCost = (1 + rnd.Next(-10, 10) / 100) * EqtyPrice.LastTradePx;
                double UnrealizedPnl = QTY * (EqtyPrice.LastTradePx - AvgCost);

                var Result = new EqtyUpdPnlCalcResult() { Message = "handlePriceEquity", PositionId = PositionId, UnrealisedPnl = UnrealizedPnl, Px = price };
                Results.Add(Result);
            }
            return Results;
        }

        protected static IList<CalcResult> handlePriceFx(String instrumentId, Price price)
        {
            // For example, update position currency/risk exposure of all positions where inst or und ccy = ccy of fx updates
            IList<CalcResult> Results = new List<CalcResult>();
            // ... add to Results ...
            Results.Add(new FxUpdCalcResult() { Message = "handlePriceFx", Px = price });
            return Results;
        }

        protected static IList<CalcResult> handlePriceRates(String instrumentId, Price price)
        {
            // For example, update yield curves, update bond floor ... etc (Fixed income). Swap valuation may be too expensive here.
            IList<CalcResult> Results = new List<CalcResult>();
            // ... add to Results ...
            Results.Add(new RatesUpdCalcResult() { Message = "handlePriceRates", Px = price });
            return Results;
        }

        protected static IList<CalcResult> handlePriceForwardFuture(String instrumentId, Price price)
        {
            /* You may perform simply calculation for example to relcaulate simple parity/premium for index options. 
             * Some high freq idx arb strategies try take profit by exploiting difference between spot/future market. 
             * You probably don't do Hedge adjustment on real time basis! 
             */
            IList<CalcResult> Results = new List<CalcResult>();
            // ... add to Results ...
            Results.Add(new ForwardFutureUpdCalcResult() { Message = "handlePriceForwardFuture", Px = price });
            return Results;
        }
    }
}
