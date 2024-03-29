﻿using Caliburn.Micro;
using Moq;
using NUnit.Framework;
using QLNet;
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
                Assert.That(enterPosition.NumTrades, Is.EqualTo(1).Or.EqualTo(3));                
                Assert.That(exitPosition.PnlPerTrade, Is.GreaterThan(0));
                Assert.That(exitPosition.PnLCum, Is.GreaterThan(0).Or.LessThan(0));
                Assert.That(exitPosition.PnLDaily, Is.GreaterThan(0).Or.LessThan(0));
                Assert.That(exitPosition.PnLCumHold, Is.GreaterThan(0));
                Assert.That(exitPosition.PnLDailyHold, Is.GreaterThan(0).Or.LessThan(0));
                Assert.That(exitPosition.NumTrades, Is.EqualTo(2).Or.EqualTo(4));
            }
        }        

        [Test]
        public void WhenGettingYearlyPnl()
        {            
            var today = DateTime.Now;            
            var pnls = new List<PnlEntity>
            {
                // Sharpe takes stdev PnlDaily, PnlDailyHold and avgs of them as well
                GiveMeAPnlEntitiy(NextDateFor(2020), 1, 100.0, 0, 0, 0),
                GiveMeAPnlEntitiy(NextDateFor(2020), 1, 110.0, 10, 10, 50),
                GiveMeAPnlEntitiy(NextDateFor(2020), 2, 120.0, 10, 20, 30),
                GiveMeAPnlEntitiy(NextDateFor(2021), 0, 40.0, 10, 90, 80),
                GiveMeAPnlEntitiy(NextDateFor(2021), 1, 130.0, 30, 80, 210),
                GiveMeAPnlEntitiy(NextDateFor(2021), 1, 140.0, 40, 30, 310),
                GiveMeAPnlEntitiy(NextDateFor(2021), 2, 150.0, 50, 190, 50),
            };

            foreach (var row in BacktestHelper.GetYearlyPnl(pnls))
            {
                Console.WriteLine($"numTrades={row.numTrades},ticker={row.ticker},year={row.year},pnl={row.pnl},pnl2={row.pnl2},sp0={row.sp0},sp1={row.sp1}");
            }

            PnlEntity GiveMeAPnlEntitiy(DateTime date, int numTrades, double pnlDaily, double pnlCum, double pnlDailyHold, double pnlCumHold)
            {
                var pnl = new Mock<PnlEntity>();
                pnl.SetupGet(p => p.Date).Returns(date);
                pnl.SetupGet(p => p.NumTrades).Returns(numTrades);
                pnl.SetupGet(p => p.PnLDaily).Returns(pnlDaily);
                pnl.SetupGet(p => p.PnLCum).Returns(pnlCum);
                pnl.SetupGet(p => p.PnLDailyHold).Returns(pnlDailyHold);
                pnl.SetupGet(p => p.PnLCumHold).Returns(pnlCumHold);
                pnl.SetupGet(p => p.Ticker).Returns("TICKER");

                return pnl.Object;           
            }

            DateTime NextDateFor(int year)
            {
                var date = new DateTime(year, 2, 3);
                int counter;

                if (!yearCounter.TryGetValue(year, out counter))
                {
                    yearCounter.Add(year, 0);
                }

                return date.AddDays(counter);
            }
        }

        Dictionary<int, int> yearCounter = new Dictionary<int, int>();
        #endregion

        #region trading signals (SignalHelper)

        [Test]
        [TestCase(SignalTypeEnum.MovingAverage)]
        [TestCase(SignalTypeEnum.LinearRegression)]
        [TestCase(SignalTypeEnum.RSI)]
        [TestCase(SignalTypeEnum.WilliamR)]
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
                Assert.That(s.Signal, Is.GreaterThan(0).Or.LessThan(0));
                Assert.That(s.PricePredicted, Is.GreaterThan(0));
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

        [Test]
        public void WhenGettingPairCorrelationItShouldReturnValidBetas()
        {
            double[] betas = new double[] { };
            var startDate = DateTime.Now;
            var endDate = DateTime.Now.AddDays(20); 

            var signals = SignalHelper.GetPairCorrelation("aTicker1", "aTicker2", startDate, endDate, 0, out betas);

            Assert.That(betas.Length, Is.GreaterThan(1));   
            Assert.That(signals.Count, Is.GreaterThan(0));
        }
        [Test]
        public void WhenGettingPairPriceRatioForEmptyInputSignals()
        {
            Assert.That(SignalHelper.GetPairPriceRatioSignal(new PairSignalEntity[] { }, 10), Is.EqualTo(Enumerable.Empty<PairSignalEntity>()));
        }
        [Test]
        public void WhenGettingPairPriceRatio()
        {
            var builder = new PairSignalBuilder();
            PairSignalEntity[] inputPairSignals = new[]
            {
                builder.NewSignal(1.1, 5.0),
                builder.NewSignal(1.0, 5.0),
                builder.NewSignal(1.2, 4.0),
                builder.NewSignal(1.0, 5.0),
                builder.NewSignal(1.3, 4.0),
            };
            int movingWindow = 3;
            var pairSignals = new List<PairSignalEntity>(SignalHelper.GetPairPriceRatioSignal(inputPairSignals, movingWindow));

            Console.WriteLine("inputs:");
            Print(inputPairSignals);
            Console.WriteLine("results:");
            Print(pairSignals);

            Assert.That(pairSignals.Count, Is.EqualTo(3));
            foreach(var signal in pairSignals)
            {
                Assert.That(signal, Is.Not.EqualTo(0));
            }
        }
     
        [Test]
        public void WhenGettingPairSpreads()
        {
            var builder = new PairSignalBuilder();            
            PairSignalEntity[] inputPairSignals = new[]
            {
                builder.NewSignal(1.0, 2.0),
                builder.NewSignal(3.0, 1.0),
                builder.NewSignal(15.0, 2.0),
                builder.NewSignal(1.0, 3.0),
                builder.NewSignal(5.0, 2.0),            
            };
            int movingWindow = 3;
            var pairSignals = new List<PairSignalEntity>(SignalHelper.GetPairSpreadSignal(inputPairSignals, movingWindow));

            Console.WriteLine("inputs:");
            Print(inputPairSignals);
            Console.WriteLine("results:");
            Print(pairSignals);

            Assert.That(pairSignals.Count, Is.EqualTo(3));
            foreach (var signal in pairSignals)
            {
                Assert.That(signal, Is.Not.EqualTo(0));
            }
        }
        #endregion

        private static void Print(IEnumerable<PairSignalEntity> signals)
        {
            foreach (var signal in signals)
            {
                Console.WriteLine(signal);
            }
        }

        private Random random = new Random();
        private double RandomPrice() => random.Next(0, 99) + random.NextDouble();
    }
}
