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
        //[Ignore("TODO PnlCum > 0 need to understand the maths in the quant routine")]
        public async Task WhenComputingLongShortPnl()
        {
            /*
             * Real data IBM 1/7/2013 from quandl 
                Date=17/07/2013 00:00:00,Price=168.82936959634,UpperBand=170.040695011224,PricePredicted=167.829670630647,LowerBand=165.61864625007,Signal=0.904285791215263,Ticker=IBM
                Date=18/07/2013 00:00:00,Price=171.81458178555,UpperBand=171.585684198481,PricePredicted=168.241005100905,LowerBand=164.896326003329,Signal=2.13687267471193,Ticker=IBM
                Date=19/07/2013 00:00:00,Price=167.95289741288,UpperBand=171.404351262573,PricePredicted=168.120381701399,LowerBand=164.836412140226,Signal=-0.10200112114266,Ticker=IBM
                Date=22/07/2013 00:00:00,Price=168.43018424545,UpperBand=171.24941463122,PricePredicted=168.043148013945,LowerBand=164.83688139667,Signal=0.241424858069982,Ticker=IBM
                Date=23/07/2013 00:00:00,Price=169.20252111999,UpperBand=171.292573132983,PricePredicted=168.362496294652,LowerBand=165.432419456321,Signal=0.573380748483464,Ticker=IBM
                Date=24/07/2013 00:00:00,Price=170.6170257329,UpperBand=171.768900283097,PricePredicted=168.74085458375,LowerBand=165.712808884403,Signal=1.23919605939557,Ticker=IBM
                Date=25/07/2013 00:00:00,Price=171.14638021994,UpperBand=172.31471474994,PricePredicted=169.124419638294,LowerBand=165.934124526648,Signal=1.26756962029308,Ticker=IBM
                Date=26/07/2013 00:00:00,Price=171.25919347128,UpperBand=172.516675679546,PricePredicted=169.582614997569,LowerBand=166.648554315592,Signal=1.14283830870289,Ticker=IBM
                Date=29/07/2013 00:00:00,Price=170.26990803648,UpperBand=172.599676976714,PricePredicted=169.774397524841,LowerBand=166.949118072968,Signal=0.350769203598906,Ticker=IBM
                Date=30/07/2013 00:00:00,Price=170.09634918827,UpperBand=172.56969713632,PricePredicted=169.961841080908,LowerBand=167.353985025496,Signal=0.103156082624174,Ticker=IBM
                Date=31/07/2013 00:00:00,Price=169.25458877445,UpperBand=172.543101770421,PricePredicted=170.004362998719,LowerBand=167.465624227017,Signal=-0.590666698461809,Ticker=IBM

              var data = new List<SignalEntity>()
            {
                new SignalEntity { Date = new DateTime(2013, 7, 17), Price = 168.82936959634, UpperBand = 170.040695011224, LowerBand = 165.61864625007, Signal =0.904285791215263, PricePredicted = 167.829670630647, Ticker="IBM"},
                new SignalEntity { Date = new DateTime(2013, 7, 18), Price = 171.81458178555, UpperBand = 171.585684198481, LowerBand = 164.896326003329, Signal =2.13687267471193, PricePredicted = 168.241005100905, Ticker="IBM"},
                new SignalEntity { Date = new DateTime(2013, 7, 19), Price = 167.95289741288, UpperBand = 171.404351262573, LowerBand = 164.836412140226, Signal =-0.10200112114266, PricePredicted = 168.120381701399, Ticker="IBM"},
            };
             */

            var startDate = DateTime.Parse("2012-7-31");
            var endDate = DateTime.Parse("2015-7-31");
            var ticker = "GS";
            var data = new List<SignalEntity>(await SignalHelper.GetStockData(ticker, startDate, endDate, PriceTypeEnum.Close));
            var signals = SignalHelper.GetSignal(data, movingWindow: 3, SignalTypeEnum.LinearRegression);

            // fudge the signals to get it to first enter->exit a long position then enter->exit a short position
            signals.ElementAt(1).Signal = -2.1;
            signals.ElementAt(4).Signal = 2.1;

            const double notional = 10_000.0;
            const double signalIn = 2;
            const double signalOut = 0.0;
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
