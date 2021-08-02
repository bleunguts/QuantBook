using Caliburn.Micro;
using QuantBook.Ch11;
using System;
using System.Collections.Generic;
using System.Data;

namespace QuantBook.Models.Strategy
{
    public enum StrategyTypeEnum { MeanReversion = 0, Momentum = 1};

    public class BacktestHelper
    {
        public static IEnumerable<PnlEntity> ComputeLongShortPnl(IEnumerable<SignalEntity> signals, double notional, double signalIn, double signalOut, StrategyTypeEnum strategyType, bool isReinvest)
        {
            var pnls = new List<PnlEntity>();

            foreach(var s in signals)
            {
                pnls.Add(new PnlEntity
                {
                    Ticker = s.Ticker,
                    Date = s.Date,
                    Price = s.Price,
                    Signal = strategyType == StrategyTypeEnum.Momentum ? -s.Signal : s.Signal,
                    TradeType = PnlTradeType.UNSPECIFIED,
                    DateIn = null,
                    PriceIn = null,
                    NumTrades = 0,
                    PnlPerTrade = 0,
                    PnlDaily = 0, 
                    PnLCum = 0
                });
            }

            PnlTradeType tradeType = default;
            int iLong = 0;
            double shares = 0;
            double pnlCum = 0.0;
            double? priceIn = null;
            DateTime? dateIn = null;
            int numTrades = 0;
            double ishort = 0;

            for (int i = 1; i < pnls.Count; i++)
            {
                bool isTrade = false;

                var item0 = pnls[i - 1];
                var item1 = pnls[i];
                double pnlDaily = 0.0;
                double pnlPerTrade = 0.0;

                // long position, compute daily PnL:
                if (tradeType ==  PnlTradeType.LONG && iLong > 0)
                {
                    pnlDaily = shares * (item1.Price - item0.Price);
                    pnlCum += pnlDaily;
                    item1.TradeType = PnlTradeType.LONG;
                    item1.DateIn = dateIn;
                    item1.PriceIn = priceIn;
                    isTrade = true;
                }

                // Enter Long Position:
                if(tradeType == PnlTradeType.UNSPECIFIED && item0.Signal < -signalIn && !isTrade)
                {
                    tradeType = PnlTradeType.LONG;
                    numTrades++;
                    dateIn = item1.Date;
                    priceIn = item1.Price;
                    shares = notional / item1.Price;
                    if (isReinvest)
                        shares = (notional + item0.PnLCum) / item1.Price;
                    iLong++;
                    item1.TradeType = PnlTradeType.LONG;
                    item1.DateIn = dateIn;
                    item1.PriceIn = priceIn;
                    isTrade = true;
                }

                // Exit Long Position:
                if(tradeType == PnlTradeType.LONG && item0.Signal > -signalOut)
                {
                    pnlPerTrade = shares * (item1.Price - (double)priceIn);
                    numTrades++;
                    item1.TradeType = PnlTradeType.LONG;
                    item1.DateIn = dateIn;
                    item1.PriceIn = priceIn;
                    tradeType = PnlTradeType.UNSPECIFIED;
                    priceIn = null;
                    shares = 0.0;
                    iLong = 0;
                    dateIn = null;
                    isTrade = true;
                }

                // in short position, compute daily PnL
                if (tradeType == PnlTradeType.SHORT && ishort > 0)
                {
                    pnlDaily = -shares * (item1.Price - item0.Price);
                    pnlCum += pnlDaily;
                    item1.TradeType = PnlTradeType.SHORT;
                    item1.DateIn = dateIn;
                    item1.PriceIn = priceIn;
                    isTrade = true;
                }

                // enter short position
                if (tradeType == PnlTradeType.UNSPECIFIED && item0.Signal > signalIn && !isTrade)
                {
                    tradeType = PnlTradeType.SHORT;
                    numTrades++;
                    dateIn = item1.Date;
                    priceIn = item1.Price;
                    shares = notional / item1.Price;
                    if (isReinvest)
                        shares = notional + item0.PnLCum / item1.Price;
                    ishort++;
                    item1.TradeType = PnlTradeType.SHORT;
                    item1.DateIn = dateIn;
                    item1.PriceIn = priceIn;
                    isTrade = true;
                }
                
                // exit short position
                if(tradeType == PnlTradeType.SHORT && item0.Signal < signalOut)
                {
                    pnlPerTrade = -shares * (item1.Price - priceIn.Value);
                    numTrades++;
                    item1.TradeType = PnlTradeType.SHORT;
                    item1.DateIn = dateIn;
                    item1.PriceIn = priceIn;
                    tradeType = PnlTradeType.UNSPECIFIED;
                    priceIn = null;
                    shares = 0.0;
                    ishort = 0;
                    dateIn = null;
                    isTrade = true;
                }

                // compute pnl for holding position
                double pnlDailyHold = notional * (item1.Price - item0.Price) / pnls[0].Price;
                double pnlCumHold = notional * (item1.Price - pnls[0].Price) / pnls[0].Price;

                item1.NumTrades = numTrades;
                item1.PnLCum = pnlCum;
                item1.PnlDaily = pnlDaily;
                item1.PnlPerTrade = pnlPerTrade;
                item1.PnlDailyHold = pnlDailyHold;
                item1.PnLCumHold = pnlCumHold;
            }

            if (strategyType == StrategyTypeEnum.Momentum)
            {
                foreach(var p in pnls)
                {
                    p.Signal = -p.Signal;
                }
            }

            return pnls;
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
