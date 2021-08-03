using Caliburn.Micro;
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
            var signals = new List<SignalEntity>(inputSignals);
            var pnlEntities = new List<PnlEntity>()
            {
                new PnlEntity(signals[0].Date, signals[0].Ticker, signals[0].Price, signals[0].Signal, PnlTradeType.POSITION_NONE)
            };

            double shares = 0;
            double pnlCum = 0.0;
            int numTrades = 0;

            bool isLongPosition = false;
            bool isShortPosition = false;
            PnlTradeType tradeType = PnlTradeType.POSITION_NONE;
            double? priceIn = null;
            DateTime? dateIn = null;

            for(int i = 1; i < signals.Count; i++)
            {
                bool hasProcessedThisSignal = false;              
                bool hasExitedPostion = false;

                var prev = signals[i - 1];
                var current = signals[i];

                double pnlDaily = 0.0;
                double pnlPerTrade = 0.0;
                double prevSignal = strategyType == StrategyTypeEnum.Momentum ? -prev.Signal : prev.Signal;
                double prevPnlCum = pnlEntities[i - 1].PnLCum;

                // long position, compute daily PnL:
                if (isLongPosition)
                {
                    pnlDaily = shares * (current.Price - prev.Price);
                    pnlCum += pnlDaily;
                    hasProcessedThisSignal = true;
                }

                // in short position, compute daily PnL
                if (isShortPosition)
                {
                    pnlDaily = -shares * (current.Price - prev.Price);
                    pnlCum += pnlDaily;
                    hasProcessedThisSignal = true;
                }

                // Enter Long Position:
                if (tradeType == PnlTradeType.POSITION_NONE && prevSignal < -signalIn && !hasProcessedThisSignal)
                {
                    tradeType = PnlTradeType.POSITION_LONG;
                    numTrades++;
                    dateIn = current.Date;
                    priceIn = current.Price;
                    shares = notional / current.Price;
                    if (isReinvest)
                        shares = (notional + prevPnlCum) / current.Price;
                    isLongPosition = true;
                    hasProcessedThisSignal = true;
                }

                // Exit Long Position:
                if (tradeType == PnlTradeType.POSITION_LONG && prevSignal > -signalOut)
                {
                    pnlPerTrade = shares * (current.Price - (double)priceIn);
                    numTrades++;
                    shares = 0.0;
                    isLongPosition = false;  
                    hasProcessedThisSignal = true;
                    hasExitedPostion = true;
                }           

                // enter short position
                if (tradeType == PnlTradeType.POSITION_NONE && prevSignal > signalIn && !hasProcessedThisSignal)
                {
                    tradeType = PnlTradeType.POSITION_SHORT;
                    numTrades++;
                    dateIn = current.Date;
                    priceIn = current.Price;
                    shares = notional / current.Price;
                    if (isReinvest)
                        shares = notional + prevPnlCum / current.Price;
                    isShortPosition = true;         
                    hasProcessedThisSignal = true;
                }

                // exit short position
                if (tradeType == PnlTradeType.POSITION_SHORT && prevSignal < signalOut)
                {
                    pnlPerTrade = -shares * (current.Price - priceIn.Value);
                    numTrades++;                                     
                    shares = 0.0;
                    isShortPosition = false;
                    hasProcessedThisSignal = true;
                    hasExitedPostion = true;
                }

                // compute pnl for holding position
                var initialPrice = signals.First().Price;
                double pnlDailyHold = notional * (current.Price - prev.Price) / initialPrice;
                double pnlCumHold = notional * (current.Price - initialPrice) / initialPrice;

                pnlEntities.Add(new PnlEntity(current.Date, current.Ticker, current.Price, current.Signal, tradeType, numTrades, pnlCum, pnlDaily, pnlPerTrade, pnlDailyHold, pnlCumHold, dateIn, priceIn));
                
                if (hasExitedPostion)
                {
                    dateIn = null;
                    priceIn = null;
                    tradeType = PnlTradeType.POSITION_NONE;
                }
            }               

            return pnlEntities;
        }     

        public static DataTable GetDrawDown(BindableCollection<PnlEntity> pnlCollection, double notional)
        {
            throw new NotImplementedException();
        }

        public static DataTable GetYearlyPnl(BindableCollection<PnlEntity> pnlCollection)
        {
            throw new NotImplementedException();
        }
    }
}
