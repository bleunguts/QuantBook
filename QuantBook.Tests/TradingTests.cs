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
            signals.ElementAt(0).Signal = -signalIn - 0.1; 
            signals.ElementAt(1).Signal = signalOut + 0.1; // trade 1 enters LONG position prevsignal < -2
            signals.ElementAt(2).Signal = -1;              // trade 2 exits LONG position prevsignal > 0
            signals.ElementAt(3).Signal = signalIn + 0.1;  
            signals.ElementAt(4).Signal = 0.1;             // trade 4 enters SHORT position prevsignal > 2
            signals.ElementAt(5).Signal = signalOut - 0.1; 
            signals.ElementAt(6).Signal = 0.1;             // trade 6 exits SHORT position prevsignal < 0             
            var longPrice = signals.ElementAt(0).Price;
            var initialPrice = signals.ElementAt(0).Price;
            signals.ElementAt(1).Price = longPrice + 50.00; // $50 gain from long trade
            signals.ElementAt(2).Price = longPrice + 51.00;
            signals.ElementAt(3).Price = initialPrice + 100;
            var shortPrice = signals.ElementAt(3).Price;
            signals.ElementAt(4).Price = shortPrice - 48.00; 
            signals.ElementAt(5).Price = shortPrice - 49.00; 
            signals.ElementAt(6).Price = shortPrice - 50.00; // $50 gain from short trade

            var pnlEntities = BacktestHelper.ComputeLongShortPnl(signals, notional, signalIn, signalOut, StrategyTypeEnum.MeanReversion, false).ToList();
            Assert.That(pnlEntities, Has.Count.GreaterThan(0));
            for (int i = 0; i < pnlEntities.Count; i++)
            {
                var entity = pnlEntities[i];
                Console.WriteLine($"PnlEntity[{i}] {entity.Date} results to PnlCumm: {entity.PnLCum} PnlCummHold: {entity.PnLCumHold}");

                if (i == 0) continue;

                Assert.That(entity.Date, Is.Not.Null);                
                Assert.That(entity.PnLCumHold, Is.TypeOf<double>());
                Assert.That(entity.PnLDaily, Is.TypeOf<double>());
            }

            // there are two trades LONG and SHORT (with two positions each enter/exit)
            Assert.That(pnlEntities.Where(p => p.TradeType == PnlTradeType.POSITION_LONG).ToList(), Has.Count.EqualTo(2));
            Assert.That(pnlEntities.Where(p => p.TradeType == PnlTradeType.POSITION_SHORT).ToList(), Has.Count.EqualTo(3));
            AssertPosition(pnlEntities.Where(p => p.TradeType == PnlTradeType.POSITION_LONG), PnlTradeType.POSITION_LONG);
            AssertPosition(pnlEntities.Where(p => p.TradeType == PnlTradeType.POSITION_SHORT), PnlTradeType.POSITION_SHORT);

            void AssertPosition(IEnumerable<PnlEntity> positions, PnlTradeType tradeType)
            {
                var enterPosition = positions.First();
                var exitPosition = positions.Last();
                Console.WriteLine($"Enter / Exit position found for {tradeType}");
                Console.WriteLine($"Exit Position NumTrades={exitPosition.NumTrades}, PnlPerTrade={exitPosition.PnlPerTrade}, PnlCum={exitPosition.PnLCum}, PnlDaily={exitPosition.PnLDaily}, PnlCumHold={exitPosition.PnLCumHold}, PnlDailyHold={exitPosition.PnLDailyHold}");
                Assert.That(enterPosition.NumTrades, Is.GreaterThan(0));                
                Assert.That(exitPosition.PnlPerTrade, Is.GreaterThan(0));
                Assert.That(exitPosition.PnLCum, Is.GreaterThan(0).Or.LessThan(0));
                Assert.That(exitPosition.PnLDaily, Is.GreaterThan(0).Or.LessThan(0));
                Assert.That(exitPosition.PnLCumHold, Is.GreaterThan(0));
                Assert.That(exitPosition.PnLDailyHold, Is.GreaterThan(0).Or.LessThan(0));
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
