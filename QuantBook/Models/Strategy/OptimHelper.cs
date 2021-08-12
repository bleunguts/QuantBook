﻿using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace QuantBook.Models.Strategy
{
    public class OptimHelper
    {
        public static List<(string ticker, int bar, double zin, double zout, int numTrades, double pnlCum, double sharpe)> OptimSingleName(IEnumerable<SignalEntity> input, SignalTypeEnum signalType, StrategyTypeEnum strategyType, bool isReinvest, Action<ModelEvents> onProgress)
        {
            double notional = 10_000.0;
            int[] bars = new int[] { 3, 5, 7, 10, 11, 12, 15, 20, 30, 40, 50, 60, 70, 80, 90, 100, 120, 150, 200, 250, 300 };
            double[] zin = signalType == SignalTypeEnum.RSI || signalType == SignalTypeEnum.WilliamR ?
                new double[] { 0.5, 0.6, 0.7, 0.8, 0.9, 1.0, 1.2, 1.4, 1.6, 1.8, 1.9 }
                :
                new double[] { 0.5, 0.6, 0.7, 0.8, 0.9, 1.0, 1.2, 1.4, 1.6, 1.8, 2.0, 2.2, 2.4, 2.6 };
            double[] zout = new double[] { 0, 0.1, 0.2, 0.3, 0.4 };

            onProgress(new ModelEvents(new List<object>
            {
                "Starting...",
                0,
                bars.Length,
                0
            }));

            var results = new List<(string ticker, int bar, double zin, double zout, int numTrades, double pnlCum, double sharpe)>();
            for (int i = 0; i < bars.Length; i++)
            {
                var movingWindow = bars[i];
                if (movingWindow >= input.Count())
                {
                    Console.WriteLine($"Zero signals for movingWindow={bars[i]}, signalType={signalType}, inputSize={input.Count()}, skipping.");
                    continue;
                }

                var signals = SignalHelper.GetSignal(input, movingWindow, signalType);                
                for (int j = 0; j < zin.Length; j++)
                {
                    for (int k = 0; k < zout.Length; k++)
                    {
                        var pnl = BacktestHelper.ComputeLongShortPnl(signals, notional, zin[j], zout[k], strategyType, isReinvest).ToList();
                        if (pnl.Any())
                        {
                            var sharpe = BacktestHelper.GetSharpe(pnl);
                            PnlEntity firstItem = pnl.First();
                            PnlEntity lastItem = pnl.Last();
                            if (Math.Abs(lastItem.PnLCum) > 0)
                            {
                                results.Add((firstItem.Ticker, movingWindow, zin[j], zout[k], lastItem.NumTrades, lastItem.PnLCum, sharpe[0]));
                            }
                        }
                    }
                }
                onProgress(new ModelEvents(new List<object>
                {
                    "Ready...",
                    0,
                    1,
                    0
                }));            

            }

            return results;
        }
    }
}
