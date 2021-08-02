using System;

namespace QuantBook.Models.Strategy
{
    public enum PnlTradeType { UNSPECIFIED = 0, LONG,
        SHORT
    }
    public class PnlEntity
    {
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
    }
}
