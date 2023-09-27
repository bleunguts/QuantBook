using Caliburn.Micro;
using QuantBook.Ch11;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;

namespace QuantBook.Models.Strategy
{
    public enum StrategyTypeEnum { MeanReversion = 0, Momentum = 1};

    public class BacktestHelper
    {   
        public static IEnumerable<PnlEntity> ComputeLongShortPnl(
            IEnumerable<SignalEntity> inputSignals, 
            double notional, // initial invest capital
            double signalIn, 
            double signalOut, 
            StrategyTypeEnum strategyType, 
            bool isReinvest)
        {       
            ActivePosition activePosition = ActivePosition.INACTIVE;
            var signals = new List<SignalEntity>(inputSignals);
            var pnlEntities = new List<PnlEntity>() { PnlEntity.Build(signals[0].Date, signals[0].Ticker, signals[0].Price, signals[0].Signal, PnlTradeType.POSITION_NONE) };
         
            double pnlCum = 0.0;
            int totalNumTrades = 0;
            for(int i = 1; i < signals.Count; i++)
            {
                bool exitingPosition = false;

                var prev = signals[i - 1];
                var current = signals[i];

                double pnlDaily = 0.0;
                double pnlPerTrade = 0.0;
                double prevSignal = strategyType == StrategyTypeEnum.Momentum ? -prev.Signal : prev.Signal;
                double prevPnlCum = pnlEntities[i - 1].PnLCum;
                
                if (activePosition.IsActive)
                {                    
                    if (activePosition.IsLongPosition()) 
                    {
                        // long position, compute daily PnL:
                        pnlDaily = activePosition.Shares * (current.Price - prev.Price);
                        pnlCum += pnlDaily;

                        // Exit Long Position:
                        if (prevSignal > -signalOut)
                        {
                            pnlPerTrade = activePosition.Shares * (current.Price - activePosition.PriceIn);
                            exitingPosition = true;
                            totalNumTrades++;
                        }
                    }
                    else if(activePosition.IsShortPosition())  
                    {
                        // in short position, compute daily PnL
                        pnlDaily = -activePosition.Shares * (current.Price - prev.Price);
                        pnlCum += pnlDaily;

                        // exit short position
                        if (prevSignal < signalOut)
                        {
                            pnlPerTrade = -activePosition.Shares * (current.Price - activePosition.PriceIn);
                            exitingPosition = true;
                            totalNumTrades++;
                        }
                    }                                                 
                }
                else
                {
                    bool testSignal(double prevSignal_, double signalIn_, out PnlTradeType tradeType)
                    {
                        // Enter Long Position:
                        if (prevSignal_ < -signalIn_)
                        {
                            tradeType = PnlTradeType.POSITION_LONG;
                            return true;
                        }

                        // enter short position
                        if (prevSignal_ > signalIn_)
                        {
                            tradeType = PnlTradeType.POSITION_SHORT;
                            return true;
                        }

                        // no signals breached 
                        tradeType = PnlTradeType.POSITION_NONE;
                        return false;
                    }

                    PnlTradeType positionType = PnlTradeType.POSITION_NONE;
                    if (testSignal(prevSignal, signalIn, out positionType))
                    {                        
                        var shares = isReinvest ? (notional + prevPnlCum) / current.Price : notional / current.Price;
                        activePosition = EnterPosition(positionType, current.Date, current.Price, shares);
                        totalNumTrades++;
                    }
                }

                // compute pnl for holding position
                var initialPrice = signals.First().Price;
                double pnlDailyHold = notional * (current.Price - prev.Price) / initialPrice;
                double pnlCumHold = notional * (current.Price - initialPrice) / initialPrice;
                
                pnlEntities.Add(PnlEntity.Build(current.Date, current.Ticker, current.Price, current.Signal, pnlCum, pnlDaily, pnlPerTrade, pnlDailyHold, pnlCumHold, totalNumTrades, activePosition));

                if (exitingPosition) { ExitPosition(ref activePosition); }
            }               

            return pnlEntities;

            void ExitPosition(ref ActivePosition position) => position = ActivePosition.INACTIVE;
            ActivePosition EnterPosition(PnlTradeType tradeType, DateTime dateIn, double priceIn, double shares) => new ActivePosition(tradeType, dateIn, priceIn, shares);
        }             

        public static List<(string ticker, string year, int numTrades, double pnl, double sp0, double pnl2, double sp1)> GetYearlyPnl(List<PnlEntity> p)
        {
            DateTime firstDate = p.First().Date;
            DateTime lastDate = p.Last().Date;

            DateTime currentDate = new DateTime(firstDate.Year, 1, 1);                     
            var result = new List<(string ticker, string year, int numTrades, double pnl, double sp0, double pnl2, double sp1)>();
            while(currentDate <= lastDate)
            {
                DateTime FIRST_DAY_OF_THAT_YEAR = new DateTime(currentDate.Year, 1, 1);
                DateTime LAST_DAY_OF_THAT_YEAR = new DateTime(currentDate.Year, 12, 31);
                var pnls = p.Where(pnl => pnl.Date >= FIRST_DAY_OF_THAT_YEAR && pnl.Date <= LAST_DAY_OF_THAT_YEAR).OrderBy(pnl => pnl.Date).ToList();
                if (pnls.Count > 0)
                {
                    PnlEntity first = pnls.First();
                    PnlEntity last = pnls.Last();                                      
                    var entitiesWithPnlDaily = pnls.Where(pnl => pnl.PnLDaily != 0).OrderBy(pnl => pnl.Date).ToList();
                    if(entitiesWithPnlDaily.Count > 0)
                    {
                        double[] sp = GetSharpe(entitiesWithPnlDaily);
                        int numTrades = last.NumTrades - first.NumTrades;
                        double pnl1 = last.PnLCum - first.PnLCum + first.PnLDaily;
                        double pnl2 = last.PnLCumHold - first.PnLCumHold + first.PnLDailyHold;
                        result.Add((first.Ticker, currentDate.Year.ToString(), numTrades, Math.Round(pnl1, 0), sp[0], Math.Round(pnl2, 0), sp[1]));
                    }
                }
                currentDate = currentDate.AddYears(1);
            }
            double[] sp1 = GetSharpe(p);
            double sum = Math.Round(p.Last().PnLCum, 0);
            double sum1 = Math.Round(p.Last().PnLCumHold, 0);
            result.Add((p.First().Ticker, "Total", p.Last().NumTrades, sum, sp1[0], sum1, sp1[1]));
            return result;
        }

