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


            PnlTradeType tradeType = PnlTradeType.POSITION_NONE;
            int iLong = 0;
            double ishort = 0;
            double shares = 0;
            double pnlCum = 0.0;
            double? priceIn = null;
            DateTime? dateIn = null;
            int numTrades = 0;

            for(int i = 1; i < signals.Count; i++)
            {
                bool isTrade = false;

                var prev = signals[i - 1];
                var current = signals[i];
                double pnlDaily = 0.0;
                double pnlPerTrade = 0.0;
                double prevSignal = strategyType == StrategyTypeEnum.Momentum ? -prev.Signal : prev.Signal;
                double prevPnlCum = pnlEntities[i - 1].PnLCum;
                bool hasExitedPostion = false;

                // long position, compute daily PnL:
                if (tradeType == PnlTradeType.POSITION_LONG && iLong > 0)
                {
                    pnlDaily = shares * (current.Price - prev.Price);
                    pnlCum += pnlDaily;
                    isTrade = true;
                }

                // Enter Long Position:
                if (tradeType == PnlTradeType.POSITION_NONE && prevSignal < -signalIn && !isTrade)
                {
                    tradeType = PnlTradeType.POSITION_LONG;
                    numTrades++;
                    dateIn = current.Date;
                    priceIn = current.Price;
                    shares = notional / current.Price;
                    if (isReinvest)
                        shares = (notional + prevPnlCum) / current.Price;
                    iLong++;
                    isTrade = true;
                }

                // Exit Long Position:
                if (tradeType == PnlTradeType.POSITION_LONG && prevSignal > -signalOut)
                {
                    pnlPerTrade = shares * (current.Price - (double)priceIn);
                    numTrades++;
                    shares = 0.0;
                    iLong = 0;
                    isTrade = true;
                    hasExitedPostion = true;
                }

                // in short position, compute daily PnL
                if (tradeType == PnlTradeType.POSITION_SHORT && ishort > 0)
                {
                    pnlDaily = -shares * (current.Price - prev.Price);
                    pnlCum += pnlDaily;
                    tradeType = PnlTradeType.POSITION_SHORT;
                    isTrade = true;
                }

                // enter short position
                if (tradeType == PnlTradeType.POSITION_NONE && prevSignal > signalIn && !isTrade)
                {
                    tradeType = PnlTradeType.POSITION_SHORT;
                    numTrades++;
                    dateIn = current.Date;
                    priceIn = current.Price;
                    shares = notional / current.Price;
                    if (isReinvest)
                        shares = notional + prevPnlCum / current.Price;
                    ishort++;           
                    isTrade = true;
                }

                // exit short position
                if (tradeType == PnlTradeType.POSITION_SHORT && prevSignal < signalOut)
                {
                    pnlPerTrade = -shares * (current.Price - priceIn.Value);
                    numTrades++;                                     
                    shares = 0.0;
                    ishort = 0;
                    isTrade = true;
                    hasExitedPostion = true;
                }

                // compute pnl for holding position
                var firstPrice = signals.First().Price;
                double pnlDailyHold = notional * (current.Price - prev.Price) / firstPrice;
                double pnlCumHold = notional * (current.Price - firstPrice) / firstPrice;

                pnlEntities.Add(new PnlEntity(current.Date, current.Ticker, current.Price, current.Signal, tradeType, numTrades, pnlCum, pnlDaily, pnlPerTrade, pnlDailyHold, pnlCumHold, dateIn, priceIn));
                
                if (hasExitedPostion)
                {
                    tradeType = PnlTradeType.POSITION_NONE;
                    dateIn = null;
                    priceIn = null;
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
