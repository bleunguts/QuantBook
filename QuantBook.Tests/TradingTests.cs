using Caliburn.Micro;
using NUnit.Framework;
using QuantBook.Models.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantBook.Tests
{
    public class TradingTests
    {
        #region Backtesting (BacktestHelper)
        [Test]
        [Ignore("TODO PnlCum > 0 need to understand the maths in the quant routine")]
        public void WhenComputingLongShortPnl()
        {

            /*
             * Real data IBM 1/1/2018 from quandl 
             * Signal Entity: Date=28/02/2018 00:00:00,Price=155.83,UpperBand=0,PricePredicted=0,LowerBand=0,Signal=0,Ticker=IBM
             * Signal Entity: Date=01/03/2018 00:00:00,Price=153.81,UpperBand=0,PricePredicted=0,LowerBand=0,Signal=0,Ticker=IBM
             * Signal Entity: Date=02/03/2018 00:00:00,Price=154.49,UpperBand=0,PricePredicted=0,LowerBand=0,Signal=0,Ticker=IBM
             */
            var input = new List<SignalEntity>()
            {
                new SignalEntity { Date = new DateTime(2018, 2, 28), Price = 155.83, UpperBand = 0, LowerBand = 0, Signal =0, PricePredicted = 0, Ticker="IBM"},
                new SignalEntity { Date = new DateTime(2018, 1, 3), Price = 153.81, UpperBand = 0, LowerBand = 0, Signal =0, PricePredicted = 0, Ticker="IBM"},
                new SignalEntity { Date = new DateTime(2018, 2, 2), Price = 154.49, UpperBand = 0, LowerBand = 0, Signal =0, PricePredicted = 0, Ticker="IBM"},
            };
            StrategyTypeEnum strategyType =  StrategyTypeEnum.MeanReversion;
            const double notional = 10_000.0;
            const double signalIn = 2.0;
            const double signalOut = 0.0;
            var pnlEntities = BacktestHelper.ComputeLongShortPnl(input, notional, signalIn, signalOut, strategyType, false);
            Assert.That(pnlEntities, Has.Count.GreaterThan(0));
            foreach(PnlEntity entity in pnlEntities)
            {
                Console.WriteLine($"PnlEntity {entity.Date} results to Pnl: {entity.PnLCum} PnlCummulative: {entity.PnLCumHold}");
                Assert.That(entity.Date, Is.Not.Null);
                Assert.That(entity.PnLCum, Is.GreaterThan(0));
                Assert.That(entity.PnLCumHold, Is.GreaterThan(0));
            }
        }

        #endregion

        #region trading signals (SignalHelper)

        [Test]
        [Ignore("WIP")]

        public void WhenGettingSignalUsingLinearRegression()
        {
            var input = new BindableCollection<SignalEntity>();
            SignalHelper.GetSignal(input, 1, SignalTypeEnum.LinearRegression);
        }

        [Test]
        public async Task WhenGettingSignalEntitiesFromQuandl()
        {
            var startDate = new DateTime(2018, 1, 1);
            var endDate = new DateTime(2019, 1, 1);
            var signalEntities = await SignalHelper.GetStockDataAsync("IBM", startDate, endDate, PriceTypeEnum.Close);
            Assert.That(signalEntities, Has.Count.GreaterThan(0));            
            foreach(var s in signalEntities)
            {
                Console.WriteLine($"Signal Entity: {s}");
            }
        }

        #endregion
    }
}