        /// <summary>
        /// Sharpe ratio is a risk measure used to determine return of the strategy above risk free rate
        /// It average of daily returns divided by standard deviation of the daily returns        
        /// </summary>        
        public static double[] GetSharpe(List<PnlEntity> pnl)
        {

            // computes annualized sharpe ratio by taking int account daily P&L from the strategy and from the buying-and-holding of position
            double avg = pnl.Average(x => x.PnLDaily);
            double std = pnl.StdDev(x => x.PnLDaily);

            // buying and holding of position
            double avg1 = pnl.Average(x => x.PnLDailyHold);
            double std1 = pnl.StdDev(x => x.PnLDailyHold);

            double sp = Math.Round(Math.Sqrt(252.0) * avg / std, 4);
            double sp1 = Math.Round(Math.Sqrt(252.0) * avg1 / std1, 4);

            // shows how your strategy performs each year 
            return new[] { sp, sp1 };
        }

        /// <summary>
        /// A percentage of maximum drawdown 
        /// in a stable market, drawdown contracts are cheaper
        /// in a bubble market, drawdown will be huge
        /// </summary>        
        public static List<DrawDownResult> GetDrawDown(List<PnlEntity> pnlInput, double notional)
        {
            var results = new List<DrawDownResult>();

            double max = 0; double maxHold = 0;
            double min = 2.0 * notional; double minHold = 2.0 * notional;

            for (int i = 0; i < pnlInput.Count; i++)
            {
                var current = pnlInput[i];
                double pnl = current.PnLCum + notional;
                double pnlHold = current.PnLCumHold + notional;
                max = Math.Max(max, pnl);
                min = Math.Min(min, pnl);
                maxHold = Math.Max(maxHold, pnlHold);
                minHold = Math.Min(minHold, pnlHold);
                double drawdown = 100.0 * (max - pnl) / max;
                double drawdownHold = 100.0 * (maxHold - pnlHold) / maxHold;
                double drawup = 100.0 * (pnl - minHold) / pnl;
                double drawupHold = 100.0 * (pnlHold - minHold) / pnlHold;
                results.Add((current.Date, pnl, drawdown, drawup, pnlHold, drawdownHold, drawupHold));
            }

            return results;
        }

        public static (IEnumerable<PairPnlEntity> pairEntities, IEnumerable<PnlEntity> pnlEntities) ComputePnLPair(PairSignalEntity[] inputPairs, int inputNotional, double signalIn, double signalOut, double hedgeRatio)
        {
            var notional = inputNotional / (1.0 + hedgeRatio);
            IEnumerable<SignalEntity> signal1 = inputPairs.Select(s => new SignalEntity
            {
                Ticker = s.Ticker1,
                Date = s.Date,
                Price = s.Price1,
                Signal = s.Signal
            });
            IEnumerable<SignalEntity> signal2 = inputPairs.Select(s => new SignalEntity
            {
                Ticker = s.Ticker2,
                Date = s.Date,
                Price = s.Price2,
                Signal = -s.Signal
            });
            var pnl1 = ComputeLongShortPnl(signal1, notional, signalIn, signalOut, StrategyTypeEnum.MeanReversion, false).ToList();
            var pnl2 = ComputeLongShortPnl(signal2, hedgeRatio * notional, signalIn, signalOut, StrategyTypeEnum.MeanReversion, false).ToList();
            var pairPnlEntities = new List<PairPnlEntity>();
            var pnlEntities = new List<PnlEntity>();
            for (int i = 0; i < pnl1.Count; i++)
            {
                double pnlPerTrade = pnl1[i].PnlPerTrade + pnl2[i].PnlPerTrade;
                double pnlDaily = pnl1[i].PnLDaily + pnl2[i].PnLDaily;
                double pnlCumHold = pnl1[i].PnLCumHold + pnl2[i].PnLCumHold;

                pairPnlEntities.Add(new PairPnlEntity(inputPairs[0].Ticker1, inputPairs[1].Ticker2, pnl1[i].Date, pnl1[i].Price, pnl2[i].Price, pnl1[i].Signal, pnl1[i].TradeType, pnl2[i].TradeType, pnl1[i].NumTrades, pnl1[i].PnLDaily, pnl1[i].PnLCum, pnl2[i].PnLDaily, pnl2[i].PnLCum, pnlPerTrade, pnlDaily, pnlCumHold));

                string ticker = $"{pnl1[i].Ticker},{pnl2[i].Ticker}";
                double pnlCum = pnl1[i].PnLCum - pnl2[i].PnLCum;
                double pnlDailyHold = pnl1[i].PnLDailyHold - pnl2[i].PnLDailyHold;
                pnlEntities.Add(new PnlEntity(ticker, pnl1[i].Date, pnl1[i].Signal, pnl1[i].NumTrades, pnlPerTrade, pnlDaily, pnlCum, pnlDailyHold, pnlCumHold));
            }

            return (pairPnlEntities, pnlEntities);
        }
    }
}
