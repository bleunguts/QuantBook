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
        [Ignore("WIP")]
        public void WhenComputingLongShortPnl()
        {
            var input = new BindableCollection<SignalEntity>();
            StrategyTypeEnum strategyType =  StrategyTypeEnum.NotImplemented;
            BacktestHelper.ComputeLongShortPnl(input, 1000.0, 100.0, 150.0, strategyType, false);
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

        #endregion
    }
}
