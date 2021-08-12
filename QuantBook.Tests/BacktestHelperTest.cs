using Caliburn.Micro;
using Moq;
using NUnit.Framework;
using QuantBook.Models.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantBook.Tests
{
    public class BacktestHelperTest
    {
        const double notional = 10_000.0;
        const double signalIn = 2;
        const double signalOut = 0.0;
        DateTime startDate = DateTime.Now;

        // prevSignal > 2 enters short
        // prevSignal < -2 enters long
        // -2 < prevSignal < 2 is allowed out of this range it will go into short or long position
        // -2 < -1.0 is still profitable
        //  -2 < -0.1 almost profitable
        // -2 < 0 signalOut barrier breached exit position
        [Test]
        public void WhenSignalIsLessThanNegSignalItShouldEnterAndExitLongTrade()
        {
            var builder = new SignalBuilder(startDate);
            var signals = new List<SignalEntity>
            {
                builder.NewSignal(-1.5),
                builder.NewSignal(-2.1),
                builder.NewSignal(-3.1), // enters long trade (prevSignal < -2) 
                builder.NewSignal(1.1), 
                builder.NewSignal(-4.1)  // exit long trade (prevSignal > 0)
            };

            var pnlEntities = BacktestHelper.ComputeLongShortPnl(signals, notional, signalIn, signalOut, StrategyTypeEnum.MeanReversion, false).ToList();
            Print(pnlEntities);
            (int enterTradeIndex, int exitTradeIndex) = ToTrades(pnlEntities, PnlTradeType.POSITION_LONG);
            var enterTrade = pnlEntities[enterTradeIndex];
            Assert.That(enterTrade.Date, Is.EqualTo(signals[enterTradeIndex].Date));
            Assert.That(enterTrade.TradeType, Is.EqualTo(PnlTradeType.POSITION_LONG));
            Assert.That(enterTrade.PriceIn, Is.EqualTo(signals[enterTradeIndex].Price));
            Assert.That(enterTrade.NumTrades, Is.EqualTo(1));
            Assert.That(enterTrade.PnlPerTrade, Is.EqualTo(0));

            var exitTrade = pnlEntities[exitTradeIndex];
            Assert.That(exitTrade.Date, Is.EqualTo(signals[exitTradeIndex].Date));
            Assert.That(exitTrade.TradeType, Is.EqualTo(PnlTradeType.POSITION_LONG));
            Assert.That(exitTrade.PriceIn, Is.EqualTo(signals[enterTradeIndex].Price));
            Assert.That(exitTrade.NumTrades, Is.EqualTo(2));
            Assert.That(exitTrade.PnlPerTrade, Is.GreaterThan(0).Or.LessThan(0));

            PrintStrategy(pnlEntities, enterTradeIndex, exitTradeIndex);
        }

        // prevSignal > 2 enters short
        // prevSignal < -2 enters long
        // -2 < prevSignal < 2 is allowed out of this range it will go into short or long position
        // prevSignal > 2 
        // 3 > 2  still cool
        // 4 > 2 still cool
        // 1 > 2 not gt 2 but still cool
        // 0 > 2 breached the point exit short position
        [Test]
        public void WhenSignalIsGreaterThanSignalItShouldEnterAndExitShortTrade()
        {
            var builder = new SignalBuilder(startDate);
            var signals = new List<SignalEntity>
            {
                builder.NewSignal(-1.5),
                builder.NewSignal(3.1),
                builder.NewSignal(0.1), // enters short trade (prevSignal > 2)
                builder.NewSignal(-10.1), 
                builder.NewSignal(0.1)   // exit short trade (prevSignal < 0) 
            };

            var pnlEntities = BacktestHelper.ComputeLongShortPnl(signals, notional, signalIn, signalOut, StrategyTypeEnum.MeanReversion, false).ToList();
            Print(pnlEntities);

            (int enterTradeIndex, int exitTradeIndex) = ToTrades(pnlEntities, PnlTradeType.POSITION_SHORT);
            var enterTrade = pnlEntities[enterTradeIndex];
            Assert.That(enterTrade.Date, Is.EqualTo(signals[enterTradeIndex].Date));
            Assert.That(enterTrade.TradeType, Is.EqualTo(PnlTradeType.POSITION_SHORT));
            Assert.That(enterTrade.PriceIn, Is.EqualTo(signals[enterTradeIndex].Price));
            Assert.That(enterTrade.NumTrades, Is.EqualTo(1));
            Assert.That(enterTrade.PnlPerTrade, Is.EqualTo(0));

            var exitTrade = pnlEntities[exitTradeIndex];
            Assert.That(exitTrade.Date, Is.EqualTo(signals[exitTradeIndex].Date));
            Assert.That(exitTrade.TradeType, Is.EqualTo(PnlTradeType.POSITION_SHORT));
            Assert.That(exitTrade.PriceIn, Is.EqualTo(signals[enterTradeIndex].Price));
            Assert.That(exitTrade.NumTrades, Is.EqualTo(2));
            Assert.That(exitTrade.PnlPerTrade, Is.GreaterThan(0).Or.LessThan(0));

            PrintStrategy(pnlEntities, enterTradeIndex, exitTradeIndex);
        }

        [Test]
        public void WhenSignalSwingsViciouslyShouldNotEnterPositionIfAlreadyTrading()
        {
            var signalIn = 2.0;
            var signalOut = -3.0;
            var random = new Random();
            const double basePrice = 1_000_000.0;
            int counter = 0;
            var builder = new SignalBuilder(startDate, (d) =>
            {                                
                return basePrice + ++counter * -10_000;
            }, 
            basePrice);

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

            var pnlEntities = BacktestHelper.ComputeLongShortPnl(signals, notional, signalIn, signalOut, StrategyTypeEnum.MeanReversion, false).ToList();
            Print(pnlEntities);
            (int enterTradeIndex, int exitTradeIndex) = ToTrades(pnlEntities, PnlTradeType.POSITION_SHORT);
            PrintStrategy(pnlEntities, enterTradeIndex, exitTradeIndex);
        }

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
            var results = OptimHelper.OptimSingleName(signals, SignalTypeEnum.MovingAverage, StrategyTypeEnum.MeanReversion, false, modelEvents => Console.WriteLine(string.Join(Environment.NewLine,modelEvents.EventList)));
            foreach(var result in results)
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

        private static (int enterTradeIndex, int exitTradeIndex) ToTrades(List<PnlEntity> pnlEntities, PnlTradeType pnlTradeType)
        {
            var first = pnlEntities.FindIndex(p => p.TradeType == pnlTradeType);
            var last = pnlEntities.FindLastIndex(p => p.TradeType == pnlTradeType);            
            return (first, last);
        }

        private static void Print(List<PnlEntity> pnlEntities) => pnlEntities.ForEach(p => Console.WriteLine(p));

        private static void PrintStrategy(List<PnlEntity> pnlEntities, int start, int end)
        {
            Console.WriteLine("Strategy PnL:");
            for (int i = start; i <= end; i++)
            {
                PnlEntity p = pnlEntities[i];
                //{0:0.##}
                Console.WriteLine($"{p.Date.ToShortDateString()},Price={p.Price:0.##},Signal={p.Signal:0.##},PnlPerTrade={p.PnlPerTrade:0.##},PnlDaily={p.PnLDaily:0.##},PnlCum={p.PnLCum:0.##},PnlDailyHold={p.PnLDailyHold:0.##},PnlCumHold={p.PnLCumHold:0.##}");
            }
        }

        class SignalBuilder
        {
            private readonly double basePrice;
            private readonly Func<double, double> randomizer;
            private DateTime currentDate;
            private readonly string ticker = "aTicker";
            private static Random random = new Random();

            public SignalBuilder(DateTime date) : this(date, null, 1.5) { }
            public SignalBuilder(DateTime date, Func<double, double> randomFunction, double basePrice)
            {
                this.currentDate = date;
                this.basePrice = basePrice;
                randomizer = randomFunction ?? new Func<double, double>(RandomPrice);
            }

            public SignalEntity NewSignal(double signal)
            {
                var entity = new Mock<SignalEntity>();
                entity.SetupGet(x => x.Date).Returns(currentDate);
                var price = randomizer(basePrice);
                entity.SetupGet(x => x.Price).Returns(price);
                entity.SetupGet(x => x.Signal).Returns(signal);
                entity.SetupGet(x => x.Ticker).Returns(ticker);

                MoveNext();
                return entity.Object;
            }

            private void MoveNext()
            {
                currentDate = currentDate.AddDays(1);
            }

            static double RandomPrice(double basePrice)
            {
                int multiplier = random.Next(0, 1) == 0 ? -1 : 1;                
                return basePrice + random.NextDouble() * multiplier;
            }
        }
    }
}
