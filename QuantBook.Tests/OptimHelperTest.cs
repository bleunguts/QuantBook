using Caliburn.Micro;
using NUnit.Framework;
using QLNet;
using QuantBook.Models.Strategy;
using System;
using System.Collections.Generic;

namespace QuantBook.Tests
{
    public class OptimHelperTest
    {
        DateTime startDate = DateTime.Now;
        DateTime endDate = DateTime.Now.AddDays(10);    

        [Test]
        public void WhenGettingOptimResultsWithVariousWindowSizes()
        {
            var builder = new SignalBuilder(startDate);
            var signals = new List<SignalEntity>
            {
                builder.NewSignal(15.5),
                builder.NewSignal(-1.1),  // enters short trade (prevSignal > 2)
                builder.NewSignal(-0.1),  // [shouldnot] enter long trade (prevSignal < -2) 
                builder.NewSignal(0.1),
                builder.NewSignal(0.4),
                builder.NewSignal(0.8),
                builder.NewSignal(0.7),
                builder.NewSignal(0.1),
                builder.NewSignal(0.4),
                builder.NewSignal(0.3),
                builder.NewSignal(0.9),
                builder.NewSignal(-10.1),
                builder.NewSignal(0.1)    // exit short trade (prevSignal < -3) 
            };
            IEventAggregator events = new EventAggregator();
            var results = OptimHelper.OptimSingleName(signals, SignalTypeEnum.MovingAverage, StrategyTypeEnum.MeanReversion, false, modelEvents => Console.WriteLine(string.Join(Environment.NewLine, modelEvents.EventList)));
            foreach (var result in results)
            {
                Console.WriteLine($"ticker={result.ticker}, bar={result.bar}, zin={result.zin}, zout{result.zout}, sharpe={result.sharpe}, pnlcum={result.pnlCum}, numTrades={result.numTrades}");
                Assert.That(result.ticker, Is.EqualTo("aTicker"));
                Assert.That(result.bar, Is.GreaterThan(0));
                Assert.That(result.zin, Is.GreaterThan(0));
                Assert.That(result.zout, Is.GreaterThanOrEqualTo(0));
                Assert.That(result.sharpe, Is.GreaterThan(0).Or.LessThan(0));
                Assert.That(result.pnlCum, Is.GreaterThan(0).Or.LessThan(0));
                Assert.That(result.numTrades, Is.GreaterThan(0));
            }
        }

        [Test]
        public void WhenGettingPairsTradingItShouldReturnAValidTable()
        {
            var result = OptimHelper.OptimPairsTrading("aTicker1", "aTicker2", startDate, endDate, 1.0, PairTypeEnum.PriceRatio, null); ;
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Rows.Count, Is.GreaterThan(1));
        }
    }
}
