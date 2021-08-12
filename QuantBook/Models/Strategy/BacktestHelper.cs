﻿using Caliburn.Micro;
using QuantBook.Ch11;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace QuantBook.Models.Strategy
{
    public enum StrategyTypeEnum { MeanReversion = 0, Momentum = 1};

    public class BacktestHelper
    {   
        public static IEnumerable<PnlEntity> ComputeLongShortPnl(IEnumerable<SignalEntity> inputSignals, double notional, double signalIn, double signalOut, StrategyTypeEnum strategyType, bool isReinvest)
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

        private static double[] GetSharpe(List<PnlEntity> pnl)
        {
            double avg = pnl.Average(x => x.PnLDaily);
            double std = pnl.StdDev(x => x.PnLDaily);
            double avg1 = pnl.Average(x => x.PnLDailyHold);
            double std1 = pnl.StdDev(x => x.PnLDailyHold);
            double sp = Math.Round(Math.Sqrt(252.0) * avg / std, 4);
            double sp1 = Math.Round(Math.Sqrt(252.0) * avg1 / std1, 4);

            return new[] { sp, sp1 };
        }

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
    }

    public class ActivePosition 
    {
        private readonly PnlTradeType tradeType;
        private readonly DateTime dateIn;
        private readonly double priceIn;
        private readonly double shares;       

        public ActivePosition(PnlTradeType tradeType, DateTime dateIn, double priceIn, double shares)
        {
            this.tradeType = tradeType;
            this.dateIn = dateIn;
            this.priceIn = priceIn;
            this.shares = shares;
        }

        private  ActivePosition() {}

        public bool IsActive => !Equals(INACTIVE);
     
        public double Shares => shares;
        public PnlTradeType TradeType => tradeType;
        public double PriceIn => priceIn;
        public DateTime DateIn => dateIn;

        public bool IsLongPosition() => TradeType == PnlTradeType.POSITION_LONG;
        public bool IsShortPosition() => TradeType == PnlTradeType.POSITION_SHORT;

        public static ActivePosition INACTIVE = new ActivePosition();
        public override bool Equals(object rhs)
        {
            var other = (ActivePosition)rhs;
            if (other == null) return false;
            return tradeType == other.tradeType &&
                    dateIn.Equals(other.dateIn) &&
                    priceIn == other.priceIn &&
                    shares == other.shares;
        }

        public override int GetHashCode()
        {
            return new { tradeType, dateIn, priceIn, shares }.GetHashCode();
        }    
    }

    public struct DrawDownResult
    {
        public DateTime date;
        public double pnl;
        public double drawdown;
        public double drawup;
        public double pnlHold;
        public double drawdownHold;
        public double drawupHold;

        public DrawDownResult(DateTime date, double pnl, double drawdown, double drawup, double pnlHold, double drawdownHold, double drawupHold)
        {
            this.date = date;
            this.pnl = pnl;
            this.drawdown = drawdown;            
            this.drawup = drawup;            
            this.pnlHold = pnlHold;
            this.drawdownHold = drawdownHold;
            this.drawupHold = drawupHold;         
        }

        public override bool Equals(object obj)
        {
            return obj is DrawDownResult other &&
                   date == other.date &&
                   pnl == other.pnl &&
                   drawdown == other.drawdown &&
                   drawup == other.drawup &&
                   pnlHold == other.pnlHold &&
                   drawdownHold == other.drawdownHold &&
                   drawupHold == other.drawupHold;
        }

        public override int GetHashCode()
        {
            int hashCode = 1211010681;
            hashCode = hashCode * -1521134295 + date.GetHashCode();
            hashCode = hashCode * -1521134295 + pnl.GetHashCode();
            hashCode = hashCode * -1521134295 + drawdown.GetHashCode();
            hashCode = hashCode * -1521134295 + drawup.GetHashCode();
            hashCode = hashCode * -1521134295 + pnlHold.GetHashCode();
            hashCode = hashCode * -1521134295 + drawdownHold.GetHashCode();
            hashCode = hashCode * -1521134295 + drawupHold.GetHashCode();
            return hashCode;
        }

        public void Deconstruct(out DateTime date, out double pnl, out double drawdown, out double drawup, out double pnlHold, out double drawdownHold, out double drawupHold)
        {
            date = this.date;
            pnl = this.pnl;
            drawdown = this.drawdown;
            drawup = this.drawup;
            pnlHold = this.pnlHold;
            drawdownHold = this.drawdownHold;
            drawupHold = this.drawupHold;
        }

        //public static implicit operator (DateTime date, double pnl, double drawdown, double drawup, double maxDrawup, double pnlHold, double drawdownHold, double drawupHold)(DrawDownResult value)
        //{
        //    return (value.date, value.pnl, value.drawdown, value.drawup, value.pnlHold, value.drawdownHold, value.drawupHold);
        //}

        public static implicit operator DrawDownResult((DateTime date, double pnl, double drawdown, double drawup, double pnlHold, double drawdownHold, double drawupHold) value)
        {
            return new DrawDownResult(value.date, value.pnl, value.drawdown, value.drawup, value.pnlHold, value.drawdownHold, value.drawupHold);
        }
    }
}
