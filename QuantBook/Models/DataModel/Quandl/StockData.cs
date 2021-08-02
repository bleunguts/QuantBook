using System;

namespace QuantBook.Models.DataModel.Quandl
{
    public class StockData
    {
        public string Ticker { get; set; }
        public DateTime? Date { get; set; }
        public double? Open { get; set; }
        public double? High { get; set; }
        public double? Low { get; set; }
        public double? Close { get; set; }
        public double? Volume { get; set; }
        public double? AdjOpen { get; set; }
        public double? AdjHigh { get; set; }
        public double? AdjLow { get; set; }
        public double? AdjClose { get; set; }
        public double? AdjVolume { get; set; }
        public double? ExDividend { get; set; }
        public double? SplitRatio { get; set; }
    }
}
