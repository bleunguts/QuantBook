using System;

namespace QuantBook.Models.Strategy
{
    public enum PnlTradeType { POSITION_NONE= 0, POSITION_LONG, POSITION_SHORT }
    public class PnlEntity
    {     
        public PnlEntity(DateTime date, string ticker, double price, double signal, PnlTradeType tradeType)
        {
            Date = date;
            Ticker = ticker;
            Price = price;
            Signal = signal;
            TradeType = tradeType;
        }

        public PnlEntity(DateTime date, string ticker, double price, double signal, PnlTradeType tradeType, int numTrades, double pnLCum, double pnLDaily, double pnlPerTrade, double pnlDailyHold, double pnlCumHold, DateTime? dateIn, double? priceIn) 
        {
            Date = date;
            Ticker = ticker;
            Price = price;
            Signal = signal;
            TradeType = tradeType;
            NumTrades = numTrades;
            PnLCum = pnLCum;
            PnlDaily = pnLDaily;
            PnlPerTrade = pnlPerTrade;
            PnlDailyHold = pnlDailyHold;
            PnLCumHold = pnlCumHold;
            DateIn = dateIn;
            PriceIn = priceIn;
        }

        public string Ticker { get; set; }
        public DateTime Date { get; internal set; }
        public double Price { get; set; }
        public double Signal { get; set; }
        public PnlTradeType TradeType { get; set; }
        public DateTime? DateIn { get; set; }
        public double? PriceIn { get; set; }
        public int NumTrades { get; set; }
        public double PnlPerTrade { get; set; }
        public double PnlDaily { get; set; }
        public double PnlDailyHold { get; set; }
        public double PnLCumHold { get; internal set; }
        public double PnLCum { get; internal set; }

        public override string ToString()
        {
            return $"Ticker={Ticker},Date={Date.ToShortDateString()},Price={Price},Signal={Signal},Type={TradeType},NumTrades={NumTrades},DateIn={DateIn},PriceIn{PriceIn},PnlPerTrade={PnlPerTrade},PnlDaily={PnlDaily},PnlCum={PnLCum},PnlDailyHold={PnlDailyHold},PnlCumHold={PnLCumHold}";
        }
    }
}
