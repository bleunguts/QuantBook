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
        public async Task WhenComputingLongShortPnl()
       {
            var startDate = DateTime.Parse("2012-7-31");
            var endDate = DateTime.Parse("2015-7-31");
            var ticker = "GS";
            var data = new List<SignalEntity>(await SignalHelper.GetStockData(ticker, startDate, endDate, PriceTypeEnum.Close));
            var signals = SignalHelper.GetSignal(data, movingWindow: 3, SignalTypeEnum.LinearRegression);
            
            const double notional = 10_000.0;
            const double signalIn = 2;
            const double signalOut = 0.0;
            // fudge the signals to get it to first enter->exit a long position then enter->exit a short position
            signals.ElementAt(1).Signal = -signalIn - 0.1;
            signals.ElementAt(4).Signal = signalIn + 0.1;
            var pnlEntities = BacktestHelper.ComputeLongShortPnl(signals, notional, signalIn, signalOut, StrategyTypeEnum.MeanReversion, false).ToList();
            Assert.That(pnlEntities, Has.Count.GreaterThan(0));
            for (int i = 0; i < pnlEntities.Count; i++)
            {
                var entity = pnlEntities[i];
                Console.WriteLine($"PnlEntity {entity.Date} results to Pnl: {entity.PnLCum} PnlCummulative: {entity.PnLCumHold}");

                if (i == 0) continue;

                Assert.That(entity.Date, Is.Not.Null);
                Assert.That(entity.PnLCumHold, Is.GreaterThan(0));
            }

            // there are two trades LONG and SHORT (with two positions each enter/exit)
            Assert.That(pnlEntities.Where(p => p.TradeType == PnlTradeType.POSITION_SHORT).ToList(), Has.Count.EqualTo(2));
            Assert.That(pnlEntities.Where(p => p.TradeType == PnlTradeType.POSITION_LONG).ToList(), Has.Count.EqualTo(2));
            AssertPosition(pnlEntities.Where(p => p.TradeType == PnlTradeType.POSITION_LONG));
            AssertPosition(pnlEntities.Where(p => p.TradeType == PnlTradeType.POSITION_SHORT));

            void AssertPosition(IEnumerable<PnlEntity> positions)
            {
                var enterPosition = positions.ElementAt(0);
                var exitPosition = positions.ElementAt(1);
                Assert.That(enterPosition.NumTrades, Is.GreaterThan(0));
                Assert.That(exitPosition.PnLCum, Is.GreaterThan(0));
                Assert.That(exitPosition.PnlPerTrade, Is.GreaterThan(0));                
            }
        }

        #endregion

        #region trading signals (SignalHelper)

        [Test]
        [TestCase(SignalTypeEnum.MovingAverage)]
        [TestCase(SignalTypeEnum.LinearRegression)]
        [TestCase(SignalTypeEnum.RSI, Ignore = "Not Implemented")]
        [TestCase(SignalTypeEnum.WilliamR, Ignore ="NotImplemented")]
        public async Task WhenComputingSignalUsingVariousProjectionTechniques(SignalTypeEnum signalType)
        {
            var startDate = new DateTime(2012, 3, 1);
            var endDate = new DateTime(2013, 7, 31);
            var ticker = "IBM";
           
            startDate = DateTime.Parse("2012-7-31");
            endDate = DateTime.Parse("2015-7-31");
            ticker = "GS";
            var rawData = await SignalHelper.GetStockData(ticker, startDate, endDate, PriceTypeEnum.Close);

            var signals = SignalHelper.GetSignal(rawData, 10, signalType);
            foreach (var s in signals)
            {
                Console.WriteLine($"Signal Data: {s}");
                Assert.That(s.PricePredicted, Is.GreaterThan(0));
                Assert.That(s.Signal, Is.GreaterThan(0).Or.LessThan(0));
                Assert.That(s.LowerBand, Is.GreaterThan(0));
                Assert.That(s.UpperBand, Is.GreaterThan(0));
            }
        }

        [Test]
        [TestCase("IBM", "2012-3-1", "2013-7-31")]
        [TestCase("GS", "2012-7-31", "2015-7-31")]
        public async Task WhenGettingSignalsFromQuandl(string ticker, string from, string to)
        {            
            var data = await SignalHelper.GetStockData(ticker, DateTime.Parse(from), DateTime.Parse(to), PriceTypeEnum.Close);
            Assert.That(data, Has.Count.GreaterThan(0));            
            data.ToList().ForEach(s => Console.WriteLine($"Raw Data: {s}"));                       
        }

        #endregion
    }
}
