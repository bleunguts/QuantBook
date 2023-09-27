using QLNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantBook.Models.Strategy
{
    public enum PairTypeEnum
    {
        PriceRatio,
        Spread
    }
    public class PairSignalEntity
    {
        public double Price1 { get; set; }
        public double Price2 { get; set; }        
        public string Ticker1 { get; set; }
        public string Ticker2 { get; set; }
        public DateTime Date { get; set; }
        public double Correlation { get; set; }
        public double Beta { get; set; }
        public double Signal { get; set; }
        public double Spread  => Price1 / Price2; 
        public PairSignalEntity(string ticker1, string ticker2, DateTime date, double price1, double price2, double correlation, double beta, double signal)
        {
            Ticker1 = ticker1;
            Ticker2 = ticker2;
            Date = date;
            Price1 = price1;
            Price2 = price2;            
            Correlation = correlation;
            Beta = beta;
            Signal = signal;
        }

        public override string ToString()
        {
            return $"Ticker1:{Ticker1}, Ticker2:{Ticker2}, Price1:{Price1}, Price2:{Price2}, Date:{Date}, Correlation:{Correlation}, Beta:{Beta}, Signal:{Signal}, Spread:{Spread}";
        }
    }
    public class PairPnlEntity
    {
        public PairPnlEntity(string ticker1, string ticker2, DateTime date, double price1, double price2, double signal, PnlTradeType tradeType1, PnlTradeType tradeType2, int numTrades, double pnLDaily1, double pnLCum1, double pnLDaily2, double pnLCum2, double pnlPerTrade, double pnlDaily, double pnlCumHold)
        {
            Ticker1 = ticker1;
            Ticker2 = ticker2;
            Date = date;
            Price1 = price1;
            Price2 = price2;
            Signal = signal;
            TradeType1 = tradeType1;
            TradeType2 = tradeType2;
            NumTrades = numTrades;
            PnLDaily1 = pnLDaily1;
            PnLCum1 = pnLCum1;
            PnLDaily2 = pnLDaily2;
            PnLCum2 = pnLCum2;
            PnlPerTrade = pnlPerTrade;
            PnlDaily = pnlDaily;
            PnlCumHold = pnlCumHold;
        }

        public string Ticker1 { get; }
        public string Ticker2 { get; }
        public DateTime Date { get; }
        public double Price1 { get; }
        public double Price2 { get; }
        public double Signal { get; }
        public PnlTradeType TradeType1 { get; }
        public PnlTradeType TradeType2 { get; }
        public int NumTrades { get; }
        public double PnLDaily1 { get; }
        public double PnLCum1 { get; }
        public double PnLDaily2 { get; }
        public double PnLCum2 { get; }
        public double PnlPerTrade { get; }
        public double PnlDaily { get; }
        public double PnlCumHold { get; }
    }
}
